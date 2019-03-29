using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltWithCamera : MonoBehaviour
{

    [SerializeField] private bool invertRotation = false;
    public static Dictionary<TiltWithCamera, RectTransform> AllRect { get; private set; }

    private void Start()
    {
        if (AllRect == null)
            AllRect = new Dictionary<TiltWithCamera, RectTransform>();

        RectTransform rect = GetComponent<RectTransform>();
        if (!rect)
            return;

        AllRect.Add(this, rect);
    }

    public static void ResetRectsList()
    {
        AllRect = new Dictionary<TiltWithCamera, RectTransform>();
    }

    public static void RotateAll(float angle)
    {
        Vector3 euler;
        if(AllRect != null)
            foreach (var rect in AllRect)
            {
                euler = rect.Value.localEulerAngles;
                euler.z = rect.Key.invertRotation ? -angle : angle;
                rect.Value.localEulerAngles = euler;
            }
    }

}
