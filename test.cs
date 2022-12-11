using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class test : MonoBehaviour {

    //public Canvas m_Canvas;
    public Image m_image;
    float a = 0.05f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //1.get data 
        //2.scalinginput.
        if (Input.GetKey(KeyCode.A))
        { m_image.transform.localScale = m_image.transform.localScale + new Vector3(a, a, 0); }
        if (Input.GetKey(KeyCode.B))
        { m_image.transform.localPosition = m_image.transform.localPosition + new Vector3(100*a, 0, 0); }

        //Debug.Log(s.x +" "+s.y);
    }
}
