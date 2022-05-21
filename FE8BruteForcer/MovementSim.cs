using System;

namespace FE8BruteForcer
{
    class MovementSim
    {
        public static bool simSimpleMovement(ushort[] currentRns, int totalX, int totalY, int moveChance = 100)
        {
            if (FE8BruteForcer.advanceRng(currentRns) >= moveChance)
            {
                return false;
            };

            int spentX = 0;
            int spentY = 0;

            while (totalX > spentX && totalY > spentY)
            {
                if (FE8BruteForcer.advanceRng(currentRns) < 50)
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
        public static bool simComplexMovement(ushort[] currentRns, char[,] grid, int moveChance = 100)
        {
            if (FE8BruteForcer.advanceRng(currentRns) >= moveChance)
            {
                return false;
            }

            int[,] costs = createCostsMap(grid, out (int, int) startIndex, out (int, int) currentIndex);

            while (!currentIndex.Equals(startIndex))
            {
                bool[] validMoves = getValidMoves(costs, currentIndex, out int validMoveCount);

                currentIndex = moveOneSpace(currentRns, validMoves, validMoveCount, currentIndex);
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
        static bool[] getValidMoves(int[,] costs, (int, int) currentIndex, out int validMoveCount)
        {
            int[] adjacentCosts = new int[] { 100, 100, 100, 100 }; // right left up down
            int lowestCost = 100;

            bool[] validMoves = new bool[] { false, false, false, false }; // right left up down
            validMoveCount = 0;

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

            for (int i = 0; i < adjacentCosts.Length; i++)
            {
                if (adjacentCosts[i] == lowestCost)
                {
                    validMoves[i] = true;
                    validMoveCount += 1;
                }
            }

            return validMoves;
        }

        static (int, int) moveOneSpace(ushort[] currentRns, bool[] validMoves, int validMoveCount, (int, int) currentIndex)
        {

            int roll = (validMoveCount == 1) ? 0 : FE8BruteForcer.advanceRng(currentRns);
            int passIfUnder = (100 / validMoveCount);

            for (int i = 0; i < validMoves.Length; i++)
            {
                if (!validMoves[i])
                {
                    continue;
                }
                if (roll >= passIfUnder)
                {
                    passIfUnder += (100 / validMoveCount);
                    if (passIfUnder == 66)
                    {
                        passIfUnder += 1; // easier than using floats or something
                    }
                    continue;
                }

                switch (i)
                {
                    case 0: return (currentIndex.Item1, currentIndex.Item2 + 1);
                    case 1: return (currentIndex.Item1, currentIndex.Item2 - 1);
                    case 2: return (currentIndex.Item1 - 1, currentIndex.Item2);
                    case 3: return (currentIndex.Item1 + 1, currentIndex.Item2);
                    default: break;
                }
            }

            throw new Exception();
        }
    }
}