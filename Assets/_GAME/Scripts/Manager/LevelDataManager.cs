using UnityEngine;

public class LevelDataManager : SingletonMono<LevelDataManager>
{
    private const string CURRENT_LEVEL_KEY = "CurrentLevel";
    
    [Header("Level Data")]
    public int currentLevel = 1;
    
    protected override void Awake()
    {
        base.Awake();
        LoadLevelData();
    }
    
    public void LoadLevelData()
    {
        currentLevel = PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 1);
    }
    
    public void SaveLevelData()
    {
        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, currentLevel);
        PlayerPrefs.Save();
    }
    
    public void SetCurrentLevel(int level)
    {
        currentLevel = level;
        SaveLevelData();
    }
    
    public void CompleteLevel()
    {
        currentLevel++;
        SaveLevelData();
    }
    
    public void ResetAllProgress()
    {
        currentLevel = 1;
        SaveLevelData();
    }
    
    
#if UNITY_EDITOR
    [ContextMenu("Debug: Print Level Data")]
    private void DebugPrintLevelData()
    {
        Debug.Log("=== LEVEL DATA ===");
        Debug.Log($"Current Level: {currentLevel}");
        Debug.Log("==================");
    }
    
    [ContextMenu("Debug: Reset Progress")]
    private void DebugResetProgress()
    {
        ResetAllProgress();
    }
    
    [ContextMenu("Debug: Set Level 10")]
    private void DebugSetLevel10()
    {
        SetCurrentLevel(10);
    }
#endif
}
