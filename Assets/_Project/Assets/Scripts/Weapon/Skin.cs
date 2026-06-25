using UnityEngine;

public abstract class WeaponSkin : Skin { }
public abstract class ProjectileSkin : Skin { }
public abstract class Skin : MonoBehaviour
{
    [SerializeField]
    private GameObject _skinPrefab;

    public GameObject SkinPrefab => _skinPrefab;
}
