using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("Home Screen")]
    public GameObject homeUI;
    public Button playButton;
    public TextMeshProUGUI playButtonLevelText; // Text hiển thị số level trên nút Play
    
    [Header("Gameplay UI")]
    public GameObject gameplayUI;
    public TextMeshProUGUI levelText;
    public Button undoButton;
    public Button restartButton;
    
    [Header("Win Popup")]
    public GameObject winPopup;
    public Button nextLevelButton;
    public Button playAgainButton;
    
    [Header("Lose Popup")]
    public GameObject losePopup;
    public Button retryButton;
    public Button loseHomeButton;
    
    [Header("Setting Panel")]
    public Button settingButton;
    public GameObject settingPanel;
    public GameObject settingShadow;
    public Button[] settingButtons;
    public Button shadowCloseButton;
    
    [Header("Setting Panel - Individual Buttons")]
    public Button soundButton;
    public Image soundButtonIcon;
    
    public Button soundBGButton;
    public Image soundBGButtonIcon;
    
    public Button homeButton;
    
    private bool isFirstGameLoad = true;
    private bool isSettingOpen = false;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameWin += ShowWinPopup;
            GameManager.Instance.OnGameLose += ShowLosePopup;
        }
        
        InitializeUIStates();
        
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);
        
        if (undoButton != null)
            undoButton.onClick.AddListener(() => GameManager.Instance?.gameplayController?.Undo());
        
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartLevel);
        
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevel);
        
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnRestartLevel);
        
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRestartLevel);
        
        if (loseHomeButton != null)
            loseHomeButton.onClick.AddListener(OnLoseHomeClicked);
        
        if (settingButton != null)
            settingButton.onClick.AddListener(ToggleSettingPanel);
        
        if (shadowCloseButton != null)
            shadowCloseButton.onClick.AddListener(CloseSettingPanel);
        
        if (soundButton != null)
            soundButton.onClick.AddListener(ToggleSound);
        
        if (soundBGButton != null)
            soundBGButton.onClick.AddListener(ToggleSoundBG);
        
        if (homeButton != null)
            homeButton.onClick.AddListener(OnHomeButtonClicked);
        
        UpdateLevelText();
        UpdateSoundButtonVisual();
        UpdateSoundBGButtonVisual();
        
        CheckAndShowHomeUI();
    }
    

    private void InitializeUIStates()
    {
        if (winPopup != null)
            winPopup.SetActive(false);
        
        if (losePopup != null)
            losePopup.SetActive(false);
        
        if (settingPanel != null)
            settingPanel.SetActive(false);
    }
    
    private void CheckAndShowHomeUI()
    {
        if (GameManager.Instance?.gameplayController == null) return;
        
        // Luôn hiện home trước khi vào game
        if (isFirstGameLoad)
        {
            if (homeUI != null)
            {
                homeUI.SetActive(true);
                AnimateHomeUIAppear();
                UpdatePlayButtonText(); // Cập nhật level text
            }
            
            if (gameplayUI != null)
                gameplayUI.SetActive(false);
        }
        else
        {
            if (homeUI != null)
                homeUI.SetActive(false);
            
            if (gameplayUI != null)
                gameplayUI.SetActive(true);
        }
    }
    
    /// <summary>
    /// Cập nhật text level trên nút Play
    /// </summary>
    private void UpdatePlayButtonText()
    {
        if (playButtonLevelText != null && LevelDataManager.Instance != null)
        {
            int level = LevelDataManager.Instance.currentLevel;
            playButtonLevelText.text = $"Level {level}";
        }
    }
    
    private void AnimateHomeUIAppear()
    {
        if (homeUI == null) return;
        
        homeUI.transform.localScale = Vector3.zero;
        homeUI.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
    }
    
    private void OnPlayButtonClicked()
    {
        if (homeUI == null) return;
        
        isFirstGameLoad = false;
        
        homeUI.transform.DOScale(0f, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => 
            {
                homeUI.SetActive(false);
                
                if (gameplayUI != null)
                    gameplayUI.SetActive(true);
                
                if (GameManager.Instance?.gameplayController != null)
                {
                    GameManager.Instance.gameplayController.OnHomeUIDismissed();
                }
            });
    }

    private void Update()
    {
        // Cập nhật level text mỗi frame (hoặc dùng event)
        UpdateLevelText();
    }

    private void UpdateLevelText()
    {
        if (levelText != null && GameManager.Instance?.gameplayController != null)
        {
            int level = GameManager.Instance.gameplayController.currentLevel;
            levelText.text = $"Level {level}";
        }
    }

    private void OnNextLevel()
    {
        if (winPopup != null)
        {
            winPopup.transform.DOScale(0f, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => 
                {
                    winPopup.SetActive(false);
                    winPopup.transform.localScale = Vector3.one;
                    
                    if (GameManager.Instance?.gameplayController != null)
                    {
                        GameManager.Instance.gameplayController.NextLevel();
                    }
                });
        }
        else
        {
            if (GameManager.Instance?.gameplayController != null)
            {
                GameManager.Instance.gameplayController.NextLevel();
            }
        }
    }
    
    private void OnRestartLevel()
    {
        if (winPopup != null && winPopup.activeSelf)
        {
            winPopup.transform.DOScale(0f, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => 
                {
                    winPopup.SetActive(false);
                    winPopup.transform.localScale = Vector3.one;
                    
                    if (GameManager.Instance?.gameplayController != null)
                    {
                        GameManager.Instance.gameplayController.RestartLevel();
                    }
                });
        }
        else if (losePopup != null && losePopup.activeSelf)
        {
            losePopup.transform.DOScale(0f, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => 
                {
                    losePopup.SetActive(false);
                    losePopup.transform.localScale = Vector3.one;
                    
                    if (GameManager.Instance?.gameplayController != null)
                    {
                        GameManager.Instance.gameplayController.RestartLevel();
                    }
                });
        }
        else
        {
            if (GameManager.Instance?.gameplayController != null)
            {
                GameManager.Instance.gameplayController.RestartLevel();
            }
        }
    }
    
    private void OnLoseHomeClicked()
    {
        if (losePopup == null) return;
        
        losePopup.transform.DOScale(0f, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => 
            {
                losePopup.SetActive(false);
                losePopup.transform.localScale = Vector3.one;
                
                if (gameplayUI != null)
                    gameplayUI.SetActive(false);
                
                if (GameManager.Instance?.gameplayController != null)
                {
                    GameManager.Instance.gameplayController.ResetToHome();
                }
                
                if (homeUI != null)
                {
                    homeUI.SetActive(true);
                    AnimateHomeUIAppear();
                    UpdatePlayButtonText();
                }
                
                isFirstGameLoad = true;
            });
    }

    private void ShowWinPopup()
    {
        if (winPopup == null) return;
        
        winPopup.SetActive(true);
        winPopup.transform.localScale = Vector3.zero;
        
        Sequence popupSeq = DOTween.Sequence();
        
        popupSeq.Append(
            winPopup.transform.DOScale(1f, 0.5f)
                .SetEase(Ease.OutBack)
        );
        
        popupSeq.Join(
            winPopup.transform.DORotate(new Vector3(0, 0, 5f), 0.1f)
                .SetEase(Ease.InOutSine)
                .SetLoops(2, LoopType.Yoyo)
        );
        
        popupSeq.Play();
    }
    
    private void ShowLosePopup()
    {
        if (losePopup == null) return;
        
        losePopup.SetActive(true);
        losePopup.transform.localScale = Vector3.zero;
        
        Sequence popupSeq = DOTween.Sequence();
        
        popupSeq.Append(
            losePopup.transform.DOScale(1f, 0.5f)
                .SetEase(Ease.OutBack)
        );
        
        popupSeq.Play();
    }
    
    //==================== SETTING PANEL ====================
    
    private void ToggleSettingPanel()
    {
        if (isSettingOpen)
            CloseSettingPanel();
        else
            OpenSettingPanel();
    }
    
    private void OpenSettingPanel()
    {
        if (settingPanel == null) return;
        
        isSettingOpen = true;
        settingPanel.SetActive(true);
        
        if (settingShadow != null)
        {
            CanvasGroup shadowCanvas = settingShadow.GetComponent<CanvasGroup>();
            if (shadowCanvas == null)
                shadowCanvas = settingShadow.AddComponent<CanvasGroup>();
            
            shadowCanvas.alpha = 0f;
            shadowCanvas.DOFade(1f, 0.3f).SetEase(Ease.OutQuad);
        }
        
        if (settingButtons != null && settingButtons.Length > 0)
        {
            StartCoroutine(AnimateSettingButtons(true));
        }
        
    }
    
    private void CloseSettingPanel()
    {
        if (settingPanel == null || !isSettingOpen) return;
        
        isSettingOpen = false;
        
        // Animate shadow fade out
        if (settingShadow != null)
        {
            CanvasGroup shadowCanvas = settingShadow.GetComponent<CanvasGroup>();
            if (shadowCanvas != null)
            {
                shadowCanvas.DOFade(0f, 0.2f).SetEase(Ease.InQuad);
            }
        }
        
        // Animate buttons biến mất nhanh
        if (settingButtons != null && settingButtons.Length > 0)
        {
            foreach (var btn in settingButtons)
            {
                if (btn != null)
                {
                    btn.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack);
                }
            }
        }
        
        DOVirtual.DelayedCall(0.2f, () => 
        {
            if (settingPanel != null)
                settingPanel.SetActive(false);
        });
    }
    
    private System.Collections.IEnumerator AnimateSettingButtons(bool show)
    {
        float delay = 0.08f;
        
        for (int i = 0; i < settingButtons.Length; i++)
        {
            if (settingButtons[i] == null) continue;
            
            Transform btnTransform = settingButtons[i].transform;
            
            if (show)
            {
                btnTransform.localScale = Vector3.zero;
                
                btnTransform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(i * delay);
            }
            
            yield return new WaitForSeconds(delay);
        }
    }
    
    private void ToggleSound()
    {
        if (SoundManager.Instance == null) return;
        
        bool currentState = SoundManager.Instance.IsSoundEnabled();
        SoundManager.Instance.SetSoundEnabled(!currentState);
        
        UpdateSoundButtonVisual();
    }
    
    private void ToggleSoundBG()
    {
        if (SoundManager.Instance == null) return;
        
        bool currentState = SoundManager.Instance.IsBGMEnabled();
        SoundManager.Instance.SetBGMEnabled(!currentState);
        
        UpdateSoundBGButtonVisual();
    }
    
    private void OnHomeButtonClicked()
    {
        CloseSettingPanel();
        
        if (winPopup != null && winPopup.activeSelf)
            winPopup.SetActive(false);
        
        if (gameplayUI != null)
            gameplayUI.SetActive(false);
        
        if (GameManager.Instance?.gameplayController != null)
        {
            GameManager.Instance.gameplayController.ResetToHome();
        }
        
        if (homeUI != null)
        {
            homeUI.SetActive(true);
            AnimateHomeUIAppear();
            UpdatePlayButtonText();
        }
        
        isFirstGameLoad = true;
    }
    
    private void UpdateSoundButtonVisual()
    {
        if (soundButtonIcon == null || SoundManager.Instance == null) return;
        
        bool soundEnabled = SoundManager.Instance.IsSoundEnabled();
        
        Color iconColor = soundButtonIcon.color;
        iconColor.a = soundEnabled ? 1f : 0.3f;
        soundButtonIcon.color = iconColor;
    }
    
    private void UpdateSoundBGButtonVisual()
    {
        if (soundBGButtonIcon == null || SoundManager.Instance == null) return;
        
        bool bgmEnabled = SoundManager.Instance.IsBGMEnabled();
        
        Color iconColor = soundBGButtonIcon.color;
        iconColor.a = bgmEnabled ? 1f : 0.3f;
        soundBGButtonIcon.color = iconColor;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameWin -= ShowWinPopup;
            GameManager.Instance.OnGameLose -= ShowLosePopup;
        }
    }
}