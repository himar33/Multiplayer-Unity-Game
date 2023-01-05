using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class UDP : MonoBehaviour
{
    public string ip;
    public int port;
    public UnityEvent<string> chatEvent;

    protected int recv;
    protected byte[] data;
    protected EndPoint endPoint;
    protected IPEndPoint ipep;
    protected Thread receiveThread;
    protected Socket client;

    protected string currentText;
    protected bool receiveMessage;

    public void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("UDPManager");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }
    public void Start()
    {
        if (chatEvent == null)
            chatEvent = new UnityEvent<string>();

        receiveMessage = false;
    }
    public void Update()
    {
        if (receiveMessage)
        {
            chatEvent.Invoke(currentText);
            receiveMessage = false;
        }
    }

    public virtual void JoinServer() { }
    public virtual void SendString(Data data) { }
    public virtual void ReceiveData() { }

    public void Deserialize(byte[] bytes)
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
        Debug.Log(">> Send: " + dataType.ToString());
    }

    public void OnDisable()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (client != null)
        {
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }

}
