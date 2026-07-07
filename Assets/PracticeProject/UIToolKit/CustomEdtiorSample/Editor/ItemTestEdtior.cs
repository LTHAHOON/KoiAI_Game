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
    public static List<ItemTestInfo> itemTestInfoList = new();
    private VisualTreeAsset itemPrefab;
    private VisualElement itemList;
    private ListView itemViewList;
    private Sprite basicItemIcon;

    private ScrollView itemScrollView;
    private VisualElement itemIcon;
    private ItemTestInfo currentItemInfo;

    [MenuItem("Tools/ItemEditor")]
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
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/PracticeProject/UIToolKit/CustomEdtiorSample/Editor/CustomEnditorTest.uxml");
        root.Add(visualTree.Instantiate());

        itemScrollView = root.Q<ScrollView>("infos");
        itemScrollView.style.visibility = Visibility.Hidden;
        itemIcon = root.Q<VisualElement>("icon");
        itemPrefab = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/PracticeProject/UIToolKit/CustomEdtiorSample/Editor/ItemTestPrefab.uxml");
        LoadData();
        itemList = root.Q("itemList");
        basicItemIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/PracticeProject/UIToolKit/CustomEdtiorSample/ItemTestIcon/f.PNG");
        CreateListView();

        root.Q<Button>("NewBtn").clicked += NewBtn_Click;
        root.Q<Button>("DelBtn").clicked += DelBtn_Click;

        itemScrollView.Q<TextField>("name").RegisterValueChangedCallback(evt =>
        {
            currentItemInfo.name = evt.newValue;
            itemViewList.Rebuild();
        });

        itemScrollView.Q<ObjectField>("iconField").RegisterValueChangedCallback(evt =>
        {
            Sprite newSprite = evt.newValue as Sprite;
            currentItemInfo.icon = newSprite == null ? basicItemIcon : newSprite;
            itemIcon.style.backgroundImage = new(newSprite == null ? basicItemIcon : newSprite);
            itemViewList.Rebuild();
        });
    }

    private void CreateListView()
    {
        //프리팹 생성하는 함수
        Func<VisualElement> makeItem = () => itemPrefab.CloneTree();

        Action<VisualElement, int> bindItem = (e, i) =>
        {
            if(itemTestInfoList[i] == null)
            {
                return;
            }

            //스크립트 상에서는 auto Template 기능을 킬 수 없기 때문에 보통 직접 값을 할당해줘야 하고
            //SerializedObject를 사용하여 비슷하게 만들수 있다. 단, auto Template이든 아니든 background는 완전히 직접 바인딩을 해줘야한다.

            //   SerializedObject so = new(itemTestInfoList[i]);
            //   e.Bind(so);
            e.Q<VisualElement>("icon").style.backgroundImage = new(itemTestInfoList[i].icon);
            e.Q<Label>("name").text = itemTestInfoList[i].name;
        };
        itemViewList = new(itemTestInfoList, 40f, makeItem, bindItem);
        itemViewList.selectionType = SelectionType.Single;
        itemViewList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        itemViewList.selectionChanged += SelectData;
        itemList.Add(itemViewList);

    }

    private void NewBtn_Click()
    {
        ItemTestInfo item = CreateInstance<ItemTestInfo>();
        item.name = "new itemTestInfo";
        item.icon = basicItemIcon;

        AssetDatabase.CreateAsset(item, $"Assets/PracticeProject/UIToolKit/CustomEdtiorSample/Data/{item.id}.asset");
        itemTestInfoList.Add(item);
        itemViewList.Rebuild();
    }

    private void DelBtn_Click()
    {
        if(currentItemInfo == null)
        {
            return;
        }
        string path = AssetDatabase.GetAssetPath(currentItemInfo);
        AssetDatabase.DeleteAsset(path);
        itemTestInfoList.Remove(currentItemInfo);
        itemViewList.Rebuild();
        itemScrollView.style.visibility = Visibility.Hidden;
    }

    private void LoadData()
    {
        itemTestInfoList.Clear();
        //string paths = Directory.GetFiles("Assets/PracticeProject/UIToolKit/CustomEdtiorSample/Data/*.asset");
        string[] itemGUID = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/PracticeProject/UIToolKit/CustomEdtiorSample/Data/" });
        for (int i = 0; i < itemGUID.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(itemGUID[i]);
            ItemTestInfo itemTestInfo = AssetDatabase.LoadAssetAtPath<ItemTestInfo>(path);
            itemTestInfoList.Add(itemTestInfo);
        }
    }

    private void SelectData(IEnumerable<object> selectedItems)
    {
        currentItemInfo = (ItemTestInfo)selectedItems.First();
        SerializedObject so = new SerializedObject(currentItemInfo);
        itemScrollView.Bind(so);

        if(currentItemInfo.icon != null)
        {
            itemIcon.style.backgroundImage = new(currentItemInfo.icon);
        }

        itemScrollView.style.visibility = Visibility.Visible;
    }
}
