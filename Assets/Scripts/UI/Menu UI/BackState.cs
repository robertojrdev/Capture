using UnityEngine;
using UnityEngine.UI;

public class BackState : MonoBehaviour
{
    public Selectable buttonToSetActive;
    private BackState back;

    private void Update()
    {
        if (Input.GetButtonDown("Back"))
        {
            CallBackState();
        }
    }

    public void SetBackState(BackState back)
    {
        this.back = back;
    }

    public void CallBackState()
    {
        if (!back)
            return;

        gameObject.SetActive(false);
        back.gameObject.SetActive(true);

        if (back.buttonToSetActive)
            back.buttonToSetActive.Select();
    }
}
