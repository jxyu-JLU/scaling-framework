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
    [DllImport("mydll_position.dll", EntryPoint = "EnumMyHid", SetLastError = true)]
    public static extern byte EnumMyHid();

    void Start()
    {
        byte r = EnumMyHid();
        Debug.Log("r = " + r.ToString());
    }
}