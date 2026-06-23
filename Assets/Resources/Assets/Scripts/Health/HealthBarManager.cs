using System.Collections.Generic;
using R3;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    [SerializeField]
    private List<HealthBar> _healthBarList;    
    [SerializeField]
    private HealthBar _healthBarPrefab;
    
    private Subject<HealthBar> OnAddHealthBar = new();
    private Subject<HealthBar> OnRemoveHealthBar = new();
    private Subject<Health> OnHealthChanged = new();
    public static HealthBarManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        
        OnAddHealthBar
            .Where(healthBar => _healthBarList.Contains(healthBar) == false)
            .Subscribe(healthBar =>
            {
                _healthBarList.Add(healthBar);
            }).AddTo(this);
        
        OnRemoveHealthBar
            .Where(healthBar => _healthBarList.Contains(healthBar))
            .Subscribe(healthBar =>
            {
                _healthBarList.Remove(healthBar);
            }).AddTo(this);
        
        OnHealthChanged.Subscribe(health =>
        {
            var healthBar = health.GetHealthBar();
            healthBar.ChangeHealthBar(health.CurrentHealth);
            if (health.CurrentHealth <= 0)
            {
                OnRemoveHealthBar.OnNext(healthBar);
            }
            
        }).AddTo(this);

    }

    public void ChangeHealth(Health health)
    {
        OnHealthChanged.OnNext(health);       
    }
    
    public HealthBar CreateHealthBar()
    {
        HealthBar newHealthBar = Instantiate(_healthBarPrefab, transform);
        OnAddHealthBar.OnNext(newHealthBar);
        return newHealthBar;
    }
    
}
