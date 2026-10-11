using UnityEngine;

public class MobilePauseHUDVisibility : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private GameObject mobileControlsRoot;

    private void LateUpdate()
    {
        GameManager manager = GameManager.Instance;

        if (manager == null || mobileControlsRoot == null)
            return;

        bool shouldHideControls =
            manager.gameState == GameState.gamePaused ||
            manager.gameState == GameState.gameLost ||
            manager.gameState == GameState.gameWon ||
            manager.gameState == GameState.restartGame;

        bool shouldShowControls = !shouldHideControls;

        if (mobileControlsRoot.activeSelf != shouldShowControls)
        {
            mobileControlsRoot.SetActive(shouldShowControls);
        }
    }
}
