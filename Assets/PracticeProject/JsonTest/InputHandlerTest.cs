using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class InputHandlerTest : MonoBehaviour
{
    [SerializeField]
    private Button _btn;
    private List<InputEntry> inputEntries = new List<InputEntry>();

    private void Start()
    {
        _btn.onClick.AddListener(WriteEntry);
        inputEntries = FileHelperTest.ReadFromJSON<InputEntry>("jsonInputTest.json");
    }

    public void WriteEntry()
    {
        inputEntries.Add(new InputEntry(Random.Range(0, 100), Random.Range(0, 100)));
        FileHelperTest.SaveToJSON(inputEntries, "jsonInputTest.json");

    }
}


[Serializable]
public class InputEntry
{
    public int a;
    public int b;
    
    public InputEntry(int A, int B)
    {
        this.a = A;
        this.b = B;
    }
}