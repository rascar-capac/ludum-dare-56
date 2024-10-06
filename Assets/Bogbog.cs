using System.Linq;
using UnityEngine;

public class Bogbog : MonoBehaviour
{
    public SerializableDictionary<TraitData, GameObject> Attributes;

    public void SetEnabledAttribute(bool is_enabled, TraitData traitType)
    {
        if(!Attributes.TryGetValue(traitType, out GameObject attribute))
        {
            return;
        }

        attribute.SetActive(is_enabled);
    }

    public void SetEnabledAllAttributes(bool is_enabled)
    {
        foreach(GameObject attribute in Attributes.Values)
        {
            attribute.SetActive(is_enabled);
        }
    }

    public void SaveCurrentState()
    {
        //save position
    }

    public void RestorePreviousState()
    {
        //restore position
    }

    private void Awake()
    {
        SetEnabledAllAttributes(false);
    }
}
