using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public enum DataType
{
    Movement,
    ChatMessage,
    ScoreUpdate,
    ChangeScene
}

public class SerializeManager : MonoBehaviour
{
    public void Serialize(object data)
    {
    }
}
