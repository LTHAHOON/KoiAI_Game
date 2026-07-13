using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI
{


    [RequireComponent(typeof(UIDocument))]
    public class CircleColorPicker : MonoBehaviour
    {
        private VisualElement root;
        private VisualElement circlePalette;
        private VisualElement picker;

        private bool isDragging = false;

        // 현재 선택된 색상 (외부에서 접근 가능)
        public Color SelectedColor { get; private set; } = Color.white;

        // 색상이 바뀔 때마다 실행할 이벤트
        public delegate void ColorChangedHandler(Color newColor);
        public event ColorChangedHandler OnColorChanged;

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;

            // UXML 요소 로드
            circlePalette = root.Q<VisualElement>("circlePalette");
            picker = root.Q<VisualElement>("picker");

            if (circlePalette == null || picker == null)
            {
                Debug.LogError("UXML에서 'CirclePalette' 또는 'Picker' 요소를 찾을 수 없습니다.");
                return;
            }

            // UI Toolkit 전용 포인터 이벤트 등록
            circlePalette.RegisterCallback<PointerDownEvent>(OnPointerDown);
            circlePalette.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            circlePalette.RegisterCallback<PointerUpEvent>(OnPointerUp);

            // 윈도우 크기 변경 등으로 레이아웃이 바뀔 때 피커 위치 재조정
            circlePalette.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnDisable()
        {
            if (circlePalette == null) return;
            circlePalette.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            circlePalette.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            circlePalette.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            circlePalette.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            isDragging = true;
            circlePalette.CapturePointer(evt.pointerId); // 마우스가 원판 밖으로 나가도 드래그 유지
            UpdatePickerAndColor(evt.position);
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (isDragging && circlePalette.HasPointerCapture(evt.pointerId))
            {
                UpdatePickerAndColor(evt.position);
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (isDragging)
            {
                circlePalette.ReleasePointer(evt.pointerId);
                isDragging = false;
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
            Rect bounds = circlePalette.worldBound;
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
            float pickerX = (bounds.width * 0.5f) + localOffset.x - (picker.layout.width * 0.5f);
            float pickerY = (bounds.height * 0.5f) + localOffset.y - (picker.layout.height * 0.5f);

            picker.style.left = pickerX;
            picker.style.top = pickerY;

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
            OnColorChanged?.Invoke(SelectedColor);

            // 콘솔로 색상 확인용 (원하지 않으면 삭제 가능)
            Debug.Log($"선택된 색상: {ColorUtility.ToHtmlStringRGB(SelectedColor)}");
        }

        private void MovePickerToCenter()
        {
            Rect bounds = circlePalette.layout;
            picker.style.left = (bounds.width * 0.5f) - (picker.layout.width * 0.5f);
            picker.style.top = (bounds.height * 0.5f) - (picker.layout.height * 0.5f);
        }
    }
}
