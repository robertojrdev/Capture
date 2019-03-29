using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class AudioSlider : MonoBehaviour
{
    public SFX.MixerGroup type = SFX.MixerGroup.Master;

    private Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
        slider.maxValue = 1;
        slider.minValue = 0;
        slider.wholeNumbers = false;
        slider.value = DbToPercentage(SFX.GetVolume(type));
        slider.onValueChanged.AddListener(x => SFX.ChangeVolume(type, PercentageToDb(x)));
    }

    public float PercentageToDb(float percentage)
    {
        percentage = Mathf.Clamp(percentage, 0, 1);
        float circular = Mathf.Sqrt(1f - ((percentage -= 1f) * percentage));
        return Mathf.Lerp(-80, 0, circular);
    }

    public float DbToPercentage(float db)
    {
        float percentage = (80 + db) / 80;
        float circular = percentage * percentage;
        Debug.Log(circular);
        return circular;
    }
}
