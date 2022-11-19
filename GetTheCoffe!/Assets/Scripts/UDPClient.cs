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

    private int recv;
    private byte[] data;
    private EndPoint endPoint;
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

        port = 9050;
        ip = "127.0.0.1";

        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ip), port);
        client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        endPoint = sender;

        data = new byte[1024];
        string welcome = "-Client: Hello, are you there?";
        data = Encoding.ASCII.GetBytes(welcome);
        client.SendTo(data, data.Length, SocketFlags.None, ipep);

        receiveThread = new Thread(new ThreadStart(ReceiveData))
        {
            IsBackground = true
        };
        receiveThread.Start();
    }

    public void SendString(string message)
    {
        try
        {
            data = new byte[1024];
            data = Encoding.ASCII.GetBytes("-Client: " + message);

            client.SendTo(data, data.Length, SocketFlags.None, endPoint);
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
                data = new byte[1024];
                recv = client.ReceiveFrom(data, ref endPoint);

                string text = Encoding.ASCII.GetString(data, 0, recv);

                Debug.Log(">> Client: " + text);

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
