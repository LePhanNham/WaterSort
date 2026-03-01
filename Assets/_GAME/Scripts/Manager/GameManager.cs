using UnityEngine;
using System;

public class GameManager : SingletonMono<GameManager>
{
    public GameConfig config;
    public GameplayController gameplayController;
    public UIManager uiManager;

    private IGameState currentState;
    
    public event Action OnGameWin;
    public event Action OnGameLose;

    protected override void Awake()
    {
        base.Awake();
        
        if (GetComponent<UISetupHelper>() == null)
        {
            gameObject.AddComponent<UISetupHelper>();
        }
    }

    private void Start()
    {
        ChangeState(new InitState(this));
    }

    public void ChangeState(IGameState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void TriggerWin()
    {
        ChangeState(new EndState(this, true));
        OnGameWin?.Invoke();
    }

    public void TriggerLose()
    {
        ChangeState(new EndState(this, false));
        OnGameLose?.Invoke();
    }
}




public enum GameState
{
    Playing, Win, Lose, Paused
}