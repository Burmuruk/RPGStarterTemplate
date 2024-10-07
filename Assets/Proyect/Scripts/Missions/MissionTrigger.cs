using Burmuruk.Tesis.Control.AI;
using Burmuruk.Tesis.Utilities;
using UnityEngine;

namespace Burmuruk.Tesis.Missions
{
    public class MissionTrigger : ActivationObject
    {
        [SerializeField] Mission mission;



        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out AIGuildMember member) && member.IsControlled)
            {
                FindObjectOfType<MissionManager>().AddMission(mission);

                Enable(true);
            }
        }
    }
}
