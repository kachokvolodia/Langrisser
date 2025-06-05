using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public List<Unit.Faction> turnOrder = new List<Unit.Faction>
    {
        Unit.Faction.AuroraEmpire,
        Unit.Faction.MoonArchonDominion,
        Unit.Faction.GoldenHand,
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
        StatusBarUI.Instance?.SetEndTurnButtonInteractable(faction == FactionManager.PlayerFaction);

        UnitManager.Instance.ApplyWaitHealing(faction);
        UnitManager.Instance.ResetUnitsForNextTurn(faction);

        if (faction == FactionManager.PlayerFaction)
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
        return CurrentFaction == FactionManager.PlayerFaction;
    }

    public void CheckVictory()
    {
        bool playerAlive = UnitManager.Instance.AllUnits.Exists(u => u.faction == FactionManager.PlayerFaction && u.currentHP > 0);
        bool enemyAlive = UnitManager.Instance.AllUnits.Exists(u =>
            u.currentHP > 0 &&
            (u.faction == Unit.Faction.AuroraEmpire ||
             u.faction == Unit.Faction.MoonArchonDominion ||
             u.faction == Unit.Faction.GoldenHand) &&
            u.faction != FactionManager.PlayerFaction);

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

