using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance;
    public static Unit.Faction PlayerFaction = Unit.Faction.AuroraEmpire;

    public FactionData[] factionDatas;
    private Dictionary<Unit.Faction, FactionData> factionDataDict = new Dictionary<Unit.Faction, FactionData>();

    public enum RelationType { Ally, Enemy, Neutral }

    // Таблица отношений: кто как относится к кому
    private Dictionary<Unit.Faction, Dictionary<Unit.Faction, RelationType>> relations =
        new Dictionary<Unit.Faction, Dictionary<Unit.Faction, RelationType>>();

    private void Awake()
    {
        Instance = this;
        BuildFactionDictionary();
        InitializeRelations();
    }

    void BuildFactionDictionary()
    {
        factionDataDict.Clear();
        if (factionDatas != null)
        {
            foreach (var data in factionDatas)
            {
                if (data != null)
                    factionDataDict[data.faction] = data;
            }
        }
    }

    public FactionData GetFactionData(Unit.Faction faction)
    {
        factionDataDict.TryGetValue(faction, out var data);
        return data;
    }

    private void InitializeRelations()
    {
        // Aurora Empire relations
        AddRelation(Unit.Faction.AuroraEmpire, Unit.Faction.AuroraEmpire, RelationType.Ally);
        AddRelation(Unit.Faction.AuroraEmpire, Unit.Faction.MoonArchonDominion, RelationType.Enemy);
        AddRelation(Unit.Faction.AuroraEmpire, Unit.Faction.GoldenHand, RelationType.Enemy);
        AddRelation(Unit.Faction.AuroraEmpire, Unit.Faction.Neutral, RelationType.Neutral);
        AddRelation(Unit.Faction.AuroraEmpire, Unit.Faction.EvilNeutral, RelationType.Enemy);

        // Moon Archon Dominion relations
        AddRelation(Unit.Faction.MoonArchonDominion, Unit.Faction.AuroraEmpire, RelationType.Enemy);
        AddRelation(Unit.Faction.MoonArchonDominion, Unit.Faction.MoonArchonDominion, RelationType.Ally);
        AddRelation(Unit.Faction.MoonArchonDominion, Unit.Faction.GoldenHand, RelationType.Enemy);
        AddRelation(Unit.Faction.MoonArchonDominion, Unit.Faction.Neutral, RelationType.Neutral);
        AddRelation(Unit.Faction.MoonArchonDominion, Unit.Faction.EvilNeutral, RelationType.Enemy);

        // Golden Hand relations
        AddRelation(Unit.Faction.GoldenHand, Unit.Faction.AuroraEmpire, RelationType.Enemy);
        AddRelation(Unit.Faction.GoldenHand, Unit.Faction.MoonArchonDominion, RelationType.Enemy);
        AddRelation(Unit.Faction.GoldenHand, Unit.Faction.GoldenHand, RelationType.Ally);
        AddRelation(Unit.Faction.GoldenHand, Unit.Faction.Neutral, RelationType.Neutral);
        AddRelation(Unit.Faction.GoldenHand, Unit.Faction.EvilNeutral, RelationType.Enemy);

        // Neutral relations
        AddRelation(Unit.Faction.Neutral, Unit.Faction.AuroraEmpire, RelationType.Neutral);
        AddRelation(Unit.Faction.Neutral, Unit.Faction.MoonArchonDominion, RelationType.Neutral);
        AddRelation(Unit.Faction.Neutral, Unit.Faction.GoldenHand, RelationType.Neutral);
        AddRelation(Unit.Faction.Neutral, Unit.Faction.Neutral, RelationType.Ally);
        AddRelation(Unit.Faction.Neutral, Unit.Faction.EvilNeutral, RelationType.Enemy);

        // EvilNeutral relations
        AddRelation(Unit.Faction.EvilNeutral, Unit.Faction.AuroraEmpire, RelationType.Enemy);
        AddRelation(Unit.Faction.EvilNeutral, Unit.Faction.MoonArchonDominion, RelationType.Enemy);
        AddRelation(Unit.Faction.EvilNeutral, Unit.Faction.GoldenHand, RelationType.Enemy);
        AddRelation(Unit.Faction.EvilNeutral, Unit.Faction.Neutral, RelationType.Enemy);
        AddRelation(Unit.Faction.EvilNeutral, Unit.Faction.EvilNeutral, RelationType.Enemy);

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
