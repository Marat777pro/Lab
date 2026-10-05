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
        public void Move(int steps, int fieldSize)
        {
            if (state == State.NotInGame)
            {
                this.location = steps;
                this.state = State.Playing;
                return;
            }

            this.distanceTraveled += Math.Abs(steps);

            int zeroBasedLocation = this.location - 1;
            int newLocation = (zeroBasedLocation + steps) % fieldSize;

            if (newLocation < 0)
            {
                newLocation += fieldSize;
            }

            this.location = newLocation + 1;
        }
    }

    class Game
    {
        public static string InputFile;
        public static string OutFile;

        public int size;
        public Player cat;
        public Player mouse;
        public GameState state;

        private List<string> pOutputs = new List<string>();

        public Game(int size)
        {
            this.size = size;
            cat = new Player("Cat");
            mouse = new Player("Mouse");
            state = GameState.Start;
        }

        public void Run()
        {
            if (!File.Exists(InputFile))
            {
                Console.WriteLine($"Ошибка: Входной файл {InputFile} не найден.");
                state = GameState.End;
                return;
            }

            string[] rawLines = File.ReadAllLines(InputFile);
            List<string> commandsLines = new List<string>();

            foreach (var line in rawLines)
            {
                string trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("//"))
                {
                    commandsLines.Add(trimmed);
                }
            }

            int currentLineIndex = 1;

            while (state != GameState.End)
            {
                if (currentLineIndex >= commandsLines.Count)
                {
                    state = GameState.End;
                    break;
                }

                string line = commandsLines[currentLineIndex];
                currentLineIndex++;

                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                char commandChar = parts[0][0];

                if (commandChar == 'P')
                {
                    DoPrintCommand();
                }
                else if (commandChar == 'M' || commandChar == 'C')
                {
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int steps))
                    {
                        DoMoveCommand(commandChar, steps);
                    }
                }

                if (cat.state == State.Playing && mouse.state == State.Playing && cat.location == mouse.location)
                {
                    cat.state = State.Winner;
                    mouse.state = State.Looser;
                    state = GameState.End;
                }
            }

            if (cat.state != State.Winner)
            {
                cat.state = State.Looser;
                mouse.state = State.Winner;
            }

            SaveOutputLog();
        }