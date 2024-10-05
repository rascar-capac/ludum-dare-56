using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TraitsManager : Singleton<TraitsManager>
{
    public SerializableDictionary<TraitData, float> CurrentTraits;

    [ContextMenu("Skip 1 tick")]
    public void RefreshTraits()
    {
        RefreshTraits(1);
    }

    public void RefreshTraits(int tickCount)
    {
        List<TraitData> traitTypes = CurrentTraits.Keys.ToList();

        foreach(TraitData type in traitTypes)
        {
            RefreshTrait(type, tickCount);
        }
    }

    public void RefreshTrait(TraitData type, int tickCount)
    {
        float totalInfluenceRatio = 0f;
        bool anyInfluenceGroupIsFulfilled = false;

        foreach(InfluenceGroup influenceGroup in type.Influences)
        {
            float influenceGroupRatio = ComputeInfluenceGroupRatio01(influenceGroup);

            if(influenceGroupRatio > 0)
            {
                anyInfluenceGroupIsFulfilled = true;
            }

            totalInfluenceRatio += tickCount * influenceGroup.InfluencePerTick * influenceGroupRatio;
        }

        if(!anyInfluenceGroupIsFulfilled)
        {
            totalInfluenceRatio = tickCount * -type.InfluenceLossPerTick;
        }

        CurrentTraits[type] = Mathf.Clamp01(CurrentTraits[type] + totalInfluenceRatio);
    }

    public float ComputeInfluenceGroupRatio01(InfluenceGroup group)
    {
        float totalInfluenceRatio = 1f;

        foreach(Condition condition in group.Conditions)
        {
            float influenceRatio = condition.GetInfluenceRatio01();

            totalInfluenceRatio *= influenceRatio;
        }

        return totalInfluenceRatio;
    }
}
