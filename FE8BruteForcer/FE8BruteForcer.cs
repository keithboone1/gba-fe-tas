using System;

namespace FE8BruteForcer
{
    class FE8BruteForcer
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
                        bruteForce();
                        break;
                    default:
                        break;
                }
            }
        }

        static void bruteForce()
        {
            Console.WriteLine("enter rn1, rn2, rn3:");

            ushort[] inputRns = new ushort[3];
            inputRns[0] = ushort.Parse(Console.ReadLine());
            inputRns[1] = ushort.Parse(Console.ReadLine());
            inputRns[2] = ushort.Parse(Console.ReadLine());

            ushort[] backupRns = new ushort[3];
            inputRns.CopyTo(backupRns, 0);

            while (true)
            {
                Console.WriteLine("enter 'quit' to quit, 'next' to find the next successful seed, 'debug' to re-run the last successful seed.");
                string command = Console.ReadLine();
                switch (command)
                {
                    case "quit":
                        return;
                    case "next":
                        bruteForceInner(inputRns);
                        inputRns.CopyTo(backupRns, 0);
                        nextRn(inputRns);
                        break;
                    case "debug":
                        backupRns.CopyTo(inputRns, 0);
                        bruteForceInner(inputRns);
                        nextRn(inputRns);
                        break;
                    default:
                        break;
                }
            }
        }

        static void bruteForceInner(ushort[] initialRns)
        {
            int burnThisMany = 0;

            ushort[] inputRns = new ushort[3];
            initialRns.CopyTo(inputRns, 0);

            while (!theseRnsWork(initialRns))
            {
                burnThisMany += 1;
                nextRn(initialRns);
                if (initialRns.Equals(inputRns)) {
                    throw new Exception();
                }
            }

            Console.WriteLine(string.Format("burn {0} RNs:", burnThisMany));
            Console.WriteLine(string.Format("RN1: {0} ({1})", initialRns[0], normalize100(initialRns[0])));
            Console.WriteLine(string.Format("RN2: {0} ({1})", initialRns[1], normalize100(initialRns[1])));
            Console.WriteLine(string.Format("RN3: {0} ({1})", initialRns[2], normalize100(initialRns[2])));
        }

        public static int nextRnTrue(ushort[] currentRns)
        {
            return advanceRng(currentRns);
        }

        public static int nextRn(ushort[] currentRns)
        {
            return normalize100(advanceRng(currentRns));
        }

        private static ushort advanceRng(ushort[] currentRns)
        {
            ushort nextRn = (ushort)((currentRns[2] >> 5) ^ (currentRns[1] << 11) ^ (currentRns[0] << 1) ^ (currentRns[1] >> 15));
            currentRns[0] = currentRns[1];
            currentRns[1] = currentRns[2];
            currentRns[2] = nextRn;
            return nextRn;
        }

        private static int normalize100(ushort rn)
        {
            return (int)Math.Floor(rn / 655.36);
        }

        static AttackResult simAttack(ushort[] currentRns, combatPreview attackerPreview)
        {
            int hitRn1 = nextRn(currentRns);
            int hitRn2 = nextRn(currentRns);

            if ((hitRn1 + hitRn2) >= attackerPreview.hit * 2)
            {
                return AttackResult.Miss;
            }

            bool pierced = false;
            if (attackerPreview.pierce > 0)
            {
                int pierceRn = nextRn(currentRns);
                if (pierceRn < attackerPreview.pierce)
                {
                    pierced = true;
                }
            }

            int critRn = nextRn(currentRns);

            if (critRn >= attackerPreview.crit)
            {
                return pierced ? AttackResult.PierceHit : AttackResult.Hit;
            }

            // visual effects RN on crit; disregard
            nextRn(currentRns);

            return pierced ? AttackResult.PierceCrit : AttackResult.Crit;
        }

        static int combatHpLoss(ushort[] currentRns, combatPreview attackerPreview, combatPreview defenderPreview)
        {
            AttackResult result = simAttack(currentRns, attackerPreview);

            switch (result)
            {
                case AttackResult.Miss:
                    return 0;
                case AttackResult.Hit:
                    return Math.Max(attackerPreview.atk - defenderPreview.def, 0);
                case AttackResult.PierceHit:
                    return attackerPreview.atk;
                case AttackResult.Crit:
                    return Math.Max(attackerPreview.atk - defenderPreview.def, 0) * 3;
                case AttackResult.PierceCrit:
                    return attackerPreview.atk * 3;
                default:
                    return 0;
            }
        }

        static (int, int) simCombat(ushort[] currentRns, combatPreview attackerPreview, combatPreview defenderPreview)
        {
            int attackerCurrentHp = attackerPreview.currentHp;
            int defenderCurrentHp = defenderPreview.currentHp;

            defenderCurrentHp -= combatHpLoss(currentRns, attackerPreview, defenderPreview);

            if (defenderCurrentHp <= 0)
            {
                return (attackerCurrentHp, defenderCurrentHp);
            }

            if (defenderPreview.inRange)
            {
                attackerCurrentHp -= combatHpLoss(currentRns, defenderPreview, attackerPreview);

                if (attackerCurrentHp <= 0)
                {
                    return (attackerCurrentHp, defenderCurrentHp);
                }
            }

            if (attackerPreview.doubles)
            {
                defenderCurrentHp -= combatHpLoss(currentRns, attackerPreview, defenderPreview);
            }
            else if (defenderPreview.doubles && defenderPreview.inRange)
            {
                attackerCurrentHp -= combatHpLoss(currentRns, defenderPreview, attackerPreview);
            }

            return (attackerCurrentHp, defenderCurrentHp);
        }

        /// <param name="growthRates">array in the order [hp, str, skl, spd, def, res, lck].</param>
        /// <returns>an array of true / false in the order [hp, str, skl, spd, def, res, lck]. True means it went up.</returns>
        static bool[] simLevel(ushort[] currentRns, int[] growthRates)
        {
            bool[] leveled = new bool[7];
            bool anyLeveled = false;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; i < 7; i++)
                {
                    leveled[j] = (nextRn(currentRns) < growthRates[j]);
                    anyLeveled = anyLeveled || leveled[j];
                }
                if (anyLeveled)
                {
                    break;
                }
            }

            return leveled;
        }

        // the bit that actually changes by enemy phase
        static bool theseRnsWork(ushort[] initialRns)
        {
            ushort[] rnsForEp = new ushort[3];
            initialRns.CopyTo(rnsForEp, 0);

            return simfe6c1load(rnsForEp);
        }

        static bool simfe6c1load(ushort[] currentRns)
        {
            int[] archerGrowths = new int[7] { 70, 35, 40, 32, 15, 10, 35 };
            int[] fighterGrowths = new int[7] { 85, 55, 35, 30, 15, 10, 15 };
            int[] brigandGrowths = new int[7] { 82, 50, 30, 20, 10, 10, 15 };

            Enemy[] enemies = new Enemy[] { new Enemy() { growths = archerGrowths, hardModeLevels = 4 },
                                            new Enemy() { growths = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growths = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growths = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growths = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growths = brigandGrowths, hardModeLevels = 4 },
                                            new Enemy() { growths = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growths = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growths = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growths = archerGrowths, hardModeLevels = 4 },
                                            new Enemy() { growths = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growths = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growths = fighterGrowths, hardModeLevels = 4 },
                                            new Enemy() { growths = fighterGrowths, hardModeLevels = 4 }, };

            int[][] growths = EnemyStatSim.rollFE6Enemies(currentRns, enemies, true);

            if (growths[5][6] > 1)
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
                Console.WriteLine(String.Format("{0} {1} {2} {3} {4} {5} {6}", growths[i][0], growths[i][1], growths[i][2], growths[i][3], growths[i][4], growths[i][5], growths[i][6]));
            }

            return true;
        }

        static bool simc2Load(ushort[] currentRns)
        {
            int[] brigandGrowths = new int[7] { 75, 50, 35, 25, 10, 13, 15 };

            int[] brigand1Growths = EnemyStatSim.rollFE8Enemy(currentRns, brigandGrowths, 2, 19, 2);

            foreach (int growth in brigand1Growths)
            {
                Console.WriteLine(growth);
            }

            return true;
        }

        static bool simValniFloor1Load(ushort[] currentRns)
        {
            ValniEnemy[] enemies = new ValniEnemy[3];
            enemies[0] = new ValniEnemy() { special = true, givePromoAutolevels = false };
            enemies[1] = new ValniEnemy() { special = true, monster = true, dropRate = 10 };
            enemies[2] = new ValniEnemy() { special = true, monster = true, dropRate = 10 };

            nextRn(currentRns);
            ValniEnemyOutput[] outputs = MapLoadingSim.SimValni(currentRns, enemies, 2);

            foreach (ValniEnemyOutput output in outputs)
            {
                Console.WriteLine(Enum.GetName(typeof(octodirection), output.position));
            }
            return true;
        }

        static bool simc10t2(ushort[] currentRns)
        {
            nextRn(currentRns);
            nextRn(currentRns);
            nextRn(currentRns);
            nextRn(currentRns);
            nextRn(currentRns);

            int ephHp = 23;

            combatPreview merc1 = new combatPreview(68, 0, false, 0, true, 25, 11, 6);
            combatPreview eph1 = new combatPreview(93, 12, false, 0, true, ephHp, 19, 7);
            MovementSim.simComplexMovement(currentRns, new char[,] { { 'S', '1', '1', '-'},
                                                                     { '1', '1', '1', 'F' } });
            (int, int) result = simCombat(currentRns, merc1, eph1);
            ephHp = result.Item2;

            combatPreview myrm1 = new combatPreview(100, 9, true, 0, true, 24, 19, 3);
            combatPreview cav1 = new combatPreview(33, 0, false, 0, true, 26, 13, 7);
            MovementSim.simSimpleMovement(currentRns, 0, 1);
            result = simCombat(currentRns, myrm1, cav1);
            if (result.Item2 <= 0)
            {
                return false;
            }

            combatPreview fighter1 = new combatPreview(53, 0);
            combatPreview cav2 = new combatPreview(100, 2, false, 0, true, 2);
            nextRn(currentRns);
            simCombat(currentRns, fighter1, cav2);

            nextRn(currentRns);
            nextRn(currentRns);
            nextRn(currentRns);
            nextRn(currentRns);
            MovementSim.simSimpleMovement(currentRns, 1, 1);

            combatPreview wyvern1 = new combatPreview(53, 0, false, 0, true, 28, 22, 10);
            combatPreview eph5 = new combatPreview(90, 12, true, 0, true, ephHp, 18, 7);
            MovementSim.simComplexMovement(currentRns, new char[,] { { 'F', '1', '-' },
                                                                     { '1', '1', '1' },
                                                                     { '1', '1', '1' },
                                                                     { '1', '1', '1' },
                                                                     { '1', '1', 'S' } });
            result = simCombat(currentRns, wyvern1, eph5);
            ephHp = result.Item2;

            combatPreview wyvern2 = new combatPreview(69, 0, false, 0, true, 27, 21, 11);
            combatPreview nessa1 = new combatPreview(0, 0, false, 0, false, 17, 15, 6);
            MovementSim.simSimpleMovement(currentRns, 0, 5);
            simCombat(currentRns, wyvern2, nessa1);
            
            combatPreview boat1 = new combatPreview(49, 0, false, 0, true, 29, 29, 8);
            combatPreview cormag1 = new combatPreview(0, 0, false, 0, false, 30, 0, 12);
            nextRn(currentRns);
            simCombat(currentRns, boat1, cormag1);

            nextRn(currentRns);
            nextRn(currentRns);
            nextRn(currentRns);

            combatPreview shaman1 = new combatPreview(31, 4, false, 0, true, 22, 8, 3);
            combatPreview eph2 = new combatPreview(100, 12, true, 0, true, ephHp, 20, 0);
            nextRn(currentRns);
            result = simCombat(currentRns, shaman1, eph2);
            ephHp = result.Item2;

            nextRn(currentRns);
            nextRn(currentRns);

            combatPreview fighter2 = new combatPreview(70, 0);
            combatPreview cav3 = new combatPreview(0, 0, false, 0, false);
            result = simCombat(currentRns, fighter2, cav3);
            if (result.Item2 == 0)
            {
                return false;
            }

            nextRn(currentRns);

            combatPreview cav4 = new combatPreview(53, 0, false, 0, true, 29, 15, 0);
            combatPreview eph3 = new combatPreview(0, 0, false, 0, false, ephHp, 0, 7);
            nextRn(currentRns);
            result = simCombat(currentRns, cav4, eph3);
            ephHp = result.Item2;

            nextRn(currentRns);

            combatPreview beran = new combatPreview(100, 4, true, 0, true, 45, 27, 17);
            combatPreview eph4 = new combatPreview(16, 8, false, 0, true, ephHp, 32, 7);
            result = simCombat(currentRns, beran, eph4);
            if (result.Item1 > 0)
            {
                return false;
            }

            return true;
        }

        static bool simc12t1(ushort[] currentRns)
        {
            // fighter move
            nextRn(currentRns);

            // far left cav move
            for (int i = 0; i < 11; i++)
            {
                nextRn(currentRns);
            }

            // left cav vs eph
            MovementSim.simSimpleMovement(currentRns, 0, 5);
            combatPreview cav1 = new combatPreview(67, 0, false, 0, true, 29, 16, 8);
            combatPreview eph1 = new combatPreview(81, 11, false, 0, true, 23, 38, 7);
            (int, int) result = simCombat(currentRns, cav1, eph1);
            if (result.Item1 > 0 || result.Item2 < 23)
            {
                return false;
            }

            // top right cav vs forde
            MovementSim.simSimpleMovement(currentRns, 0, 6);
            combatPreview cav2 = new combatPreview(63, 0, false, 0, true, 31, 20, 8);
            combatPreview forde1 = new combatPreview(77, 30, true, 0, true, 26, 18, 10);
            result = simCombat(currentRns, cav2, forde1);
            if (result.Item1 > 0 || result.Item2 < 26)
            {
                return false;
            }

            return true;
        }

        static bool simc12t2(ushort[] currentRns)
        {
            nextRn(currentRns);
            MovementSim.simSimpleMovement(currentRns, 1, 1);
            combatPreview fighter1 = new combatPreview(53, 0, false, 0, true, 30, 19, 4);
            combatPreview kyle1 = new combatPreview(100, 1, false, 0, true, 25, 15, 9);
            (int, int) result = simCombat(currentRns, fighter1, kyle1);
            if (result.Item2 < 25)
            {
                return false;
            }

            MovementSim.simSimpleMovement(currentRns, 3, 1);
            combatPreview pirate1 = new combatPreview(40, 0, false, 0, true, 29, 17, 4);
            combatPreview josh1 = new combatPreview(100, 19, true, 0, true, 29, 19, 7);
            result = simCombat(currentRns, pirate1, josh1);

            nextRn(currentRns);
            MovementSim.simSimpleMovement(currentRns, 1, 1);
            combatPreview merc1 = new combatPreview(54, 32, false, 0, true, 30, 18, 7);
            combatPreview cormag1 = new combatPreview(76, 1, false, 2, true, 40, 24, 12);
            result = simCombat(currentRns, merc1, cormag1);
            if (result.Item2 < 34)
            {
                return false;
            }

            nextRn(currentRns);
            MovementSim.simSimpleMovement(currentRns, 1, 1);
            combatPreview merc2 = new combatPreview(78, 0);
            combatPreview tana1 = new combatPreview(0, 0, false, 0, false);
            result = simCombat(currentRns, merc2, tana1);
            if (result.Item2 < 1)
            {
                return false;
            }

            for (int i = 0; i < 6; i++)
            {
                nextRn(currentRns);
            }
            MovementSim.simSimpleMovement(currentRns, 1, 2);
            nextRn(currentRns);
            combatPreview bael1 = new combatPreview(43, 0, false, 0, true, 33, 25, 7);
            combatPreview seth1 = new combatPreview(93, 5, true, 0, true, 30, 20, 13);
            result = simCombat(currentRns, bael1, seth1);

            MovementSim.simSimpleMovement(currentRns, 2, 0);
            combatPreview bael2 = new combatPreview(43, 0, false, 0, true, 34, 26, 8);
            combatPreview cormag2 = new combatPreview(84, 4, true, 2, true, 40, 23, 12);
            result = simCombat(currentRns, bael2, cormag2);

            // one garg moves
            MovementSim.simSimpleMovement(currentRns, 1, 5);

            MovementSim.simSimpleMovement(currentRns, 1, 1);
            combatPreview cav1 = new combatPreview(54, 0);
            combatPreview tana2 = new combatPreview(0, 0, false, 0, false);
            result = simCombat(currentRns, cav1, tana2);
            if (result.Item2 < 1)
            {
                return false;
            }

            return true;
        }

        static bool simc12t3(ushort[] currentRns)
        {
            if (nextRn(currentRns) + nextRn(currentRns) > 97)
            {
                return false;
            }
            if (nextRn(currentRns) < 2)
            {
                return false;
            }
            if (nextRn(currentRns) > 4)
            {
                return false;
            }
            nextRn(currentRns);
            if (nextRn(currentRns) + nextRn(currentRns) < 122)
            {
                nextRn(currentRns);
            }
            if (nextRn(currentRns) + nextRn(currentRns) > 97)
            {
                return false;
            }
            if (nextRn(currentRns) < 2)
            {
                return false;
            }
            if (nextRn(currentRns) > 4)
            {
                return false;
            }

            return true;
        }

        static bool simc18t1(ushort[] currentRns)
        {
            nextRn(currentRns);
            nextRn(currentRns);

            combatPreview garg1 = new combatPreview(56, 0);
            combatPreview phant1 = new combatPreview(85, 0);
            if (!MovementSim.simSimpleMovement(currentRns, 0, 3, 50))
            {
                return false;
            }
            if (simCombat(currentRns, garg1, phant1).Item2 == 0)
            {
                return false;
            }


            combatPreview mogall1 = new combatPreview(61, 0, false, 0, true, 25, 17, 5);
            combatPreview cormag1 = new combatPreview(80, 5, true, 10, true, 40, 25, 3);
            MovementSim.simSimpleMovement(currentRns, 0, 1);
            (int, int) result = simCombat(currentRns, mogall1, cormag1);
            if (result.Item1 > 0 || result.Item2 != 40)
            {
                return false;
            }

            // not sure what these are
            nextRn(currentRns);
            nextRn(currentRns);
            nextRn(currentRns);

            combatPreview gorgon2 = new combatPreview(56, 0);
            combatPreview cormag2 = new combatPreview(0, 0, false, 0, false);
            MovementSim.simSimpleMovement(currentRns, 5, 1);

            if (simCombat(currentRns, gorgon2, cormag2).Item2 == 0)
            {
                return false;
            }

            combatPreview gorgon1 = new combatPreview(73, 0);
            combatPreview cormag3 = new combatPreview(66, 0);
            MovementSim.simSimpleMovement(currentRns, 1, 3);
            if (simCombat(currentRns, gorgon1, cormag3).Item2 == 0)
            {
                return false;
            }

            return true;
        }

        static bool simc18t2(ushort[] currentRns)
        {
            combatPreview bossGorgon = new combatPreview(72, 1, false, 0, true, 46, 31, 11);
            combatPreview cormag1 = new combatPreview(61, 5, true, 11, true, 26, 28, 3);
            MovementSim.simSimpleMovement(currentRns, 1, 1);
            (int, int) result = simCombat(currentRns, bossGorgon, cormag1);
            if (result.Item1 > 0)
            {
                return false;
            }

            simLevel(currentRns, new int[] { 0, 0, 0, 0, 0, 0, 0 });

            combatPreview garg1 = new combatPreview(22, 5, false, 0, true, 4, 1, 0);
            combatPreview rennac1 = new combatPreview(100, 35, true, 0, true, 1, 1, 0);
            MovementSim.simSimpleMovement(currentRns, 0, 4);
            result = simCombat(currentRns, garg1, rennac1);
            if (result.Item1 > 0 || result.Item2 <= 0)
            {
                return false;
            }

            combatPreview mogall1 = new combatPreview(61, 0, false, 0, true, 25, 17, 3);
            combatPreview cormag2 = new combatPreview(75, 5, true, 11, true, 40, 28, 3);
            MovementSim.simSimpleMovement(currentRns, 3, 1);
            result = simCombat(currentRns, mogall1, cormag2);
            if (result.Item1 > 0 || result.Item2 < 40)
            {
                return false;
            }

            combatPreview mogall2 = new combatPreview(70, 0, false, 0, true, 25, 17, 5);
            combatPreview eph1 = new combatPreview(75, 0, true, 0, true, 34, 16, 16);
            MovementSim.simComplexMovement(currentRns, new char[,] { { 'S', '1', '1', '1',},
                                                                     { '1', '1', '1', '1' }, 
                                                                     { '-', '1', '1', 'F' } });
            result = simCombat(currentRns, mogall2, eph1);
            if (result.Item1 > 10)
            {
                return false;
            }

            combatPreview mogall3 = new combatPreview(68, 0, false, 0, true, 25, 17, 4);
            combatPreview syrene1 = new combatPreview(84, 5, true, 0, true, 27, 24, 12);
            MovementSim.simSimpleMovement(currentRns, 0, 3);
            result = simCombat(currentRns, mogall3, syrene1);
            if (result.Item1 > 0)
            {
                return false;
            }

            combatPreview gorgon1 = new combatPreview(57, 0);
            combatPreview rennac2 = new combatPreview(0, 0, false, 0, false);
            MovementSim.simSimpleMovement(currentRns, 2, 0);
            result = simCombat(currentRns, gorgon1, rennac2);
            if (result.Item2 <= 0)
            {
                return false;
            }

            //combatPreview garg2 = new combatPreview(49, 0, false, 0, true, 35, 21, 10);
            //combatPreview syrene2 = new combatPreview(73, 5, false, 0, true, 27, 21, 10);
            //MovementSim.simSimpleMovement(currentRns, 2, 0);
            //result = simCombat(currentRns, garg2, syrene2);
            //if (result.Item1 > 0 || result.Item2 < 27)
            //{
            //    return false;
            //}

            return true;
        }

        static bool simc18t3(ushort[] currentRns)
        {
            int ephHp = 23;

            combatPreview bael1 = new combatPreview(40, 0, false, 0, true, 42, 30, 10);
            combatPreview syrene1 = new combatPreview(92, 34, true, 0, true, 1, 22, 10);
            MovementSim.simSimpleMovement(currentRns, 0, 0);
            (int, int) result = simCombat(currentRns, bael1, syrene1);
            if (result.Item1 > 0 || result.Item2 <= 0)
            {
                return false;
            }

            combatPreview bael2 = new combatPreview(42, 0, false, 0, true, 40, 30, 10);
            combatPreview syrene2 = new combatPreview(87, 33, true, 0, true, 1, 22, 10);
            MovementSim.simSimpleMovement(currentRns, 0, 0);
            result = simCombat(currentRns, bael2, syrene2);
            if (result.Item1 > 0 || result.Item2 <= 0)
            {
                return false;
            }

            // top middle
            combatPreview bael3 = new combatPreview(52, 0, false, 0, true, 39, 29, 9);
            combatPreview ephraim1 = new combatPreview(93, 3, true, 0, true, ephHp, 49, 11);
            MovementSim.simSimpleMovement(currentRns, 2, 1);
            result = simCombat(currentRns, bael3, ephraim1);
            if (result.Item1 > 0 || result.Item2 < 0)
            {
                return false;
            }
            ephHp = result.Item2;

            // bottom right
            combatPreview bael4 = new combatPreview(50, 0, false, 0, true, 40, 22, 10);
            combatPreview ephraim2 = new combatPreview(92, 4, true, 0, true, ephHp, 49, 11);
            MovementSim.simSimpleMovement(currentRns, 1, 0);
            result = simCombat(currentRns, bael4, ephraim2);
            if (result.Item1 > 0 || result.Item2 < 0)
            {
                return false;
            }
            ephHp = result.Item2;

            // top right
            combatPreview bael5 = new combatPreview(52, 0, false, 0, true, 41, 30, 9);
            combatPreview ephraim3 = new combatPreview(97, 3, true, 0, true, ephHp, 49, 11);
            MovementSim.simSimpleMovement(currentRns, 0, 1);
            result = simCombat(currentRns, bael5, ephraim3);
            if (result.Item1 > 0 || result.Item2 < 0)
            {
                return false;
            }
            ephHp = result.Item2;

            // top left
            combatPreview bael6 = new combatPreview(48, 0, false, 0, true, 40, 31, 9);
            combatPreview seth1 = new combatPreview(100, 8, true, 0, true, 30, 50, 13);
            MovementSim.simSimpleMovement(currentRns, 0, 0);
            result = simCombat(currentRns, bael6, seth1);
            if (result.Item1 > 0 || result.Item2 < 0)
            {
                return false;
            }

            // middle left
            //combatPreview bael7 = new combatPreview();
            //combatPreview ephraim4 = new combatPreview();

            //combatPreview bael6 = new combatPreview(45, 0, false, 0, true, 41, 30, 10);
            //combatPreview seth1 = new combatPreview(100, 9, true, 0, true, 30, 50, 13);
            //MovementSim.simSimpleMovement(currentRns, 0, 0);
            //result = simCombat(currentRns, bael6, seth1);
            //if (result.Item2 < 30)
            //{
            //    return false;
            //}

            //combatPreview bael7 = new combatPreview(46, 0, false, 0, true, 41, 30, 10);
            //combatPreview seth2 = new combatPreview(100, 8, true, 0, true, 30, 50, 13);
            //MovementSim.simSimpleMovement(currentRns, 0, 0);
            //result = simCombat(currentRns, bael7, seth2);
            //if (result.Item2 < 30)
            //{
            //    return false;
            //}

            //combatPreview garg1 = new combatPreview(59, 0, false, 0, true, 33, 36, 11);
            //combatPreview ephraim2 = new combatPreview(74, 3, true, 0, true, 33, 16, 11);
            //MovementSim.simSimpleMovement(currentRns, 1, 1);
            //result = simCombat(currentRns, garg1, ephraim2);
            //if (result.Item2 < 33)
            //{
            //    return false;
            //}

            //combatPreview garg2 = new combatPreview(51, 0, false, 0, true, 35, 24, 11);
            //combatPreview tana1 = new combatPreview(86, 6, true, 0, true, 25, 13, 8);
            //MovementSim.simSimpleMovement(currentRns, 2, 1);
            //result = simCombat(currentRns, garg2, tana1);
            //if (result.Item2 < 25)
            //{
            //    return false;
            //}

            //combatPreview gorg2 = new combatPreview();
            //combatPreview cormag4 = new combatPreview();

            //combatPreview garg3 = new combatPreview();
            //combatPreview syrene3 = new combatPreview();

            return true;
        }

        static bool simc8t3(ushort[] currentRns)
        {
            for (int i = 0; i < 6; i++)
            {
                nextRn(currentRns);
            }

            MovementSim.simSimpleMovement(currentRns, 0, 1, 100);
            combatPreview knight1 = new combatPreview(72, 0, false, 0, true, 24, 20, 11);
            combatPreview eirika1 = new combatPreview(95, 13, true, 0, true, 16, 22, 3);
            (int, int) result = simCombat(currentRns, knight1, eirika1);
            if (result.Item1 < 0 || result.Item2 < 0)
            {
                return false;
            }

            nextRn(currentRns);

            MovementSim.simSimpleMovement(currentRns, 1, 2, 100);
            combatPreview soldier1 = new combatPreview(55, 0, false, 0, true, 28, 19, 2);
            combatPreview seth1 = new combatPreview(94, 5, true, 0, true, 30, 21, 13);
            result = simCombat(currentRns, soldier1, seth1);

            for (int i = 0; i < 5; i++)
            {
                nextRn(currentRns);
            }

            MovementSim.simSimpleMovement(currentRns, 0, 0, 100);
            combatPreview soldier3 = new combatPreview(69, 0, false, 0, true, 28, 18, 1);
            combatPreview eirika2 = new combatPreview(95, 11, true, 0, true, 16, 10, 3);
            result = simCombat(currentRns, soldier3, eirika2);
            if (result.Item1 > 0 || result.Item2 < 16)
            {
                return false;
            }

            return true;
        }
    }

    class Unit
    {
        public CombatOdds odds;
        public CombatPower power;
        public CombatQuirks quirks;

        public Unit(CombatOdds odds, CombatPower power, CombatQuirks quirks)
        {
            this.odds = odds;

            if (this.power != null)
            {
                this.power = power;
            }
            else
            {
                this.power = new CombatPower();
            }

            this.quirks = quirks;
        }
    }

    class CombatOdds
    {
        public int Hit;
        public int Crit;
    }

    class CombatPower
    {
        public int Atk;
        public int Def;
        public int HP;
        public CombatRange Range;
    }

    class CombatQuirks
    {
        public bool uncounterable;
        public bool cannotCrit;
    }

    class combatPreview
    {
        public int hit;
        public int crit;
        public bool doubles;
        public int pierce;
        public bool inRange;
        public int currentHp;
        public int atk;
        public int def;
          
        public combatPreview(int hit, int crit, bool doubles = false, int pierce = 0, bool inRange = true, int currentHp = 1, int atk = 1, int def = 0)
        {
            this.hit = hit;
            this.crit = crit;
            this.doubles = doubles;
            this.pierce = pierce;
            this.inRange = inRange;
            this.currentHp = currentHp;
            this.atk = atk;
            this.def = def;
        }
    }

    public enum AttackResult
    {
        Miss,
        Hit,
        PierceHit,
        Crit,
        PierceCrit
    }

    public enum CombatRange
    {
        One,
        Two,
        OneTwo,
        TwoThree,
        Uncounterable
    }
}
