using System;

namespace Burmuruk.Tesis.Inventory
{
    public interface IUsable
    {
        void Use(object args, Action callback);
    }
}
