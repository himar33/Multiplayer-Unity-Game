using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Chat : MonoBehaviour
{
    public GameObject startButton;
    public List<string> messages;
    public TMP_Text text;

    private UDP udp;

    private void Start()
    {
        udp = FindObjectOfType<UDP>();
        if (udp)
        {
            udp.chatEvent.AddListener(SetText);
            if (udp is UDPServer) startButton.SetActive(true);
            else startButton.SetActive(false);
        }
    }

    public void SetText(string message)
    {
        messages.Add(message);
        text.text += "\n" + message;
    }

    public void AddMessage(string message)
    {
        if (udp) udp.SendString(new MessageData(message));
    }

    public void StartGame()
    {
        if (udp) udp.SendString(new ChangeSceneData(4));
        ChangeScene.GoToScene(4);
    }
}
