using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Chat : MonoBehaviour
{
    public List<string> messages;
    public TMP_Text text;

    private void Start()
    {
        UDPServer server = FindObjectOfType<UDPServer>();
        if (server)
        {
            server.chatEvent.AddListener(SetText);
        }

        UDPClient client = FindObjectOfType<UDPClient>();
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
        UDPServer server = FindObjectOfType<UDPServer>();
        if (server)
        {
            server.SendString(message);
            return;
        }

        UDPClient client = FindObjectOfType<UDPClient>();
        if (client)
        {
            client.SendString(message);
            return;
        }
    }
}
