using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace QTTermination
{
    public class IncidentWorker_QTVisit : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            return base.CanFireNowSub(parms) && map.mapPawns.AllPawnsSpawned.All(p => p.kindDef.defName != "QT_PawnKind");
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;

            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 entryCell, map, CellFinder.EdgeRoadChance_Neutral))
                return false;

            PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamed("QT_PawnKind");
            Faction faction = Find.FactionManager.OfPlayer; // aparece "neutral" ligada al jugador para poder interactuar

            PawnGenerationRequest request = new PawnGenerationRequest(
                kind,
                null,
                PawnGenerationContext.NonPlayer,
                map.Tile,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: false,
                canGeneratePawnRelations: false,
                colonistRelationChanceFactor: 0f,
                forceAddFreeWarmLayerIfNeeded: true);

            Pawn qt = PawnGenerator.GeneratePawn(request);
            qt.Name = new NameSingle("QT");

            GenSpawn.Spawn(qt, entryCell, map, Rot4.Random);

            LordJob_VisitColony lordJob = new LordJob_VisitColony(Faction.OfPlayer, map.Center);
            LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, map, new[] { qt });

            // Añade el núcleo de comportamiento: a partir de aquí, todo lo controla Hediff_QTCore.
            Hediff core = HediffMaker.MakeHediff(HediffDef.Named("QT_Core"), qt);
            qt.health.AddHediff(core);

            SendStandardLetter(
                "QT_ArrivalLetterLabel".Translate(),
                "QT_ArrivalLetterText".Translate(),
                LetterDefOf.PositiveEvent,
                parms,
                qt);

            return true;
        }
    }
}
