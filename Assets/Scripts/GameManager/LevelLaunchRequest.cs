using UnityEngine;

// Luu level duoc chon truoc khi load MainGameScene.
public static class LevelLaunchRequest
{
    // -1 = khong co level duoc yeu cau.
    private static int pendingLevelIndex = -1;

    // levelNumber: 1, 2, 3, 4.
    public static bool SelectLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > 4)
        {
            Debug.LogError(
                "Invalid level: " + levelNumber
            );
            return false;
        }

        // List trong GameManager danh index tu 0.
        pendingLevelIndex = levelNumber - 1;

        return true;
    }

    // Doc mot lan roi xoa yeu cau,
    // tranh giu level cu khi choi lan sau.
    public static int Consume()
    {
        int result = pendingLevelIndex;

        pendingLevelIndex = -1;

        return result;
    }

    public static void Reset()
    {
        pendingLevelIndex = -1;
    }
}
