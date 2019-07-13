using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda.Base.Interfaces
{
    public interface ICard
    {
        string Title { get; set; }
        int ID { get; set; }
        Months Month { get; set; }
        CardMotive Motive { get; set; }
    }
}
