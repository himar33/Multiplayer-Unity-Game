using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Chat : MonoBehaviour
{
    public GameObject startButton;
    public List<string> messages;
    public TMP_Text text;

    private void Start()
    {
        UDP.instance.chatEvent.AddListener(SetText);
        if (UDP.instance is UDPServer) startButton.SetActive(true);
        else startButton.SetActive(false);
    }

    public void SetText(string message)
    {
        messages.Add(message);
        text.text += "\n" + message;
    }

    public void AddMessage(string message)
    {
        UDP.instance.SendString(new MessageData(message));
    }

    public void StartGame()
    {
        UDP.instance.SendString(new ChangeSceneData(4));
        ChangeScene.GoToScene(4);
    }
}
