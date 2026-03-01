using System;
using System.Collections.Generic;
using System.Linq;

public class TubeData
{
    public int Id { get; private set; }
    public int Capacity { get; private set; }
    private Stack<ColorType> liquids;

    // Observer Pattern: Events cho View lắng nghe
    public event Action<ColorType, int> OnLiquidAdded; // color, currentCount
    public event Action<int> OnLiquidRemoved;          // remainingCount
    public event Action OnTubeCompleted;

    public TubeData(int id, int capacity)
    {
        Id = id;
        Capacity = capacity;
        liquids = new Stack<ColorType>(capacity);
    }

    public bool IsEmpty() => liquids.Count == 0;
    public bool IsFull() => liquids.Count == Capacity;
    public ColorType PeekTop() => IsEmpty() ? ColorType.None : liquids.Peek();
    public int GetLiquidCount() => liquids.Count;

    public void AddLiquid(ColorType color, bool triggerEvent = true)
    {
        if (IsFull()) return;
        liquids.Push(color);
        if (triggerEvent)
        {
            OnLiquidAdded?.Invoke(color, liquids.Count);
            if (IsCompleted() && !IsEmpty()) OnTubeCompleted?.Invoke();
        }
    }

    public ColorType RemoveLiquid(bool triggerEvent = true)
    {
        if (IsEmpty()) return ColorType.None;
        ColorType color = liquids.Pop();
        if (triggerEvent) OnLiquidRemoved?.Invoke(liquids.Count);
        return color;
    }

    public bool IsCompleted()
    {
        if (IsEmpty()) return true;
        if (!IsFull()) return false;
        ColorType first = liquids.Peek();
        return liquids.All(l => l == first);
    }
    
    public void CheckAndTriggerCompletedEvent()
    {
        if (IsCompleted() && !IsEmpty())
        {
            OnTubeCompleted?.Invoke();
        }
    }
    
    public void Clear() => liquids.Clear();
}

public enum ColorType { None, Red, Blue, Green, Yellow, Purple, Orange, Pink, Cyan }
