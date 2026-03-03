using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WinPopupController : MonoBehaviour
{
    [Header("References")]
    public GameObject root;
    public RectTransform winImage;
    public Image bgShadow;
    public RectTransform buttonsContainer;
    public CanvasGroup buttonsCanvasGroup;
    [Header("Game UI control")]
    public CanvasGroup gameCanvasGroup;
    public bool hideGameUI = false;
    public bool disableInputOnShow = true;

    [Header("Optional hookup")]
    public GameplayController gameplayController;

    [Header("Timings")]
    public float winPopDuration = 0.3f;
    public float bgFadeDuration = 0.35f;
    public float buttonsDelay = 0.15f;
    public float shadowAlpha = 250f/255f;

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

    private float _prevGameAlpha = 1f;
    private bool _prevGameInteractable = true;
    private bool _prevGameBlocks = true;

    public void ShowWin()
    {
        if (root == null)
            return;

        root.SetActive(true);

        if (winImage != null)
            winImage.localScale = Vector3.zero;

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
            SoundManager.Instance.PlayWin();

        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (root != null)
            root.SetActive(true);

        if (winImage != null)
            winImage.localScale = Vector3.zero;

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

        if (winImage != null)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(winImage.DOScale(1.15f, winPopDuration).SetEase(Ease.OutBack));
            seq.Append(winImage.DOScale(1f, 0.12f));
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

    public void OnNextButton()
    {
        if (gameplayController != null)
            gameplayController.NextLevel();
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

        if (winImage != null)
            winImage.DOScale(0f, 0.22f).SetEase(Ease.InBack);

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
