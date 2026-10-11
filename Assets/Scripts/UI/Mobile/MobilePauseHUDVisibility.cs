
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

        bool isPaused =
            manager.gameState == GameState.gamePaused;

        bool shouldShowControls = !isPaused;

        if (mobileControlsRoot.activeSelf != shouldShowControls)
        {
            mobileControlsRoot.SetActive(shouldShowControls);
        }
    }
}
