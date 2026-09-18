using UnityEngine;

namespace KoiAI.AI
{
    public class AITargetContext
    {
        public Transform Target { get; private set; }
        public float Distance { get; private set; }
        public bool HasTarget => Target != null;

        public void SetTarget(Transform owner, Transform target)
        {
            Target = target;
            Vector3 ownerPos = owner.position;
            ownerPos.y = 0f;
            Vector3 targetPos = target.position;
            targetPos.y = 0f;
            Distance = target ? Vector3.Distance(ownerPos, targetPos) : float.MaxValue;
        }

        public void Clear()
        {
            Target = null;
            Distance = float.MaxValue;
        }
    }
}
