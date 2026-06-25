using UnityEngine;

public abstract class FadeCondition : ScriptableObject
{
    public abstract void Init();
    public abstract bool IsPossibleFadeIn();
    public abstract bool IsPossibleFadeOut();

}
