using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class WaterStreamEffect : MonoBehaviour
{
    private Image streamImage;
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool isInitialized = false;
    
    public void Initialize(Color waterColor)
    {
        if (!isInitialized)
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = gameObject.AddComponent<RectTransform>();
            
            streamImage = GetComponent<Image>();
            if (streamImage == null)
                streamImage = gameObject.AddComponent<Image>();
            
            streamImage.raycastTarget = false;
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            
            
            isInitialized = true;
        }
        
        streamImage.color = waterColor;
        streamImage.DOKill();
        
        Color c = streamImage.color;
        c.a = 1f;
        streamImage.color = c;
        
        rectTransform.sizeDelta = new Vector2(120f, 100f);
        rectTransform.DOKill();
        transform.DOKill();
    }
    
    public void AnimateStream(Vector3 startWorldPos, Vector3 endWorldPos, float duration, System.Action onComplete = null)
    {
        transform.position = startWorldPos;
        rectTransform.localRotation = Quaternion.identity;
        
        float distance = Vector3.Distance(startWorldPos, endWorldPos);
        
        Sequence seq = DOTween.Sequence();
        
        seq.Append(rectTransform.DOSizeDelta(new Vector2(120f, distance), duration)
            .SetEase(Ease.Linear));
        
        seq.Join(streamImage.DOFade(0f, duration * 0.2f)
            .SetDelay(duration * 0.8f));
        
        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
            gameObject.SetActive(false);
        });
    }
    
    public void AnimateStreamGrowing(Vector3 sourceMouth, Vector3 startWaterLevel, Vector3 endWaterLevel, float duration)
    {
        transform.position = sourceMouth;
        rectTransform.localRotation = Quaternion.identity;
        
        float startDistance = Vector3.Distance(sourceMouth, startWaterLevel);
        float endDistance = Vector3.Distance(sourceMouth, endWaterLevel);
        
        float lengthMultiplier = 1.3f;
        startDistance *= lengthMultiplier;
        endDistance *= lengthMultiplier;
        
        rectTransform.sizeDelta = new Vector2(120f, startDistance);
        
        rectTransform.DOSizeDelta(new Vector2(120f, endDistance), duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                streamImage.DOFade(0f, 0.1f).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            });
    }
    
    public void DOKill()
    {
        if (streamImage != null)
            streamImage.DOKill();
        
        if (rectTransform != null)
            rectTransform.DOKill();
        
        if (transform != null)
            transform.DOKill();
        
        gameObject.SetActive(false);
    }
}
