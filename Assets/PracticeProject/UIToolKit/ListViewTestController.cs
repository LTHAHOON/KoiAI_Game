using UnityEngine;
using UnityEngine.UIElements;

public class ListViewTestController : MonoBehaviour
{
    [SerializeField]
    private ItemTestOS[] itemTestOsData;
    private VisualElement _root;
    private ListView _listView;
    void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _listView = _root.Q<ListView>();
        _listView.itemsSource = itemTestOsData;
    }
}
