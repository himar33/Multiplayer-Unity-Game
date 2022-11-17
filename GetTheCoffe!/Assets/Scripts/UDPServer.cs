using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Events;


public class UDPServer : MonoBehaviour
{
    public int port;
    public string ip;
    public string serverName;
    public UnityEvent<string> chatEvent;

    private EndPoint endPoint;
    private Thread receiveThread;
    private Socket client;

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
        Debug.Log("UDP Server Initializing");

        int recv;
        byte[] data = new byte[1024];
        port = 9050;
        ip = "127.0.0.1";

        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ip), port);
        client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client.Bind(ipep);

        endPoint = ipep;

        string welcome = "Welcome to my test server";
        data = Encoding.ASCII.GetBytes(welcome);
        client.SendTo(data, data.Length, SocketFlags.None, endPoint);

        receiveThread = new Thread(new ThreadStart(ReceiveData))
        {
            IsBackground = true
        };
        receiveThread.Start();

        Debug.Log("Sending to " + ip + " : " + port);
        Debug.Log("Testing: nc -lu " + ip + " : " + port);
    }

    public void SetServerName(string n)
    {
        serverName = n;
    }

    private void ReceiveData()
    {
        bool canReceive = true;
        
        while (canReceive)
        {
            try
            {
                byte[] data = new byte[1024];
                int recv = client.ReceiveFrom(data, ref endPoint);

                string text = Encoding.ASCII.GetString(data, 0, recv);

                Debug.Log(">> " + text);

                client.SendTo(data, recv, SocketFlags.None, endPoint);

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
