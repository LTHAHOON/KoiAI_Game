using System;
using R3;
using UnityEngine;

public class Health : UIFollowHandle
{
    [SerializeField]
    private float _maxHealth = 100;

    private ReactiveProperty<float> _currentHealth = new(0);
    private ReactiveProperty<HealthBar> _healthBar = new();
    private bool _isDelayChanging = false;
    private void Awake()
    {
        _currentHealth.Value = _maxHealth;
        _currentHealth
            .Skip(1)
            .Subscribe(healthValue =>
            {
                HealthBarManager.Instance.ChangeHealth(this);
                if (healthValue <= 0)
                {
                    OnDead();
                }
            }).AddTo(this);

        _healthBar
            .Where(healthBar => healthBar != null)
            .Subscribe(healthBar =>
            {
                if(healthBar.IsBlockFollowUI)
                {
                    return;
                }
                var connector = GetUIFollowConnector();
                connector.OnNext(healthBar);
            }).AddTo(this);
    }
    
    private void Start()
    {
        _healthBar.Value = HealthBarManager.Instance.CreateHealthBar();
        _healthBar.Value.Init(_currentHealth.CurrentValue, _maxHealth);
    }

    public void ChangeHealth(float amount)
    {
        _currentHealth.Value += amount;
    }

    public void ChangeDelayHealth(float amount, float delayTime)
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
    
    private void OnDead()
    {
        Debug.Log("Dead");
    }
    
    public HealthBar GetHealthBar() => _healthBar.Value;

    public override FollowableUI GetFollowableUI()
    {
        throw new NotImplementedException();
    }

    public float CurrentHealth => _currentHealth.CurrentValue;
    public float MaxHealth => _maxHealth;
}
