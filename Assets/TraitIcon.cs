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
    public Sprite UnknownTraitSprite;

    public void Initialize(TraitData data, TraitInfo info)
    {
        Data = data;
        Text.text = data.Name;
        SetStatus(info.Status, force: true);
    }

    public void SetStatus(ETraitStatus status, bool force = false)
    {
        if(force)
        {
            gameObject.SetActive(status != ETraitStatus.NotPossessed);
        }
        else
        {
            if(status == ETraitStatus.NotPossessed)
            {
                if(gameObject.activeSelf)
                {
                    DOTween.Sequence()
                        .Join(transform.DOScale(Vector3.zero, BumpDuration).SetEase(Ease.InBack))
                        .Join(transform.DORotate(new Vector3(0, 0, 20f), BumpDuration).SetEase(Ease.InBack))
                        .OnComplete(() => gameObject.SetActive(false));
                }
            }
            else
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
        }

        Icon.sprite = status == ETraitStatus.Developing ? UnknownTraitSprite : Data.Icon;
        Text.transform.parent.gameObject.SetActive(status != ETraitStatus.Developing);
        GreatBackground.SetActive(status == ETraitStatus.Great);
    }
}
