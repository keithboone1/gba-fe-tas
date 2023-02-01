using System;

namespace FEBruteForcer
{
    class FEBruteForcer
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("enter 'quit' to quit, 'test' to test an enemy phase");
                string command = Console.ReadLine();
                switch (command)
                {
                    case "quit":
                        return;
                    case "test":
                        rnPrompt();
                        break;
                    default:
                        break;
                }
            }
        }

        static void rnPrompt()
        {
            Console.WriteLine("enter rn1, rn2, rn3:");

            currentRns[0] = ushort.Parse(Console.ReadLine());
            currentRns[1] = ushort.Parse(Console.ReadLine());
            currentRns[2] = ushort.Parse(Console.ReadLine());

            bool firstNext = true;

            while (true)
            {
                Console.WriteLine("enter 'quit' to quit, 'next' to find the next successful seed, 'debug' to re-run the last successful seed.");
                string command = Console.ReadLine();
                switch (command)
                {
                    case "quit":
                        return;
                    case "next":
                        if (!firstNext)
                        {
                            nextRn();
                        }
                        firstNext = false;
                        bruteForce();
                        break;
                    case "debug":
                        bruteForce();
                        break;
                    default:
                        break;
                }
            }
        }

        static void bruteForce()
        {
            int burned = 0;

            ushort[] initialRns = new ushort[3];
            currentRns.CopyTo(initialRns, 0);

            while (!theseRnsWork(initialRns))
            {
                burned += 1;
                initialRns.CopyTo(currentRns, 0);
                nextRn();
                currentRns.CopyTo(initialRns, 0);
            }

            initialRns.CopyTo(currentRns, 0);

            Console.WriteLine(string.Format("burn {0} RNs:", burned));
            Console.WriteLine(string.Format("RN1: {0} ({1})", initialRns[0], normalize100(initialRns[0])));
            Console.WriteLine(string.Format("RN2: {0} ({1})", initialRns[1], normalize100(initialRns[1])));
            Console.WriteLine(string.Format("RN3: {0} ({1})", initialRns[2], normalize100(initialRns[2])));
        }

        // switch game-specific behavior
        public static int game = 0;
        private static ushort[] currentRns = new ushort[3];

        public static int nextRnTrue()
        {
            return advanceRng();
        }

        public static int nextRn()
        {
            return normalize100(advanceRng());
        }

        private static ushort advanceRng()
        {
            ushort nextRn = (ushort)((currentRns[2] >> 5) ^ (currentRns[1] << 11) ^ (currentRns[0] << 1) ^ (currentRns[1] >> 15));
            currentRns[0] = currentRns[1];
            currentRns[1] = currentRns[2];
            currentRns[2] = nextRn;
            return nextRn;
        }

        private static int normalize100(ushort rn)
        {
            if (game == 6)
            {
                return rn / 655;
            }
            if (game == 7 || game == 8)
            {
                return (int)Math.Floor(rn / 655.36);
            }
            throw new Exception("Remember to set the game");
        }

        // the bit that actually changes by enemy phase
        static bool theseRnsWork(ushort[] initialRns)
        {
            ushort[] rnsForEp = new ushort[3];
            initialRns.CopyTo(rnsForEp, 0);

            return TestCases.simfe6c1ep1();
        }
    }
}
