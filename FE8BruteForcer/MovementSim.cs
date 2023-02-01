using System;
using System.Collections.Generic;

namespace FE8BruteForcer
{
    class MovementSim
    {
        public static bool simSimpleMovement(int totalX, int totalY, int moveChance = 100)
        {
            if (FE8BruteForcer.nextRn() >= moveChance)
            {
                return false;
            };

            int spentX = 0;
            int spentY = 0;

            while (totalX > spentX && totalY > spentY)
            {
                if (FE8BruteForcer.nextRn() < 50)
                {
                    spentX += 1;
                }
                else
                {
                    spentY += 1;
                }
            }

            return true;
        }

        // Grid format:
        // [1-6]: terrain penalty
        // -: untraversable
        // S: start
        // F: finish
        public static bool simComplexMovement(char[,] grid, int moveChance = 100)
        {
            if (FE8BruteForcer.nextRn() >= moveChance)
            {
                return false;
            }

            int[,] costs = createCostsMap(grid, out (int, int) startIndex, out (int, int) currentIndex);

            while (!currentIndex.Equals(startIndex))
            {
                List<(int, int)> validMoves = getValidMoves(costs, currentIndex);
                currentIndex = pickMove(validMoves);
            }

            return true;
        }

        static int[,] createCostsMap(char[,] grid, out (int, int) startIndex, out (int, int) currentIndex)
        {
            int gridHeight = grid.GetLength(0);
            int gridWidth = grid.GetLength(1);
            int[,] costs = initializeCostsGrid(grid, out startIndex, out currentIndex);
            bool costsAllCalculated = false;

            while (!costsAllCalculated)
            {
                costsAllCalculated = true;

                for (int i = 0; i < gridHeight; i++)
                {
                    for (int j = 0; j < gridWidth; j++)
                    {
                        switch (grid[i, j])
                        {
                            case 'S':
                            case '-':
                            case 'F': break;
                            default:
                                int terrainPenalty = grid[i, j] - '0';

                                if (j + 1 != gridWidth && costs[i, j + 1] + terrainPenalty < costs[i, j])
                                {
                                    costs[i, j] = costs[i, j + 1] + terrainPenalty;
                                    costsAllCalculated = false;
                                }
                                if (j != 0 && costs[i, j - 1] + terrainPenalty < costs[i, j])
                                {
                                    costs[i, j] = costs[i, j - 1] + terrainPenalty;
                                    costsAllCalculated = false;
                                }
                                if (i != 0 && costs[i - 1, j] + terrainPenalty < costs[i, j])
                                {
                                    costs[i, j] = costs[i - 1, j] + terrainPenalty;
                                    costsAllCalculated = false;
                                }
                                if (i + 1 != gridHeight && costs[i + 1, j] + terrainPenalty < costs[i, j])
                                {
                                    costs[i, j] = costs[i + 1, j] + terrainPenalty;
                                    costsAllCalculated = false;
                                }

                                break;
                        }
                    }
                }
            }

            return costs;
        }

        static int[,] initializeCostsGrid(char[,] grid, out (int, int) startIndex, out (int, int) currentIndex)
        {
            int gridHeight = grid.GetLength(0);
            int gridWidth = grid.GetLength(1);
            startIndex = (0, 0);
            currentIndex = (0, 0);

            int[,] costs = new int[gridHeight, gridWidth];

            for (int i = 0; i < gridHeight; i++)
            {
                for (int j = 0; j < gridWidth; j++)
                {
                    switch (grid[i, j])
                    {
                        case 'S':
                            costs[i, j] = 0;
                            startIndex = (i, j);
                            break;
                        case 'F':
                            currentIndex = (i, j);
                            goto default;
                        case '-':
                        default:
                            costs[i, j] = 100;
                            break;
                    }
                }
            }

            return costs;
        }

        // It is important to evaluate in the order right left up down. This is the order that the RNG uses to pick a path.
        static List<(int, int)> getValidMoves(int[,] costs, (int, int) currentIndex)
        {
            int[] adjacentCosts = new int[] { 100, 100, 100, 100 }; // right left up down
            int lowestCost = 100;

            List<(int, int)> validMoves = new List<(int, int)>();

            if (currentIndex.Item2 + 1 != costs.GetLength(1)) // right
            {
                adjacentCosts[0] = costs[currentIndex.Item1, currentIndex.Item2 + 1];
            }

            if (currentIndex.Item2 != 0) // left
            {
                adjacentCosts[1] = costs[currentIndex.Item1, currentIndex.Item2 - 1];
            }

            if (currentIndex.Item1 != 0) // up
            {
                adjacentCosts[2] = costs[currentIndex.Item1 - 1, currentIndex.Item2];
            }

            if (currentIndex.Item1 + 1 != costs.GetLength(0)) // down
            {
                adjacentCosts[3] = costs[currentIndex.Item1 + 1, currentIndex.Item2];
            }

            for (int i = 0; i < adjacentCosts.Length; i++)
            {
                lowestCost = Math.Min(lowestCost, adjacentCosts[i]);
            }

            if (adjacentCosts[0] == lowestCost)
            {
                validMoves.Add((currentIndex.Item1, currentIndex.Item2 + 1));
            }

            if (adjacentCosts[1] == lowestCost)
            {
                validMoves.Add((currentIndex.Item1, currentIndex.Item2 - 1));
            }

            if (adjacentCosts[2] == lowestCost)
            {
                validMoves.Add((currentIndex.Item1 - 1, currentIndex.Item2));
            }

            if (adjacentCosts[3] == lowestCost)
            {
                validMoves.Add((currentIndex.Item1 + 1, currentIndex.Item2));
            }

            return validMoves;
        }

        static (int, int) pickMove(List<(int, int)> validMoves)
        {
            int roll = (validMoves.Count == 1) ? 0 : FE8BruteForcer.nextRnTrue();
            int index = roll * validMoves.Count / 65536;
            return validMoves[index];
        }
    }
}