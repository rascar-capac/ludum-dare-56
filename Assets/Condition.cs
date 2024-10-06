using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Condition
{
    public AConditionData Type;
    public bool IsRange;
    public FloatRange Thresholds;

    public float GetInfluenceRatio01(IReadOnlyDictionary<ParameterData, float> parameters)
    {
        if(Type is TraitData traitType && (TraitsManager.Instance.Traits[traitType].Status == ETraitStatus.NotPossessed || TraitsManager.Instance.Traits[traitType].Status == ETraitStatus.Developing))
        {
            return 0;
        }

        if(IsRange)
        {
            return Thresholds.Contains(GetParameterValue(parameters)) ? 1 : 0;
        }
        else
        {
            return Mathf.Clamp01(Thresholds.RemapTo(GetParameterValue(parameters)));
        }
    }

    public float GetParameterValue(IReadOnlyDictionary<ParameterData, float> parameters)
    {
        return Type switch
        {
            ParameterData parameterType => parameters[parameterType],
            TraitData traitType => TraitsManager.Instance.Traits[traitType].Value,
            _ => throw new NotImplementedException()
        };
    }
}
