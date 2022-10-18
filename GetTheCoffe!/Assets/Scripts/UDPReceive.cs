using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPReceive : MonoBehaviour
{
    public string ip;
    public int port;

    private Thread receiveThread;
    private Socket client;

    public void JoinServer()
    {
        Debug.Log("UDPReceive Initializing");

        port = 9999;
        ip = "127.0.0.1";

        client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        SendString("One player enter the server");

        Debug.Log("Sending to " + ip + " : " + port);
        Debug.Log("Test-Sending to this Port: nc -u " + ip + " " + port + "");
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
