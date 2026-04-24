using UnityEngine;

public enum BallSpecialAbilityCollisionType
{
    Object,
    Wall
}

public struct BallSpecialAbilityTriggerContext
{
    public BallController ballController;
    public BallData ballData;
    public BallSpecialAbilityCollisionType collisionType;
    public Vector3 hitPosition;
    public GameObject hitObject;
    public int triggerCount;
    public int remainingTriggerCount;

    public BallSpecialAbilityTriggerContext(
        BallController ballController,
        BallData ballData,
        BallSpecialAbilityCollisionType collisionType,
        Vector3 hitPosition,
        GameObject hitObject,
        int triggerCount,
        int remainingTriggerCount)
    {
        this.ballController = ballController;
        this.ballData = ballData;
        this.collisionType = collisionType;
        this.hitPosition = hitPosition;
        this.hitObject = hitObject;
        this.triggerCount = triggerCount;
        this.remainingTriggerCount = remainingTriggerCount;
    }
}
