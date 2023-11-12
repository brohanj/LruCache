namespace CompanyName.Sdk.Utils.Core;

public interface ILruCache<in TKey, TValue>
    where TKey : notnull
{
    public void Set(TKey key, TValue value);

    public bool TryGetValue(TKey key, out TValue? value);
}
