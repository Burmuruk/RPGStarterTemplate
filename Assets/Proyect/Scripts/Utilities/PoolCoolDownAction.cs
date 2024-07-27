using Burmuruk.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Burmuruk.Tesis.Utilities
{
    public class PoolCoolDownAction<T>
    {
        private Queue<CoolDownAction> timers = new();
        private List<(T character, Coroutine coroutine, CoolDownAction cd)> runningTimers = new();
    }
}
