using R3;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace KoiAI.Nav
{

    public class NavigationController : MonoBehaviour
    {
        [SerializeField] 
        private NavMeshAgent _navMeshAgent;
        [SerializeField] 
        private NavigationData _navigationData;

        private float _maxMoveSpeed = 0f;
        private NavMeshPath _navMeshPath;
        private Rigidbody _rigidBody;
        private bool _hasDestination;

        private bool IsAgentReady => _navMeshAgent && _navMeshAgent.isActiveAndEnabled && _navMeshAgent.isOnNavMesh;


        private void Awake()
        {
            Initialize();
        }

        private void FixedUpdate()
        {
            if (!IsAgentReady || !_hasDestination)
            {
                return;
            }

            Vector3 targetVelocity = Vector3.ClampMagnitude(_navMeshAgent.velocity, _maxMoveSpeed);
            targetVelocity.y = _rigidBody.linearVelocity.y;
            _rigidBody.linearVelocity = targetVelocity;
            _navMeshAgent.nextPosition = _rigidBody.position;
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
                    _navMeshAgent.updatePosition = false;
                    _navMeshAgent.updateRotation = false;
                    TryGetComponent(out _rigidBody);
                    break;
            }
        }

        public void MoveToDest(Vector3 destination, float maxMoveSpeed)
        {
            if (!IsAgentReady)
            {
                return;
            }

            if (!TryGetNavMeshPath(out NavMeshPath path, destination))
            {
                if (!NavMesh.SamplePosition(destination, out NavMeshHit navMeshHit, 10, _navMeshAgent.areaMask)
                    || !TryGetNavMeshPath(out path, navMeshHit.position))
                {
                    StopMovement_Force();
                    return;
                }
            }
            
            if (!_navMeshAgent.SetPath(path))
            {
                StopMovement_Force();
                return;
            }

            _maxMoveSpeed = maxMoveSpeed;
            _navMeshAgent.speed = maxMoveSpeed;
            _hasDestination = true;
        }

        //강제로 멈추기
        public void StopMovement_Force()
        {
            if (IsAgentReady)
            {
                _navMeshAgent.ResetPath();
            }

            if (_rigidBody)
            {
                _rigidBody.linearVelocity = new Vector3(0f, _rigidBody.linearVelocity.y, 0f);
            }

            _maxMoveSpeed = 0f;
            _hasDestination = false;
        }

        public bool IsAgentArrived()
        {
            if (!IsAgentReady)
            {
                return true;
            }

            if (_navMeshAgent.pathPending)
            {
                return false;
            }

            if (_navMeshAgent.hasPath && _navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance)
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

            if (!TryGetNavMeshPath(out NavMeshPath path, destination))
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

        public float CurrentMoveSpeed => _navMeshAgent.velocity.magnitude;
        public Rigidbody Rigidbody=> _rigidBody;
    }
}
