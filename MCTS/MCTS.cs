using System;
using System.Collections.Generic;
using System.Text;

namespace MCTS
{
    // State of a game.
    public interface IState
    {
        public List<IAction> GetLegalActions();             // Returns a list of all possible actions from this state.

        public IState Move(IAction action);                 // Changes the state with a new value.

        public bool IsGameOver(IParticipant participant);   // Returns true if the game is over.

        public int GameResult(IParticipant participant);    // Returns 1 on win, 0 on draw and -1 on loss.

        public IParticipant NextMove();                     // Returns the participant that can move next.
    }

    // A move that creates a new state of the game.
    public interface IAction
    {

    }

    // An entity such as player or AI that takes part in the game.
    public interface IParticipant
    {

    }

    public class SearchNode
    {
        public IState State;                                    // State of the game
        public IParticipant Entity;                             // Entity performing the search.
        public IParticipant Opponent;                           // Entity being played against.

        public SearchNode Parent;                               // If it's a child node, this is equal to the node that it was derived from. Null for the root node.
        public IAction ParentAction;                            // Action that led to this node. 

        public List<SearchNode> Children;                       // All possible actions from the current node.

        public int NumberOfVisits { get; private set; }         // Number of times this node is visited.
        protected Dictionary<int, int> Results;                 // Results from each visit are counted in this dictionary. 1 is wins and -1 is losses. 
        protected List<IAction> UntriedActions;                 // List of all possible actions.

        protected int SimulationNumber;                         // Number of search iterations 

        public bool IsTerminalNode => State.IsGameOver(Entity);
        public bool IsFullyExpanded => UntriedActions.Count == 0;

        protected static Random Random = new Random();

        public SearchNode(IState state, int simulationNumber, SearchNode parent = null, IAction parentAction = null, IParticipant entity = null, IParticipant opponent = null)
        {
            State = state;
            Entity = entity;
            Opponent = opponent;
            Parent = parent;
            ParentAction = parentAction;
            Children = new List<SearchNode>();

            SimulationNumber = simulationNumber;

            NumberOfVisits = 0;
            Results = new Dictionary<int, int>()
            {
                [-1] = 0,
                [0]  = 0,
                [1]  = 0
            };

            UntriedActions = state.GetLegalActions();
        }

        protected int WinLossDifference()
        {
            var wins = Results[1];
            var losses = Results[-1];

            return wins - losses;
        }

        // Generate next state from the current one depending on the carried out action.
        protected virtual SearchNode Expand()
        {
            var last = UntriedActions.Count - 1;
            var action = UntriedActions[last];
            UntriedActions.RemoveAt(last);

            var nextState = State.Move(action);
            var childNode = new SearchNode(nextState, SimulationNumber, this, action, Entity, Opponent);
            Children.Add(childNode);

            return childNode;
        }


        // Picks a random move
        protected virtual IAction ChooseMove(List<IAction> possibleMoves, IState currentState)
        {
            var r = Random.Next(0, possibleMoves.Count);
            return possibleMoves[r];
        }

        // Picks a node to run rollout on.
        protected virtual SearchNode ChooseRolloutNode()
        {
            var current = this;

            while (!current.IsTerminalNode)
            {
                if (!current.IsFullyExpanded)
                {
                    return current.Expand();
                }
                else
                {
                    current = current.BestChild();
                }
            }

            return current;
        }

        // Simulate the game from the current state until an outcome is found. 
        protected IState Rollout()
        {
            var currentRolloutState = State;

            while (!currentRolloutState.IsGameOver(Entity))
            {
                var possibleMoves = currentRolloutState.GetLegalActions();

                var action = ChooseMove(possibleMoves, currentRolloutState);
                currentRolloutState = currentRolloutState.Move(action);
            }

            return currentRolloutState;
        }

        // Update the statistics for all nodes when a result is reached.
        protected void Backpropagate(int result)
        {
            NumberOfVisits++;
            Results[result]++;
            Parent?.Backpropagate(result);
        }

        protected SearchNode BestChild(float explorationParameter = 0.1f)
        {
            var bestWeight = float.MinValue;
            var bestIndex = 0;

            for (int i = 0; i < Children.Count; i++)
            {
                var weight = UCT(Children[i], explorationParameter);
                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    bestIndex = i;
                }
            }

            return Children[bestIndex];
        }

        protected float UCT(SearchNode child, float explorationParameter)
        {
            return ((float)child.WinLossDifference() / child.NumberOfVisits) + explorationParameter * MathF.Sqrt(2 * MathF.Log((float)NumberOfVisits / child.NumberOfVisits));
        }

        public virtual SearchNode BestAction(int simulationNumber)
        {
            for (int i = 0; i < simulationNumber; i++)
            {
                var rolloutNode = ChooseRolloutNode();
                var result = rolloutNode.Rollout().GameResult(Entity);
                rolloutNode.Backpropagate(result);
            }

            return BestChild(explorationParameter: 0);
        }

        // This function returns the node that corresponds to the best possible move.
        public SearchNode GetBestAction()
        {
            return BestAction(SimulationNumber);
        }
    }

    public class SearchNodeHeavy : SearchNode
    {
        public SearchNodeHeavy(IState state, int simulationNumber, IParticipant entity = null, IParticipant opponent = null, SearchNode parent = null, IAction parentAction = null) 
            : base(state, simulationNumber, parent, parentAction, entity, opponent)
        {

        }

        protected override IAction ChooseMove(List<IAction> possibleMoves, IState currentState)
        {
            var opponentMove = currentState.NextMove() == Opponent;

            if (opponentMove)
            {
                foreach (var move in possibleMoves)
                {
                    // If this move will lead to the entity losing, then the opponent will most likely select it. 
                    if (currentState.Move(move).IsGameOver(Entity))
                    {
                        return move;
                    }
                }

                // If no end game states exist, try picking one that the opponent is likely to choose.
                var node = new SearchNode(currentState, SimulationNumber, entity: Opponent, opponent: Entity);
                return node.BestAction(SimulationNumber / 2).ParentAction;
            }


            var r = Random.Next(0, possibleMoves.Count);
            return possibleMoves[r];
        }

        // Generate next state from the current one depending on the carried out action.
        protected override SearchNode Expand()
        {
            var last = UntriedActions.Count - 1;
            var action = UntriedActions[last];
            UntriedActions.RemoveAt(last);

            var nextState = State.Move(action);
            var childNode = new SearchNode(nextState, SimulationNumber, entity: Entity, opponent: Opponent, parent: this, parentAction: action);
            Children.Add(childNode);

            return childNode;
        }
    }
}
