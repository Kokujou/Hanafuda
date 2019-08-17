using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hanafuda
{
    public class UninformedStatePropsCalculator
    {
        private readonly List<Card> _newCards = new List<Card>();
        private readonly UninformedBoard _state;
        private readonly Dictionary<Card, float> _activeHand;
        private readonly bool _turn;
        private readonly int _activeHandSize;
        private readonly List<Card> _activeCollection;
        private readonly YakuCollection _uninformedYakuProps;
        private readonly List<CardProperties> _cardProperties;

        public UninformedStatePropsCalculator(UninformedBoard state, List<CardProperties> cardProps, bool turn)
        {
            _state = state;
            _cardProperties = cardProps;
            _turn = turn;

            _activeHand = _turn ? _state.computer.Hand.ToDictionary(x => x, x => 1f) : _state.UnknownCards;
            _activeHandSize = turn ? _state.computer.Hand.Count : _state.OpponentHandSize;
            _activeCollection = turn ? _state.computer.CollectedCards : _state.OpponentCollection;

            _newCards = GetNewCards();

            UninformedCards uninformedCardProps = new UninformedCards(cardProps, _state, turn);
            _uninformedYakuProps = new YakuCollection(cardProps, _newCards, _activeCollection, _activeHandSize);
        }

        public float GetGlobalMaximum()
        {
            var result = float.NegativeInfinity;
            foreach (YakuProperties yakuProp in _uninformedYakuProps)
            {
                float value = GetYakuQuality(yakuProp);
                if (value > result)
                    result = value;
            }
            return result;
        }

        public float GetLocalMaximum()
        {
            float TotalCardValue = 0f;
            try
            {
                TotalCardValue = _uninformedYakuProps
                    .Where(x => x.Targeted)
                    .Sum(x => GetYakuQuality(x));
            }
            catch (Exception e) { Debug.Log($"Could not evaluate Total Card Value: \n{e}"); }
            return TotalCardValue;
        }

        public float GetCollectionValue()
            => _newCards.Sum(x => _cardProperties.First(y => y.card.Title == x.Title).RelevanceForYaku.Sum(z => z.Value));

        private float GetYakuQuality(YakuProperties yakuProps)
            => (_activeHandSize - yakuProps.MinTurns) * yakuProps.Probability;

        private List<Card> GetNewCards()
        {
            if (_turn)
                return _state.computer.CollectedCards
                    .Except(_state.parent.computer.CollectedCards)
                    .ToList();
            else
                return _state.OpponentCollection
                    .Except(_state.parent.OpponentCollection)
                    .ToList();
        }
    }
}
