using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "PinBall/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "New Enemy";
    public float maxHealth = 5000f;
    public Sprite enemySprite;
}
