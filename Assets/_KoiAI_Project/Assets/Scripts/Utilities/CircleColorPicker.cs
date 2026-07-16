using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.Utilities
{
    public interface ICircleColorPickerHandler 
    {
        public void OnColorChanged(Color newColor);
    }

    public class CircleColorPicker
    {
        private VisualElement _root;
        private VisualElement _circlePalette;
        private VisualElement _picker;

        private bool _isDragging = false;

        public Color SelectedColor { get; private set; } = Color.white;
        private ICircleColorPickerHandler _colorPickerHandler;

        public CircleColorPicker(ICircleColorPickerHandler colorPickerHandler, VisualElement root, string circlePaletteName, string pickerName)
        {
            if (root == null || colorPickerHandler == null)
            {
                return;
            }
            Init(colorPickerHandler, root, circlePaletteName, pickerName);
        }

        public CircleColorPicker(ICircleColorPickerHandler colorPickerHandler, VisualElement root, bool bInitRegister, string circlePaletteName, string pickerName)
        {
            if (root == null || colorPickerHandler == null)
            {
                return;
            }
            Init(colorPickerHandler, root, circlePaletteName, pickerName);
            if (bInitRegister)
            {
                RegisterAllCallBack();
            }
        }

        private void Init(ICircleColorPickerHandler colorPickerHandler, VisualElement root, string circlePaletteName, string pickerName)
        {
            _root = root;
            _colorPickerHandler = colorPickerHandler;
            _circlePalette = _root.Q<VisualElement>(circlePaletteName);
            _picker = _root.Q<VisualElement>(pickerName);

            if (_circlePalette == null || _picker == null)
            {
                Debug.LogError("UXML에서 'CirclePalette' 또는 'Picker' 요소를 찾을 수 없습니다.");
                return;
            }
        }

        public void RegisterAllCallBack()
        {
            if (_circlePalette == null)
            {
                return;
            }
            _circlePalette.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _circlePalette.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _circlePalette.RegisterCallback<PointerUpEvent>(OnPointerUp);

            // 윈도우 크기 변경 등으로 레이아웃이 바뀔 때 피커 위치 재조정
            _circlePalette.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            MovePickerToCenter();
        }

        public void UnregisterAllCallBack()
        {
            if (_circlePalette == null)
            {
                return;
            }
            _circlePalette.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            _circlePalette.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            _circlePalette.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            _circlePalette.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            _isDragging = true;
            _circlePalette.CapturePointer(evt.pointerId); // 마우스가 원판 밖으로 나가도 드래그 유지
            UpdatePickerAndColor(evt.position);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_isDragging && _circlePalette.HasPointerCapture(evt.pointerId))
            {
                UpdatePickerAndColor(evt.position);
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (_isDragging)
            {
                _circlePalette.ReleasePointer(evt.pointerId);
                _isDragging = false;
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // 초기화 시 또는 화면 크기 변경 시 피커를 원판 중심에 배치
            MovePickerToCenter();
        }

        private void UpdatePickerAndColor(Vector2 screenPosition)
        {
            // 1. 전역 스크린 좌표를 원판 중심 기준의 로컬 좌표로 변환
            Rect bounds = _circlePalette.worldBound;
            Vector2 center = bounds.center;
            Vector2 localOffset = screenPosition - center;

            float maxRadius = bounds.width * 0.5f;
            float currentRadius = localOffset.magnitude;

            // 2. 피커가 원판 밖으로 나가지 못하도록 반지름 제한 (Clamp)
            if (currentRadius > maxRadius)
            {
                localOffset = localOffset.normalized * maxRadius;
                currentRadius = maxRadius;
            }

            // 3. 피커 UI 위치 업데이트 (원판 내부 좌표계 기준 계산)
            // UI Toolkit의 Absolute 포지션은 좌측 상단(0,0) 기준이므로 변환 필요
            float pickerX = (bounds.width * 0.5f) + localOffset.x - (_picker.layout.width * 0.5f);
            float pickerY = (bounds.height * 0.5f) + localOffset.y - (_picker.layout.height * 0.5f);

            _picker.style.left = pickerX;
            _picker.style.top = pickerY;

            // 4. 수학적 HSV 색상 계산
            // Atan2 결과를 0~360도로 변환 후 0~1 값으로 정규화 (Hue)
            float angle = Mathf.Atan2(-localOffset.y, localOffset.x) * Mathf.Rad2Deg; // Y축 반전 반영
            if (angle < 0) angle += 360f;
            float hue = angle / 360f;

            // 중심에서 멀어질수록 채도가 높아짐 (Saturation)
            float saturation = Mathf.Clamp01(currentRadius / maxRadius);

            // 명도(Value)는 기본값 1f (필요시 외부 슬라이더로 변경 가능)
            float value = 1f;

            // 5. 최종 색상 추출 및 이벤트 발송
            SelectedColor = Color.HSVToRGB(hue, saturation, value);
            _colorPickerHandler?.OnColorChanged(SelectedColor);

            // 콘솔로 색상 확인용 (원하지 않으면 삭제 가능)
            Debug.Log($"선택된 색상: {ColorUtility.ToHtmlStringRGB(SelectedColor)}");
        }

        private void MovePickerToCenter()
        {
            Rect bounds = _circlePalette.layout;
            _picker.style.left = (bounds.width * 0.5f) - (_picker.layout.width * 0.5f);
            _picker.style.top = (bounds.height * 0.5f) - (_picker.layout.height * 0.5f);
        }
    }
}
