using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/CharacterProgress", order = 1)]
    public class CharacterProgress : ScriptableObject
    {
        [SerializeField] bool initilized;
        [SerializeField] CharacterData[] statsList;

        Dictionary<CharacterType, Dictionary<int, BasicStats>> levelStatsDic;

        [Serializable]
        private struct CharacterData
        {
            public CharacterType characterType;
            public List<LevelData> levelData;
        }

        [Serializable]
        private struct LevelData
        {
            public int level;
            public BasicStats stats;
        }

        public BasicStats GetDataByLevel(CharacterType type, int level)
        {
            if (levelStatsDic == null)
                Initlize();

            return levelStatsDic[type][level];
        }

        private void Initlize()
        {
            //if (initilized) return;
            levelStatsDic = new Dictionary<CharacterType, Dictionary<int, BasicStats>>();

            foreach ( var statData in statsList)
            {
                if (levelStatsDic.ContainsKey(statData.characterType))
                    continue;

                levelStatsDic[statData.characterType] = new Dictionary<int, BasicStats>();

                foreach (var levelData in statData.levelData)
                {
                    levelStatsDic[statData.characterType][levelData.level] = levelData.stats;
                }
            }

            //initilized = true;
        }
    }
}
