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

    private Thread receiveThread;
    private IPEndPoint endPoint;
    private Socket client;

    public void InitServer()
    {
        Debug.Log("UDPSend Initializing");

        port = 4231;
        ip = "192.168.1.22";

        endPoint = new IPEndPoint(IPAddress.Any, port);
        client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client.Bind(endPoint);

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

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
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)(sender);

            byte[] data = Encoding.ASCII.GetBytes(message);

            client.SendTo(data, data.Length, SocketFlags.None, senderRemote);
            Debug.Log(message);
        }
        catch (System.Exception err)
        {
            Debug.Log(err.ToString());
        }
    }

    public void SetServerName(string n)
    {
        serverName = n;
    }

    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderRemote = (EndPoint)sender;

                byte[] data = new byte[1024];
                int recv = client.ReceiveFrom(data, data.Length, SocketFlags.None, ref senderRemote);

                string text = Encoding.ASCII.GetString(data, 0, recv);

                client.SendTo(data, recv, SocketFlags.None, senderRemote);

                Debug.Log(">> " + text);
            }
            catch (System.Exception err)
            {
                Debug.Log(err.ToString());
            }
        }
    }
}
