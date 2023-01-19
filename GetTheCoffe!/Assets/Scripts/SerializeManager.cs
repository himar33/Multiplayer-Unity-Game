using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public enum DataType
{
    Movement,
    ChatMessage,
    ScoreUpdate,
    ChangeScene,
    Instantiate
}

public abstract class Data
{
    public DataType dataType;
    public abstract byte[] Serialize();
}

public class MessageData : Data
{
    public string message;
    public MessageData(string _message = "")
    {
        message = _message;
        dataType = DataType.ChatMessage;
    }
    public override byte[] Serialize()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writter = new BinaryWriter(stream);
        writter.Write((byte)dataType);
        writter.Write(message);
        return stream.ToArray();
    }
    public static MessageData Deserialize(BinaryReader reader)
    {
        MessageData newMessage = new MessageData();
        newMessage.message = reader.ReadString();
        return newMessage;
    }
}

public class MovementData : Data
{
    public Vector3 position;
    public string objectName;
    public MovementData(Vector3 _position, string _objectName)
    {
        position = _position;
        objectName = _objectName;
        dataType = DataType.Movement;
    }
    public override byte[] Serialize()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writter = new BinaryWriter(stream);
        writter.Write((byte)dataType);
        writter.Write(objectName);
        writter.Write(position.x);
        writter.Write(position.y);
        writter.Write(position.z);
        return stream.ToArray();
    }
    public static MovementData Deserialize(BinaryReader reader)
    {
        MovementData newMovement = new MovementData(Vector3.zero, "");
        newMovement.objectName = reader.ReadString();
        newMovement.position.x = reader.ReadSingle();
        newMovement.position.y = reader.ReadSingle();
        newMovement.position.z = reader.ReadSingle();
        return newMovement;
    }
}

public class ChangeSceneData : Data
{
    public int sceneIndex;
    public ChangeSceneData(int _sceneIndex)
    {
        sceneIndex = _sceneIndex;
        dataType = DataType.ChangeScene;
    }
    public override byte[] Serialize()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writter = new BinaryWriter(stream);
        writter.Write((byte)dataType);
        writter.Write(sceneIndex);
        return stream.ToArray();
    }
    public static ChangeSceneData Deserialize(BinaryReader reader)
    {
        ChangeSceneData newSceneData = new ChangeSceneData(0);
        newSceneData.sceneIndex = reader.ReadInt32();
        return newSceneData;
    }
}

public class InstantiateData : Data
{
    public string prefabName;
    public Vector3 position;
    public InstantiateData(string _prefabName, Vector3 _position)
    {
        prefabName = _prefabName;
        position = _position;
        dataType = DataType.Instantiate;
    }
    public override byte[] Serialize()
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writter = new BinaryWriter(stream);
        writter.Write((byte)dataType);
        writter.Write(prefabName);
        writter.Write(position.x);
        writter.Write(position.y);
        writter.Write(position.z);
        return stream.ToArray();
    }
    public static InstantiateData Deserialize(BinaryReader reader)
    {
        InstantiateData newInstance = new InstantiateData("", Vector3.zero);
        newInstance.prefabName = reader.ReadString();
        newInstance.position.x = reader.ReadSingle();
        newInstance.position.y = reader.ReadSingle();
        newInstance.position.z = reader.ReadSingle();
        return newInstance;
    }
}