using R3;
using System;
using UnityEngine;

namespace KoiAI.Health
{
    using KoiAI.Core;
    using KoiAI.Interact;
    using KoiAI.ItemProp;
    using KoiAI.Utilities;

    public class Health : UIFollowHandle, IHealthProvider
    {
        [SerializeField]
        private HealthData _healthData;
        [SerializeField]
        private bool _hasHealthBar = true;
        private ReactiveProperty<float> _currentHealth = new(0);
        private bool _isDelayChanging = false;
        public Action<EntityIdentity> OnDeath;
        private bool _isDead = false;

        //체력에 영향을 주는 상대 데이터
        private EntityIdentity _lastInstigator;

        private void Awake()
        {
            _currentHealth.Value = _healthData.MaxHealth;
            _currentHealth
                .Skip(1)
                .Subscribe(healthValue =>
                {
                    if (!_isDead && healthValue <= 0)
                    {
                        _isDead = true;
                        OnDeath?.Invoke(_lastInstigator);
                    }
                }).AddTo(this);
        }

        private void Start()
        {
            if (_hasHealthBar)
            {
                HealthBarManager.Instance.CreateOrGetHealthBar(this);
            }
        }

        public void ChangeHealth(float amount, EntityIdentity instigator = null)
        {
            _lastInstigator = instigator;

            _currentHealth.Value += amount;
        }

        public void ChangeDelayHealth(float amount, float delayTime, EntityIdentity instigator = null)
        {
            if (_isDelayChanging)
            {
                return;
            }
            _isDelayChanging = true;
            Observable.Timer(TimeSpan.FromSeconds(delayTime))
                .Subscribe(_ =>
                {
                    _isDelayChanging = false;
                    ChangeHealth(amount);
                });
        }


        public void RefreshItemPickUpCondition(ItemPickUpCondition currentConditionData, ItemPickUpCondition compareCondition)
        {
            var conditionData = currentConditionData.hpCompareCondition;

            conditionData.SetCompareValue(CurrentHealthRatio);
            currentConditionData.hpCompareCondition = conditionData;
        }

        public override FollowableUI RegisterUIFollowHandle()
        {
            return HealthBarManager.Instance.CreateOrGetHealthBar(this);
        }

        public float CurrentHealthRatio => Mathf.Clamp01(CurrentHealth / MaxHealth);
        public float CurrentHealth => _currentHealth.CurrentValue;
        public float MaxHealth => _healthData.MaxHealth;
        public bool IsDead => _isDead;
        public HealthData HealthData => _healthData;
        //Observable<float> 대신 ReadOnlyReactiveProperty<float> 사용
        public ReadOnlyReactiveProperty<float> CurrentHealthReactive => _currentHealth;
    }
}
