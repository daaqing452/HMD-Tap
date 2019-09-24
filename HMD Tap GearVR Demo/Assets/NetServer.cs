using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class NetServer : NetBase {
    public string IP = null;
    TcpListener listener;
    List<NetClient> netClientList = new List<NetClient>();

    public NetServer() {
        string hostName = Dns.GetHostName();
        IPAddress[] addressList = Dns.GetHostAddresses(hostName);
        foreach (IPAddress ip in addressList) {
            if (ip.ToString().IndexOf("192.168.") != -1) {
                IP = ip.ToString();
                break;
            }
            if (ip.ToString().Substring(0, 3) != "127" && ip.ToString().Split('.').Length == 4) IP = ip.ToString();
        }
    }

    public void Start() {
        listener = new TcpListener(IPAddress.Parse(IP), PORT);
        info = "IP: " + IP + ":" + PORT;
        Thread listenThread = new Thread(ListenThread);
        listenThread.Start();
    }

    public void Send(string name, string s) {
        foreach (NetClient netClient in netClientList) {
            if (netClient.name == name) {
                netClient.Send(s);
                break;
            }
        }
    }

    public void BroadCast(string s) {
        foreach (NetClient netClient in netClientList) {
            netClient.Send(s);
        }
    }

    /*public void BroadCast(byte[] b) {
        foreach (NetClient netClient in netClientList) {
            netClient.Send(b);
        }
    }*/

    public Dictionary<NetClient, List<string>> Recv() {
        Dictionary<NetClient, List<string>> result = new Dictionary<NetClient, List<string>>();
        foreach (NetClient netClient in netClientList) {
            List<string> subResult = netClient.Recv();
            if (subResult.Count == 0) continue;
            result[netClient] = subResult;
        }
        return result;
    }

    public void Disconnected(NetClient netClient) {
        info = "leave";
        netClientList.Remove(netClient);
    }

    void ListenThread() {
        listener.Start();
        while (true) {
            TcpClient client = listener.AcceptTcpClient();
            NetClient netClient = new NetClient();
            info = "come " + client.Client.RemoteEndPoint;
            netClient.Communicate(this, client);
            netClientList.Add(netClient);
        }
    }
}