using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace QTTermination
{
    /// <summary>
    /// Hediff oculto en QT que vigila el combate y va escalando fases:
    /// Carefree -> Careless -> Sensory Overload -> Termination.
    /// Se dispara por daño recibido o por tiempo en combate; también puede
    /// escalar sola tras cierto tiempo desde que entra en combate.
    /// </summary>
    public class Hediff_QTCore : HediffWithComps
    {
        private int ticksInCombat = 0;
        private int currentPhase = 0; // 0=none,1=Carefree,2=Careless,3=SensoryOverload,4=Termination
        private int lastPulseTick = -9999;

        private static readonly HediffDef[] PhaseDefs =
        {
            null,
            HediffDef.Named("QT_Phase_Carefree"),
            HediffDef.Named("QT_Phase_Careless"),
            HediffDef.Named("QT_Phase_SensoryOverload"),
            HediffDef.Named("QT_Phase_Termination"),
        };

        // Cuántos ticks de combate hacen falta para pasar de cada fase a la siguiente.
        private static readonly int[] TicksToEscalate = { 0, 900, 1400, 1800, int.MaxValue };

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            SetPhase(1);
        }

        public override void PostTick()
        {
            base.PostTick();

            if (pawn == null || pawn.Dead || pawn.Map == null)
                return;

            bool inDanger = pawn.mindState != null && pawn.mindState.enemyTarget != null;
            bool wasDamagedRecently = pawn.health?.summaryHealth != null && pawn.health.summaryHealth.SummaryHealthPercent < 0.9f;

            if (inDanger || wasDamagedRecently)
            {
                ticksInCombat++;
            }

            if (currentPhase > 0 && currentPhase < 4 && ticksInCombat >= TicksToEscalate[currentPhase])
            {
                ticksInCombat = 0;
                SetPhase(currentPhase + 1);
            }

            // Pulso de "Sensory Overload": cada ~4 segundos aturde a pawns cercanos.
            if (currentPhase == 3 && Find.TickManager.TicksGame - lastPulseTick > 240)
            {
                lastPulseTick = Find.TickManager.TicksGame;
                EmitSensoryPulse();
            }
        }

        private void SetPhase(int phase)
        {
            if (phase == currentPhase)
                return;

            // Quita el hediff de la fase anterior si lo hay.
            if (currentPhase > 0 && currentPhase < PhaseDefs.Length && PhaseDefs[currentPhase] != null)
            {
                Hediff old = pawn.health.hediffSet.GetFirstHediffOfDef(PhaseDefs[currentPhase]);
                if (old != null)
                    pawn.health.RemoveHediff(old);
            }

            currentPhase = phase;

            if (currentPhase > 0 && currentPhase < PhaseDefs.Length && PhaseDefs[currentPhase] != null)
            {
                Hediff newHediff = HediffMaker.MakeHediff(PhaseDefs[currentPhase], pawn);
                pawn.health.AddHediff(newHediff);
            }

            if (currentPhase == 4)
            {
                Messages.Message(
                    "QT_TerminationBeginMessage".Translate(pawn.LabelShort).Resolve(),
                    pawn,
                    MessageTypeDefOf.ThreatBig);
            }
            else if (currentPhase == 2)
            {
                Messages.Message(
                    pawn.LabelShort + " empieza a comportarse de forma errática.",
                    pawn,
                    MessageTypeDefOf.ThreatSmall);
            }
            else if (currentPhase == 3)
            {
                Messages.Message(
                    pawn.LabelShort + " emite una energía caótica. ¡Alejaos!",
                    pawn,
                    MessageTypeDefOf.ThreatBig);
            }
        }

        private void EmitSensoryPulse()
        {
            FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 3.5f);

            foreach (Pawn other in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, 6.5f, true)
                         .OfType<Pawn>())
            {
                if (other == pawn || other.Dead || other.RaceProps == null || !other.RaceProps.Humanlike)
                    continue;

                Hediff existing = other.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("QT_Disoriented"));
                if (existing == null)
                {
                    Hediff h = HediffMaker.MakeHediff(HediffDef.Named("QT_Disoriented"), other);
                    other.health.AddHediff(h);
                }
                else
                {
                    existing.Severity = 1f;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksInCombat, "ticksInCombat", 0);
            Scribe_Values.Look(ref currentPhase, "currentPhase", 0);
            Scribe_Values.Look(ref lastPulseTick, "lastPulseTick", -9999);
        }
    }
}
