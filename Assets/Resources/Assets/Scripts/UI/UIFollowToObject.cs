using R3;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

[DefaultExecutionOrder(300)]
[RequireComponent(typeof(RectTransform))]
public class UIFollowToObject : MonoBehaviour
{
    [SerializeField]
    CanvasType _parentCanvasType;
    [SerializeField]
    private Vector3 _viewPortOffset;
    [SerializeField]
    private Vector3 _worldOffset;
    [SerializeField]
    private Vector3 _scaleOffset;
    [SerializeField]
    private float _maxDistance;
    [SerializeField]
    private float _scaleMax;
    [SerializeField]
    private float _scaleMin;
    
    public GameObject _targetObject;
    private RectTransform _rectTransform;
    private Camera _camera;
    private void Awake()
    {
        _camera = Camera.main;
        _rectTransform = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        UIFollowProcess();
    }

    private void UIFollowProcess()
    {
        Vector3 targetWorldPos = _targetObject.transform.position + _worldOffset;
        Vector3 targetViewPortPos = _camera.WorldToViewportPoint(targetWorldPos) + _viewPortOffset;
        if (targetViewPortPos.z <= 0)
            return;
        Debug.Log(targetViewPortPos.z);
        _rectTransform.anchoredPosition = UIManager.Instance.ViewPortToAnchoredPoint(targetViewPortPos, _parentCanvasType);

        Vector3 dir = (_camera.transform.position - _targetObject.transform.position);
        float distance = dir.sqrMagnitude;
        float scale = distance / _maxDistance;
        scale = Mathf.Clamp(scale, _scaleMin, _scaleMax);
        Vector3 scaleVector = new Vector3(scale, scale, scale);
        transform.localScale = scaleVector + _scaleOffset;
    }

    public void SetTargetObject(GameObject targetObj)
    {
        _targetObject = targetObj;
    }
}
