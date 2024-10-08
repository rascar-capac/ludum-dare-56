using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugTraitInfo : MonoBehaviour
{
    public TMP_Text Name;
    public Image BackgroundFill;
    public Image ForegroundFill;
    public Color RealtimeColor;
    public Color PreviewColor;
    public TMP_Text RealTimeValueText;
    public TMP_Text PreviewValueText;

    public TraitData Type;

    public void Initialize(TraitData type)
    {
        Name.text = type.name;
        Type = type;
    }

    public void Refresh(float realtimeValue, float? previewValue = null)
    {
        bool realtimeValueIsGreater = previewValue == null || realtimeValue > previewValue.Value;

        BackgroundFill.fillAmount = realtimeValueIsGreater ? realtimeValue : previewValue.Value;
        BackgroundFill.color = realtimeValueIsGreater ? RealtimeColor : PreviewColor;

        RealTimeValueText.text = realtimeValue.ToString();

        if(previewValue != null)
        {
            ForegroundFill.enabled = true;
            ForegroundFill.fillAmount = realtimeValueIsGreater ? previewValue.Value : realtimeValue;
            ForegroundFill.color = realtimeValueIsGreater ? PreviewColor : RealtimeColor;

            PreviewValueText.text = "> " + previewValue.ToString();
        }
        else
        {
            ForegroundFill.enabled = false;
            PreviewValueText.enabled = false;
        }
    }
}
