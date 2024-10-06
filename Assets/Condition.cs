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
            ParameterData parameter_data => parameters[parameter_data],
            TraitData trait_data => TraitsManager.Instance.Traits[trait_data].Value,
            _ => throw new NotImplementedException()
        };
    }
}
