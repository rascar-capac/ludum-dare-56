using System;
using UnityEngine;

[Serializable]
public class Condition
{
    public AConditionData Type;
    public bool IsRange;
    public FloatRange Thresholds;

    public float GetInfluenceRatio01()
    {
        if(IsRange)
        {
            return Thresholds.Contains(GetParameterValue()) ? 1 : 0;
        }
        else
        {
            return Mathf.Clamp01(Thresholds.RemapTo(GetParameterValue()));
        }
    }

    public float GetParameterValue()
    {
        return Type switch
        {
            ParameterData parameter_data => ParametersManager.Instance.Parameters[parameter_data],
            TraitData trait_data => TraitsManager.Instance.Traits[trait_data].Value,
            _ => throw new NotImplementedException()
        };
    }
}
