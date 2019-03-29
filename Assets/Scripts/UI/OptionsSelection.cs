using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsSelection : MonoBehaviour
{
    private GameObject selectedOption;

    public void SelectOption(GameObject option)
    {
        if (selectedOption && selectedOption == option)
            return;

        if (selectedOption)
            selectedOption.SetActive(false);

        if (option != null)
        {
            selectedOption = option;
            option.SetActive(true);
        }
    }

}
