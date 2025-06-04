using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance;

    public enum RelationType { Ally, Enemy, Neutral }

    // Таблица отношений: кто как относится к кому
    private Dictionary<Unit.Faction, Dictionary<Unit.Faction, RelationType>> relations =
        new Dictionary<Unit.Faction, Dictionary<Unit.Faction, RelationType>>();

    private void Awake()
    {
        Instance = this;
        InitializeRelations();
    }

    private void InitializeRelations()
    {
        // Для каждой фракции прописываем отношения ко всем остальным
        AddRelation(Unit.Faction.Player, Unit.Faction.Player, RelationType.Ally);
        AddRelation(Unit.Faction.Player, Unit.Faction.PlayerAlly, RelationType.Ally);
        AddRelation(Unit.Faction.Player, Unit.Faction.Enemy, RelationType.Enemy);
        AddRelation(Unit.Faction.Player, Unit.Faction.EnemyAlly, RelationType.Enemy);
        AddRelation(Unit.Faction.Player, Unit.Faction.Neutral, RelationType.Neutral);
        AddRelation(Unit.Faction.Player, Unit.Faction.EvilNeutral, RelationType.Enemy);

        // Аналогично для других фракций:
        AddRelation(Unit.Faction.PlayerAlly, Unit.Faction.Player, RelationType.Ally);
        AddRelation(Unit.Faction.PlayerAlly, Unit.Faction.PlayerAlly, RelationType.Ally);
        AddRelation(Unit.Faction.PlayerAlly, Unit.Faction.Enemy, RelationType.Enemy);
        AddRelation(Unit.Faction.PlayerAlly, Unit.Faction.EnemyAlly, RelationType.Enemy);
        AddRelation(Unit.Faction.PlayerAlly, Unit.Faction.Neutral, RelationType.Neutral);
        AddRelation(Unit.Faction.PlayerAlly, Unit.Faction.EvilNeutral, RelationType.Enemy);

        AddRelation(Unit.Faction.Enemy, Unit.Faction.Player, RelationType.Enemy);
        AddRelation(Unit.Faction.Enemy, Unit.Faction.PlayerAlly, RelationType.Enemy);
        AddRelation(Unit.Faction.Enemy, Unit.Faction.Enemy, RelationType.Ally);
        AddRelation(Unit.Faction.Enemy, Unit.Faction.EnemyAlly, RelationType.Ally);
        AddRelation(Unit.Faction.Enemy, Unit.Faction.Neutral, RelationType.Neutral);
        AddRelation(Unit.Faction.Enemy, Unit.Faction.EvilNeutral, RelationType.Enemy);

        AddRelation(Unit.Faction.EnemyAlly, Unit.Faction.Player, RelationType.Enemy);
        AddRelation(Unit.Faction.EnemyAlly, Unit.Faction.PlayerAlly, RelationType.Enemy);
        AddRelation(Unit.Faction.EnemyAlly, Unit.Faction.Enemy, RelationType.Ally);
        AddRelation(Unit.Faction.EnemyAlly, Unit.Faction.EnemyAlly, RelationType.Ally);
        AddRelation(Unit.Faction.EnemyAlly, Unit.Faction.Neutral, RelationType.Neutral);
        AddRelation(Unit.Faction.EnemyAlly, Unit.Faction.EvilNeutral, RelationType.Enemy);

        AddRelation(Unit.Faction.Neutral, Unit.Faction.Player, RelationType.Neutral);
        AddRelation(Unit.Faction.Neutral, Unit.Faction.PlayerAlly, RelationType.Neutral);
        AddRelation(Unit.Faction.Neutral, Unit.Faction.Enemy, RelationType.Neutral);
        AddRelation(Unit.Faction.Neutral, Unit.Faction.EnemyAlly, RelationType.Neutral);
        AddRelation(Unit.Faction.Neutral, Unit.Faction.Neutral, RelationType.Ally); // сами с собой всегда Ally
        AddRelation(Unit.Faction.Neutral, Unit.Faction.EvilNeutral, RelationType.Enemy);

        AddRelation(Unit.Faction.EvilNeutral, Unit.Faction.Player, RelationType.Enemy);
        AddRelation(Unit.Faction.EvilNeutral, Unit.Faction.PlayerAlly, RelationType.Enemy);
        AddRelation(Unit.Faction.EvilNeutral, Unit.Faction.Enemy, RelationType.Enemy);
        AddRelation(Unit.Faction.EvilNeutral, Unit.Faction.EnemyAlly, RelationType.Enemy);
        AddRelation(Unit.Faction.EvilNeutral, Unit.Faction.Neutral, RelationType.Enemy);
        AddRelation(Unit.Faction.EvilNeutral, Unit.Faction.EvilNeutral, RelationType.Ally); // монстры между собой друзья

    }

    private void AddRelation(Unit.Faction a, Unit.Faction b, RelationType rel)
    {
        if (!relations.ContainsKey(a))
            relations[a] = new Dictionary<Unit.Faction, RelationType>();
        relations[a][b] = rel;
    }

    // Метод для проверки отношений
    public RelationType GetRelation(Unit.Faction a, Unit.Faction b)
    {
        if (relations.ContainsKey(a) && relations[a].ContainsKey(b))
            return relations[a][b];
        return RelationType.Neutral;
    }
}
