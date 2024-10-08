using TMPro;
using UnityEngine;

public class DebugParameterInfo : MonoBehaviour
{
    public TMP_Text Name;
    public TMP_Text LastValueApplied;
    public TMP_Text PreviewValue;

    public ParameterData Type;

    public void Initialize(ParameterData type)
    {
        Name.text = type.name;
        Type = type;
    }

    public void Refresh(float lastValueApplied, float? previewValue = null)
    {
        LastValueApplied.text = lastValueApplied.ToString("N1");

        if(previewValue != null)
        {
            PreviewValue.enabled = true;
            PreviewValue.text = "> " + previewValue.Value.ToString("N1");
        }
        else
        {
            PreviewValue.enabled = false;
        }
    }
}
