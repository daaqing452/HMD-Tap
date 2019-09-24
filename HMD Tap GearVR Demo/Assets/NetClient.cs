using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetClient : NetBase {
    NetServer netServer;
    TcpClient client;
    StreamWriter writer;
    List<string> recvs = new List<string>();
    object recv_mutex = new object();

    public void Connect(string IP) {
        client = new TcpClient(IP, PORT);
        if (client.Connected) {
            info = "connect to " + IP + ":" + PORT;
            Thread receiveThread = new Thread(ReceiveThread);
            receiveThread.Start();
            writer = new StreamWriter(client.GetStream());
            Send("rename display");
        } else {
            info = "connect failed";
        }
    }

    public void Communicate(NetServer netServer, TcpClient client) {
        this.netServer = netServer;
        this.client = client;
        writer = new StreamWriter(client.GetStream());
        Thread receiveThread = new Thread(ReceiveThread);
        receiveThread.Start();
    }

    public List<string> Recv() {
        List<string> result = new List<string>();
        lock (recv_mutex) {
            for (int i = 0; i < recvs.Count; i++) {
                result.Add(recvs[i]);
            }
            recvs.Clear();
        }
        return result;
    }

    public void Send(string s) {
        if (writer != null) {
            writer.Write(s + "\n");
            writer.Flush();
        }
    }

    /*public void Send(byte[] b) {
        if (writer != null) {
            client.GetStream().Write(b, 0, b.Length);
            client.GetStream().Flush();
        }
    }*/
    
    void ReceiveThread() {
        StreamReader reader = new StreamReader(client.GetStream());
        while (true) {
            string line;
            char[] c = new char[1024];
            try {
                line = reader.ReadLine();
                if (line == null) break;
            } catch {
                break;
            }
            lock (recv_mutex) {
                recvs.Add(line);
            }
        }
        reader.Close();
        reader = null;
        info = "disconnected";
        netServer.Disconnected(this);
    }

    /*public Vector3 zp;
    public Quaternion zr;
    byte[] b = new byte[1024];
    int cnt = 0;
    void ReceiveThread() {
        StreamReader reader = new StreamReader(client.GetStream());
        while (true) {
            try {
                client.GetStream().Read(b, 0, 1);
                if (b[0] == 0xa0) {
                    client.GetStream().Read(b, 0, 28);
                    Vector3 p;
                    p.x = BitConverter.ToSingle(b, 0);
                    p.y = BitConverter.ToSingle(b, 4);
                    p.z = BitConverter.ToSingle(b, 8);
                    Quaternion r;
                    r.x = BitConverter.ToSingle(b, 12);
                    r.y = BitConverter.ToSingle(b, 16);
                    r.z = BitConverter.ToSingle(b, 20);
                    r.w = BitConverter.ToSingle(b, 24);
                    zp = p;
                    zr = r;
                    cnt++;
                    //Debug.Log(cnt);
                }
            } catch {
                break;
            }
        }
        reader.Close();
        reader = null;
        info = "disconnected";
        netServer.Disconnected(this);
    }*/
}
