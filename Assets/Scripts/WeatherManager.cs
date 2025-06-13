using UnityEngine;

public enum WeatherType { Clear, Rain, Fog }

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance;
    public WeatherType CurrentWeather = WeatherType.Clear;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RandomizeWeather()
    {
        CurrentWeather = (WeatherType)Random.Range(0, 3);
        Debug.Log($"[WEATHER] {CurrentWeather}");
        StatusBarUI.Instance?.SetWeatherInfo(CurrentWeather);
    }
}