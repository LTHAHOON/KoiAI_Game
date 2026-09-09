using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace KoiAI.Nav
{
    public class NavigationController : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private NavigationData _navigationData;

        private float _curMoveSpeed = 0f;
        private NavMeshPath _navMeshPath;
        private Rigidbody _rigidBody;
        private Vector3 _lastDestination;
        private bool _hasDestination;


        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            _navMeshPath = new();
            _navMeshAgent.agentTypeID = _navigationData.GetAgentTypeID();
            _navMeshAgent.avoidancePriority = _navigationData.AvoidancePriority;
            _navMeshAgent.speed = _navigationData.MoveSpeed;
            _navMeshAgent.angularSpeed = _navigationData.AngularSpeed;
            _navMeshAgent.acceleration = _navigationData.Acceleration;
            _navMeshAgent.stoppingDistance = _navigationData.StoppingDistance;

            switch (_navigationData.AgentPhyscisType)
            {
                case AgentPhysicsType.AgentPhysicsUpdate:
                    _navMeshAgent.updatePosition = true;
                    _navMeshAgent.updateRotation = true;
                    break;

                case AgentPhysicsType.RigidPhysicsUpdate:
                    SetUpRigidMoveSubscription();
                    break;
            }
        }

        public void SetUpRigidMoveSubscription()
        {
            _navMeshAgent.updatePosition = false;
            _navMeshAgent.updateRotation = false;
            TryGetComponent(out _rigidBody);
            if (_rigidBody)
            {
                _rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            }
            Observable.Interval(TimeSpan.Zero, UnityTimeProvider.FixedUpdate)
                            .Subscribe(_ =>
                            {
                                //Path계산이 끝날 경우
                                if (!_navMeshAgent.pathPending && _navMeshAgent.hasPath)
                                {
                                    Vector3 desiredVelocity = _navMeshAgent.desiredVelocity;

                                    Vector3 targetVelocity = desiredVelocity * _curMoveSpeed;
                                    targetVelocity.y = _rigidBody.linearVelocity.y;

                                    _rigidBody.linearVelocity = targetVelocity;
                                }

                                _navMeshAgent.nextPosition = _rigidBody.position;

                                if (IsMoveStop())
                                {
                                    _rigidBody.linearVelocity = new Vector3(0, _rigidBody.linearVelocity.y, 0);
                                }
                            }).AddTo(this);
        }

        public void MoveToDest(Vector3 destination, float moveSpeed)
        {
            if (_hasDestination && (_lastDestination - destination).sqrMagnitude <= 0.01f)
            {
                _curMoveSpeed = moveSpeed;
                return;
            }

            if (!CanMoveToDestination(out _, destination))
            {
                return;
            }

            _navMeshAgent.SetDestination(destination);
            _lastDestination = destination;
            _hasDestination = true;

            switch (_navigationData.AgentPhyscisType)
            {
                case AgentPhysicsType.RigidPhysicsUpdate:
                    if (_rigidBody)
                    {
                        _curMoveSpeed = moveSpeed;
                    }
                    break;
            }
        }

        public void ResetPath()
        {
            _navMeshAgent.ResetPath();
            _hasDestination = false;
        }

        public void StopMovement()
        {
            _navMeshAgent.ResetPath();
            _curMoveSpeed = 0f;

            if (_rigidBody)
            {
                _rigidBody.linearVelocity = new Vector3(0f, _rigidBody.linearVelocity.y, 0f);
            }
        }

        public bool IsMoveStop()
        {
            if (_navMeshAgent.pathPending)
            {
                return false;
            }

            return !_navMeshAgent.hasPath || _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance;
        }

        public bool CanMoveToDestination(out Vector3[] path, Vector3 destination)
        {
            path = default;
            if (_navMeshPath == null) return false;

            if (_navMeshAgent.CalculatePath(destination, _navMeshPath))
            {
                if (_navMeshPath.status == NavMeshPathStatus.PathInvalid)
                {
                    return false;
                }
                path = _navMeshPath.corners;
                return true;
            }
            return false;
        }
    }
}