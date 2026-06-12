using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class ActivateRandomValue<T> where T : Component
{
    [Header("성공 확률 값 (0~100 사이)")]
    [SerializeField]
    private float _chanceValue;
    [SerializeField]
    private T _activateTarget;

    public T ActivateTarget => _activateTarget;
    public float GetRandomValue()
    {
        float randomValue =  Random.Range(_chanceValue, 100f);
        return randomValue;
    }
}
