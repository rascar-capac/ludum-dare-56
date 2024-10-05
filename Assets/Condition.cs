using System;

[Serializable]
public class Condition
{
    public AConditionData Type;
    public bool IsRange;
    public FloatRange Thresholds;
}
