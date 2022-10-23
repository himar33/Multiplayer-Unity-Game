using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Events;

public class UDPClient : MonoBehaviour
{
    public string ip;
    public int port;
    public UnityEvent<string> chatEvent;

    private Thread receiveThread;
    private Socket client;

    private string currentText;
    private bool receiveMessage;

    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("ClientManager");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        if (chatEvent == null)
            chatEvent = new UnityEvent<string>();

        receiveMessage = false;
    }

    public void JoinServer()
    {
        Debug.Log("UDP Client Initializing");

        port = 9999;
        ip = "127.0.0.1";

        client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        receiveThread = new Thread(new ThreadStart(ReceiveData))
        {
            IsBackground = true
        };
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
            EndPoint senderRemote = sender;

            byte[] data = Encoding.ASCII.GetBytes(message);

            client.SendTo(data, data.Length, SocketFlags.None, senderRemote);
        }
        catch (System.Exception err)
        {
            Debug.Log(err.ToString());
        }
    }

    private void ReceiveData()
    {
        bool canReceive = true;

        while (canReceive)
        {
            try
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Parse(ip), port);
                EndPoint senderRemote = sender;

                byte[] data = new byte[1024];
                int recv = client.ReceiveFrom(data, data.Length, SocketFlags.None, ref senderRemote);

                string text = Encoding.ASCII.GetString(data, 0, recv);

                Debug.Log(">> " + text);

                currentText = text;
                receiveMessage = true;
            }
            catch (System.Exception err)
            {
                Debug.Log(err.ToString());
                canReceive = false;
            }
        }
    }

    private void Update()
    {
        if (receiveMessage)
        {
            chatEvent.Invoke(currentText);
            receiveMessage = false;
        }
    }

    private void OnDisable()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (client != null)
        {
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}
