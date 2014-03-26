using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HearthstoneBot
{

    public interface Action
    {
        /**
         * Perform the action.
         */
        void perform();

        /**
         * Delay after taking action.
         */
        int delay();
    }

    public class CardAction : Action
    {
        // The card in your hand
        private Card card;

        // Whether to pickup or drop the card
        // Note: You must pick up card first before dropping
        private bool pickup;

        public CardAction(Card card, bool pickup)
        {
            this.card = card;
            this.pickup = pickup;
        }

        /**
         * Implementation to perform the action.
         */
        public void perform()
        {
            API.drop_card(card, pickup);
        }

        public int delay()
        {
            return 2000;
        }

        public override string ToString()
        {
            return "CardAction(card=" + card.GetEntity().GetName() + ", pickup=" + pickup + ")";
        }
    }

    public class AttackAction : Action
    {
        // The card on the battle field - can be either friendly minion or enemy target
        // Note: Attack action must be taken with friendly minion first before enemy target.
        private Card card;

        public AttackAction(Card card)
        {
            this.card = card;
        }

        public Card getCard()
        {
            return card;
        }

        public void perform()
        {
            API.attack(card);
        }

        public int delay()
        {
            return 2000;
        }

        public override string ToString()
        {
            return "AttackAction(card=" + card.GetEntity().GetName() + ")";
        }
    }

    public class MouseOverCard : Action
    {
        private Card card;

        public MouseOverCard(Card card)
        {
            this.card = card;
        }

        public int delay()
        {
            return 1000;
        }

        public void perform()
        {
            PrivateHacker.HandleMouseOverCard(card);
        }
    }

    public class MouseOffCard : Action
    {
        public void perform()
        {
            PrivateHacker.HandleMouseOffCard();
        }

        public int delay()
        {
            return 500;
        }
    }
}
