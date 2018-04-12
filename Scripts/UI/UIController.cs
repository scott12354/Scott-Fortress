using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour {
    [SerializeField]
    GameObject buildItemWindowPrefab;
	// Use this for initialization
	//void Start () {
		
	//}
	
	//// Update is called once per frame
	//void Update () {
		
	//}

    public void openBuildItemWindow()
    {
        var go = transform.Find("Build Item Window");
        if (go == null)
        {
            var gogo = Instantiate(buildItemWindowPrefab);
            gogo.transform.SetParent(transform);
        }
        //else do nothing
    }
}
