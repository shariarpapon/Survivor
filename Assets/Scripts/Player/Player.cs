using System.Collections;
using UnityEngine;
using Survivor.Core;
using Survivor.WorldManagement;

public class Player : MonoBehaviour, IInitializer
{
    public static event System.Action OnPlayerDeadCallbacks;
    public static event System.Action OnPlayerSpawnCallbacks;

    public static bool IsImmortal = false;
    public static bool IsExhausted;
    public static bool IsDead;

    public new string name;
    public VitalSystem vitals;

    [SerializeField] private float exhaustedHealthDmgRate;
    [SerializeField] private float coldHealthDmgRate;
    [SerializeField] private float hotHealthDmgRate;
    [SerializeField] private float temperatureChangeRate;

    public IEnumerator Init()
    {
        OnPlayerSpawn();
        yield return null;
    }

    private void Update() 
    {
        if (!GameManager.IsGameInitialized) return;

        if(!IsImmortal) UpdateVitalSystem();
    }

    private void UpdateVitalSystem() 
    {
        if (IsExhausted) OnPlayerExhausted();
        if (IsDead) OnPlayerDead();

        vitals.temperature.value = GetPlayerSurroundingTemperature(WorldManager.Instance.overworld);

        switch (vitals.GetBodyTemperatureState)
        {
            default: break;
            case VitalSystem.BodyTemperature.Cold:
                AddHealth(-coldHealthDmgRate * Time.deltaTime);
                break;
            case VitalSystem.BodyTemperature.Hot:
                AddHealth(-hotHealthDmgRate * Time.deltaTime);
                break;
        }
        
    }

    public float GetPlayerSurroundingTemperature(World world) 
    {
        float envTemp = world.GetTemperatureAtPosition(transform.position);
        return Mathf.Lerp(vitals.temperature.value, envTemp, Time.deltaTime * temperatureChangeRate);
    }

    private void OnPlayerSpawn()
    {
        vitals.health.value = vitals.health.max;
        vitals.energy.value = vitals.energy.max;

        OnPlayerSpawnCallbacks?.Invoke();
    }

    private void OnPlayerDead()
    {
        Debug.Log("Player is dead");
        OnPlayerDeadCallbacks?.Invoke();
    }

    private void OnPlayerExhausted() 
    {
        Debug.Log("Player is exausted");
        vitals.health.value -= exhaustedHealthDmgRate * Time.deltaTime;
        InGameUIManager.Instance.UpdatePlayerUI();
    }

    public void AddVitals(float healthAmount, float energyAmount) 
    {
        AddHealth(healthAmount);
        AddEnergy(energyAmount);
    }

    public void AddHealth(float amount) 
    {
        vitals.health.value += amount;
        vitals.UpdateVitals();
        InGameUIManager.Instance.UpdatePlayerUI();
        IsDead = vitals.IsDead;
    }

    public void AddEnergy(float amount) 
    {
        vitals.energy.value += amount;
        vitals.UpdateVitals();
        InGameUIManager.Instance.UpdatePlayerUI();
        IsExhausted = vitals.IsExhausted;
    }

}
