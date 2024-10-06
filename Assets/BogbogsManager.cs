using System.Collections.Generic;

public class BogbogsManager : Singleton<BogbogsManager>
{
    public List<Bogbog> Bogbogs;

    public void SetEnabledAttribute(bool is_enabled, TraitData traitType)
    {
        foreach(Bogbog bogbog in Bogbogs)
        {
            bogbog.SetEnabledAttribute(is_enabled, traitType);
        }
    }

    private void TraitsManager_OnTraitStatusChanged(TraitData traitData, ETraitStatus status)
    {
        switch(status)
        {
            case ETraitStatus.NotPossessed:
            {
                SetEnabledAttribute(false, traitData);

                break;
            }
            case ETraitStatus.Unknown:
            case ETraitStatus.Discovered:
            case ETraitStatus.Great:
            {
                SetEnabledAttribute(true, traitData);

                break;
            }
        }
    }

    public void SaveCurrentState()
    {
        foreach(Bogbog bogbog in Bogbogs)
        {
            bogbog.SaveCurrentState();
        }
    }

    public void RestorePreviousState()
    {
        foreach(Bogbog bogbog in Bogbogs)
        {
            bogbog.RestorePreviousState();
        }
    }

    public override void Awake()
    {
        base.Awake();

        TraitsManager.Instance.OnTraitStatusChanged.AddListener(TraitsManager_OnTraitStatusChanged);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(TraitsManager.HasInstance)
        {
            TraitsManager.Instance.OnTraitStatusChanged.RemoveListener(TraitsManager_OnTraitStatusChanged);
        }
    }
}
