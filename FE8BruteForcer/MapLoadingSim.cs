namespace FE8BruteForcer
{
    class MapLoadingSim
    {
        public static void SetValniMovers(ushort[] currentRns, ValniEnemy[] enemies, int numberOfMovers)
        {
            int assignedMovers = 0;

            while (assignedMovers < numberOfMovers)
            {
                int newIndex = FE8BruteForcer.advanceRng(currentRns) * enemies.Length / 100;
                if (!enemies[newIndex].moves)
                {
                    assignedMovers += 1;
                    enemies[newIndex].moves = true;
                }
            }
        }

        public static ValniEnemyOutput SimValniEnemy(ushort[] currentRns, ValniEnemy input)
        {
            ValniEnemyOutput output = new ValniEnemyOutput();

            if (input.isBoss)
            {
                //stat rolls
                EnemyStatSim.rollFE8Enemy(currentRns, input.growthRates, false, 0, input.hmLevels);
            }
            else
            {
                //class
                FE8BruteForcer.advanceRng(currentRns);

                //level
                FE8BruteForcer.advanceRng(currentRns);
                int LEVELS_PLACEHOLDER = 1; // until i figure out how valni level generation works, this will at least cause stat rolls to happen.

                //held item
                FE8BruteForcer.advanceRng(currentRns);
                FE8BruteForcer.advanceRng(currentRns);

                //dropped item
                bool doesDrop = (FE8BruteForcer.advanceRng(currentRns) < input.dropRate);
                if (doesDrop)
                {
                    FE8BruteForcer.advanceRng(currentRns);
                }

                //stat rolls
                EnemyStatSim.rollFE8Enemy(currentRns, input.growthRates, input.isPromoted, LEVELS_PLACEHOLDER, input.hmLevels);
            }

            // movement
            if (input.moves)
            {
                output.position = (octodirection)(FE8BruteForcer.advanceRng(currentRns) * 8 / 100);
            }

            return output;
        }

        /* How to use:
         * You'll need to burn 1 RN to determine how many enemies move. This appears to be hard-coded based on the number of enemies in Valni.
         * Then, pass in an array of ValniEnemies and the number of movers. The function will return an array of their rolled positions.
         * NOTE that this is not their final positions, because it doesn't handle collision checks. You'll need to run those by hand afterward.
         * In the future I'd like to handle stat generation, weapon drops, etc., but I'm lazy and don't tend to code stuff until someone needs it for an LTC.
         */
        public static ValniEnemyOutput[] SimValni(ushort[] currentRns, ValniEnemy[] enemies, int numberOfMovers)
        {
            ValniEnemyOutput[] outputs = new ValniEnemyOutput[enemies.Length];

            SetValniMovers(currentRns, enemies, numberOfMovers);

            for (int i = 0; i < enemies.Length; i += 1)
            {
                outputs[i] = SimValniEnemy(currentRns, enemies[i]);
            }

            return outputs;
        }
    }

    public class ValniEnemy
    {
        public bool isBoss = false;
        public int dropRate = 0;
        public bool moves = false;
        public bool isPromoted = false;
        public int hmLevels = 3;
        public int[] growthRates = new int[] { 0, 0, 0, 0, 0, 0, 0 };
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
