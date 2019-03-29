using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour 
{
	private static WeatherManager instance;

	[SerializeField] private Light sun;

	private Coroutine changeWeatherRoutine = null;

	void Awake()
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

	void Start()
	{
		if(!sun)
			sun = RenderSettings.sun;
	}

	public static void OnEnterRegion(WeatherRegion region)
	{
		if(!instance)
			return;

		if(instance.changeWeatherRoutine != null)
		{
			instance.StopCoroutine(instance.changeWeatherRoutine);
		}

		instance.changeWeatherRoutine = instance.StartCoroutine(instance.ChangeWeather(region));

	}

	IEnumerator ChangeWeather(WeatherRegion region)
	{
		float delta = 0;
		Quaternion initialRotation = sun.transform.rotation;
		Quaternion nextRotation = Quaternion.Euler(region.rotation);
		while(delta <= 1)
		{
			sun.transform.rotation = Quaternion.Lerp(initialRotation, nextRotation, delta);
 			sun.color = Color.Lerp(sun.color, region.color, delta);
			sun.intensity = Mathf.Lerp(sun.intensity, region.intensity, delta);

			delta += Time.deltaTime / region.changeSpeed;
			yield return null;
		}

		changeWeatherRoutine = null;
	}

}
