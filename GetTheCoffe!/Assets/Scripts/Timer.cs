using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private float time = 20;
    [SerializeField] private Volume volume;

    private float timeRemaining;
    private bool timerIsRunning = false;
    float lerp;
    public TMP_Text timeText;

    private void Start()
    {
        StartTimer();
    }

    void Update()
    {
        if (timerIsRunning)
        {

            if (timeRemaining > 0)
            {
                SetVolumeEffect();

                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                Debug.Log("Timer has run out!");
                timeRemaining = 0;
                timerIsRunning = false;
            }
        }
    }

    private void SetVolumeEffect()
    {
        VolumeProfile profile = volume.sharedProfile;
        if (!profile.TryGet<ChromaticAberration>(out var chromatic))
        {
            chromatic = profile.Add<ChromaticAberration>(false);
        }
        lerp += Time.deltaTime / time;
        chromatic.intensity.value = Mathf.Lerp(0, 1, lerp);
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void StartTimer()
    {
        timerIsRunning = false;
        ResetTimer();
        timerIsRunning = true;
        Debug.Log("Starting timer with " + time);
    }

    public void ResumeTimer()
    {
        if (!timerIsRunning && timeRemaining > 0)
        {
            timerIsRunning = true;
            Debug.Log("Timer resumed at " + timeRemaining);
        }
    }

    public void PauseTimer()
    {
        if (timerIsRunning && timeRemaining > 0)
        {
            timerIsRunning = false;
            Debug.Log("Timer paused at " + timeRemaining);
        }
    }

    public void SetTimer(float newTime)
    {
        time = newTime;
        ResetTimer();
        Debug.Log("Timer changed to " + newTime);
    }

    public void ResetTimer()
    {
        timeRemaining = time;
        Debug.Log("Timer reset");
    }
}
