using System;

namespace FEBruteForcer
{
    class CombatSim
    {
        static AttackResult simAttack(CombatPreview attackerPreview, CombatPreview defenderPreview)
        {
            bool sureStrike = false;
            if (attackerPreview.sureStrike)
            {
                int sureStrikeRn = FEBruteForcer.nextRn();
                if (sureStrikeRn < attackerPreview.level)
                {
                    sureStrike = true;
                }
            }

            if (!sureStrike)
            {
                int hitRn1 = FEBruteForcer.nextRn();
                int hitRn2 = FEBruteForcer.nextRn();

                if ((hitRn1 + hitRn2) >= attackerPreview.hit * 2)
                {
                    return AttackResult.Miss;
                }
            }

            bool greatShield = false;
            if (defenderPreview.greatShield)
            {
                int greatShieldRn = FEBruteForcer.nextRn();
                if (greatShieldRn < attackerPreview.level)
                {
                    greatShield = true;
                }
            }

            bool pierced = false;
            if (attackerPreview.pierce && !greatShield)
            {
                int pierceRn = FEBruteForcer.nextRn();
                if (pierceRn < attackerPreview.level)
                {
                    pierced = true;
                }
            }

            bool crit = false;
            int critRn = FEBruteForcer.nextRn();
            if (critRn < attackerPreview.crit)
            {
                crit = true;
            }

            bool silencer = false;
            if (crit)
            {
                int silencerRn = FEBruteForcer.nextRn();
                if (attackerPreview.silencer && silencerRn < 50)
                {
                    silencer = true;
                }
            }

            if (silencer)
            {
                return AttackResult.Silencer;
            }
            if (greatShield)
            {
                return AttackResult.Miss;
            }
            if (pierced)
            {
                return crit ? AttackResult.PierceCrit : AttackResult.PierceHit;
            }
            return crit ? AttackResult.Crit : AttackResult.Hit;
        }

        static int combatHpLoss(CombatPreview attackerPreview, CombatPreview defenderPreview)
        {
            AttackResult result = simAttack(attackerPreview, defenderPreview);

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
                case AttackResult.Silencer:
                    return defenderPreview.currentHp;
                default:
                    return 0;
            }
        }

        public static (int, int) simCombat(CombatPreview attackerPreview, CombatPreview defenderPreview)
        {
            int attackerCurrentHp = attackerPreview.currentHp;
            int defenderCurrentHp = defenderPreview.currentHp;

            defenderCurrentHp -= combatHpLoss(attackerPreview, defenderPreview);

            if (defenderCurrentHp <= 0)
            {
                return (attackerCurrentHp, defenderCurrentHp);
            }

            if (defenderPreview.inRange)
            {
                attackerCurrentHp -= combatHpLoss(defenderPreview, attackerPreview);

                if (attackerCurrentHp <= 0)
                {
                    return (attackerCurrentHp, defenderCurrentHp);
                }
            }

            if (attackerPreview.doubles)
            {
                defenderCurrentHp -= combatHpLoss(attackerPreview, defenderPreview);
            }
            else if (defenderPreview.doubles && defenderPreview.inRange)
            {
                attackerCurrentHp -= combatHpLoss(defenderPreview, attackerPreview);
            }

            return (attackerCurrentHp, defenderCurrentHp);
        }
    }

    class CombatPreview
    {
        public int hit = 0;
        public int crit = 0;
        public bool doubles = false;
        public int currentHp = 1;
        public int atk = 1;
        public int def = 0;
        public bool inRange = true;
        public int level = 1;
        public bool pierce = false;
        public bool greatShield = false;
        public bool sureStrike = false;
        public bool silencer = false;
    }

    enum AttackResult
    {
        Miss,
        Hit,
        PierceHit,
        Crit,
        PierceCrit,
        Silencer,
    }
}
