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

    public void SendString(Data data)
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

    private void ReceiveData()
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

    private void Deserialize(byte[] bytes)
    {
        MemoryStream stream = new MemoryStream();
        stream.Write(bytes, 0, bytes.Length);
        BinaryReader reader = new BinaryReader(stream);

        stream.Seek(0, SeekOrigin.Begin);
        DataType dataType = (DataType)reader.ReadByte();
        switch (dataType)
        {
            case DataType.Movement:
                GameObject go = GameObject.Find(reader.ReadString());
                if (go)
                {
                    Vector3 newPos = new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                    go.transform.position = newPos;
                }
                break;
            case DataType.ChatMessage:
                string text = reader.ReadString();
                currentText = text;
                receiveMessage = true;
                break;
            case DataType.ScoreUpdate:
                break;
            case DataType.ChangeScene:
                ChangeScene.GoToScene(reader.ReadInt32());
                break;
            default:
                break;
        }
        Debug.Log(">> Server send: " + dataType.ToString());
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
