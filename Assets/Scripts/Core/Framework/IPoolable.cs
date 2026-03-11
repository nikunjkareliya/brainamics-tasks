namespace Game.Framework
{
    /// <summary>
    /// Implement on Components that need reset callbacks when pooled.
    /// Optional — ObjectPool works with any Component, but calls these
    /// methods automatically on items that implement this interface.
    /// </summary>
    public interface IPoolable
    {
        void OnPoolGet();
        void OnPoolReturn();
    }
}
