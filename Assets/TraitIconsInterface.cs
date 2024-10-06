using UnityEngine;

public class TraitIconsInterface : MonoBehaviour
{
    public Transform Container;
    public TraitIcon TraitIconPrefab;
    public SerializableDictionary<TraitData, TraitIcon> TraitIcons;

    private void TraitsManager_OnTraitStatusChanged(TraitData data, ETraitStatus status)
    {
        if(!TraitIcons.TryGetValue(data, out TraitIcon icon) || data.IsHidden)
        {
            return;
        }

        icon.RefreshStatus(status);
    }

    private void TraitsManager_OnTraitValueChanged(TraitData data, float oldValue, float newValue)
    {
        if(!TraitIcons.TryGetValue(data, out TraitIcon icon) || data.IsHidden)
        {
            return;
        }

        icon.RefreshFluctuation(difference: newValue - oldValue);
    }

    private void Awake()
    {
        foreach(Transform child in Container)
        {
            Destroy(child.gameObject);
        }

        TraitsManager.Instance.OnTraitStatusChanged.AddListener(TraitsManager_OnTraitStatusChanged);
        TraitsManager.Instance.OnTraitValueChanged.AddListener(TraitsManager_OnTraitValueChanged);

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
            TraitsManager.Instance.OnTraitStatusChanged.RemoveListener(TraitsManager_OnTraitStatusChanged);
            TraitsManager.Instance.OnTraitValueChanged.RemoveListener(TraitsManager_OnTraitValueChanged);
        }
    }
}
