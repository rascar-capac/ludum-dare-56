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
    public IReadOnlyDictionary<TraitData, TraitInfo> SavedTraits;

    public UnityEvent<TraitData, ETraitStatus> OnTraitStatusChanged { get; } = new();

    [ContextMenu("Skip 1 tick")]
    public void RefreshTraits()
    {
        RefreshTraits(1, ParametersManager.Instance.Parameters);
    }

    public void RefreshTraits(int tickCount, IReadOnlyDictionary<ParameterData, float> parameters)
    {
        List<TraitData> traitTypes = Traits.Keys.ToList();

        foreach(TraitData type in traitTypes)
        {
            RefreshTrait(type, tickCount, parameters);
        }
    }

    public void RefreshTrait(TraitData type, int tickCount, IReadOnlyDictionary<ParameterData, float> parameters)
    {
        float totalInfluenceRatio = 0f;
        bool anyInfluenceGroupIsFulfilled = false;

        foreach(InfluenceGroup influenceGroup in type.Influences)
        {
            float influenceGroupRatio = ComputeInfluenceGroupRatio01(influenceGroup, parameters);

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

    public float ComputeInfluenceGroupRatio01(InfluenceGroup group, IReadOnlyDictionary<ParameterData, float> parameters)
    {
        float totalInfluenceRatio = 1f;

        foreach(Condition condition in group.Conditions)
        {
            float influenceRatio = condition.GetInfluenceRatio01(parameters);

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
            traitInfo.Status = ETraitStatus.Discovered;
        }
        else
        {
            traitInfo.Status = ETraitStatus.Great;
        }

        Traits[type] = traitInfo;
        OnTraitStatusChanged.Invoke(type, traitInfo.Status);
    }

    private void ParametersManager_OnParametersChanged(
        IReadOnlyDictionary<ParameterData, float> parameters,
        int tickCount
        )
    {
        RefreshTraits(tickCount, parameters);
    }

    private void ParametersManager_OnPreviewed(
        IReadOnlyDictionary<ParameterData, float> parameters,
        int tickCount
        )
    {
        SavedTraits = new Dictionary<TraitData, TraitInfo>(Traits);
        RefreshTraits(tickCount, parameters);
    }

    private void ParametersManager_OnPreviewLeft()
    {
        Traits = new(SavedTraits.ToDictionary(trait => trait.Key, trait => trait.Value));
    }

    public override void Awake()
    {
        base.Awake();

        ParametersManager.Instance.OnParametersChanged.AddListener(ParametersManager_OnParametersChanged);
        ParametersManager.Instance.OnPreviewed.AddListener(ParametersManager_OnPreviewed);
        ParametersManager.Instance.OnPreviewLeft.AddListener(ParametersManager_OnPreviewLeft);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(ParametersManager.HasInstance)
        {
            ParametersManager.Instance.OnParametersChanged.RemoveListener(ParametersManager_OnParametersChanged);
            ParametersManager.Instance.OnPreviewed.RemoveListener(ParametersManager_OnPreviewed);
            ParametersManager.Instance.OnPreviewLeft.RemoveListener(ParametersManager_OnPreviewLeft);
        }
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
