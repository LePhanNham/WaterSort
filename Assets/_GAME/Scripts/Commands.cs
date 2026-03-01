using System.Collections.Generic;

public interface ICommand
{
    void Execute();
    void Undo();
}

public class PourCommand : ICommand
{
    private TubeData source;
    private TubeData target;
    private int segmentsMoved;
    private TubeView sourceView;
    private TubeView targetView;

    public PourCommand(TubeData source, TubeData target, int segmentsMoved, TubeView sourceView, TubeView targetView)
    {
        this.source = source;
        this.target = target;
        this.segmentsMoved = segmentsMoved;
        this.sourceView = sourceView;
        this.targetView = targetView;
    }

    public void Execute() { /* Logic đổ đã xử lý ở Controller để lấy animation, Command chủ yếu lưu Data */ }

    public void Undo()
    {
        for (int i = 0; i < segmentsMoved; i++)
        {
            ColorType color = target.RemoveLiquid(false); // Cập nhật data không trigger event thường
            source.AddLiquid(color, false);
            
            targetView.ForceRemoveTopSegment();
            sourceView.ForceAddSegment(color);
        }
    }
}

public class UndoSystem
{
    private Stack<ICommand> commandHistory = new Stack<ICommand>();

    public void RecordCommand(ICommand command)
    {
        commandHistory.Push(command);
    }

    public void UndoLastMove()
    {
        if (commandHistory.Count > 0)
        {
            ICommand lastCmd = commandHistory.Pop();
            lastCmd.Undo();
        }
    }
    
    public void Clear() => commandHistory.Clear();
}