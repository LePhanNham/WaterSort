public interface IGameState
{
    void Enter();
    void Exit();
}

public class InitState : IGameState
{
    private GameManager game;
    public InitState(GameManager game) => this.game = game;

    public void Enter()
    {
        game.gameplayController.InitializeLevel();
        game.ChangeState(new PlayState(game));
    }
    public void Exit() { }
}

public class PlayState : IGameState
{
    private GameManager game;
    public PlayState(GameManager game) => this.game = game;

    public void Enter() { }
    public void Exit() { game.gameplayController.EnableInput(false); }
}

public class EndState : IGameState
{
    private GameManager game;
    private bool isWin;
    public EndState(GameManager game, bool isWin)
    {
        this.game = game;
        this.isWin = isWin;
    }

    public void Enter()
    {
        // UI sẽ lắng nghe event OnGameWin/Lose từ GameManager để hiện Popup
    }
    public void Exit() { }
}