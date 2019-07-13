
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Hanafuda.Base;
using Hanafuda.Base.Interfaces;

namespace Hanafuda
{
    public abstract class ISpielfeld : MonoBehaviour, IHanafudaBoard
    {
        /*
         * Main Variables
         */
        public bool Turn { get; set; }
        public List<ICard> Deck { get; set; }
        public List<ICard> Field { get; set; }
        public List<Player> Players { get; set; }

        List<ICard> IHanafudaBoard.Deck
        {
            set { Deck = value.Cast<ICard>().ToList(); }
            get => Deck.Cast<ICard>().ToList();
        }
        List<ICard> IHanafudaBoard.Field
        {
            set { Field = value.Cast<ICard>().ToList(); }
            get => Field.Cast<ICard>().ToList();
        }

        protected Transform EffectCam, Hand1, Hand2, Field3D, Deck3D;
        protected Communication PlayerInteraction;
        public GameInfo InfoUI;

        /*
         * Animation Part
         */
        protected ICard[] Hovered;

        public abstract void AnimateAction(PlayerAction action);
        public abstract void CollectCards(List<ICard> ToCollect);
        protected abstract void HoverCards(params ICard[] cards);
        protected abstract void HoverMatches(Months month);
        protected abstract void SelectionToField(ICard card);
        protected abstract void DrawFromDeck();

        /*
         * Interaction Part
         */
        protected List<ICard> Collection;
        protected List<ICard> TurnCollection;
        public PlayerAction currentAction;

        public abstract void HoverHand(ICard card);
        protected abstract void HandleMatches(ICard card, bool fromDeck = false);
        public abstract void SayKoiKoi(bool koikoi);
        public abstract void SelectCard(ICard card, bool fromDeck = false);
        public abstract void DrawnGame();
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