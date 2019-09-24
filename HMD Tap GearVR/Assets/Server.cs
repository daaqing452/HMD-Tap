using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server {
    public string IP = null;
    public const int PORT = 10441;
    TcpListener listener;
    TcpClient client;
    StreamReader reader;
    StreamWriter writer;

    List<string> recvs = new List<string>();
    object recv_mutex = new object();
    public string info = "not set up";

    public Server() {
        string hostName = Dns.GetHostName();
        IPAddress[] addressList = Dns.GetHostAddresses(hostName);
        foreach (IPAddress ip in addressList) {
            if (ip.ToString().IndexOf("192.168.") != -1) {
                IP = ip.ToString();
                break;
            }
            if (ip.ToString().Substring(0, 3) != "127" && ip.ToString().Split('.').Length == 4) IP = ip.ToString();
        }
        info = "IP: " + IP + ":" + PORT;
        listener = new TcpListener(IPAddress.Parse(IP), PORT);
    }

    public void Start() {
        Thread listenThread = new Thread(ListenThread);
        listenThread.Start();
    }

    void ListenThread() {
        listener.Start();
        while (true) {
            TcpClient client = listener.AcceptTcpClient();
            Thread receiveThread = new Thread(ReceiveThread);
            receiveThread.Start(client);
        }
    }

    void ReceiveThread(object clientObject) {
        client = (TcpClient)clientObject;
        info = "client in (" + client.Client.RemoteEndPoint.ToString() + ")";
        reader = new StreamReader(client.GetStream());
        writer = new StreamWriter(client.GetStream());
        while (true) {
            string line;
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
        writer.Close();
        reader.Close();
        writer = null;
        reader = null;
        info = "IP: " + IP + ":" + PORT;
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
}
