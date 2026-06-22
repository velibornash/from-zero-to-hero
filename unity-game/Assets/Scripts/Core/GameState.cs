using System;
using UnityEngine;

namespace FromZeroToHero.Core
{
    [Serializable]
    public sealed class GameState
    {
        [Header("Resources")]
        public int Wood = 24;
        public int Food = 18;
        public int Gold = 12;

        [Header("Village")]
        public int Morale = 50;
        public int Population = 5;
        public int Workers = 0;
        public int BaseHp = 100;
        public int Day = 1;

        [Header("Story Flags")]
        public bool FirstFire;
        public bool Wolves;
        public bool Ivan;
        public bool Dragon;
        public bool Marko;

        public bool CanAfford(int wood, int food, int gold)
        {
            return Wood >= wood && Food >= food && Gold >= gold;
        }

        public void Spend(int wood, int food, int gold)
        {
            Wood -= wood;
            Food -= food;
            Gold -= gold;
        }

        public void AddWood(int amount) => Wood += amount;
        public void AddFood(int amount) => Food += amount;
        public void AddGold(int amount) => Gold += amount;
    }
}
