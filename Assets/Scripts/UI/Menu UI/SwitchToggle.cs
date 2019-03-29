using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SwitchToggle : MonoBehaviour
{
    public bool switchImage = false;
    public Sprite CheckedImage;
    public Sprite UncheckedImage;
    public Image checkMark;
    private Toggle thisToggle;

    private void Awake()
    {
        thisToggle = GetComponent<Toggle>();
        thisToggle.onValueChanged.AddListener(x => SwitchImageOnValueChanged(x));
    }

    public void Switch()
    {
        thisToggle.isOn = !thisToggle.isOn;
    }

    private void SwitchImageOnValueChanged(bool isChecked)
    {
        if (switchImage && checkMark)
        {
            if (isChecked)
            {
                checkMark.sprite  = CheckedImage;
            }
            else
            {
                checkMark.sprite = UncheckedImage;
            }
        }
    }

}
