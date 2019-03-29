using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Paths;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

public class PathManager : MonoBehaviour
{
    public static PathManager instance;

    [SerializeField] private Camera cam;
    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject pathList;
    [SerializeField] private GameObject pointsList;
    [SerializeField] private GameObject pathListHolder;
    [SerializeField] private GameObject pointsListHolder;
    [SerializeField] private GameObject pathListItemPrefab;
    [SerializeField] private GameObject pointsListItemPrefab;
    [SerializeField] private Button     pathVisualizationButton;
    [SerializeField] private Button     pointVisualizationButton;

    [Header("Capture")]
    [SerializeField] private string renderFolder = "ScreenshotFolder";
    [SerializeField] private int frameRate = 25;
    [SerializeField] private GameObject renderUI;
    [SerializeField] private InputField  renderPathInput;
    [SerializeField] private InputField  renderPathResolution;
    [SerializeField] private Button renderStartButton;
    [SerializeField] private Button renderCancelButton;
    [SerializeField] private GameObject[] ObjectsToHideInRender;

    [Header("Buttons")]
    [SerializeField] private Button addPathButton;

    private List<Path> paths = new List<Path>();
    private List<ListItem> listItem_paths = new List<ListItem>();
    private Dictionary<GameObject, ListItem> listItem_points = new Dictionary<GameObject, ListItem>();
    private int selectedPathIndex = -1;  //none
    private int selectedPointIndex = -1; //none
    public static bool IsActive { get; private set; }
    private Coroutine visualizationRountine;

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
        renderCancelButton.onClick.AddListener(() => renderUI.SetActive(false));
        renderUI.SetActive(false);

        string s = Application.persistentDataPath;
        s += @"/" + renderFolder;
        renderFolder = s;

        addPathButton.onClick.AddListener(() => AddPath());

