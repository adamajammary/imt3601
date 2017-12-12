using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {

	public float power = 0.7f;
	public float duration = 1.0f;
	public Transform camera;
	public float slowDownAmount = 1.0f;
	public bool isShaking = false;

	Vector3 startPosition;
	float initialDuration;

	// Use this for initialization
	void Start () {
		startPosition = camera.localPosition;
		initialDuration = duration;	
	}
	
	// Update is called once per frame
	void Update () {
		
		
	}


	void LateUpdate () {
		if (isShaking) {
			if (duration > 0) {
				camera.localPosition = startPosition + Random.insideUnitSphere * power;
				duration -= Time.deltaTime * slowDownAmount;
			}
			else
			{
				isShaking = false;
				duration = initialDuration;
				camera.localPosition = startPosition;
			}
		}
	}


}
