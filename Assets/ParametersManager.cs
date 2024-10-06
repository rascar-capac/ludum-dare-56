using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class ParametersManager : Singleton<ParametersManager>
{
    public SerializableDictionary<ParameterData, float> Parameters;
    public int CurrentAttemptsCount;
    public bool IsInPreview;
    public IReadOnlyDictionary<ParameterData, float> PreviewParameters;

    public IReadOnlyDictionary<ParameterData, float> ParametersDisplayed => IsInPreview ? PreviewParameters : Parameters;

    public UnityEvent<IReadOnlyDictionary<ParameterData, float>> OnParametersChanged { get; } = new();
    public UnityEvent<IReadOnlyDictionary<ParameterData, float>> OnPreviewed { get; } = new();
    public UnityEvent OnPreviewLeft { get; } = new();
    public UnityEvent OnCommited { get; } = new();

    public void SetParameters(IReadOnlyDictionary<ParameterData, float> parameters)
    {
        List<ParameterData> parameterTypes = Parameters.Keys.ToList();

        foreach(ParameterData type in parameterTypes)
        {
            Parameters[type] = parameters[type];
        }

        OnParametersChanged.Invoke(Parameters);
    }

    public void Preview(IReadOnlyDictionary<ParameterData, float> parameters)
    {
        if(CurrentAttemptsCount >= 3)
        {
            return;
        }

        TryQuitPreview();

        BogbogsManager.Instance.SaveCurrentState();
        CurrentAttemptsCount++;
        IsInPreview = true;
        PreviewParameters = parameters;
        OnPreviewed.Invoke(parameters);
        //post process
    }

    public void TryQuitPreview()
    {
        if(!IsInPreview)
        {
            return;
        }

        BogbogsManager.Instance.RestorePreviousState();
        IsInPreview = false;
        PreviewParameters = null;
        OnPreviewLeft.Invoke();
        //post process
    }

    public void Commit(IReadOnlyDictionary<ParameterData, float> parameters)
    {
        TryQuitPreview();
        SetParameters(parameters);
        CurrentAttemptsCount = 0;
        //handle fade in/out
        //potential defeat/win
        //stop post process
        OnCommited.Invoke();
    }
}
