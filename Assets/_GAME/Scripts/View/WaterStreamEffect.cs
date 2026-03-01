using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Hiệu ứng dòng nước chảy khi rót giữa 2 ống
/// </summary>
public class WaterStreamEffect : MonoBehaviour
{
    private Image streamImage;
    private RectTransform rectTransform;
    
    public void Initialize(Color waterColor)
    {
        rectTransform = GetComponent<RectTransform>();
        
        streamImage = gameObject.AddComponent<Image>();
        streamImage.color = waterColor;
        streamImage.raycastTarget = false;
        
        rectTransform.sizeDelta = new Vector2(15f, 100f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;
    }
    
    public void AnimateStream(Vector3 startWorldPos, Vector3 endWorldPos, float duration, System.Action onComplete = null)
    {
        transform.position = startWorldPos;
        rectTransform.localRotation = Quaternion.identity;
        
        float distance = Vector3.Distance(startWorldPos, endWorldPos);
        
        Sequence seq = DOTween.Sequence();
        
        seq.Append(rectTransform.DOSizeDelta(new Vector2(15f, distance), duration * 0.4f)
            .SetEase(Ease.OutQuad));
        
        seq.Append(rectTransform.DOSizeDelta(new Vector2(15f, 0f), duration * 0.6f)
            .SetEase(Ease.InQuad));
        
        seq.Join(transform.DOMove(endWorldPos, duration * 0.6f)
            .SetEase(Ease.InQuad));
        
        seq.Join(streamImage.DOFade(0f, duration * 0.3f)
            .SetDelay(duration * 0.5f));
        
        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
            Destroy(gameObject);
        });
    }
}
