using System;
using System.Linq;

namespace FEBruteForcer
{
    class EnemyStatSim
    {
        /// <param name="growthRates">an array of the unit's growth rates in the order [hp, str, skl, spd, def, res, lck]</param>
        /// <param name="levels">how many levels were gained</param>
        /// <param name="rollLuck">whether to roll luck levels. fe7 enemies are all luckcels</param>
        /// <returns>an array of how many levels were gained per stat in the order [hp, str, skl, spd, def, res, lck]</returns>
        private static int[] rollStats(int[] growthRates, int levels, bool rollLuck)
        {
            int length = rollLuck ? 7 : 6;
            int[] coeffs = new int[length];
            int[] rns = new int[length];
            coeffs[0] = FEBruteForcer.nextRn();
            rns[0] = FEBruteForcer.nextRn();
            coeffs[1] = FEBruteForcer.nextRn();
            rns[1] = FEBruteForcer.nextRn();
            coeffs[2] = FEBruteForcer.nextRn();
            rns[2] = FEBruteForcer.nextRn();
            coeffs[3] = FEBruteForcer.nextRn();
            rns[3] = FEBruteForcer.nextRn();
            coeffs[4] = FEBruteForcer.nextRn();
            rns[4] = FEBruteForcer.nextRn();
            coeffs[5] = FEBruteForcer.nextRn();
            rns[5] = FEBruteForcer.nextRn();
            if (rollLuck)
            {
                coeffs[6] = FEBruteForcer.nextRn();
                rns[6] = FEBruteForcer.nextRn();
            }

            int[] procs = new int[length];
            for (int i = 0; i < length; i++)
            {
                double baserate = growthRates[i] * levels / 100.0;
                double adjusted = baserate * (0.875 + ((coeffs[i] / 100.0) * 0.25));
                procs[i] = (int)Math.Truncate(adjusted);
                double residue = adjusted - Math.Truncate(adjusted);
                if (rns[i] < (int)(100 * residue))
                {
                    procs[i]++;
                }
            }
            return procs;
        }

        public static int[] rollEnemy(int[] growthRates, int unpromotedLevels, int promotedLevels, int hardModeLevels)
        {
            bool rollLuck = (FEBruteForcer.game == 7 ? false : true);
            int length = (!rollLuck ? 6 : 7);
            int[] zeroes = new int[length];
            Array.Fill(zeroes, 0);

            int[] unpromotedGains = (unpromotedLevels > 0) ? rollStats(growthRates, unpromotedLevels, rollLuck) : zeroes;
            int[] promotedGains = (promotedLevels > 0) ? rollStats(growthRates, promotedLevels, rollLuck) : zeroes;
            int[] hmGains = (hardModeLevels > 0) ? rollStats(growthRates, hardModeLevels, rollLuck) : zeroes;
            int[] finalGains = new int[length];
            for (int i = 0; i < length; i++)
            {
                finalGains[i] = unpromotedGains[i] + promotedGains[i] + hmGains[i];
            }
            return finalGains;
        }

        public static int[][] rollEnemies(Enemy[] enemies, bool doubleHmb)
        {
            int[][] grownStats = new int[enemies.Length][];

            for (int i = 0; i < enemies.Length; i++)
            {
                grownStats[i] = rollEnemy(enemies[i].growthRates, enemies[i].unpromotedLevels, enemies[i].promotedLevels, enemies[i].hardModeLevels);
            }
            if (doubleHmb)
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    int[] doubledHmb = rollEnemy(enemies[i].growthRates, 0, 0, enemies[i].hardModeLevels);
                    grownStats[i] = grownStats[i].Zip(doubledHmb, (a, b) => a + b).ToArray();
                }
            }

            return grownStats;
        }
    }

    public class Enemy
    {
        public int[] growthRates = new int[] { 0, 0, 0, 0, 0, 0, 0 };
        public int unpromotedLevels = 0;
        public int promotedLevels = 0;
        public int hardModeLevels = 0;
    }
}
