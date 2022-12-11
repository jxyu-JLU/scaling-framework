using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;
using System.IO;
using UnityEngine;

public class pen_data : MonoBehaviour
{
    public string username;
    [DllImport("mydll_position.dll", EntryPoint = "EnumMyHid", SetLastError = true)]
    public static extern byte EnumMyHid();

    void Start()
    {
        byte r = EnumMyHid();
        Debug.Log("r = " + r.ToString());
    }
    void OnGUI() 
    {
        GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
        myButtonStyle.fontSize = 20;
        username = GUI.TextField(new Rect(10, 50, 150, 30), username, myButtonStyle);
        GUI.Label(new Rect(10, 10, 150, 30), "实验者姓名", myButtonStyle);
    }
}