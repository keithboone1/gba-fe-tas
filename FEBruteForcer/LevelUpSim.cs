namespace FEBruteForcer
{
    class LevelUpSim
    {
        /// <param name="growthRates">array in the order [hp, str, skl, spd, def, res, lck].</param>
        /// <returns>an array of grown stats in the order [hp, str, skl, spd, def, res, lck]. [2 1 0 0 1 0 1] = +2 HP, +1 Str, +1 Def, +1 Lck.</returns>
        public static int[] simLevel(int[] growthRates)
        {
            int[] leveled = new int[7];
            bool anyLeveled = false;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; i < 7; i++)
                {
                    int guaranteed = growthRates[j] / 100;
                    int randomPortion = growthRates[j] % 100;
                    int randomLevel = FEBruteForcer.nextRn() < randomPortion ? 1 : 0;
                    leveled[j] = guaranteed + randomLevel;
                    anyLeveled = anyLeveled || (leveled[j] > 0);
                }

                if (anyLeveled)
                {
                    break;
                }
            }

            return leveled;
        }
    }

    public enum Stats
    {
        HP = 0,
        Str = 1,
        Skl = 2,
        Spd = 3,
        Def = 4,
        Res = 5,
        Lck = 6,
    }
}
