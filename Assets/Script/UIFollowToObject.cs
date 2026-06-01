using System;
using Unity.VisualScripting;
using UnityEngine;

public class UIFollowToObject : MonoBehaviour
{
    [SerializeField]
    RectTransform _canvasRectTransform;
    [SerializeField]
    private GameObject _targetObject;
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
    
    private RectTransform _rectTransform;
    private Camera _camera;
    private void Awake()
    {
        _camera = Camera.main;
        _rectTransform = GetComponent<RectTransform>();
    }
    
    //캐릭터 물리 이동 프레임에 맞추기
    private void FixedUpdate()
    {
        Vector3 targetWorldPos = _targetObject.transform.position + _worldOffset;
        Vector3 targetViewPortPos = _camera.WorldToViewportPoint(targetWorldPos) + _viewPortOffset;
        if (targetViewPortPos.z <= 0)
            return;
        _rectTransform.anchoredPosition = new Vector2(targetViewPortPos.x * _canvasRectTransform.rect.width, targetViewPortPos.y
            * _canvasRectTransform.rect.height);

        Vector3 dir = (_camera.transform.position - _targetObject.transform.position);
        float distance = dir.sqrMagnitude;
        float scale = distance / _maxDistance;
        scale = Mathf.Clamp(scale, _scaleMin, _scaleMax);
        Vector3 scaleVector = new Vector3(scale, scale, scale);
        transform.localScale = scaleVector + _scaleOffset;
    }
}
