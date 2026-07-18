using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace QTTermination
{
    [StaticConstructorOnStartup]
    public static class QTHarmonyInit
    {
        static QTHarmonyInit()
        {
            Harmony harmony = new Harmony("kyliam.qttermination");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("[QT: Termination] Harmony patches cargados.");
        }
    }

    /// <summary>
    /// Se engancha a la generación de gizmos de CUALQUIER pawn del jugador
    /// seleccionado para insertar el Gizmo_QTDodge cuando hay una QT en fase
    /// Termination activa en el mapa. Así el jugador no necesita tener
    /// seleccionada a QT para poder reaccionar a tiempo.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
    public static class Patch_Pawn_GetGizmos_QTDodge
    {
        public static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance == null || __instance.Map == null || !__instance.IsColonistPlayerControlled)
                return;

            QTDodgeMapComponent comp = __instance.Map.GetComponent<QTDodgeMapComponent>();
            if (comp == null)
                return;

            Hediff_TerminationCharge charge = comp.FindActiveCharge();
            if (charge == null)
                return;

            __result = __result.Concat(new Gizmo[] { new Gizmo_QTDodge(charge) });
        }
    }
}
