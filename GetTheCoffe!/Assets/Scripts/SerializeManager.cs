using System.IO;
using UnityEngine;

public enum DataType
{
    Movement,
    ChatMessage,
    ScoreUpdate,
    ChangeScene,
    Instantiate,
    RemoveObject,
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
        using var stream = new MemoryStream();
        using var writter = new BinaryWriter(stream);
        writter.Write((byte)dataType);
        writter.Write(message);
        return stream.ToArray();
    }
    public static MessageData Deserialize(BinaryReader reader)
    {
        return new MessageData(reader.ReadString());
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
        using var stream = new MemoryStream();
        using var writter = new BinaryWriter(stream);
        writter.Write((byte)dataType);
        writter.Write(position.x);
        writter.Write(position.y);
        writter.Write(position.z);
        writter.Write(objectName);
        return stream.ToArray();
    }
    public static MovementData Deserialize(BinaryReader reader)
    {
        return new MovementData(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()), reader.ReadString());
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
        using var stream = new MemoryStream();
        using var writter = new BinaryWriter(stream);
        writter.Write((byte)dataType);
        writter.Write(sceneIndex);
        return stream.ToArray();
    }
    public static ChangeSceneData Deserialize(BinaryReader reader)
    {
        return new ChangeSceneData(reader.ReadInt32());
    }
}

public class InstantiateData : Data
{
    public string prefabName;
    public string username;
    public Vector3 position;
    public InstantiateData(string _prefabName, Vector3 _position, string _username = "")
    {
        prefabName = _prefabName;
        position = _position;
        username = _username;
        dataType = DataType.Instantiate;
    }
    public override byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writter = new BinaryWriter(stream);
        writter.Write((byte)dataType);
        writter.Write(prefabName);
        writter.Write(position.x);
        writter.Write(position.y);
        writter.Write(position.z);
        writter.Write(username);
        return stream.ToArray();
    }
    public static InstantiateData Deserialize(BinaryReader reader)
    {
        return new InstantiateData(reader.ReadString(), new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()), reader.ReadString());
    }
}

public class RemoveObjectData : Data
{
    public string objectName;
    public RemoveObjectData(string _objectName)
    {
        objectName = _objectName;
        dataType = DataType.RemoveObject;
    }
    public override byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writter = new BinaryWriter(stream);
        writter.Write((byte)dataType);
        writter.Write(objectName);
        return stream.ToArray();
    }
    public static RemoveObjectData Deserialize(BinaryReader reader)
    {
        return new RemoveObjectData(reader.ReadString());
    }
}

public class AddPointData : Data
{
    public AddPointData()
    {
        dataType = DataType.ScoreUpdate;
    }
    public override byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writter = new BinaryWriter(stream);
        writter.Write((byte)dataType);
        return stream.ToArray();
    }
    public static AddPointData Deserialize()
    {
        return new AddPointData();
    }
}