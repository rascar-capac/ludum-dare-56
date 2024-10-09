using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BogbogsManager : Singleton<BogbogsManager>
{
    public List<Bogbog> Bogbogs;
    public SerializableDictionary<Transform, Bogbog> Spots;
    public SerializableDictionary<Transform, Bogbog> DestinationPoints;
    public FloatRange MinMaxDurationBeforeNextDestination;
    public FloatRange MinMaxReproductionProbabilityPerTick;
    public FloatRange MinMaxDeathRatioPerTick;
    public Bogbog BogbogPrefab;
    public Transform BogbogContainer;
    public TraitData HornyTraitData;
    public TraitData HurtTraitData;

    public List<Bogbog> DeadBogbogs;
    public List<Bogbog> BogbogsBeforePreview;
    public List<Bogbog> DeadBogbogsBeforePreview;
    public int MaxBogbogCount => DestinationPoints.Count - 1;

    public SerializableDictionary<Bogbog, float> NextDestinationAssignmentTimes;

    public UnityEvent OnBogbogCountChanged { get; } = new();
    public UnityEvent<int> OnAllBogbogDead { get; } = new();

    public void SaveCurrentState()
    {
        foreach(Bogbog bogbog in Bogbogs)
        {
            Bogbog copy = Instantiate(bogbog, BogbogContainer);
            copy.name = "BogbogCopy";
            BogbogsBeforePreview.Add(copy);
            copy.gameObject.SetActive(false);
        }

        foreach(Bogbog deadBogbog in DeadBogbogs)
        {
            Bogbog copy = Instantiate(deadBogbog, BogbogContainer);
            copy.name = "BogbogCopy";
            DeadBogbogsBeforePreview.Add(copy);
            copy.gameObject.SetActive(false);
        }
    }

    public void RestorePreviousState()
    {
        int previousCount = Bogbogs.Count;

        foreach(Bogbog bogbog in Bogbogs)
        {
            Destroy(bogbog.gameObject);
        }

        foreach(Bogbog deadBogbog in DeadBogbogs)
        {
            Destroy(deadBogbog.gameObject);
        }

        Bogbogs.Clear();
        DeadBogbogs.Clear();

        foreach(Bogbog savedBogbog in BogbogsBeforePreview)
        {
            savedBogbog.name = "Bogbog";
            Bogbogs.Add(savedBogbog);
            savedBogbog.gameObject.SetActive(true);
        }

        foreach(Bogbog savedDeadBogbog in DeadBogbogsBeforePreview)
        {
            savedDeadBogbog.name = "Bogbog";
            DeadBogbogs.Add(savedDeadBogbog);
            savedDeadBogbog.gameObject.SetActive(true);
        }

        BogbogsBeforePreview.Clear();
        DeadBogbogsBeforePreview.Clear();

        if(previousCount != Bogbogs.Count)
        {
            OnBogbogCountChanged.Invoke();
        }
    }

    public void AssignSpots()
    {
        int spotsToAssignCount = Random.Range(0, Mathf.Min(Spots.Count, Bogbogs.Count) + 1);

        List<Bogbog> availableBogbogs = new(Bogbogs);
        List<Transform> availableSpots = new(Spots.Keys);

        for(int assignmentIndex = 0; assignmentIndex < spotsToAssignCount; assignmentIndex++)
        {
            Bogbog bogbog = availableBogbogs[Random.Range(0, availableBogbogs.Count)];
            Transform spot = availableSpots[Random.Range(0, availableSpots.Count)];
            AssignSpot(bogbog, spot);
            AssignDestination(bogbog, null);
            availableBogbogs.Remove(bogbog);
            availableSpots.Remove(spot);
        }

        List<Transform> availableDestinationPoints = new(DestinationPoints.Keys);

        foreach(Bogbog bogbog in availableBogbogs)
        {
            Transform point = availableDestinationPoints[Random.Range(0, availableDestinationPoints.Count)];
            AssignSpot(bogbog, null);
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

        if(availableDestinations.Count == 0)
        {
            Debug.LogError("No more available destination point");

            return null;
        }

        Transform newDestination = availableDestinations[Random.Range(0, availableDestinations.Count)];

        return newDestination;
    }

    public Transform GetRandomSpot()
    {
        List<Transform> availableSpots = Spots.Where(spot => spot.Value == null).Select(spot => spot.Key).ToList();

        if(availableSpots.Count == 0)
        {
            return null;
        }

        Transform newSpot = availableSpots[Random.Range(0, availableSpots.Count)];

        return newSpot;
    }

    public void AssignSpot(Bogbog bogbog, Transform spot)
    {
        Transform previousSpot = bogbog.AssignedSpot;
        bogbog.AssignSpot(spot);
        FreeSpot(previousSpot);

        if(spot != null)
        {
            Spots[spot] = bogbog;
        }
    }

    public void FreeSpot(Transform spot)
    {
        if(spot == null)
        {
            return;
        }

        Spots[spot] = null;
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

    public void TickDeath(int tickCount)
    {
        TraitInfo hurtTraitInfo = TraitsManager.Instance.Traits[HurtTraitData];

        if(hurtTraitInfo.Status == ETraitStatus.NotPossessed || hurtTraitInfo.Status == ETraitStatus.Developing)
        {
            return;
        }

        for(int tickIndex = 0; tickIndex < tickCount; tickIndex++)
        {
            if(Bogbogs.Count == 0)
            {
                OnAllBogbogDead.Invoke(ParametersManager.Instance.TotalTicksCounter - tickCount + Mathf.Max(0, tickIndex - 1));

                return;
            }

            float deathRatio = MinMaxDeathRatioPerTick.RemapFrom(hurtTraitInfo.Value, 0.3f, 1f, must_clamp: true);
            int deathCount = Mathf.CeilToInt(Bogbogs.Count * deathRatio);

            for(int bogbogIndex = 0; bogbogIndex < deathCount; bogbogIndex++)
            {
                KillBogbog(Bogbogs[bogbogIndex]);
            }
        }
    }

    public void KillBogbog(Bogbog bogbog)
    {
        if(bogbog.IsAssignedToSpot)
        {
            AssignSpot(bogbog, null);
        }
        else
        {
            AssignDestination(bogbog, null);
        }

        bogbog.Kill();
        Bogbogs.Remove(bogbog);
        DeadBogbogs.Add(bogbog);
        OnBogbogCountChanged.Invoke();
    }

    public void TickReproduction(int tickCount)
    {
        TraitInfo hornyTraitInfo = TraitsManager.Instance.Traits[HornyTraitData];

        if(hornyTraitInfo.Status == ETraitStatus.NotPossessed || hornyTraitInfo.Status == ETraitStatus.Developing)
        {
            return;
        }

        if(Bogbogs.Count < 2)
        {
            return;
        }

        for(int tickIndex = 0; tickIndex < tickCount; tickIndex++)
        {
            if(Bogbogs.Count == MaxBogbogCount)
            {
                return;
            }

            float probability = MinMaxReproductionProbabilityPerTick.RemapFrom(hornyTraitInfo.Value, 0.3f, 1f, must_clamp: true);

            if(Random.value < probability)
            {
                SpawnBogbog();
            }
        }
    }

    public void SpawnBogbog()
    {
        if(Bogbogs.Count == MaxBogbogCount)
        {
            return;
        }

        Bogbog bogbog = Instantiate(BogbogPrefab);
        Bogbogs.Add(bogbog);
        OnBogbogCountChanged.Invoke();

        Transform random_spot = GetRandomSpot();
        Transform random_destination = GetRandomDestination();

        if(random_destination == null || random_spot != null && Random.value > 0.5)
        {
            AssignSpot(bogbog, random_spot);
        }
        else
        {
            AssignDestination(bogbog, random_destination, instant: true);
        }

        bogbog.ResetAnimation();
        RefreshAllAttributes(bogbog);
    }

    public void RefreshAllAttributes(Bogbog bogbog)
    {
        foreach(TraitData traitType in bogbog.Attributes.Keys)
        {
            RefreshAttribute(traitType, bogbog);
        }
    }

    public void RefreshAttribute(TraitData traitType, Bogbog bogbog)
    {
        switch(TraitsManager.Instance.Traits[traitType].Status)
        {
            case ETraitStatus.NotPossessed:
            case ETraitStatus.Developing:
            {
                bogbog.SetEnabledAttribute(false, traitType);

                break;
            }
            case ETraitStatus.Possessed:
            case ETraitStatus.Great:
            {
                bogbog.SetEnabledAttribute(true, traitType);

                break;
            }
        }
    }

    private void TraitsManager_OnTraitChanged(TraitData traitType, TraitInfo traitInfo)
    {
        foreach(Bogbog bogbog in Bogbogs)
        {
            RefreshAttribute(traitType, bogbog);
        }
    }

    private void ParametersManager_OnCommited(IReadOnlyDictionary<ParameterData, float> parameters, int tickCount)
    {
        TickDeath(tickCount);
        TickReproduction(tickCount);
        AssignSpots();
    }

    private void ParametersManager_OnPreviewed(IReadOnlyDictionary<ParameterData, float> parameters, int tickCount)
    {
        TickDeath(tickCount);
        TickReproduction(tickCount);
        AssignSpots();
    }

    public override void Awake()
    {
        base.Awake();

        foreach(Bogbog bogbog in Bogbogs)
        {
            Destroy(bogbog.gameObject);
        }

        Bogbogs.Clear();

        SpawnBogbog();
        SpawnBogbog();

        TraitsManager.Instance.OnTraitChanged.AddListener(TraitsManager_OnTraitChanged);
        ParametersManager.Instance.OnCommited.AddListener(ParametersManager_OnCommited);
        ParametersManager.Instance.OnPreviewed.AddListener(ParametersManager_OnPreviewed);
    }

    private void Update()
    {
        HandleDestinations();
    }

    private void OnDrawGizmos()
    {
        foreach(Transform destinationPoint in DestinationPoints.Keys)
        {
            Gizmos.DrawSphere(destinationPoint.position, 0.1f);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(TraitsManager.HasInstance)
        {
            TraitsManager.Instance.OnTraitChanged.RemoveListener(TraitsManager_OnTraitChanged);
        }

        if(ParametersManager.HasInstance)
        {
            ParametersManager.Instance.OnCommited.AddListener(ParametersManager_OnCommited);
            ParametersManager.Instance.OnPreviewed.AddListener(ParametersManager_OnPreviewed);
        }
    }
}
