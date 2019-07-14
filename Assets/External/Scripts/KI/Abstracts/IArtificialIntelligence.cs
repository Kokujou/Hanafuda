using System.Collections.Generic;

namespace Hanafuda
{
    public interface IArtificialIntelligence
    {
        Dictionary<string, float> GetWeights();
        void SetWeight(string name, float value);
        Move MakeTurn(IHanafudaBoard board, int playerID);
        Move RequestDeckSelection(Spielfeld board, Move baseMove, int playerID);
    }
}