using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Chat : MonoBehaviour
{
    public List<string> messages;
    public TMP_Text text;
    private UDPClient client;

    private void Start()
    {
        client = FindObjectOfType<UDPClient>();
        if (client)
        {
            client.chatEvent.AddListener(SetText);
        }
    }

    public void SetText(string message)
    {
        messages.Add(message);
        text.text += "\n" + message;
    }

    public void AddMessage(string message)
    {
        if (client)
        {
            client.SendString(message);
            return;
        }
    }
}
