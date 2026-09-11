using UnityEngine;
using System;

namespace KoiAI.AI
{
    using KoiAI.AnimatorSystem;
    using KoiAI.Item;
    using KoiAI.Utilities;
    using NaughtyAttributes;
    using System.Collections.Generic;

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

            if(!Brain.TryGetComponent<WeaponActivateGroup>(out _weaponRandomGroup))
            {
                _weaponRandomGroup = Brain.GetComponentInChildren<WeaponActivateGroup>(true);
                if (_weaponRandomGroup == null)
                {
                    Debug.LogError("This Object needs WeaponActivateRandomGroup Component");
                    return;
                }
            }

            for (int i = 0; i < _weaponRandomGroup.ActivateTargets.Count; i++)
            {
                _weaponRandomGroup.ActivateTargets[i].ActivateTarget.Init(null);
            }

            if (Brain.AIAnimatorData.IsValid())
            {
                _animParamData = Brain.AIAnimatorData.AnimParamData;
            }
        }

        public override void EnterFeature()
        {
            if (Brain.IsDead)
            {
                return;
            }

            _weaponController = ActivateRandom.GetRandomActivateTarget(_weaponRandomGroup);
            _weaponController.StartAiming();
        }

        public override void UpdateFeature()
        {
            if (Brain.IsDead)
            {
                return;
            }

            Debug.Log("Attacking");

            if(_curAttackTime < _extensionData.AttackDelayTime)
            {
                _curAttackTime += Time.deltaTime;
                return;
            }
            _curAttackTime = 0f;
            _weaponController = ActivateRandom.GetRandomActivateTarget(_weaponRandomGroup);
            _weaponController.Activate();
        }

        public override void ExitFeature()
        {
            _weaponController.EndAiming();
        }
    }
}
