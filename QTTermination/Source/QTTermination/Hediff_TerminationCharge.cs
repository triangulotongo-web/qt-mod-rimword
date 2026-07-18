using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace QTTermination
{
    /// <summary>
    /// Representa la carga final de QT. Sube de severidad automáticamente.
    /// Mientras está activa, el mapa recibe un Gizmo global "¡ESQUIVAR!"
    /// (vía MapComponent, ver QTDodgeMapComponent) que el jugador debe
    /// pulsar dentro de una ventana de tiempo corta o QT detona.
    /// </summary>
    public class Hediff_TerminationCharge : HediffWithComps
    {
        public const int ChargeTicks = 420; // ~7 segundos a velocidad normal
        public const int DodgeWindowTicks = 90; // ~1.5s de margen real para pulsar el gizmo

        private int ticksAlive = 0;
        private bool dodgePressedThisCycle = false;

        public bool InDodgeWindow => ticksAlive >= ChargeTicks - DodgeWindowTicks;
        public float ChargeFraction => Mathf.Clamp01((float)ticksAlive / ChargeTicks);

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            ticksAlive = 0;
            dodgePressedThisCycle = false;
            FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 4f);
        }

        public override void PostTick()
        {
            base.PostTick();
            if (pawn == null || pawn.Dead || pawn.Map == null)
                return;

            ticksAlive++;
            Severity = ChargeFraction;

            if (ticksAlive >= ChargeTicks)
            {
                if (dodgePressedThisCycle)
                {
                    // ¡Esquivado! Reinicia la carga con un breve respiro (más rápido cada vez).
                    Messages.Message("QT_DodgedMessage".Translate().Resolve(), pawn, MessageTypeDefOf.PositiveEvent);
                    ticksAlive = 0;
                    dodgePressedThisCycle = false;
                }
                else
                {
                    Detonate();
                }
            }
        }

        public void RegisterDodgePressed()
        {
            if (InDodgeWindow)
            {
                dodgePressedThisCycle = true;
                FleckMaker.ThrowMicroSparks(pawn.DrawPos, pawn.Map);
            }
            else
            {
                // Pulsado demasiado pronto: penalización menor, la carga se acelera.
                ticksAlive = System.Math.Min(ticksAlive + 60, ChargeTicks - 1);
                Messages.Message("QT_DodgedTooEarlyMessage".Translate().Resolve(), MessageTypeDefOf.RejectInput);
            }
        }

        private void Detonate()
        {
            Map map = pawn.Map;
            IntVec3 pos = pawn.Position;

            GenExplosion.DoExplosion(
                center: pos,
                map: map,
                radius: 6.9f,
                damType: DamageDefOf.Bomb,
                instigator: pawn,
                damAmount: 80,
                armorPenetration: 0.6f);

            Messages.Message("QT_TerminationDetonatedMessage".Translate().Resolve(), new TargetInfo(pos, map), MessageTypeDefOf.NegativeEvent);

            // La propia QT vuelve al estado inicial tras detonar (queda muy débil).
            ticksAlive = 0;
            dodgePressedThisCycle = false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksAlive, "ticksAlive", 0);
            Scribe_Values.Look(ref dodgePressedThisCycle, "dodgePressedThisCycle", false);
        }
    }
}
