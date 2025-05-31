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

        Dictionary<CharacterType, Dictionary<int, CharacterData>> levelStatsDic;

        [Serializable]
        private struct CharacterData
        {
            public CharacterType charactyerType;
            public int level;
            public BasicStats stats;
        }

        public BasicStats GetDataByLevel(CharacterType type, int level)
        {
            Initlize();

            return levelStatsDic[type][level].stats;
        }

        private void Initlize()
        {
            if (initilized) return;

            var charactersDic = new Dictionary<CharacterType, Dictionary<int, CharacterData>>();
            var statsDic = new Dictionary<int, CharacterData>();

            foreach ( var statData in statsList)
            {
                if (!charactersDic.ContainsKey(statData.charactyerType))
                {
                    statsDic = new()
                    {
                        { statData.level, statData },
                    };

                    charactersDic.Add(statData.charactyerType, statsDic);
                }
                else
                {
                    charactersDic[statData.charactyerType] = new();

                    if (!charactersDic[statData.charactyerType].ContainsKey(statData.level))
                    {
                        charactersDic[statData.charactyerType].Add(statData.level, statData);
                    }
                    else
                    {
                        charactersDic[statData.charactyerType][statData.level] = statData;
                    }
                }
            }

            levelStatsDic = charactersDic;

            //initilized = true;
        }
    }
}
