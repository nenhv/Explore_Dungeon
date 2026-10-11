
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMap : SingletonMonobehaviour<DungeonMap>
{
    #region Header GameObject References
    [Space(10)]
    [Header("GameObject References")]
    #endregion
    [SerializeField] private GameObject minimapUI;
    private Camera dungeonMapCamera;
    private Camera cameraMain;
    private bool isTeleporting = false;

    private void Start(){
        cameraMain = Camera.main;

        Transform playerTransform = GameManager.Instance.GetPlayer().transform;

        Unity.Cinemachine.CinemachineVirtualCamera cinemachineVirtualCamera = GetComponentInChildren<Unity.Cinemachine.CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = playerTransform;

        dungeonMapCamera = GetComponentInChildren<Camera>();
        dungeonMapCamera.gameObject.SetActive(false);
    }

    
    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.gameState != GameState.dungeonOverviewMap ||
            dungeonMapCamera == null ||
            isTeleporting)
        {
            return;
        }

    #if UNITY_ANDROID && !UNITY_EDITOR
        // Android: xu ly cham vao ban do.
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                GetRoomClicked(touch.position);
                break;
            }
        }
    #else
        // PC va Unity Editor: giu input chuot.
        if (Input.GetMouseButtonDown(0))
        {
            GetRoomClicked(Input.mousePosition);
        }
    #endif
    }



    private void GetRoomClicked(Vector2 screenPosition)
    {
        if (isTeleporting)
            return;

        bool touchedRoom = false;

        Vector3 worldPosition =
            dungeonMapCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    0f
                )
            );

        worldPosition.z = 0f;

        Collider2D[] colliders =
            Physics2D.OverlapCircleAll(
                new Vector2(worldPosition.x, worldPosition.y),
                1f
            );

        foreach (Collider2D collider in colliders)
        {
            InstantiatedRoom instantiatedRoom =
                collider.GetComponent<InstantiatedRoom>();

            // Bỏ qua nếu collider không phải là phòng
            if (instantiatedRoom == null || instantiatedRoom.room == null)
            {
                continue;
            }

            // ĐÃ TÌM THẤY PHÒNG HỢP LỆ TRÊN BẢN ĐỒ -> Đánh dấu ở đây
            touchedRoom = true;

            Room room = instantiatedRoom.room;

            // Giữ nguyên business rule: phòng chưa dọn sạch hoặc chưa từng đi qua thì không cho teleport
            if (!room.isClearedOfEnemies || !room.isPreviouslyVisisted)
            {
                continue;
            }

            isTeleporting = true;

            // Coroutine này sẽ tự động đóng bản đồ sau khi teleport
            StartCoroutine(MovePlayerToRoom(worldPosition, room));
            return;
        }
        // Nếu bấm ra ngoài khoảng trống (không chạm vào bất kỳ phòng nào)
        // Tùy theo hướng dẫn bài học, nếu muốn bấm ra ngoài để đóng bản đồ thì xử lý ở đây:
        if (!touchedRoom)
        {
            ClearDungeonOverViewMap();
        }
    }


    private IEnumerator MovePlayerToRoom(Vector3 worldPosition, Room room){
        StaticEventHandler.CallRoomChangedEvent(room);

        yield return StartCoroutine(GameManager.Instance.Fade(0f, 1f, 0f, Color.black));

        ClearDungeonOverViewMap();

        GameManager.Instance.GetPlayer().playerControl.DisablePlayer();

        Vector3 spawnPosition = HelperUtilities.GetSpawnPositionNearestToPlayer(worldPosition);

        GameManager.Instance.GetPlayer().transform.position = spawnPosition;

        yield return StartCoroutine(GameManager.Instance.Fade(1f, 0f, 1f, Color.black));

        GameManager.Instance.GetPlayer().playerControl.EnablePlayer();
        isTeleporting = false;
    }

    public void DisplayDungeonOverViewMap(){
        GameManager.Instance.previousGameState = GameManager.Instance.gameState;
        GameManager.Instance.gameState = GameState.dungeonOverviewMap;

        GameManager.Instance.GetPlayer().playerControl.DisablePlayer();

        cameraMain.gameObject.SetActive(false);
        dungeonMapCamera.gameObject.SetActive(true);

        ActivateRoomsForDisplay();

        minimapUI.SetActive(false);
    }

    public void ClearDungeonOverViewMap(){
        GameManager.Instance.gameState = GameManager.Instance.previousGameState;
        GameManager.Instance.previousGameState = GameState.dungeonOverviewMap;

        GameManager.Instance.GetPlayer().playerControl.EnablePlayer();

        cameraMain.gameObject.SetActive(true);
        dungeonMapCamera.gameObject.SetActive(false);

        minimapUI.SetActive(true);
    }

    private void ActivateRoomsForDisplay(){
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary){
            Room room = keyValuePair.Value;

            room.instantiatedRoom.gameObject.SetActive(true);
        }
    }
}
