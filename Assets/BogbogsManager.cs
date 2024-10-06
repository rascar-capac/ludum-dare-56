using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BogbogsManager : Singleton<BogbogsManager>
{
    public List<Bogbog> Bogbogs;
    public List<Transform> Spots;
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

    public void AssignSpots()
    {
        int spotsToAssignCount = Random.Range(0, Mathf.Min(Spots.Count, Bogbogs.Count) + 1);

        List<Bogbog> availableBogbogs = new(Bogbogs);
        List<Transform> availableSpots = new(Spots);

        for(int assignmentIndex = 0; assignmentIndex < spotsToAssignCount; assignmentIndex++)
        {
            Bogbog bogbog = availableBogbogs[Random.Range(0, availableBogbogs.Count)];
            Transform spot = availableSpots[Random.Range(0, availableSpots.Count)];
            bogbog.AssignSpot(spot);
            AssignDestination(bogbog, null);
            availableBogbogs.Remove(bogbog);
            availableSpots.Remove(spot);
        }

        List<Transform> availableDestinationPoints = new(DestinationPoints.Keys);

        foreach(Bogbog bogbog in availableBogbogs)
        {
            Transform point = availableDestinationPoints[Random.Range(0,availableDestinationPoints.Count)];
            AssignDestination(bogbog, point, instant: true);
            availableDestinationPoints.Remove(point);
        }
    }

    public void HandleDestinations()
    {
        foreach(Bogbog bogbog in Bogbogs)
        {
            if(bogbog.IsAssignedToSpot)
            {
                continue;
            }

            if(!NextDestinationAssignmentTimes.TryGetValue(bogbog, out float nextTime) || Time.time > nextTime)
            {
                AssignDestination(bogbog, GetRandomDestination());
            }
        }
    }

    public Transform GetRandomDestination()
    {
        List<Transform> availableDestinations = DestinationPoints.Where(point => point.Value == null).Select(point => point.Key).ToList();
        Transform newDestination = availableDestinations[Random.Range(0, availableDestinations.Count)];

        return newDestination;
    }

    public void AssignDestination(Bogbog bogbog, Transform destination, bool instant = false)
    {
        Transform previousDestination = bogbog.AssignedDestination;
        bogbog.AssignDestination(destination, instant);
        FreeDestination(previousDestination);
        NextDestinationAssignmentTimes[bogbog] = Time.time + MinMaxDurationBeforeNextDestination.GetRandomValue();

        if(destination != null)
        {
            DestinationPoints[destination] = bogbog;
        }
    }

    public void FreeDestination(Transform destination)
    {
        if(destination == null )
        {
            return;
        }

        DestinationPoints[destination] = null;
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

    private void ParametersManager_OnCommited()
    {
        AssignSpots();
    }

    private void ParametersManager_OnPreviewed(IReadOnlyDictionary<ParameterData, float> parameters, int tickCount)
    {
        AssignSpots();
    }

    public override void Awake()
    {
        base.Awake();

        AssignSpots();

        TraitsManager.Instance.OnTraitStatusChanged.AddListener(TraitsManager_OnTraitStatusChanged);
        ParametersManager.Instance.OnCommited.AddListener(ParametersManager_OnCommited);
        ParametersManager.Instance.OnPreviewed.AddListener(ParametersManager_OnPreviewed);
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

        if(ParametersManager.HasInstance)
        {
            ParametersManager.Instance.OnCommited.AddListener(ParametersManager_OnCommited);
            ParametersManager.Instance.OnPreviewed.AddListener(ParametersManager_OnPreviewed);
        }
    }
}
