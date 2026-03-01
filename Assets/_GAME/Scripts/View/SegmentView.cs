using UnityEngine;

public class SegmentView : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public ColorType currentColor;

    public void Setup(ColorType type, GameConfig config)
    {
        currentColor = type;
        spriteRenderer.color = config.GetColorValue(type);
    }
}