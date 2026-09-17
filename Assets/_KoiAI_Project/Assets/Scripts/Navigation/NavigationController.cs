using Cysharp.Threading.Tasks;
using R3;
using System;
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

        private bool IsAgentReady => _navMeshAgent && _navMeshAgent.isActiveAndEnabled && _navMeshAgent.isOnNavMesh;

        public float SurroundRadius => _navMeshAgent.radius;

        public float SurroundHeight => _navMeshAgent.height;

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

        private void SetUpRigidMoveSubscription()
        {
            _navMeshAgent.updatePosition = false;
            _navMeshAgent.updateRotation = false;
            TryGetComponent(out _rigidBody);

            Observable.Interval(TimeSpan.Zero, UnityTimeProvider.FixedUpdate)
                            .Subscribe(_ =>
                            {
                                if (!IsAgentReady)
                                {
                                    if (_rigidBody)
                                    {
                                        _rigidBody.linearVelocity = new Vector3(
                                            0f,
                                            _rigidBody.linearVelocity.y,
                                            0f);
                                    }
                                    return;
                                }

                                //Path계산이 끝날 경우
                                if (!_navMeshAgent.pathPending && _navMeshAgent.hasPath)
                                {
                                    Vector3 moveDir = _navMeshAgent.desiredVelocity.normalized;

                                    Vector3 targetVelocity = moveDir * _curMoveSpeed;
                                    targetVelocity.y = _rigidBody.linearVelocity.y;

                                    _rigidBody.linearVelocity = targetVelocity;
                                }

                                _navMeshAgent.nextPosition = _rigidBody.position;

                                if (IsAgentArrived())
                                {
                                    _rigidBody.linearVelocity = new Vector3(0, _rigidBody.linearVelocity.y, 0);
                                }
                            }).AddTo(this);
        }

        public void MoveToDest(Vector3 destination, float moveSpeed)
        {
            if (!IsAgentReady)
            {
                return;
            }

    
            if(!TryGetNavMeshPath(out _, destination))
            {
                if (NavMesh.SamplePosition(destination, out NavMeshHit navMeshHit, 10, NavMesh.AllAreas))
                {
                    if (!TryGetNavMeshPath(out _, destination))
                    {
                        _navMeshAgent.ResetPath();
                        return;
                    }
                }
                else
                {
                    _navMeshAgent.ResetPath();
                    return;
                }
                destination = navMeshHit.position;
            }

            _navMeshAgent.SetDestination(destination);

            _lastDestination = destination;
            _curMoveSpeed = moveSpeed;
        }

        public void StopMovement()
        {
            _curMoveSpeed = 0f;

            if (IsAgentReady)
            {
                _navMeshAgent.ResetPath();
            }

            if (_rigidBody)
            {
                _rigidBody.linearVelocity = new Vector3(0f, _rigidBody.linearVelocity.y, 0f);
            }
        }

        public bool IsAgentArrived()
        {
            if (_navMeshAgent.pathPending)
            {
                return false;
            }

            if(_navMeshAgent.hasPath && _navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance)
            {
                return false;
            }

            return true;
        }

        public bool TryGetPathDistance(Vector3 destination, out float distance)
        {
            distance = 0f;

            if (!IsAgentReady)
            {
                return false;
            }

            if (TryGetNavMeshPath(out NavMeshPath path, destination) || path == null)
            {
                return false;
            }

            Vector3[] corners = path.corners;

            for (int i = 1; i < corners.Length; i++)
            {
                distance += Vector3.Distance(corners[i - 1], corners[i]);
            }

            return true;
        }

        private bool TryGetNavMeshPath(out NavMeshPath path, Vector3 destination)
        {
            path = default;
            if (_navMeshPath == null || !IsAgentReady)
            {
                return false;
            }

            if (_navMeshAgent.CalculatePath(destination, _navMeshPath))
            {
                if (_navMeshPath.status == NavMeshPathStatus.PathInvalid || _navMeshPath.status == NavMeshPathStatus.PathPartial)
                {
                    return false;
                }
                path = _navMeshPath;
                return true;
            }
            return false;
        }
    }
}