using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("Home Screen")]
    public GameObject homeUI;
    public Button playButton;
    
    [Header("Gameplay UI")]
    public GameObject gameplayUI;
    public TextMeshProUGUI levelText;
    public Button undoButton;
    public Button restartButton;
    
    [Header("Win Popup")]
    public GameObject winPopup;
    public Button nextLevelButton;
    public Button playAgainButton;
    
    [Header("Setting Panel")]
    public Button settingButton;
    public GameObject settingPanel;
    public GameObject settingShadow;
    public Button[] settingButtons;
    public Button shadowCloseButton;
    
    [Header("Setting Panel - Individual Buttons")]
    public Button soundButton;
    public Image soundButtonIcon;
    
    private bool isFirstGameLoad = true;
    private bool isSettingOpen = false;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameWin += ShowWinPopup;
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
        
        if (settingButton != null)
            settingButton.onClick.AddListener(ToggleSettingPanel);
        
        if (shadowCloseButton != null)
            shadowCloseButton.onClick.AddListener(CloseSettingPanel);
        
        if (soundButton != null)
            soundButton.onClick.AddListener(ToggleSound);
        
        UpdateLevelText();
        UpdateSoundButtonVisual();
        
        CheckAndShowHomeUI();
    }
    

    private void InitializeUIStates()
    {
        if (winPopup != null)
            winPopup.SetActive(false);
        
        if (settingPanel != null)
            settingPanel.SetActive(false);
    }
    
    private void CheckAndShowHomeUI()
    {
        if (GameManager.Instance?.gameplayController == null) return;
        
        int currentLevel = GameManager.Instance.gameplayController.currentLevel;
        
        if (isFirstGameLoad && currentLevel >= 2)
        {
            if (homeUI != null)
            {
                homeUI.SetActive(true);
                AnimateHomeUIAppear();
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
        else
        {
            if (GameManager.Instance?.gameplayController != null)
            {
                GameManager.Instance.gameplayController.RestartLevel();
            }
        }
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
    
    private void UpdateSoundButtonVisual()
    {
        if (soundButtonIcon == null || SoundManager.Instance == null) return;
        
        bool soundEnabled = SoundManager.Instance.IsSoundEnabled();
        
        Color iconColor = soundButtonIcon.color;
        iconColor.a = soundEnabled ? 1f : 0.3f;
        soundButtonIcon.color = iconColor;
        
        // Option 2: Change color (gray when off, white when on)
        // soundButtonIcon.color = soundEnabled ? Color.white : Color.gray;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameWin -= ShowWinPopup;
        }
    }
}