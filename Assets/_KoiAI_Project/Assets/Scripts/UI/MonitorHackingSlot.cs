using Cysharp.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace KoiAI.UI
{
    public class MonitorHackingSlot : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _completedText;
        [SerializeField]
        private Material _dangerMaterialPreset;
        [SerializeField]
        private Material _successMaterialPreset;

        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private char _completedChar;
        private float _shuffleDelay;
        private Tween _suffleTextLoopTween;

        public void Initalize(char completedChar, float shuffleDelay)
        {
            _completedChar = completedChar;
            _shuffleDelay = shuffleDelay;
            _completedText.fontSharedMaterial = _dangerMaterialPreset;
            InitalizeSuffleTextTween();
        }
        
        private void InitalizeSuffleTextTween()
        {
            _suffleTextLoopTween ??= DOVirtual.DelayedCall(_shuffleDelay, () =>
            {
                int index = Random.Range(0, Chars.Length);
                _completedText.SetTextFormat("{0}", Chars[index]);
            }).SetLoops(-1).Pause();
        }

        public void StartShuffleTextLoop()
        {
            InitalizeSuffleTextTween();
            _completedText.fontSharedMaterial = _dangerMaterialPreset;
            _suffleTextLoopTween?.Play();
        }

        public void RestartSuffleTextLoop()
        {
            InitalizeSuffleTextTween();
            _completedText.fontSharedMaterial = _dangerMaterialPreset;
            _suffleTextLoopTween?.Restart();
        }

        public void EndShuffleTextLoop()
        {
            _suffleTextLoopTween?.Kill();
            _suffleTextLoopTween = null;
            _completedText.text = string.Empty;
        }

        public void CompleteText()
        {
            _suffleTextLoopTween.Pause();
            _completedText.SetTextFormat("{0}", _completedChar);
            _completedText.fontSharedMaterial = _successMaterialPreset;
        }

        public bool IsCompletedText => _suffleTextLoopTween != null && !_suffleTextLoopTween.IsPlaying();
    }
}
