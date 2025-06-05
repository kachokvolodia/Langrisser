using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public List<Unit.Faction> turnOrder = new List<Unit.Faction>
    {
        Unit.Faction.Player,
        Unit.Faction.PlayerAlly,
        Unit.Faction.Enemy,
        Unit.Faction.EnemyAlly,
        Unit.Faction.Neutral,
        Unit.Faction.EvilNeutral
    };

    private int currentIndex = 0;

    public Unit.Faction CurrentFaction => turnOrder[currentIndex];

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartTurn(CurrentFaction);
    }

    void StartTurn(Unit.Faction faction)
    {
        StatusBarUI.Instance?.SetTurnInfo(faction);
        StatusBarUI.Instance?.SetEndTurnButtonInteractable(faction == Unit.Faction.Player);

        UnitManager.Instance.ApplyWaitHealing(faction);
        UnitManager.Instance.ResetUnitsForNextTurn(faction);

        if (faction == Unit.Faction.Player)
        {
            Debug.Log("Ход игрока!");
        }
        else
        {
            EnemyManager.Instance.DoFactionTurn(faction, EndCurrentTurn);
            Debug.Log($"Ход фракции {faction}");
        }
    }

    public void EndCurrentTurn()
    {
        if (EnemyManager.Instance == null) return;
        currentIndex = (currentIndex + 1) % turnOrder.Count;
        StartTurn(CurrentFaction);
    }

    // Compatibility with old UI
    public void EndPlayerTurn()
    {
        EndCurrentTurn();
    }

    public bool IsPlayerTurn()
    {
        return CurrentFaction == Unit.Faction.Player;
    }

    public void CheckVictory()
    {
        bool playerAlive = UnitManager.Instance.AllUnits.Exists(u => u.faction == Unit.Faction.Player && u.currentHP > 0);
        bool enemyAlive = UnitManager.Instance.AllUnits.Exists(u => u.faction == Unit.Faction.Enemy && u.currentHP > 0);

        if (!playerAlive)
        {
            Debug.Log("Поражение!");
        }
        else if (!enemyAlive)
        {
            Debug.Log("Победа!");
            GridManager.Instance.UnlockExit();
        }
    }
}

