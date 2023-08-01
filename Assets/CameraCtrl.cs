using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour {

    public Transform m_Camera;
    public Transform m_Pivot;

    public float rotate_Speed;
    bool flag = false;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            flag = true;           
        }
        if (Input.GetMouseButtonUp(0))
        {
            flag = false;
        }
        if (flag)
        {
            float h = rotate_Speed * Input.GetAxis("Mouse X");
            float v = rotate_Speed * Input.GetAxis("Mouse Y");

            m_Camera.RotateAround(m_Pivot.position, Vector3.up, h);
        }
    }
}
