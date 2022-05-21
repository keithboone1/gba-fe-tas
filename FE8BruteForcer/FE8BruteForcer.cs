using System;

namespace FE8BruteForcer
{
    class FE8BruteForcer
    {
        static void Main(string[] args)
        {
            while (true)
            {
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

        static void simComplexMovementTest()
        {
            Console.WriteLine("enter rn1, rn2, rn3:");

            ushort[] initialRns = new ushort[3];
            initialRns[0] = ushort.Parse(Console.ReadLine());
            initialRns[1] = ushort.Parse(Console.ReadLine());
            initialRns[2] = ushort.Parse(Console.ReadLine());

            char[,] grid = new char[,] { { '1', '1', '1', '1', '3', '1'},
                                         { 'F', '3', '1', '1', '1', 'S'},
                                         { '1', '1', '1', '1', '1', '-'} };

            MovementSim.simComplexMovement(initialRns, grid, 100);

            Console.WriteLine(string.Format("RN1: {0} ({1})", initialRns[0], normalize100(initialRns[0])));
            Console.WriteLine(string.Format("RN2: {0} ({1})", initialRns[1], normalize100(initialRns[1])));
            Console.WriteLine(string.Format("RN3: {0} ({1})", initialRns[2], normalize100(initialRns[2])));
        }

        static void bruteForce()
        {
            Console.WriteLine("enter rn1, rn2, rn3:");

            ushort[] initialRns = new ushort[3];
            initialRns[0] = ushort.Parse(Console.ReadLine());
            initialRns[1] = ushort.Parse(Console.ReadLine());
            initialRns[2] = ushort.Parse(Console.ReadLine());

            ushort[] backupRns = new ushort[3];
            initialRns.CopyTo(backupRns, 0);

            while (true)
            {
                string command = Console.ReadLine();
                switch (command)
                {
                    case "quit":
                        return;
                    case "next":
                        bruteForceInner(initialRns);
                        initialRns.CopyTo(backupRns, 0);
                        advanceRng(initialRns);
                        break;
                    case "debug":
                        backupRns.CopyTo(initialRns, 0);
                        bruteForceInner(initialRns);
                        advanceRng(initialRns);
                        break;
                    default:
                        break;
                }
            }
        }

        static void bruteForceInner(ushort[] initialRns)
        {
            int burnThisMany = 0;

            while (!theseRnsWork(initialRns))
            {
                burnThisMany += 1;
                advanceRng(initialRns);
            }

            Console.WriteLine(string.Format("burn {0} RNs:", burnThisMany));
            Console.WriteLine(string.Format("RN1: {0} ({1})", initialRns[0], normalize100(initialRns[0])));
            Console.WriteLine(string.Format("RN2: {0} ({1})", initialRns[1], normalize100(initialRns[1])));
            Console.WriteLine(string.Format("RN3: {0} ({1})", initialRns[2], normalize100(initialRns[2])));
        }

        public static int advanceRng(ushort[] currentRns)
        {
            ushort nextRn = (ushort)((currentRns[2] >> 5) ^ (currentRns[1] << 11) ^ (currentRns[0] << 1) ^ (currentRns[1] >> 15));
            currentRns[0] = currentRns[1];
            currentRns[1] = currentRns[2];
            currentRns[2] = nextRn;
            return normalize100(nextRn);
        }

        static int normalize100(ushort rn)
        {
            return (int)Math.Floor(rn / 655.36);
        }

        static AttackResult simAttack(ushort[] currentRns, combatPreview attackerPreview)
        {
            int hitRn1 = advanceRng(currentRns);
            int hitRn2 = advanceRng(currentRns);

            if ((hitRn1 + hitRn2) >= attackerPreview.hit * 2)
            {
                return AttackResult.Miss;
            }

            bool pierced = false;
            if (attackerPreview.pierce > 0)
            {
                int pierceRn = advanceRng(currentRns);
                if (pierceRn < attackerPreview.pierce)
                {
                    pierced = true;
                }
            }

            int critRn = advanceRng(currentRns);

            if (critRn >= attackerPreview.crit)
            {
                return pierced ? AttackResult.PierceHit : AttackResult.Hit;
            }

            // visual effects RN on crit; disregard
            advanceRng(currentRns);

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

        static void simLevel(ushort[] currentRns)
        {
            for (int i = 0; i < 21; i++)
            {
                advanceRng(currentRns);
            }
        }

        // the bit that actually changes by enemy phase
        static bool theseRnsWork(ushort[] initialRns)
        {
            ushort[] rnsForEp = new ushort[3];
            initialRns.CopyTo(rnsForEp, 0);

            return simc12t1(rnsForEp);
        }

        static bool simc12t1(ushort[] currentRns)
        {
            // fighter move
            advanceRng(currentRns);

            // far left cav move
            for (int i = 0; i < 11; i++)
            {
                advanceRng(currentRns);
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
            advanceRng(currentRns);
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

            advanceRng(currentRns);
            MovementSim.simSimpleMovement(currentRns, 1, 1);
            combatPreview merc1 = new combatPreview(54, 32, false, 0, true, 30, 18, 7);
            combatPreview cormag1 = new combatPreview(76, 1, false, 2, true, 40, 24, 12);
            result = simCombat(currentRns, merc1, cormag1);
            if (result.Item2 < 34)
            {
                return false;
            }

            advanceRng(currentRns);
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
                advanceRng(currentRns);
            }
            MovementSim.simSimpleMovement(currentRns, 1, 2);
            advanceRng(currentRns);
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
            if (advanceRng(currentRns) + advanceRng(currentRns) > 97)
            {
                return false;
            }
            if (advanceRng(currentRns) < 2)
            {
                return false;
            }
            if (advanceRng(currentRns) > 4)
            {
                return false;
            }
            advanceRng(currentRns);
            if (advanceRng(currentRns) + advanceRng(currentRns) < 122)
            {
                advanceRng(currentRns);
            }
            if (advanceRng(currentRns) + advanceRng(currentRns) > 97)
            {
                return false;
            }
            if (advanceRng(currentRns) < 2)
            {
                return false;
            }
            if (advanceRng(currentRns) > 4)
            {
                return false;
            }

            return true;
        }

        static bool simc18t1(ushort[] currentRns)
        {
            advanceRng(currentRns);
            advanceRng(currentRns);

            combatPreview garg1 = new combatPreview(56, 0);
            combatPreview phant1 = new combatPreview(0, 0);
            if (!MovementSim.simSimpleMovement(currentRns, 0, 3, 50))
            {
                return false;
            }
            if (simCombat(currentRns, garg1, phant1).Item2 != 0)
            {
                return false;
            }


            combatPreview mogall1 = new combatPreview(61, 0, false, 0, true, 25, 17, 5);
            combatPreview cormag1 = new combatPreview(80, 5, true, 10, true, 40, 25, 3);
            MovementSim.simSimpleMovement(currentRns, 0, 1);
            (int, int) result = simCombat(currentRns, mogall1, cormag1);
            if (result.Item1 > 0 || result.Item2 < 40)
            {
                return false;
            }

            // not sure what these are
            advanceRng(currentRns);
            advanceRng(currentRns);
            advanceRng(currentRns);

            combatPreview gorgon2 = new combatPreview(56, 0);
            combatPreview cormag2 = new combatPreview(0, 0, false, 0, false);
            MovementSim.simSimpleMovement(currentRns, 5, 1);

            if (simCombat(currentRns, gorgon2, cormag2).Item2 == 0)
            {
                return false;
            }

            combatPreview gorgon1 = new combatPreview(73, 0);
            combatPreview eph1 = new combatPreview(66, 0);
            MovementSim.simSimpleMovement(currentRns, 0, 3);
            if (simCombat(currentRns, gorgon1, eph1).Item2 == 0)
            {
                return false;
            }

            return true;
        }

        static bool simc18t2(ushort[] currentRns)
        {
            combatPreview bossGorgon = new combatPreview(72, 1, false, 0, true, 46, 31, 11);
            combatPreview cormag1 = new combatPreview(66, 5, true, 11, true, 40, 25, 3);
            MovementSim.simSimpleMovement(currentRns, 1, 1);
            (int, int) result = simCombat(currentRns, bossGorgon, cormag1);
            if (result.Item1 > 28 || result.Item1 <= 0 || result.Item2 < 40)
            {
                return false;
            }

            // shortcut: rennac must crit at least one
            combatPreview garg1 = new combatPreview(22, 5, false, 0, true, 4, 1, 0);
            combatPreview rennac1 = new combatPreview(100, 35, true, 0, true, 1, 1, 0);
            MovementSim.simSimpleMovement(currentRns, 0, 4);
            result = simCombat(currentRns, garg1, rennac1);
            if (result.Item1 > 0 || result.Item2 <= 0)
            {
                return false;
            }

            combatPreview mogall1 = new combatPreview(61, 0, false, 0, true, 25, 17, 3);
            combatPreview cormag2 = new combatPreview(80, 5, true, 11, true, 40, 25, 3);
            MovementSim.simSimpleMovement(currentRns, 3, 1);
            result = simCombat(currentRns, mogall1, cormag2);
            if (result.Item1 > 0 || result.Item2 < 40)
            {
                return false;
            }

            combatPreview mogall2 = new combatPreview(70, 0, false, 0, true, 25, 17, 5);
            combatPreview eph1 = new combatPreview(75, 0, true, 0, true, 34, 16, 16);
            MovementSim.simSimpleMovement(currentRns, 3, 2);
            result = simCombat(currentRns, mogall2, eph1);
            if (result.Item1 > 10)
            {
                return false;
            }

            combatPreview mogall3 = new combatPreview(68, 0, false, 0, true, 2, 1, 0);
            combatPreview syrene1 = new combatPreview(74, 0, true, 0, true, 20, 1, 0);
            MovementSim.simSimpleMovement(currentRns, 1, 4);
            result = simCombat(currentRns, mogall3, syrene1);
            if (result.Item1 > 0)
            {
                return false;
            }

            combatPreview gorgon1 = new combatPreview(74, 0);
            combatPreview phantom1 = new combatPreview(0, 0);
            MovementSim.simSimpleMovement(currentRns, 1, 1);
            result = simCombat(currentRns, gorgon1, phantom1);
            if (result.Item2 > 0)
            {
                return false;
            }

            combatPreview gorgon2 = new combatPreview(65, 0, false, 0, true, 23, 35, 8);
            combatPreview cormag3 = new combatPreview(68, 3, true, 11, true, 40, 25, 3);
            MovementSim.simSimpleMovement(currentRns, 1, 1);
            result = simCombat(currentRns, gorgon2, cormag3);
            if (result.Item1 > 0 || result.Item2 < 40)
            {
                return false;
            }

            combatPreview garg2 = new combatPreview(49, 0, false, 0, true, 35, 21, 10);
            combatPreview syrene2 = new combatPreview(73, 5, false, 0, true, 27, 21, 10);
            MovementSim.simSimpleMovement(currentRns, 2, 0);
            result = simCombat(currentRns, garg2, syrene2);
            if (result.Item1 == 35 || result.Item2 < 27)
            {
                return false;
            }

            return true;
        }

        static bool simc18t3(ushort[] currentRns)
        {
            int cormagHp = 40;
            int syreneHp = 22;
            bool bael3died, bael4died, bael5died;

            combatPreview bossGorgon = new combatPreview(72, 1, false, 0, true, 18, 31, 11);
            combatPreview cormag1 = new combatPreview(66, 5, true, 12, true, cormagHp, 25, 3);
            MovementSim.simSimpleMovement(currentRns, 1, 1);
            (int, int) result = simCombat(currentRns, bossGorgon, cormag1);
            cormagHp = result.Item2;
            if (result.Item1 > 0 || cormagHp <= 0 || cormagHp == 40)
            {
                return false;
            }

            combatPreview bael1 = new combatPreview(44, 0, false, 0, true, 42, 30, 10);
            combatPreview syrene1 = new combatPreview(89, 8, true, 0, true, syreneHp, 24, 10);
            MovementSim.simSimpleMovement(currentRns, 0, 0);
            result = simCombat(currentRns, bael1, syrene1);
            syreneHp = result.Item2;
            if (syreneHp <= 0)
            {
                return false;
            }

            combatPreview bael2 = new combatPreview(40, 0, false, 0, true, 40, 30, 10);
            combatPreview syrene2 = new combatPreview(88, 7, true, 0, true, syreneHp, 24, 10);
            MovementSim.simSimpleMovement(currentRns, 0, 1);
            result = simCombat(currentRns, bael2, syrene2);
            syreneHp = result.Item2;
            if (syreneHp <= 0)
            {
                return false;
            }

            combatPreview bael3 = new combatPreview(42, 0, false, 0, true, 41, 30, 10);
            combatPreview cormag2 = new combatPreview(82, 7, true, 12, true, cormagHp, 25, 12);
            MovementSim.simSimpleMovement(currentRns, 4, 1);
            result = simCombat(currentRns, bael3, cormag2);
            if (result.Item2 <= 0)
            {
                return false;
            }
            bael3died = (result.Item1 <= 0);
            simLevel(currentRns);

            if (bael3died)
            {
                combatPreview bael4 = new combatPreview(45, 0, false, 0, true, 42, 22, 9);
                combatPreview cormag2point5 = new combatPreview(79, 6, true, 13, true, 12, 25, 12);
                MovementSim.simSimpleMovement(currentRns, 3, 0);
                result = simCombat(currentRns, bael4, cormag2point5);
                if (result.Item2 < 12)
                {
                    return false;
                }
                bael4died = (result.Item1 <= 0);
            }
            else
            {
                combatPreview bael4 = new combatPreview(53, 0, false, 0, true, 42, 22, 9); // bottom bael, poison claw
                combatPreview ephraim1 = new combatPreview(77, 2, true, 0, true, 33, 16, 11);
                MovementSim.simSimpleMovement(currentRns, 3, 1);
                result = simCombat(currentRns, bael4, ephraim1);
                if (result.Item2 < 33)
                {
                    return false;
                }
                bael4died = (result.Item1 <= 0);
            }

            combatPreview bael5 = new combatPreview(44, 0, false, 0, true, 42, 30, 10);
            combatPreview cormag3 = new combatPreview(82, 7, true, 13, true, cormagHp, 25, 12);
            if (bael3died && bael4died)
            {
                MovementSim.simSimpleMovement(currentRns, 2, 1);
            }
            else if (bael3died || bael4died)
            {
                MovementSim.simSimpleMovement(currentRns, 3, 0);
            }
            else
            {
                MovementSim.simSimpleMovement(currentRns, 2, 2);
            }
            result = simCombat(currentRns, bael5, cormag3);
            if (result.Item2 <= 0)
            {
                return false;
            }
            bael5died = (result.Item1 <= 0);

            if (!bael3died && !bael4died && !bael5died)
            {
                return false;
            }

            combatPreview bael6 = new combatPreview(45, 0, false, 0, true, 41, 30, 10);
            combatPreview seth1 = new combatPreview(100, 9, true, 0, true, 30, 50, 13);
            MovementSim.simSimpleMovement(currentRns, 0, 0);
            result = simCombat(currentRns, bael6, seth1);
            if (result.Item2 < 30)
            {
                return false;
            }

            combatPreview bael7 = new combatPreview(46, 0, false, 0, true, 41, 30, 10);
            combatPreview seth2 = new combatPreview(100, 8, true, 0, true, 30, 50, 13);
            MovementSim.simSimpleMovement(currentRns, 0, 0);
            result = simCombat(currentRns, bael7, seth2);
            if (result.Item2 < 30)
            {
                return false;
            }

            combatPreview garg1 = new combatPreview(59, 0, false, 0, true, 33, 36, 11);
            combatPreview ephraim2 = new combatPreview(74, 3, true, 0, true, 33, 16, 11);
            MovementSim.simSimpleMovement(currentRns, 1, 1);
            result = simCombat(currentRns, garg1, ephraim2);
            if (result.Item2 < 33)
            {
                return false;
            }

            combatPreview garg2 = new combatPreview(51, 0, false, 0, true, 35, 24, 11);
            combatPreview tana1 = new combatPreview(86, 6, true, 0, true, 25, 13, 8);
            MovementSim.simSimpleMovement(currentRns, 2, 1);
            result = simCombat(currentRns, garg2, tana1);
            if (result.Item2 < 25)
            {
                return false;
            }

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
                advanceRng(currentRns);
            }

            MovementSim.simSimpleMovement(currentRns, 0, 1, 100);
            combatPreview knight1 = new combatPreview(72, 0, false, 0, true, 24, 20, 11);
            combatPreview eirika1 = new combatPreview(95, 13, true, 0, true, 16, 22, 3);
            (int, int) result = simCombat(currentRns, knight1, eirika1);
            if (result.Item1 < 0 || result.Item2 < 0)
            {
                return false;
            }

            advanceRng(currentRns);

            MovementSim.simSimpleMovement(currentRns, 1, 2, 100);
            combatPreview soldier1 = new combatPreview(55, 0, false, 0, true, 28, 19, 2);
            combatPreview seth1 = new combatPreview(94, 5, true, 0, true, 30, 21, 13);
            result = simCombat(currentRns, soldier1, seth1);

            for (int i = 0; i < 5; i++)
            {
                advanceRng(currentRns);
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
