using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DaysCounter : MonoBehaviour
{
    public TMP_Text Counter;

    public void Refresh()
    {
        Counter.text = ParametersManager.Instance.TotalTicksCounter.ToString();
        Counter.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f);
    }

    private void ParametersManager_OnPreviewed(IReadOnlyDictionary<ParameterData, float> parameters, int tickCount)
    {
        Refresh();
    }

    private void ParametersManager_OnPreviewClosed()
    {
        Refresh();
    }

    private void ParametersManager_OnCommited(IReadOnlyDictionary<ParameterData, float> parameters, int tickCount)
    {
        Refresh();
    }

    private void Awake()
    {
        ParametersManager.Instance.OnPreviewed.AddListener(ParametersManager_OnPreviewed);
        ParametersManager.Instance.OnPreviewClosed.AddListener(ParametersManager_OnPreviewClosed);
        ParametersManager.Instance.OnCommited.AddListener(ParametersManager_OnCommited);
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDestroy()
    {
        if(ParametersManager.HasInstance)
        {
            ParametersManager.Instance.OnPreviewed.RemoveListener(ParametersManager_OnPreviewed);
            ParametersManager.Instance.OnPreviewClosed.RemoveListener(ParametersManager_OnPreviewClosed);
            ParametersManager.Instance.OnCommited.RemoveListener(ParametersManager_OnCommited);
        }
    }
}