        LoadPaths();
    }

    #region Path List
    public static void SetActive(bool active)
    {
        if (!instance)
            return;

        instance.selectedPathIndex = -1;
        instance.selectedPointIndex = -1;
        instance.UI.SetActive(active);
        instance.pointsList.SetActive(false);
        instance.pathList.SetActive(active);

        IsActive = active;
    }

    public void SelectPath(ListItem item)
    {
        //reset last
        if(selectedPathIndex > -1)
            listItem_paths[selectedPathIndex].IsSelectedColor(false);

        //set new
        selectedPathIndex = item.Index;
        listItem_paths[selectedPathIndex].IsSelectedColor(true);
    }

    private void EditPath()
    {
        if (selectedPathIndex <= -1)
            return;

        pathList.SetActive(false);
        pointsList.SetActive(true);
        FulfillPointList(paths[selectedPathIndex]);
    }

    private void EditPathName(ListItem item)
    {
        item.input.enabled = true;
        item.input.Select();
        item.IsSelectedColor(false);
    }

    private void SavePathName(string name, Path path, ListItem item)
    {
        path.name = name;
        item.input.text = name;
        item.input.enabled = false;
        item.IsSelectedColor(true);
    }

    private void AddPath(Path path = null)
    {
        GameObject obj = Instantiate(pathListItemPrefab, pathListHolder.transform);
        ListItem item = obj.AddComponent<ListItem>();
        listItem_paths.Add(item);
        if (path == null)
            path = new Path();
        paths.Add(path);
        obj.SetActive(true);

        //set button primary and secondary functions
        item.button = obj.GetComponent<Button>();
        item.primaryFunction = () => SelectPath(item);
        item.secondaryFunction = () => EditPathName(item);
        item.button.onClick.AddListener(() => StartCoroutine(ButtonFunction(item)));

        //finish item setup
        item.input = obj.GetComponentInChildren<InputField>();
        item.input.text = path.name;
        item.input.onEndEdit.AddListener(s => SavePathName(s, path, item));
        item.input.enabled = false;

        //selectedPathIndex = item.Index;
        SelectPath(item);
    }

    private void DeletePath()
    {
        if (selectedPathIndex <= -1)
            return;

        Destroy(listItem_paths[selectedPathIndex].gameObject);
        listItem_paths.RemoveAt(selectedPathIndex);
        paths.RemoveAt(selectedPathIndex);
        selectedPathIndex--;
    }

    public void VisualizePaths()
    {
        if (visualizationRountine != null)
        {
            StopCoroutine(visualizationRountine);
            visualizationRountine = null;
            pathVisualizationButton.GetComponentInChildren<Text>().text = "Visualize";
        }
        else
        {
            if (paths.Count > 0)
            {
                visualizationRountine = StartCoroutine(VisualizePathsRoutine(false));
                pathVisualizationButton.GetComponentInChildren<Text>().text = "Stop";
            }
        }
    }

    public void RenderPath()
    {
        if (selectedPathIndex <= -1)
            return;

        renderUI.SetActive(true);
        renderStartButton.onClick.RemoveAllListeners();
        renderStartButton.onClick.AddListener(() => StartCoroutine(VisualizePathsRoutine(true)));
        renderStartButton.onClick.AddListener(() => renderUI.SetActive(false));
        renderPathInput.text = renderFolder + @"/Together/p_00/";
    }

    private IEnumerator VisualizePathsRoutine(bool render)
    {
        WeatherControl.SetVisualizationMode(true);

        System.IO.DirectoryInfo info = null;
        string folder = renderPathInput.text;
        GameObject[] objectsToHide = null;
        if (render)
        {
            Time.captureFramerate = frameRate;
            info = System.IO.Directory.CreateDirectory(folder);
            UI.SetActive(false);
            Cursor.visible = false;

            objectsToHide = GetActiveObjects(ObjectsToHideInRender);
            SetObjectsActive(objectsToHide, false);
        }

        for (int i = 0; i < paths.Count; i++)
        {
            SelectPath(listItem_paths[i]);
            int pointsCount = paths[i].Points.Count;

            if (pointsCount <= 1) // loop below not end with just one point
                continue;

            for (int j = 0; j < pointsCount - 1; j++)
            {
                PathPoint atualPoint = paths[i].Points[j];
                PathPoint nextPoint = paths[i].Points[j + 1];
                Debug.Log("Point: '" + atualPoint.name + "' started");
                float distance = Vector3.Distance(atualPoint.position, nextPoint.position);

                Vector3 sunEuler = atualPoint.sunEuler;
                Color sunColor = atualPoint.sunColor;
                float sunIntensity = atualPoint.sunIntensity;

                float delta = 0;
                bool first = true; //keep delta 0 on the first interaction
                while (delta <= 1)
                {
                    if (!first)
                        delta += Time.deltaTime * (atualPoint.speed / distance); //if delta is calculated in the end of interaciton it will not run last frame
                    else
                        first = false;

                    cam.transform.position = Vector3.Lerp(atualPoint.position, nextPoint.position, delta);
                    cam.transform.rotation = Quaternion.Lerp(atualPoint.rotation, nextPoint.rotation, delta);

                    sunEuler = Vector3.Lerp(atualPoint.sunEuler, nextPoint.sunEuler, delta);
                    sunColor = Color.Lerp(atualPoint.sunColor, nextPoint.sunColor, delta);
                    sunIntensity = Mathf.Lerp(atualPoint.sunIntensity, nextPoint.sunIntensity, delta);

                    WeatherControl.ChangeSunPropriertys(sunEuler, WeatherControl.VectorProperty.euler);
                    WeatherControl.ChangeSunPropriertys(new Vector3(sunColor.r, sunColor.g, sunColor.b), WeatherControl.VectorProperty.color);
                    WeatherControl.ChangeSunPropriertys(sunIntensity, WeatherControl.Proprierty.intensity);

                    if (render)
                    {
                        try
                        {
                            string name = string.Format("{0}/{1:D04} shot.png", folder, Time.frameCount);
                            ScreenCapture.CaptureScreenshot(name, int.Parse(renderPathResolution.text));
                        }
                        catch { Debug.LogError("Error"); }
                    }

                    //cancel
                    if (Input.GetKeyDown(KeyCode.Space))
                        break;
                    yield return null;
                }
            }
        }
        yield return null;

        WeatherControl.SetVisualizationMode(false);

        if (render)
        {
            Time.captureFramerate = 0;
            UI.SetActive(true);
            Cursor.visible = true;
            Debug.Log("Images stored at: " + info.FullName);
            SetObjectsActive(objectsToHide, true);
            //show render info and open folder
            try
            {
                string path = info.FullName;
                path = path.Replace(@"/", @"\");
                System.Diagnostics.Process.Start("explorer.exe", "/select," + path);
            }
            catch { }
        }

        visualizationRountine = null;
        pathVisualizationButton.GetComponentInChildren<Text>().text = "Visualize";
    }

    private void LoadPaths()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string dataPath = Application.dataPath + @"/Resources/Paths/Path_scene_" + sceneName + ".pth";

        if (System.IO.File.Exists(dataPath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.FileStream file = System.IO.File.Open(dataPath, System.IO.FileMode.Open);
            List<Path> data = bf.Deserialize(file) as List<Path>;
            file.Close();

            for (int i = 0; i < data.Count; i++)
            {
                AddPath(data[i]);
            }
        }
    }              //TODO

    private void SavePaths()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string dataPath = Application.dataPath + @"/Resources/Paths/Path_scene_" + sceneName + ".pth";
        BinaryFormatter bf = new BinaryFormatter();
        System.IO.FileStream file = System.IO.File.Open(dataPath, System.IO.FileMode.OpenOrCreate);

        List<Path> data = new List<Path>(paths);
        bf.Serialize(file, data);

        file.Close();
    }              //TODO
    #endregion

    private IEnumerator ButtonFunction(ListItem item)
    {
        item.primaryFunction.Invoke();
        item.button.onClick.RemoveAllListeners();
        item.button.onClick.AddListener(item.secondaryFunction);
        yield return new WaitForSeconds(0.5f);
        item.button.onClick.RemoveAllListeners();
        item.button.onClick.AddListener(() => StartCoroutine(ButtonFunction(item)));
    }

    private GameObject[] GetActiveObjects(GameObject[] objs)
    {
        List<GameObject> active = new List<GameObject>();
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].activeSelf)
            {
                active.Add(objs[i]);
            }
        }
        return active.ToArray();
    } 

    private void SetObjectsActive(GameObject[] objs, bool active)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            objs[i].SetActive(active);
        }
    }

    public void SetValueToIfEmpty(InputField input) //used in renderPathResolution in inspector
    {
        if(input.text == "")
        {
            input.text = "1";
        }
    }

    #region Path Editor                     
    private void SelectPoint(ListItem item)
    {        
        //reset last
        if (selectedPointIndex > -1)
            listItem_points.ElementAt(selectedPointIndex).Value.IsSelectedColor(false);

        //set new
        selectedPointIndex = item.ParentIndex;
        listItem_points.ElementAt(selectedPointIndex).Value.IsSelectedColor(true);
        Debug.Log("Selected point: " + selectedPointIndex);
    }

    public void VisualizePoints()
    {
        if (visualizationRountine != null)
        {
            StopCoroutine(visualizationRountine);
            visualizationRountine = null;
            pointVisualizationButton.GetComponentInChildren<Text>().text = "Visualize";
        }
        else
        {
            if (selectedPathIndex >= 0 && paths[selectedPathIndex].Points.Count > 0)
            {
                visualizationRountine = StartCoroutine(VisualizePointsRoutine(false));
                pointVisualizationButton.GetComponentInChildren<Text>().text = "Stop";
            }
        }
    }

    public void RenderPoints()
    {
        if (selectedPointIndex <= -1 || selectedPathIndex <= -1)
            return;

        renderUI.SetActive(true);
        renderStartButton.onClick.RemoveAllListeners();
        renderStartButton.onClick.AddListener(() => StartCoroutine(VisualizePointsRoutine(true)));
        renderStartButton.onClick.AddListener(() => renderUI.SetActive(false));
        renderPathInput.text = renderFolder + @"/Individual/path_" + selectedPathIndex + @"/p_00/";
    }

    private IEnumerator VisualizePointsRoutine(bool render)
    {
        WeatherControl.SetVisualizationMode(true);

        System.IO.DirectoryInfo info = null;
        string folder = renderPathInput.text;
        GameObject[] objectsToHide = null;
        if (render)
        {
            Time.captureFramerate = frameRate;
            info = System.IO.Directory.CreateDirectory(folder);
            UI.SetActive(false);
            Cursor.visible = false;

            objectsToHide = GetActiveObjects(ObjectsToHideInRender);
            SetObjectsActive(objectsToHide, false);
        }

        Path path = paths[selectedPathIndex];
        int pointsCount = path.Points.Count;

        for (int i = 0; i < pointsCount - 1; i++)
        {
            PathPoint atualPoint = path.Points[i];
            PathPoint nextPoint = path.Points[i + 1];
            SelectPoint(listItem_points.ElementAt(i).Value);
            Debug.Log("Point: '" + atualPoint.name + "' started");
            float distance = Vector3.Distance(atualPoint.position, nextPoint.position);

            Vector3 sunEuler = atualPoint.sunEuler;
            Color sunColor = atualPoint.sunColor;
            float sunIntensity = atualPoint.sunIntensity;

            float delta = 0;
            bool first = true; //keep delta 0 on the first interaction
            while (delta <= 1)
            {
                if (!first)
                    delta += Time.deltaTime * (atualPoint.speed / distance); //if delta is calculated in the end of interaciton it will not run last frame
                else
                    first = false;

                cam.transform.position = Vector3.Lerp(atualPoint.position, nextPoint.position, delta);
                cam.transform.rotation = Quaternion.Lerp(atualPoint.rotation, nextPoint.rotation, delta);

                sunEuler = Vector3.Lerp(atualPoint.sunEuler, nextPoint.sunEuler, delta);
                sunColor = Color.Lerp(atualPoint.sunColor, nextPoint.sunColor, delta);
                sunIntensity = Mathf.Lerp(atualPoint.sunIntensity, nextPoint.sunIntensity, delta);

                WeatherControl.ChangeSunPropriertys(sunEuler, WeatherControl.VectorProperty.euler);
                WeatherControl.ChangeSunPropriertys(new Vector3(sunColor.r, sunColor.g, sunColor.b), WeatherControl.VectorProperty.color);
                WeatherControl.ChangeSunPropriertys(sunIntensity, WeatherControl.Proprierty.intensity);


                if (render)
                {
                    string name = string.Format("{0}/{1:D04} shot.png", folder, Time.frameCount);
                    ScreenCapture.CaptureScreenshot(name, int.Parse(renderPathResolution.text));
                }

                //cancel
                if (Input.GetKeyDown(KeyCode.Space))
                    break;

                yield return null;
                Debug.Log("delta - " + delta);
            }
        }

        SelectPoint(listItem_points.ElementAt(pointsCount - 1).Value);
        WeatherControl.SetVisualizationMode(false);

        if (render)
        {
            Time.captureFramerate = 0;
            UI.SetActive(true);
            Cursor.visible = true;
            Debug.Log("Images stored at: " + info.FullName);
            SetObjectsActive(objectsToHide, true);
            //show render info and open folder
            try
            {
                string filePath = info.FullName;
                filePath = filePath.Replace(@"/", @"\");
                System.Diagnostics.Process.Start("explorer.exe", "/select," + filePath);
            }
            catch { }
        }

        visualizationRountine = null;
        pointVisualizationButton.GetComponentInChildren<Text>().text = "Visualize";
    }

    public void DeletePoint()
    {
        if (selectedPointIndex <= -1 || selectedPathIndex <= -1)
            return;

        Destroy(listItem_points.Keys.ElementAt(selectedPointIndex));
        Debug.Log("Removed: " + listItem_points.Remove(listItem_points.Keys.ElementAt(selectedPointIndex)));
        paths[selectedPathIndex].RemovePoint(selectedPointIndex);
        selectedPointIndex--;
    }

    public void AddPoint()
    {
        if (selectedPathIndex <= -1)
            return;

        PathPoint point = new PathPoint
        (
            cam.transform.position,
            cam.transform.rotation,
            paths[selectedPathIndex].LastPointSpeed,
            cam.fieldOfView,
            "Point - " + paths[selectedPathIndex].PointsNameCount
        );

        point.SetWeather(WeatherControl.atualSunEuler, WeatherControl.atualSunColor, WeatherControl.atualSunIntensity);

        AddPointToList(point);
        paths[selectedPathIndex].AddPoint(point);
    }

    private void AddPointToList(PathPoint point)    
    {
        GameObject obj = Instantiate(pointsListItemPrefab, pointsListHolder.transform);

        GameObject objName = obj.transform.GetChild(0).gameObject;
        GameObject objSpeed = obj.transform.GetChild(1).gameObject;

        ListItem itemName = objName.AddComponent<ListItem>();
        ListItem itemSpeed = objSpeed.AddComponent<ListItem>();

        listItem_points.Add(obj, itemName);
        obj.SetActive(true);

        Debug.Log(point.name);

        //set name button primary and secondary functions
        itemName.button = objName.GetComponent<Button>();
        itemName.primaryFunction = () => SelectPoint(itemName);
        itemName.secondaryFunction = () => EditPointName(itemName);
        itemName.button.onClick.AddListener(() => StartCoroutine(ButtonFunction(itemName)));

        //finish name item setup
        itemName.input = objName.GetComponentInChildren<InputField>();
        itemName.input.text = point.name;
        itemName.input.onEndEdit.AddListener(s => SavePointName(s, point, itemName));
        itemName.input.enabled = false;

        //set speed button primary and secondary functions
        itemSpeed.button = objSpeed.GetComponent<Button>();
        itemSpeed.primaryFunction = () => SelectPoint(itemSpeed);
        itemSpeed.secondaryFunction = () => EditPointSpeed(itemSpeed);
        itemSpeed.button.onClick.AddListener(() => StartCoroutine(ButtonFunction(itemSpeed)));

        //finish speed item setup
        itemSpeed.input = objSpeed.GetComponentInChildren<InputField>();
        itemSpeed.input.enabled = false;
        itemSpeed.input.contentType = InputField.ContentType.DecimalNumber;
        itemSpeed.inputTag = "Speed: ";
        itemSpeed.input.text = itemSpeed.inputTag + point.speed;
        Debug.Log("point speed: " + point.speed);
        itemSpeed.input.onEndEdit.AddListener(s => SavePointSpeed(s, point, itemSpeed));

        selectedPointIndex = itemName.ParentIndex;
    }

    public void UpdatePoint()
    {
        if (selectedPointIndex <= -1 || selectedPathIndex <= -1)
            return;

        Debug.Log("Update point");

        PathPoint point = paths[selectedPathIndex].Points[selectedPointIndex];

        point.SetWeather(WeatherControl.atualSunEuler, WeatherControl.atualSunColor, WeatherControl.atualSunIntensity);
        point.position = cam.transform.position;
        point.rotation = cam.transform.rotation;
    }

    public void GoToPoint()
    {
        if (selectedPointIndex <= -1 || selectedPathIndex <= -1)
            return;

        PathPoint point = paths[selectedPathIndex].Points[selectedPointIndex];

        WeatherControl.SetVisualizationMode(true);
        WeatherControl.ChangeSunPropriertys(new Vector3(point.sunColor.r, point.sunColor.g, point.sunColor.b), WeatherControl.VectorProperty.color);
        WeatherControl.ChangeSunPropriertys(point.sunEuler, WeatherControl.VectorProperty.euler);
        WeatherControl.ChangeSunPropriertys(point.sunIntensity, WeatherControl.Proprierty.intensity);

        Debug.Log("Go to");
        cam.transform.position = point.position;
        cam.transform.rotation = point.rotation;

        WeatherControl.SetVisualizationMode(false);
    }

    public void BackToPathList()
    {
        if (visualizationRountine != null)
        {
            StopCoroutine(visualizationRountine);
            visualizationRountine = null;
            pointVisualizationButton.GetComponentInChildren<Text>().text = "Visualize";
        }

        pathList.SetActive(true);
        pointsList.SetActive(false);
    }

    private void EditPointName(ListItem item)
    {
        item.input.enabled = true;
        item.input.Select();
        item.IsSelectedColor(false);
    }

    private void SavePointName(string name, PathPoint point, ListItem item)
    {
        point.name = name;
        item.input.text = item.inputTag + name;
        item.input.enabled = false;
        item.IsSelectedColor(true);
        Debug.Log("Nome salvo");
    }

    private void EditPointSpeed(ListItem item)
    {
        string s = item.input.text;
        s = s.Remove(0, item.inputTag.Length);
        item.input.text = s;
        item.input.enabled = true;
        item.input.Select();
    }

    private void SavePointSpeed(string speed, PathPoint point, ListItem item)
    {
        item.input.enabled = false;
        point.speed = float.Parse(speed);
        item.input.text = item.inputTag + speed;
        Debug.Log("saved point speed: " + point.speed);
    }

    private void FulfillPointList(Path selectedPath)
    {
        //reset list
        int listItemsAmount = pointsListHolder.transform.childCount -1;
        listItem_points = new Dictionary<GameObject, ListItem>();
        for (int i = listItemsAmount; i > 0; i--)
        {
            Destroy(pointsListHolder.transform.GetChild(i).gameObject);
        }

        //fulfill
        for (int i = 0; i < selectedPath.Points.Count; i++)
        {
            AddPointToList(selectedPath.Points[i]);
        }

        Debug.Log("count - " + selectedPath.Points.Count);

        selectedPointIndex = -1;
    }
    #endregion

    private void OnDestroy()
    {
        SavePaths();
    }
}

public class ListItem : MonoBehaviour
{
    public InputField input;
    public Button button;
    public UnityEngine.Events.UnityAction primaryFunction;
    public UnityEngine.Events.UnityAction secondaryFunction;
    public string inputTag = "";
    public Color originalButtonColor = Color.white;
    public Color selectedButtonColor = (Color.grey + Color.white) / 1.75f;

    private Image buttonImage;

    public int Index
    {
        get
        {
            return transform.GetSiblingIndex() - 1;
        }
    }
    public int ParentIndex
    {
        get
        {
            return transform.parent.GetSiblingIndex() - 1;
        }
    }
    public void IsSelectedColor(bool selected)
    {
        if (!buttonImage)
            return;

        if(selected)
        {
            buttonImage.color = selectedButtonColor;
        }
        else
        {
            buttonImage.color = originalButtonColor;
        }
    }

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
    }
}
