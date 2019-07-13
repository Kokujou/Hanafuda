using Hanafuda.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda.Base.Interfaces
{
    public interface IHanafudaBoard
    {
        List<ICard> Deck { get; set; }
        List<ICard> Field { get; set; }
        List<Player> Players { get; set; }
        bool Turn { get; set; }
    }
}
