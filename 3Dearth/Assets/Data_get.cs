using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Runtime.InteropServices;
using System;
using System.IO;
using Valve.VR;



public class Data_get : MonoBehaviour {

    bool ExitThread = false;
    public bool ExitThread1 = true;
    //bool StartConvert = false;
    //bool StartSave = true;

    public byte[] DataAxis9 = new byte[66];
    public long[] DataAxis9_Length = new long[1];
    public long[] DataAxis9_ReadState = new long[1];
    public byte[] DataAudio = new byte[66];
    public long[] DataAudio_Length = new long[1];
    public long[] DataAudio_ReadState = new long[1];
    public byte[] DataUltrasound = new byte[66];
    private byte [] DataToUSB = new byte[66];
    public byte[] Audio_Data = new byte[1920000];
    public byte[] Audio_Data1 = new byte[1920000];

    public byte[] USBDATA = new byte[66];
    public long[] USBDATA_Length = new long[1];
    public long[] USBDATA_ReadState = new long[1];

    public int Audio_Data_adr = 0;
    public GameObject cube;
    public float a, A;
    public float b, B;
    public float c, C;
    public float w, W;
    public float x, y, z;
    float X, Y, Z;
    public bool Speechisrecognized = false;
    public bool IsWriting = false;
    public bool EndWriting = false;
    public int GetWriting_State;

    public bool UponRecognize = false;
    public string output;

    public bool PenPositionSimulation = false;  // simulate pen movement instead of reading from real device   (false = 2m , true = 4m)

    public bool button = false;
    public bool buttonpress = false;
    public bool triggerpress = false;
    public bool old_button = false;
    bool Noerror = true;
    bool Controllerstate = false;
    VRControllerState_t VRcontrollerstate;
    public Vector3 Va, Va_correct, Va_r;
    public Quaternion Qa, Qa_correct, Qa_relative;
    CVRSystem vrSystem;
    //public getrotation m_getrotation;
    //public Data_get m_Data_get;
    TrackedDevicePose_t[] allPoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
    int Data_state = 2;
    Vector3 tempR;
    public float pitch_mot, yaw_mot, roll_mot, x_mot, y_mot, z_mot, pitch_mot_old, yaw_mot_old, roll_mot_old, x_mot_old, y_mot_old, z_mot_old;
    public float vx, vy, vz, vpitch, vyaw, vroll;
    public void Awake()
    {
        Screen.SetResolution(1280, 1024, true);
    }

	// Use this for initialization
	void Start () 
    {
        TimeBeginPeriod(1);

        byte r = EnumMyHid();
        Debug.Log("r = " + r.ToString());
        DataToUSB[0] = 0x00;
        DataToUSB[1] = 0x80;
        DataToUSB[2] = 0x01;
        DataToUSB[3] = 0xf4;
        DataToUSB[4] = 0x3d;
        DataToUSB[5] = 0xd5;
        DataToUSB[6] = 0x20;
        WriteMydev(DataToUSB);
        //var error = EVRInitError.None;
        //IntPtr svrSystem = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Other);
        //if (error != EVRInitError.None)
        //{
        //    Debug.Log("EVRInitError.None");
        //    Noerror = false;
        //}
        //else
        //{
        //    vrSystem = new CVRSystem(svrSystem);
        //    PrintOpenVRDevices();
        //    //Thread vive = new Thread(Threadvive);
        //    //vive.Start();
        //    //Thread axis9 = new Thread(ThreadAxis9);
        //    //axis9.Start();
        //}
        Thread axis9 = new Thread(ThreadAxis9);
        axis9.Start();
        //Thread audio = new Thread(ThreadAudio);
        //audio.Start();
	}

