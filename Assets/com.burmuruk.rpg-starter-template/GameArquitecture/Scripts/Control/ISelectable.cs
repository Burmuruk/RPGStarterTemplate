using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burmuruk.Tesis.Control
{
    public interface ISelectable
    {
        bool IsSelected { get; }
        void Select();
        void Deselect();
    }
}
