using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class WeatherRegion : MonoBehaviour 
{
	public Vector3 rotation;
	public Color color;
	public float intensity;
	public float changeSpeed;

	BoxCollider coll;
	Rigidbody rb;

	void Start()
	{
		coll = GetComponent<BoxCollider>();
		rb = GetComponent<Rigidbody>();

		coll.isTrigger = true;
		rb.isKinematic = true;

		gameObject.layer = 9;
	}
	
	void OnTriggerEnter(Collider other)
	{
        if(other.gameObject.layer == 10) // player layer
		    WeatherManager.OnEnterRegion(this);
	}
}
