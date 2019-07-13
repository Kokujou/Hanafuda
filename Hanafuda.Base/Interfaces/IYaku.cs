using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda.Base.Interfaces
{
    public interface IYaku
    {
        int AddPoints { get; set; }
        int BasePoints { get; set; }
        string JName { get; set; }
        string Title { get; set; }
        int[] Mask { get; set; }
        int MinSize { get; set; }
        List<string> Names { get; set; }
        CardMotive Motive { get; set; }
    }
}
