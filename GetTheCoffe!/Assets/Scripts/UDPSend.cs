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

        byte[] data = new byte[1024];
        port = 9999;
        ip = "127.0.0.1";

        endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client.Bind(endPoint);

        SendString(serverName);

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        Debug.Log("Sending to " + ip + " : " + port);
        Debug.Log("Testing: nc -lu " + ip + " : " + port);
    }

    public void SendString(string message)
    {
        try
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Parse(ip), port);
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
        bool canReceive = true;

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint senderRemote = (EndPoint)sender;

        while (canReceive)
        {
            try
            {
                byte[] data = new byte[1024];
                int recv = client.ReceiveFrom(data, data.Length, SocketFlags.None, ref senderRemote);

                string text = Encoding.ASCII.GetString(data, 0, recv);

                Debug.Log(">> " + text);
            }
            catch (System.Exception err)
            {
                Debug.Log(err.ToString());
                canReceive = false;
            }
        }
    }

    private void OnDisable()
    {
        if(receiveThread != null) receiveThread.Abort();
    }
}
