using UnityEngine;

public class DayNightSystem : MonoBehaviour
{
    public Material dayFogMaterial;
    public Material nightFogMaterial;
    public Material daySkybox;
    public Material nightSkybox;
    public GameObject dayLight;
    public GameObject nightLight;

    public void SetDay()
    {
        GameCamera.instance.GetComponent<ImageEffects>().material = dayFogMaterial;
        RenderSettings.skybox = daySkybox;
        nightLight.SetActive(false);
        dayLight.SetActive(true);
    }
    public void SetNight()
    {
        GameCamera.instance.GetComponent<ImageEffects>().material = nightFogMaterial;
        RenderSettings.skybox = nightSkybox;
        nightLight.SetActive(true);
        dayLight.SetActive(false);
    }
}
