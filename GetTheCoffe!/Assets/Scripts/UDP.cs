using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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

    public static UDP instance;

    #region Deserialize variables
    protected bool desMovement = false;
    protected BinaryReader readerMovement = null;

    protected bool desInstantiate = false;
    protected BinaryReader readerInstantiate = null;
    #endregion

    public void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("UDPManager");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    public void Start()
    {
        if (chatEvent == null)
            chatEvent = new UnityEvent<string>();

        receiveMessage = false;

        if (SceneManager.GetActiveScene().buildIndex == 4)
        {

        }
    }
    public void Update()
    {
        if (receiveMessage)
        {
            chatEvent.Invoke(currentText);
            receiveMessage = false;
        }

        CheckDeserialize("DeserializeMovement", readerMovement, desMovement);
        CheckDeserialize("DeserializeInstantiate", readerInstantiate, desInstantiate);
    }

    private void CheckDeserialize(string methodName, BinaryReader reader, bool methodBool)
    {
        if (methodBool && reader != null)
        {
            SendMessage(methodName, reader);
            reader = null;
            methodBool = false;
        }
    }

    public virtual void JoinServer() { }
    public virtual void SendString(Data data) { }
    public virtual void ReceiveData() { }



    #region Deserialize Methods
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
                desMovement = true;
                readerMovement = reader;
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
            case DataType.Instantiate:
                desInstantiate = true;
                readerInstantiate = reader;
                break;
            default:
                break;
        }
        Debug.Log(">> Received: " + dataType.ToString());
    }
    public void DeserializeInstantiate(BinaryReader reader)
    {
        GameObject go = Resources.Load(reader.ReadString()) as GameObject;

        Vector3 goPos = new Vector3
        {
            x = reader.ReadSingle(),
            y = reader.ReadSingle(),
            z = reader.ReadSingle()
        };

        Instantiate(go, goPos, go.transform.rotation);
    }

    public void DeserializeMovement(BinaryReader reader)
    {
        GameObject go = GameObject.Find(reader.ReadString());
        if (go)
        {
            if (go.TryGetComponent<PlayerController>(out PlayerController player))
            {
                player._input.x = reader.ReadSingle();
                player._input.y = reader.ReadSingle();
                player._input.z = reader.ReadSingle();
            }
            else
            {
                Vector3 newPos = new Vector3();
                newPos.x = reader.ReadSingle();
                newPos.y = reader.ReadSingle();
                newPos.z = reader.ReadSingle();
                go.transform.position = newPos;
            }
        }
    }
    #endregion

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
