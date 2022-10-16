using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPReceive : MonoBehaviour
{
    public int port;

    private IPEndPoint endPoint;
    private Thread receiveThread;
    private Socket client;

    public string lastReceivedUDPPacket = "";
    public string allReceivedUDPPackets = "";

    public void JoinServer()
    {
        Debug.Log("UDPReceive Initializing");

        port = 4231;

        Debug.Log("Sending to 192.168.1.22 : " + port);
        Debug.Log("Test-Sending to this Port: nc -u 192.168.1.22  " + port + "");

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        endPoint = new IPEndPoint(IPAddress.Parse("192.168.1.22"), port);
        client.Bind(endPoint);

        while (true)
        {
            try
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderRemote = (EndPoint)sender;

                byte[] data = new byte[256];
                int recv = client.ReceiveFrom(data, data.Length, SocketFlags.None, ref senderRemote);

                string text = Encoding.ASCII.GetString(data, 0, recv);
                Debug.Log(">> " + text);

                lastReceivedUDPPacket = text;
                allReceivedUDPPackets += text;
            }
            catch (System.Exception err)
            {
                Debug.Log(err.ToString());
            }
        }
    }

    public string GetLatestUDPPacket()
    {
        allReceivedUDPPackets = "";
        return lastReceivedUDPPacket;
    }
}
