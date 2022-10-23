using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Events;
public class TCPClient : MonoBehaviour
{
    public string ip;
    public int port;
    public UnityEvent<string> chatEvent;

    private Thread receiveThread;
    private Socket server;

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
        Debug.Log("TCP Client Initializing");

        port = 9999;
        ip = "127.0.0.1";
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);

        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            server.Connect(ipep);
        }
        catch (SocketException e)
        {
            Debug.LogError(e.ToString());
            return;
        }

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
            byte[] data = Encoding.ASCII.GetBytes(message);

            server.Send(data, data.Length, SocketFlags.None);
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
                byte[] data = new byte[1024];
                int recv = server.Receive(data, data.Length, SocketFlags.None);

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
        if (server != null) server.Close();
    }
}
