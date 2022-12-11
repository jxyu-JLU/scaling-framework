using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using System.IO;
using Valve.VR;



public class Data_get : MonoBehaviour {

    bool ExitThread = false;
    public bool ExitThread1 = true;
    //bool StartConvert = false;
    //bool StartSave = true;
    Vector3 tempR;
    public byte[] DataAxis9 = new byte[66];
    public long[] DataAxis9_Length = new long[1];
    public long[] DataAxis9_ReadState = new long[1];
    public byte[] DataAudio = new byte[66];
    public long[] DataAudio_Length = new long[1];
    public long[] DataAudio_ReadState = new long[1];
    public byte[] DataUltrasound = new byte[66];
    public int old_dataultrasound = 0;
    public byte[] Audio_Data = new byte[1920000];
    public byte[] Audio_Data1 = new byte[1920000];

    public byte[] USBDATA = new byte[66];
    public long[] USBDATA_Length = new long[1];
    public long[] USBDATA_ReadState = new long[1];
    // depend which device used
    int Data_state = 2;
    //public GameObject cube;
    public float a, A;
    public float b, B;
    public float c, C;
    public float w, W;
    public float eulerx, eulery, eulerz;
    public float accelx, accely, accelz, accelx0, accely0, accelz0;
    public float x, y, z;
    float X, Y, Z;
    public bool Speechisrecognized = false;
    public bool IsWriting = false;
    public bool EndWriting = false;
    public int GetWriting_State;

    public bool UponRecognize = false;
    public string output;
    public int recordingstate = 0;
    public int m = 0;
    int Ticktime = 0;
    bool detectbool = false;
    bool writebool = false;
    public Vector3 Pen_Position;

    public bool PenPositionSimulation = true;  // simulate pen movement instead of reading from real device   (false = 2m , true = 4m)


    float pitch, yaw, roll, pitch_old, yaw_old, roll_old, delta_pitch, delta_yaw, delta_roll;
    float delta_pitch_old, delta_roll_old, delta_yaw_old, delta2_pitch, delta2_roll, delta2_yaw;
    float x_old, y_old, z_old, delta_x, delta_y, delta_z, delta_x_old, delta_z_old, delta_y_old, delta2_x, delta2_z, delta2_y;
    FileStream fileInput2;
    StreamWriter streamInput2;

    double[][] DOF_6 = new double[41][];

    public bool start_roll = false;
    public bool real_start_roll = false;
    public bool real_start_rollpca = false;

    public float[] pitchtemp = new float[100];
    public float[] yawtemp = new float[100];
    public float[] rolltemp = new float[100];
    public float[] xdatatemp = new float[100];
    public float[] ydatatemp = new float[100];
    public float[] zdatatemp = new float[100];

    public double[] xverifydata = new double[100];
    public double[] yverifydata = new double[100];
    public double[] zverifydata = new double[100];

	public int rollingstate = 1;
    public int rollingstate_pca = 1;
	float rollingtime = 0;
    float rollingtime_pca = 0;
    //float roll_temp;
    public float pitch_disp, yaw_disp, roll_disp, x_disp, y_disp, z_disp;
    public float pitch_disp_pca, yaw_disp_pca, roll_disp_pca, x_disp_pca, y_disp_pca, z_disp_pca;
    public float pitch_mot, yaw_mot, roll_mot, x_mot, y_mot, z_mot, pitch_mot_old, yaw_mot_old, roll_mot_old, x_mot_old, y_mot_old, z_mot_old;
    public float pitch_temp, yaw_temp, roll_temp, x_temp, y_temp, z_temp;
    public float pitch_temp_pca, yaw_temp_pca, roll_temp_pca, x_temp_pca, y_temp_pca, z_temp_pca;
    public float vx, vy, vz, vpitch, vyaw, vroll;
    public bool clutched = true;
    public bool clutched_pca = true;

