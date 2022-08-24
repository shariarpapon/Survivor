[System.Serializable]
public class VitalSystem
{
    public FloatRange health;
    public FloatRange energy;
    public FloatRange temperature;

    public BodyTemperature GetBodyTemperatureState
    { 
        get 
        {
            if (temperature.value > temperature.max) return BodyTemperature.Hot;
            else if (temperature.value < temperature.min) return BodyTemperature.Cold;
            else return BodyTemperature.Normal;
        } 
    }

    public bool IsDead { get { return health.value <= 0; } }
    public bool IsExhausted { get { return energy.value <= 0; } }
    public bool IsDamaged { get { return health.value < health.max; } }

    public float HealthPercentage { get { return health.value / health.max; } }
    public float EnergyPercentage { get { return energy.value / energy.max; } }

    public void UpdateVitals() 
    {
        if (health.value > health.max) health.value = health.max;
        else if (health.value < health.min) health.value = health.min;

        if (energy.value > energy.max) energy.value = energy.max;
        else if (energy.value < energy.min) energy.value = energy.min;
    }

    public enum BodyTemperature 
    {
        Normal,
        Cold,
        Hot
    }
}
