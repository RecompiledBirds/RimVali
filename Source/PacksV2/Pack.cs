using AvaliMod;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimvali.Rewrite.Packs
{
    /// <summary>
    /// This class represents a pack of avali.
    /// </summary>
    public class Pack : IExposable, ILoadReferenceable
    {
        private Date creationDate = new Date();
        private List<DeathDate> deathDates = new List<DeathDate>();
        private Faction faction;
        private Pawn leaderPawn;
        private string name = "NoName";
        private HashSet<Pawn> pawns = new HashSet<Pawn>();
        private int id;

        /// <summary>
        /// Create a pack with the intial leader and the pack's faction affiliation.
        /// </summary>
        /// <param name="faction"></param>
        /// <param name="pawn"></param>
        public Pack(Faction faction, Pawn pawn, int id)
        {
            this.faction = faction;
            SetLeader(pawn);
            this.id = id;
            this.name = $"{pawn.Name.ToStringShort}'s pack";
        }

        /// <summary>
        /// FOR USE WITH LOADING. SHOULD NOT BE USED ELSEWHERE.
        /// </summary>
        public Pack() { }

        

        /// <summary>
        /// Gets all pawns in the pack, including the leader.
        /// </summary>
        public HashSet<Pawn> GetAllPawns
        {
            get
            {
                HashSet<Pawn> result = new HashSet<Pawn>();
                result.AddRange(GetPawns);
                result.Add(leaderPawn);
                return result;
            }
        }

        public HashSet<Pawn> GetPawns
        {
            get
            {
                return pawns;
            }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Faction Faction
        {
            get
            {
                return faction;
            }
        }

        public Pawn Leader
        {
            get
            {
                return leaderPawn;
            }
        }

        public Date CreationDate
        {
            get
            {
                return creationDate;
            }
        }

        public int ID
        {
            get
            {
                return id;
            }
        }

        public List<DeathDate> DeathDates
        {
            get
            {
                return deathDates;
            }
        }

        /// <summary>
        /// Pack's overall opinion of a pawn.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public float GetAvgOpinionOf(Pawn pawn)
        {
            float result = GetAllPawns.Sum(packMember => packMember.relations.OpinionOf(pawn)) /
                   (float)(GetAllPawns.Count());
            RimValiUtility.LogAnaylitics($"Pack {this.name} average opinion of {pawn.Name.ToStringShort}: {result}");
            return result;
        }

        /// <summary>
        /// Get a pawn's opinion of a pack.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public float GetPawnOpinionOf(Pawn pawn)
        {
            float result = GetAllPawns.Sum(packMember => pawn.relations.OpinionOf(packMember) / (float)GetAllPawns.Count); 
            RimValiUtility.LogAnaylitics($"Pawn {pawn.Name.ToStringShort} average opinion of {this.name}: {result}");
            return result;
        }
       
        /// <summary>
        /// Sets the leader of the pack.
        /// </summary>
        /// <param name="pawn"></param>
        public void SetLeader(Pawn pawn)
        {
            RimValiUtility.LogAnaylitics($"Setting {pawn.Name.ToStringShort} as leader of {this.Name}");
            leaderPawn = pawn;
            if (pawns.Contains(pawn))
                pawns.Remove(pawn);
        }

        /// <summary>
        /// Check if a pawn's packmates are in the same room.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public bool CheckIfPackmatesInRoom(Pawn pawn)
        {
            if (pawn.Spawned && pawn.Map != null)
            {
                Room room = pawn.GetRoom();
                return room != null && pawn.Position.Roofed(pawn.Map) && this.GetAllPawns.Any(packmate => pawn!=packmate && packmate.Spawned && packmate.GetRoom() != null && packmate.GetRoom() == room);
            }

            return false;
        }

        /// <summary>
        /// Add a pawn to the pack.
        /// </summary>
        /// <param name="pawn">The pawn to be added.</param>
        public void AddPawn(Pawn pawn) => pawns.Add(pawn);

        /// <summary>
        /// Remove a pawn from the pack. 
        /// </summary>
        /// <param name="pawn">The pawn to remove.</param>
        /// <param name="registerAsDead">Should the pawn be set as dead?</param>
        public void RemovePawn(Pawn pawn, bool registerAsDead = false)
        {
            RimValiUtility.LogAnaylitics($"Removing {pawn.Name.ToStringShort} from {this.Name}");
            if (pawns.Contains(pawn))
            {
                pawns.Remove(pawn);
            }
            if (pawn == leaderPawn)
            {
                Pawn packmate = pawns.First();
                SetLeader(packmate);
            }
            if (registerAsDead)
            {
                deathDates.Add(new DeathDate(pawn));
            }
            PacksV2WorldComponent packsComp = Find.World.GetComponent<PacksV2WorldComponent>();
            packsComp.ClearPawnPack(pawn);

        }




        public void ExposeData()
        {
            Scribe_References.Look(ref leaderPawn, "leader");
            Scribe_Collections.Look(ref deathDates, "deathDates",LookMode.Deep);
            Scribe_Collections.Look(ref pawns, "pawns",LookMode.Reference);
            Scribe_Values.Look(ref name, "name");
            Scribe_Deep.Look(ref creationDate, "creationDate");
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref id,"id");
        }

        public string GetUniqueLoadID()
        {
            return $"pack_{id}";
        }
    }
}
