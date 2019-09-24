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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    NetBase netBase;
    NetServer netServer;
    NetClient netClient;

    public GameObject cam, camBase, player, canvas, bullet;
    public List<GameObject> weapons;
    int weaponIndex = 0;

    Vector3 bulletPosition;
    Text text_Network, text_Connect, text_Recv, text_Send;

    string platform = "unset";

    void Start () {
        text_Network = GameObject.Find("Text Network").GetComponent<Text>();
        text_Connect = GameObject.Find("Text Connect").transform.Find("Text").GetComponent<Text>();
        text_Recv = GameObject.Find("Text Recv").GetComponent<Text>();
        text_Send = GameObject.Find("Text Send").GetComponent<Text>();

        bulletPosition = bullet.transform.localPosition;
        bullet.transform.position = new Vector3(100, 100, 100);

        Input.gyro.enabled = true;

        switch (Application.platform) {
            case RuntimePlatform.WindowsEditor:
                platform = "windows";
                break;
            case RuntimePlatform.WindowsPlayer:
                platform = "windows";
                break;
            case RuntimePlatform.Android:
                platform = "android";
                break;
            default:
                platform = "error";
                break;
        }

        if (platform == "windows") {
            cam.GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;
        } else if (platform == "android") {
            netBase = netServer = new NetServer();
            netServer.Start();
        }
    }
    
	void Update () {
        if (netBase != null) {
            text_Network.text = netBase.info;
        }
        player.transform.position = cam.transform.position;
        player.transform.rotation = cam.transform.rotation;

        // input
        if (platform == "windows") {
            if (Input.GetKeyDown(KeyCode.H)) {
                canvas.SetActive(!canvas.activeSelf);
            }
            if (Input.GetKeyDown(KeyCode.A)) {
                netClient.Send("message from windows " + new System.Random().Next(100));
            }
            if (Input.GetKeyDown(KeyCode.B)) {
                Fire();
            }
            if (Input.GetKeyDown(KeyCode.C)) {
                Reload();
            }
            if (Input.GetKeyDown(KeyCode.D)) {
                ChangeWeapon();
            }

        } else if (platform == "android") {
            if (OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad, OVRInput.Controller.Touchpad)) {
                Fire();
                netServer.Send("display", "fire");
            }
            if (OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad, OVRInput.Controller.LTrackedRemote)) {
                Reload();
                netServer.Send("display", "reload");
            }
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTrackedRemote)) {
                ChangeWeapon();
                netServer.Send("display", "change_weapon");
            }
        }

        // recv
        if (platform == "windows") {
            if (netClient != null) {
                List<string> recvs = netClient.Recv();
                foreach (string s in recvs) {
                    string[] arr = s.Split(' ');
                    string op = arr[0];
                    switch (op) {
                        case "transform":
                            float px = float.Parse(arr[1]), py = float.Parse(arr[2]), pz = float.Parse(arr[3]);
                            float rx = float.Parse(arr[4]), ry = float.Parse(arr[5]), rz = float.Parse(arr[6]), rw = float.Parse(arr[7]);
                            camBase.transform.position = new Vector3(px, px, pz);
                            camBase.transform.rotation = new Quaternion(rx, ry, rz, rw);
                            break;
                        case "message":
                            text_Recv.text = s.Substring(8);
                            break;
                        case "location":
                            CommonRender(int.Parse(arr[1]));
                            break;
                        case "fire":
                            Fire();
                            break;
                        case "reload":
                            Reload();
                            break;
                        case "change_weapon":
                            ChangeWeapon();
                            break;
                    }
                }

                /*camBase.transform.position = netClient.zp;
                camBase.transform.rotation = netClient.zr;*/
            }
        } else if (platform == "android") {
            Dictionary<NetClient, List<string>> recvs = netServer.Recv();
            foreach (KeyValuePair<NetClient, List<string>> pair in recvs) {
                NetClient netClient = pair.Key;
                List<string> ss = pair.Value;
                foreach (string s in ss) {
                    string[] arr = s.Split(' ');
                    string op = arr[0];
                    switch (op) {
                        case "message":
                            text_Recv.text = s.Substring(8);
                            break;
                        case "rename":
                            netClient.name = arr[1];
                            break;
                        case "location":
                            netServer.Send("display", s);
                            CommonRender(int.Parse(arr[1]));
                            break;
                    }
                }
            }
        }
    }

    private void FixedUpdate() {
        if (platform == "windows") {
            // do nothing
        } else if (platform == "android") {
            Vector3 p = cam.transform.position;
            Quaternion r = cam.transform.rotation;
            netServer.Send("display", "transform " + p.x + " " + p.y + " " + p.z + " " + r.x + " " + r.y + " " + r.z + " " + r.w);

            /*List<byte> b = new List<byte>();
            b.Add(0xa0);
            b.AddRange(BitConverter.GetBytes(p.x));
            b.AddRange(BitConverter.GetBytes(p.y));
            b.AddRange(BitConverter.GetBytes(p.z));
            b.AddRange(BitConverter.GetBytes(r.x));
            b.AddRange(BitConverter.GetBytes(r.y));
            b.AddRange(BitConverter.GetBytes(r.z));
            b.AddRange(BitConverter.GetBytes(r.w));
            netServer.BroadCast(b.ToArray());*/

            Vector3 acc = Input.gyro.userAcceleration;
            Vector3 rot = Input.gyro.rotationRate;
            netServer.Send("backend", "motion " + acc.x + " " + acc.y + " " + acc.z + " " + rot.x + " " + rot.y + " " + rot.z);
        }
    }

    public void Button_Connect_OnClick() {
        string IP = text_Connect.text;
        netBase = netClient = new NetClient();
        netClient.Connect(IP);
    }

    void CommonRender(int idx) {
        switch (idx) {
            case 0:
                GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bullet.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                bullet.transform.position = cam.transform.position;
                Rigidbody rb = bullet.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.velocity = cam.transform.forward * 5;
                text_Send.text = idx.ToString();
                break;
            case 1:
                break;
            case 2:
                break;
        }
    }

    const string MAIN_WEAPON = "MachineGun_08";

    void Fire() {
        GameObject weapon = weapons[weaponIndex];
        if (weapon.name == MAIN_WEAPON) {
            weapon.GetComponent<Animation>().Play("MachineGun_shoot");
            bullet.transform.localPosition = bulletPosition;
            bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 20;
            GameObject.Find("Particle System").GetComponent<ParticleSystem>().Play();
        }
    }

    void Reload() {
        GameObject weapon = weapons[weaponIndex];
        if (weapon.name == MAIN_WEAPON) {
            weapon.GetComponent<Animation>().Play("MachineGun_reload");
        }
    }
    
    void ChangeWeapon() {
        weaponIndex = (weaponIndex + 1) % weapons.Count;
        for (int i = 0; i < weapons.Count; i++) {
            GameObject weapon = weapons[i];
            weapon.SetActive(i == weaponIndex);
        }
    }
}
