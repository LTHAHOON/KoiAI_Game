using System;
using R3;
using UnityEngine;

public class RepeatTest : MonoBehaviour
{
    private int _damage = 10;
    private int _curHP = 100;
    private int count = 0;
    private Subject<int> _damageSubject = new();
    private void Start()
    {
        _damageSubject.Subscribe(damage =>
        {
            Observable.Interval(TimeSpan.FromSeconds(2))
                .Where(_ =>
                {
                    if (count >= 3)
                    {
                        count = 0;
                        return false;
                    }
                    return true;
                })
                .Subscribe(_ =>
                {
                    ++count;
                    _curHP -= damage;
                    Debug.Log($"HP: {_curHP}");

                });
        });
        Damage();
    }

    public void Damage()
    {
        _damageSubject.OnNext(_damage);
    }
}
