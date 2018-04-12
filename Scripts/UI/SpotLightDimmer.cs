using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotLightDimmer : MonoBehaviour {
    Light myLight;
    [SerializeField]
    [Range(1.0f, 8.0f)]
    private float maxIntensity = 7.0f;
	// Use this for initialization
	void Awake () {
        myLight = GetComponent<Light>();
	}
	
	// Update is called once per frame
	void Update () {
        myLight.intensity = maxIntensity * (1- TimeOfDay.Instance.getLightPercent());
	}
}
