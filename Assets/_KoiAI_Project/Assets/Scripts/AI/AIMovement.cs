using System;
using UnityEngine;

namespace KoiAI.AI
{
    using Cysharp.Threading.Tasks;
    using KoiAI.A_Star;
    using KoiAI.AnimatorSystem;
    using KoiAI.Audio;
    using KoiAI.CustomPhysics;
    using KoiAI.SurroundPos;

    [Serializable]
    public class AIMovementExtensionData : AIFeatureExtensionData
    {
        #region 보정값 및 추가 이동 데이터
        [SerializeField]
        private SurroundPosContext _surroundPosContext;
        [SerializeField]
        private AudioData _stepAuidoData;
        [SerializeField]
        private AudioData _stopStepAudioData;
        [SerializeField]
        private AudioData _jumpAudioData;
        [SerializeField]
        private float _moveSpeedMod = 10f;
        [SerializeField]
        private float _jumpForceMod = 10f;
        [SerializeField]
        private int _jumpMaxCountMod = 1;
        [SerializeField]
        private float _stepAudioThresold;
        [SerializeField]
        private float _sizeForMoveStopMod;

        #endregion

        #region 물리 데이터
        [SerializeField]
        private RigidbodyData _rigidData;
        [SerializeField]
        private CapsuleColliderData _colliderData;
        #endregion
        
        public RigidbodyData RigidData => _rigidData;
        public CapsuleColliderData ColliderData => _colliderData;
        public AudioData StepAuidoData => _stepAuidoData;
        public AudioData StopStepAudioData => _stopStepAudioData;
        public AudioData JumpAudioData => _jumpAudioData;
        public float MoveSpeedMod => _moveSpeedMod;
        public float JumpForceMod => _jumpForceMod;
        public int JumpMaxCountMod => _jumpMaxCountMod;
        public float StepAudioThresold => _stepAudioThresold;
        public float SizeForMoveStopMod => _sizeForMoveStopMod;
        public SurroundPosContext SurroundPosContext => _surroundPosContext;
    }

    [Serializable]
    public class AIMovementValueData : AIFeatureValueData
    {
        #region 이동 데이터

        [SerializeField]
        private float _moveSpeed = 10f;
        [SerializeField]
        private float _jumpForce = 10f;
        [SerializeField]
        private int _jumpMaxCount = 1;
        [SerializeField]
        private float _sizeForMoveStop = 3f;

        [SerializeField]
        private WayPointData _moveWapointData;

        #endregion

        public float MoveSpeed => _moveSpeed;
        public float JumpForce => _jumpForce;
        public int JumpMaxCount => _jumpMaxCount;
        public float SizeForMoveStop => _sizeForMoveStop;
        public WayPointData MoveWayPointData => _moveWapointData;
    }

    public class AIMovement : AIFeature
    {
        private AIMovementValueData _valueData;
        private AIMovementExtensionData _extensionData;
        private AnimatorParamData _animParamData;
        private GameObject _target;
        private bool _bHasTarget = false;
        private float _ratioStopDistance = 0f;
        public override void InitFeature(AIFeatureValueData enemyFeatureValueData = null,
            AIFeatureExtensionData enemyFeatureExtensionData = null)
        {
            if (enemyFeatureValueData is not AIMovementValueData valueData
                || enemyFeatureExtensionData is not AIMovementExtensionData extensionData)
            {
                return;
            }
            _valueData = valueData;
            _extensionData = extensionData;
            if (Brain.AIAnimatorData.IsValid())
            {
                //애니메이터 파라미터 데이터 초기화
                _animParamData = Brain.AIAnimatorData.AnimParamData;
            }
            else
            {
                Debug.Log("Check: EnemyAnimatorData is not valid.");
            }
        }

        public override void EnterFeature()
        {
            _bHasTarget = TryGetTarget(out _target);
            _ratioStopDistance = UnityEngine.Random.Range(-1f, 1f);
            if (!_bHasTarget)
                return;
        }

        public override void ExitFeature()
        {
            SurroundPosManager.Instance.ReleaseSurroundPos(Brain.gameObject, _target);
            Brain.AgentController.StopMovement();
            if(Brain.AIAnimator)
            {
                Brain.AIAnimator.SetBool(_animParamData.WalkParmID, false);
            }
            _bHasTarget = false;
        }

        public override async void UpdateFeature()
        {
            if (!Brain.TargetContext.HasTarget && !_bHasTarget)
            {
                return;
            }

            float stopDistance = _valueData.SizeForMoveStop + _extensionData.SizeForMoveStopMod;
            if(SurroundPosManager.Instance.TryGetSurroundPos(_extensionData.SurroundPosContext,Brain.gameObject , 
                                                            _target, out SurroundPosSlot surroundPosSlot))
            {
                
                Vector3 moveDir = -(_target.transform.position - Brain.transform.position).normalized;
                //약간의 랜덤 위치 분포
                Vector3 targetPos = surroundPosSlot.Position + moveDir * stopDistance *_ratioStopDistance;
                Brain.AgentController.MoveToDest(targetPos, _valueData.MoveSpeed + _extensionData.MoveSpeedMod);
                await UniTask.WaitForEndOfFrame();
                
            }
            bool isMoving = !Brain.AgentController.IsAgentArrived();

            if (Brain.AIAnimator)
            {
                Brain.AIAnimator.SetBool(_animParamData.WalkParmID, isMoving);
            }

        }
    }
}
