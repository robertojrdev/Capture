using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScreen : MonoBehaviour
{

    private static LoadScreen instance;

    public Image frontLoadImage;
    public Text loadText;
    public string loadIndicator;
    public float smoothSpeed = 0.5f;
    public UnityEvent onCompleteLoad;

    private float loadProgress = 0;
    private Coroutine smoothLoadBarRoutine;
    private AsyncOperation loadSceneOperation;
    private static string sceneNameToLoad = "";

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

    private void Start()
    {
        if (sceneNameToLoad != null && sceneNameToLoad != "")
            StartCoroutine(instance.StartLoadSceneAsync(sceneNameToLoad));
        else
            Debug.LogWarning("NoSceneToLoad");

        onCompleteLoad.AddListener(() => OnCompleteSceneLoad());
    }

    public static void StartLoadScene(int sceneIndex = -1, string sceneName = "", LoadSceneMode mode = LoadSceneMode.Additive) //add a function to load something or a scene in a asynchronous way
    {
        if (sceneName == "")
        {
            if (sceneIndex >= 0)
                sceneName = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
            else
                return;
        }

        SceneManager.LoadScene("Scenes/Load Scene/Load Scene", mode);
        Debug.Log("Load Scene Started: " + sceneName);

        sceneNameToLoad = sceneName;
    }

    private IEnumerator StartLoadSceneAsync(string sceneToLoad)
    {
        loadSceneOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
        loadSceneOperation.allowSceneActivation = false;

        while(loadSceneOperation.progress < .9f)
        {
            loadProgress = loadSceneOperation.progress;
            yield return null;
        }
        loadProgress = 1;
        onCompleteLoad.Invoke();
    }

    public void StartLoadBar()
    {
        smoothLoadBarRoutine = StartCoroutine(SmoothLoadBar());
    }

    private IEnumerator SmoothLoadBar()
    {
        while(true)
        {
            frontLoadImage.fillAmount = Mathf.Lerp(frontLoadImage.fillAmount, loadProgress + .1f, smoothSpeed * Time.deltaTime);
            if(loadText)
                loadText.text = Mathf.Round(frontLoadImage.fillAmount * 100) + loadIndicator;
            yield return null;
        }
    }

    private void OnCompleteSceneLoad()
    {
        Debug.Log("Load Complete");
    }

    public void ShowLoadedScene()
    {
        loadSceneOperation.allowSceneActivation = true;
    }
}
