using System;
using System.Collections.Generic;
using System.IO;

namespace CatAndMouseGame
{
    enum State
    {
        Winner,
        Looser,
        Playing,
        NotInGame
    }

    enum GameState
    {
        Start,
        End
    }

    class Player
    {
        public string name;
        public int location;
        public State state = State.NotInGame;
        public int distanceTraveled = 0;

        public Player(string name)
        {
            this.name = name;
            this.location = 1;
        }