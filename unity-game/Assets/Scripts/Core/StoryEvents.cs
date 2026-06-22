using System;
using System.Collections.Generic;
using UnityEngine;

namespace FromZeroToHero.Core
{
    public sealed class StoryEvents
    {
        private readonly List<StoryEvent> events = new();

        public StoryEvents()
        {
            events.Add(new FirstFireEvent());
            events.Add(new WolvesEvent());
            events.Add(new IvanEvent());
            events.Add(new DragonEvent());
            events.Add(new MarkoEvent());
        }

        public bool TryAdvance(GameState state, Action<string, string> onEvent)
        {
            foreach (StoryEvent ev in events)
            {
                if (ev.Completed || !ev.CanTrigger(state))
                    continue;

                ev.Apply(state);
                ev.Completed = true;
                onEvent?.Invoke(ev.Title, ev.Description);
                return true;
            }

            return false;
        }
    }

    public abstract class StoryEvent
    {
        public bool Completed { get; set; }
        public abstract string Title { get; }
        public abstract string Description { get; }
        public abstract bool CanTrigger(GameState state);
        public abstract void Apply(GameState state);
    }

    public sealed class FirstFireEvent : StoryEvent
    {
        public override string Title => "Prvi oganj sela";
        public override string Description => "Selo se stabilizuje. Osnovni svet i ekonomija se otključavaju.";
        public override bool CanTrigger(GameState state) => state.Day >= 1 && !state.FirstFire;
        public override void Apply(GameState state)
        {
            state.FirstFire = true;
            state.AddWood(6);
            state.AddFood(4);
        }
    }

    public sealed class WolvesEvent : StoryEvent
    {
        public override string Title => "Vukovi sa Rtnja";
        public override string Description => "Prvi talas napada testira odbranu sela.";
        public override bool CanTrigger(GameState state) => state.Wood >= 20 && !state.Wolves;
        public override void Apply(GameState state)
        {
            state.Wolves = true;
            state.Morale = Mathf.Max(0, state.Morale - 2);
        }
    }

    public sealed class IvanEvent : StoryEvent
    {
        public override string Title => "Senka Kosančića Ivana";
        public override string Description => "Moral postaje deo gameplay-a.";
        public override bool CanTrigger(GameState state) => state.Morale >= 55 && !state.Ivan;
        public override void Apply(GameState state)
        {
            state.Ivan = true;
            state.Morale = Mathf.Min(100, state.Morale + 10);
        }
    }

    public sealed class DragonEvent : StoryEvent
    {
        public override string Title => "Zmaj sa planine";
        public override string Description => "Otvara se prvi boss događaj.";
        public override bool CanTrigger(GameState state) => state.Day >= 4 && !state.Dragon;
        public override void Apply(GameState state)
        {
            state.Dragon = true;
            state.AddGold(8);
        }
    }

    public sealed class MarkoEvent : StoryEvent
    {
        public override string Title => "Dolazak Kraljevića Marka";
        public override string Description => "Legenda stiže kada je selo najugroženije.";
        public override bool CanTrigger(GameState state) => state.BaseHp <= 30 && !state.Marko;
        public override void Apply(GameState state)
        {
            state.Marko = true;
            state.BaseHp = Mathf.Min(100, state.BaseHp + 25);
        }
    }
}
