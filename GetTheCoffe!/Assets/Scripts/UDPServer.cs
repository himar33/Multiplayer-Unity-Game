using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Events;
using System.IO;

public class UDPServer : MonoBehaviour
{
    public int port;
    public string ip;
    public string serverName;
    public UnityEvent<string> chatEvent;

    private int recv;
    private byte[] data;
    private EndPoint endPoint;
    private IPEndPoint ipep;
    private Thread receiveThread;
    private Socket client;
    private MemoryStream stream;

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

    private void Update()
    {
        if (receiveMessage)
        {
            chatEvent.Invoke(currentText);
            receiveMessage = false;
        }
    }

    public void InitServer()
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

    public void SendString(DataType dataType, object data)
    {
        try
        {
            // Creamos un stream de destino
            MemoryStream stream = new MemoryStream();

            // Creamos un escritor binario y le proporcionamos el stream de destino
            BinaryWriter writer = new BinaryWriter(stream);

            // Escribimos el tipo de dato en el stream
            writer.Write((byte)dataType);

            // Escribimos el objeto serializado en el stream
            writer.Write(data);

            // Obtenemos el array de bytes serializado del stream
            byte[] serializedData = stream.ToArray();

            client.SendTo(serializedData, serializedData.Length, SocketFlags.None, endPoint);
            Debug.Log(">> Server send: " + data.ToString());
        }
        catch (System.Exception err)
        {
            Debug.Log(err.ToString());
        }
    }

    private void ReceiveData()
    {
        bool canReceive = true;

        data = new byte[1024];
        recv = client.ReceiveFrom(data, ref endPoint);

        string welcome = "-Server: Welcome to my test server";
        data = Encoding.ASCII.GetBytes(welcome);
        client.SendTo(data, data.Length, SocketFlags.None, endPoint);

        while (canReceive)
        {
            try
            {
                data = new byte[1024];
                recv = client.ReceiveFrom(data, ref endPoint);

                string text = Encoding.ASCII.GetString(data, 0, recv);

                Debug.Log(">> Server receive: " + text);

                client.SendTo(data, recv, SocketFlags.None, endPoint);

                Debug.Log(">> Server send: " + text);

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
