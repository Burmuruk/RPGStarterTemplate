using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Missions
{
    public class MissionManager : MonoBehaviour
    {
        [SerializeField] List<Mission> missions = new List<Mission>();

        public event Action<Mission> OnMissionStarted;

        public void DisplayMission()
        {

        }

        public void FinishMission()
        {

        }

        public void AddMission(Mission mission)
        {
            missions.Add(mission);
            mission.OnStarted += OnMissionStarted;

            mission.Start();
        }
    }
}
