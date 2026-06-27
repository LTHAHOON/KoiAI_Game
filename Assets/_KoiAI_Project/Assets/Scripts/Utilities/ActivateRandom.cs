using UnityEngine;

namespace KoiAI.Utilities
{
    public static class ActivateRandom
    {
        public static T GetRandomActivateTarget<T>(ActivateRandomValue<T>[] activateValues) where T : Component
        {
            if (activateValues == null)
            {
                return null;
            }
            ActivateRandomValue<T> curActivateValue = activateValues[0];

            for (int i = 1; i < activateValues.Length; i++)
            {
                if (curActivateValue == null || activateValues[i] == null)
                {
                    return null;
                }

                float curRandomValue = curActivateValue.GetRandomValue();
                float randomValue = activateValues[i].GetRandomValue();
                if (curRandomValue < randomValue)
                {
                    curActivateValue = activateValues[i];
                }
            }
            return curActivateValue.ActivateTarget;
        }
    }
}
