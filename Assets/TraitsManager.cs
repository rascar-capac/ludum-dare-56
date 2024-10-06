using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TraitsManager : Singleton<TraitsManager>
{
    public SerializableDictionary<TraitData, TraitInfo> Traits;
    public float DevelopingTraitThreshold;
    public float GreatTraitThreshold;
    public IReadOnlyDictionary<TraitData, TraitInfo> SavedTraits;

    public UnityEvent<TraitData, ETraitStatus> OnTraitStatusChanged { get; } = new();
    public UnityEvent<TraitData, float, float> OnTraitValueChanged { get; } = new();

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

        float oldValue = Traits[type].Value;
        TraitInfo traitInfo = Traits[type];
        traitInfo.Value = Mathf.Clamp01(traitInfo.Value + totalInfluenceRatio);
        Traits[type] = traitInfo;

        if(traitInfo.Value != oldValue)
        {
            OnTraitValueChanged.Invoke(type, oldValue, traitInfo.Value);
        }

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
        else if(traitInfo.Value < DevelopingTraitThreshold)
        {
            traitInfo.Status = ETraitStatus.Developing;
        }
        else if(traitInfo.Value < GreatTraitThreshold)
        {
            traitInfo.Status = ETraitStatus.Possessed;
        }
        else
        {
            traitInfo.Status = ETraitStatus.Great;
        }

        ETraitStatus oldStatus = Traits[type].Status;
        Traits[type] = traitInfo;

        if(traitInfo.Status != oldStatus)
        {
            OnTraitStatusChanged.Invoke(type, traitInfo.Status);
        }
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

    private void ParametersManager_OnPreviewClosed()
    {
        Traits = new(SavedTraits.ToDictionary(trait => trait.Key, trait => trait.Value));
    }

    public override void Awake()
    {
        base.Awake();

        ParametersManager.Instance.OnParametersChanged.AddListener(ParametersManager_OnParametersChanged);
        ParametersManager.Instance.OnPreviewed.AddListener(ParametersManager_OnPreviewed);
        ParametersManager.Instance.OnPreviewClosed.AddListener(ParametersManager_OnPreviewClosed);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(ParametersManager.HasInstance)
        {
            ParametersManager.Instance.OnParametersChanged.RemoveListener(ParametersManager_OnParametersChanged);
            ParametersManager.Instance.OnPreviewed.RemoveListener(ParametersManager_OnPreviewed);
            ParametersManager.Instance.OnPreviewClosed.RemoveListener(ParametersManager_OnPreviewClosed);
        }
    }
}

[Serializable]
public struct TraitInfo
{
    public float Value;
    public ETraitStatus Status;
}

public enum ETraitStatus
{
    NotPossessed = 0,
    Developing = 1,
    Possessed = 2,
    Great = 3,
}
