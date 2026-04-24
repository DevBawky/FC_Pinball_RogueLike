using UnityEngine;

[CreateAssetMenu(fileName = "Self Destruct Chance Ability", menuName = "PinBall/Ball Special Ability/Self Destruct Chance")]
public class SelfDestructChanceOnHitAbility : BallSpecialAbilityBase
{
    [Header("Self Destruct Settings")]
    [Range(0f, 1f)]
    public float destroyChance = 0.25f;

    public override void Activate(BallSpecialAbilityTriggerContext context)
    {
        if (context.ballController == null)
        {
            return;
        }

        if (Random.value > destroyChance)
        {
            return;
        }

        Destroy(context.ballController.gameObject);
    }
}
