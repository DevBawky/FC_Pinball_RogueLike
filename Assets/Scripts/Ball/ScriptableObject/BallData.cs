using UnityEngine;

public enum ScoreType { Chips, Multiplier }

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
    public bool isPiercing = false;   
    public bool isHeavy = false;      
    public bool isExplosive = false;  
}