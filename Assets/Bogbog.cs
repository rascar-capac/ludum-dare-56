using DG.Tweening;
using UnityEngine;

public class Bogbog : MonoBehaviour
{
    public SerializableDictionary<TraitData, GameObject> Attributes;
    public Transform AssignedDestination;
    public Transform AssignedSpot;
    public bool IsAssignedToSpot => AssignedSpot != null;
    public float MetersPerSecond;
    public float MovementRotationAngle;
    public float MovementRotationDuration;
    public Animator Animator;

    public bool DestinationIsReached => (AssignedDestination.position - transform.position).sqrMagnitude < 0.1f;

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

    public void AssignSpot(Transform spot)
    {
        transform.position = spot.position;

        if(spot != null)
        {
            AssignedDestination = null;
        }

        AssignedSpot = spot;
    }

    public void AssignDestination(Transform destination, bool instant = false)
    {
        if(instant)
        {
            transform.position = destination.position;
        }

        if(destination != null)
        {
            AssignedSpot = null;
        }

        AssignedDestination = destination;

        UpdateMovementAnimation();
    }

    public void UpdateMovementAnimation()
    {
        transform.DOKill();

        if(AssignedDestination == null || DestinationIsReached)
        {
            transform.rotation = Quaternion.identity;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, -MovementRotationAngle);
            transform.DORotate(new Vector3(0f, 0f, MovementRotationAngle), MovementRotationDuration).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void MoveToDestination()
    {
        if(AssignedDestination == null || DestinationIsReached)
        {
            return;
        }

        Vector3 direction = (AssignedDestination.position - transform.position).normalized;
        Vector3 translation = MetersPerSecond * Time.deltaTime * direction;
        transform.Translate(translation);

        if(DestinationIsReached)
        {
            UpdateMovementAnimation();
        }
    }

    private void Awake()
    {
        SetEnabledAllAttributes(false);
        Animator.Play("idle0", 0, Random.value);
    }

    private void Update()
    {
        MoveToDestination();
    }
}
