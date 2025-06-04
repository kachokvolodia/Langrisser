using System.Collections.Generic;
using UnityEngine;

public static class ExperienceManager
{
    public const int MaxLevel = 10;

    // base exp required for level 2; increases with level
    public static int ExpForLevel(int level)
    {
        // level 1 -> 0 exp, level 2 -> 100, then +50 each level
        if (level <= 1) return 0;
        return 100 + (level - 2) * 50;
    }

    public static int ExpToNextLevel(int currentLevel)
    {
        return ExpForLevel(currentLevel + 1) - ExpForLevel(currentLevel);
    }

    public static int GetKillExp(Unit defeated)
    {
        // experience reward grows with level of defeated unit
        int baseExp = 20;
        return baseExp + defeated.level * 5;
    }

    public static void AwardExperience(Unit killer, Unit defeated)
    {
        int totalExp = GetKillExp(defeated);
        if (killer.isCommander)
        {
            if (killer.squad != null && killer.squad.Count > 0)
            {
                int commanderExp = Mathf.CeilToInt(totalExp * 0.5f);
                killer.AddExperience(commanderExp);
                int share = totalExp - commanderExp;
                int per = Mathf.FloorToInt((float)share / killer.squad.Count);
                foreach (var s in killer.squad)
                {
                    if (s != null)
                        s.AddExperience(per);
                }
            }
            else
            {
                killer.AddExperience(totalExp);
            }
        }
        else
        {
            int soldierExp = Mathf.CeilToInt(totalExp * 0.6f);
            killer.AddExperience(soldierExp);
            Unit commander = killer.commander;
            int commanderExp = Mathf.FloorToInt(totalExp * 0.2f);
            if (commander != null)
                commander.AddExperience(commanderExp);
            if (commander != null && commander.squad != null)
            {
                List<Unit> others = new List<Unit>();
                foreach (var u in commander.squad)
                    if (u != null && u != killer)
                        others.Add(u);
                if (others.Count > 0)
                {
                    int remaining = totalExp - soldierExp - commanderExp;
                    int per = Mathf.FloorToInt((float)remaining / others.Count);
                    foreach (var u in others)
                        u.AddExperience(per);
                }
            }
        }
    }
}

