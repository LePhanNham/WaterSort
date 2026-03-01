using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "SortWater/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Tube Settings")]
    public int tubeCapacity = 4;
    public int totalTubes = 8;
    public int emptyTubes = 2;
    
    [Header("Difficulty Settings")]
    public int startingColors = 3;
    public int maxColors = 8;
    public int colorsPerLevel = 1;
    
    [System.Serializable]
    public struct ColorMapping {
        public ColorType colorType;
        public Color colorValue;
    }
    
    public ColorMapping[] colorMappings;

    public Color GetColorValue(ColorType type)
    {
        if (type == ColorType.None) return Color.clear;
        
        foreach (var mapping in colorMappings)
        {
            if (mapping.colorType == type)
            {
                Color c = mapping.colorValue;
                c.a = 1f;
                return c;
            }
        }
        
        Debug.LogWarning($"Không tìm thấy màu cho {type}, dùng màu mặc định");
        return Color.white;
    }
}