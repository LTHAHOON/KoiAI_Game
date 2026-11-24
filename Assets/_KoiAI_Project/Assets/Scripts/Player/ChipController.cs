using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using KoiAI.Rendering;

namespace KoiAI.Player
{
    using DG.Tweening;
    using KoiAI.Input;
    using KoiAI.UI;

    public class ChipController : MonoBehaviour
    {
        [SerializeField]
        private ChipView _chipView;
        [SerializeField]
        private Material _detectMaterial;
        [SerializeField]
        private Ease _detect_dissolveEase = Ease.Linear;
        [SerializeField]
        private float _detect_dissolveDuration = 0.35f;
        [SerializeField]
        private float _detect_duration = 10f;
        [SerializeField]
        private float _coolTime_duration = 5f;

        private Tween _detectDissolveTween;
        private readonly int DissolveId = Shader.PropertyToID("_Dissolve");
        private readonly int AlphaId = Shader.PropertyToID("_Alpha");
        private float _prevTime;
        private bool _isCoolingDown = false;
        private bool _isUsingChip = false;

        public void UnlockChip()
        {
            InitChip();
            _chipView.SetVisible(true);
        }

        private void OnDisable()
        {
            if (InputService.PlayerIA != null)
            {
                InputService.PlayerIA.Player.UseChip.performed -= UseChip;
            }

            if (_detectMaterial != null)
            {
                _detectMaterial.SetFloat(DissolveId, 1f);
                DetectMaskRendererFeature.SetScreenDissolve(1f, _detectMaterial.GetFloat(AlphaId));
            }

            DetectMaskRendererFeature.SetDetectionProgress(0f);
            if (_detectMaterial == null)
            {
                DetectMaskRendererFeature.SetScreenDissolve(1f, 0f);
            }
            _detectDissolveTween?.Kill();
        }

        public void InitChip()
        {
            if (InputService.PlayerIA == null || _detectMaterial == null)
            {
                Debug.LogError("ChipController requires initialized input and a detect material.", this);
                return;
            }

            InputService.PlayerIA.Player.UseChip.performed += UseChip;
            _detectMaterial.SetFloat(DissolveId, 1f);
            UpdateDetectScreenMask(1f);
            _detectDissolveTween = _detectMaterial.DOFloat(-1f, DissolveId, _detect_dissolveDuration)
            .SetEase(_detect_dissolveEase)
            .SetAutoKill(false)
            .OnUpdate(() =>
            {
                float dissolve = _detectMaterial.GetFloat(DissolveId);
                UpdateDetectScreenMask(dissolve);
            })
            .Pause();
        }

        private void UpdateDetectScreenMask(float dissolve)
        {
            DetectMaskRendererFeature.SetDetectionProgress(Mathf.InverseLerp(1f, -1f, dissolve));
            DetectMaskRendererFeature.SetScreenDissolve(dissolve, _detectMaterial.GetFloat(AlphaId));
        }

        private void UseChip(InputAction.CallbackContext context)
        {
            if(_isUsingChip || _isCoolingDown)
            {
                Debug.Log("쿨타임이 돌고있습니다.");
                return;
            }
            _isUsingChip = true;
            _chipView.SetChipCooltimeFiilAmount(1f);
            UseChipAsync().Forget();
        }

        private async UniTaskVoid UseChipAsync()
        {
            try
            {
                destroyCancellationToken.ThrowIfCancellationRequested();
                _detectDissolveTween?.PlayForward();
                await UniTask.Delay(TimeSpan.FromSeconds(_detect_dissolveDuration), cancellationToken: destroyCancellationToken);
                await UniTask.Delay(TimeSpan.FromSeconds(_detect_duration), cancellationToken: destroyCancellationToken);
                _detectDissolveTween?.PlayBackwards();
                await StartCooltimeAsync();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                _isUsingChip = false;
            }
        }

        private async UniTask StartCooltimeAsync()
        {
            try
            {
                _isCoolingDown = true;
                _prevTime = Time.unscaledTime;

                float curDuration = Time.unscaledTime - _prevTime;
                while(_coolTime_duration >= curDuration)
                {
                    destroyCancellationToken.ThrowIfCancellationRequested();
                    float fillAmount = Mathf.Clamp01(1f - (curDuration / _coolTime_duration));
                    _chipView.SetChipCooltimeFiilAmount(fillAmount);
                    await UniTask.Yield(PlayerLoopTiming.LastEarlyUpdate, destroyCancellationToken);
                    destroyCancellationToken.ThrowIfCancellationRequested();
                    curDuration = Time.unscaledTime - _prevTime;
                }
            }
            catch(OperationCanceledException) { }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                _isCoolingDown = false;
            }
        }
    }
}
