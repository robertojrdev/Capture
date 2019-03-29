using UnityEngine;
 using System.Collections;
 
 [RequireComponent(typeof(LineRenderer))]
 public class Line : MonoBehaviour {
 
    LineRenderer lineRenderer;
    public Transform[] transformPoints;
    private Vector3[] linePoints;
    private int pointCount = 0;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
		Debug.Log(transform.childCount);

		for (int i = 0; i < transform.childCount; i++)
		{
			transformPoints[i] = transform.GetChild(i);
		}

		DrawLine();
	}

	void DrawLine ()
	{
		pointCount = transformPoints.Length;
        linePoints = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
			if(transformPoints[i])
	        	linePoints[i] = transformPoints[i].position;
        }
		
		for (int i = 0; i < pointCount; i++)
		{
			float t = i / (float)pointCount;
			lineRenderer.positionCount = pointCount;
			lineRenderer.SetPositions(linePoints);
		} 
    }
 }