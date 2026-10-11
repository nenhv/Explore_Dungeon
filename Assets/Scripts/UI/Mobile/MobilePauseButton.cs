
using UnityEngine;

[DisallowMultipleComponent]
public class MobilePauseButton : MonoBehaviour
{
    // Ham duoc goi khi nguoi choi cham nut PAUSE.
    public void OnPauseClicked()
    {
        GameManager manager = GameManager.Instance;

        if (manager == null)
        {
            Debug.LogWarning(
                "MobilePauseButton: GameManager not found!"
            );
            return;
        }

        // Chi cho phep Pause o nhung trang thai
        // ma phim ESC cua game goc ho tro.
        switch (manager.gameState)
        {
            case GameState.playingLevel:
            case GameState.engagingEnemies:
            case GameState.bossStage:
            case GameState.engagingBoss:
            case GameState.gamePaused:
                manager.PauseGameMenu();
                break;
        }
    }
}
