using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//Note to self
//this script requires a kinematic type rigid body on the minion prefab to work.



public class LightProximityDimmer : MonoBehaviour {
    Light theLightObject;
    List<GameObject> insides;
	// Use this for initialization
	void Awake () {
        theLightObject = transform.Find("Spotlight").gameObject.GetComponent<Light>();
        insides = new List<GameObject>();
    }
	
	// Update is called once per frame
	void Update () {
		if (insides.Count > 0)
        {
            float minvalue = 10000;
            foreach (var item in insides)
            {
                var dist = Vector2.Distance(item.transform.position, transform.position);
                if (dist < minvalue)
                    minvalue = dist;
            }
            theLightObject.intensity = (minvalue / 60f) * 8.0f;
        }
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (insides == null)
        //    insides = new List<GameObject>();
        if (!insides.Contains(collision.gameObject))
        {
            insides.Add(collision.gameObject);
        }
        var vec = collision.transform.position;
        var dist = Vector2.Distance(vec, transform.position);
        dist /= 60f;
        float newIntensity = dist * 8.0f;
        theLightObject.intensity = newIntensity;

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (insides.Contains(collision.gameObject))
        {
            insides.Remove(collision.gameObject);
        } else
        {
            Debug.Log("What?!?");
        }

        if (insides.Count==0)
            theLightObject.intensity = 7.0f;
    }
}