    //public selectiontask1 taskinfo;
    //public Selectiontask0 taskinfo_pca;
	//public getrotation m_getrotation;
	public int selectiontoolstate;//这里1为按键方式，2为旋转方式。
	public float pitch_rollingdata, yaw_rollingdata, x_rollingdata, y_rollingdata, z_rollingdata;
	public float x_rollingdata1, y_rollingdata1, z_rollingdata1;
	Vector3 temp_orientation;
	DateTime Datetime;
    float[] pitchdiffdata15 = new float[15];
    float[] yawdiffdata15 = new float[15];
    float[] rolldiffdata15 = new float[15];
    float[] xdiffdata15 = new float[15];
    float[] ydiffdata15 = new float[15];
    float[] zdiffdata15 = new float[15];

    float[] r_p = new float[15];
    float[] r_y = new float[15];
    float[] r_x = new float[15];
    float[] r_yy = new float[15];
    float[] r_z = new float[15];
    float Gconstant = 1;
    private int data_lenth = 15;

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
    //cubecolor m_cubecolor;
    //DateTime Datetime;
    DateTime Datetime111;
    int oldtime, time;
    int Hz = 0;
    int i = 0;
    public void Awake()
    {
        //Screen.SetResolution(1280, 1024, true);
        //Application.targetFrameRate = 90;
    }

