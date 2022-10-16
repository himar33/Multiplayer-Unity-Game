using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MyBox;

public class UDPSend : MonoBehaviour
{
    public int port;
    public string ip;
    public string serverName;

    private IPEndPoint endPoint;
    private Socket client;

    public void InitServer()
    {
        Debug.Log("UDPSend Initializing");

        port = 4231;
        ip = "192.168.1.22";

        client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        endPoint = new IPEndPoint(IPAddress.Parse(ip), port);

        SendString(serverName);

        Debug.Log("Sending to " + ip + " : " + port);
        Debug.Log("Testing: nc -lu " + ip + " : " + port);
    }

#if UNITY_EDITOR
    [ButtonMethod]
    public void TestSend()
    {
        SendString("TESTING MESSAGE");
    }
#endif

    public void SendString(string message)
    {
        try
        {
            byte[] data = Encoding.ASCII.GetBytes(message);

            client.SendTo(data, data.Length, SocketFlags.None, endPoint);
        }
        catch (System.Exception err)
        {
            print(err.ToString());
        }
    }

    public void SetServerName(string n)
    {
        serverName = n;
    }
}
