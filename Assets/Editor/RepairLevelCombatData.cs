#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class RepairLevelCombatData
{
    private const string LevelFolder =
        "Assets/ScriptableObjectAssets/Dungeon/Levels/";

    [MenuItem("Tools/Dungeon/Repair Level 3 & 4 Combat Data")]
    public static void Repair()
    {
        DungeonLevelSO level2 = LoadLevel(2);
        DungeonLevelSO level3 = LoadLevel(3);
        DungeonLevelSO level4 = LoadLevel(4);

        if (level2 == null || level3 == null || level4 == null)
        {
            Debug.LogError("Missing DungeonLevelSO assets.");
            return;
        }

        int updatedRooms = 0;
        int updatedHealth = 0;

        foreach (DungeonLevelSO target in
                 new[] { level3, level4 })
        {
            foreach (RoomTemplateSO room in
                     target.roomTemplateList.Distinct())
            {
                if (room == null ||
                    room.roomEnemySpawnParametersList == null ||
                    room.enemiesByLevelList == null)
                    continue;

                var sourceParams =
                    room.roomEnemySpawnParametersList.Find(
                        p => p != null && p.dungeonLevel == level2);

                var sourcePool =
                    room.enemiesByLevelList.Find(
                        p => p != null && p.dungeonLevel == level2);

                // Rooms without combat configuration are allowed.
                if (sourceParams == null && sourcePool == null)
                    continue;

                if (sourceParams == null ||
                    sourcePool == null ||
                    sourcePool.spawnableObjectRatioList == null ||
                    sourcePool.spawnableObjectRatioList.Count == 0)
                {
                    Debug.LogError(
                        $"Incomplete source combat data: {room.name}");
                    continue;
                }

                bool changed = false;

                if (!room.roomEnemySpawnParametersList.Exists(
                        p => p != null && p.dungeonLevel == target))
                {
                    Undo.RecordObject(room, "Add level spawn parameters");

                    room.roomEnemySpawnParametersList.Add(
                        new RoomEnemySpawnParameters
                        {
                            dungeonLevel = target,
                            minTotalEnemiesToSpawn =
                                sourceParams.minTotalEnemiesToSpawn,
                            maxTotalEnemiesToSpawn =
                                sourceParams.maxTotalEnemiesToSpawn,
                            minConcurrentEnemies =
                                sourceParams.minConcurrentEnemies,
                            maxConcurrentEnemies =
                                sourceParams.maxConcurrentEnemies,
                            minSpawnInterval =
                                sourceParams.minSpawnInterval,
                            maxSpawnInterval =
                                sourceParams.maxSpawnInterval,
                            totalFirstWave =
                                sourceParams.totalFirstWave
                        });

                    changed = true;
                }

                if (!room.enemiesByLevelList.Exists(
                        p => p != null && p.dungeonLevel == target))
                {
                    Undo.RecordObject(room, "Add level enemy pool");

                    var ratios = sourcePool.spawnableObjectRatioList
                        .Where(x => x != null)
                        .Select(x => new SpawnableObjectRatio<EnemyDetailsSO>
                        {
                            dungeonObject = x.dungeonObject,
                            ratio = x.ratio
                        }).ToList();

                    room.enemiesByLevelList.Add(
                        new SpawnableObjectByLevel<EnemyDetailsSO>
                        {
                            dungeonLevel = target,
                            spawnableObjectRatioList = ratios
                        });

                    changed = true;
                }

                if (changed)
                {
                    EditorUtility.SetDirty(room);
                    updatedRooms++;
                }

                // Ensure enemy health exists for this level.
                foreach (var entry in sourcePool.spawnableObjectRatioList)
                {
                    EnemyDetailsSO enemy = entry?.dungeonObject;

                    if (enemy == null ||
                        enemy.enemyHealthDetailsArray == null)
                        continue;

                    if (enemy.enemyHealthDetailsArray.Any(
                            h => h.dungeonLevel == target))
                        continue;

                    var sourceHealth =
                        enemy.enemyHealthDetailsArray.FirstOrDefault(
                            h => h.dungeonLevel == level2);

                    if (sourceHealth.dungeonLevel == null)
                    {
                        Debug.LogError(
                            $"Missing Level 2 health: {enemy.name}");
                        continue;
                    }

                    Undo.RecordObject(enemy, "Add level enemy health");

                    enemy.enemyHealthDetailsArray =
                        enemy.enemyHealthDetailsArray.Concat(
                            new[]
                            {
                                new EnemyHealthDetails
                                {
                                    dungeonLevel = target,
                                    enemyHealthAmount =
                                        sourceHealth.enemyHealthAmount
                                }
                            }).ToArray();

                    EditorUtility.SetDirty(enemy);
                    updatedHealth++;
                }
            }
        }

        AssetDatabase.SaveAssets();

        Debug.Log(
            $"Combat data repair complete. " +
            $"Room entries added: {updatedRooms}, " +
            $"enemy health entries added: {updatedHealth}. " +
            "Review Console errors before testing.");
    }

    private static DungeonLevelSO LoadLevel(int number)
    {
        return AssetDatabase.LoadAssetAtPath<DungeonLevelSO>(
            LevelFolder + $"DungeonLevel_Level{number}.asset");
    }
}
#endif
