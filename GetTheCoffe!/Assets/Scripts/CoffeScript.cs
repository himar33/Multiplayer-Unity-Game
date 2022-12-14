using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeScript : MonoBehaviour
{
    [SerializeField] private int getTime = 15;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindObjectOfType<MazeSpawner>().GenerateCoffe();
            FindObjectOfType<Timer>().AddTime(getTime);
        }
    }
}
