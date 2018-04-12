using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickReceiver : MonoBehaviour
{
    private Vector3 startMousePosition;
    public void OnMouseDown()
    {
        startMousePosition = MapManager.Instance.convertToGameGrid(
            Camera.main.ScreenToWorldPoint(Input.mousePosition));

        if (!EventSystem.current.IsPointerOverGameObject() &&
            GameManager.Instance.currentMouseMode == MouseMode.BUILD)
        {
            GameManager.Instance.startDrag(
                    MapManager.Instance.convertToGameGrid(
                        Camera.main.ScreenToWorldPoint(Input.mousePosition)));
        }
    }

    public void OnMouseUp()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (GameManager.Instance.currentMouseMode != MouseMode.BUILD)
            {
                GameManager.Instance.clickedMap(
                    MapManager.Instance.convertToGameGrid(
                        Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            } else
            {
                GameManager.Instance.endDrag(
                    MapManager.Instance.convertToGameGrid(
                        Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            }
        }
    }
}
