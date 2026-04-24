using UnityEngine;

[CreateAssetMenu(fileName = "Random Score Bonus Ability", menuName = "PinBall/Ball Special Ability/Random Score Bonus")]
public class RandomScoreBonusOnHitAbility : BallSpecialAbilityBase
{
    [Header("Random Bonus Settings")]
    [Min(0f)]
    public float chipsBonus = 10f;
    [Min(0f)]
    public float multiplierBonus = 0.5f;

    public override void Activate(BallSpecialAbilityTriggerContext context)
    {
        if (MainGameUIManager.Instance == null)
        {
            return;
        }

        bool grantChipsBonus = Random.value < 0.5f;
        ScoreType bonusType = grantChipsBonus ? ScoreType.Chips : ScoreType.Multiplier;
        float bonusValue = grantChipsBonus ? chipsBonus : multiplierBonus;

        if (bonusValue <= 0f)
        {
            return;
        }

        MainGameUIManager.Instance.SpawnFlyingScore(context.hitPosition, bonusType, bonusValue);
    }
}
