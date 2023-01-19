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
    protected bool canDesMovement = false;
    protected MovementData movementData;

    protected bool canDesInstantiate = false;
    protected InstantiateData instantiateData;

    protected bool canChangeScene = false;
    protected ChangeSceneData sceneData;
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 4)
        {
            Vector3 newPlayerPos = FindObjectOfType<MazeSpawner>().GetMazePosition();
            newPlayerPos.y += 0.3f;

            GameObject playerPrefab = Resources.Load("Player") as GameObject;
            GameObject newPlayer = Instantiate(playerPrefab, newPlayerPos, playerPrefab.transform.rotation);
            newPlayer.GetComponent<PlayerController>().isMainPlayer = true;

            Debug.Log(newPlayer.name);
            if (UDP.instance is UDPServer) newPlayer.name = "ServerPlayer";
            else newPlayer.name = "ClientPlayer";

            Debug.Log("Player created at: " + newPlayer.transform.position);

            SendString(new InstantiateData("Player", newPlayerPos));
        }
    }

    public void Start()
    {
        if (chatEvent == null)
            chatEvent = new UnityEvent<string>();

        receiveMessage = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public void Update()
    {
        if (receiveMessage)
        {
            chatEvent.Invoke(currentText);
            receiveMessage = false;
        }

        CheckDeserialize("DeserializeScene", sceneData, canChangeScene);
        CheckDeserialize("DeserializeInstantiate", instantiateData, canDesInstantiate);
        CheckDeserialize("DeserializeMovement", movementData, canDesMovement);
    }

    public void CheckDeserialize(string methodName, Data data, bool methodBool)
    {
        if (methodBool)
        {
            SendMessage(methodName, data);
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
                canDesMovement = true;
                movementData = MovementData.Deserialize(reader);
                break;
            case DataType.ChatMessage:
                string text = reader.ReadString();
                currentText = text;
                receiveMessage = true;
                break;
            case DataType.ScoreUpdate:
                break;
            case DataType.ChangeScene:
                canChangeScene = true;
                sceneData = ChangeSceneData.Deserialize(reader);
                break;
            case DataType.Instantiate:
                canDesInstantiate = true;
                instantiateData = InstantiateData.Deserialize(reader);
                break;
            default:
                break;
        }
        Debug.Log(">> Received: " + dataType.ToString());
    }
    public void DeserializeInstantiate(InstantiateData reader)
    {
        if (SceneManager.GetActiveScene().buildIndex == 4)
        {
            GameObject go = Resources.Load(reader.prefabName) as GameObject;

            Vector3 goPos = reader.position;

            GameObject newPlayer = Instantiate(go, goPos, go.transform.rotation);

            if (newPlayer.name == "Player(Clone)")
            {
                if (UDP.instance is UDPServer) newPlayer.name = "ClientPlayer";
                else newPlayer.name = "ServerPlayer";
            }
            canDesInstantiate = false;
        }
    }

    public void DeserializeMovement(MovementData reader)
    {
        GameObject go = GameObject.Find(reader.objectName);
        if (go)
        {
            if (go.TryGetComponent<PlayerController>(out PlayerController player))
                player._input = reader.position;
            else
                go.transform.position = reader.position;

            canDesMovement = false;
        }
    }

    public void DeserializeScene(ChangeSceneData reader)
    {
        ChangeScene.GoToScene(reader.sceneIndex);
        canChangeScene = false;
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
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
