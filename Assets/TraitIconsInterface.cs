using UnityEngine;

public class TraitIconsInterface : MonoBehaviour
{
    public Transform Container;
    public TraitIcon TraitIconPrefab;
    public SerializableDictionary<TraitData, TraitIcon> TraitIcons;

    private void TraitsManager_OnTraitStatusChanged(TraitData type, ETraitStatus oldStatus, ETraitStatus newStatus)
    {
        if(oldStatus == newStatus || !TraitIcons.TryGetValue(type, out TraitIcon icon))
        {
            return;
        }

        icon.SetStatus(newStatus);
    }

    private void Awake()
    {
        foreach(Transform child in Container)
        {
            Destroy(child.gameObject);
        }

        TraitsManager.Instance.OnTraitStatusChanged.AddListener(TraitsManager_OnTraitStatusChanged);

        foreach((TraitData type, TraitInfo info) in TraitsManager.Instance.Traits)
        {
            TraitIcon icon = Instantiate(TraitIconPrefab, Container);
            icon.Initialize(type, info);
            TraitIcons[type] = icon;
        }
    }
}
