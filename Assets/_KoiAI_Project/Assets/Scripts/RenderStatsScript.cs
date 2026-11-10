using System.Text;
using Unity.Profiling;
using UnityEngine;
 
public class RenderStatsScript : MonoBehaviour
{
    string statsText;
    ProfilerRecorder setPassCallsRecorder;
    ProfilerRecorder drawCallsRecorder;
    ProfilerRecorder verticesRecorder;
    readonly StringBuilder statsBuilder = new(500);

    void OnEnable()
    {
        setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
    }
 
    void OnDisable()
    {
        setPassCallsRecorder.Dispose();
        drawCallsRecorder.Dispose();
        verticesRecorder.Dispose();
    }

    void Update()
    {
        statsBuilder.Clear();
        if (setPassCallsRecorder.Valid)
            statsBuilder.Append("SetPass Calls: ").Append(setPassCallsRecorder.LastValue).AppendLine();
        if (drawCallsRecorder.Valid)
            statsBuilder.Append("Draw Calls: ").Append(drawCallsRecorder.LastValue).AppendLine();
        if (verticesRecorder.Valid)
            statsBuilder.Append("Vertices: ").Append(verticesRecorder.LastValue).AppendLine();
        statsText = statsBuilder.ToString();
    }
 
    void OnGUI()
    {
        GUI.TextArea(new Rect(10, 30, 250, 50), statsText);
    }
}
