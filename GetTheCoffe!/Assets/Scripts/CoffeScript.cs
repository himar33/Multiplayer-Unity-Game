using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeScript : MonoBehaviour
{
    [SerializeField] private int timeBonus = 15;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (UDP.instance is UDPServer && other.name == "ServerPlayer" || UDP.instance is UDPClient && other.name == "ClientPlayer")
            {
                FindObjectOfType<Timer>().AddTime(timeBonus);
                RemoveAndCreateCoffe();
            }
        }
    }

    private static void RemoveAndCreateCoffe()
    {
        MazeSpawner mazeSpawner = FindObjectOfType<MazeSpawner>();
        GameObject coffe = mazeSpawner.GenerateCoffe();
        UDP.instance.SendString(new RemoveObjectData("Coffe(Clone)"));
        UDP.instance.SendString(new InstantiateData("Coffe", coffe.transform.position));
    }
}
