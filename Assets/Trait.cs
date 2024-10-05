using System;

[Serializable]
public class Trait: Condition
{
    public override float ParameterValue => TraitsManager.Instance.CurrentTraits[ Type as TraitData ];
}
