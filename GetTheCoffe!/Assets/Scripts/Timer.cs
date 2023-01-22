using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private float time;
    [SerializeField] private float timeEffect;
    [SerializeField] private Volume volume;
    private ChromaticAberration chromaticAberration;

    private float timeRemaining;
    private bool timerIsRunning = false;
    private float lerp;
    public TMP_Text timeText;

    private void Start()
    {
        VolumeProfile profile = volume.sharedProfile;
        if (profile.TryGet(out chromaticAberration))
        {
            chromaticAberration.intensity.value = 0;
        }
        else
        {
            chromaticAberration = profile.Add<ChromaticAberration>();
            chromaticAberration.intensity.value = 0;
        }

        StartTimer();
    }

    void Update()
    {
        if (!timerIsRunning) return;
        if (timeRemaining <= 0)
        {
            Debug.Log("Timer has run out!");
            timeRemaining = 0;
            timerIsRunning = false;

            ChangeScene.GoToScene(4);
            UDP.instance.SendString(new ChangeSceneData(4));
            return;
        }
        if (timeRemaining <= timeEffect) UpdateVolumeEffect();
        timeRemaining -= Time.deltaTime;
        DisplayTime(timeRemaining);
    }

    private void UpdateVolumeEffect()
    {
        lerp += Time.deltaTime / timeEffect;
        SetChromaticIntensity(Mathf.Lerp(0, 1, lerp));
    }

    private void SetChromaticIntensity(float value)
    {
        chromaticAberration.intensity.value = value;
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay = Mathf.Max(timeToDisplay, 0);
        int minutes = (int)timeToDisplay / 60;
        int seconds = (int)timeToDisplay % 60;
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    public void StartTimer()
    {
        timerIsRunning = false;
        ResetTimer();
        timerIsRunning = true;
        Debug.Log("Starting timer with " + timeRemaining);
    }

    public void AddTime(float newTime = 0)
    {
        if (timerIsRunning)
        {
            timerIsRunning = false;
            lerp = 0;
            SetChromaticIntensity(0);
            timeRemaining += newTime;
            timerIsRunning = true;
        }
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

    public void ResetTimer()
    {
        lerp = 0;
        SetChromaticIntensity(0);
        timeRemaining = time;
        Debug.Log("Timer reset");
    }
}
