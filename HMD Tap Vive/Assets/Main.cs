using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HTC.UnityPlugin.Vive;

public class Main : MonoBehaviour {
    // main
    Server server;
    static string[] MODES = new string[] { "auto log", "predict"};
    static int modeIdx = 0;
    string mode = MODES[modeIdx];
    const float FPS_REFRESH_TIME = 1;
    float fps, fps_frameCount = 0, fps_lastTime = 0;

	public GameObject cam, conL, conR;
	GameObject con;
	HandRole handRole;

    // ui
    const int N_TEXT = 3;
    Text[] text;
    Text text_fps, text_network, text_cnt, text_predict, text_autoLog;
    GameObject button_autoLog, button_predict, homeStuff;

    // log
    XFileWriter logger = null;
    bool logging = false;
    int logCnt = 0;
    
    // predict
    int predictShowFrame = 0;

    // speed test
    float speed = 0.5f;

    void Start() {
		/*cam = GameObject.Find("Camera (eye)"); 
		conL = GameObject.Find("Controller (left)");
		conR = GameObject.Find("Controller (right)");*/
		if (conR == null) {
			con = conL;
			handRole = HandRole.LeftHand;
		} else {
			con = conR;
			handRole = HandRole.RightHand;
		}
		Debug.Log (conL);
		Debug.Log (conR);
		Debug.Log (con);

		text = new Text[N_TEXT];
        for (int i = 0; i < N_TEXT; i++) text[i] = GameObject.Find("Text " + i).GetComponent<Text>();
        text_fps = GameObject.Find("Text fps").GetComponent<Text>();
        text_network = GameObject.Find("Text network").GetComponent<Text>();
        text_autoLog = text[1];
        text_cnt     = text[2];
        text_predict = text[2];

        button_autoLog = GameObject.Find("Button Auto Log");
        button_predict = GameObject.Find("Button Predict");
        
        homeStuff = GameObject.Find("HomeStuff");
        
        ModeChange(0);

        server = new Server();
        server.Start();
    }
   
	void Update () {
        text_fps.text = "fps: " + fps;
        text_network.text = server.info;
        
		float posX = ViveInput.GetAxis(handRole, ControllerAxis.PadX);
		float posY = ViveInput.GetAxis(handRole, ControllerAxis.PadY);
		Vector2 pos = new Vector2(posX, posY);
		text [0].text = posX + " " + posY;

        if (mode == "auto log") {
            text_cnt.text = logCnt.ToString();
            text_autoLog.text = "Auto Log: " + logging.ToString();
			if (ViveInput.GetPressDown(handRole, ControllerButton.Trigger)) {
				logging = !logging;
            }
			if (ViveInput.GetPressDown(handRole, ControllerButton.Pad)) {
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

		if (ViveInput.GetPressDown(handRole, ControllerButton.Pad)) {
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

        if (mode == "auto log") {
            button_autoLog.GetComponent<Image>().color = Color.yellow;
        }

        if (mode == "predict") {
            button_predict.GetComponent<Image>().color = Color.yellow;
        }
    }

    void ModeReset() {
        button_autoLog.GetComponent<Image>().color = Color.white;
        button_predict.GetComponent<Image>().color = Color.white;
        logging = false;
        text_autoLog.text = "";
        homeStuff.SetActive(false);
    }
}
