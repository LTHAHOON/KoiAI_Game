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
        private float _ditherDuration = 0.6f;
        [SerializeField]
        private VisualEffect[] _pixelBurnVFX;
        
        private Material[] _materials;
        private static readonly int DitherAlphaID = Shader.PropertyToID("_Dither_Alpha");
        public override void Initialize()
        {
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            if(meshRenderers == null || meshRenderers.Length <= 0)
            {
                return;
            }
            _materials = meshRenderers.Select(meshRenderer => meshRenderer.material).ToArray();
        }

        public override void OnDeath(Health health)
        {
            if(_materials == null)
            {
                return;
            }

            for(int i = 0; i < _pixelBurnVFX.Length; ++i)
            {
                _pixelBurnVFX[i].Reinit();
                _pixelBurnVFX[i].Play();
            }

            for(int i = 0; i < _materials.Length; ++i)
            {
                _materials[i].DOFloat(0f, DitherAlphaID, _ditherDuration);
            }
        }
    }
}
