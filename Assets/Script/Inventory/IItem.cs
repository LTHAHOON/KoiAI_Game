using Mono.Cecil;
using UnityEngine;

public interface IItem<T>
{
    ItemCategory Category { get; }
    public Texture2D Texture { get; }
    T Controller { get; }
}
public abstract class Weapon<T> : MonoBehaviour, IItem<T>
{
    public ItemCategory Category => ItemCategory.Weapon;
    public abstract T Controller { get; }
}
public abstract class Resource<T> : MonoBehaviour, IItem<T>
{
    public ItemCategory Category => ItemCategory.Resource;
    public abstract T Controller { get; }
}
