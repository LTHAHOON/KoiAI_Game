using Codice.CM.Common;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.UIElements;

public class ItemTestEdtior : EditorWindow
{
    public static List<ItemTestInfo> _itemTestInfoList = new();
    //VisualElement를 생성해주는 프리팹 객체
    private VisualTreeAsset _itemPrefab;
    //왼쪽 뷰에 있는 리스트뷰의 parent
    private VisualElement _itemList;
    //왼쪽 뷰에 있는 리스트뷰
    private ListView _itemViewList;
    //기본 아이콘 스프라이트
    private Sprite _basicItemIcon;

    //오른쪽 뷰에 있는 스크롤뷰
    private ScrollView _itemScrollView;
    //오른쪽 뷰에 있는 아이콘
    private VisualElement itemIcon;
    //선택된 아이템 정보
    private ItemTestInfo _currentItemInfo;

    [MenuItem("Tools/Test/ItemTestEditor")]
    public static void ShowEample()
    {
        ItemTestEdtior wnd = GetWindow<ItemTestEdtior>();
        wnd.titleContent = new GUIContent("ItemEditor");
        Vector2 minSize = new Vector2(1000f, 600f);
        wnd.minSize = minSize;
    }

    private void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        //보통 이 방식은 옛날방식이고 이제는 SerializeField를 사용해서 Default Reference방식을 선호합니다.
        string rootPath = AssetDatabase.GUIDToAssetPath("6a01cb204b3df274d9e9849c7c58eb77");
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(rootPath);
        //VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/PracticeProject/UIToolKit/CustomEdtiorSample/Editor/CustomEnditorTest.uxml");
        root.Add(visualTree.Instantiate());

        _itemScrollView = root.Q<ScrollView>("infos");
        _itemScrollView.style.visibility = Visibility.Hidden;
        itemIcon = root.Q<VisualElement>("icon");
        _itemPrefab = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/PracticeProject/UIToolKit/CustomEdtiorSample/Editor/ItemTestPrefab.uxml");
        LoadData();
        _itemList = root.Q("itemList");
        _basicItemIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/PracticeProject/UIToolKit/CustomEdtiorSample/ItemTestIcon/f.PNG");
        CreateListView();

        root.Q<Button>("NewBtn").clicked += NewBtn_Click;
        root.Q<Button>("DelBtn").clicked += DelBtn_Click;

        _itemScrollView.Q<TextField>("name").RegisterValueChangedCallback(evt =>
        {
            _currentItemInfo.name = evt.newValue;
            _itemViewList.Rebuild();
        });

        _itemScrollView.Q<ObjectField>("iconField").RegisterValueChangedCallback(evt =>
        {
            Sprite newSprite = evt.newValue as Sprite;
            _currentItemInfo.icon = newSprite == null ? _basicItemIcon : newSprite;
            itemIcon.style.backgroundImage = new(newSprite == null ? _basicItemIcon : newSprite);
            _itemViewList.Rebuild();
        });
    }

    private void CreateListView()
    {
        //프리팹 생성하는 함수
        Func<VisualElement> makeItem = () => _itemPrefab.CloneTree();

        Action<VisualElement, int> bindItem = (e, i) =>
        {
            if(_itemTestInfoList[i] == null)
            {
                return;
            }

            //스크립트 상에서는 auto Template 기능을 킬 수 없기 때문에 보통 직접 값을 할당해줘야 하고
            //SerializedObject를 사용하여 비슷하게 만들수 있다. 단, auto Template이든 아니든 background는 완전히 직접 바인딩을 해줘야한다.

            //   SerializedObject so = new(itemTestInfoList[i]);
            //   e.Bind(so);
            e.Q<VisualElement>("icon").style.backgroundImage = new(_itemTestInfoList[i].icon);
            e.Q<Label>("name").text = _itemTestInfoList[i].name;
        };
        _itemViewList = new(_itemTestInfoList, 40f, makeItem, bindItem);
        _itemViewList.selectionType = SelectionType.Single;
        _itemViewList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        _itemViewList.selectionChanged += SelectData;
        _itemList.Add(_itemViewList);

    }

    private void NewBtn_Click()
    {
        ItemTestInfo item = CreateInstance<ItemTestInfo>();
        item.name = "new itemTestInfo";
        item.icon = _basicItemIcon;

        AssetDatabase.CreateAsset(item, $"Assets/PracticeProject/UIToolKit/CustomEdtiorSample/Data/{item.id}.asset");
        _itemTestInfoList.Add(item);
        _itemViewList.Rebuild();
    }

    private void DelBtn_Click()
    {
        if(_currentItemInfo == null)
        {
            return;
        }
        string path = AssetDatabase.GetAssetPath(_currentItemInfo);
        AssetDatabase.DeleteAsset(path);
        _itemTestInfoList.Remove(_currentItemInfo);
        _itemViewList.Rebuild();
        _itemScrollView.style.visibility = Visibility.Hidden;
    }

    private void LoadData()
    {
        _itemTestInfoList.Clear();
        //string paths = Directory.GetFiles("Assets/PracticeProject/UIToolKit/CustomEdtiorSample/Data/*.asset");
        string[] itemGUID = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/PracticeProject/UIToolKit/CustomEdtiorSample/Data/" });
        for (int i = 0; i < itemGUID.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(itemGUID[i]);
            ItemTestInfo itemTestInfo = AssetDatabase.LoadAssetAtPath<ItemTestInfo>(path);
            _itemTestInfoList.Add(itemTestInfo);
        }
    }

    private void SelectData(IEnumerable<object> selectedItems)
    {
        _currentItemInfo = (ItemTestInfo)selectedItems.First();
        SerializedObject so = new SerializedObject(_currentItemInfo);
        _itemScrollView.Bind(so);

        if(_currentItemInfo.icon != null)
        {
            itemIcon.style.backgroundImage = new(_currentItemInfo.icon);
        }

        _itemScrollView.style.visibility = Visibility.Visible;
    }
}
