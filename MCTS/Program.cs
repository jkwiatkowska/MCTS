using System;
using System.Collections.Generic;

namespace MCTS
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                TTTGame.PlayGame();

                if (Utility.ReadKey("Play again? Y/N") != ConsoleKey.Y)
                {
                    Environment.Exit(0);
                }
            }
        }

        public static class TTTGame
        {
            public enum eParticipantMode
            {
                AI,
                Input
            }

            const int MaxSimulations = 300000;
            const int MinSimulations = 1000;
            const int MaxGames = 100;
            static int NumberOfGames;

            static int Turn;
            static TTTState CurrentState;

            class Participant
            {
                public TTTParticipant GameParticipant;
                public eParticipantMode ParticipantMode;
                public int SimulationNumber;
                public bool HeavyPlayout;
                public Dictionary<int, int> Results;

                public Participant()
                {
                    Results = new Dictionary<int, int>()
                    {
                        [1] = 0,
                        [0] = 0,
                        [-1] = 0
                    };
                }

                public string Result()
                {
                    return $"[{GameParticipant.Symbol}] W: {Results[1]} D: {Results[0]} Losses: {Results[-1]}";
                }
            }

            static Dictionary<char, Participant> Participants;

            public static void PlayGame()
            {
                Participants = new Dictionary<char, Participant>();
                NumberOfGames = 1;

                foreach (var p in TTTState.InitialPlayers)
                {
                    var text = $"Select mode for player {p}:\n" +
                               $"    1. AI\n" +
                               $"    2. Input";

                    var key = Utility.ReadKey(text);

                    while (key != ConsoleKey.D1 && key != ConsoleKey.D2)
                    {
                        key = Utility.ReadKey("Invalid input.");
                    }

                    var newP = new Participant();
                    newP.GameParticipant = new TTTParticipant(p);

                    if (key == ConsoleKey.D1)
                    {
                        newP.ParticipantMode = eParticipantMode.AI;

                        Console.WriteLine("Iteration number:");

                        var simulationNumber = 0;
                        while(!int.TryParse(Console.ReadLine(), out simulationNumber))
                        {
                            Console.WriteLine("Invalid input.");
                        }

                        if (simulationNumber > MaxSimulations)
                        {
                            Console.WriteLine($"Iteration number too large, decreased to {MaxSimulations}");
                            simulationNumber = MaxSimulations;
                        }

                        if (simulationNumber < MinSimulations)
                        {
                            Console.WriteLine($"Iteration number too small, increased to {MinSimulations}");
                            simulationNumber = MinSimulations;
                        }

                        newP.SimulationNumber = simulationNumber;

                        key = Utility.ReadKey("Heavy playout? (Y/N)");
                        while(key != ConsoleKey.Y && key != ConsoleKey.N)
                        {
                            key = Utility.ReadKey("Invalid input.");
                        }

                        newP.HeavyPlayout = key == ConsoleKey.Y;
                    }
                    else
                    {
                        newP.ParticipantMode = eParticipantMode.Input;
                    }

                    Participants.Add(p, newP);
                }

                var ai = true;
                foreach (var p in TTTState.InitialPlayers)
                {
                    if (Participants[p].ParticipantMode != eParticipantMode.AI)
                    {
                        ai = false;
                        break;
                    }
                }

                if (ai)
                {
                    Console.WriteLine("Number of games:");
                    while (!int.TryParse(Console.ReadLine(), out NumberOfGames))
                    {
                        Console.WriteLine("Invalid input.");
                    }

                    if (NumberOfGames > MaxGames)
                    {
                        Console.WriteLine($"Iteration number too large, decreased to {MaxGames}");
                        NumberOfGames = MaxGames;
                    }

                    if (NumberOfGames < 1)
                    {
                        NumberOfGames = 1;
                    }
                }

                for (int i = 0; i < NumberOfGames; i++)
                {
                    Turn = 0;
                    CurrentState = new TTTState();

                    var player = CurrentState.NextMove() as TTTParticipant;
                    var opponent = CurrentState.Players[0] == player ? CurrentState.Players[1] : CurrentState.Players[0];
                    if (Participants[(player).Symbol].ParticipantMode == eParticipantMode.Input)
                    {
                        CurrentState.ShowBoard();
                    }

                    while (!CurrentState.IsGameOver(CurrentState.NextMove()))
                    {
                        Turn++;
                        Move(player, opponent);

                        opponent = player;
                        player = CurrentState.NextMove() as TTTParticipant;
                    }

                    foreach (var p in Participants)
                    {
                        var result = CurrentState.GameResult(p.Value.GameParticipant);
                        p.Value.Results[result]++;
                    }

                    Console.WriteLine(CurrentState.Result());
                }

                if (NumberOfGames > 1)
                {
                    foreach (var p in Participants)
                    {
                        Console.WriteLine(p.Value.Result());
                    }
                }
            }

            static void Move(TTTParticipant player, TTTParticipant opponent)
            {
                Console.WriteLine($"Turn {Turn} ({player.Symbol}):");

                switch (Participants[player.Symbol].ParticipantMode)
                {
                    case eParticipantMode.AI:
                    {
                        var root = Participants[player.Symbol].HeavyPlayout ? new SearchNodeHeavy(CurrentState, Participants[player.Symbol].SimulationNumber, entity: player, opponent: opponent) :
                                                                              new SearchNode(CurrentState, Participants[player.Symbol].SimulationNumber, entity: player, opponent: opponent);

                        var selectedNode = root.GetBestAction();
                        CurrentState = selectedNode.State as TTTState;
                        CurrentState.ShowBoard();
                        break;
                    }
                    case eParticipantMode.Input:
                    {
                        int x, y;
                        while (true)
                        {
                            while (!int.TryParse(Utility.ReadKeyLine("Column: ").ToString(), out x) && x >= 0 && x < TTTState.BoardSize)
                            {
                                Console.WriteLine($"Invalid input. Input must be between 1 and {TTTState.BoardSize}");
                            }

                            while (!int.TryParse(Utility.ReadKeyLine("Row: ").ToString(), out y) && y >= 0 && y < TTTState.BoardSize)
                            {
                                Console.WriteLine($"Invalid input. Input must be between 1 and {TTTState.BoardSize}");
                            }

                            if (!CurrentState.IsValidMove(y - 1, x - 1))
                            {
                                Console.WriteLine("Invalid Position.");
                                continue;
                            }

                            break;
                        }

                        CurrentState = new TTTState(CurrentState, y - 1, x - 1);
                        CurrentState.ShowBoard();

                        break;
                    }
                }
            }
        }
    }
}
