using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public static void GoToScene(int index)
    {
        AudioManager.instance.PlaySound(AudioManager.instance.buttonSound);
        SceneManager.LoadScene(index);
    }
}
