using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private Text interactionText;
    [SerializeField] private List<GameObject> UIObjects;

    private void Awake()
    {
        if(instance && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void ShowInteractionText(string text)
    {
        if (!interactionText)
            return;

        interactionText.gameObject.SetActive(true);
        interactionText.text = text;
    }

    public void HideInteractionText()
    {
        if (!interactionText)
            return;
        interactionText.gameObject.SetActive(false);
    }
	
    public void SetUIActive(bool active)
    {
        for (int i = 0; i < UIObjects.Count; i++)
        {
            UIObjects[i].SetActive(active);
        }
    }

}
