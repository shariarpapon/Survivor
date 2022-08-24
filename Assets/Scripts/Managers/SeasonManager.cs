using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Survivor.Core;

public class SeasonManager : MonoBehaviour, IInitializer
{
    public static SeasonManager Instance { get; private set; }

    [Tooltip("The length of the day in seconds")]
    [SerializeField] private float dayLength;
    [SerializeField] private float startTime;
    [SerializeField] private float time;
    [SerializeField] private AnimationCurve nightVolumeWeight;
    [SerializeField] private AnimationCurve skyExposure;
    [SerializeField] private AnimationCurve sunLightIntensity;

    [Space]
    [SerializeField] private Volume nightVolume;
    [SerializeField] private Light sunLight;
    [SerializeField] private Material skyboxMaterial;

    private float dt;

    private void Awake() 
    {
        if (Instance == null) Instance = this;
        else 
        {
            Destroy(gameObject);
            return;
        }
    }

    public IEnumerator Init() 
    {
        time = startTime;
        dt = 1/dayLength;
        yield return null;
    }

    private void Update() 
    {
        RunCycle();
    }

    private void RunCycle() 
    {
        if (GameManager.GameMode != GameMode.Playing) return;

        time += dt * Time.deltaTime;
        if (time >= 1) time = 0;

        UpdateSun();
        UpdateSky();
        UpdateFog();
        UpdatePostProcessing();
    }

    private void UpdateSun() 
    {
        sunLight.intensity = sunLightIntensity.Evaluate(time);
    }

    private void UpdateSky()
    {
        skyboxMaterial.SetFloat("_Exposure", skyExposure.Evaluate(time));
    }

    private void UpdateFog()
    {

    }

    private void UpdatePostProcessing() 
    {
        nightVolume.weight = nightVolumeWeight.Evaluate(time);
    }
}
