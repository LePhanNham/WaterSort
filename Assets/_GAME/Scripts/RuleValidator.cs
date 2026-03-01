public static class RuleValidator
{
    public static bool CanPour(TubeData source, TubeData target)
    {
        if (source == null || target == null || source.Id == target.Id) return false;
        if (source.IsEmpty() || target.IsFull()) return false;
        if (target.IsEmpty()) return true;
        return source.PeekTop() == target.PeekTop();
    }
}