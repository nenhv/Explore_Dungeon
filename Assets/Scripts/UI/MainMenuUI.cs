using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{

    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject quitButton;
    [SerializeField] private GameObject highScoresButton;
    [SerializeField] private GameObject instructionsButton;
    [SerializeField] private GameObject returnToMainMenuButton;
    [Header("Level Selection")]
    [SerializeField] private GameObject levelSelectPanel;
    private bool isInstructionSceneLoaded = false;
    private bool isHighScoresSceneLoaded = false;

    private void Start()
    {
        LevelLaunchRequest.Reset();

        MusicManager.Instance.PlayMusic(
            GameResources.Instance.mainMenuMusic,
            0f,
            2f
        );

        SceneManager.LoadScene(
            "CharacterSelectorScene",
            LoadSceneMode.Additive
        );

        returnToMainMenuButton.SetActive(false);

        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
        }
    }

    public void PlayGame()
    {
        // Enter The Dungeon bat dau tu Level 1.
        LevelLaunchRequest.SelectLevel(1);

        SceneManager.LoadScene("MainGameScene");
    }

    public void PlayHardMode()
    {
        // Hard Mode dung cau hinh rieng,
        // khong su dung level cho tu Choose Level.
        LevelLaunchRequest.Reset();

        SceneManager.LoadScene("HardModeScene");
    }
    
    public void OpenLevelSelection()
    {
        SetCharacterSelectorVisible(false);
        if (levelSelectPanel == null)
        {
            Debug.LogError(
                "MainMenuUI: Level Select Panel is missing!"
            );
            return;
        }

        levelSelectPanel.SetActive(true);
    }

    public void CloseLevelSelection()
    {
        SetCharacterSelectorVisible(true);
        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
        }
    }

    public void PlaySelectedLevel(int levelNumber)
    {
        // Truyen so level duoc bam:
        // Level 1 = 1, Level 2 = 2...
        bool valid = LevelLaunchRequest.SelectLevel(
            levelNumber
        );

        if (!valid)
            return;

        SceneManager.LoadScene("MainGameScene");
    }

    public void LoadHighScores(){
        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        isHighScoresSceneLoaded = true;

        SceneManager.UnloadSceneAsync("CharacterSelectorScene");

        returnToMainMenuButton.SetActive(true);

        SceneManager.LoadScene("HighScoreScene", LoadSceneMode.Additive);
    }

    public void LoadCharacterSelector(){
        returnToMainMenuButton.SetActive(false);

        if (isHighScoresSceneLoaded){
            SceneManager.UnloadSceneAsync("HighScoreScene");
            isHighScoresSceneLoaded = false;
        }
        else if (isInstructionSceneLoaded){
            SceneManager.UnloadSceneAsync("InstructionsScene");
            isInstructionSceneLoaded = false;
        }

        playButton.SetActive(true);
        quitButton.SetActive(true);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(true);

        SceneManager.LoadScene("CharacterSelectorScene", LoadSceneMode.Additive);
    }

    public void LoadInstructions(){
        playButton.SetActive(false);
        quitButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        isInstructionSceneLoaded = true;

        SceneManager.UnloadSceneAsync("CharacterSelectorScene");

        returnToMainMenuButton.SetActive(true);

        SceneManager.LoadScene("InstructionsScene", LoadSceneMode.Additive);
    }

    private void SetCharacterSelectorVisible(bool visible)
    {
        // Lay scene chon nhan vat dang load Additive.
        Scene characterScene =
            SceneManager.GetSceneByName("CharacterSelectorScene");

        if (!characterScene.IsValid() ||
            !characterScene.isLoaded)
        {
            Debug.LogWarning(
                "CharacterSelectorScene is not loaded."
            );
            return;
        }

        // Lay cac GameObject goc cua scene.
        GameObject[] rootObjects =
            characterScene.GetRootGameObjects();

        foreach (GameObject root in rootObjects)
        {
            // An phan nhan vat va giao dien nhap ten.
            if (root.name == "CharacterSelector" ||
                root.name == "OptionsInputUI")
            {
                root.SetActive(visible);
            }
        }
    }

    public void QuitGame(){
        Application.Quit();
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate(){
        HelperUtilities.ValidateCheckNullValue(this, nameof(playButton), playButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(quitButton), quitButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(highScoresButton), highScoresButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(instructionsButton), instructionsButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(returnToMainMenuButton), returnToMainMenuButton);
    }
#endif
    #endregion
}
