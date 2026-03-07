using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ExploreConfig", menuName = "Explore/ExploreConfig")]
public class ExploreConfigData : ScriptableObject
{
    public int levelsPerStageData = 15;
    public int levelsPerEnvironment = 30;

    public List<ExploreStageData> stageProfiles;

    public int GetLocalLevel(int absoluteLevel)
    {
        if (absoluteLevel < 0) absoluteLevel = 0;
        return absoluteLevel % levelsPerStageData;
    }

    public ExploreStageData GetStageData(int absoluteLevel)
    {
        if (absoluteLevel < 0) absoluteLevel = 0;
        int index = absoluteLevel / levelsPerStageData;

        if (stageProfiles != null && index < stageProfiles.Count)
        {
            return stageProfiles[index];
        }

        if (stageProfiles != null && stageProfiles.Count > 0)
        {
            return stageProfiles[stageProfiles.Count - 1];
        }
        return null;
    }
}