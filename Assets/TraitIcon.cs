using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TraitIcon : MonoBehaviour
{
    public GameObject GreatBackground;
    public Image Icon;
    public TMP_Text Text;
    public TraitData Data;
    public float BumpDuration;
    public float GreatBumpDuration;

    public void Initialize(TraitData data, TraitInfo info)
    {
        Data = data;
        Icon.sprite = data.Icon;
        SetStatus(info.Status, force: true);
    }

    public void SetStatus(ETraitStatus status, bool force = false)
    {
        if(force)
        {
            gameObject.SetActive(status != ETraitStatus.NotPossessed);
            GreatBackground.transform.localScale = status == ETraitStatus.Great ? Vector3.one : Vector3.one * 0.7f;
        }
        else
        {
            if(status == ETraitStatus.NotPossessed
                && gameObject.activeSelf
                )
            {
                DOTween.Sequence()
                    .Join(transform.DOScale(Vector3.zero, BumpDuration).SetEase(Ease.InBack))
                    .Join(transform.DORotate(new Vector3(0, 0, 20f), BumpDuration).SetEase(Ease.InBack))
                    .OnComplete(() => gameObject.SetActive(false));
            }
            else if( status != ETraitStatus.NotPossessed
                && !gameObject.activeSelf
                )
            {
                DOTween.Sequence()
                    .Join(transform.DOScale(Vector3.one, BumpDuration).SetEase(Ease.OutBack))
                    .Join(transform.DORotate(new Vector3(0, 0, 0f), BumpDuration).SetEase(Ease.OutBack))
                    .OnStart(() =>
                    {
                        gameObject.SetActive(true);
                        transform.localScale = Vector3.zero;
                        transform.rotation = Quaternion.Euler(0, 0, 20f);
                    });
            }

            GreatBackground.transform.DOScale(status == ETraitStatus.Great ? Vector3.one : Vector3.one * 0.7f, GreatBumpDuration).SetEase(Ease.OutQuint);
        }

        Text.text = status == ETraitStatus.Unknown ? "?" : Data.Name;
    }
}
