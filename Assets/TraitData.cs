using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TraitData : AConditionData
{
    public Sprite Icon;
    public string Name;
    public List<InfluenceGroup> Influences;
    public float InfluenceLossPerTick;
}
