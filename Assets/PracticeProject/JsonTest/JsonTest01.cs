using Newtonsoft.Json;
using UnityEngine;

class Data
{
    public string name;
    public int level;
    public int coin = 100;
    public bool skill;
}

public class JsonTest01 : MonoBehaviour
{
    Data player = new Data() { name = "이태훈", level = 50, coin = 100, skill = false};
    void Start()
    {
        /*
        string jsonData = JsonUtility.ToJson(player);
        string jsonData2 = "{\"name\": \"이태훈\", \"level\": \"50\", \"skill\": \"true\"}";
        Data data = JsonUtility.FromJson<Data>(jsonData2);
        jsonData = JsonUtility.ToJson(data);    
        Debug.Log(jsonData);
        */

        /*
        JsonTestData jsonTestData = new JsonTestData();
        jsonTestData.hp = 123;
        jsonTestData.atk = 1234;
        jsonTestData.def = 12345;

        string jsonData = JsonUtility.ToJson(jsonTestData, true);
        Debug.Log(jsonData);*/

        JsonTestData[] jsonTestData = new JsonTestData[2];
        jsonTestData[0] = new();
        jsonTestData[0].hp = 123;
        jsonTestData[0].atk = 1234;
        jsonTestData[0].def = 12345;
        jsonTestData[1] = new();
        jsonTestData[1].hp = 12;
        jsonTestData[1].atk = 134;
        jsonTestData[1].def = 1345;
        string jsonData = JsonHelperTest.ToJson(jsonTestData, true);
        Debug.Log(jsonData);
    }
}
