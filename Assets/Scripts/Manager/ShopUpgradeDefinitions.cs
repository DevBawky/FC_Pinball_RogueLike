using System;
using UnityEngine;

public enum ShopUpgradeType
{
    BallMaxHealthRatio,
    WallCollisionDamageReduction,
    ScoreGain
}

[Serializable]
public struct ScoreUpgradeValue
{
    public int chipsBonus;
    public float multiplierBonus;

    public ScoreUpgradeValue(int chipsBonus, float multiplierBonus)
    {
        this.chipsBonus = chipsBonus;
        this.multiplierBonus = multiplierBonus;
    }
}
