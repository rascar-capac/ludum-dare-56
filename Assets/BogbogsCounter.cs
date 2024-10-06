using DG.Tweening;
using TMPro;
using UnityEngine;

public class BogbogsCounter : MonoBehaviour
{
    public TMP_Text Counter;

    public void Refresh()
    {
        Counter.text = $"{BogbogsManager.Instance.Bogbogs.Count}/{BogbogsManager.Instance.MaxBogbogCount}";
        Counter.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f);
    }

    private void BogbogsManager_OnBogbogCountChanged()
    {
        Refresh();
    }

    private void Awake()
    {
        BogbogsManager.Instance.OnBogbogCountChanged.AddListener(BogbogsManager_OnBogbogCountChanged);
    }

    private void OnDestroy()
    {
        if(BogbogsManager.HasInstance)
        {
            BogbogsManager.Instance.OnBogbogCountChanged.RemoveListener(BogbogsManager_OnBogbogCountChanged);
        }
    }
}
