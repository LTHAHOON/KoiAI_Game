using UnityEngine;

namespace KoiAI.Aim
{
    public class CannonAim : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer _line;
        private int _curPositionCount = 0;
        private int _maxPositionCount = 0;

        private void OnDisable()
        {
            _curPositionCount = 0;
        }

        public void InitLine(int lineCount)
        {
            _line.positionCount = lineCount;
            _maxPositionCount = lineCount;
        }

        public void SetLinePosition(int index, Vector3 position)
        {
            if (index < _maxPositionCount)
            {
                ++_curPositionCount;
                _line.SetPosition(index, position);
            }
        }
        public void ClearEmptyLine()
        {
            _line.positionCount = _curPositionCount;
            _curPositionCount = 0;
        }
    }
}
