using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public static float pixelToUnits = 1f;
    [SerializeField]
    private float cameraSpeed = 1f;
    [SerializeField]
    public static float scale = 1f;
    [SerializeField]
    float minZoom = 100f;
    [SerializeField]
    float maxZoom = 175f;
    [SerializeField]
    float mouseZoomMultiplier = 3.0f;


    Camera theCam;

    //static Vector3 oldMousePosition;

    public Vector2 nativeResolution = Vector2.zero;

    void Awake()
    {

        theCam = GetComponent<Camera>();
        var pos = theCam.transform.position;
        pos.x = GameManager.Instance.mapHeight*(GameManager.Instance.tileSize.x / 2);
        pos.y = GameManager.Instance.mapHeight*(GameManager.Instance.tileSize.y / 2);
        theCam.transform.position = pos;
        if (theCam.orthographic)
        {

            var dir = Screen.height;
            var res = nativeResolution.y;

            scale = dir / res;
            pixelToUnits *= scale;

            theCam.orthographicSize = (dir / 2.0f) / pixelToUnits;

        }

    }

    private void FixedUpdate()
    {
        theCam.orthographicSize = trimZoom(theCam.orthographicSize + (-1* mouseZoomMultiplier* Input.mouseScrollDelta.y));
        var currentPos = transform.position;
        Vector2 moveVec = new Vector2(0,0);

        if (Input.GetKey(KeyCode.A))
        {
            moveVec.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveVec.x += 1;
        }
        if (Input.GetKey(KeyCode.W))
        {
            moveVec.y += 1;
        }
        if (Input.GetKey(KeyCode.S)) 
        {
            moveVec.y -= 1;
        }
        moveVec.Normalize();
        moveVec *= cameraSpeed*Time.deltaTime;

        transform.position = currentPos + new Vector3(moveVec.x, moveVec.y,0);
    }

    public float trimZoom(float num)
    {
        if (num > maxZoom)
        {
            num = maxZoom;
        } else if (num < minZoom)
        {
            num = minZoom;
        }
        return num;
    }
}
