using UnityEngine;

public abstract class BallSpecialAbilityBase : ScriptableObject
{
    [TextArea]
    public string designerNote;

    public abstract void Activate(BallSpecialAbilityTriggerContext context);
}
