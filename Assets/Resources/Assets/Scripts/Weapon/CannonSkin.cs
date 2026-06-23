using System;
using UnityEngine;


public class CannonSkin : WeaponSkin
{
    [SerializeField]
    private Transform _firePoint;
    [SerializeField]
    private AudioData _fireAudioData;
    [SerializeField]
    private ParticleSystem _firePT;

    public Transform FirePoint => _firePoint;
    public AudioData FireAudioData => _fireAudioData;
    public ParticleSystem FirePT => _firePT;
}
