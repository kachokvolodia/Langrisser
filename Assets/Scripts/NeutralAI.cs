using UnityEngine;

/// <summary>
/// AI для нейтральных существ. Наследует EnemyAI и
/// может быть расширен особым поведением в будущем.
/// </summary>
public class NeutralAI : EnemyAI
{
    /// <summary>
    /// Заглушка для логики нейтралов
    /// </summary>
    public void ExecuteFactionStrategy()
    {
        Debug.Log("Neutral AI strategy placeholder");
    }
}
