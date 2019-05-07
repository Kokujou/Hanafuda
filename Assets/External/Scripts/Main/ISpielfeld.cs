using System.Collections.Generic;
using UnityEngine;

namespace Hanafuda
{
    public abstract class ISpielfeld : MonoBehaviour
    {
        /*
         * Main Variables
         */
        public bool Turn;
        public List<Card> Deck;
        public List<Card> Field;
        public List<Player> players;

        protected Transform EffectCam, Hand1, Hand2, Field3D, Deck3D;
        protected Communication PlayerInteraction;
        public GameInfo InfoUI;

        /*
         * Animation Part
         */
        protected Card[] Hovered;

        public abstract void AnimateAction(PlayerAction action);
        public abstract void CollectCards(List<Card> ToCollect);
        protected abstract void HoverCards(params Card[] cards);
        protected abstract void HoverMatches(Card.Months month);
        protected abstract void SelectionToField(Card card);
        protected abstract void DrawFromDeck();

        /*
         * Interaction Part
         */
        protected List<Card> Collection;
        protected PlayerAction currentAction;

        public abstract void HoverHand(Card card);
        protected abstract bool HandleMatches(Card card, bool fromDeck = false);
        public abstract void SayKoiKoi(bool koikoi);
        public abstract void SelectCard(Card card, bool fromDeck = false);
        protected abstract void OpponentTurn();
        protected abstract void ApplyMove(Move move);

        /*
         * Initialization Part
         */
        protected const int MaxDispersionPos = 5;
        protected const int MaxDispersionAngle = 60;
        protected const float CardWidth = 11f;

        public abstract void Init(List<Player> Players);
        protected abstract void GenerateDeck(int seed = -1);
        protected abstract void FieldSetup();
        public abstract void BuildField(int fieldSize = 8);
        public abstract void BuildHands(int hand1Size = 8, int hand2Size = 8);
        public abstract void BuildDeck();
    }
}