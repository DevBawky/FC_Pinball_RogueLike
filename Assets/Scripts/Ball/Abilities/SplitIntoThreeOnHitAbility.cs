using UnityEngine;

[CreateAssetMenu(fileName = "Split Into Three Ability", menuName = "PinBall/Ball Special Ability/Split Into Three")]
public class SplitIntoThreeOnHitAbility : BallSpecialAbilityBase
{
    [Header("Split Settings")]
    [Min(2)]
    public int totalBallCount = 3;
    [Range(5f, 180f)]
    public float spreadAngle = 40f;
    [Min(0f)]
    public float spawnOffset = 0.35f;

    public override void Activate(BallSpecialAbilityTriggerContext context)
    {
        if (context.ballController == null || totalBallCount <= 1)
        {
            return;
        }

        Vector2 baseDirection = context.ballController.GetCurrentDirection();
        if (baseDirection == Vector2.zero)
        {
            baseDirection = Vector2.up;
        }

        context.ballController.SetTravelDirection(baseDirection);

        int additionalBallCount = totalBallCount - 1;
        for (int i = 0; i < additionalBallCount; i++)
        {
            float t = additionalBallCount == 1 ? 0.5f : i / (float)(additionalBallCount - 1);
            float angle = Mathf.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, t);
            Vector2 splitDirection = Rotate(baseDirection, angle);

            context.ballController.SpawnDuplicateBall(splitDirection, spawnOffset);
        }
    }

    private Vector2 Rotate(Vector2 direction, float angle)
    {
        return Quaternion.Euler(0f, 0f, angle) * direction.normalized;
    }
}
