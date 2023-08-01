using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class PlayerCtrl : MonoBehaviour {
    public enum PlayerStates
    {
        Idle,
        Walk,
        Run
    }
    public Animator m_Animator;
    public PlayerStates _state = PlayerStates.Idle;
    public Slider m_time1, m_time2;
    public Slider m_time3, m_time4;
    public Transform left, right;
    
    public GameObject m_Parameters_Running;
    public GameObject m_Parameters_Walking;
    public GameObject m_Shock;
    public GameObject run_HeadAngle, run_StepWidth, run_Balance, run_VO;
    public GameObject run_FlightTime, run_ContactTime, run_StepLength, run_Shock;
    public GameObject walk_HeadAngle, walk_StepWidth, walk_Balance, walk_VO;
    public GameObject walk_FlightTime, walk_ContactTime, walk_StepLength;

    public Slider[] m_walk, m_run;

    private bool isShocked=false;
	private float state = 0.0f;
	private float vertical_walk = 0.0f, vertical_run = 0.0f;
    private float step_length_walk = 0.0f, step_length_run = 0.0f;
    private float target_State = 0.0f;
    private float shock_value = 0.0f;
    private float one_step_time_walk = 0.5f;
    private float one_step_time_run = 0.5f;
    private bool ok = true;

    // Standard Run Parameter
    private float MinHeadAngleRun = -45, MaxHeadAngleRun = 45, DefaultHeadAngleRun = 0;
    private float MinBalanceRun = -5, MaxBalanceRun = 5, DefaultBalanceRun = 0;
    private float MinStepWidthRun = 0, MaxStepWidthRun = 0.3f, DefaultStepWidthRun = 0.04f;
    private float MinVerticalRun = 0.03f, MaxVerticalRun = 0.2f, DefaultVerticalRun = 0.06f;
    private float MinStepLengthRun = 0, MaxStepLengthRun = 1.8f, DefaultStepLengthRun = 0.7f;
    private float MinFlightTimeRun = 0.04f, MaxFlightTimeRun = 0.16f, DefaultFlightTimeRun = 0.1f;
    private float MinContactTimeRun = 0.18f, MaxContactTimeRun = 0.43f, DefaultContactTimeRun = 0.4f;//0.25f;
    private float MinShockRun = 30f, MaxShockRun = 150f, ThresholdShockRun = 110f;

    // Standard Walk Parameter
    private float MinHeadAngleWalk = -45, MaxHeadAngleWalk = 45, DefaultHeadAngleWalk = 0;
    private float MinBalanceWalk = -5, MaxBalanceWalk = 5, DefaultBalanceWalk = 0;
    private float MinStepWidthWalk = 0, MaxStepWidthWalk = 0.3f, DefaultStepWidthWalk = 0.04f;
    private float MinVerticalWalk = 0.03f, MaxVerticalWalk = 0.12f, DefaultVerticalWalk = 0.06f;
    private float MinStepLengthWalk = 0, MaxStepLengthWalk = 1f, DefaultStepLengthWalk = 0.65f;
    private float MinSingleTimeWalk = 0.3f, MaxSingleTimeWalk = 0.55f, DefaultSingleTimeWalk = 0.44f;
    private float MinDoubleTimeWalk = 0.1f, MaxDoubleTimeWalk = 0.25f, DefaultDoubleTimeWalk = 0.14f;

    private float current_speed = 1.0f;

    void Start()
    {
        InitIdle();
        StartCoroutine("SetValues");
    }
	
	// Update is called once per frame
	void Update () {
        state += (target_State - state) * Time.deltaTime * 2;
        if (Mathf.Abs(target_State - state) < 1e-3) state = target_State;
        if (state <= 0.05f)
        {
            m_Animator.SetFloat("Action_State", 0.0f);
            m_Animator.SetLayerWeight(10, 0.0f);
            m_Animator.SetLayerWeight(9, 0.0f);

            //m_Parameters_Walking.SetActive(false);
            //m_Parameters_Running.SetActive(false);
        }

        m_Animator.SetFloat("Action_State", state);

        if (state < 0.7f) isShocked = false;
        else if (shock_value >= 110.0f) isShocked = true;

        UpdateVerticalRun(vertical_run);
        UpdateStepLengthRun(step_length_run);

        UpdateVerticalWalk(vertical_walk);
        UpdateStepLengthWalk(step_length_walk);

        if (state >= 0.95f)
        {
            //m_Parameters_Walking.SetActive(false);
            //m_Parameters_Running.SetActive(true);
//            if (Mathf.Abs(target_State-state)>0.04f) InitRun();
        }
        if (state >= 0.45f && state <= 0.55f)
        {
            //m_Parameters_Walking.SetActive(true);
            //m_Parameters_Running.SetActive(false);
//            if (Mathf.Abs(target_State - state) > 0.04f) InitWalk();
        }
    }

    // Make Default
    public void MakeDefault()
    {
        if (target_State == 0.5f) InitWalk();
        if (target_State == 1.0f) InitRun();
    }

    // Initialize idle parameters
    public void InitIdle()
    {
        UpdateHeadAngle(0);
        UpdateBalanceLeft(0);
        UpdateStepWidth(0);
        UpdateVerticalRun(0);
        UpdateVerticalWalk(0);
        UpdateStepLengthRun(0);
        UpdateStepLengthWalk(0);
        m_time1.value = 0;
        m_time2.value = 0;
        m_time3.value = 0;
        m_time4.value = 0;
    }

    public void SetIdle()
    {
        UpdateHeadAngle(0);
        UpdateBalanceLeft(0);
        UpdateStepWidth(0);
        UpdateVerticalRun(0);
        UpdateVerticalWalk(0);
        UpdateStepLengthRun(0);
        UpdateStepLengthWalk(0);
        m_time1.value = 0;
        m_time2.value = 0;
        m_time3.value = 0;
        m_time4.value = 0;
        UpdateState(0f);
    }
    // get slider value for real value
    public float GetVal(float min, float max, float def, float amin, float amax)
    {
        def = Mathf.Min(max, Mathf.Max(def, min));
        return (def - min) / (max - min) * (amax - amin) + amin;
    }

    // get real value for silder value
    public float GetText(float min, float max, float val, float amin, float amax)
    {
        return (val - amin) / (amax - amin) * (max - min) + min;
    }

    // Initialize walk parameters
    public void InitWalk()
    {
        m_walk[0].value = GetVal(MinHeadAngleWalk, MaxHeadAngleWalk, DefaultHeadAngleWalk, -1, 1);
        m_walk[1].value = GetVal(MinBalanceWalk, MaxBalanceWalk, DefaultBalanceWalk, -1, 1);
        m_walk[2].value = GetVal(MinStepWidthWalk, MaxStepWidthWalk, DefaultStepWidthWalk, 0, 1);
        m_walk[3].value = GetVal(MinVerticalWalk, MaxVerticalWalk, DefaultVerticalWalk, 0, 1);
        m_walk[4].value = GetVal(MinStepLengthWalk, MaxStepLengthWalk, DefaultStepLengthWalk, 0, 1);
        m_walk[5].value = GetVal(MinSingleTimeWalk, MaxSingleTimeWalk, DefaultSingleTimeWalk, 0, 0.8f);
        m_walk[6].value = GetVal(MinDoubleTimeWalk, MaxDoubleTimeWalk, DefaultDoubleTimeWalk, 0, 0.8f);
        int id = 0;
        UpdateHeadAngle(m_walk[id++].value);
        UpdateBalanceLeft(m_walk[id++].value);
        UpdateStepWidth(m_walk[id++].value);
        UpdateVerticalWalk(m_walk[id++].value);
        UpdateStepLengthWalk(m_walk[id++].value);
        m_time3.value = m_walk[id++].value;
        m_time4.value = m_walk[id++].value;
    }

    public void SetWalk(float head_angle, float balance, float step_width, float vo, float step_length, float single_time, float double_time)
    {
        m_walk[0].value = GetVal(MinHeadAngleWalk, MaxHeadAngleWalk, head_angle, -1, 1);
        m_walk[1].value = GetVal(MinBalanceWalk, MaxBalanceWalk, balance, -1, 1);
        m_walk[2].value = GetVal(MinStepWidthWalk, MaxStepWidthWalk, step_width, 0, 1);
        m_walk[3].value = GetVal(MinVerticalWalk, MaxVerticalWalk, vo, 0, 1);
        m_walk[4].value = GetVal(MinStepLengthWalk, MaxStepLengthWalk, step_length, 0, 1);
        m_walk[5].value = GetVal(MinSingleTimeWalk, MaxSingleTimeWalk, single_time, 0, 0.8f);
        m_walk[6].value = GetVal(MinDoubleTimeWalk, MaxDoubleTimeWalk, double_time, 0, 0.8f);
        int id = 0;
        UpdateHeadAngle(m_walk[id++].value);
        UpdateBalanceLeft(m_walk[id++].value);
        UpdateStepWidth(m_walk[id++].value);
        UpdateVerticalWalk(m_walk[id++].value);
        UpdateStepLengthWalk(m_walk[id++].value);
        m_time3.value = m_walk[id++].value;
        m_time4.value = m_walk[id++].value;
        UpdateState(0.5f);
    }
    // Initialize run parameters
    public void InitRun()
    {
        m_run[0].value = GetVal(MinHeadAngleRun, MaxHeadAngleRun, DefaultHeadAngleRun, -1, 1);
        m_run[1].value = GetVal(MinBalanceRun, MaxBalanceRun, DefaultBalanceRun, -1, 1);
        m_run[2].value = GetVal(MinStepWidthRun, MaxStepWidthRun, DefaultStepWidthRun, 0, 1);
        m_run[3].value = GetVal(MinVerticalRun, MaxVerticalRun, DefaultVerticalRun, 0, 1);
        m_run[4].value = GetVal(MinStepLengthRun, MaxStepLengthRun, DefaultStepLengthRun, 0, 1);
        m_run[5].value = GetVal(MinFlightTimeRun, MaxFlightTimeRun, DefaultFlightTimeRun, 0, 0.8f);
        m_run[6].value = GetVal(MinContactTimeRun, MaxContactTimeRun, DefaultContactTimeRun, 0, 0.8f);
        m_run[7].value = GetVal(MinShockRun, MaxShockRun, 30, 30, 150);
        int id = 0;
        UpdateHeadAngle(m_run[id++].value);
        UpdateBalanceLeft(m_run[id++].value);
        UpdateStepWidth(m_run[id++].value);
        UpdateVerticalRun(m_run[id++].value);
        UpdateStepLengthRun(m_run[id++].value);
        m_time1.value = m_run[id++].value;
        m_time2.value = m_run[id++].value;
        UpdateShock(m_run[id++].value);
    }

    public void SetRun(float head_angle, float balance, float step_width, float vo, float step_length, float flight_time, float contact_time, float shock)
    {
        m_run[0].value = GetVal(MinHeadAngleRun, MaxHeadAngleRun, head_angle, -1, 1);
        m_run[1].value = GetVal(MinBalanceRun, MaxBalanceRun, balance, -1, 1);
        m_run[2].value = GetVal(MinStepWidthRun, MaxStepWidthRun, step_width, 0, 1);
        m_run[3].value = GetVal(MinVerticalRun, MaxVerticalRun, vo, 0, 1);
        m_run[4].value = GetVal(MinStepLengthRun, MaxStepLengthRun, step_length, 0, 1);
        m_run[5].value = GetVal(MinFlightTimeRun, MaxFlightTimeRun, flight_time, 0, 0.8f);
        m_run[6].value = GetVal(MinContactTimeRun, MaxContactTimeRun, contact_time, 0, 0.8f);
        m_run[7].value = GetVal(MinShockRun, MaxShockRun, shock, 30, 150);
        int id = 0;
        UpdateHeadAngle(m_run[id++].value);
        UpdateBalanceLeft(m_run[id++].value);
        UpdateStepWidth(m_run[id++].value);
        UpdateVerticalRun(m_run[id++].value);
        UpdateStepLengthRun(m_run[id++].value);
        m_time1.value = m_run[id++].value;
        m_time2.value = m_run[id++].value;
        UpdateShock(m_run[id++].value);
        UpdateState(1.0f);
    }
    // Save destination status for changing current status
    public void UpdateState(float a) 
    {
        if (Mathf.Abs(a - 0.5f) < 0.1f)
        {
            if (ok)
            {
                ok = false;
                InitWalk();
            }
        }
        if (Mathf.Abs(a - 1.0f) < 0.1f)
        {
            if (ok)
            {
                ok = false;
                InitRun();
            }
        }
        target_State = a;
    }

    // control head angle
    public void UpdateHeadAngle(float a)
    {
        run_HeadAngle.GetComponent<Text>().text = GetText(MinHeadAngleRun, MaxHeadAngleRun, a, -1, 1).ToString("0.0");
        walk_HeadAngle.GetComponent<Text>().text = GetText(MinHeadAngleWalk, MaxHeadAngleWalk, a, -1, 1).ToString("0.0");
        if (a >= 0.0f)
        {
            m_Animator.SetLayerWeight(1, Mathf.Abs(a));
            m_Animator.SetLayerWeight(2, 0.0f);
        }
        else
        {
            m_Animator.SetLayerWeight(2, Mathf.Abs(a));
            m_Animator.SetLayerWeight(1, 0.0f);
        }
    }

    // control step width
    public void UpdateStepWidth(float a)
    {
        run_StepWidth.GetComponent<Text>().text = GetText(MinStepWidthRun, MaxStepWidthRun, a, 0, 1).ToString("0.00");
        walk_StepWidth.GetComponent<Text>().text = GetText(MinStepWidthWalk, MaxStepWidthWalk, a, 0, 1).ToString("0.00");

        m_Animator.SetLayerWeight(3, (a>=0.04f) ? a - 0.04f : 0.0f);
        m_Animator.SetLayerWeight(4, (a >= 0.04f) ? a - 0.04f : 0.0f);
        m_Animator.SetLayerWeight(11, (a >= 0.04f) ? 0.0f : ((0.04f - a)*(0.6f / 0.04f)));

        
    }

    // control step length when running
    public void UpdateStepLengthRun(float a)
    {
        step_length_run = a;
        m_Animator.SetLayerWeight(9, (step_length_run * 0.8f) + 0.2f);

        m_Animator.SetFloat("Action_Length", (step_length_run * 0.8f) + 0.2f);

//        m_Animator.speed = current_speed + (1.0f - a);

        run_StepLength.GetComponent<Text>().text = GetText(MinStepLengthRun, MaxStepLengthRun, a, 0, 1).ToString("0.00");
    }

    // control step length when walking
    public void UpdateStepLengthWalk(float a)
    {
        step_length_walk = a;
        m_Animator.SetLayerWeight(9, (step_length_walk * 0.8f) + 0.2f);

        m_Animator.SetFloat("Action_Length", (step_length_walk * 0.8f) + 0.2f);

//        m_Animator.speed = current_speed + (1.0f - a);

        walk_StepLength.GetComponent<Text>().text = GetText(MinStepLengthWalk, MaxStepLengthWalk, a, 0, 1).ToString("0.00");
    }
    // control balance
    public void UpdateBalanceLeft(float a)
    {
        run_Balance.GetComponent<Text>().text = GetText(MinBalanceRun, MaxBalanceRun, a, -1, 1).ToString("0.00");
        walk_Balance.GetComponent<Text>().text = GetText(MinBalanceWalk, MaxBalanceWalk, a, -1, 1).ToString("0.00");
        if (a >= -0.4f && a <= 0.4f) return;
        if (a < 0)
        {
            m_Animator.SetLayerWeight(5, (-0.4f - a) / 0.6f);
            m_Animator.SetLayerWeight(6, 0);
        }
        else
        {
            m_Animator.SetLayerWeight(6, (a - 0.4f) / 0.6f);
            m_Animator.SetLayerWeight(5, 0);
        }
    }

    // control shock effect
    public void UpdateShock(float a)
    {
        isShocked = state >= 0.7f && a >= 110;
        shock_value = a;
        run_Shock.GetComponent<Text>().text = GetText(MinShockRun, MaxShockRun, a, 30, 150).ToString("0");
    }

    // control vertical oscillation when running
	public void UpdateVerticalRun(float a)
	{
		vertical_run = a;
        m_Animator.SetLayerWeight(8, a);
        run_VO.GetComponent<Text>().text = GetText(MinVerticalRun, MaxVerticalRun, a, 0, 1).ToString("0.00");
    }

    // control vertical oscillation when walking
    public void UpdateVerticalWalk(float a)
    {
        vertical_walk = a;
        m_Animator.SetLayerWeight(8, a);
        walk_VO.GetComponent<Text>().text = GetText(MinVerticalWalk, MaxVerticalWalk, a, 0, 1).ToString("0.00");
    }


    // set time for shock effect when running
    public void Run_hit(int a)
    {
        if (!isShocked) return;
        if (a == 4)
        {
            Instantiate(m_Shock, right);
        }
        else
        {
            Instantiate(m_Shock, left);
        }
    }

    // count flight time when running
    public float real_Run_Flight_Time()
    {
        return (m_time1.value - m_time1.minValue) / (m_time1.maxValue - m_time1.minValue) * (MaxFlightTimeRun - MinFlightTimeRun) + MinFlightTimeRun;
    }

    // count contact time when running
    public float real_Run_Contact_Time()
    {
        return (m_time2.value - m_time2.minValue) / (m_time2.maxValue - m_time2.minValue) * (MaxContactTimeRun - MinContactTimeRun) + MinContactTimeRun;
    }

    // count single stance time when walking
    public float real_Walk_Single_Time()
    {
        return (m_time3.value - m_time3.minValue) / (m_time3.maxValue - m_time3.minValue) * (MaxSingleTimeWalk - MinSingleTimeWalk) + MinSingleTimeWalk;
    }

    // count double stance time for walking
    public float real_Walk_Double_Time()
    {
        return (m_time4.value - m_time4.minValue) / (m_time4.maxValue - m_time4.minValue) * (MaxDoubleTimeWalk - MinDoubleTimeWalk) + MinDoubleTimeWalk;
    }

    // count time for one step when running
    public float real_Run_step_time()
    {
        return real_Run_Contact_Time() + real_Run_Flight_Time();
    }

    // count time for one step when walking
    public float real_Walk_step_time()
    {
        return real_Walk_Double_Time() + real_Walk_Single_Time();
    }

    // control speed of flight when walking
    public void Walk_Flight()
	{
        if (cnt > 0) //Debug.Log((Time.time - pre) / cnt + " ");
        if (cnt == 0) pre = Time.time;
        cnt = (cnt + 1) % 10;
        m_Animator.speed = GetVal(0, 1f, m_walk[4].value, 1.0625f, 0.4949f) / real_Walk_Single_Time() * 24 / 30;
        current_speed = m_Animator.speed;
        walk_FlightTime.GetComponent<Text>().text = real_Walk_Single_Time().ToString("0.00");
    }

    // control speed of double stance when walking
    private float sum = 0;
	public void Walk_Contact()
	{
        m_Animator.speed = GetVal(0, 1f, m_walk[4].value, 1.0625f, 0.4949f) / real_Walk_Double_Time() * 6 / 30;
        current_speed = m_Animator.speed;
        walk_ContactTime.GetComponent<Text>().text = real_Walk_Double_Time().ToString("0.00");
	}
    private float pre = 0;
    private int cnt = 0;
    // control speed of single stance when running
    public void Run_Contact(int a)
	{
        if (cnt > 0) Debug.Log((Time.time - pre) / cnt);
        if (cnt == 0) pre = Time.time;
        cnt = (cnt + 1) % 10;
        m_Animator.speed = GetVal(0, 1f, m_run[4].value, 1.0225f, 0.2965f) / real_Run_Contact_Time() * 9 / 18;
        current_speed = m_Animator.speed;
        run_ContactTime.GetComponent<Text>().text = real_Run_Contact_Time().ToString("0.00");
    }

    // control flight speed when running
    public void Run_Flight(int a)
    {
        m_Animator.speed = GetVal(0, 1f, m_run[4].value, 1.0225f, 0.2965f) / real_Run_Flight_Time() * 9 / 18;
        current_speed = m_Animator.speed;
        run_FlightTime.GetComponent<Text>().text = real_Run_Flight_Time().ToString("0.00");
    }

    //interface from native android or ios
    private JSONNode jSONData;
    private string param;

    //UnityPlayer.UnitySendMessage("TransObj", "ProcessParam","param");
    public void ProcessParam(string param)
    {
        jSONData = SimpleJSON.JSON.Parse(param);
        string state = jSONData["state"].ToString();
        if(state == "0")
        {
            //idle
            SetIdle();
        }
        else if(state == "1")
        {
            //walking
            try
            {
                JSONNode walking = jSONData["walking"];
                float step_width = walking["step_width"].AsFloat;
                float head_angle = walking["head_angle"].AsFloat;
                float vo = walking["vo"].AsFloat;
                float single_stance_time = walking["single_stance_time"].AsFloat;
                float double_stance_time = walking["double_stance_time"].AsFloat;
                float balance = walking["balance"].AsFloat;
                float step_length = walking["step_length"].AsFloat;
                SetWalk(head_angle, balance, step_width, vo, step_length, single_stance_time, double_stance_time);
            }
            catch (Exception) {
            }
        }
        else if(state == "2")
        {
            //running
            try
            {
                JSONNode running = jSONData["running"].Value;
                float step_width = running["step_width"].AsFloat;
                float head_angle = running["head_angle"].AsFloat;
                float vo = running["vo"].AsFloat;
                float flight_time = running["flight_time"].AsFloat;
                float contact_time = running["contact_time"].AsFloat;
                float balance = running["balance"].AsFloat;
                float step_length = running["step_length"].AsFloat;
                float shock = running["shock"].AsFloat;
                SetRun(head_angle, balance, step_width, vo, step_length, flight_time, contact_time, shock);
            }
            catch (Exception)
            {

            }
        }
    }

    public IEnumerator SetValues()
    {
        //test param
        string param = "{\"state\": 1, \"walking\":{\"step_width\":0.1452,\"head_angle\":7.231,\"vo\":0.072,\"single_stance_time\": 0.2912,\"double_stance_time\":0.2149,\"cadence\":119.235,\"balance\":1.23856,\"step_length\":0.7729},\"running\" : null}";
        while(true)
        {
            ProcessParam(param);
            yield return new WaitForSeconds(2.0f);
        }
    }
}
