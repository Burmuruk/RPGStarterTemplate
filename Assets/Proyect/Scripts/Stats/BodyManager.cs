using System;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [Serializable]
    public class BodyManager
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject head;
        [SerializeField] private GameObject back;
        [SerializeField] private GameObject lArm;
        [SerializeField] private GameObject rArm;
        [SerializeField] private GameObject lLeg;
        [SerializeField] private GameObject rLeg;
        [SerializeField] private GameObject lFot;
        [SerializeField] private GameObject rFot;

        public GameObject GetPart(BodyPart part)
        {
            return part switch
            {
                BodyPart.Head => head,
                BodyPart.Back => back,
                BodyPart.LArm => lArm,
                BodyPart.RArm => rArm,
                BodyPart.LLeg => lLeg,
                BodyPart.RLeg => rLeg,
                BodyPart.LFot => lFot,
                BodyPart.RFot => rFot,
                _ => playerPrefab
            };
        }

        public enum BodyPart
        {
            None,
            Body,
            Head,
            Back,
            RArm,
            LArm,
            RLeg,
            LLeg,
            LFot,
            RFot,
        }
    }
}