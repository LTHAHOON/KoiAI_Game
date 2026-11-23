using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace KoiAI.UI
{
    public partial class MonitorHackingView
    {
        private async UniTaskVoid StartMonitorHacking(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_startHackingDelay), cancellationToken: token);

                MoveHackingTexts(token).Forget();
                StartTimeLimit(token).Forget();
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    ShuffleHackingTextPositions();
                    for (int i = 0; i < _hackingTexts_Pos.Count; ++i)
                    {
                        SpawnHackingText(_hackingTextPositionOrder[i]);
                        if (i < _hackingTexts_Pos.Count - 1)
                        {
                            float minDelay = Mathf.Max(0f, _minHackingTextDelay);
                            float maxDelay = Mathf.Max(minDelay, _maxHackingTextDelay);
                            float delay = UnityEngine.Random.Range(minDelay, maxDelay);
                            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
                        }
                    }
                    await UniTask.Delay(TimeSpan.FromSeconds(_hackingTextDelay), cancellationToken: token);
                }

            }
            catch (OperationCanceledException)
            {
                Debug.Log("ReStart");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private async UniTaskVoid StartTimeLimit(CancellationToken token)
        {
            try
            {
                _curRemainingTime = _timeLimit;
                while(_curRemainingTime > 0)
                {
                    token.ThrowIfCancellationRequested();
                    _curRemainingTime -=  Mathf.Min(Time.unscaledDeltaTime, 0.1f);
                    _timeText.SetTextFormat("{0:F2}", _curRemainingTime);
                    _timerGuageImage.fillAmount = Mathf.Clamp01(_curRemainingTime / _timeLimit);
                    _timerCompass.localEulerAngles = new(0f, 0f, -(360f - (_curRemainingTime * (360f / _timeLimit))));
                    await UniTask.NextFrame(token);
                    Debug.Log($"[ID:{GetInstanceID()}] Frame:{Time.frameCount}, dt:{Time.deltaTime}");
                }
                
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                if (!token.IsCancellationRequested && _completedCount < _completedWord.Length)
                {
                    ResetHackingSession();
                    ShowRestartButton();
                }
            }
        }

        private void ShuffleHackingTextPositions()
        {
            for (int i = _hackingTextPositionOrder.Length - 1; i > 0; --i)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                (_hackingTextPositionOrder[i], _hackingTextPositionOrder[randomIndex]) = (_hackingTextPositionOrder[randomIndex], _hackingTextPositionOrder[i]);
            }
        }

        private void SpawnHackingText(int positionIndex)
        {
            float dangerRatio = _dangerRatioWhenDanger;
            if (_hackingSlots[positionIndex].IsCompletedText)
            {
                dangerRatio = _dangerRatioWhenSuccess;
            }
            
            RectTransform textPosition = _hackingTexts_Pos[positionIndex];
            TMP_Text hackingText = _hackingTextPool.Pop();
            int codeNumber = UnityEngine.Random.Range(_minCodeNumber, _maxCodeNumber + 1);
            hackingText.SetTextFormat("0x{0}", codeNumber);
            float ratio = UnityEngine.Random.Range(0f, 100f);
            if(ratio <= dangerRatio)
            {
                hackingText.fontSharedMaterial = _dangerMaterialPreset;
            }
            else
            {
                hackingText.fontSharedMaterial = _successMaterialPreset;
            }
            hackingText.rectTransform.SetParent(textPosition, false);
            hackingText.rectTransform.anchoredPosition = Vector2.zero;
            _dicCurHackingTexts[textPosition].Add(hackingText);
        }

        private void ClearHackingTexts()
        {
            if (_dicCurHackingTexts == null || _hackingTextPool == null)
            {
                return;
            }

            foreach (List<TMP_Text> hackingTexts in _dicCurHackingTexts.Values)
            {
                foreach (TMP_Text hackingText in hackingTexts)
                {
                    hackingText.rectTransform.SetParent(_hackingText_PosParent, false);
                    hackingText.rectTransform.anchoredPosition = Vector2.zero;
                    _hackingTextPool.Return(hackingText);
                }
                hackingTexts.Clear();
            }
        }

        private async UniTask MoveHackingTexts(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    Rect hackingBarBounds = GetLocalBounds(_hackingBar, _hackingBarWorldCorners);
                    float removeHackingTextY = _screenRectTransform.InverseTransformPoint(_removeHackingTextTransform.position).y;
                    foreach (List<TMP_Text> hackingTexts in _dicCurHackingTexts.Values)
                    {
                        foreach (TMP_Text hackingText in hackingTexts)
                        {
                            float targetPosX = hackingText.rectTransform.anchoredPosition.x;
                            float targetPosY = hackingText.rectTransform.anchoredPosition.y - Mathf.Min(Time.unscaledDeltaTime, 0.1f) * _hackingTextSpeed;
                            hackingText.rectTransform.anchoredPosition = new(targetPosX, targetPosY);
                            Rect hackingTextBounds = GetLocalBounds(hackingText.rectTransform, _hackingTextWorldCorners);
                            if (hackingBarBounds.Overlaps(hackingTextBounds))
                            {
                                bool isSuccess = hackingText.fontSharedMaterial == _successMaterialPreset;
                                if (hackingText.fontSharedMaterial == _dangerMaterialPreset)
                                {
                                    ConsumeSignal();
                                }
                                if(isSuccess && !_hackingSlots[_curHackingBarIndex].IsCompletedText)
                                {
                                    ++_completedCount;
                                    _hackingSlots[_curHackingBarIndex].CompleteText();
                                }
                                else if(!isSuccess  && _hackingSlots[_curHackingBarIndex].IsCompletedText)
                                {
                                    --_completedCount;
                                    _hackingSlots[_curHackingBarIndex].RestartSuffleTextLoop();
                                }
                                OnReachHackingTextWithBar?.Invoke(isSuccess);
                                hackingTexts.Remove(hackingText);
                                _hackingTextPool.Return(hackingText);

                                if (_completedCount == _completedWord.Length)
                                {
                                    await StartLoadingRewardProgress(token);
                                    await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: token);
                                    token.ThrowIfCancellationRequested();
                                    StopHackingSession();
                                    _parentMonitorScreenView.OpenChildView<MonitorHackingRewardView>();
                                    return;
                                }
                                break;
                            }

                            if (hackingTextBounds.center.y <= removeHackingTextY)
                            {
                                hackingTexts.Remove(hackingText);
                                _hackingTextPool.Return(hackingText);
                                break;
                            }
                        }
                    }
                    await UniTask.NextFrame(token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("MoveRestart");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private Rect GetLocalBounds(RectTransform target, Vector3[] worldCorners)
        {
            target.GetWorldCorners(worldCorners);
            Vector3 firstCorner = _screenRectTransform.InverseTransformPoint(worldCorners[0]);
            float minX = firstCorner.x;
            float maxX = firstCorner.x;
            float minY = firstCorner.y;
            float maxY = firstCorner.y;

            for (int i = 1; i < worldCorners.Length; ++i)
            {
                Vector3 localCorner = _screenRectTransform.InverseTransformPoint(worldCorners[i]);
                minX = Mathf.Min(minX, localCorner.x);
                maxX = Mathf.Max(maxX, localCorner.x);
                minY = Mathf.Min(minY, localCorner.y);
                maxY = Mathf.Max(maxY, localCorner.y);
            }

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

    }
}
