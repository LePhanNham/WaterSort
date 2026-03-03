using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LosePopupController : MonoBehaviour
{
    [Header("References")]
    public GameObject root;
    public RectTransform loseImage;
    public Image bgShadow;
    public RectTransform buttonsContainer;
    public CanvasGroup buttonsCanvasGroup;

    [Header("Optional hookup")]
    public GameplayController gameplayController;

    [Header("Timings")]
    public float imagePopDuration = 0.5f;
    public float bgFadeDuration = 0.35f;
    public float buttonsDelay = 0.15f;
    [Tooltip("Target alpha for background shadow (0-1). For alpha=250 use 250f/255f.")]
    public float shadowAlpha = 250f/255f;

    [Header("Game UI control")]
    public CanvasGroup gameCanvasGroup; // optional canvas group for game UI to disable/hide
    public bool hideGameUI = false; // if true, set game UI alpha to 0 when showing
    public bool disableInputOnShow = true; // if true, disable interaction on game UI while popup shown

    private float _prevGameAlpha = 1f;
    private bool _prevGameInteractable = true;
    private bool _prevGameBlocks = true;

    private void Awake()
    {
        if (root != null)
            root.SetActive(false);

        if (gameCanvasGroup != null)
        {
            _prevGameAlpha = gameCanvasGroup.alpha;
            _prevGameInteractable = gameCanvasGroup.interactable;
            _prevGameBlocks = gameCanvasGroup.blocksRaycasts;
        }

        
    }

    public void ShowLose()
    {
        if (root == null)
            return;

        root.SetActive(true);

        if (loseImage != null)
            loseImage.localScale = Vector3.zero;

        if (bgShadow != null)
        {
            var c = bgShadow.color;
            c.a = 0f;
            bgShadow.color = c;
            bgShadow.raycastTarget = true;
        }

        if (buttonsCanvasGroup != null)
        {
            buttonsCanvasGroup.alpha = 0f;
            buttonsCanvasGroup.interactable = false;
            buttonsCanvasGroup.blocksRaycasts = false;
        }

        if (buttonsContainer != null)
            buttonsContainer.anchoredPosition += Vector2.down * 40f;

        if (gameCanvasGroup != null)
        {
            if (disableInputOnShow)
            {
                gameCanvasGroup.interactable = false;
                gameCanvasGroup.blocksRaycasts = false;
            }

            if (hideGameUI)
                gameCanvasGroup.alpha = 0f;
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayLose();

        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (root != null)
            root.SetActive(true);

        if (loseImage != null)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(loseImage.DOScale(1.15f, imagePopDuration).SetEase(Ease.OutBack));
            seq.Append(loseImage.DOScale(1f, 0.12f));
            seq.Play();
            yield return seq.WaitForCompletion();
        }

        if (bgShadow != null)
            bgShadow.DOFade(shadowAlpha, bgFadeDuration).SetEase(Ease.Linear);

        yield return new WaitForSeconds(bgFadeDuration);

        yield return new WaitForSeconds(buttonsDelay);

        if (buttonsCanvasGroup != null)
            buttonsCanvasGroup.DOFade(1f, 0.25f).OnComplete(() => { buttonsCanvasGroup.interactable = true; buttonsCanvasGroup.blocksRaycasts = true; });

        if (buttonsContainer != null)
            buttonsContainer.DOAnchorPosY(buttonsContainer.anchoredPosition.y + 40f, 0.35f).SetEase(Ease.OutCubic);
    }

    public void OnHomeButton()
    {
        if (gameplayController != null)
            gameplayController.ResetToHome();
        Hide();
    }

    public void OnReplayButton()
    {
        if (gameplayController != null)
            gameplayController.RestartLevel();
        Hide();
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        if (buttonsCanvasGroup != null)
            buttonsCanvasGroup.DOFade(0f, 0.18f).SetEase(Ease.Linear).OnComplete(() => { buttonsCanvasGroup.interactable = false; buttonsCanvasGroup.blocksRaycasts = false; });

        if (bgShadow != null)
            bgShadow.DOFade(0f, 0.18f).SetEase(Ease.Linear).OnComplete(()=> { bgShadow.raycastTarget = false; });

        if (loseImage != null)
            loseImage.DOScale(0f, 0.22f).SetEase(Ease.InBack);

        yield return new WaitForSeconds(0.22f);

        if (root != null)
            root.SetActive(false);

        // restore game UI state
        if (gameCanvasGroup != null)
        {
            gameCanvasGroup.alpha = _prevGameAlpha;
            gameCanvasGroup.interactable = _prevGameInteractable;
            gameCanvasGroup.blocksRaycasts = _prevGameBlocks;
        }
    }
}
