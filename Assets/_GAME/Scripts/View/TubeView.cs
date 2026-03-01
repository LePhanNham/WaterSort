using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// TubeView sử dụng UI Image với Fill Amount thay vì spawn nhiều GameObject
/// </summary>
public class TubeView : MonoBehaviour, IPointerClickHandler
{
    public TubeData Data { get; private set; }
    private GameConfig config;

    [Header("UI Image Fill Amount (4 Images xếp từ dưới lên)")]
    public Image[] liquidImages; // 4 Image, index 0 = đáy
    
    [Header("Separator Lines (Optional - giữa các segments)")]
    public Image[] separatorLines; // 3 lines giữa 4 segments, có thể null
    
    [Header("Click Area (Image với Raycast Target = true)")]
    public Image clickableArea; // Image để nhận click, nếu null sẽ tự tìm
    
    [Header("Cap/Lid Settings")]
    public GameObject tubeCap; // Nắp ống, sẽ hiện khi tube completed
    public float capAnimDuration = 0.4f; // Thời gian animation đóng nắp
    
    [Header("Animation Settings")]
    public float selectMoveDistance = 20f; // Khoảng cách move lên khi select
    [Tooltip("Offset thời gian sound pour (âm = sớm hơn, dương = muộn hơn). Mặc định: -0.05s")]
    public float pourSoundOffset = -0.05f; // Play sound sớm hơn animation một chút
    
    [Header("Water Stream Settings")]
    public Transform tubeMouthPoint; // Optional: điểm miệng ống để spawn water stream
    public float tubeHeight = 200f; // Chiều cao ống (nếu không có tubeMouthPoint)
    
    private Vector3 originalLocalPos;
    private Vector3 originalPos;
    private LayoutElement layoutElement;
    private int originalSiblingIndex;
    
    private List<Tween> idleTweens = new List<Tween>();
    private bool isAnimating = false;
    
    private Canvas parentCanvas;

    public void Setup(TubeData data, GameConfig config)
    {
        this.Data = data;
        this.config = config;
        
        layoutElement = GetComponent<LayoutElement>();
        parentCanvas = GetComponentInParent<Canvas>();
        
        if (parentCanvas == null)
            Debug.LogError($"Tube {data.Id}: Cannot find parent Canvas!");
        
        originalSiblingIndex = transform.GetSiblingIndex();
        
        RefreshOriginalPosition();
        
        if (tubeCap != null)
            tubeCap.SetActive(false);

        if (liquidImages == null || liquidImages.Length != config.tubeCapacity)
        {
            return;
        }

        if (config == null)
        {
            return;
        }

        foreach (var img in liquidImages)
        {
            if (img == null)
                continue;
            
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Vertical;
            img.fillOrigin = (int)Image.OriginVertical.Bottom;
            img.fillAmount = 0;
            img.color = Color.white;
        }

        if (clickableArea == null)
            clickableArea = GetComponent<Image>();
        
        if (clickableArea != null)
            clickableArea.raycastTarget = true;

        RebuildView();

        data.OnTubeCompleted += OnTubeCompleted;
        
        StartIdleAnimation();
    }
    
    private void StartIdleAnimation()
    {
        if (Data == null || Data.IsEmpty() || isAnimating) return;
        
        StopIdleAnimation();
        
        // Wave effect cho từng liquid image có nước
        for (int i = 0; i < liquidImages.Length; i++)
        {
            if (liquidImages[i].fillAmount > 0)
            {
                float delay = i * 0.1f; // Delay giữa các layer
                
                // Subtle scale wave - individual tweens với infinite loops
                Tween waveTween = liquidImages[i].transform.DOScale(new Vector3(1.02f, 1.01f, 1f), 1.5f)
                    .SetDelay(delay)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                    
                idleTweens.Add(waveTween);
            }
        }
    }
    
    private void StopIdleAnimation()
    {
        // Kill all idle tweens
        foreach (var tween in idleTweens)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
        }
        idleTweens.Clear();
        
