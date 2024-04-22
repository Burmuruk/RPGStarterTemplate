using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Burmuruk.Tesis.Fighting
{
    public class EnemyDetection : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Transform farPercept;
        [SerializeField] protected Transform closePercept;
        [Space(), Header("Status"), Space()]
        [SerializeField] protected bool hasFarPerception;
        [SerializeField] protected bool hasClosePerception;

        protected Collider[] eyesPerceibed, earsPerceibed;
        protected bool isTargetFar = false;
        protected bool isTargetClose = false;

        //protected virtual void FixedUpdate()
        //{
        //    eyesPerceibed = Physics.OverlapSphere(farPercept.position, stats.EyesRadious, 1 << 10);
        //    earsPerceibed = Physics.OverlapSphere(closePercept.position, stats.EarsRadious, 1 << 10);
        //}

        //private void OnDrawGizmosSelected()
        //{
        //    if (!stats || !farPercept || !closePercept) return;

        //    Gizmos.color = Color.red;
        //    Gizmos.DrawWireSphere(farPercept.position, stats.EyesRadious);
        //    Gizmos.color = Color.blue;
        //    Gizmos.DrawWireSphere(closePercept.position, stats.EarsRadious);
        //}
    }
}
