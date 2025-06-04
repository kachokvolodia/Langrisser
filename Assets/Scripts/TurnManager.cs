using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum Turn
    {
        Player,
        Enemy
    }

    public Turn currentTurn = Turn.Player;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("Игра началась! Ход игрока.");
    }

    public void EndPlayerTurn()
    {
        currentTurn = Turn.Enemy;
        Debug.Log("Ход врага!");

        EnemyManager.Instance.DoAllEnemiesTurn(StartPlayerTurn);
    }

    public void StartPlayerTurn()
    {
        // Сначала лечим юниты игрока, если они ждали прошлый ход
        UnitManager.Instance.ApplyWaitHealing(Unit.Faction.Player, Unit.Faction.PlayerAlly);

        // Затем обнуляем флаги действий только у юнитов игрока
        UnitManager.Instance.ResetUnitsForNextTurn(Unit.Faction.Player, Unit.Faction.PlayerAlly);

        currentTurn = Turn.Player;
        Debug.Log("Снова ход игрока!");
    }


    public bool IsPlayerTurn()
    {
        return currentTurn == Turn.Player;
    }
    public void CheckVictory()
    {
        bool playerAlive = UnitManager.Instance.AllUnits.Any(u => u.faction == Unit.Faction.Player && u.currentHP > 0);
        bool enemyAlive = UnitManager.Instance.AllUnits.Any(u => u.faction == Unit.Faction.Enemy && u.currentHP > 0);

        if (!playerAlive)
        {
            Debug.Log("Поражение!");
            // Тут вызывай окно поражения, рестарт, выход и т.п.
        }
        else if (!enemyAlive)
        {
            Debug.Log("Победа!");
            // Тут вызывай окно победы, переход на следующий уровень и т.п.
        }
    }
}
