﻿using System.Collections.Generic;

namespace Hanafuda
{
    public interface IArtificialIntelligence
    {
        Dictionary<string, float> GetWeights();
        void SetWeight(string name, float value);
        Move MakeTurn(Spielfeld board);
    }
}