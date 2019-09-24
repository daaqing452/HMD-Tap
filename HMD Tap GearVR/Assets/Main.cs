/*
 *  Object obj = OVRInput.Get(Event, Device)
 * 
 *  Event:
 *      OVRInput.Axis2D.PrimaryTouchpad         x,y on touchpad
 *      OVRInput.Touch.PrimaryTouchpad          touch on touchpad
 *      OVRInput.Button.PrimaryTouchpad         press touchpad
 *      OVRInput.Button.PrimaryIndexTrigger     press trigger
 *  
 *  Device:
 *      OVRInput.Controller.Touchpad            touchpad on hmd
 *      OVRInput.Controller.LTrackedRemote      controller (left mode)
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {
    // main
    Server server;
    static string[] MODES = new string[] { "auto log", "press log", "predict"};
    static int modeIdx = 0;
    string mode = MODES[modeIdx];
    const float FPS_REFRESH_TIME = 1;
    float fps, fps_frameCount = 0, fps_lastTime = 0;

    // ui
    const int N_TEXT = 3;
    Text[] text;
    Text text_fps, text_network, text_cnt, text_predict, text_autoLog;
    GameObject button_autoLog, button_pressLog, button_predict, button_demo;
    GameObject loggingFront, loggingBack, homeStuff;

    // log
    XFileWriter logger = null;
    bool logging = false;
    int logCnt = 0;

    // press log
    const float LOG_TIME = 2.0f;
    RectTransform loggingFront_Transform;
    float logStartTime = -1, loggingBack_Width;

    // predict
    int predictShowFrame = 0;

    // speed test
    float speed = 0.5f;

    void Start() {
        text = new Text[N_TEXT];
        for (int i = 0; i < N_TEXT; i++) text[i] = GameObject.Find("Text " + i).GetComponent<Text>();
        text_fps = GameObject.Find("Text fps").GetComponent<Text>();
        text_network = GameObject.Find("Text network").GetComponent<Text>();
        text_autoLog = text[1];
        text_cnt     = text[2];
        text_predict = text[2];

        button_autoLog = GameObject.Find("Button Auto Log");
        button_pressLog = GameObject.Find("Button Press Log");
        button_predict = GameObject.Find("Button Predict");
        button_demo = GameObject.Find("Button Demo");

        loggingFront = GameObject.Find("LoggingFront");
        loggingBack = GameObject.Find("LoggingBack");
        logger = new XFileWriter("acc");
        loggingFront_Transform = loggingFront.GetComponent<RectTransform>();
        loggingBack_Width = loggingBack.GetComponent<RectTransform>().rect.width;

        homeStuff = GameObject.Find("HomeStuff");

        Input.gyro.enabled = true;
        ModeChange(0);

        server = new Server();
        server.Start();
    }
   
    void Update () {
        text_fps.text = "fps: " + fps;
        text_network.text = server.info;
        Vector2 pos = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad, OVRInput.Controller.LTrackedRemote);

        if (mode == "press log") {
            text_cnt.text = "Count: " + logCnt.ToString();
            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTrackedRemote)) {
                float now = Time.realtimeSinceStartup;
                if (logStartTime < 0) {
                    logStartTime = now;
                    logger.WriteLine("start");
                    logCnt++;
                }
                if (now - logStartTime <= LOG_TIME) {
                    logging = true;
                    float progress = (now - logStartTime) / LOG_TIME * loggingBack_Width;
                    loggingFront_Transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, progress);
                } else {
                    logging = false;
                    loggingFront_Transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
                }
            } else {
                logging = false;
                loggingFront_Transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
                logStartTime = -1;
                logger.Flush();
            }
        }

        if (mode == "auto log") {
            text_cnt.text = logCnt.ToString();
            text_autoLog.text = "Auto Log: " + logging.ToString();
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTrackedRemote)) {
                logging = !logging;
            }
            if (OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad, OVRInput.Controller.LTrackedRemote)) {
                if (pos.y <= -0.85) homeStuff.SetActive(!homeStuff.activeSelf);
            }
        }

        if (mode == "predict") {
            predictShowFrame = Mathf.Max(predictShowFrame - 1, 0);
            if (predictShowFrame == 0) text_predict.text = "";
            List<string> recvs = server.Recv();
            foreach (string s in recvs) {
                string[] arr = s.Split(' ');
                string tag = arr[0];
                if (tag == "result") {
                    string show = "";
                    switch (int.Parse(arr[1])) {
                        case 0: show = "left"; break;
                        case 1: show = "right"; break;
                        case 2: show = "upper-left"; break;
                        case 3: show = "upper-right"; break;
                        case 4: show = "lower-left"; break;
                        case 5: show = "lower-right"; break;
                        case 6: show = "front-left"; break;
                        case 7: show = "front-right"; break;
                    }
                    predictShowFrame = 30;
                    text_predict.text = show;
                }
            }
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad, OVRInput.Controller.LTrackedRemote)) {
            if (pos.x <= -0.85) ModeChange(-1);
            if (pos.x >= 0.85) ModeChange(1);
        }
    }

    private void FixedUpdate() {
        GetFps();
        string label = "frame";
        Vector3 acc = Input.gyro.userAcceleration;
        Vector3 rot = Input.gyro.rotationRate;
        string s = label + " " + acc.x + " " + acc.y + " " + acc.z + " " + rot.x + " " + rot.y + " " + rot.z;
        if (logging) logger.WriteLine(s);
        if (mode == "predict") server.Send(s);
    }

    void GetFps() {
        fps_frameCount += 1;
        float fps_nowTime = Time.realtimeSinceStartup;
        if (fps_nowTime - fps_lastTime > FPS_REFRESH_TIME) {
            fps = fps_frameCount / (fps_nowTime - fps_lastTime);
            fps_frameCount = 0;
            fps_lastTime = fps_nowTime;
        }
    }

    void ModeChange(int d) {
        modeIdx = (modeIdx + d + MODES.Length) % MODES.Length;
        mode = MODES[modeIdx];
        ModeReset();

        if (mode == "press log") {
            button_pressLog.GetComponent<Image>().color = Color.yellow;
            loggingFront.SetActive(true);
            loggingBack.SetActive(true);
        }

        if (mode == "auto log") {
            button_autoLog.GetComponent<Image>().color = Color.yellow;
        }

        if (mode == "predict") {
            button_predict.GetComponent<Image>().color = Color.yellow;
        }
    }

    void ModeReset() {
        button_pressLog.GetComponent<Image>().color = Color.white;
        button_autoLog.GetComponent<Image>().color = Color.white;
        button_predict.GetComponent<Image>().color = Color.white;
        button_demo.GetComponent<Image>().color = Color.white;
        loggingFront.SetActive(false);
        loggingBack.SetActive(false);
        logging = false;
        text_autoLog.text = "";
        homeStuff.SetActive(false);
    }
}
