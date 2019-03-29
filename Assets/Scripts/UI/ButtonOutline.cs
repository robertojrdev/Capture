using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Outline))]
public class ButtonOutline : EventTrigger
{
    private static GameObject selectedButton;
    public Outline outline;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        outline = GetComponent<Outline>();
        outline.outline.SetActive(false);
    }

    public override void OnDeselect(BaseEventData data)
    {
        outline.outline.SetActive(false);
    }

    public override void OnSelect(BaseEventData data)
    {
        outline.outline.SetActive(true);
    }

    private void OnDisable()
    {
        outline.outline.SetActive(false);
    }
}
