﻿using System;
using System.Linq;

namespace FEBruteForcer
{
    class MapLoadingSim
    {
        public static void SetValniMovers(ValniEnemy[] enemies, int numberOfMovers)
        {
            int assignedMovers = 0;
            ValniEnemy[] possibleMovers = enemies.Where(e => e.special).ToArray();

            while (assignedMovers < numberOfMovers)
            {
                int newIndex = FEBruteForcer.nextRnTrue() * possibleMovers.Length / 65536;
                if (!possibleMovers[newIndex].moves)
                {
                    assignedMovers += 1;
                    possibleMovers[newIndex].moves = true;
                }
            }
        }

        public static ValniEnemyOutput SimValniEnemy(ValniEnemy input)
        {
            ValniEnemyOutput output = new ValniEnemyOutput();

            if (input.monster)
            {
                //class
                FEBruteForcer.nextRn();

                //level
                FEBruteForcer.nextRn();
                int LEVELS_PLACEHOLDER = 1; // until i figure out how valni level generation works, this will at least cause stat rolls to happen.

                //held item
                FEBruteForcer.nextRn();
                FEBruteForcer.nextRn();

                //dropped item
                bool doesDrop = (FEBruteForcer.nextRn() < input.dropRate);
                if (doesDrop)
                {
                    FEBruteForcer.nextRn();
                }

                //stat rolls
                EnemyStatSim.rollEnemy(input.growthRates, input.givePromoAutolevels ? 19 : 0, LEVELS_PLACEHOLDER, input.hmLevels);
            } 
            else
            {
                EnemyStatSim.rollEnemy(input.growthRates, input.givePromoAutolevels ? 19 : 0, input.level, input.hmLevels);
            }

            // movement
            if (input.moves)
            {
                output.position = (octodirection)(FEBruteForcer.nextRnTrue() * 8 / 65536);
            }

            return output;
        }

        /* How to use:
         * You'll need to burn 1 RN to determine how many enemies move. This appears to be hard-coded based on the number of enemies in Valni.
         * Then, pass in an array of ValniEnemies and the number of movers. The function will return an array of their rolled positions.
         * NOTE that this is not their final positions, because it doesn't handle collision checks. You'll need to run those by hand afterward.
         * In the future I'd like to handle stat generation, weapon drops, etc., but I'm lazy and don't tend to code stuff until someone needs it for an LTC.
         */
        public static ValniEnemyOutput[] SimValni(ValniEnemy[] enemies, int numberOfMovers)
        {
            ValniEnemyOutput[] outputs = new ValniEnemyOutput[enemies.Length];

            SetValniMovers(enemies, numberOfMovers);

            for (int i = 0; i < enemies.Length; i += 1)
            {
                outputs[i] = SimValniEnemy(enemies[i]);
            }

            return outputs;
        }
    }

    public class ValniEnemy
    {
        public bool special = false;
        public bool monster = false;
        public int dropRate = 0;
        public int[] growthRates = new int[] { 0, 0, 0, 0, 0, 0, 0 };
        public bool givePromoAutolevels = false;
        public int level = 0;
        public int hmLevels = 3;
        public bool moves = false;
    }

    public class ValniEnemyOutput
    {
        public octodirection position = octodirection.noMove;
    }

    public enum octodirection
    {
        upLeft = 0,
        up,
        upRight,
        left,
        right,
        downLeft,
        down,
        downRight,
        noMove,
    }
}
