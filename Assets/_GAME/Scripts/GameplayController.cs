using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameplayController : MonoBehaviour
{
    public Transform tubesContainer;
    public TubeView tubePrefab; 
    public WinPopupController winPopup;
    public LosePopupController losePopup;

    [Header("Level Management")]
    public int currentLevel = 1;
    private bool isFirstGameLoad = true;

    private List<TubeData> tubeDatas = new List<TubeData>();
    private List<TubeView> tubeViews = new List<TubeView>();
    
    private UndoSystem undoSystem = new UndoSystem();
    private TubeView selectedTubeView = null;
    private bool isInputEnabled = false;
    
    private void Start()
    {
        if (LevelDataManager.Instance != null)
        {
            currentLevel = LevelDataManager.Instance.currentLevel;
        }
    }

    public void InitializeLevel()
    {
        if (tubesContainer != null)
        {
            tubesContainer.gameObject.SetActive(true);
        }
        
        foreach(var view in tubeViews) 
        {
            if (view != null)
                Destroy(view.gameObject);
        }
        tubeDatas.Clear();
        tubeViews.Clear();
        undoSystem.Clear();

        if (GameManager.Instance == null || GameManager.Instance.config == null)
        {
            return;
        }

        GameConfig config = GameManager.Instance.config;

        GridLayoutGroup gridLayout = tubesContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.enabled = true;
        }

        for (int i = 0; i < config.totalTubes; i++)
        {
            TubeData data = new TubeData(i, config.tubeCapacity);
            tubeDatas.Add(data);
            
            TubeView view = Instantiate(tubePrefab, tubesContainer);
            
            LayoutElement layoutElement = view.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = view.gameObject.AddComponent<LayoutElement>();
            }
            layoutElement.ignoreLayout = false;
            
            tubeViews.Add(view);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(tubesContainer.GetComponent<RectTransform>());
        
        StartCoroutine(SetupTubesAfterLayout(config));
    }

    private System.Collections.IEnumerator SetupTubesAfterLayout(GameConfig config)
    {
        yield return new WaitForEndOfFrame();

        GenerateRandomLevel(config);
        
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(tubesContainer.GetComponent<RectTransform>());
        
        yield return new WaitForEndOfFrame();
        
        Dictionary<TubeView, Vector3> tubeGridPositions = new Dictionary<TubeView, Vector3>();
        for (int i = 0; i < tubeViews.Count; i++)
        {
            var tubeView = tubeViews[i];
            Vector3 pos = tubeView.transform.localPosition;
            tubeGridPositions[tubeView] = pos;
        }
        
        GridLayoutGroup gridLayout = tubesContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.enabled = false;
        }
        
        foreach (var tubeView in tubeViews)
        {
            LayoutElement layoutElement = tubeView.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                layoutElement.ignoreLayout = true;
            }
        }
        
        tubesContainer.gameObject.SetActive(true);
        yield return StartCoroutine(AnimateTubesAppear(tubeGridPositions));
    }
    
    public void OnHomeUIDismissed()
    {
        if (tubeViews.Count == 0)
        {
            InitializeLevel();
        }
        else
        {
            isInputEnabled = true;
        }
    }
    
    private System.Collections.IEnumerator AnimateTubesAppear(Dictionary<TubeView, Vector3> gridPositions)
    {
        foreach (var tubeView in tubeViews)
        {
            tubeView.transform.localScale = Vector3.zero;
            Vector3 gridPos = gridPositions[tubeView];
            tubeView.transform.localPosition = gridPos + Vector3.up * 100f;
        }
        
        float delayBetweenTubes = 0.05f;
        
        for (int i = 0; i < tubeViews.Count; i++)
        {
            TubeView tubeView = tubeViews[i];
            Vector3 targetPos = gridPositions[tubeView];
            
            Sequence tubeSeq = DOTween.Sequence();
            
            tubeSeq.Append(
                tubeView.transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
            );
            
            tubeSeq.Join(
                tubeView.transform.DOLocalMove(targetPos, 0.3f)
                    .SetEase(Ease.OutQuad)
            );
            
            tubeSeq.Play();
            
            yield return new WaitForSeconds(delayBetweenTubes);
        }
        
        yield return new WaitForSeconds(0.3f);
        
        isFirstGameLoad = false;
        isInputEnabled = true;
    }

    private void GenerateRandomLevel(GameConfig config)
    {
        int colorsForThisLevel = Mathf.Min(
            config.startingColors + (currentLevel - 1) * config.colorsPerLevel,
            config.maxColors
        );
        
        int maxPossibleColors = config.totalTubes - config.emptyTubes;
        colorsForThisLevel = Mathf.Min(colorsForThisLevel, maxPossibleColors);
        
        for (int i = 0; i < colorsForThisLevel; i++)
        {
            ColorType color = (ColorType)(i + 1);
            for (int j = 0; j < config.tubeCapacity; j++)
            {
                tubeDatas[i].AddLiquid(color, false);
            }
        }
        
        BreakAllCompletedTubes(colorsForThisLevel, config);
        
        int shuffleMoves = CalculateShuffleMoves(currentLevel, colorsForThisLevel, config.tubeCapacity);
        for (int move = 0; move < shuffleMoves; move++)
        {
            List<(int source, int target)> validMoves = new List<(int, int)>();
            
            for (int source = 0; source < config.totalTubes; source++)
            {
                if (tubeDatas[source].IsEmpty()) continue;
                
                for (int target = 0; target < config.totalTubes; target++)
                {
                    if (source == target) continue;
                    if (tubeDatas[target].IsFull()) continue;
                    
                    validMoves.Add((source, target));
                }
            }
            
            if (validMoves.Count == 0)
                break;
            
            var selectedMove = validMoves[Random.Range(0, validMoves.Count)];
            int sourceIdx = selectedMove.source;
            int targetIdx = selectedMove.target;
            
            ColorType topColor = tubeDatas[sourceIdx].PeekTop();
            int amountToMove = Random.Range(1, Mathf.Min(4, tubeDatas[sourceIdx].GetLiquidCount() + 1));
            amountToMove = Mathf.Min(amountToMove, config.tubeCapacity - tubeDatas[targetIdx].GetLiquidCount());
            
            for (int i = 0; i < amountToMove; i++)
            {
                if (tubeDatas[sourceIdx].IsEmpty()) break;
                if (tubeDatas[targetIdx].IsFull()) break;
                
                ColorType color = tubeDatas[sourceIdx].RemoveLiquid(false);
                tubeDatas[targetIdx].AddLiquid(color, false);
            }
        }
        
        int completedCount = 0;
        for (int i = 0; i < config.totalTubes; i++)
        {
            if (tubeDatas[i].IsCompleted() && !tubeDatas[i].IsEmpty())
                completedCount++;
        }
        
        if (completedCount > 0)
        {
            foreach (var data in tubeDatas)
            {
                while (!data.IsEmpty()) data.RemoveLiquid(false);
            }
            GenerateRandomLevel(config); 
            return;
        }
        
        for (int i = 0; i < tubeViews.Count; i++)
        {
            tubeViews[i].Setup(tubeDatas[i], config);
        }
    }
    
    private int CalculateShuffleMoves(int level, int colorCount, int tubeCapacity)
    {
        int baseMoves = colorCount * tubeCapacity; 
        int levelBonus = (level - 1) * 5; 
        int colorComplexity = colorCount * 2;
        
        int totalMoves = baseMoves + levelBonus + colorComplexity;
        
        totalMoves = Mathf.Clamp(totalMoves, 15, 150);
        
        return totalMoves;
    }

    private void BreakAllCompletedTubes(int colorCount, GameConfig config)
    {
        for (int i = 0; i < colorCount; i++)
        {
            if (!tubeDatas[i].IsCompleted() || tubeDatas[i].IsEmpty()) continue;
            
            int targetIdx = -1;
            for (int j = 0; j < config.totalTubes; j++)
            {
                if (i == j) continue;
                if (!tubeDatas[j].IsFull())
                {
                    targetIdx = j;
                    break;
                }
            }
            
            if (targetIdx == -1)
                continue;
            
            int amountToBreak = Random.Range(1, Mathf.Min(3, config.tubeCapacity));
            for (int k = 0; k < amountToBreak; k++)
            {
                if (tubeDatas[i].IsEmpty()) break;
                if (tubeDatas[targetIdx].IsFull()) break;
                
                ColorType color = tubeDatas[i].RemoveLiquid(false);
                tubeDatas[targetIdx].AddLiquid(color, false);
            }
        }
        
        int stillCompleted = 0;
        for (int i = 0; i < colorCount; i++)
        {
            if (tubeDatas[i].IsCompleted() && !tubeDatas[i].IsEmpty())
            {
                stillCompleted++;
            }
        }
        
        if (stillCompleted > 0)
        {
            BreakAllCompletedTubes(colorCount, config);
        }
    }

    public void EnableInput(bool enable) => isInputEnabled = enable;

    public void OnTubeClicked(TubeView clickedView)
    {
        if (!isInputEnabled) return;

        if (selectedTubeView == null)
        {
            if (!clickedView.Data.IsEmpty() && !clickedView.Data.IsCompleted())
            {
                selectedTubeView = clickedView;
                selectedTubeView.SelectAnimation(true);
            }
        }
        else
        {
            if (selectedTubeView == clickedView)
            {
                selectedTubeView.SelectAnimation(false); // Deselect
                selectedTubeView = null;
            }
            else if (RuleValidator.CanPour(selectedTubeView.Data, clickedView.Data))
            {
                ExecutePour(selectedTubeView, clickedView);
                selectedTubeView = null;
            }
            else
            {
                clickedView.PlayErrorShake(); // Báo lỗi
            }
        }
    }

    private void ExecutePour(TubeView sourceView, TubeView targetView)
    {
        isInputEnabled = false; 
        
        sourceView.transform.DOKill(); // Kill animation đang chạy
        sourceView.SelectAnimation(false);

        TubeData sourceData = sourceView.Data;
        TubeData targetData = targetView.Data;
        
        int movedAmount = 0;
        ColorType colorToMove = sourceData.PeekTop();

        // Xử lý Logic Data trước
        while (!sourceData.IsEmpty() && !targetData.IsFull() && sourceData.PeekTop() == colorToMove)
        {
            targetData.AddLiquid(sourceData.RemoveLiquid(false), false); // Không trigger event lẻ tẻ
            movedAmount++;
        }

        undoSystem.RecordCommand(new PourCommand(sourceData, targetData, movedAmount, sourceView, targetView));

        // Kích hoạt Animation tổng ở View
        sourceView.PlayPourAnimation(targetView, movedAmount, colorToMove, () => 
        {
            targetData.CheckAndTriggerCompletedEvent();
            sourceData.CheckAndTriggerCompletedEvent();
            
            CheckWinCondition();
            CheckLoseCondition();
            isInputEnabled = true;
        });
    }
    
    public void Undo() => undoSystem.UndoLastMove();

    public void NextLevel()
    {
        currentLevel++;
        
        if (LevelDataManager.Instance != null)
        {
            LevelDataManager.Instance.SetCurrentLevel(currentLevel);
        }
        
        InitializeLevel();
    }

    public void RestartLevel()
    {
        InitializeLevel();
    }
    
    public void ResetToHome()
    {
        foreach(var view in tubeViews)
        {
            if (view != null)
            {
                Destroy(view.gameObject);
            }
        }
        
        tubeDatas.Clear();
        tubeViews.Clear();
        undoSystem.Clear();
        
        isInputEnabled = false;
        selectedTubeView = null;
        
        if (tubesContainer != null)
        {
            tubesContainer.gameObject.SetActive(false);
        }
        
        isFirstGameLoad = true;
    }

    private void CheckWinCondition()
    {
        foreach (var data in tubeDatas)
        {
            if (!data.IsCompleted()) return;
        }
        
        StartCoroutine(WinCelebrationSequence());
    }
    
    private void CheckLoseCondition()
    {
        if (HasAnyValidMove())
        {
            return;
        }
        
        bool allCompleted = true;
        foreach (var data in tubeDatas)
        {
            if (!data.IsCompleted())
            {
                allCompleted = false;
                break;
            }
        }
        
        if (!allCompleted)
        {
            StartCoroutine(LoseSequence());
        }
    }
    
    private bool HasAnyValidMove()
    {
        for (int i = 0; i < tubeDatas.Count; i++)
        {
            if (tubeDatas[i].IsEmpty() || tubeDatas[i].IsCompleted())
                continue;
                
            for (int j = 0; j < tubeDatas.Count; j++)
            {
                if (i == j) continue;
                
                if (RuleValidator.CanPour(tubeDatas[i], tubeDatas[j]))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private System.Collections.IEnumerator LoseSequence()
    {
        isInputEnabled = false;
        yield return new WaitForSeconds(0.5f);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerLose();
        
            // show lose popup if assigned
            if (losePopup != null)
            {
                losePopup.ShowLose();
            }
        }
    }
    
    private System.Collections.IEnumerator WinCelebrationSequence()
    {
        isInputEnabled = false;
        yield return new WaitForSeconds(0.5f);
        
        Sequence celebrationSeq = DOTween.Sequence();
        
        foreach (var tubeView in tubeViews)
        {
            if (tubeView.Data.IsCompleted() && !tubeView.Data.IsEmpty())
            {
                celebrationSeq.Join(
                    tubeView.transform.DOScale(1.2f, 0.3f)
                        .SetEase(Ease.OutBack)
                        .SetLoops(2, LoopType.Yoyo)
                );
                
                celebrationSeq.Join(
                    tubeView.transform.DORotate(new Vector3(0, 0, 10f), 0.15f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(4, LoopType.Yoyo)
                );
            }
        }
        
        yield return celebrationSeq.WaitForCompletion();
        
        yield return new WaitForSeconds(0.3f);
        foreach (var tubeView in tubeViews)
        {
            tubeView.transform.localScale = Vector3.one;
            tubeView.transform.rotation = Quaternion.identity;
        }
        
        // Hoàn thành level và lưu progress
        if (LevelDataManager.Instance != null)
        {
            LevelDataManager.Instance.CompleteLevel();
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerWin();
        }
        
        // show win popup UI animation if assigned
        if (winPopup != null)
        {
            winPopup.ShowWin();
        }
    }
}