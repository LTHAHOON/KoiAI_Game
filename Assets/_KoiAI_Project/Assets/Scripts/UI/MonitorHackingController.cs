using UnityEngine;
using System;
using R3;
using UnityEngine.InputSystem;
using KoiAI.Pool;
using KoiAI.Utilities;

namespace KoiAI.UI
{
    using KoiAI.Input;
    using KoiAI.Interact;

    public class MonitorHackingController : MonoBehaviour
    {
        [SerializeField]
        private MonitorInteractable _monitorInteract;
        [SerializeField]
        private ParticleSystem _hackingFailedParticle;
        [SerializeField]
        private ParticleSystem _hackingSuccessParticle;
        [SerializeField]
        private PoolSize _hackingParticlePoolSize = new(1, 5);
        [SerializeField]
        private MonitorHackingView _monitorHackingView;
        [SerializeField]
        private float _holdDelayHackingBar = 0.5f;

        private Pool<ParticleSystem> _hackingFailedParticlePool;
        private Pool<ParticleSystem> _hackingSuccessParticlePool;
        private float _curHoldHackingBarAxis = 0f;
        private float _curHoldDelayTime = 0f;
        private bool _isHolding;

        private void Awake()
        {
            _monitorHackingView.OnReachHackingTextWithBar += OnReachHackingTextWithBar;

            _monitorInteract.OnInteract.Subscribe(context =>
            {
                InputService.PlayerIA.Monitor.Hacking_Move.started += OnStartedHackingMoveBar;
                InputService.PlayerIA.Monitor.Hacking_Move.performed += OnHoldHackingMoveBar;
                InputService.PlayerIA.Monitor.Hacking_Move.canceled += OnCancelHackingMoveBar;
            }).AddTo(this);
        }

        private void Start()
        {
            ulong poolID = _hackingFailedParticle.GetEntityULongID();
            PoolManager.Instance.AddPool(poolID, _hackingFailedParticle, _hackingParticlePoolSize, PoolName.MonitorHacking);
            PoolManager.Instance.TryGetPool(poolID, out _hackingFailedParticlePool);

            poolID = _hackingSuccessParticle.GetEntityULongID();
            PoolManager.Instance.AddPool(poolID, _hackingSuccessParticle, _hackingParticlePoolSize, PoolName.MonitorHacking);
            PoolManager.Instance.TryGetPool(poolID, out _hackingSuccessParticlePool);
        }

        private void Update()
        {
            if (_isHolding)
            {
                if (_curHoldDelayTime < _holdDelayHackingBar)
                {
                    _curHoldDelayTime += Time.unscaledDeltaTime;
                    return;
                }
                MoveHackingMoveBar(_curHoldHackingBarAxis);
                _curHoldDelayTime = 0f;
            }
        }

        private void MoveHackingMoveBar(float axis)
        {
            //오른쪽으로 바 이동
            if (axis > 0)
            {
                _monitorHackingView.MoveHackingBar(false);
            }
            //왼쪽으로 바 이동
            else if (axis < 0)
            {
                _monitorHackingView.MoveHackingBar(true);
            }
        }

        private void OnReachHackingTextWithBar(bool success)
        {
            Pool<ParticleSystem> particlePool = success ? _hackingSuccessParticlePool : _hackingFailedParticlePool;

            ParticleSystem particle = particlePool.Pop();
            particle.transform.SetPositionAndRotation(_monitorHackingView.HackingBarWorldPos, particle.transform.rotation);
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Play(true);

            ParticleSystem.MainModule main = particle.main;
            float returnDelay = main.duration + main.startDelay.constantMax + main.startLifetime.constantMax;
            PoolManager.Instance.ReturnDelay(particlePool, particle, returnDelay);
        }

        private void OnStartedHackingMoveBar(InputAction.CallbackContext context)
        {
            float axis = context.ReadValue<float>();
            MoveHackingMoveBar(axis);
        }

        private void OnHoldHackingMoveBar(InputAction.CallbackContext context)
        {
            _curHoldHackingBarAxis = context.ReadValue<float>();
            _isHolding = true;
            MoveHackingMoveBar(_curHoldHackingBarAxis);
        }

        private void OnCancelHackingMoveBar(InputAction.CallbackContext context)
        {
            _isHolding = false;
            _curHoldDelayTime = 0f;
            _curHoldHackingBarAxis = 0f;
        }

        private void OnDestroy()
        {
            _monitorHackingView.OnReachHackingTextWithBar -= OnReachHackingTextWithBar;
        }
    }
}
