using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    #region Tooltip
    [Tooltip("MovementDetailsSO scriptable object containing movement details such as speed")]
    #endregion
    [SerializeField] private MovementDetailsSO movementDetails;

    private Player player;
    // Component tìm mục tiêu gần nhất.
    private AutoAimTargeting autoAimTargeting;

    // Có mục tiêu hợp lệ khi đang giữ FIRE hay không.
    private bool hasMobileTarget = false;

    // Lưu hướng bắn cuối cùng trên mobile.
    private Vector2 lastMobileAimDirection = Vector2.right;
    private bool leftMouseDownPreviousFrame = false;
    private int currentWeaponIndex = 1;
    // Huong di chuyen hop le cuoi cung.
    private Vector2 lastMoveDirection = Vector2.down;
    private float moveSpeed;
    private Coroutine playerRollCoroutine;
    private WaitForFixedUpdate waitForFixedUpdate;
    [HideInInspector] public bool isPlayerRolling = false;
    private float playerRollCooldownTimer = 0f;
    [HideInInspector] private bool isPlayerMovementDisabled = false;

    private bool reverse = false;
    private int previousWeaponIndex = 0;
    private float fireTime = 0;

    private void Awake(){
        player = GetComponent<Player>();
        autoAimTargeting =
        GetComponent<AutoAimTargeting>();
        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start(){
        waitForFixedUpdate = new WaitForFixedUpdate();

        SetStartingWeapon();

        SetPlayerAnimationSpeed();
    }

    private void SetStartingWeapon(){
        int index = 1;
        foreach(Weapon weapon in player.weaponList){
            if(weapon.weaponDetails == player.playerDetails.startingWeapon){
                SetWeaponByIndex(index);
                break;
            }
            index++ ;
        }
    }

    private void SetPlayerAnimationSpeed(){
        player.animator.speed = moveSpeed / Settings.baseSpeedForPlayerAnimation;
    }

    
    private void Update()
    {
        if (isPlayerMovementDisabled || isPlayerRolling)
        {
            // Khong cho cac lenh cham cu duoc xu ly sau.
            MobileActionButton.ClearPendingPresses();

            fireTime = 0f;
            leftMouseDownPreviousFrame = false;
            hasMobileTarget = false;

            return;
        }

        // 1. Di chuyen bang joystick hoac WASD.
        MovementInput();

        // 2. Xu ly nut Roll.
        HandleMobileRollInput();

        // Neu vua bat dau Roll, dung xu ly input khac.
        if (isPlayerRolling)
        {
            MobileActionButton.ClearPendingPresses();

            fireTime = 0f;
            leftMouseDownPreviousFrame = false;
            hasMobileTarget = false;

            PlayerRollCooldownTimer();
            return;
        }

        // 3. Ngam, ban, doi sung, nap dan.
        WeaponInput();

        // 4. Tuong tac voi vat pham.
        UseItemInput();

        // 5. Cap nhat cooldown Roll.
        PlayerRollCooldownTimer();
    }

    private void MovementInput()
    {
        Vector2 direction = Vector2.zero;

        bool rightMouseButtonDown = Input.GetMouseButtonDown(1);

        // 1. Lấy dữ liệu từ joystick đang hoạt động.
        MobileJoystick joystick = MobileJoystick.Instance;

        if (joystick != null && joystick.IsPressed)
        {
            // Đang điều khiển bằng joystick.
            direction = joystick.Direction;

            // Không dùng chuột phải để Roll trong lúc kéo joystick.
            rightMouseButtonDown = false;
        }
        else
        {
            // Không sử dụng joystick -> giữ điều khiển PC.
            float horizontalMovement = Input.GetAxisRaw("Horizontal");
            float verticalMovement = Input.GetAxisRaw("Vertical");

            direction = new Vector2(
                horizontalMovement,
                verticalMovement
            );

            // Tránh đi chéo nhanh hơn đi thẳng.
            direction = Vector2.ClampMagnitude(direction, 1f);
        }
        
        // Joystick dang duoc keo:
        // cap nhat huong nhin theo huong di chuyen.
        if (joystick != null &&
            joystick.IsPressed &&
            joystick.Direction.sqrMagnitude > 0.0001f)
        {
            lastMobileAimDirection =
                joystick.Direction.normalized;
        }
        // Ghi nho huong di chuyen de dung cho Roll.
        if (direction.sqrMagnitude > 0.0001f)
        {
            lastMoveDirection = direction.normalized;
        }
        // 2. Kiểm tra có yêu cầu di chuyển không.
        if (direction.sqrMagnitude > 0.0001f)
        {
            // 3. Di chuyển bình thường.
            if (!rightMouseButtonDown)
            {
                player.movementByVelocityEvent
                    .CallMovementByVelocityEvent(
                        direction,
                        moveSpeed
                    );
            }
            // 4. Giữ lại cơ chế Roll của PC.
            else if (playerRollCooldownTimer <= 0f)
            {
                PlayerRoll((Vector3)direction);
            }
        }
        else
        {
            // 5. Không có input -> đứng yên.
            player.idleEvent.CallIdleEvent();
        }
    }

    private void HandleMobileRollInput()
    {
        // Chi xu ly khi co mot lan cham Roll moi.
        bool rollPressed =
            MobileActionButton.ConsumePress(
                MobileActionType.Roll
            );

        if (!rollPressed)
            return;

        // Khong Roll neu dang Roll hoac dang cooldown.
        if (isPlayerRolling || playerRollCooldownTimer > 0f)
            return;

        Vector2 rollDirection = lastMoveDirection;

        // Uu tien huong joystick neu nguoi choi dang keo.
        MobileJoystick joystick = MobileJoystick.Instance;

        if (joystick != null &&
            joystick.IsPressed &&
            joystick.Direction.sqrMagnitude > 0.0001f)
        {
            rollDirection = joystick.Direction.normalized;
        }

        if (rollDirection.sqrMagnitude <= 0.0001f)
            return;

        // Goi lai co che Roll co san trong project.
        PlayerRoll((Vector3)rollDirection);
    }

    private void PlayerRoll(Vector3 direction){
        playerRollCoroutine = StartCoroutine(PlayerRollRoutine(direction));
    }

    private IEnumerator PlayerRollRoutine(Vector3 direction){
        float minDistance = 0.1f;

        isPlayerRolling = true;
        Vector3 targetPosition = player.transform.position + (Vector3)direction * movementDetails.rollDistance;

        while(Vector3.Distance(player.transform.position, targetPosition) > minDistance){
            player.movementToPositionEvent.CallMovementToPositionEvent(
                targetPosition,
                player.transform.position,
                movementDetails.rollSpeed,
                direction,
                isPlayerRolling
            );

            yield return waitForFixedUpdate;
        }

        isPlayerRolling = false;

        playerRollCooldownTimer = movementDetails.rollCooldownTime;
        player.transform.position = targetPosition;
    }

    private void PlayerRollCooldownTimer(){
        if(playerRollCooldownTimer >= 0f){
            playerRollCooldownTimer -= Time.deltaTime;
        }
    }

    private void WeaponInput(){
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);

        FireWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

        SwitchWeaponInput();

        ReloadWeaponInput();
    }

    
    private void AimWeaponInput(
        out Vector3 weaponDirection,
        out float weaponAngleDegrees,
        out float playerAngleDegrees,
        out AimDirection playerAimDirection)
    {
        hasMobileTarget = false;

        MobileFireButton fireButton =
            MobileFireButton.Instance;

        bool fireButtonHeld =
            fireButton != null && fireButton.IsPressed;

        Vector3 aimTargetPosition;

        // 1. Nguoi choi dang giu nut FIRE.
        if (fireButtonHeld)
        {
            Vector3 enemyTargetPosition = Vector3.zero;

            bool foundEnemy =
                autoAimTargeting != null &&
                autoAimTargeting.TryGetTarget(
                    out enemyTargetPosition
                );

            if (foundEnemy)
            {
                hasMobileTarget = true;

                aimTargetPosition = enemyTargetPosition;

                // Luu huong ngam cuoi cung.
                Vector3 directionToEnemy =
                    aimTargetPosition - transform.position;

                if (directionToEnemy.sqrMagnitude > 0.0001f)
                {
                    lastMobileAimDirection =
                        ((Vector2)directionToEnemy).normalized;
                }
            }
            else
            {
                // Khong co Enemy -> giu huong cu.
                aimTargetPosition =
                    transform.position +
                    (Vector3)lastMobileAimDirection * 100f;
            }
        }
        
        // 2. Neu dang keo joystick:
        // uu tien nhin theo huong joystick.
        else if (
            MobileJoystick.Instance != null &&
            MobileJoystick.Instance.IsPressed &&
            MobileJoystick.Instance.Direction.sqrMagnitude > 0.0001f
        )
        {
            aimTargetPosition =
                transform.position +
                (Vector3)lastMobileAimDirection * 100f;
        }

        // 3. Android: khi tha joystick,
        // giu huong nhin cuoi cung.
        else if (Application.isMobilePlatform)
        {
            aimTargetPosition =
                transform.position +
                (Vector3)lastMobileAimDirection * 100f;
        }

        // 4. PC: khi khong dung joystick,
        // tiep tuc ngam theo con tro chuot.
        else
        {
            aimTargetPosition =
                HelperUtilities.GetMouseWorldPosition();
        }
        
        // 4. Tinh vector tu nong sung den muc tieu.
        weaponDirection =
            aimTargetPosition -
            player.activeWeapon.GetShootPosition();

        // 5. Tinh vector tu Player den muc tieu.
        Vector3 playerDirection =
            aimTargetPosition - transform.position;

        // 6. Tinh cac goc ngam.
        weaponAngleDegrees =
            HelperUtilities.GetAngleFromVector(
                weaponDirection
            );

        playerAngleDegrees =
            HelperUtilities.GetAngleFromVector(
                playerDirection
            );

        // 7. Xac dinh huong cho animation.
        playerAimDirection =
            HelperUtilities.GetAimDirection(
                playerAngleDegrees
            );

        // 8. Goi event ngam sung co san.
        player.aimWeaponEvent.CallAimWeaponEvent(
            playerAimDirection,
            playerAngleDegrees,
            weaponAngleDegrees,
            weaponDirection
        );
    }
 
    private void FireWeaponInput(
        Vector3 weaponDirection,
        float weaponAngleDegrees,
        float playerAngleDegrees,
        AimDirection playerAimDirection)
    {
        MobileFireButton fireButton =
            MobileFireButton.Instance;

        MobileJoystick moveJoystick =
            MobileJoystick.Instance;

        bool fireButtonHeld =
            fireButton != null && fireButton.IsPressed;

        // Chi ban bang FIRE khi tim thay Enemy hop le.
        bool fireFromMobile =
            fireButtonHeld && hasMobileTarget;

        bool fireFromMouse = false;

        // PC van co the ban bang chuot nhu truoc.
        if (!Application.isMobilePlatform)
        {
            bool movingByJoystick =
                moveJoystick != null &&
                moveJoystick.IsPressed;

            bool mouseOverUI =
                EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject();

            // Khong de click UI bien thanh lenh ban PC.
            fireFromMouse =
                Input.GetMouseButton(0) &&
                !movingByJoystick &&
                !fireButtonHeld &&
                !mouseOverUI;
        }

        bool shouldFire =
            fireFromMobile || fireFromMouse;

        if (shouldFire)
        {
            fireTime += Time.deltaTime;

            player.fireWeaponEvent.CallFireWeaponEvent(
                true,
                leftMouseDownPreviousFrame,
                playerAimDirection,
                playerAngleDegrees,
                weaponAngleDegrees,
                weaponDirection,
                fireTime
            );

            leftMouseDownPreviousFrame = true;
        }
        else
        {
            fireTime = 0f;
            leftMouseDownPreviousFrame = false;
        }
    }


    private void SwitchWeaponInput(){
        bool mobileSwitchPressed =
            MobileActionButton.ConsumePress(
                MobileActionType.SwitchWeapon
            );

        if (mobileSwitchPressed && player.weaponList.Count > 1)
        {
            NextWeapon();
        }
        if (Input.mouseScrollDelta.y < 0f){
            PreviousWeapon();
        }
        if (Input.mouseScrollDelta.y > 0f){
            NextWeapon();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl)){
            SwitchBetweenTwoWeapon();
        }
        if (Input.GetKeyDown(KeyCode.Q)){
            FastSwitchWeapon();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)){
            SetWeaponByIndex(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)){
            SetWeaponByIndex(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)){
            SetWeaponByIndex(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)){
            SetWeaponByIndex(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5)){
            SetWeaponByIndex(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6)){
            SetWeaponByIndex(6);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7)){
            SetWeaponByIndex(7);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8)){
            SetWeaponByIndex(8);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9)){
            SetWeaponByIndex(9);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0)){
            SetWeaponByIndex(10);
        }
        if (Input.GetKeyDown(KeyCode.Minus)){
            SetCurrentWeaponToFirstInTheList();
        }
    }

    private void SetWeaponByIndex(int weaponIndex){
        Weapon currentWeapon = player.activeWeapon.GetCurrentWeapon();
        
        if (weaponIndex -1 < player.weaponList.Count){
            previousWeaponIndex = currentWeaponIndex;
            currentWeaponIndex = weaponIndex;
            Weapon weapon = player.weaponList[weaponIndex - 1];
            if(weapon == currentWeapon) return;
            player.stopReloadWeaponEvent.CallStopReloadWeapon(currentWeapon);
            player.setActiveWeaponEvent.CallSetActiveWeaponEvent(weapon);
        }
    }

    private void PreviousWeapon(){
        currentWeaponIndex--;
        if(currentWeaponIndex < 1){
            currentWeaponIndex = player.weaponList.Count;
        }
        SetWeaponByIndex(currentWeaponIndex);

    }

    private void NextWeapon(){
        currentWeaponIndex++;
        if(currentWeaponIndex > player.weaponList.Count){
            currentWeaponIndex = 1;
        }
        SetWeaponByIndex(currentWeaponIndex);
    }

    private void SwitchBetweenTwoWeapon(){
        if(!reverse){
            NextWeapon();
        } else {
            PreviousWeapon();
        }
        reverse = !reverse;
    }

    private void FastSwitchWeapon(){
        SetWeaponByIndex(previousWeaponIndex);
    }

    
    private void ReloadWeaponInput()
    {
        // Lay lenh tu nut Reload cam ung.
        bool mobileReloadPressed =
            MobileActionButton.ConsumePress(
                MobileActionType.Reload
            );

        Weapon currentWeapon =
            player.activeWeapon.GetCurrentWeapon();

        if (currentWeapon == null)
            return;

        int weaponClipAmmoCapacity =
            currentWeapon.weaponDetails.weaponClipAmmoCapacity;

        int weaponClipRemainingAmmo =
            currentWeapon.weaponClipRemainingAmmo;

        bool hasInfiniteAmmo =
            currentWeapon.weaponDetails.hasInfiniteAmmo;

        bool hasInfiniteClipCapacity =
            currentWeapon.weaponDetails.hasInfiniteClipCapacity;

        // Khong nap lai khi dang reload.
        if (currentWeapon.isWeaponReloading)
            return;

        // Khong con dan du tru.
        if (currentWeapon.weaponRemainingAmmo <= 0 &&
            !hasInfiniteAmmo)
        {
            return;
        }

        // Bang dan da day.
        if (weaponClipRemainingAmmo == weaponClipAmmoCapacity)
            return;

        bool keyboardReloadPressed =
            Input.GetKeyDown(KeyCode.R);

        // Giu co che tu reload khi het dan.
        bool shouldAutoReload =
            !hasInfiniteClipCapacity &&
            currentWeapon.weaponClipRemainingAmmo <= 0;

        if (keyboardReloadPressed ||
            mobileReloadPressed ||
            shouldAutoReload)
        {
            // Dung event Reload co san.
            player.reloadWeaponEvent.CallReloadWeaponEvent(
                currentWeapon,
                0
            );
        }
    }


    
    private void UseItemInput()
    {
        bool keyboardUsePressed =
            Input.GetKeyDown(KeyCode.E);

        bool mobileUsePressed =
            MobileActionButton.ConsumePress(
                MobileActionType.Use
            );

        // Khong nhan E va khong cham USE -> bo qua.
        if (!keyboardUsePressed && !mobileUsePressed)
            return;

        // Giu nguyen ban kinh tuong tac cua game goc.
        float useItemRadius = 2.5f;

        Collider2D[] nearbyColliders =
            Physics2D.OverlapCircleAll(
                player.GetPlayerPosition(),
                useItemRadius
            );

        foreach (Collider2D collider2D in nearbyColliders)
        {
            IUseable useable =
                collider2D.GetComponent<IUseable>();

            if (useable != null)
            {
                useable.UseItem();
            }
        }
    }


    private void OnCollisionEnter2D(Collision2D collision){
        StopPlayerRollRountine();
    }
    private void OnCollisionStay2D(Collision2D collision){
        StopPlayerRollRountine();
    }

    private void StopPlayerRollRountine(){
        if(playerRollCoroutine != null){
            StopCoroutine(playerRollCoroutine);

            isPlayerRolling = false;
        }
    }


    public void EnablePlayer()
    {
        // Khong su dung lai cac lenh cham cu.
        MobileActionButton.ResetAllInputs();

        isPlayerMovementDisabled = false;
    }

    public void DisablePlayer()
    {
        isPlayerMovementDisabled = true;

        // Xoa cac yeu cau action dang cho.
        MobileActionButton.ResetAllInputs();

        fireTime = 0f;
        leftMouseDownPreviousFrame = false;
        hasMobileTarget = false;

        player.idleEvent.CallIdleEvent();
    }


    private void SetCurrentWeaponToFirstInTheList(){
        List<Weapon> tempWeaponList = new List<Weapon>();

        Weapon currentWeapon = player.weaponList[currentWeaponIndex - 1];
        currentWeapon.weaponListPosition = 1;
        tempWeaponList.Add(currentWeapon);

        int index = 2;
        foreach (Weapon weapon in player.weaponList){
            if(weapon == currentWeapon) continue;
            tempWeaponList.Add(weapon);
            weapon.weaponListPosition = index;
            index++;
        }

        player.weaponList = tempWeaponList;
        
        currentWeaponIndex = 1;

        SetWeaponByIndex(currentWeaponIndex);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate(){
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }

#endif
    #endregion
}
