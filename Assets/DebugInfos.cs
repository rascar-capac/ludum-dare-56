using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugInfos : MonoBehaviour
{
    public bool IsEnabled;
    public TMP_Text RealtimeBogbogCount;
    public TMP_Text PreviewBogbogCount;
    public Transform TraitContainer;
    public DebugTraitInfo TraitPrefab;
    public Transform ParameterContainer;
    public DebugParameterInfo ParameterPrefab;

    public List<DebugTraitInfo> TraitInfos;
    public List<DebugParameterInfo> ParameterInfos;

    public void Initialize()
    {
        foreach(Transform child in TraitContainer)
        {
            Destroy(child.gameObject);
        }

        foreach(Transform child in ParameterContainer)
        {
            Destroy(child.gameObject);
        }

        foreach(TraitData traitData in TraitsManager.Instance.Traits.Keys)
        {
            DebugTraitInfo traitInfo = Instantiate(TraitPrefab, TraitContainer);
            traitInfo.Initialize(traitData);
            TraitInfos.Add(traitInfo);
        }

        foreach(ParameterData parameterData in ParametersManager.Instance.Parameters.Keys)
        {
            DebugParameterInfo parameterInfo = Instantiate(ParameterPrefab, ParameterContainer);
            parameterInfo.Initialize(parameterData);
            ParameterInfos.Add(parameterInfo);
        }
    }

    public void UpdateDebugInfos()
    {
        RealtimeBogbogCount.text = ParametersManager.Instance.IsInPreview ? BogbogsManager.Instance.BogbogsBeforePreview.Count.ToString() : BogbogsManager.Instance.Bogbogs.Count.ToString();

        if(ParametersManager.Instance.IsInPreview)
        {
            PreviewBogbogCount.enabled = true;
            PreviewBogbogCount.text = "> " + BogbogsManager.Instance.Bogbogs.Count.ToString();
        }
        else
        {
            PreviewBogbogCount.enabled = false;
        }

        foreach(DebugTraitInfo traitInfo in TraitInfos)
        {
            float realtimeValue;
            float? previewValue;

            if(ParametersManager.Instance.IsInPreview)
            {
                realtimeValue = TraitsManager.Instance.TraitsBeforePreview[traitInfo.Type].Value;
                previewValue = TraitsManager.Instance.Traits[traitInfo.Type].Value;
                previewValue = previewValue != realtimeValue ? previewValue : null;
            }
            else
            {
                realtimeValue = TraitsManager.Instance.Traits[traitInfo.Type].Value;
                previewValue = null;
            }

            traitInfo.Refresh(realtimeValue, previewValue);
        }

        foreach(DebugParameterInfo parameterInfo in ParameterInfos)
        {
            float lastValueApplied = ParametersManager.Instance.Parameters[parameterInfo.Type];
            float? previewValue;

            if(ParametersManager.Instance.IsInPreview)
            {
                previewValue = ParametersManager.Instance.PreviewParameters[parameterInfo.Type];
                previewValue = previewValue != lastValueApplied ? previewValue : null;
            }
            else
            {
                previewValue = null;
            }

            parameterInfo.Refresh(lastValueApplied, previewValue);
        }
    }

    private void Awake()
    {
        Initialize();
        gameObject.SetActive(IsEnabled);
    }

    private void Update()
    {
        if(!IsEnabled)
        {
            return;
        }

        UpdateDebugInfos();
    }

    private void OnValidate()
    {
        if(Application.isPlaying)
        {
            gameObject.SetActive(IsEnabled);
        }
    }
}
