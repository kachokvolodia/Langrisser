using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance;
    public static Faction PlayerFaction = Faction.AuroraEmpire;

    public FactionData[] factionDatas;
    private Dictionary<Faction, FactionData> factionDataDict = new Dictionary<Faction, FactionData>();

    public enum RelationType { Ally, Enemy, Neutral }

    // Таблица отношений: кто как относится к кому
    private Dictionary<Faction, Dictionary<Faction, RelationType>> relations =
        new Dictionary<Faction, Dictionary<Faction, RelationType>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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

    public FactionData GetFactionData(Faction faction)
    {
        factionDataDict.TryGetValue(faction, out var data);
        return data;
    }

    private void InitializeRelations()
    {
        // Aurora Empire relations
        AddRelation(Faction.AuroraEmpire, Faction.AuroraEmpire, RelationType.Ally);
        AddRelation(Faction.AuroraEmpire, Faction.MoonArchonDominion, RelationType.Enemy);
        AddRelation(Faction.AuroraEmpire, Faction.GoldenHand, RelationType.Enemy);
        AddRelation(Faction.AuroraEmpire, Faction.Neutral, RelationType.Neutral);
        AddRelation(Faction.AuroraEmpire, Faction.EvilNeutral, RelationType.Enemy);

        // Moon Archon Dominion relations
        AddRelation(Faction.MoonArchonDominion, Faction.AuroraEmpire, RelationType.Enemy);
        AddRelation(Faction.MoonArchonDominion, Faction.MoonArchonDominion, RelationType.Ally);
        AddRelation(Faction.MoonArchonDominion, Faction.GoldenHand, RelationType.Enemy);
        AddRelation(Faction.MoonArchonDominion, Faction.Neutral, RelationType.Neutral);
        AddRelation(Faction.MoonArchonDominion, Faction.EvilNeutral, RelationType.Enemy);

        // Golden Hand relations
        AddRelation(Faction.GoldenHand, Faction.AuroraEmpire, RelationType.Enemy);
        AddRelation(Faction.GoldenHand, Faction.MoonArchonDominion, RelationType.Enemy);
        AddRelation(Faction.GoldenHand, Faction.GoldenHand, RelationType.Ally);
        AddRelation(Faction.GoldenHand, Faction.Neutral, RelationType.Neutral);
        AddRelation(Faction.GoldenHand, Faction.EvilNeutral, RelationType.Enemy);

        // Neutral relations
        AddRelation(Faction.Neutral, Faction.AuroraEmpire, RelationType.Neutral);
        AddRelation(Faction.Neutral, Faction.MoonArchonDominion, RelationType.Neutral);
        AddRelation(Faction.Neutral, Faction.GoldenHand, RelationType.Neutral);
        AddRelation(Faction.Neutral, Faction.Neutral, RelationType.Ally);
        AddRelation(Faction.Neutral, Faction.EvilNeutral, RelationType.Enemy);

        // EvilNeutral relations
        AddRelation(Faction.EvilNeutral, Faction.AuroraEmpire, RelationType.Enemy);
        AddRelation(Faction.EvilNeutral, Faction.MoonArchonDominion, RelationType.Enemy);
        AddRelation(Faction.EvilNeutral, Faction.GoldenHand, RelationType.Enemy);
        AddRelation(Faction.EvilNeutral, Faction.Neutral, RelationType.Enemy);
        // EvilNeutral должны дружить между собой
        AddRelation(Faction.EvilNeutral, Faction.EvilNeutral, RelationType.Ally);

    }

    private void AddRelation(Faction a, Faction b, RelationType rel)
    {
        if (!relations.ContainsKey(a))
            relations[a] = new Dictionary<Faction, RelationType>();
        relations[a][b] = rel;
    }

    // Метод для проверки отношений
    public RelationType GetRelation(Faction a, Faction b)
    {
        if (relations.ContainsKey(a) && relations[a].ContainsKey(b))
            return relations[a][b];
        return RelationType.Neutral;
    }
}