    private void PrintOpenVRDevices()
    {
        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            var deviceClass = vrSystem.GetTrackedDeviceClass(i);
            if (deviceClass != ETrackedDeviceClass.Invalid)
            {
                //var deviceReading = OpenVR.GetDeviceReading(i);
                Debug.Log("OpenVR device at " + i + ": " + deviceClass);// + " and pos " + deviceReading);
                //if (deviceClass == ETrackedDeviceClass.Controller) m_cubecolor.s_record = true;
            }
        }
    }

    void GetvivePositionrotation(HmdMatrix34_t pose)
    {
        Vector3 m = new Vector3(0, 0, 0);

        m.x = pose.m[3];
        m.y = pose.m[7];
        m.z = -pose.m[11];

        float w = (float)Math.Sqrt(Math.Max(0, 1 + pose.m[0] + pose.m[5] + pose.m[10])) / 2;
        float x = (float)Math.Sqrt(Math.Max(0, 1 + pose.m[0] - pose.m[5] - pose.m[10])) / 2;
        float y = (float)Math.Sqrt(Math.Max(0, 1 - pose.m[0] + pose.m[5] - pose.m[10])) / 2;
        float z = (float)Math.Sqrt(Math.Max(0, 1 - pose.m[0] - pose.m[5] + pose.m[10])) / 2;
        float x1 = (float)Math.Sqrt(Math.Max(0, 1 + pose.m[0] - pose.m[5] - pose.m[10])) / 2;
        float y1 = (float)Math.Sqrt(Math.Max(0, 1 - pose.m[0] + pose.m[5] - pose.m[10])) / 2;
        _copysign(ref x, pose.m[9] - pose.m[6]);
        _copysign(ref y, pose.m[2] - pose.m[8]);
        _copysign(ref x1, -pose.m[9] - -pose.m[6]);
        _copysign(ref y1, -pose.m[2] - -pose.m[8]);
        _copysign(ref z, pose.m[4] - pose.m[1]);


        this.Va = m;
        this.Qa = new Quaternion(w, x, y, z);
        this.Qa_correct = new Quaternion(w, x1, y1, z);
        this.Va_r = QuaternionToEuler_new(Qa_correct);
    }
    private static void _copysign(ref float sizeval, float signval)
    {
        if (signval > 0 != sizeval > 0)
            sizeval = -sizeval;
    }
    public Vector3 QuaternionToEuler_new(Quaternion q)
    {
        //This is the code from 
        //http://www.mawsoft.com/blog/?p=197
        var rotation = q;
        double q0 = rotation.w;//B
        double q1 = rotation.x;//A
        double q2 = rotation.y;//C
        double q3 = rotation.z;//W
        Vector3 radAngles = new Vector3(0, 0, 0);
        radAngles.z = (float)Math.Atan2(2 * (q0 * q1 + q2 * q3), 1 - 2 * (Math.Pow(q1, 2) + Math.Pow(q2, 2))) * 180 / (float)Math.PI;
        radAngles.x = (float)Math.Asin(2 * (q0 * q2 - q3 * q1)) * 180 / (float)Math.PI;
        radAngles.y = (float)Math.Atan2(2 * (q0 * q3 + q1 * q2), 1 - 2 * (Math.Pow(q2, 2) + Math.Pow(q3, 2))) * 180 / (float)Math.PI;
        return radAngles;
    }

    // Update is called once per frame
    void Update () 
    {
        cube.transform.rotation = new Quaternion(B,A,C,   W);

        //if (UponRecognize)
        //{
        //    output = kdxf_recognizor.Recognize();
        //    Debug.Log(output);
        //    Speechisrecognized = true;
        //    UponRecognize = false;
        //}
	}
    void getpenposition() 
    {
        if (!PenPositionSimulation)
        {
            ReadMydev(DataUltrasound);
            x = BitConverter.ToInt32(DataUltrasound, 1);   //将基数据类型转换为一个字节数组以及将一个字节数组转换为基数据类型。
            y = BitConverter.ToInt32(DataUltrasound, 5);   //返回转换的字节数组中指定位置处的四个字节从 32 位有符号的整数。
            z = BitConverter.ToInt32(DataUltrasound, 9);
        }
        else
        {
            //float s = (DateTime.Now.Second % 10 + DateTime.Now.Millisecond / 1000f) / 10f;  // 0 ~ 1
            //x = 200 * Mathf.Sin(s * 2f * 3.1416f);
            //y = 200 * Mathf.Cos(s * 2f * 3.1416f);
            //z = 200 * Mathf.Sin(s * 2f * 3.1416f + 1f);
            ReadMydev(DataUltrasound, USBDATA_Length, USBDATA_ReadState);
            X = BitConverter.ToInt32(DataUltrasound, 1);   //将基数据类型转换为一个字节数组以及将一个字节数组转换为基数据类型。
            Y = BitConverter.ToInt32(DataUltrasound, 5);   //返回转换的字节数组中指定位置处的四个字节从 32 位有符号的整数。
            Z = BitConverter.ToInt32(DataUltrasound, 9);

            x = X;
            y = -Y;
            z = Z;
        }
    }
    public int getpeninputstate() 
    {
        int temp = 0;
        if (DataUltrasound[13] == 2||buttonpress) temp = 2;
        if (DataUltrasound[13] == 4&&Data_state == 2) temp = 0;

        if (DataUltrasound[13] == 3||triggerpress) temp = 3;
        if (DataUltrasound[13] == 1) temp = 1;
        return temp;
    }

    void OnDestroy()
    {
        ExitThread = true;
        OpenVR.Shutdown();
    }

    void ThreadAxis9()
    {
        while (!ExitThread)
        {
            if (Data_state == 1)
            {
                if (Noerror)
                {
                    vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, allPoses);
                    var pose = allPoses[1];
                    //Debug.Log("1");
                    if (pose.bPoseIsValid)
                    {
                        //Debug.Log("2");
                        var absTracking = pose.mDeviceToAbsoluteTracking;
                        Controllerstate = vrSystem.GetControllerState(1, ref VRcontrollerstate); //拿button的数据。看看是用线程还是啥。20220511.
                                                                                                 //Debug.Log(VRcontrollerstate.ulButtonPressed.ToString());
                        ulong trigger = VRcontrollerstate.ulButtonPressed & (1UL << ((int)EVRButtonId.k_EButton_SteamVR_Touchpad));
                        ulong trigger1 = VRcontrollerstate.ulButtonPressed & (1UL << ((int)EVRButtonId.k_EButton_SteamVR_Trigger));
                        if (trigger > 0L && !buttonpress)// VRcontrollerstate.ulButtonPressed != 0)
                        {
                            buttonpress = true;
                            //Debug.Log("buttonpress");
                            if ((old_button == false) && (buttonpress))
                            {
                                button = true;
                                //那边的程序需要响应一下。
                                //tbcc.vivebutton = true;
                                Debug.Log("button");
                            }
                            else button = false;
                            //buttonstate_vive.Text = "已按下";
                            //ClickedEventArgs e;
                            //e.controllerIndex = controllerIndex;
                            //e.flags = (uint)controllerState.ulButtonPressed;
                            //e.padX = controllerState.rAxis0.x;
                            //e.padY = controllerState.rAxis0.y;
                            //OnTriggerClicked(e);
                            ////https://blog.csdn.net/wo2nihehe2/article/details/61197659
                        }
                        else if (trigger == 0L && buttonpress)
                        {
                            buttonpress = false;
                            //buttonstate_vive.Text = "未按下";
                            //Debug.Log("no-buttonpress");
                            button = false;
                        }
                        if (trigger1 > 0L && !triggerpress)
                        {
                            triggerpress = true;
                        }
                        else if (trigger1 == 0L && triggerpress)
                        {
                            triggerpress = false;
                        }
                        GetvivePositionrotation(absTracking);
                        //var mat = new SteamVR_Utils.RigidTransform(absTracking);
                        //Debug.Log(mat.pos + " " + mat.rot);
                        //Va = mat.pos;
                        //Qa = mat.rot;
                        x_mot = -Va.x * 100;
                        y_mot = Va.y * 100;
                        z_mot = -Va.z * 100;
                        Quaternion Qb = Quaternion.Euler(Va_r.x, Va_r.y - 180, Va_r.z);
                        //Vector3 v3a = Qb.eulerAngles;
                        Vector3 v3a = Qa.eulerAngles;
                        Vector3 v3a1 = Qa_correct.eulerAngles;
                        //pitch_mot = Va_r.x;
                        //yaw_mot = 180 - Va_r.y;
                        //roll_mot = -Va_r.z;
                        yaw_mot = v3a.z - 180;
                        pitch_mot = v3a.y;
                        roll_mot = -Va_r.z;
                        //roll_mot = v3a.x;
                        //pitch_mot = v3a.x;
                        //yaw_mot = v3a.y;
                        //roll_mot = v3a.z;
                        //if (pitch_mot > 0) pitch_mot = 180 - pitch_mot;
                        //else if (pitch_mot < 0) pitch_mot = -180 - pitch_mot;
                        if ((pitch_mot > 180) && (pitch_mot < 360)) pitch_mot -= 360;//防止出现0°和360°的问题。
                        if ((yaw_mot > 180) && (yaw_mot < 360)) yaw_mot -= 360;
                        if ((roll_mot > 180) && (roll_mot < 360)) roll_mot -= 360;

                        //roll_mot = Va_r.z;
                        old_button = buttonpress;
                    }
                }
            }
            //pen数据获取
            if (Data_state == 2)
            {
                getpenposition();
                Read9AxisData(DataAxis9, DataAxis9_Length, DataAxis9_ReadState);

                a = BitConverter.ToInt32(DataAxis9, 29);//Byte29:q0的0-7位  Byte30:q0的8-15位  Byte31:q0的16-23位  Byte32:q0的24-31位
                b = BitConverter.ToInt32(DataAxis9, 33);//Byte33:q1的0-7位  Byte34:q1的8-15位  Byte35:q1的16-23位  Byte36:q1的24-31位
                c = BitConverter.ToInt32(DataAxis9, 37);//Byte37:q2的0-7位  Byte38:q2的8-15位  Byte39:q2的16-23位  Byte40:q2的24-31位
                w = BitConverter.ToInt32(DataAxis9, 41);//Byte41:q3的0-7位  Byte42:q3的8-15位  Byte43:q3的16-23位  Byte44:q3的24-31位

                //Debug.Log(DataAxis9);
                A = a / 1073741824;
                B = b / 1073741824;
                C = c / 1073741824;
                W = w / 1073741824;

                //Console.WriteLine(a+" "+b+" "+c+" "+w);
                Quaternion tempQ = new Quaternion(B, A, C, W);

                //Vector3 tempR = FromQ2(tempQ);
                tempR = QuaternionToEuler_new(tempQ);
                pitch_mot = -tempR.x;
                yaw_mot = 180 + tempR.z;
                roll_mot = 180 + tempR.y;
                if ((pitch_mot > 180) && (pitch_mot < 360)) pitch_mot -= 360;//防止出现0°和360°的问题。
                if ((yaw_mot > 180) && (yaw_mot < 360)) yaw_mot -= 360;
                if ((roll_mot > 180) && (roll_mot < 360)) roll_mot -= 360;
                yaw_mot = -yaw_mot;
                x_mot = x;
                y_mot = y;
                z_mot = -z / 100;
                if (DataUltrasound[13] == 2) buttonpress = true;
                else buttonpress = false;
            }
            //Read9AxisData(DataAxis9, DataAxis9_Length, DataAxis9_ReadState);
            ////if (DataAxis9_Length[0] != 0)
            ////    Debug.Log(DataAxis9[2]);
            //if (DataAxis9_ReadState[0] == 1167)
            //    ExitThread = true;
            ////Debug.Log(DataAxis9[29] + "  " + DataAxis9[30] + "  " + DataAxis9[31] + "  " + DataAxis9[32]
            //// + "  " + DataAxis9[33] + "  " + DataAxis9[34] + "  " + DataAxis9[35] + "  " + DataAxis9[36]
            //// + "  " + DataAxis9[37] + "  " + DataAxis9[38] + "  " + DataAxis9[39] + "  " + DataAxis9[40]
            //// + "  " + DataAxis9[41] + "  " + DataAxis9[42] + "  " + DataAxis9[43] + "  " + DataAxis9[44]
            //// );
            //a = BitConverter.ToInt32(DataAxis9, 29);
            //b = BitConverter.ToInt32(DataAxis9, 33);
            //c = BitConverter.ToInt32(DataAxis9, 37);
            //w = BitConverter.ToInt32(DataAxis9, 41);
            //A = a / 1073741824;
            //B = b / 1073741824;
            //C = c / 1073741824;
            //W = w / 1073741824;

            //getpenposition();
            //GetWriting_State = getpeninputstate();
            vx = (x_mot - x_mot_old) / Time.deltaTime;
            vy = (y_mot - y_mot_old) / Time.deltaTime;
            vz = (z_mot - z_mot_old) / Time.deltaTime;
            vpitch = (pitch_mot - pitch_mot_old) / Time.deltaTime;
            vyaw = (yaw_mot - yaw_mot_old) / Time.deltaTime;
            vroll = (roll_mot - roll_mot_old) / Time.deltaTime;
            Thread.Sleep(8);
        }
    }
    void OnGUI()
    {
        GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
        myButtonStyle.fontSize = 50;
        GUIStyle myButtonStyle1 = new GUIStyle(GUI.skin.button);
        myButtonStyle1.fontSize = 90;
        if (GUI.Button(new Rect(Screen.width - 320, Screen.height - 320, 300, 50), "switch pen", myButtonStyle))
        {
            if (Data_state == 1)
            {
                Data_state = 2;//从vive转超声
                OpenVR.Shutdown();
            }

            //if (Data_state == 2) Data_state = 1;//从超声转vive
        }
        if (GUI.Button(new Rect(Screen.width - 320, Screen.height - 260, 300, 50), "switch vive", myButtonStyle))
        {
            //if (Data_state == 1) Data_state = 2;//从vive转超声
            if (Data_state == 2)
            {
                Data_state = 1;//从超声转vive
                var error = EVRInitError.None;
                IntPtr svrSystem = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Other);
                if (error != EVRInitError.None)
                {
                    Debug.Log("EVRInitError.None");
                    Noerror = false;
                }
                else
                {
                    vrSystem = new CVRSystem(svrSystem);
                    PrintOpenVRDevices();
                    //Thread vive = new Thread(Threadvive);
                    //vive.Start();
                    //Thread axis9 = new Thread(ThreadAxis9);
                    //axis9.Start();
                }
            }

        }
        if (Data_state == 1) GUI.Label(new Rect(20, Screen.height - 60, 200, 50), "Vive", myButtonStyle);
        if (Data_state == 2) GUI.Label(new Rect(20, Screen.height - 60, 200, 50), "Pen", myButtonStyle);
        //GUI.Label(new Rect(240, Screen.height - 180, 600, 50), "pitch = " + pitch_mot, myButtonStyle);
        //GUI.Label(new Rect(240, Screen.height - 120, 600, 50), "yaw = " + yaw_mot, myButtonStyle);
        //GUI.Label(new Rect(240, Screen.height - 60, 600, 50), "roll = " + roll_mot, myButtonStyle);
        //if (detectbool)
        //{
        //    GUI.Label(new Rect(Screen.width / 2 - 450, Screen.height - 200, 900, 100), "平稳平稳平稳.....", myButtonStyle1);
        //}
        //else
        //    GUI.Label(new Rect(Screen.width / 2 - 450, Screen.height - 200, 900, 100), "...不稳不稳不稳", myButtonStyle1);
    }
    void ThreadAudio()
    {
        //System.Random random = new System.Random();
        int Tick = 0;
        int Tick2 = 0;
        int Tick3 = 0;
        DateTime StartTime = DateTime.Now;
        DateTime EndTime;
        while (!ExitThread)
        {
            DataAudio[1] = 0;
            ReadAudioData(DataAudio, DataAudio_Length, DataAudio_ReadState);
            if (DataAudio_Length[0] != 0 || DataAudio[1] > 0)
            {
                if (DataAudio[2] == 0x01)
                {
                    //Debug.Log("语音开始录制");
                    Audio_Data_adr = 0;
                    for (UInt16 i = 0; i < DataAudio[1]; i += 2)
                    {
                        Audio_Data[Audio_Data_adr] = DataAudio[3 + i + 1];
                        Audio_Data[Audio_Data_adr + 1] = DataAudio[3 + i];
                        Audio_Data_adr += 2;
                        Tick = 0;
                        Tick2 = 0;
                        StartTime = DateTime.Now;
                    }
                }
                else if (DataAudio[2] == 0x02)
                {
                    //if (random.Next() % 10 == 0)
                    //    Debug.Log(DataAudio[1]);
                    for (UInt16 i = 0; i < DataAudio[1]; i += 2)
                    {
                        Audio_Data[Audio_Data_adr] = DataAudio[3 + i + 1];
                        Audio_Data[Audio_Data_adr + 1] = DataAudio[3 + i];
                        Audio_Data_adr += 2;
                    }
                    //Debug.Log(DataAudio[1].ToString("X2"));
                }
                else if (DataAudio[2] == 0x03)
                {

                    //Debug.Log("语音结束录制");
                    EndTime = DateTime.Now;
                    double mm = (EndTime - StartTime).TotalMilliseconds;
                    Debug.Log("Tick: " + Tick.ToString() + ", " + (Tick / mm * 1000).ToString());
                    Debug.Log("Tick2: " + Tick2.ToString() + ", " + (Tick2 / mm * 1000).ToString());
                    Debug.Log("Audio_Data_adr: " + Audio_Data_adr.ToString());
                    //SavWav.Save("Temp_old", Audio_Data, Audio_Data_adr);
                    //Debug.Log("语音结束录制1");
                    SavWav_new.Save("Temp_new.wav", Audio_Data, Audio_Data_adr - 100);
					//Debug.Log("语音结束录制2");

                    //使用微软识别器
					/*
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo = new System.Diagnostics.ProcessStartInfo("./YJCRecognizor.exe");
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    //proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.Default;

                    //Debug.Log(proc.StartInfo.StandardOutputEncoding);

                    proc.Start();
                    output = proc.StandardOutput.ReadLine();
                    //proc.Kill();
					*/

                    //使用科大讯飞识别器
                    UponRecognize = true;
                }
                Tick++;
            }
            else
            {
            }
            Tick2++;
        }
    }
    
    [DllImport("winmm.dll", EntryPoint="timeBeginPeriod", SetLastError=true)]
    public static extern uint TimeBeginPeriod(uint uMilliseconds);

    [DllImport("winmm.dll", EntryPoint="timeEndPeriod", SetLastError=true)]
    public static extern uint TimeEndPeriod(uint uMilliseconds);
    //
    [DllImport("mydll.dll", EntryPoint = "Read9AxisData", SetLastError = true)]
    public static extern void Read9AxisData(byte[] pBuf, long[] plen, long[] pReadState);

    [DllImport("mydll.dll", EntryPoint = "ReadAudioData", SetLastError = true)]
    public static extern void ReadAudioData(byte[] pBuf, long[] plen, long[] pReadState);

    [DllImport("mydll.dll", EntryPoint = "WriteMydev", SetLastError = true)]
    public static extern void WriteMydev(byte[] pBuf, long plen, long[] pReadState);

    [DllImport("mydll.dll", EntryPoint = "EnumMyHid", SetLastError = true)]
    public static extern byte EnumMyHid();

    [DllImport("mydllv2.dll", EntryPoint = "ReadMydev", SetLastError = true)]
    public static extern void ReadMydev(byte[] pBuf);

    [DllImport("mydllv2.dll", EntryPoint = "WriteMydev", SetLastError = true)]
    public static extern void WriteMydev(byte[] pBuf1);

    [DllImport("mydll_position.dll", EntryPoint = "ReadMydev", SetLastError = true)]
    public static extern void ReadMydev(byte[] pBuf, long[] plen, long[] pReadState);
}
