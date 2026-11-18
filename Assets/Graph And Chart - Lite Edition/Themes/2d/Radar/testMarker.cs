using UnityEngine;
using System.Collections;
using ChartAndGraph;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
public class testMarker : MonoBehaviour
{
    public GameObject Place;

	// Use this for initialization
	void Start ()
    {
	}
    Vector2 GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Pointer.current != null)
            return Pointer.current.position.ReadValue();
        return new Vector2();   
#else
        return Input.mousePosition;
#endif
    }
    // Update is called once per frame
    void Update ()
    {
        var radar = GetComponent<RadarChart>();
        string group;
        double amount;
        if(radar.SnapWorldPointToPosition(GetMousePosition(), out group,out amount))
        {
            Vector3 position;
            if(radar.ItemToWorldPosition(group,amount ,out position))
            {
                Place.transform.position = position;
            }
        }
    }
}
