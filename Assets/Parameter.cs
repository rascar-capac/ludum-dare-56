using System;

[Serializable]
public class Parameter : Condition
{
    public override float ParameterValue => ParametersManager.Instance.CurrentParameters[ Type as ParameterData ];
}
