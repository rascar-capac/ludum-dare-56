using System;

[Serializable]
public abstract class Condition
{
    public AConditionData Type;
    public bool IsRange;
    public FloatRange Thresholds;

    public abstract float ParameterValue { get; }

    public virtual float GetInfluenceRatio()
    {
        if(IsRange)
        {
            return Thresholds.Contains(ParameterValue) ? 1 : -1;
        }
        else
        {
            return Thresholds.RemapTo(ParameterValue);
        }
    }
}
