using System;

namespace FEBruteForcer
{
    // The actual enemy phase or stat generation under test. Will keep some old examples around as a style hint.
    // Plus can use them to regression test...
    class TestCases
    {
        // input: 61695 48618 6768
        // output: RN1: 49210 (75) RN2: 37768 (57) RN3: 2801 (4)
        public static bool simroylevel()
        {
            FEBruteForcer.game = 6;

            //roy attacks
            CombatPreview roy = new CombatPreview() { hit = 83, crit = 11, atk = 11 };
            CombatPreview fighter = new CombatPreview() { currentHp = 19, def = 4 };
            (int, int) result1 = CombatSim.simCombat(roy, fighter);
            if (result1.Item2 > 0)
            {
                return false;
            }

            //roy levels up
            int[] royGrowths = new int[7] { 80, 40, 50, 40, 25, 30, 60 };
            int[] royLevel = LevelUpSim.simLevel(royGrowths);
            if (royLevel[0] == 0 || royLevel[1] == 0 || royLevel[2] == 0 || royLevel[3] == 0 || royLevel[4] == 0 || royLevel[5] == 0 || royLevel[6] == 0)
            {
                return false;
            }

            return true;
        }

        // input: 13701 27333 178
        // output: RN1: 987 (1) RN2: 14872 (22) RN3: 10298 (15)
        public static bool simfe6c1ep1()
        {
            FEBruteForcer.game = 6;

            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();

            int royHp = 18;

            // fighter 1
            CombatPreview fighter1 = new CombatPreview() { hit = 42, currentHp = 26, atk = 16, def = 4 };
            CombatPreview roy1 = new CombatPreview() { hit = 83, crit = 11, currentHp = royHp, atk = 11, def = 5 };
            MovementSim.simSimpleMovement(0, 0);
            (int, int) result = CombatSim.simCombat(fighter1, roy1);
            if (result.Item1 == 26 || result.Item2 == 18)
            {
                return false;
            }
            royHp = result.Item2;

            // fighter 2
            CombatPreview fighter2 = new CombatPreview() { hit = 43, currentHp = 26, atk = 16, def = 2 };
            CombatPreview roy2 = new CombatPreview() { hit = 100, crit = 10, currentHp = royHp, atk = 11, def = 5 };
            MovementSim.simSimpleMovement(3, 1);
            result = CombatSim.simCombat(fighter2, roy2);
            if (result.Item1 > 1 || result.Item2 < 1)
            {
                return false;
            }
            royHp = result.Item2;

            // fighter 3
            CombatPreview fighter3 = new CombatPreview() { hit = 44, currentHp = 26, atk = 16, def = 2 };
            CombatPreview roy3 = new CombatPreview() { hit = 100, crit = 11, currentHp = royHp, atk = 11, def = 5 };
            MovementSim.simSimpleMovement(1, 1);
            result = CombatSim.simCombat(fighter3, roy3);
            if (result.Item1 > 0 || result.Item2 < 1)
            {
                return false;
            }

            FEBruteForcer.nextRn();

            // archer
            CombatPreview archer1 = new CombatPreview() { hit = 74, atk = 13 };
            CombatPreview roy4 = new CombatPreview() { inRange = false, currentHp = royHp, def = 5 };
            MovementSim.simSimpleMovement(1, 4);
            result = CombatSim.simCombat(archer1, roy4);
            if (result.Item2 < 1)
            {
                return false;
            }

            return true;
        }

        public static bool simfe6c1load()
        {
            FEBruteForcer.game = 6;

            int[] archerGrowths = new int[7] { 70, 35, 40, 32, 15, 10, 35 };
            int[] fighterGrowths = new int[7] { 85, 55, 35, 30, 15, 10, 15 };
            int[] brigandGrowths = new int[7] { 82, 50, 30, 20, 10, 10, 15 };

            Enemy[] enemies = new Enemy[] { new Enemy() { growthRates = archerGrowths, hardModeLevels = 4 },
                                            new Enemy() { growthRates = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growthRates = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growthRates = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growthRates = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growthRates = brigandGrowths, hardModeLevels = 4 },
                                            new Enemy() { growthRates = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growthRates = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growthRates = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growthRates = archerGrowths, hardModeLevels = 4 },
                                            new Enemy() { growthRates = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growthRates = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growthRates = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growthRates = fighterGrowths, hardModeLevels = 4 }, };

            int[][] growths = EnemyStatSim.rollEnemies(enemies, true);

            if (growths[5][6] > 1 || (13 - (growths[5][4] + 3)) * 3 < (growths[5][0] + 20))
            {
                return false;
            }
            if ((11 - (growths[12][4] + 2)) * 3 < (growths[12][0] + 20))
            {
                return false;
            }
            if ((11 - (growths[13][4] + 2)) * 3 < (growths[13][0] + 20))
            {
                return false;
            }

            for (int i = 0; i < 14; i++)
            {
                Console.WriteLine(string.Format("{0} {1} {2} {3} {4} {5} {6}", growths[i][0], growths[i][1], growths[i][2], growths[i][3], growths[i][4], growths[i][5], growths[i][6]));
            }

            return true;
        }

