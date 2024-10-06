using DG.Tweening;
using UnityEngine;

public class Bogbog : MonoBehaviour
{
    public SerializableDictionary<TraitData, GameObject> Attributes;
    public Transform AssignedDestination;
    public float MetersPerSecond;
    public float MovementRotationAngle;
    public float MovementRotationDuration;

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

    public void AssignDestination(Transform destination)
    {
        AssignedDestination = destination;
        transform.rotation = Quaternion.Euler(0f, 0f, -MovementRotationAngle);
        transform.DOKill();
        transform.DORotate(new Vector3(0f, 0f, MovementRotationAngle), MovementRotationDuration).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }

    public void MoveToDestination()
    {
        if(AssignedDestination == null)
        {
            return;
        }

        if((AssignedDestination.position - transform.position).sqrMagnitude < 0.1f)
        {
            AssignedDestination = null;
            transform.rotation = Quaternion.identity;
            transform.DOKill();

            return;
        }

        Vector3 direction = (AssignedDestination.position - transform.position).normalized;
        Vector3 translation = MetersPerSecond * Time.deltaTime * direction;
        transform.Translate(translation);
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
