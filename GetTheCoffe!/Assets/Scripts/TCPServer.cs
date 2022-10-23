using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Events;

public class TCPServer : MonoBehaviour
{
    public int port;
    public string ip;
    public string serverName;
    public UnityEvent<string> chatEvent;

    private Thread receiveThread;
    public Socket client;
    public Socket server;

    private string currentText;
    private bool receiveMessage;

    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("ServerManager");

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

    public void InitServer()
    {
        Debug.Log("TCP Server Initializing");

        port = 9999;
        ip = "127.0.0.1";

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(endPoint);
        server.Listen(10);

        receiveThread = new Thread(new ThreadStart(ReceiveData))
        {
            IsBackground = true
        };
        receiveThread.Start();

        Debug.Log("Sending to " + ip + " : " + port);
        Debug.Log("Testing: nc -lu " + ip + " : " + port);
    }

    public void SendString(string message)
    {
        try
        {
            byte[] data = new byte[1024];
            data = Encoding.ASCII.GetBytes(message);

            if (client != null) client.Send(data, data.Length, SocketFlags.None);
        }
        catch (SocketException e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void SetServerName(string n)
    {
        serverName = n;
    }

    private void ReceiveData()
    {
        bool canReceive = true;

        client = server.Accept();

        while (canReceive)
        {
            try
            {
                byte[] data = new byte[1024];
                int recv = client.Receive(data, data.Length, SocketFlags.None);
                if (recv == 0) break;

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
        if (client != null) client.Close();
        if (server != null) server.Close();
    }
}
