using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CameraUI
{
    public class ZoomSlider : MonoBehaviour
    {
        public float lineInterval;
        public float lineMaxSize;
        public float lineMinSize;
        [Tooltip("How far from the center the line will start to use minimum size")]
        [Range(0, 1)]public float rangeFromCenter = 0.5f;
        public GameObject mainHolder;
        public GameObject linesHolder;
        public GameObject linePrefab;
        public Text valueText;
        
        [HideInInspector] public float maxZoom;
        [HideInInspector] public float minZoom;
        private RectTransform mainRect;
        private RectTransform holderRect;
        private List<RectTransform> instantiatedLines = new List<RectTransform>();

        private void Awake()
        {
            mainRect = mainHolder.GetComponent<RectTransform>();
            holderRect = linesHolder.GetComponent<RectTransform>();
            InstantiateLines();
        }

        private void InstantiateLines()
        {
            int linesCount = Mathf.CeilToInt((maxZoom - minZoom) / lineInterval) + 1;
            for (int i = 0; i < linesCount; i++)
            {
                GameObject line = Instantiate(linePrefab, linesHolder.transform);
                RectTransform lineRect = line.GetComponent<RectTransform>();
                instantiatedLines.Add(lineRect);
                line.SetActive(true);
            }
        }

        public void SetZoom(float zoom)
        {
            if(!mainRect || !holderRect)
            {
                mainRect = mainHolder.GetComponent<RectTransform>();
                holderRect = linesHolder.GetComponent<RectTransform>();
            }


            float zoomDelta = (zoom - minZoom) / (maxZoom - minZoom);
            float holderPos = mainRect.position.y + zoomDelta * holderRect.rect.height;
            Vector3 newPos = holderRect.position;
            newPos.y = holderPos;
            holderRect.position = newPos;

            SetValueText(zoom);
            SetLineSize();
        }

        private void SetValueText(float zoom)
        {
            float val =(float) decimal.Round((decimal)zoom, 1);
            if(val % 1 == 0)
                valueText.text = val + ".0x";
            else
                valueText.text = val + "x";
        }

        private void SetLineSize()
        {
            float mainMiddle = mainRect.position.y;
            float mainHeight = mainRect.rect.height;
            float rangeFromCenterInPixels = (mainHeight / 2) * rangeFromCenter;

            float lineMiddle, distFromMiddle, deltaRelativeToCenter, lineSize;
            foreach (var line in instantiatedLines)
            {
                lineMiddle = line.position.y;
                distFromMiddle = Mathf.Abs(lineMiddle - mainMiddle);

                deltaRelativeToCenter = distFromMiddle / rangeFromCenterInPixels;
                lineSize = Mathf.Lerp(lineMaxSize, lineMinSize, deltaRelativeToCenter);
                line.sizeDelta = new Vector2(line.sizeDelta.x, lineSize);
            }
        }
    }
}
