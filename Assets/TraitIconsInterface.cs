using UnityEngine;

public class TraitIconsInterface : MonoBehaviour
{
    public Transform Container;
    public TraitIcon TraitIconPrefab;
    public SerializableDictionary<TraitData, TraitIcon> TraitIcons;

    private void TraitsManager_OnTraitChanged(TraitData data, TraitInfo traitInfo)
    {
        if(!TraitIcons.TryGetValue(data, out TraitIcon icon) || data.IsHidden)
        {
            return;
        }

        icon.RefreshStatus(traitInfo.Status);
        icon.RefreshFluctuation(traitInfo.Status, difference: traitInfo.Fluctuation);
    }

    private void Awake()
    {
        foreach(Transform child in Container)
        {
            Destroy(child.gameObject);
        }

        TraitsManager.Instance.OnTraitChanged.AddListener(TraitsManager_OnTraitChanged);

        foreach((TraitData type, TraitInfo info) in TraitsManager.Instance.Traits)
        {
            TraitIcon icon = Instantiate(TraitIconPrefab, Container);
            icon.Initialize(type, info);
            TraitIcons[type] = icon;
        }
    }

    private void OnDestroy()
    {
        if(TraitsManager.HasInstance)
        {
            TraitsManager.Instance.OnTraitChanged.RemoveListener(TraitsManager_OnTraitChanged);
        }
    }
}
