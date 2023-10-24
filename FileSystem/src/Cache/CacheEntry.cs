namespace SharpGrip.FileSystem.Cache
{
    public class CacheEntry<TKey, TValue> : ICacheEntry
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }

        public CacheEntry(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}