using System;
using System.Collections.Generic;

[Serializable]
public struct InfluenceGroup
{
    public List<Condition> Conditions;
    public float InfluencePerTick;
}
