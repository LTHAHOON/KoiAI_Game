using UnityEngine;

[CreateAssetMenu(fileName = "new ItemTestOS", menuName = "Practice/ItemTestOS")]
public class ItemTestOS : ScriptableObject
{
    public enum ETest { Test1, Test2}

    [SerializeField]
    private ETest _eTest;

    [SerializeField]
    private string _name;
}
