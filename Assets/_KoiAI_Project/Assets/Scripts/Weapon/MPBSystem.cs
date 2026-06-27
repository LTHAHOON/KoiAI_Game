using UnityEngine;

namespace KoiAI.Weapon
{
    public class MPBSystem
    {
        private static MaterialPropertyBlock _mpb; 
        public static void ChangeMaterialProperty<T>(Renderer renderer, int propertyHash, T value)
        {
            if (_mpb == null)
            {
                _mpb = new MaterialPropertyBlock();
            }
            renderer.GetPropertyBlock(_mpb);
            switch (value)
            {
                case float fValue:
                {
                    _mpb.SetFloat(propertyHash, fValue);
                    break;
                }
                case Color cValue:
                {
                    _mpb.SetColor(propertyHash, cValue);
                    break;
                }
                case Vector4 v4Value:
                {
                    _mpb.SetVector(propertyHash, v4Value);
                    break;
                }
                case Texture2D tValue:
                {
                    _mpb.SetTexture(propertyHash, tValue);
                    break;
                }
            }
            renderer.SetPropertyBlock(_mpb);
            _mpb.Clear();
        }
    }
}
