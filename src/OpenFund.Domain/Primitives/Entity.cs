namespace OpenFund.Domain.Primitives;

[Serializable]
public abstract class Entity<TKey> where TKey : IComparable
{
    public virtual TKey Id { get; protected set; }
}