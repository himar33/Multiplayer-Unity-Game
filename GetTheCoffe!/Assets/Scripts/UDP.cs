using Cinemachine;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UDP : MonoBehaviour
{
    public string ip;
    public int port;
    public UnityEvent<string> chatEvent;

    public string username;

    [System.Serializable]
    public class VariableTMPPair
    {
        public int variable;
        public TMP_Text textMeshPro;
    }

    public List<VariableTMPPair> variableTMPPairs;

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
    protected List<MovementData> movementData = new List<MovementData>();
    protected List<InstantiateData> instantiateData = new List<InstantiateData>();
    protected List<ChangeSceneData> sceneData = new List<ChangeSceneData>();
    protected List<RemoveObjectData> removeData = new List<RemoveObjectData>();
    protected List<AddPointData> addPointData = new List<AddPointData>();
    #endregion

    public void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("UDPManager");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 4) return;

        MazeSpawner maze = FindObjectOfType<MazeSpawner>();
        Vector3 newPlayerPos = maze.GetMazePosition();
        newPlayerPos.y += 0.3f;
        GameObject playerPrefab = Resources.Load("Player") as GameObject;
        GameObject newPlayer = Instantiate(playerPrefab, newPlayerPos, playerPrefab.transform.rotation);

        PlayerController playerController = newPlayer.GetComponent<PlayerController>();
        playerController.isMainPlayer = true;
        playerController.SetUsernameText(username);

        Transform userpoints = GameObject.FindGameObjectWithTag("Userpoints").transform;
        userpoints.parent.GetComponent<TMP_Text>().text = username;

        if (variableTMPPairs.Count <= 0)
        {
            variableTMPPairs = new List<VariableTMPPair>();
            variableTMPPairs.Add(new VariableTMPPair { variable = 0, textMeshPro = userpoints.GetComponent<TMP_Text>() });
            variableTMPPairs.Add(new VariableTMPPair { variable = 0, textMeshPro = GameObject.FindGameObjectWithTag("Enemypoints").GetComponent<TMP_Text>() });
        }
        else
        {
            variableTMPPairs[0].textMeshPro = userpoints.GetComponent<TMP_Text>();
            variableTMPPairs[0].textMeshPro.text = variableTMPPairs[0].variable.ToString();

            variableTMPPairs[1].textMeshPro = GameObject.FindGameObjectWithTag("Enemypoints").GetComponent<TMP_Text>();
            variableTMPPairs[1].textMeshPro.text = variableTMPPairs[1].variable.ToString();
        }

        if (UDP.instance is UDPServer)
        {
            GameObject coffe = maze.GenerateCoffe();
            SendString(new InstantiateData("Coffe", coffe.transform.position));
            newPlayer.name = "ServerPlayer";
        }
        else
        {
            newPlayer.name = "ClientPlayer";
        }

        CinemachineVirtualCamera cm = FindObjectOfType<CinemachineVirtualCamera>();
        cm.Follow = newPlayer.transform;
        SendString(new InstantiateData("Player", newPlayerPos, username));
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

        if (sceneData.Count > 0)
        {
            for (int i = sceneData.Count - 1; i >= 0; i--)
            {
                DeserializeScene(sceneData[i]);
                sceneData.RemoveAt(i);
            }
        }
        if (instantiateData.Count > 0)
        {
            for (int i = instantiateData.Count - 1; i >= 0; i--)
            {
                DeserializeInstantiate(instantiateData[i]);
                instantiateData.RemoveAt(i);
            }
        }
        if (movementData.Count > 0)
        {
            for (int i = movementData.Count - 1; i >= 0; i--)
            {
                DeserializeMovement(movementData[i]);
                movementData.RemoveAt(i);
            }
        }
        if (removeData.Count > 0)
        {
            for (int i = removeData.Count - 1; i >= 0; i--)
            {
                DeserializeRemoveObject(removeData[i]);
                removeData.RemoveAt(i);
            }
        }
        if (addPointData.Count > 0)
        {
            for (int i = addPointData.Count - 1; i >= 0; i--)
            {
                DeserializeAddPoint();
                addPointData.RemoveAt(i);
            }
        }
    }

    public virtual void JoinServer() { }
    public virtual void SendString(Data data) { }
    public virtual void ReceiveData() { }



    #region Deserialize Methods
    public void Deserialize(byte[] bytes)
    {
        using var stream = new MemoryStream();
        stream.Write(bytes, 0, bytes.Length);
        using var reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        DataType dataType = (DataType)reader.ReadByte();
        switch (dataType)
        {
            case DataType.Movement:
                movementData.Add(MovementData.Deserialize(reader));
                break;
            case DataType.ChatMessage:
                string text = reader.ReadString();
                currentText = text;
                receiveMessage = true;
                break;
            case DataType.ScoreUpdate:
                addPointData.Add(AddPointData.Deserialize());
                break;
            case DataType.ChangeScene:
                sceneData.Add(ChangeSceneData.Deserialize(reader));
                break;
            case DataType.Instantiate:
                instantiateData.Add(InstantiateData.Deserialize(reader));
                break;
            case DataType.RemoveObject:
                removeData.Add(RemoveObjectData.Deserialize(reader));
                break;
            default:
                break;
        }
        Debug.Log(">> Received: " + dataType.ToString());
    }

    public void DeserializeInstantiate(InstantiateData reader)
    {
        if (SceneManager.GetActiveScene().buildIndex != 4) return;

        GameObject go = Resources.Load(reader.prefabName) as GameObject;
        Vector3 goPos = reader.position;

        GameObject newPlayer = Instantiate(go, goPos, go.transform.rotation);

        if (newPlayer.name == "Player(Clone)")
        {
            newPlayer.name = UDP.instance is UDPServer ? "ClientPlayer" : "ServerPlayer";
            PlayerController player = newPlayer.GetComponent<PlayerController>();
            player.SetUsernameText(reader.username);
            GameObject.FindGameObjectWithTag("Enemypoints").transform.parent.GetComponent<TMP_Text>().text = reader.username;
        }
    }

    public void DeserializeMovement(MovementData reader)
    {
        GameObject go = GameObject.Find(reader.objectName);
        if (go == null) return;

        if (go.TryGetComponent<PlayerController>(out var player))
        {
            player._input = reader.position;
        }
        else
        {
            go.transform.position = reader.position;
        }
    }

    public void DeserializeScene(ChangeSceneData reader)
    {
        ChangeScene.GoToScene(reader.sceneIndex);
    }

    public void DeserializeRemoveObject(RemoveObjectData reader)
    {
        GameObject go;
        if (go = GameObject.Find(reader.objectName))
        {
            Destroy(go);
        }
    }

    public void DeserializeAddPoint()
    {
        variableTMPPairs[0].variable += 1;
        variableTMPPairs[0].textMeshPro.text = variableTMPPairs[0].variable.ToString();

        ChangeScene.GoToScene(4);
        SendString(new ChangeSceneData(4));
    }
    #endregion

    public void SetUsername(string name)
    {
        username = name;
    }

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
