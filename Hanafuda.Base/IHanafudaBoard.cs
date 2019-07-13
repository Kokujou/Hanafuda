using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda.Base
{
    public interface IHanafudaBoard
    {
        List<Card> Deck { get; set; }
        List<Card> Field { get; set; }
        List<Player> Players { get; set; }
        bool Turn { get; set; }
    }
}
