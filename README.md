# MCTS
Monte Carlo Tree Search Implementation

This solution implements the Monte Carlo search algorithm with the goal of creating an AI that can be used in turn-based games. A set of interfaces is used to represent the game state, actions and players and an example implementation based on a game of tic tac toe was written to test the MTCS implementation. The search itself can also be expanded upon to account for criteria specific to an implemented game - in this case, the search can optionally predict the most likely opponent moves when traversing the tree, resulting in a more defensive strategy.
