using DG.Tweening;
using UnityEngine;

public class Bogbog : MonoBehaviour
{
    public SerializableDictionary<TraitData, GameObject> Attributes;
    public float MetersPerSecond;
    public float MovementRotationAngle;
    public float MovementRotationDuration;
    public Animator Animator;

    public Transform AssignedDestination;
    public Transform AssignedSpot;
    public bool IsDead;

    public bool IsAssignedToSpot => AssignedSpot != null;
    public bool DestinationIsReached => (AssignedDestination.position - transform.position).sqrMagnitude < 0.1f;

    public void SetEnabledAttribute(bool is_enabled, TraitData traitData)
    {
        if(!Attributes.TryGetValue(traitData, out GameObject attribute) || traitData.IsHidden)
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
        if(spot != null)
        {
            transform.position = spot.position;
            AssignedDestination = null;
        }

        AssignedSpot = spot;

        ResetAnimation();
    }

    public void AssignDestination(Transform destination, bool instant = false)
    {
        if(destination != null)
        {
            if(instant)
            {
                transform.position = destination.position;
            }

            AssignedSpot = null;
        }

        AssignedDestination = destination;

        ResetAnimation();
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

    public void Kill()
    {
        Animator.enabled = false;
        IsDead = true;
    }

    public void ResetAnimation()
    {
        Animator.Play("idle0", 0, Random.value);
    }

    private void Awake()
    {
        SetEnabledAllAttributes(false);
    }

    private void Update()
    {
        MoveToDestination();
    }
}
