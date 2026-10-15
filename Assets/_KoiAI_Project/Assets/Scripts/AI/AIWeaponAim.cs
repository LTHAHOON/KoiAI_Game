using UnityEngine;
using System;

namespace KoiAI.AI
{
    using KoiAI.Item;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(AIBrain), typeof(Animator))]
    public class AIWeaponAim : MonoBehaviour
    {
        #region 조준 데이터

        [SerializeField]
        private WeaponControllerBase _weapon;
        [SerializeField]
        private Transform _weaponPivot;
        [SerializeField]
        private Transform _firePoint;
        [Min(0.01f)]
        [SerializeField]
        private float _blendSpeed = 6f;
        [Range(0f, 120f)]
        [SerializeField]
        private float _maxAimAngle = 100f;
        [Range(0f, 1f)]
        [SerializeField]
        private float _bodyClearance = 0.12f;

        #endregion

        private AIBrain _brain;
        private Animator _animator;
        private Transform _hips;
        private Transform _chest;
        private Arm _leftArm;
        private Arm _rightArm;
        private Transform _cachedTarget;
        private Collider _targetCollider;
        private Vector3 _weaponLocalPosition;
        private Quaternion _weaponLocalRotation;
        private bool _restoreWeapon;
        private float _weight;

        public WeaponControllerBase Weapon => _weapon;
        public event Action OnAimUpdated;

        private bool CanAim => _brain && _brain.isActiveAndEnabled && !_brain.IsDead
            && _brain.TargetContext.HasTarget && _weapon && _weapon.IsAiming() && !_weapon.IsFireLoading();

        private sealed class Arm
        {
            public Transform Upper;
            public Transform Lower;
            public Transform Hand;
            private Quaternion _upperRotation;
            private Quaternion _lowerRotation;
            private Quaternion _handRotation;

            public void SavePose()
            {
                _upperRotation = Upper.localRotation;
                _lowerRotation = Lower.localRotation;
                _handRotation = Hand.localRotation;
            }

            public void RestorePose()
            {
                Upper.localRotation = _upperRotation;
                Lower.localRotation = _lowerRotation;
                Hand.localRotation = _handRotation;
            }

            public float Length => Vector3.Distance(Upper.position, Lower.position)
                + Vector3.Distance(Lower.position, Hand.position);
        }

        private void Awake()
        {
            _brain = GetComponent<AIBrain>();
            _animator = GetComponent<Animator>();
            if (!_animator.isHuman || !_weapon || !_weaponPivot || !_firePoint)
            {
                Debug.LogError("AIWeaponAim에는 Humanoid Animator, 무기 컨트롤러, 무기 회전축, 발사 위치가 필요합니다.", this);
                enabled = false;
                return;
            }
            _hips = _animator.GetBoneTransform(HumanBodyBones.Hips);
            _chest = _animator.GetBoneTransform(HumanBodyBones.Chest);
            if (!_chest)
            {
                _chest = _animator.GetBoneTransform(HumanBodyBones.Spine);
            }
            _leftArm = new()
            {
                Upper = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm),
                Lower = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm),
                Hand = _animator.GetBoneTransform(HumanBodyBones.LeftHand)
            };
            _rightArm = new()
            {
                Upper = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm),
                Lower = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm),
                Hand = _animator.GetBoneTransform(HumanBodyBones.RightHand)
            };
        }

        private void Update()
        {
            RestorePose();
            _weight = Mathf.MoveTowards(_weight, CanAim ? 1f : 0f, _blendSpeed * Time.deltaTime);
        }

        private void LateUpdate()
        {
            //애니메이션 적용이 끝난 자세를 기준으로 무기와 양팔을 보정하기
            if (_weight > 0f && !_brain.IsDead && _brain.TargetContext.HasTarget && !_weapon.IsFireLoading())
            {
                UpdateAim();
                if (CanAim)
                {
                    OnAimUpdated?.Invoke();
                }
            }
        }

        private void UpdateAim()
        {
            Transform target = _brain.TargetContext.Target;
            if (_cachedTarget != target)
            {
                _cachedTarget = target;
                _targetCollider = target.GetComponent<Collider>();
                if (!_targetCollider)
                {
                    _targetCollider = target.GetComponentInChildren<Collider>();
                }
            }
            Vector3 targetPoint = _targetCollider ? _targetCollider.bounds.center : target.position;
            Vector3 leftHand = _leftArm.Hand.position;
            Vector3 rightHand = _rightArm.Hand.position;
            Vector3 pivot = (leftHand + rightHand) * 0.5f;
            Vector3 muzzleOffset = _firePoint.position - pivot;
            Vector3 forward = _firePoint.forward;
            Vector3 up = transform.up;
            Vector3 side = Vector3.ProjectOnPlane(_rightArm.Upper.position - _leftArm.Upper.position, up).normalized;
            Vector3 bodyForward = Vector3.Cross(side, up).normalized;
            if (Vector3.Dot(bodyForward, transform.forward) < 0f)
            {
                bodyForward = -bodyForward;
            }
            float armLength = Mathf.Min(_leftArm.Length, _rightArm.Length);
            float halfWidth = Vector3.Distance(_leftArm.Upper.position, _rightArm.Upper.position) * 0.5f;
            float clearance = armLength * _bodyClearance;
            Vector3 shift = Vector3.zero;
            Quaternion correction = Quaternion.identity;

            //양손의 무기 파지 위치를 유지하면서 몸통 안쪽으로 들어간 무기를 밀어내기
            for (int i = 0; i < 8; i++)
            {
                Vector3 toTarget = targetPoint - pivot - shift;
                float along = Vector3.Dot(muzzleOffset, forward);
                float perpendicularSquared = Mathf.Max(0f, muzzleOffset.sqrMagnitude - along * along);
                float rayDistance = Mathf.Sqrt(Mathf.Max(0.0001f, toTarget.sqrMagnitude - perpendicularSquared)) - along;
                correction = Quaternion.FromToRotation(muzzleOffset + forward * rayDistance, toTarget);
                correction = Quaternion.RotateTowards(Quaternion.identity, correction, _maxAimAngle);
                Vector3 leftGoal = pivot + shift + correction * (leftHand - pivot);
                Vector3 rightGoal = pivot + shift + correction * (rightHand - pivot);
                float push = Mathf.Max(GetBodyPushDistance(leftGoal, side, bodyForward, halfWidth, clearance),
                    GetBodyPushDistance(rightGoal, side, bodyForward, halfWidth, clearance));
                float lift = Mathf.Max(0f, Vector3.Dot(_hips.position - up * (armLength * 0.15f)
                    - (Vector3.Dot(leftGoal, up) < Vector3.Dot(rightGoal, up) ? leftGoal : rightGoal), up));
                if (push + lift < 0.001f)
                {
                    break;
                }
                shift += bodyForward * push + up * lift;
            }
            correction = Quaternion.Slerp(Quaternion.identity, correction, _weight);
            shift *= _weight;
            Vector3 leftPosition = pivot + shift + correction * (leftHand - pivot);
            Vector3 rightPosition = pivot + shift + correction * (rightHand - pivot);
            Quaternion leftRotation = correction * _leftArm.Hand.rotation;
            Quaternion rightRotation = correction * _rightArm.Hand.rotation;
            _weaponLocalPosition = _weaponPivot.localPosition;
            _weaponLocalRotation = _weaponPivot.localRotation;
            _leftArm.SavePose();
            _rightArm.SavePose();
            _restoreWeapon = true;
            _weaponPivot.SetPositionAndRotation(pivot + shift + correction * (_weaponPivot.position - pivot),
                correction * _weaponPivot.rotation);
            SolveArm(_leftArm, leftPosition, leftRotation, -side, bodyForward);
            SolveArm(_rightArm, rightPosition, rightRotation, side, bodyForward);
        }

        private float GetBodyPushDistance(Vector3 hand, Vector3 side, Vector3 forward, float halfWidth, float clearance)
        {
            float height = Vector3.Dot(hand - _hips.position, transform.up);
            if (height < -clearance || Vector3.Dot(hand - _chest.position, transform.up) > clearance)
            {
                return 0f;
            }
            Vector3 center = Vector3.Lerp(_hips.position, _chest.position,
                Mathf.Clamp01(height / Mathf.Max(0.01f, Vector3.Dot(_chest.position - _hips.position, transform.up))));
            float lateral = Vector3.Dot(hand - center, side) / (halfWidth + clearance);
            if (Mathf.Abs(lateral) >= 1f)
            {
                return 0f;
            }
            float front = (halfWidth * 0.65f + clearance) * Mathf.Sqrt(1f - lateral * lateral);
            return Mathf.Max(0f, front - Vector3.Dot(hand - center, forward));
        }

        private void SolveArm(Arm arm, Vector3 goal, Quaternion rotation, Vector3 outside, Vector3 forward)
        {
            Vector3 shoulder = arm.Upper.position;
            float upperLength = Vector3.Distance(shoulder, arm.Lower.position);
            float lowerLength = Vector3.Distance(arm.Lower.position, arm.Hand.position);
            Vector3 direction = goal - shoulder;
            float distance = Mathf.Clamp(direction.magnitude, Mathf.Abs(upperLength - lowerLength) + 0.001f,
                upperLength + lowerLength - 0.001f);
            direction.Normalize();
            //팔꿈치가 몸통 바깥쪽으로 굽혀지도록 방향을 보정하기
            Vector3 pole = arm.Lower.position - shoulder + outside * (upperLength + lowerLength) * (0.6f * _weight)
                + forward * upperLength * (0.2f * _weight);
            pole = Vector3.ProjectOnPlane(pole, direction).normalized;
            if (pole.sqrMagnitude < 0.001f)
            {
                pole = Vector3.ProjectOnPlane(outside, direction).normalized;
            }
            float along = (upperLength * upperLength - lowerLength * lowerLength + distance * distance) / (2f * distance);
            float bend = Mathf.Sqrt(Mathf.Max(0f, upperLength * upperLength - along * along));
            Vector3 elbow = shoulder + direction * along + pole * bend;
            Vector3 hand = shoulder + direction * distance;
            arm.Upper.rotation = Quaternion.FromToRotation(arm.Lower.position - shoulder, elbow - shoulder) * arm.Upper.rotation;
            arm.Lower.rotation = Quaternion.FromToRotation(arm.Hand.position - arm.Lower.position, hand - arm.Lower.position) * arm.Lower.rotation;
            arm.Hand.rotation = rotation;
        }

        private void RestorePose()
        {
            if (!_restoreWeapon || !_weaponPivot)
            {
                return;
            }
            _weaponPivot.SetLocalPositionAndRotation(_weaponLocalPosition, _weaponLocalRotation);
            _leftArm.RestorePose();
            _rightArm.RestorePose();
            _restoreWeapon = false;
        }

        private void OnDisable()
        {
            RestorePose();
            _weight = 0f;
        }
    }
}
