using UnityEngine;

public class HideInitialTextOnPickupController : MonoBehaviour 
{
    private void Update()
    {
        float axis = Input.GetAxis("Horizontal");
        axis += Input.GetAxis("Vertical");
        axis += Input.GetAxis("Horizontal Camera");
        axis += Input.GetAxis("Vertical Camera");

        if (Input.anyKeyDown || axis > 0.2f)
        {
            HideText();
        }
    }

    void HideText()
    {
        gameObject.SetActive(false);
    }
}