        public static bool simValniFloor1Load()
        {
            FEBruteForcer.game = 8;

            ValniEnemy[] enemies = new ValniEnemy[3];
            enemies[0] = new ValniEnemy() { special = true, givePromoAutolevels = false };
            enemies[1] = new ValniEnemy() { special = true, monster = true, dropRate = 10 };
            enemies[2] = new ValniEnemy() { special = true, monster = true, dropRate = 10 };

            FEBruteForcer.nextRn();
            ValniEnemyOutput[] outputs = MapLoadingSim.SimValni(enemies, 2);

            foreach (ValniEnemyOutput output in outputs)
            {
                Console.WriteLine(Enum.GetName(typeof(octodirection), output.position));
            }
            return true;
        }

        public static bool simc10t2()
        {
            FEBruteForcer.game = 8;

            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();

            int ephHp = 23;

            CombatPreview merc1 = new CombatPreview() { hit = 68, currentHp = 25, atk = 11, def = 6 };
            CombatPreview eph1 = new CombatPreview() { hit = 93, crit = 12, currentHp = ephHp, atk = 19, def = 7 };
            MovementSim.simComplexMovement(new char[,] { { 'S', '1', '1', '-'},
                                                         { '1', '1', '1', 'F' } });
            (int, int) result = CombatSim.simCombat(merc1, eph1);
            ephHp = result.Item2;

            CombatPreview myrm1 = new CombatPreview() { hit = 100, crit = 9, currentHp = 24, atk = 19, def = 3 };
            CombatPreview cav1 = new CombatPreview() { hit = 33, currentHp = 26, atk = 13, def = 7 };
            MovementSim.simSimpleMovement(0, 1);
            result = CombatSim.simCombat(myrm1, cav1);
            if (result.Item2 <= 0)
            {
                return false;
            }

            CombatPreview fighter1 = new CombatPreview() { hit = 53 };
            CombatPreview cav2 = new CombatPreview() { hit = 100, crit = 2, currentHp = 2 };
            FEBruteForcer.nextRn();
            CombatSim.simCombat(fighter1, cav2);

            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();
            MovementSim.simSimpleMovement(1, 1);

            CombatPreview wyvern1 = new CombatPreview() { hit = 53, currentHp = 28, atk = 22, def = 10 };
            CombatPreview eph5 = new CombatPreview() { hit = 90, crit = 12, doubles = true, currentHp = ephHp, atk = 18, def = 7 };
            MovementSim.simComplexMovement(new char[,] { { 'F', '1', '-' },
                                                         { '1', '1', '1' },
                                                         { '1', '1', '1' },
                                                         { '1', '1', '1' },
                                                         { '1', '1', 'S' } });
            result = CombatSim.simCombat(wyvern1, eph5);
            ephHp = result.Item2;

            CombatPreview wyvern2 = new CombatPreview() { hit = 69, atk = 21 };
            CombatPreview nessa1 = new CombatPreview() { inRange = false, currentHp = 17, def = 6 };
            MovementSim.simSimpleMovement(0, 5);
            CombatSim.simCombat(wyvern2, nessa1);

            CombatPreview boat1 = new CombatPreview() { hit = 49, atk = 29 };
            CombatPreview cormag1 = new CombatPreview() { inRange = false, currentHp = 30, def = 12 };
            FEBruteForcer.nextRn();
            CombatSim.simCombat(boat1, cormag1);

            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();

            CombatPreview shaman1 = new CombatPreview() { hit = 31, crit = 4, currentHp = 22, atk = 8, def = 3 };
            CombatPreview eph2 = new CombatPreview() { hit = 100, crit = 12, doubles = true, currentHp = ephHp, atk = 20, def = 0 };
            FEBruteForcer.nextRn();
            result = CombatSim.simCombat(shaman1, eph2);
            ephHp = result.Item2;

            FEBruteForcer.nextRn();
            FEBruteForcer.nextRn();

            CombatPreview fighter2 = new CombatPreview() { hit = 70 };
            CombatPreview cav3 = new CombatPreview() { inRange = false };
            result = CombatSim.simCombat(fighter2, cav3);
            if (result.Item2 == 0)
            {
                return false;
            }

            FEBruteForcer.nextRn();
            CombatPreview cav4 = new CombatPreview() { hit = 53, atk = 15 };
            CombatPreview eph3 = new CombatPreview() { inRange = false, currentHp = ephHp, def = 7 };
            FEBruteForcer.nextRn();
            result = CombatSim.simCombat(cav4, eph3);
            ephHp = result.Item2;

            FEBruteForcer.nextRn();

            CombatPreview beran = new CombatPreview() { hit = 100, crit = 4, doubles = true, currentHp = 45, atk = 27, def = 17 };
            CombatPreview eph4 = new CombatPreview() { hit = 16, crit = 8, currentHp = ephHp, atk = 32, def = 7 };
            result = CombatSim.simCombat(beran, eph4);
            if (result.Item1 > 0)
            {
                return false;
            }

            return true;
        }
    }
}
