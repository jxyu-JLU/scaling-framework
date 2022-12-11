using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class scalingmap : MonoBehaviour {
    public Data_get m_Data_get;
    public Image m_image;
    Vector2 tempvec2,stempvec2,anchor;
    bool buttonstate = false;
    bool triggerstate = false;
    public float pitch_mot, yaw_mot, roll_mot, x_mot, y_mot, z_mot;
    float vx, vy, vz, vpitch, vyaw, vroll, input_vp, input_vr;
    float factor_r, factor_p;
    float scalingdata,scalingdata_r, scalingdata_p;
    float inputdatar, inputdatap;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate()
    {
        x_mot = m_Data_get.x_mot;
        y_mot = m_Data_get.y_mot;
        z_mot = m_Data_get.z_mot;
        pitch_mot = m_Data_get.pitch_mot;
        yaw_mot = m_Data_get.yaw_mot;
        roll_mot = m_Data_get.roll_mot;
        vx = m_Data_get.vx;
        vy = m_Data_get.vy;
        input_vp = m_Data_get.vz;
        vpitch = m_Data_get.vpitch;
        vyaw = m_Data_get.vyaw;
        input_vr = m_Data_get.vroll;
        Movefunc();
        Scalingfunc();
        //x_mot_old = x_mot;
        //y_mot_old = y_mot;
        //z_mot_old = z_mot;
        //pitch_mot_old = pitch_mot;
        //yaw_mot_old = yaw_mot;
        //roll_mot_old = roll_mot;
    }
    private void Scalingfunc()
    {
        //刚刚按下的状态，和保持按下的状态，以及按键抬起的状态。
        if ((!buttonstate) && (m_Data_get.buttonpress))
        {
            buttonstate = true;

            //start scaling            
            
            //inputdata0p = inputdatap;
            //inputdata0r = inputdatar;
        }
        if ((buttonstate) && (m_Data_get.buttonpress))
        {
            if (Math.Abs(input_vp) < 10) factor_p = 0;
            else factor_p = 1;
            if (Math.Abs(input_vr) < 10) factor_r = 0;
            else factor_r = 1;
            ////需要给出速度的阈值，大于多少怎么处理，加入比较。_old_old
            //Debug.Log("Vr= "+ input_vr+ ";  Vp= " + input_vp + "; f_r= " + factor_r + "; f_p= " + factor_p+"; 比值="+ Math.Abs(input_vr / (input_vp + 0.0001)));
            if (Math.Abs(input_vr / (input_vp + 10)) > 3)
            {
                scalingdata_r = scalingdata_r + factor_r * (inputdatar - inputdatar);
                //scalingdata = Mathf.Lerp(scalingdata_old,(float)Math.Pow(scalingdata_r * Mathf.PI / 18, 3)+ scalingdata_p / 10,0.5f) ;//以角度为准。累加形式
            }
            else
            {
                scalingdata_p = scalingdata_p + factor_p * (inputdatap - inputdatap);
                //scalingdata = (float)Math.Pow(scalingdata_r * Mathf.PI / 18, 3) + scalingdata_p / 10;//以位移为准。累加形式
            }
            scalingdata = scalingdata_r * scalingdata_r / 36 + scalingdata_p * 1.5f;
            m_image.rectTransform.localScale = new Vector3(scalingdata + 1, scalingdata + 1, 1);
        }//按键抬起停止缩放？这块需要再考虑考虑。4-2-4
        if ((buttonstate) && (!m_Data_get.buttonpress))
        {
            
            //结束缩放
            buttonstate = false;
            //scalingdata_p = 0;
            //scalingdata_r = 0;
            //scalingdata = 0;
            //scalingdata_old = 0;
                     
        }

    }

    void Movefunc()
    {
        if (!triggerstate && m_Data_get.triggerpress)
        {
            triggerstate = true;
            //record the position
            tempvec2 = new Vector2(x_mot, y_mot);
        }
        if (triggerstate && m_Data_get.triggerpress)
        {
            //move and change the anchor.
            stempvec2 = new Vector2(x_mot, y_mot);
            anchor = stempvec2 - tempvec2;
            m_image.rectTransform.pivot = new Vector2((anchor.x + 4096) / 8192, (anchor.y + 2048) / 4096);
            //move
            m_image.rectTransform.position = anchor;
        }
    }
}
