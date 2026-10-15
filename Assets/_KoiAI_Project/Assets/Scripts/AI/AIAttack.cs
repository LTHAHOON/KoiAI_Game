using UnityEngine;
using System;

namespace KoiAI.AI
{
    using KoiAI.AnimatorSystem;
    using KoiAI.Item;
    using KoiAI.Utilities;

    [Serializable]
    public class AIAttackExtensionData : AIFeatureExtensionData
    {
        #region 공격 데이터
        [SerializeField]
        private float _attackDelayTime = 1f;

        #endregion
        public float AttackDelayTime => _attackDelayTime;
    }

    public class AIAttack : AIFeature
    {
        private AIAttackExtensionData _extensionData;
        private WeaponActivateGroup _weaponRandomGroup;
        private WeaponControllerBase _weaponController;
        private AIWeaponAim _weaponAim;
        private WeaponControllerBase _pendingShot;
        private int _pendingShotFrame = -1;
        private float _curAttackTime = 0f;
        private AnimatorParamData _animParamData;


        public override void InitFeature(AIFeatureValueData enemyFeatureValueData = null,
            AIFeatureExtensionData enemyFeatureExtensionData = null)
        {
            if(enemyFeatureExtensionData is not AIAttackExtensionData extensionData)
            {
                return;
            }
            _extensionData = extensionData;
            if(Brain.TryGetComponent(out _weaponAim))
            {
                _weaponAim.OnAimUpdated += OnAimUpdated;
                Brain.destroyCancellationToken.Register(() =>
                {
                    if (_weaponAim)
                    {
                        _weaponAim.OnAimUpdated -= OnAimUpdated;
                    }
                });
            }

            if(!Brain.TryGetComponent<WeaponActivateGroup>(out _weaponRandomGroup))
            {
                _weaponRandomGroup = Brain.GetComponentInChildren<WeaponActivateGroup>(true);
                if (_weaponRandomGroup == null)
                {
                    Debug.LogError("This Object needs WeaponActivateRandomGroup Component");
                    return;
                }
            }

            if (Brain.AIAnimatorData.IsValid())
            {
                _animParamData = Brain.AIAnimatorData.AnimParamData;
            }

            for (int i = 0; i < _weaponRandomGroup.ActivateTargets.Count; i++)
            {
                WeaponControllerBase weapon = _weaponRandomGroup.ActivateTargets[i].ActivateTarget;
                weapon.OnReloadStateChanged -= OnReloadStateChanged;
                weapon.OnReloadStateChanged += OnReloadStateChanged;
                Brain.destroyCancellationToken.Register(() => weapon.OnReloadStateChanged -= OnReloadStateChanged);
                weapon.Init(null);
            }
        }

        private void OnReloadStateChanged(bool isReloading)
        {
            if (Brain && _animParamData)
            {
                _animParamData.SetReloadParameter(Brain.AIAnimator, isReloading && !Brain.IsDead);
            }
        }

        public override void EnterFeature()
        {
            if (Brain.IsDead)
            {
                return;
            }

            _weaponController = ActivateRandom.GetRandomActivateTarget(_weaponRandomGroup);
            if (_weaponController)
            {
                _weaponController.StartAiming();
            }
        }

        public override void UpdateFeature()
        {
            if (Brain.IsDead || !_weaponController)
            {
                return;
            }

            if (_weaponController.IsFireLoading())
            {
                _curAttackTime = 0f;
                return;
            }

            //Debug.Log("Attacking");

            if(_curAttackTime < _extensionData.AttackDelayTime)
            {
                _curAttackTime += Time.deltaTime;
                return;
            }
            _curAttackTime = 0f;
            _pendingShot = null;
            _weaponController = ActivateRandom.GetRandomActivateTarget(_weaponRandomGroup);
            if (_weaponController)
            {
                if (_weaponAim && _weaponAim.isActiveAndEnabled && _weaponAim.Weapon == _weaponController)
                {
                    _pendingShot = _weaponController;
                    _pendingShotFrame = Time.frameCount;
                }
                else
                {
                    _weaponController.Activate();
                }
            }
        }

        private void OnAimUpdated()
        {
            if(Brain.IsDead || !Brain.TargetContext.HasTarget)
            {
                return;
            }
            if (!_pendingShot || _pendingShotFrame != Time.frameCount || _pendingShot != _weaponController)
            {
                return;
            }
            if(!_pendingShot.IsAiming() || _pendingShot.IsFireLoading())
            {
                return;
            }

            // 조준 보정이 끝난 후 발사합니다.
            _pendingShot.Activate();
            _pendingShot = null;
        }

        public override void ExitFeature()
        {
            _pendingShot = null;
            if(Brain.IsDead)
            {
                return;
            }
            if (_weaponController)
            {
                _weaponController.EndAiming();
                _weaponController = null;
            }
        }
    }
}
