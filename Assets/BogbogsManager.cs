using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BogbogsManager : Singleton<BogbogsManager>
{
    public List<Bogbog> Bogbogs;
    public SerializableDictionary<Transform, Bogbog> DestinationPoints;
    public SerializableDictionary<Bogbog, float> NextDestinationAssignmentTimes;
    public FloatRange MinMaxDurationBeforeNextDestination;

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

    public void HandleDestinations()
    {
        foreach(Bogbog bogbog in Bogbogs)
        {
            if(!NextDestinationAssignmentTimes.TryGetValue(bogbog, out float nextTime) || Time.time > nextTime)
            {
                Transform previousDestination = bogbog.AssignedDestination;
                AssignRandomDestination(bogbog);
                FreeDestination(previousDestination);
            }
        }
    }

    public void AssignRandomDestination(Bogbog bogbog)
    {
        List<Transform> availableDestinations = DestinationPoints.Where(point => point.Value == null).Select(point => point.Key).ToList();
        Transform newDestination = availableDestinations[Random.Range(0, availableDestinations.Count)];
        bogbog.AssignDestination(newDestination);
        DestinationPoints[newDestination] = bogbog;
        NextDestinationAssignmentTimes[bogbog] = Time.time + MinMaxDurationBeforeNextDestination.GetRandomValue();
    }

    public void FreeDestination(Transform destination)
    {
        if(destination == null )
        {
            return;
        }

        DestinationPoints[destination] = null;
    }

    public override void Awake()
    {
        base.Awake();

        TraitsManager.Instance.OnTraitStatusChanged.AddListener(TraitsManager_OnTraitStatusChanged);
    }

    private void Update()
    {
        HandleDestinations();
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
