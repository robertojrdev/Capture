using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeatherControl : MonoBehaviour
{
    public static WeatherControl instance;

    [SerializeField] private Light sun;
    [SerializeField] private Slider intensitySlider;
    [SerializeField] private Text intensityValue;

    [SerializeField] private Slider xSlider;
    [SerializeField] private Text xValue;
    [SerializeField] private Slider ySlider;
    [SerializeField] private Text yValue;
    [SerializeField] private Slider zSlider;
    [SerializeField] private Text zValue;
    
    [SerializeField] private Slider rSlider;
    [SerializeField] private Text rValue;
    [SerializeField] private Slider gSlider;
    [SerializeField] private Text gValue;
    [SerializeField] private Slider bSlider;
    [SerializeField] private Text bValue;

    [SerializeField] private Button resetButton;

    private Vector3 sunEuler;
    private Color sunColor;
    private float sunIntensity;

    //used to reset values after the visualization mode
    public static Vector3 atualSunEuler { get; private set; }
    public static Color atualSunColor { get; private set; }
    public static float atualSunIntensity { get; private set; }
    public static bool IsInVisualizationMode { get; private set; }

    private Vector3 originalEuler;
    private Color originalColor;
    private float originalIntensity;

    public enum Proprierty
    {
        x, y, z, intensity, r, g, b
    }

    public enum VectorProperty
    {
        euler, color
    }

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
        if(!sun)
            sun = RenderSettings.sun;

        originalIntensity = sun.intensity;
        originalColor = sun.color;
        originalEuler = sun.transform.eulerAngles;

        resetButton.onClick.AddListener(() => ResetToOriginalValues());

        intensitySlider.maxValue = 5;
        xSlider.maxValue = 360;
        ySlider.maxValue = 360;
        zSlider.maxValue = 360;
        rSlider.maxValue = 1;
        gSlider.maxValue = 1;
        bSlider.maxValue = 1;

        intensitySlider.minValue = 0;
        xSlider.minValue = -90;
        ySlider.minValue = 0;
        zSlider.minValue = 0;
        rSlider.minValue = 0;
        gSlider.minValue = 0;
        bSlider.minValue = 0;

        intensitySlider.value = atualSunIntensity = sun.intensity;
        atualSunEuler = sun.transform.eulerAngles;
        atualSunColor = sun.color;

        xSlider.value = sun.transform.eulerAngles.x;
        ySlider.value = sun.transform.eulerAngles.y;
        zSlider.value = sun.transform.eulerAngles.z;
        rSlider.value = sun.color.r;
        gSlider.value = sun.color.g;
        bSlider.value = sun.color.b;

        intensityValue.text = Math.Round(intensitySlider.value, 2).ToString();
        xValue.text = Math.Round(xSlider.value, 2).ToString();
        yValue.text = Math.Round(ySlider.value, 2).ToString();
        zValue.text = Math.Round(zSlider.value, 2).ToString();
        rValue.text = Math.Round(rSlider.value, 2).ToString();
        gValue.text = Math.Round(gSlider.value, 2).ToString();
        bValue.text = Math.Round(bSlider.value, 2).ToString();

        intensitySlider.onValueChanged.AddListener(x => ChangeSunPropriertys(x, Proprierty.intensity));
        xSlider.onValueChanged.AddListener(x => ChangeSunPropriertys(x, Proprierty.x));
        ySlider.onValueChanged.AddListener(y => ChangeSunPropriertys(y, Proprierty.y));
        zSlider.onValueChanged.AddListener(z => ChangeSunPropriertys(z, Proprierty.z));
        rSlider.onValueChanged.AddListener(r => ChangeSunPropriertys(r, Proprierty.r));
        gSlider.onValueChanged.AddListener(g => ChangeSunPropriertys(g, Proprierty.g));
        bSlider.onValueChanged.AddListener(b => ChangeSunPropriertys(b, Proprierty.b));

        sunEuler = sun.transform.eulerAngles;
        sunColor = sun.color;
    }

    public static void ChangeSunPropriertys(float value, Proprierty proprierty)
    {
        if (!instance)
            return;

        switch (proprierty)
        {
            case Proprierty.x:
                instance.sunEuler.x = value;
                instance.xValue.text = Math.Round(value, 2).ToString();
                instance.sun.transform.eulerAngles = instance.sunEuler;
                break;
            case Proprierty.y:
                instance.sunEuler.y = value;
                instance.yValue.text = Math.Round(value, 2).ToString();
                instance.sun.transform.eulerAngles = instance.sunEuler;
                break;
            case Proprierty.z:
                instance.sunEuler.z = value;
                instance.zValue.text = Math.Round(value, 2).ToString();
                instance.sun.transform.eulerAngles = instance.sunEuler;
                break;
            case Proprierty.intensity:
                instance.intensitySlider.value = value;
                instance.sun.intensity = value;
                instance.intensityValue.text = Math.Round(value, 2).ToString();
                instance.sunIntensity = value;
                break;
            case Proprierty.r:
                instance.sunColor.r = value;
                instance.rValue.text = Math.Round(value, 2).ToString();
                instance.sun.color = instance.sunColor;
                break;
            case Proprierty.g:
                instance.sunColor.g = value;
                instance.gValue.text = Math.Round(value, 2).ToString();
                instance.sun.color = instance.sunColor;
                break;
            case Proprierty.b:
                instance.sunColor.b = value;
                instance.bValue.text = Math.Round(value, 2).ToString();
                instance.sun.color = instance.sunColor;
                break;
            default:
                break;
        }

        if(!IsInVisualizationMode)
        {
            atualSunEuler = instance.sunEuler;
            atualSunColor = instance.sunColor;
            atualSunIntensity = instance.sunIntensity;
        }
    }

    public static void ChangeSunPropriertys(Vector3 value, VectorProperty proprierty)
    {
        if (!instance)
            return;

        switch (proprierty)
        {
            case VectorProperty.euler:
                instance.sunEuler = value;
                instance.xSlider.value = value.x;
                instance.ySlider.value = value.y;
                instance.zSlider.value = value.z;

                instance.xValue.text = Math.Round(value.x, 2).ToString();
                instance.yValue.text = Math.Round(value.y, 2).ToString();
                instance.zValue.text = Math.Round(value.z, 2).ToString();

                instance.sun.transform.eulerAngles = instance.sunEuler;
                break;
            case VectorProperty.color:
                instance.sunColor = new Vector4(value.x, value.y, value.z, 1);
                instance.rSlider.value = value.x;
                instance.gSlider.value = value.y;
                instance.bSlider.value = value.z;

                instance.rValue.text = Math.Round(value.x, 2).ToString();
                instance.gValue.text = Math.Round(value.y, 2).ToString();
                instance.bValue.text = Math.Round(value.z, 2).ToString();

                instance.sun.color = instance.sunColor;
                break;
            default:
                break;
        }

        if (!IsInVisualizationMode)
        {
            atualSunEuler = instance.sunEuler;
            atualSunColor = instance.sunColor;
        }
    }

    public static void SetVisualizationMode(bool visualization)
    {
        if (!instance)
            return;

        if (!visualization)
            instance.ResetVisualization();
        IsInVisualizationMode = visualization;

        Debug.Log("Weather in visualizatrion = " + visualization);
    }

    private void ResetToOriginalValues()
    {
        ChangeSunPropriertys(new Vector3(originalColor.r, originalColor.g, originalColor.b), VectorProperty.color);
        ChangeSunPropriertys(originalEuler, VectorProperty.euler);

        ChangeSunPropriertys(originalIntensity, Proprierty.intensity);
        intensitySlider.value = originalIntensity;
        intensityValue.text = intensitySlider.value.ToString();
    }

    private void ResetVisualization()
    {
        ChangeSunPropriertys(atualSunColor.r, Proprierty.r);
        ChangeSunPropriertys(atualSunColor.g, Proprierty.g);
        ChangeSunPropriertys(atualSunColor.b, Proprierty.b);

        ChangeSunPropriertys(atualSunEuler.x, Proprierty.x);
        ChangeSunPropriertys(atualSunEuler.y, Proprierty.y);
        ChangeSunPropriertys(atualSunEuler.z, Proprierty.z);

        ChangeSunPropriertys(atualSunIntensity, Proprierty.intensity);

    }
}
