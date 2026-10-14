using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace KoiAI.Health
{
    public class PixelBurnDeath : Death
    {
        [SerializeField]
        private VisualEffect[] _pixelBurnVFX;
        

        public override void Initialize() { }

        public override void OnDeath(Health health)
        {
            for(int i = 0; i < _pixelBurnVFX.Length; ++i)
            {
                _pixelBurnVFX[i].Reinit();
                _pixelBurnVFX[i].Play();
            }
        }
    }
}
