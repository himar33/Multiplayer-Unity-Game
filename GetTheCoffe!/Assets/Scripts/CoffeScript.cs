using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (FindObjectOfType<CoffeScript>())
            {
                MazeSpawner.SafeDestroy(FindObjectOfType<CoffeScript>().gameObject);
            }
        }
    }
}
