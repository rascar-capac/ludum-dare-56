using System.Collections.Generic;
using System.Linq;

public class TraitsManager : Singleton<TraitsManager>
{
    public Dictionary<TraitData, float> CurrentTraits;

    public void RefreshTraits()
    {
        List<TraitData> traitTypes = CurrentTraits.Keys.ToList();

        foreach(TraitData type in traitTypes)
        {
            RefreshTrait(type);
        }
    }

    public void RefreshTrait(TraitData type)
    {
        foreach(InfluenceGroup influence_group in type.Influences)
        {
            float total_influence_ratio = 1f;

            foreach(Condition condition in influence_group.Conditions)
            {
                total_influence_ratio = condition.GetInfluenceRatio();
            }

            CurrentTraits[type] += influence_group.InfluencePerTick * total_influence_ratio;
        }
    }
}
