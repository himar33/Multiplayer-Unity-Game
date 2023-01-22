using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.Events;
using System.IO;

public class UDPClient : UDP
{
    public override void JoinServer()
    {
        Debug.Log("UDP Client Initializing");

        port = 9050;
        ip = "127.0.0.1";

        ipep = new IPEndPoint(IPAddress.Parse(ip), port);
        client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        data = new byte[1024];
        string welcome = "-Client: Hello, are you there?";
        SendString(new MessageData(welcome));

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        endPoint = sender;

        receiveThread = new Thread(ReceiveData)
        {
            IsBackground = true
        };
        receiveThread.Start();
    }

    public override void SendString(Data data)
    {
        try
        {
            byte[] serializedData = new byte[1024];
            serializedData = data.Serialize();

            client.SendTo(serializedData, serializedData.Length, SocketFlags.None, ipep);
            Debug.Log(">> Client send: " + data.dataType.ToString());
        }
        catch (System.Exception err)
        {
            Debug.Log(err.ToString());
        }
    }

    public override void ReceiveData()
    {
        while (true)
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
                break;
            }
        }
    }
}
