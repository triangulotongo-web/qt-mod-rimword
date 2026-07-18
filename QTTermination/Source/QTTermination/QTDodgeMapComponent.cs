using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace QTTermination
{
    /// <summary>
    /// Busca en el mapa a cualquier QT en fase Termination y, si la hay,
    /// añade un Gizmo global visible para el jugador con la cuenta atrás
    /// y el botón de esquive. Esto imita la ventana de reacción de un
    /// juego de ritmo sin necesitar una interfaz de notas completa.
    /// </summary>
    public class QTDodgeMapComponent : MapComponent
    {
        public QTDodgeMapComponent(Map map) : base(map) { }

        public Hediff_TerminationCharge FindActiveCharge()
        {
            foreach (Pawn p in map.mapPawns.AllPawnsSpawned)
            {
                if (p.health?.hediffSet == null) continue;
                Hediff h = p.health.hediffSet.hediffs.FirstOrDefault(x => x is Hediff_TerminationCharge);
                if (h != null)
                    return (Hediff_TerminationCharge)h;
            }
            return null;
        }
    }

    /// <summary>
    /// Gizmo global inyectado vía Harmony en MainTabWindow_Menu /
    /// la barra de comandos del colonista seleccionado. Ver HarmonyPatches.cs.
    /// </summary>
    public class Gizmo_QTDodge : Gizmo
    {
        private readonly Hediff_TerminationCharge charge;

        public Gizmo_QTDodge(Hediff_TerminationCharge charge)
        {
            this.charge = charge;
            Order = -200f;
        }

        public override float GetWidth(float maxWidth) => 200f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Widgets.DrawWindowBackground(rect);

            bool ready = charge.InDodgeWindow;
            Color barColor = ready ? new Color(1f, 0.2f, 0.2f) : new Color(0.9f, 0.6f, 0.9f);

            Rect barRect = rect.ContractedBy(6f);
            barRect.height = 16f;
            Widgets.FillableBar(barRect, charge.ChargeFraction, SolidColorMaterials.NewSolidColorTexture(barColor));

            Rect labelRect = rect.ContractedBy(6f);
            labelRect.y += 18f;
            labelRect.height = 20f;
            string label = ready ? "¡ESQUIVAR AHORA!" : "QT está cargando...";
            GUI.color = ready ? Color.red : Color.white;
            Widgets.Label(labelRect, label);
            GUI.color = Color.white;

            Rect buttonRect = rect.ContractedBy(6f);
            buttonRect.y += 40f;
            buttonRect.height = 28f;

            bool clicked = Widgets.ButtonText(buttonRect, "Esquivar");
            if (clicked)
            {
                charge.RegisterDodgePressed();
                return new GizmoResult(GizmoState.Interacted);
            }

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