	// Use this for initialization
	void Start () 
    {
        oldtime = 0;
        Datetime111 = DateTime.Now;
        byte r = EnumMyHid();
        Debug.Log("r = " + r.ToString());

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
        //Thread Roll_Judge = new Thread(Threadrollingdect);
        //Roll_Judge.Start();
        //Thread audio = new Thread(ThreadAudio);
        //audio.Start();
        //Thread writing = new Thread(ThreadWrite);
        //writing.Start();
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
    void FixedUpdate () 
    {
        //cube.transform.rotation = new Quaternion(B,A,C,   W);
        time = DateTime.Now.Second;
        //Debug.Log(Time.deltaTime.ToString());
        //if (time - oldtime < 0) Hz = 1000 / (1000 + time - oldtime);
        //else Hz = 1000 / (time - oldtime);
        i = i + 1;
        //if (i > 60000) i = 0;
        if ((time % 3 == 0) && time != oldtime)
        {
            //Debug.Log("计数 i=" + i);
            Hz = i / 3;
            i = 0;
        }
        ThreadAxis9();

        if (Input.GetKeyDown (KeyCode.A))
        {
			Datetime = DateTime.Now;
            Debug.Log("A");
            ExitThread1 = false;          
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("B");
            ExitThread1 = true;
        }
        //judgenumber value change while the rolling state change
        oldtime = time;
	}
    void Getpenposition() 
    {
        ReadMydev(DataUltrasound, USBDATA_Length, USBDATA_ReadState);
        X = BitConverter.ToInt32(DataUltrasound, 1);   //将基数据类型转换为一个字节数组以及将一个字节数组转换为基数据类型。
        Y = BitConverter.ToInt32(DataUltrasound, 5);   //返回转换的字节数组中指定位置处的四个字节从 32 位有符号的整数。
        Z = BitConverter.ToInt32(DataUltrasound, 9);

        x = X;
        y = -Y;
        z = Z;
        Pen_Position = new Vector3(x / 100, y / 100, z / 100);
    }

    void OnDestroy()
    {
        ExitThread = true;
        OpenVR.Shutdown();
    }

    void ThreadAxis9()
    {

        //vive数据获取
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
                        if (ExitThread1)
                        {
                            Datetime = DateTime.Now;
                            ExitThread1 = false;
                        }
                        else ExitThread1 = true;
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
                    yaw_mot = v3a.z-180;
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
            Getpenposition();
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
            yaw_mot = 180+tempR.z;
            roll_mot = 180+tempR.y;
            if ((pitch_mot > 180) && (pitch_mot < 360)) pitch_mot -= 360;//防止出现0°和360°的问题。
            if ((yaw_mot > 180) && (yaw_mot < 360)) yaw_mot -= 360;
            if ((roll_mot > 180) && (roll_mot < 360)) roll_mot -= 360;
            yaw_mot = -yaw_mot;
            x_mot = x / 100;
            y_mot = y / 100;
            z_mot = -z / 100;
            if (DataUltrasound[13] == 2) buttonpress = true;
            else buttonpress = false;
        }
        //计算速度数据。
        vx = (x_mot - x_mot_old) / Time.deltaTime;
        vy = (y_mot - y_mot_old) / Time.deltaTime;
        vz = (z_mot - z_mot_old) / Time.deltaTime;
        vpitch = (pitch_mot - pitch_mot_old) / Time.deltaTime;
        vyaw = (yaw_mot - yaw_mot_old) / Time.deltaTime;
        vroll = (roll_mot - roll_mot_old) / Time.deltaTime;

        //记录数据
        //if ((DataUltrasound[13] == 3 || triggerpress || Input.GetMouseButtonDown(0)) && !writebool) writebool = true;
        //if ((DataUltrasound[13] == 3 || triggerpress || Input.GetMouseButtonDown(0)) && writebool) writebool = false;

        if (!ExitThread1)
        {
            fileInput2 = new FileStream(@"F:\scaling-"+Data_state+"-" + Datetime.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv", FileMode.Append, FileAccess.Write);
            streamInput2 = new StreamWriter(fileInput2);

            streamInput2.WriteLine(x_mot + "," + y_mot + "," + z_mot + "," +
                                   pitch_mot + "," + yaw_mot + "," + roll_mot + "," +
                                   vx + "," + vy + "," + vz+ "," + Data_state + "," + DateTime.Now.Millisecond
                                   );
            streamInput2.Close();
            fileInput2.Close();
            Debug.Log("writing");
        }

        x_mot_old = x_mot;
        y_mot_old = y_mot;
        z_mot_old = z_mot;
        pitch_mot_old = pitch_mot;
        yaw_mot_old = yaw_mot;
        roll_mot_old = roll_mot;
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
        if (Data_state == 1)GUI.Label(new Rect(20, Screen.height - 60, 200, 50), "Vive", myButtonStyle);
        if (Data_state == 2)GUI.Label(new Rect(20, Screen.height - 60, 200, 50), "Pen", myButtonStyle);
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
    
    [DllImport("winmm.dll", EntryPoint="timeBeginPeriod", SetLastError=true)]
    public static extern uint TimeBeginPeriod(uint uMilliseconds);

    [DllImport("winmm.dll", EntryPoint="timeEndPeriod", SetLastError=true)]
    public static extern uint TimeEndPeriod(uint uMilliseconds);
    //
    [DllImport("mydll2018.dll", EntryPoint = "Read9AxisData", SetLastError = true)]
    public static extern void Read9AxisData(byte[] pBuf, long[] plen, long[] pReadState);

    [DllImport("mydll2018.dll", EntryPoint = "ReadAudioData", SetLastError = true)]
    public static extern void ReadAudioData(byte[] pBuf, long[] plen, long[] pReadState);

    [DllImport("mydll2018.dll", EntryPoint = "WriteMydev", SetLastError = true)]
    public static extern void WriteMydev(byte[] pBuf, long plen, long[] pReadState);

    [DllImport("mydll2018.dll", EntryPoint = "EnumMyHid", SetLastError = true)]
    public static extern byte EnumMyHid();

    //[DllImport("mydllv2.dll", EntryPoint = "ReadMydev", SetLastError = true)]
    //public static extern void ReadMydev(byte[] pBuf);

    //[DllImport("mydllv2.dll", EntryPoint = "WriteMydev", SetLastError = true)]
    //public static extern void WriteMydev(byte[] pBuf1);

    [DllImport("mydll_position.dll", EntryPoint = "ReadMydev", SetLastError = true)]
    public static extern void ReadMydev(byte[] pBuf, long[] plen, long[] pReadState);
}
