using System;
using System.Collections.Generic;
using System.Text;

namespace MCTS
{
    public class TTTState : IState
    {
        public const int BoardSize = 3;

        static List<List<char>> InitialState = new List<List<char>>()
        {
            new List<char>(){' ', ' ', ' ' },
            new List<char>(){' ', ' ', ' ' },
            new List<char>(){' ', ' ', ' ' },
        };

        public static List<char> InitialPlayers => new List<char> { 'X', 'O' };

        List<List<char>> Board;

        public List<TTTParticipant> Players;                                        // Player symbols are placed on the board.
        int NextPlayerIndex;                                                        // The index of the player that will make the next move.

        public TTTState()
        {
            Board = InitialState;
            Players = new List<TTTParticipant>();
            foreach (var p in InitialPlayers)
            {
                Players.Add(new TTTParticipant(p));
            }
        }

        public TTTState(TTTState previousState, TTTAction action)
        {
            Board = new List<List<char>>();
            Players = previousState.Players;

            for (int i = 0; i < BoardSize; i++)
            {
                Board.Add(new List<char>());
                for (int j = 0; j < BoardSize; j++)
                {
                    Board[i].Add(previousState.Board[i][j]);
                }
            }

            Board[action.X][action.Y] = Players[action.PlayerIndex].Symbol;

            NextPlayerIndex = action.PlayerIndex + 1;
            if (NextPlayerIndex >= Players.Count)
            {
                NextPlayerIndex = 0;
            }
        }

        public TTTState(TTTState previousState, int x, int y) : this(previousState, new TTTAction(x, y, previousState.NextPlayerIndex))
        {
            
        }

        public void ShowBoard()
        {
            for (int i = 0; i < BoardSize; i++)
            {
                Console.Write("    ");
                Console.WriteLine(new string('-', BoardSize * 3 + 1));
                Console.Write("    |");
                for (int j = 0; j < BoardSize; j++)
                {
                    Console.Write($"{Board[i][j]} |");
                }
                Console.Write("\n");
            }
            Console.Write("    ");
            Console.WriteLine(new string('-', BoardSize * 3 + 1));

            Console.Write("\n");
        }

        public int GameResult(IParticipant participant)
        {
            var p = participant as TTTParticipant;
            if (TryHorizontal(p, out var result))
            {
                return result;
            }

            if (TryVertical(p, out result))
            {
                return result;
            }

            if (TryDiagonal(p, out result))
            {
                return result;
            }

            if (TryAntiDiagonal(p, out result))
            {
                return result;
            }

            return 0;
        }

        public string Result()
        {
            foreach (var p in Players)
            {
                if (GameResult(p) == 1)
                {
                    return $"{p.Symbol} WINS";
                }
            }

            return "DRAW";
        }

        #region Board Checks
        bool TryHorizontal(TTTParticipant participant, out int result)
        {
            result = 0;

            for (int i = 0; i < BoardSize; i++)
            {
                var last = Board[0][i];

                var end = true;

                for (int j = 0; j < BoardSize; j++)
                {
                    if (last != Board[j][i])
                    {
                        end = false;
                        break;
                    }

                    last = Board[j][i];
                }

                if (end && last != ' ')
                {
                    if (participant.Symbol == last)
                    {
                        result = 1;
                    }
                    else
                    {
                        result = -1;
                    }

                    return true;
                }
            }

            return false;
        }

        bool TryVertical(TTTParticipant participant, out int result)
        {
            result = 0;

            for (int i = 0; i < BoardSize; i++)
            {
                var last = Board[i][0];
                var end = true;

                for (int j = 0; j < BoardSize; j++)
                {
                    if (last != Board[i][j])
                    {
                        end = false;
                        break;
                    }

                    last = Board[i][j];
                }

                if (end && last != ' ')
                {
                    if (participant.Symbol == last)
                    {
                        result = 1;
                    }
                    else
                    {
                        result = -1;
                    }

                    return true;
                }
            }

            return false;
        }

        bool TryDiagonal(TTTParticipant participant, out int result)
        {
            result = 0;

            var last = Board[0][0];
            var end = true;

            for (int i = 1; i < BoardSize; i++)
            {
                end = last == Board[i][i];

                if (!end)
                {
                    return false;
                }

                last = Board[i][i];
            }

            if (end && last != ' ')
            {
                if (participant.Symbol == last)
                {
                    result = 1;
                }
                else
                {
                    result = -1;
                }

                return true;
            }

            return false;
        }

        bool TryAntiDiagonal(TTTParticipant participant, out int result)
        {
            result = 0;

            var last = Board[0][BoardSize - 1];
            var end = true;

            for (int i = 1; i < BoardSize; i++)
            {
                end = last == Board[i][BoardSize - 1 - i];

                if (!end)
                {
                    return false;
                }

                last = Board[i][BoardSize - 1 - i];
            }

            if (end && last != ' ')
            {
                if (participant.Symbol == last)
                {
                    result = 1;
                }
                else
                {
                    result = -1;
                }

                return true;
            }

            return false;
        }
        #endregion

        public List<IAction> GetLegalActions()
        {
            var list = new List<IAction>();

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    if (Board[i][j] == ' ')
                    {
                        list.Add(new TTTAction(i, j, NextPlayerIndex));
                    }
                }
            }

            return list;
        }

        public bool IsValidMove(int x, int y)
        {
            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize)
            {
                return false;
            }

            return Board[x][y] == ' ';
        }

        public bool IsGameOver(IParticipant participant)
        {
            if (GameResult(participant) != 0)
            {
                return true;
            }

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    if (Board[i][j] == ' ')
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public IState Move(IAction action)
        {
            return new TTTState(this, action as TTTAction);
        }

        public IParticipant NextMove()
        {
            return Players[NextPlayerIndex];
        }
    }

    public class TTTAction : IAction
    {
        public int X;
        public int Y;
        public int PlayerIndex;
    
        public TTTAction(int x, int y, int playerIndex)
        {
            X = x;
            Y = y;
    
            PlayerIndex = playerIndex;
        }
    }
    
    public class TTTParticipant : IParticipant
    {
        public char Symbol;

        public TTTParticipant(char symbol)
        {
            Symbol = symbol;
        }
    }
}

