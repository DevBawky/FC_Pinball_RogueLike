using UnityEngine;

public enum ScoreType { Chips, Multiplier }
public enum BallSpecialAbilityTriggerTarget { Object, Wall, Any }

[System.Serializable]
public class BallSpecialAbilityDefinition
{
    public bool useSpecialAbility = false;
    public string abilityName = "New Special Ability";

    [TextArea]
    public string abilityDescription = "Describe what this special ability does.";

    public BallSpecialAbilityTriggerTarget triggerTarget = BallSpecialAbilityTriggerTarget.Any;

    [Min(0)]
    public int maxTriggerCount = 0;
    public BallSpecialAbilityBase abilityAsset;

    public bool HasLimitedTriggerCount => maxTriggerCount > 0;
}

[CreateAssetMenu(fileName = "New Ball", menuName = "PinBall/Ball Data")]
public class BallData : ScriptableObject
{

    [Header("기본 정보")]
    public string ballName = "기본 공";
    [TextArea]
    public string description = "가장 기본적인 형태의 공입니다.";
    public Sprite ballSprite;

    [Header("이동 스탯")]
    public float baseSpeed = 15f;     

    [Header("전투 스탯")]
    public int maxHealth = 15;        
    
    [Header("점수 부여 스탯 (Balatro Style)")]
    public ScoreType scoreType = ScoreType.Chips; // 합(Chips)인지 곱(Multiplier)인지
    public float scoreValue = 1f; // 칩이면 +10, 배수면 +0.5 등 부여할 수치

    [Header("특수 속성 (빌딩용)")]
    [Header("Shop")]
    [Min(0)]
    public int price = 3;

    [Header("Special Ability Trigger")]
    public BallSpecialAbilityDefinition specialAbility = new BallSpecialAbilityDefinition();

    public bool HasSpecialAbility =>
        specialAbility != null
        && specialAbility.useSpecialAbility
        && specialAbility.abilityAsset != null;

    public bool CanTriggerSpecialAbility(BallSpecialAbilityCollisionType collisionType, int currentTriggerCount)
    {
        if (!HasSpecialAbility)
        {
            return false;
        }

        if (!MatchesSpecialAbilityTrigger(collisionType))
        {
            return false;
        }

        if (!specialAbility.HasLimitedTriggerCount)
        {
            return true;
        }

        return currentTriggerCount < specialAbility.maxTriggerCount;
    }

    public bool MatchesSpecialAbilityTrigger(BallSpecialAbilityCollisionType collisionType)
    {
        if (!HasSpecialAbility)
        {
            return false;
        }

        return specialAbility.triggerTarget == BallSpecialAbilityTriggerTarget.Any
            || (specialAbility.triggerTarget == BallSpecialAbilityTriggerTarget.Object && collisionType == BallSpecialAbilityCollisionType.Object)
            || (specialAbility.triggerTarget == BallSpecialAbilityTriggerTarget.Wall && collisionType == BallSpecialAbilityCollisionType.Wall);
    }

    private void OnValidate()
    {
        if (specialAbility == null)
        {
            specialAbility = new BallSpecialAbilityDefinition();
        }
    }
}
