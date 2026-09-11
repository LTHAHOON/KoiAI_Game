using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

namespace KoiAI.Nav
{
    public enum AgentPhysicsType
    {
        AgentPhysicsUpdate,
        RigidPhysicsUpdate
    }

    [CreateAssetMenu(fileName = "new NavigationData", menuName = "KoiAI/Navigation/NavigationData")]
    public class NavigationData : ScriptableObject
    {
        [SerializeField]
        private int _agentTypeIndex;
        [SerializeField]
        private AgentPhysicsType _agentPhysicsType;
        [ShowIf(nameof(_agentPhysicsType), AgentPhysicsType.AgentPhysicsUpdate)]
        [SerializeField]
        private float _moveSpeed;
        [ShowIf(nameof(_agentPhysicsType), AgentPhysicsType.AgentPhysicsUpdate)]
        [SerializeField]
        private float _angularSpeed;
        [ShowIf(nameof(_agentPhysicsType), AgentPhysicsType.AgentPhysicsUpdate)]
        [SerializeField]
        private float _acceleration;
        [SerializeField]
        private float _stoppingDistance;
        [SerializeField]
        private int _avoidancePriority = 50;
        
        public int GetAgentTypeID()
        {
            NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(_agentTypeIndex);
            return settings.agentTypeID;
        }
        public AgentPhysicsType AgentPhyscisType => _agentPhysicsType;
        public float MoveSpeed => _moveSpeed;
        public float AngularSpeed => _angularSpeed;
        public float Acceleration => _acceleration;
        public float StoppingDistance => _stoppingDistance;
        public int AvoidancePriority => _avoidancePriority;   
    }
}
