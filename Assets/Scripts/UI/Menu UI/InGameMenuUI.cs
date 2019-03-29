using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenuUI : MonoBehaviour
{
    public static InGameMenuUI instance;

    [SerializeField] private Button continueButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button albumButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Animator transitionAnimator;
    [Space(10)]
    [Header("Sub settings setup")]
    [SerializeField] private GameObject buttonsPrefab;
    [SerializeField] private RectTransform resolutionContentPanel;
    [SerializeField] private ScrollRect resolutionScroolRect;
    [Header("Quality settings")]
    [SerializeField] private RectTransform qualityContentPanel;
    [SerializeField] private ScrollRect qualityScroolRect;
    [Header("Menus")]
    [SerializeField] private List<GameObject> deactivatedAtStart = new List<GameObject>();
    [SerializeField] private List<GameObject> activatedAtStart = new List<GameObject>();

    private bool IsMouseEvent { get; set; }

    private void Awake()
    {
        if (instance && instance != this)
            Destroy(this);
        else
            instance = this;
    }

    private void Start()
    {
        continueButton.onClick.AddListener(() => Continue());
        albumButton.onClick.AddListener(() => Album());
        quitButton.onClick.AddListener(() => Quit());

        InstantiateResolutionButtons();
        InstantiateQualitySettingsButtons();
    }

    private void OnEnable()
    {
        continueButton.Select();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Pause"))
            Continue();
    }

    private void Continue()
    {
        GameManager.instance.SetGameState(GameManager.GameState.Playing);
    }

    private void Album()
    {
    }

    private void Quit()
    {
        LoadScreen.StartLoadScene(sceneIndex: 0);
    }

    public void ResetMenu()
    {
        foreach (var obj in deactivatedAtStart)
        {
            obj.SetActive(false);
        }

        foreach (var obj in activatedAtStart)
        {
            obj.SetActive(true);
        }
    }

    private void InstantiateResolutionButtons()
    {
        GameObject buttonObj;
        List<Button> buttonsList = new List<Button>();
        foreach (var resolution in Screen.resolutions)
        {
            buttonObj = Instantiate(buttonsPrefab, resolutionContentPanel.transform);
            Text[] texts = buttonObj.transform.GetComponentsInChildren<Text>();
            foreach (var text in texts)
            {
                text.text = resolution.width + " x " + resolution.height;
            }
            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => Screen.SetResolution(resolution.width, resolution.height, true));
            buttonsList.Add(button);

            //set function to on selected by the button EventTrigger
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            EventTrigger buttonTrigger = button.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Select;
            entry.callback.AddListener(x => SnapScroolViewToSelected(buttonRect, resolutionContentPanel, resolutionScroolRect));
            buttonTrigger.triggers.Add(entry);
        }

        SetButtonsNavigation(buttonsList);
    }

    private void InstantiateQualitySettingsButtons()
    {
        GameObject buttonObj;
        List<Button> buttonsList = new List<Button>();
        int i = 0;
        foreach (var quality in QualitySettings.names)
        {
            buttonObj = Instantiate(buttonsPrefab, qualityContentPanel.transform);
            Text[] texts = buttonObj.transform.GetComponentsInChildren<Text>();
            foreach (var text in texts)
            {
                text.text = quality.ToUpper();
            }
            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => QualitySettings.SetQualityLevel(i, true));
            buttonsList.Add(button);

            //set function to on selected by the button EventTrigger
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            EventTrigger buttonTrigger = button.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Select;
            entry.callback.AddListener(x => SnapScroolViewToSelected(buttonRect, qualityContentPanel, qualityScroolRect));
            buttonTrigger.triggers.Add(entry);

            i++;
        }

        SetButtonsNavigation(buttonsList);
    }

    private static void SetButtonsNavigation(List<Button> buttonsList)
    {
        Navigation nav = new Navigation() { mode = Navigation.Mode.Explicit };
        for (int i = 0; i < buttonsList.Count; i++)
        {
            if (i == 0)
                nav.selectOnUp = buttonsList[buttonsList.Count - 1];
            else
                nav.selectOnUp = buttonsList[i - 1];

            if (i == buttonsList.Count - 1)
                nav.selectOnDown = buttonsList[0];
            else
                nav.selectOnDown = buttonsList[i + 1];

            buttonsList[i].navigation = nav;
        }
    }

    private void SnapScroolViewToSelected(RectTransform target, RectTransform contentPanel, ScrollRect scrollRect)
    {
        if (Input.GetAxis("Vertical") == 0)
            return;

        contentPanel.anchoredPosition =
            (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
            - (Vector2)scrollRect.transform.InverseTransformPoint(target.position) - new Vector2(0, scrollRect.viewport.rect.height / 2); // - new Vector2(0, target.rect.height);
    }
}
