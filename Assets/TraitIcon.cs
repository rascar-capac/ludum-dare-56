using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TraitIcon : MonoBehaviour
{
    public GameObject GreatBackground;
    public Image Icon;
    public TMP_Text Text;
    public TraitData Data;

    public void Initialize(TraitData data, TraitInfo info)
    {
        Data = data;
        Icon.sprite = data.Icon;
        SetStatus(info.Status);
    }

    public void SetStatus(ETraitStatus status)
    {
        gameObject.SetActive(status != ETraitStatus.NotPossessed);
        GreatBackground.SetActive(status == ETraitStatus.Great);
        Text.text = status == ETraitStatus.Unknown ? "?" : Data.Name;
    }
}
