using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.Utilities.FadeInOutColor
{
    [CustomEditor(typeof(TimeFadeCondition))]
    public class TimeFadeConditionEditor : UnityEditor.Editor
    {
        private SerializedProperty _duration;
        private SerializedProperty _fadeInStartTime;
        private SerializedProperty _fadeInEndTime;
        private SerializedProperty _fadeOutStartTime;
        private SerializedProperty _fadeOutEndTime;

        private FloatField _durationField;
        private FloatField _fadeInStartField;
        private FloatField _fadeInEndField;
        private FloatField _fadeOutStartField;
        private FloatField _fadeOutEndField;
        private MinMaxSlider _fadeInSlider;
        private MinMaxSlider _fadeOutSlider;
        private VisualElement _fadeInRange;
        private VisualElement _fadeOutRange;
        private Label _endTimeLabel;

        private void OnEnable()
        {
            _duration = serializedObject.FindProperty("_duration");
            _fadeInStartTime = serializedObject.FindProperty("_fadeInStartTime");
            _fadeInEndTime = serializedObject.FindProperty("_fadeInEndTime");
            _fadeOutStartTime = serializedObject.FindProperty("_fadeOutStartTime");
            _fadeOutEndTime = serializedObject.FindProperty("_fadeOutEndTime");
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            _durationField = new FloatField("Duration");
            _durationField.RegisterValueChangedCallback(OnDurationChanged);
            root.Add(_durationField);

            root.Add(new Label("Fade Timeline")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 8f,
                    marginBottom = 4f
                }
            });

            root.Add(CreateTimeline());
            root.Add(CreateTimeRange(
                "Fade In",
                _fadeInStartTime,
                _fadeInEndTime,
                out _fadeInStartField,
                out _fadeInEndField,
                out _fadeInSlider));
            root.Add(CreateTimeRange(
                "Fade Out",
                _fadeOutStartTime,
                _fadeOutEndTime,
                out _fadeOutStartField,
                out _fadeOutEndField,
                out _fadeOutSlider));

            _durationField.SetValueWithoutNotify(Mathf.Max(0.01f, _duration.floatValue));
            RefreshRangeControls();
            root.TrackSerializedObjectValue(serializedObject, _ => RefreshRangeControls());

            return root;
        }

        private VisualElement CreateTimeline()
        {
            VisualElement timeline = new VisualElement();
            timeline.style.marginBottom = 4f;

            _fadeInRange = CreateTimelineRow(timeline, "In", new Color(0.25f, 0.65f, 0.95f));
            _fadeOutRange = CreateTimelineRow(timeline, "Out", new Color(0.95f, 0.55f, 0.25f));

            VisualElement timeLabels = new VisualElement();
            timeLabels.style.flexDirection = FlexDirection.Row;
            timeLabels.style.justifyContent = Justify.SpaceBetween;
            timeLabels.style.marginLeft = 30f;
            timeLabels.style.marginRight = 38f;
            timeLabels.Add(new Label("0s"));
            _endTimeLabel = new Label();
            timeLabels.Add(_endTimeLabel);
            timeline.Add(timeLabels);

            return timeline;
        }

        private static VisualElement CreateTimelineRow(VisualElement parent, string label, Color rangeColor)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.height = 14f;

            Label rowLabel = new Label(label);
            rowLabel.style.width = 30f;
            row.Add(rowLabel);

            VisualElement track = new VisualElement();
            track.style.flexGrow = 1f;
            track.style.height = 9f;
            track.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
            track.style.position = Position.Relative;

            VisualElement range = new VisualElement();
            range.style.position = Position.Absolute;
            range.style.top = 0f;
            range.style.bottom = 0f;
            range.style.backgroundColor = rangeColor;
            track.Add(range);
            row.Add(track);

            VisualElement endSpacer = new VisualElement();
            endSpacer.style.width = 38f;
            row.Add(endSpacer);

            parent.Add(row);
            return range;
        }

        private VisualElement CreateTimeRange(
            string label,
            SerializedProperty startProperty,
            SerializedProperty endProperty,
            out FloatField startField,
            out FloatField endField,
            out MinMaxSlider rangeSlider)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            Label rowLabel = new Label(label);
            rowLabel.style.width = 65f;
            row.Add(rowLabel);

            startField = CreateTimeField(startProperty);
            startField.style.width = 48f;
            row.Add(startField);

            rangeSlider = new MinMaxSlider();
            rangeSlider.style.flexGrow = 1f;
            rangeSlider.RegisterValueChangedCallback(evt =>
                UpdateRange(startProperty, endProperty, evt.newValue.x, evt.newValue.y));
            row.Add(rangeSlider);

            endField = CreateTimeField(endProperty);
            endField.style.width = 48f;
            row.Add(endField);

            startField.RegisterValueChangedCallback(evt =>
                UpdateRange(startProperty, endProperty, evt.newValue, endProperty.floatValue));
            endField.RegisterValueChangedCallback(evt =>
                UpdateRange(startProperty, endProperty, startProperty.floatValue, evt.newValue));

            return row;
        }

        private FloatField CreateTimeField(SerializedProperty property)
        {
            FloatField field = new FloatField();
            field.SetValueWithoutNotify(property.floatValue);
            return field;
        }

        private void OnDurationChanged(ChangeEvent<float> evt)
        {
            float duration = Mathf.Max(0.01f, evt.newValue);
            serializedObject.Update();
            _duration.floatValue = duration;
            ClampRange(_fadeInStartTime, _fadeInEndTime, duration);
            ClampRange(_fadeOutStartTime, _fadeOutEndTime, duration);
            serializedObject.ApplyModifiedProperties();

            _durationField.SetValueWithoutNotify(duration);
            RefreshRangeControls();
        }

        private void UpdateRange(
            SerializedProperty startProperty,
            SerializedProperty endProperty,
            float start,
            float end)
        {
            float duration = Mathf.Max(0.01f, _duration.floatValue);
            start = Mathf.Clamp(start, 0f, duration);
            end = Mathf.Clamp(end, start, duration);

            serializedObject.Update();
            startProperty.floatValue = start;
            endProperty.floatValue = end;
            serializedObject.ApplyModifiedProperties();
            RefreshRangeControls();
        }

        private void RefreshRangeControls()
        {
            if (_duration == null || _fadeInSlider == null || _fadeOutSlider == null)
            {
                return;
            }

            serializedObject.Update();
            float duration = Mathf.Max(0.01f, _duration.floatValue);
            _durationField.SetValueWithoutNotify(duration);
            _endTimeLabel.text = $"{_duration.floatValue:0.##}s";

            RefreshRange(
                _fadeInStartTime,
                _fadeInEndTime,
                _fadeInStartField,
                _fadeInEndField,
                _fadeInSlider,
                _fadeInRange,
                duration);
            RefreshRange(
                _fadeOutStartTime,
                _fadeOutEndTime,
                _fadeOutStartField,
                _fadeOutEndField,
                _fadeOutSlider,
                _fadeOutRange,
                duration);
        }

        private static void RefreshRange(
            SerializedProperty startProperty,
            SerializedProperty endProperty,
            FloatField startField,
            FloatField endField,
            MinMaxSlider rangeSlider,
            VisualElement rangeElement,
            float duration)
        {
            float start = Mathf.Clamp(startProperty.floatValue, 0f, duration);
            float end = Mathf.Clamp(endProperty.floatValue, start, duration);

            startField.SetValueWithoutNotify(start);
            endField.SetValueWithoutNotify(end);
            rangeSlider.lowLimit = 0f;
            rangeSlider.highLimit = duration;
            rangeSlider.SetValueWithoutNotify(new Vector2(start, end));
            rangeElement.style.left = Length.Percent(start / duration * 100f);
            rangeElement.style.width = Length.Percent((end - start) / duration * 100f);
        }

        private static void ClampRange(SerializedProperty startProperty, SerializedProperty endProperty, float duration)
        {
            float start = Mathf.Clamp(startProperty.floatValue, 0f, duration);
            float end = Mathf.Clamp(endProperty.floatValue, start, duration);
            startProperty.floatValue = start;
            endProperty.floatValue = end;
        }
    }
}
