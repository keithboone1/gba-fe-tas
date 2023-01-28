using System;

namespace FE8BruteForcer
{
    class EnemyStatSim
    {
        /// <param name="growths">an array of the unit's growths in the order [hp, str, skl, spd, def, res, lck]</param>
        /// <param name="levels">how many levels were gained</param>
        /// <param name="rollLuck">whether to roll luck levels. fe7 enemies are all luckcels</param>
        /// <returns>an array of how many levels were gained per stat in the order [hp, str, skl, spd, def, res, lck]</returns>
        private static int[] rollStats(ushort[] currentRns, int[] growths, int levels, bool rollLuck)
        {
            int length = rollLuck ? 7 : 6;
            int[] coeffs = new int[length];
            int[] rns = new int[length];
            coeffs[0] = FE8BruteForcer.advanceRng(currentRns);
            rns[0] = FE8BruteForcer.advanceRng(currentRns);
            coeffs[1] = FE8BruteForcer.advanceRng(currentRns);
            rns[1] = FE8BruteForcer.advanceRng(currentRns);
            coeffs[2] = FE8BruteForcer.advanceRng(currentRns);
            rns[2] = FE8BruteForcer.advanceRng(currentRns);
            coeffs[3] = FE8BruteForcer.advanceRng(currentRns);
            rns[3] = FE8BruteForcer.advanceRng(currentRns);
            coeffs[4] = FE8BruteForcer.advanceRng(currentRns);
            rns[4] = FE8BruteForcer.advanceRng(currentRns);
            coeffs[5] = FE8BruteForcer.advanceRng(currentRns);
            rns[5] = FE8BruteForcer.advanceRng(currentRns);
            if (rollLuck)
            {
                coeffs[6] = FE8BruteForcer.advanceRng(currentRns);
                rns[6] = FE8BruteForcer.advanceRng(currentRns);
            }

            int[] procs = new int[length];
            for (int i = 0; i < length; i++)
            {
                double baserate = growths[i] * levels / 100.0;
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

        public static int[] rollFE6Enemy(ushort[] currentRns, int[] growths, bool isPromoted, int levels, int hardModeLevels, bool doubleHMB)
        {
            int[] promotedGains = (isPromoted) ? rollStats(currentRns, growths, 20, true) : new int[] { 0, 0, 0, 0, 0, 0, 0 };
            int[] normalGains = (levels > 0) ? rollStats(currentRns, growths, levels, true) : new int[] { 0, 0, 0, 0, 0, 0, 0 };
            int[] hmGains = (hardModeLevels > 0) ? rollStats(currentRns, growths, hardModeLevels, true) : new int[] { 0, 0, 0, 0, 0, 0, 0 };
            int[] hmGains2 = (doubleHMB) ? rollStats(currentRns, growths, hardModeLevels, true) : new int[] { 0, 0, 0, 0, 0, 0, 0 };
            int[] finalGains = new int[7];
            for (int i = 0; i < 7; i++)
            {
                finalGains[i] = promotedGains[i] + normalGains[i] + hmGains[i] + hmGains2[i];
            }
            return finalGains;
        }

        public static int[] rollFE7Enemy(ushort[] currentRns, int[] growths, bool isPromoted, int levels, bool giveHardModeLevels)
        {
            int[] promotedGains = (isPromoted) ? rollStats(currentRns, growths, 20, true) : new int[] { 0, 0, 0, 0, 0, 0, 0 };
            int[] normalGains = (levels > 0) ? rollStats(currentRns, growths, levels, false) : new int[] { 0, 0, 0, 0, 0, 0 };
            int[] hmGains = (giveHardModeLevels) ? rollStats(currentRns, growths, 5, false) : new int[] { 0, 0, 0, 0, 0, 0 };
            int[] finalGains = new int[6];
            for (int i = 0; i < 6; i++)
            {
                finalGains[i] = promotedGains[i] + normalGains[i] + hmGains[i];
            }
            return finalGains;
        }

        public static int[] rollFE8Enemy(ushort[] currentRns, int[] growths, bool isPromoted, int levels, int hardModeLevels)
        {
            int[] promotedGains = (isPromoted) ? rollStats(currentRns, growths, 20, true) : new int[] { 0, 0, 0, 0, 0, 0, 0 };
            int[] normalGains = (levels > 0) ? rollStats(currentRns, growths, levels, true) : new int[] { 0, 0, 0, 0, 0, 0, 0 };
            int[] hmGains = (hardModeLevels > 0) ? rollStats(currentRns, growths, hardModeLevels, true) : new int[] { 0, 0, 0, 0, 0, 0, 0 };
            int[] finalGains = new int[7];
            for (int i = 0; i < 7; i++)
            {
                finalGains[i] = promotedGains[i] + normalGains[i] + hmGains[i];
            }
            return finalGains;
        }
    }
}
