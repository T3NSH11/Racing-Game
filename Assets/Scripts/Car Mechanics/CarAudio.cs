using System.Collections;
using UnityEngine;

public class CarAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource revvingSound;
    public AudioSource reverseRevvingSound;
    public AudioSource idleSound;
    public AudioSource startSound;

    [Header("Audio Settings")]
    public float revvingMaxVolume;
    public float revvingMaxPitch;
    public float reverseRevvingMaxVolume;
    public float reverseRevvingMaxPitch;
    public float idleMaxVolume;

    [Header("Engine Settings")]
    public float speedRatio;
    private float revLimiter;
    public float LimiterSound = 1f;
    public float LimiterFrequency = 3f;
    public float LimiterEngage = 0.8f;
    public bool isEngineRunning = false;

    private CarMechanics carController;

    private void Start()
    {
        carController = GetComponent<CarMechanics>();
        SetInitialAudioVolumes();
    }

    private void Update()
    {
        float speedSign = GetSpeedSign();
        UpdateSpeedRatio(speedSign);
        UpdateAudioVolumes(speedSign);
    }

    public IEnumerator StartEngine()
    {
        startSound.Play();
        carController.isEngineOn = 1;
        yield return new WaitForSeconds(0.6f);
        isEngineRunning = true;
        yield return new WaitForSeconds(0.4f);
        carController.isEngineOn = 2;
    }

    private void SetInitialAudioVolumes()
    {
        idleSound.volume = 0;
        revvingSound.volume = 0;
        reverseRevvingSound.volume = 0;
    }

    private float GetSpeedSign()
    {
        if (carController)
        {
            return Mathf.Sign(carController.GetSpeedDifference());
        }
        return 0;
    }

    private void UpdateSpeedRatio(float speedSign)
    {
        if (carController)
        {
            speedRatio = Mathf.Abs(carController.GetSpeedDifference());
        }
        if (speedRatio > LimiterEngage)
        {
            revLimiter = (Mathf.Sin(Time.time * LimiterFrequency) + 1f) * LimiterSound * (speedRatio - LimiterEngage);
        }
    }

    private void UpdateAudioVolumes(float speedSign)
    {
        if (isEngineRunning)
        {
            idleSound.volume = Mathf.Lerp(0.1f, idleMaxVolume, speedRatio);
            if (speedSign > 0)
            {
                SetRunningSound();
            }
            else
            {
                SetReverseSound();
            }
        }
        else
        {
            idleSound.volume = 0;
            revvingSound.volume = 0;
        }
    }

    private void SetRunningSound()
    {
        reverseRevvingSound.volume = 0;
        revvingSound.volume = Mathf.Lerp(0.3f, revvingMaxVolume, speedRatio);
        revvingSound.pitch = Mathf.Lerp(0.3f, revvingMaxPitch, speedRatio);
    }

    private void SetReverseSound()
    {
        revvingSound.volume = 0;
        reverseRevvingSound.volume = Mathf.Lerp(0f, reverseRevvingMaxVolume, speedRatio);
        reverseRevvingSound.pitch = Mathf.Lerp(0.2f, reverseRevvingMaxPitch, speedRatio);
    }
}
