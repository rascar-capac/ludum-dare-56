using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TraitsManager : Singleton<TraitsManager>
{
    public SerializableDictionary<TraitData, TraitInfo> Traits;
    public float UnknownTraitThreshold;
    public float GreatTraitThreshold;

    public UnityEvent<TraitData, ETraitStatus> OnTraitStatusChanged { get; } = new();

    [ContextMenu("Skip 1 tick")]
    public void RefreshTraits()
    {
        RefreshTraits(1);
    }

    public void RefreshTraits(int tickCount)
    {
        List<TraitData> traitTypes = Traits.Keys.ToList();

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

        TraitInfo traitInfo = Traits[type];
        traitInfo.Value = Mathf.Clamp01(traitInfo.Value + totalInfluenceRatio);
        Traits[type] = traitInfo;

        RefreshTraitStatus(type);
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

    public void RefreshTraitStatus(TraitData type)
    {
        TraitInfo traitInfo = Traits[type];

        if(traitInfo.Value == 0)
        {
            traitInfo.Status = ETraitStatus.NotPossessed;
        }
        else if(traitInfo.Value < UnknownTraitThreshold)
        {
            if(!traitInfo.HasBeenDiscovered)
            {
                traitInfo.Status = ETraitStatus.Unknown;
            }
            else
            {
                traitInfo.Status = ETraitStatus.Discovered;
            }
        }
        else if(traitInfo.Value < GreatTraitThreshold)
        {
            traitInfo.Status = ETraitStatus.Great;
        }

        Traits[type] = traitInfo;
        OnTraitStatusChanged.Invoke(type, traitInfo.Status);
    }
}

[Serializable]
public struct TraitInfo
{
    public float Value;
    public bool HasBeenDiscovered;
    public ETraitStatus Status;
}

public enum ETraitStatus
{
    NotPossessed = 0,
    Unknown = 1,
    Discovered = 2,
    Great = 3,
}
