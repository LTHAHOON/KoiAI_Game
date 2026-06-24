using UnityEngine;

public enum CanvasType
{
    BaseCanvas,
    BloomCanvas,
}

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Canvas _baseCanvas;
    [SerializeField]
    private Canvas _bloomCanvas;

    public static UIManager Instance { get; private set; }
    public void Awake()
    {
        Instance = this;
    }

    public Vector2 ViewPortToAnchoredPoint(Vector3 viewPort, CanvasType canvasType = CanvasType.BaseCanvas)
    {
        RectTransform canvasRect = canvasType switch
        {
            CanvasType.BaseCanvas => _baseCanvas.transform as RectTransform,
            CanvasType.BloomCanvas => _bloomCanvas.transform as RectTransform,
            _ => null
        };
        if(!canvasRect)
        {
            return Vector3.zero;
        }

        Vector2 anchoredPoint = new(viewPort.x * canvasRect.rect.width, viewPort.y * canvasRect.rect.height);
        return anchoredPoint;
    }
}
