
using UnityEngine;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class AutoAimTargeting : MonoBehaviour
{
    [Header("Auto Aim Settings")]

    [SerializeField, Min(0.1f)]
    private float maxTargetDistance = 14f;

    [SerializeField, Min(0.02f)]
    private float targetSearchInterval = 0.12f;

    private Enemy currentTarget;

    private float nextSearchTime;

    // PlayerControl gọi hàm này để lấy mục tiêu.
    public bool TryGetTarget(out Vector3 targetPosition)
    {
        targetPosition = Vector3.zero;

        GameManager manager = GameManager.Instance;

        if (manager == null)
            return false;

        Room currentRoom = manager.GetCurrentRoom();

        if (currentRoom == null)
            return false;

        // Chỉ tìm lại khi hết thời gian quét hoặc
        // mục tiêu đang giữ không còn hợp lệ.
        if (Time.time >= nextSearchTime ||
            (currentTarget != null &&
            !IsValidTarget(currentTarget, currentRoom)))
        {
            currentTarget = FindNearestEnemy(currentRoom);

            nextSearchTime =
                Time.time + targetSearchInterval;
        }

        if (!IsValidTarget(currentTarget, currentRoom))
            return false;

        // Lấy tâm vùng va chạm thay vì gốc Transform.
        PolygonCollider2D hitbox =
            currentTarget.GetComponent<PolygonCollider2D>();

        targetPosition = hitbox.bounds.center;

        return true;
    }

    private Enemy FindNearestEnemy(Room currentRoom)
    {
        EnemySpawner spawner = EnemySpawner.Instance;

        if (spawner == null)
            return null;

        // EnemySpawner của project tạo Enemy
        // là các GameObject con của nó.
        Enemy[] enemies =
            spawner.GetComponentsInChildren<Enemy>(false);

        Enemy nearestEnemy = null;

        float shortestDistanceSqr =
            maxTargetDistance * maxTargetDistance;

        Vector3 playerPosition = transform.position;

        foreach (Enemy enemy in enemies)
        {
            if (!IsValidTarget(enemy, currentRoom))
                continue;

            Vector3 enemyPosition =
                enemy.transform.position;

            float distanceSqr =
                (enemyPosition - playerPosition).sqrMagnitude;

            if (distanceSqr < shortestDistanceSqr)
            {
                shortestDistanceSqr = distanceSqr;

                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    private bool IsValidTarget(
        Enemy enemy,
        Room currentRoom)
    {
        if (enemy == null || !enemy.isActiveAndEnabled)
            return false;

        if (currentRoom == null)
            return false;

        Health health = enemy.GetComponent<Health>();

        if (health == null || !health.IsAlive)
            return false;

        // Enemy dang materialize co collider bi tat.
        PolygonCollider2D hitbox =
            enemy.GetComponent<PolygonCollider2D>();

        if (hitbox == null || !hitbox.enabled)
            return false;

        Vector3 position = enemy.transform.position;

        // Khong chon Enemy o phong khac.
        // +/- 0.5f la khoang dung sai cho toa do bien.
        if (position.x < currentRoom.lowerBounds.x - 0.5f ||
            position.x > currentRoom.upperBounds.x + 0.5f ||
            position.y < currentRoom.lowerBounds.y - 0.5f ||
            position.y > currentRoom.upperBounds.y + 0.5f)
        {
            return false;
        }

        float distanceSqr =
            (position - transform.position).sqrMagnitude;

        if (distanceSqr >
            maxTargetDistance * maxTargetDistance)
        {
            return false;
        }

        return true;
    }

    private void OnDisable()
    {
        currentTarget = null;
        nextSearchTime = 0f;
    }

    // Ve ban kinh auto aim trong Scene View.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            maxTargetDistance
        );
    }
}
