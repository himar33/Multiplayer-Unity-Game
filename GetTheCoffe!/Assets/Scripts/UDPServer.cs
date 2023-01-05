using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.Events;
using System.IO;

public class UDPServer : UDP
{
    public string serverName;

    public override void JoinServer()
    {
        Debug.Log("UDP Server Initializing");

        port = 9050;
        ip = "127.0.0.1";

        ipep = new IPEndPoint(IPAddress.Any, port);
        client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client.Bind(ipep);

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        endPoint = sender;

        receiveThread = new Thread(new ThreadStart(ReceiveData))
        {
            IsBackground = true
        };
        receiveThread.Start();
    }

    public void SetServerName(string n)
    {
        serverName = n;
    }

    public override void SendString(Data data)
    {
        try
        {
            byte[] serializedData = new byte[1024];
            serializedData = data.Serialize();

            client.SendTo(serializedData, serializedData.Length, SocketFlags.None, endPoint);
            Debug.Log(">> Server send: " + data.dataType.ToString());
        }
        catch (System.Exception err)
        {
            Debug.Log(err.ToString());
        }
    }

    public override void ReceiveData()
    {
        bool canReceive = true;

        data = new byte[1024];
        recv = client.ReceiveFrom(data, ref endPoint);

        string welcome = "-Server: Welcome to my test server";
        SendString(new MessageData(welcome));

        while (canReceive)
        {
            try
            {
                data = new byte[1024];
                recv = client.ReceiveFrom(data, ref endPoint);

                Deserialize(data);
            }
            catch (System.Exception err)
            {
                Debug.Log(err.ToString());
                canReceive = false;
            }
        }
    }
}
