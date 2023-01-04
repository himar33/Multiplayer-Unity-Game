using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Chat : MonoBehaviour
{
    public GameObject startButton;
    public List<string> messages;
    public TMP_Text text;

    private UDPServer server;
    private UDPClient client;

    private void Start()
    {
        server = FindObjectOfType<UDPServer>();
        if (server)
        {
            server.chatEvent.AddListener(SetText);
            startButton.SetActive(true);
        }

        client = FindObjectOfType<UDPClient>();
        if (client)
        {
            client.chatEvent.AddListener(SetText);
            startButton.SetActive(false);
        }
    }

    public void SetText(string message)
    {
        messages.Add(message);
        text.text += "\n" + message;
    }

    public void AddMessage(string message)
    {
        if (server)
        {
            server.SendString(new MessageData(message));
            return;
        }

        if (client)
        {
            client.SendString(new MessageData(message));
            return;
        }
    }

    public void StartGame()
    {
        if (server)
        {
            server.SendString(new ChangeSceneData(4));
            ChangeScene.GoToScene(4);
        }
    }
}