        // Reset scales
        foreach (var img in liquidImages)
        {
            if (img != null)
                img.transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Rebuild toàn bộ view từ Data (dùng cho Init và Undo)
    /// </summary>
    private void RebuildView()
    {
        // Reset hết
        foreach (var img in liquidImages)
        {
            img.fillAmount = 0;
            img.color = Color.clear;
        }

        if (Data.IsEmpty()) return;

        // Lấy toàn bộ stack từ Data
        Stack<ColorType> tempStack = new Stack<ColorType>();
        while (!Data.IsEmpty()) 
            tempStack.Push(Data.RemoveLiquid(false));

        // Rebuild lại cả Data và View
        List<ColorType> colors = new List<ColorType>();
        int index = 0;
        while (tempStack.Count > 0 && index < liquidImages.Length)
        {
            ColorType color = tempStack.Pop();
            Data.AddLiquid(color, false);
            colors.Add(color);
            
            Color visualColor = config.GetColorValue(color);
            liquidImages[index].color = visualColor;
            liquidImages[index].fillAmount = 1f;
            index++;
        }
        
        UpdateSeparators(colors);
    }
    
    private void CreateWaterStream(Vector3 startPos, Vector3 endPos, Color waterColor, float delay, float duration)
    {
        if (parentCanvas == null)
            return;
        
        GameObject streamObj = new GameObject("WaterStream");
        streamObj.transform.SetParent(parentCanvas.transform, true);
        
        // Add RectTransform
        RectTransform rt = streamObj.AddComponent<RectTransform>();
        
        // Add WaterStreamEffect component
        WaterStreamEffect waterStream = streamObj.AddComponent<WaterStreamEffect>();
        waterStream.Initialize(waterColor);
        
        // Delay trước khi bắt đầu stream
        if (delay > 0)
        {
            DOVirtual.DelayedCall(delay, () =>
            {
                if (waterStream != null)
                    waterStream.AnimateStream(startPos, endPos, duration);
            });
        }
        else
        {
            // Không delay, chạy ngay
            waterStream.AnimateStream(startPos, endPos, duration);
        }
    }
    
    private Vector3 GetTubeMouthPosition(bool isTilted)
    {
        if (tubeMouthPoint != null)
        if (tubeMouthPoint != null)
        {
            return tubeMouthPoint.position;
        }
        
        // Offset đơn giản dựa trên vị trí hiện tại
        if (isTilted)
        {
            // Khi nghiêng -45°, miệng ống dịch sang trái và lên trên
            // Tính toán: khi xoay -45°, điểm top của ống sẽ ở vị trí mới
            float cos45 = 0.707f; // cos(45°)
            float sin45 = 0.707f; // sin(45°)
            float halfHeight = tubeHeight * 0.5f;
            
            // Offset khi nghiêng: x giảm, y vẫn là chiều cao nhân cos(45)
            float offsetX = -halfHeight * sin45;  // Dịch sang trái
            float offsetY = halfHeight * cos45;   // Chiều cao giảm do nghiêng
            
            return transform.position + new Vector3(offsetX, offsetY, 0);
        }
        else
        {
            // Không nghiêng, miệng ống ở phía trên
            return transform.position + new Vector3(0, tubeHeight * 0.5f, 0);
        }
    }
    

    private Vector3 GetWaterLevelPosition()
    {
        if (Data == null) return transform.position;
        
        int liquidCount = Data.GetLiquidCount();
        
        // Mỗi segment chiếm 1/4 chiều cao ống (capacity = 4)
        float segmentHeight = tubeHeight / config.tubeCapacity;
        
        // Vị trí mức nước = đáy ống + (số segments * chiều cao 1 segment)
        float waterLevelOffset = liquidCount * segmentHeight;
        
        // Đáy ống ở vị trí: center - (height/2)
        float bottomY = transform.position.y - (tubeHeight * 0.5f);
        
        return new Vector3(transform.position.x, bottomY + waterLevelOffset, transform.position.z);
    }
    
    private void UpdateSeparators(List<ColorType> colors)
    {
        if (separatorLines == null || separatorLines.Length == 0) return;
        for (int i = 0; i < separatorLines.Length && i < colors.Count - 1; i++)
        {
            if (separatorLines[i] == null) continue;
            
            // Hiện separator nếu 2 segments liền kề cùng màu
            bool shouldShow = i < colors.Count - 1 && colors[i] == colors[i + 1] && colors[i] != ColorType.None;
            separatorLines[i].enabled = shouldShow;
        }
    }

    // UI Event System - Click handler
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance != null && GameManager.Instance.gameplayController != null)
            GameManager.Instance.gameplayController.OnTubeClicked(this);
    }

    public void RefreshOriginalPosition()
    {
        originalLocalPos = transform.localPosition;
        originalPos = transform.position;
    }

    public void SelectAnimation(bool isSelected)
    {
        if (isSelected)
        {
            isAnimating = true;
            StopIdleAnimation();
            
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayBottleUp();
        }
        else
        {
            isAnimating = false;
            
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayBottleDown();
        }
        
        if (layoutElement != null)
            layoutElement.ignoreLayout = isSelected;
        
        // Dùng localPosition để không bị ảnh hưởng bởi parent transform
        float targetY = originalLocalPos.y + (isSelected ? selectMoveDistance : 0f);
        
        // Kill animation cũ trước khi chạy mới
        transform.DOKill();
        
        transform.DOLocalMoveY(targetY, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => 
            {
                if (!isSelected && layoutElement != null)
                    layoutElement.ignoreLayout = false;
                
                if (!isSelected && !Data.IsCompleted())
                    StartIdleAnimation();
            });
    }

    public void PlayErrorShake()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayError();
            
        transform.DOShakePosition(0.2f, new Vector3(20f, 0, 0), 10, 90, false, true);
    }

    public void PlayPourAnimation(TubeView targetView, int amount, ColorType colorToMove, System.Action onComplete)
    {
        isAnimating = true;
        StopIdleAnimation();
        targetView.StopIdleAnimation();
        
        transform.DOKill();
        
        if (layoutElement != null)
            layoutElement.ignoreLayout = true;
        
        transform.SetAsLastSibling();
        
        Sequence seq = DOTween.Sequence();
        
        // 1. Bay lên và tiến gần target (điều chỉnh cho UI coordinates)
        Vector3 targetOffset = new Vector3(-150f, 120f, 0);
        seq.Append(transform.DOMove(targetView.transform.position + targetOffset, 0.3f).SetEase(Ease.OutQuad));
        
        seq.Append(transform.DORotate(new Vector3(0, 0, -45f), 0.25f).SetEase(Ease.InOutSine));

        int sourceCurrentIndex = Data.GetLiquidCount() - 1 + amount;
        int targetStartIndex = targetView.Data.GetLiquidCount() - amount;

        float pourDuration = 0.2f;
        float startPourTime = 0.55f;
        
        float soundTime = startPourTime + pourSoundOffset;
        seq.InsertCallback(soundTime, () => 
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayPour();
        });

        for (int i = 0; i < amount; i++)
        {
            int sourceIdx = sourceCurrentIndex - i;
            int targetIdx = targetStartIndex + i;
            
            float delayTime = startPourTime + (i * pourDuration);
            
            int capturedTargetIdx = targetIdx;
            
            seq.InsertCallback(delayTime, () => 
            {
                Vector3 sourceMouthPos = GetTubeMouthPosition(true);
                
                float segmentHeight = tubeHeight / config.tubeCapacity;
                float bottomY = targetView.transform.position.y - (tubeHeight * 0.5f);
                float waterLevelY = bottomY + (capturedTargetIdx * segmentHeight);
                Vector3 targetWaterLevelPos = new Vector3(targetView.transform.position.x, waterLevelY, targetView.transform.position.z);
                
                Color streamColor = config.GetColorValue(colorToMove);
                CreateWaterStream(sourceMouthPos, targetWaterLevelPos, streamColor, 0f, pourDuration);
            });
            
            if (sourceIdx >= 0 && sourceIdx < liquidImages.Length)
            {
                seq.Insert(delayTime, 
                    liquidImages[sourceIdx].DOFillAmount(0, pourDuration)
                        .SetEase(Ease.InOutSine)
                );
            }
            
            if (targetIdx >= 0 && targetIdx < targetView.liquidImages.Length)
            {
                seq.InsertCallback(delayTime, () => 
                {
                    targetView.liquidImages[targetIdx].color = config.GetColorValue(colorToMove);
                });
                seq.Insert(delayTime, 
                    targetView.liquidImages[targetIdx].DOFillAmount(1f, pourDuration)
                        .SetEase(Ease.InOutSine)
                );
            }
        }

        // 4. Quay về vị trí ban đầu
        seq.AppendInterval(0.15f);
        seq.Append(transform.DORotate(Vector3.zero, 0.25f).SetEase(Ease.InOutSine));
        seq.Append(transform.DOMove(originalPos, 0.35f).SetEase(Ease.OutQuad));
        
        seq.OnComplete(() => 
        {
            transform.localPosition = originalLocalPos;
            transform.SetSiblingIndex(originalSiblingIndex);
            
            if (layoutElement != null)
                layoutElement.ignoreLayout = false;
            
            // Stop pour sound khi animation rót xong
            if (SoundManager.Instance != null)
                SoundManager.Instance.StopPour();
            
            isAnimating = false;
            if (!Data.IsCompleted())
                StartIdleAnimation();
            if (!targetView.Data.IsCompleted())
                targetView.StartIdleAnimation();
            
            UpdateSeparatorsFromData();
            targetView.UpdateSeparatorsFromData();
            
            onComplete?.Invoke();
        });
    }
    
    private void UpdateSeparatorsFromData()
    {
        if (separatorLines == null || Data == null) return;
        
        List<ColorType> colors = new List<ColorType>();
        for (int i = 0; i < liquidImages.Length; i++)
        {
            if (liquidImages[i].fillAmount > 0)
            {
                // Lấy màu từ visual (có thể không chính xác 100% nhưng đủ dùng)
                // Hoặc có thể track trong list riêng
                colors.Add(ColorType.None); // Placeholder
            }
        }
        
        // Tạm thời disable tất cả separators (cần data structure tốt hơn để track colors)
        foreach (var sep in separatorLines)
        {
            if (sep != null)
                sep.enabled = false;
        }
    }

    // Dùng cho Undo hoặc Init nhanh
    public void ForceAddSegment(ColorType color)
    {
        int index = Data.GetLiquidCount() - 1; // Data đã được add rồi
        if (index >= 0 && index < liquidImages.Length)
        {
            liquidImages[index].color = config.GetColorValue(color);
            liquidImages[index].fillAmount = 1f;
        }
        
        UpdateSeparatorsFromData();
    }

    public void ForceRemoveTopSegment()
    {
        int index = Data.GetLiquidCount(); // Vị trí sắp xóa
        if (index >= 0 && index < liquidImages.Length)
        {
            liquidImages[index].fillAmount = 0;
            liquidImages[index].color = Color.clear;
        }
        
        if (!Data.IsCompleted() && tubeCap != null && tubeCap.activeSelf)
        {
            tubeCap.SetActive(false);
            
            if (!Data.IsEmpty())
                StartIdleAnimation();
        }
        
        UpdateSeparatorsFromData();
    }

    private void OnTubeCompleted()
    {
        StopIdleAnimation();
        
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBottleFull();
        
        transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.3f, 5, 1);
        
        if (tubeCap != null)
        {
            Sequence capSeq = DOTween.Sequence();
            capSeq.AppendInterval(0.3f);
            
            capSeq.AppendCallback(() => 
            {
                tubeCap.SetActive(true);
                tubeCap.transform.localScale = new Vector3(1, 0, 1);
                
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayBottleClose();
            });
            
            capSeq.Append(tubeCap.transform.DOScaleY(1f, capAnimDuration).SetEase(Ease.OutBounce));
            
            Image capImage = tubeCap.GetComponent<Image>();
            if (capImage != null)
                capSeq.Join(capImage.DOFade(0, 0.1f).From().SetLoops(3, LoopType.Yoyo));
        }
    }

    private void OnDestroy()
    {
        StopIdleAnimation();
        
        if (Data != null) 
            Data.OnTubeCompleted -= OnTubeCompleted;
    }
}