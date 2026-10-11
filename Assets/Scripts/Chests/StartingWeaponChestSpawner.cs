using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class StartingWeaponChestSpawner : MonoBehaviour
{
    private const int ChestCount = 3;

    [Header("Chest Prefab")]
    [SerializeField] private Chest chestPrefab;

    [Header("Three Starting Weapons (None = Random)")]
    [SerializeField] private WeaponDetailsSO[] weapons =
        new WeaponDetailsSO[ChestCount];

    [Header("Random Pool (Auto-Fill from Inspector menu)")]
    [SerializeField] private List<WeaponDetailsSO> randomWeaponPool =
        new List<WeaponDetailsSO>();

    [Header("Positions Relative To Player")]
    [SerializeField] private Vector2[] offsets =
    {
        new Vector2(-5.5f, 3f),
        new Vector2(0f, 5f),
        new Vector2(5.5f, 3f)
    };

    private bool hasSpawned;

    public void SpawnStartingChests(Room room, Vector3 playerPosition)
    {
        if (hasSpawned)
            return;

        if (room == null || room.instantiatedRoom == null)
            return;

        if (chestPrefab == null ||
            weapons == null || weapons.Length != ChestCount ||
            offsets == null || offsets.Length != ChestCount)
        {
            Debug.LogError(
                "StartingWeaponChestSpawner: Assign Chest Prefab and keep " +
                "Weapons / Offsets at exactly 3 elements.", this);
            return;
        }

        Player player = GameManager.Instance.GetPlayer();
        if (player == null)
            return;

        // Reserve every explicitly assigned weapon BEFORE randomizing any None slot.
        // This also prevents a random slot from picking a later fixed slot's weapon.
        HashSet<WeaponDetailsSO> reserved =
            new HashSet<WeaponDetailsSO>();

        foreach (Weapon weaponInInventory in player.weaponList)
        {
            if (weaponInInventory != null &&
                weaponInInventory.weaponDetails != null)
            {
                reserved.Add(weaponInInventory.weaponDetails);
            }
        }

        for (int i = 0; i < ChestCount; i++)
        {
            WeaponDetailsSO fixedWeapon = weapons[i];
            if (fixedWeapon == null)
                continue;

            if (!reserved.Add(fixedWeapon))
            {
                Debug.LogError(
                    "StartingWeaponChestSpawner: Fixed weapon in slot " + i +
                    " is already owned or duplicated: " + fixedWeapon.weaponName,
                    this);
                return;
            }
        }

        // Collect eligible random weapons, filtering duplicates and owned/fixed weapons.
        List<WeaponDetailsSO> available = new List<WeaponDetailsSO>();
        HashSet<WeaponDetailsSO> poolSeen = new HashSet<WeaponDetailsSO>();

        if (randomWeaponPool != null)
        {
            foreach (WeaponDetailsSO candidate in randomWeaponPool)
            {
                if (candidate != null &&
                    !reserved.Contains(candidate) &&
                    poolSeen.Add(candidate))
                {
                    available.Add(candidate);
                }
            }
        }

        int randomSlots = 0;
        for (int i = 0; i < ChestCount; i++)
        {
            if (weapons[i] == null)
                randomSlots++;
        }

        if (available.Count < randomSlots)
        {
            Debug.LogError(
                "StartingWeaponChestSpawner: Need " + randomSlots +
                " random weapons, but only " + available.Count +
                " eligible weapons remain. Open the component menu and run " +
                "'Auto-Fill Player Weapon Pool' in the Unity Editor.",
                this);
            return;
        }

        // Never overwrite serialized Inspector slots; randomize into a local array.
        WeaponDetailsSO[] selected = new WeaponDetailsSO[ChestCount];
        for (int i = 0; i < ChestCount; i++)
        {
            if (weapons[i] != null)
            {
                selected[i] = weapons[i];
                continue;
            }

            int randomIndex = Random.Range(0, available.Count);
            selected[i] = available[randomIndex];
            available.RemoveAt(randomIndex); // Sampling without replacement.
        }

        // Search for the closest valid spot to each requested offset.
        // The entrance room has walls / blocking tiles, so fixed offsets can fail.
        Vector3[] positions = new Vector3[ChestCount];
        List<Vector3> placed = new List<Vector3>(ChestCount);

        for (int i = 0; i < ChestCount; i++)
        {
            Vector3 desiredPosition = playerPosition + (Vector3)offsets[i];
            desiredPosition.z = 0f;

            if (!TryFindChestPosition(
                    room,
                    desiredPosition,
                    playerPosition,
                    placed,
                    out positions[i]))
            {
                Debug.LogError(
                    "StartingWeaponChestSpawner: Could not find a safe spot " +
                    "for chest " + (i + 1) +
                    " near " + desiredPosition +
                    ". Adjust Offsets or the spawn room layout.", this);
                return;
            }

            placed.Add(positions[i]);
        }

        hasSpawned = true;

        for (int i = 0; i < ChestCount; i++)
        {
            Chest chest = Instantiate(
                chestPrefab,
                positions[i],
                Quaternion.identity,
                room.instantiatedRoom.transform);

            chest.name = "StartingChest_" + (i + 1) + "_" +
                         selected[i].weaponName;

            // Reuse the game's existing materialize/open/collect pipeline.
            chest.Initialize(true, 0, selected[i], 0);
            Debug.Log("Starting chest " + (i + 1) + " position: " +
                      positions[i], this);
            Debug.Log("Starting chest " + (i + 1) + ": " +
                      selected[i].weaponName, this);
        }
    }

    // Search in expanding square rings, favoring positions near the requested offset.
    // No random guessing, and each chest remains far enough from the others.
    private bool TryFindChestPosition(
        Room room,
        Vector3 desiredPosition,
        Vector3 playerPosition,
        List<Vector3> existingPositions,
        out Vector3 result)
    {
        const int maxSearchRadius = 12;
        const float minimumChestDistance = 5.5f;
        const float minimumPlayerDistance = 2.8f;

        Grid grid = room.instantiatedRoom.grid;
        Vector3Int centerCell = grid.WorldToCell(desiredPosition);

        for (int radius = 0; radius <= maxSearchRadius; radius++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    // Evaluate only the current square ring (no repeated checks).
                    if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) != radius)
                        continue;

                    Vector3Int cell = centerCell + new Vector3Int(x, y, 0);
                    Vector3 candidate = grid.GetCellCenterWorld(cell);
                    candidate.z = 0f;

                    if (!IsPositionValid(room, candidate))
                        continue;

                    if (Vector3.Distance(candidate, playerPosition) <
                        minimumPlayerDistance)
                        continue;

                    bool tooCloseToOtherChest = false;
                    foreach (Vector3 existing in existingPositions)
                    {
                        if (Vector3.Distance(candidate, existing) <
                            minimumChestDistance)
                        {
                            tooCloseToOtherChest = true;
                            break;
                        }
                    }

                    if (tooCloseToOtherChest)
                        continue;

                    result = candidate;
                    return true;
                }
            }
        }

        result = Vector3.zero;
        return false;
    }

    
    private bool IsPositionValid(Room room, Vector3 worldPosition)
    {
        InstantiatedRoom instantiatedRoom = room.instantiatedRoom;

        if (instantiatedRoom == null || instantiatedRoom.grid == null)
            return false;

        Grid grid = instantiatedRoom.grid;
        Vector3Int cell = grid.WorldToCell(worldPosition);

        // Giu ruong cach mep phong de tranh de len tuong.
        if (cell.x < room.lowerBounds.x + 2 ||
            cell.x > room.upperBounds.x - 2 ||
            cell.y < room.lowerBounds.y + 2 ||
            cell.y > room.upperBounds.y - 2)
        {
            return false;
        }

        Tilemap groundTilemap = instantiatedRoom.groundTilemap;
        Tilemap collisionTilemap = instantiatedRoom.collisionTilemap;

        if (groundTilemap == null || collisionTilemap == null)
            return false;

        // Vi tri trung tam phai co san nha.
        if (!groundTilemap.HasTile(cell))
            return false;

        // Su dung dung danh sach tile cam di chuyen
        // da duoc cau hinh trong GameResources.
        TileBase[] blockedTiles =
            GameResources.Instance.enemyUnwalkableCollisionTilesArray;

        // Kiem tra vung 3x3 quanh ruong.
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int checkCell =
                    cell + new Vector3Int(x, y, 0);

                TileBase tile = collisionTilemap.GetTile(checkCell);

                if (tile == null)
                    continue;

                // Chi chan nhung tile thuc su la vat can.
                foreach (TileBase blockedTile in blockedTiles)
                {
                    if (tile == blockedTile)
                        return false;
                }
            }
        }

        return true;
    }


#if UNITY_EDITOR
    [ContextMenu("Auto-Fill Player Weapon Pool")]
    private void AutoFillPlayerWeaponPool()
    {
        const string folder = "Assets/ScriptableObjectAssets/Weapons/Player";

        string[] guids = UnityEditor.AssetDatabase.FindAssets(
            "t:WeaponDetailsSO", new[] { folder });

        List<WeaponDetailsSO> found = new List<WeaponDetailsSO>();
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            WeaponDetailsSO weapon =
                UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponDetailsSO>(path);

            if (weapon != null && !found.Contains(weapon))
                found.Add(weapon);
        }

        found.Sort((a, b) =>
            string.CompareOrdinal(a.weaponName, b.weaponName));

        randomWeaponPool = found;
        UnityEditor.EditorUtility.SetDirty(this);
        if (gameObject.scene.IsValid())
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                gameObject.scene);
        }

        Debug.Log("StartingWeaponChestSpawner: Found " + found.Count +
                  " player weapons in project. Save the scene.", this);
    }
#endif
}
