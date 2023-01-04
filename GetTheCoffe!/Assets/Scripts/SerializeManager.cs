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
    ChangeScene
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
}
