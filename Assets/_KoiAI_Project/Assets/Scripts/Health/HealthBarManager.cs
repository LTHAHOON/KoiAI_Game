using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace KoiAI.Health
{
    public class HealthBarManager : MonoBehaviour
    {
        [Serializable]
        private struct HealthBarConnectData
        {
            #region 수동으로 연결할 체력바 데이터

            [SerializeField] 
            private HealthBar _healthBar;       
            [SerializeField]
            private Health _health;
        
            public HealthBar HealthBar => _healthBar;
            public Health Health => _health;

            #endregion
        }

        [Serializable]
        private struct HealthBarParentData
        {
            #region 체력 생성 위치 데이터

            [SerializeField]
            private HealthBarType _healthBarType;
            [SerializeField] 
            private Transform _parent;   
        
            public HealthBarType HealthBarType => _healthBarType;
            public Transform Parent => _parent;

            #endregion
        }
    
        [Header("수동으로 연결할 체력바 데이터")]
        [SerializeField]
        private HealthBarConnectData[] _healthBarConnectDatas;
        [Header("체력 생성 위치 데이터")]
        [SerializeField]
        private HealthBarParentData[] _healthBarParentDatas;
    
        private Dictionary<Health, HealthBar> _dicHealthBar;    
        private readonly Subject<Health> OnAddHealthBar = new();
        private readonly Subject<Health> OnRemoveHealthBar = new();
        private readonly Subject<Health> OnHealthChanged = new();
        public static HealthBarManager Instance { get; private set; }
    
        private void Awake()
        {
            Instance = this;
            Init();
        }

        private void Init()
        {
            InitHealthBarDictionary();
            AssignOnAddHealthBar();
            AssignOnRemoveHealthBar();
            AssginOnHealthChanged();
        }

        /// <summary>
        /// 수동으로 연결할 체력바 데이터를 딕셔너리에 추가세팅
        /// </summary>
        private void InitHealthBarDictionary()
        {
            _dicHealthBar = new();
            for (int i = 0; i < _healthBarConnectDatas.Length; i++)
            {
                _dicHealthBar.TryAdd(_healthBarConnectDatas[i].Health, _healthBarConnectDatas[i].HealthBar);
            }
        }
    
        /// <summary>
        /// 체력바 생성 로직 연결
        /// </summary>
        private void AssignOnAddHealthBar()
        {
            OnAddHealthBar
                .Where(health => _dicHealthBar.ContainsKey(health) == false)
                .Subscribe(health =>
                {
                    HealthBarData healthBarData = health.HealthData.HealthBarData;
                    if (healthBarData)
                    {
                        bool bGet = TryGetHealthBarParent(healthBarData.HealthBarType, out Transform parent);
                        if (bGet)
                        {
                            HealthBar newHealthBar = Instantiate(healthBarData.HealthBarPrefab, parent);
                            _dicHealthBar.Add(health, newHealthBar);    
                        }
                    }
                }).AddTo(this);
        }

        /// <summary>
        /// 체력바 삭제 로직 연결
        /// </summary>
        private void AssignOnRemoveHealthBar()
        {
            OnRemoveHealthBar
                .Where(health => _dicHealthBar.ContainsKey(health))
                .Subscribe(health =>
                {
                    _dicHealthBar.Remove(health);
                }).AddTo(this);
        }

        /// <summary>
        /// 체력바 수치 변경 로직 연결
        /// </summary>
        private void AssginOnHealthChanged()
        {
            OnHealthChanged.Subscribe(health =>
            {
                var healthBar = health.GetHealthBar();
                healthBar.ChangeHealthBar(health.CurrentHealth);
                if (health.CurrentHealth <= 0)
                {
                    OnRemoveHealthBar.OnNext(health);
                }
            
            }).AddTo(this);
        }
    
        /// <summary>
        /// TryGet: 체력바 생성할 위치 가져오기 
        /// </summary>
        private bool TryGetHealthBarParent(HealthBarType healthBarType, out Transform parent)
        {
            if (_healthBarParentDatas.Length > 0)
            {
                for (int i = 0; i < _healthBarParentDatas.Length; i++)
                {
                    if (_healthBarParentDatas[i].HealthBarType == healthBarType)
                    {
                        parent = _healthBarParentDatas[i].Parent;
                        return true;
                    }
                }
            }
            parent = null;
            return false;
        }
    
        /// <summary>
        ///  체력바 수치 변경 로직 실행
        /// </summary>
        public void ChangeHealth(Health health)
        {
            OnHealthChanged.OnNext(health);       
        }
    
        /// <summary>
        ///  체력바 생성 로직 실행
        /// </summary>
        public HealthBar CreateHealthBar(Health health)
        {
            OnAddHealthBar.OnNext(health);
            if (_dicHealthBar.TryGetValue(health, out HealthBar newHealthBar))
            {
                return newHealthBar;       
            }
            return null;
        }

        /// <summary>
        ///  TryGet: 체력바 객체 반환
        /// </summary>
        public bool TryGetHealthBar(Health health, out HealthBar healthBar)
        {
            bool isExist = IsExistHealthBar(health);
            if (isExist)
            {
                healthBar = _dicHealthBar[health];
            }
            else
            {
                healthBar = null;
            }
            return isExist;
        }
    
        /// <summary>
        /// Is: 체력바 존재에 대한 여부
        /// </summary>
        private bool IsExistHealthBar(Health health)
        {
            bool isExist = false;
            if (_dicHealthBar.Count > 0)
            {
                isExist = _dicHealthBar.ContainsKey(health);
            }
            return isExist;
        }
    }
}
