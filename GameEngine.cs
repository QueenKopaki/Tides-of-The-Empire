using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StarWarsRpgCs;

public class GameCharacter
{
    public string Name { get; set; } = "";
    public string Species { get; set; } = "Human";
    public string Role { get; set; } = "Smuggler";
    public string Homeworld { get; set; } = "Corellia";
    public string Background { get; set; } = "Scoundrel";
    public Dictionary<string, int> Stats { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public List<string> Inventory { get; set; } = new();
    public List<string> HangarInventory { get; set; } = new();
    public List<string> KnownBlueprints { get; set; } = new();
    public List<string> Crafting { get; set; } = new();
    public string EquippedWeapon { get; set; } = "Blaster Pistol";
    public Ship? Ship { get; set; }
    public string Location { get; set; } = "";
    public int Credits { get; set; }
    public int CurrencyKind { get; set; } = 0;
    public string CurrencyType { get; set; } = "Galactic Credits";
    public int Armor { get; set; }
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Stamina { get; set; }
    public int MaxStamina { get; set; }
    public string Condition { get; set; } = "Healthy";
    public string CurrentState { get; set; } = "Steady";
    public int Stress { get; set; }
    public int Morale { get; set; }
    public int Reputation { get; set; }
    public string Faction { get; set; } = "Independent";
    public int Experience { get; set; }
    public List<string> Notes { get; set; } = new();
    public List<string> StatusEffects { get; set; } = new();
    public List<string> ReputationHistory { get; set; } = new();
    public FamilyRecord Family { get; set; } = new();
    public Dictionary<string, int> SpeciesRelations { get; set; } = new();
    public bool IsAlive { get; set; } = true;
    public int Age { get; set; } = 22;
    public bool ForceResistance { get; set; }
    public bool HasLatentForcePotential { get; set; }
    public bool ForcePotentialKnown { get; set; }
    public bool ForceTrainingDeclined { get; set; }
    public string ForceSeekerFaction { get; set; } = "";
    public int LastLootRotation { get; set; } = -999;
    public int LastEncounterRotation { get; set; } = -999;
    public int LastMiningRotation { get; set; } = -999;
    public List<string> OwnedVehicles { get; set; } = new();
    public string ActiveVehicle { get; set; } = "";

    // ── Jedi / Force system ──────────────────────────────────────────────────
    public bool IsForceUser { get; set; }               // set once Jedi training accepted
    public string JediRank { get; set; } = "";          // Initiate → Padawan → Knight → Master
    public int JediXp { get; set; }                     // XP within Jedi order
    public bool LightsaberCrafted { get; set; }
    public string LightsaberColor { get; set; } = "";   // e.g. "blue", "green", "yellow"
    public List<string> ForceAbilities { get; set; } = new(); // unlocked abilities
    public int ForcePoints { get; set; }                // current force pool
    public int MaxForcePoints { get; set; }             // scales with rank
    public int ForceAwakeningRotation { get; set; } = -1; // -1 = not yet assigned
    public bool JediAwakeningTriggered { get; set; }
    public bool JediTrainingDeclined { get; set; }
    public string JediMasterName { get; set; } = "";    // the Jedi who found you
    public bool SpouseHidden { get; set; }              // Jedi hiding attachment

    // ── Armor + equipment ───────────────────────────────────────────────────
    public string EquippedArmor { get; set; } = "";     // name of currently worn armor
    public int MaxInventorySlots { get; set; } = 24;     // backpack capacity (items)

    // ── Resource gathering ──────────────────────────────────────────────────
    public int LastHarvestRotation { get; set; } = -999;
    public int LastWoodcutRotation { get; set; } = -999;
    public List<string> UnlockedSubZones { get; set; } = new(); // e.g. "Tibanna Gas Clouds"

    // ── Survival / nutrition ────────────────────────────────────────────────
    public int Hunger { get; set; } = 100;          // 0 = starving, 100 = well-fed
    public int Energy { get; set; } = 100;          // 0 = exhausted, 100 = energized
    public int LastMealRotation { get; set; } = 0;  // last rotation the character ate

    // ── Slot-based armor + backpack ─────────────────────────────────────────────────
    // slot keys: "Helmet" "Chest" "Arms" "Legs" "Boots" "Belt" "Full Suit"
    public Dictionary<string, string> EquippedArmorPieces { get; set; } = new();
    public string EquippedBackpack { get; set; } = "";
    public long BaseInventoryMicroScu { get; set; } = 2_000_000L; // 2 SCU base (on-body)
    public PlayerHome? Home { get; set; }

    // ── Weapon / tool slots ───────────────────────────────────────────────────
    public string OffHandWeapon { get; set; } = "";  // off-hand (pistols, shoto, etc.)
    public string EquippedTool  { get; set; } = "";  // active harvesting tool

    // ── Discovery tracking ───────────────────────────────────────────────────
    // Items seen for the first time trigger basic processing blueprint unlocks.
    public HashSet<string> DiscoveredItems { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // ── Blaster mods (weapon name → slot → mod name) ────────────────────────
    public Dictionary<string, Dictionary<string, string>> InstalledMods { get; set; } = new();
}

public class FamilyRecord
{
    public bool Married { get; set; }
    public string? SpouseName { get; set; }
    public string? SpouseSpecies { get; set; }
    public List<string> Children { get; set; } = new();

    // Romance system
    public int LovePoints { get; set; } = 0;        // 0–100; 70+ allows proposal
    public string FlirtStage { get; set; } = "stranger";  // stranger → acquaintance → crush → devoted → engaged → married
    public string RomanceTargetName { get; set; } = "";
    public string RomanceTargetSpecies { get; set; } = "";
    public string RomanceTargetPlanet { get; set; } = "";
    public int HeartbreakCooldown { get; set; } = 0; // rotations before new romance
    public List<string> GiftLog { get; set; } = new();
    public List<string> RomanceHistory { get; set; } = new();
}

public class Ship
{
    public string Name { get; set; } = "Marauder";
    public string Model { get; set; } = "YT-1300";
    public List<string> Ascii { get; set; } = new();
    public int Hull { get; set; } = 24;
    public int Shield { get; set; } = 12;
    public int CrewCapacity { get; set; } = 6;
    public List<string> Crew { get; set; } = new();
    public int Fuel { get; set; } = 100;
    public int MaxFuel { get; set; } = 100;
    public string SizeClass { get; set; } = "M";
    public int HyperdriveClass { get; set; } = 3;
    public int FuelEfficiencyPercent { get; set; } = 100;
    public int TravelHoursModifier { get; set; }
    public string BaseWeapon { get; set; } = "Laser cannon";
    public string Weapon { get; set; } = "Laser cannon";
    public List<string> Parts { get; set; } = new();
    public List<string> Upgrades { get; set; } = new();
    public List<string> Armaments { get; set; } = new();
    public List<ShipHardpointSlot> HardpointSlots { get; set; } = new();
    // ── Cargo & special modules ──────────────────────────────────────────────
    public int CargoCapacity { get; set; } = 20;         // cargo slot count
    public List<string> CargoItems { get; set; } = new(); // items in cargo hold
    public bool HasGasHarvester { get; set; }            // Gas Harvesting Laser fitted
    public int GasHoldCount { get; set; }                // number of Gas Hold modules
    // 0 = auto-derive from SizeClass; set explicitly to override
    public long PersonalStorageCapacityMicroScu { get; set; } = 0L;
    public List<string> PersonalStorageItems { get; set; } = new();
}

public class ShipHardpointSlot
{
    public string SlotName { get; set; } = "";
    public string Position { get; set; } = "Hull";
    public string SlotSize { get; set; } = "S";
    public List<string> AllowedCategories { get; set; } = new();
    public string MountedArmament { get; set; } = "";
}

public class PlanetData
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Economy { get; set; } = "";
    public string Region { get; set; } = "";
    public List<string> DayEvents { get; set; } = new();
    public List<string> NightEvents { get; set; } = new();
    public int TravelCost { get; set; } = 20;
    public string Era { get; set; } = "Old Republic";
    public string ThreatLevel { get; set; } = "Moderate";
    public string Sector { get; set; } = "Unknown";
    public bool HasDockyard { get; set; } = false;
    public bool HasIndustrialFurnace { get; set; } = false;
    // Zones valid on this planet. Empty = use defaults for planet type.
    public List<string> ValidZones { get; set; } = new();
    // Shipyard manufacturer — only ships from this maker available here. Empty = general catalog.
    public string ShipyardManufacturer { get; set; } = "";
    public string ShipyardName { get; set; } = "";
    // Orbital station names (if any)
    public List<string> OrbitalStations { get; set; } = new();
}

public class SpaceStation
{
    public string Name { get; set; } = "";
    public string OrbitingPlanet { get; set; } = "";
    public string Description { get; set; } = "";
    public string Region { get; set; } = "";
    public string Manufacturer { get; set; } = ""; // ships available here from this maker
    public bool HasShipyard { get; set; } = true;
    public bool HasMerchant { get; set; } = true;
    public int TravelCostFromPlanet { get; set; } = 5;
    public int LinerFeeFromGalaxy { get; set; } = 200;
}

public class PlanetImportEntry
{
    public string Name { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public string Description { get; set; } = "";
    public string Region { get; set; } = "Unknown Regions";
    public string Sector { get; set; } = "Unknown";
    public string Economy { get; set; } = "Trade and survival";
    public string Era { get; set; } = "Old Republic";
    public string ThreatLevel { get; set; } = "Moderate";
    public int TravelCost { get; set; } = 20;
    public bool HasDockyard { get; set; }
    public bool HasIndustrialFurnace { get; set; }
    public List<string> DayEvents { get; set; } = new();
    public List<string> NightEvents { get; set; } = new();
}

public class WorldClock
{
    public int Rotation { get; private set; } = 1;
    public int Hour { get; private set; } = 8;
    public string TimeOfDay => Hour < 6 ? "night" : Hour < 18 ? "day" : "night";

    public void SetTime(int rotation, int hour)
    {
        Rotation = rotation;
        Hour = hour;
        while (Hour >= 24) { Hour -= 24; Rotation += 1; }
        while (Hour < 0) { Hour += 24; Rotation -= 1; }
    }

    public void Advance(int hours)
    {
        Hour += hours;
        while (Hour >= 24) { Hour -= 24; Rotation += 1; }
    }
}

public class RaceData
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string EraUnlock { get; set; } = "Old Republic";
    public bool RequiresMarriage { get; set; }
    public Dictionary<string, int> BaseStats { get; set; } = new();
    public List<string> StartingSkills { get; set; } = new();
}

public class EraData
{
    public string Name { get; set; } = "";
    public int StartRotation { get; set; }
    public string Description { get; set; } = "";
}

public class ShipBlueprint
{
    public string Name { get; set; } = "";
    public string Model { get; set; } = "";
    public string Era { get; set; } = "Old Republic";
    /// <summary>Base craft / component cost used by the crafting system.</summary>
    public int Cost { get; set; }
    /// <summary>Showroom purchase price paid to a dealer — no parts or blueprint required.</summary>
    public int PurchasePrice { get; set; }
    /// <summary>"planetary" = available at any dockyard; "orbital" = requires an orbital station shipyard.</summary>
    public string ShipyardTier { get; set; } = "planetary";
    public List<string> RequiredParts { get; set; } = new();
    public List<string> Ascii { get; set; } = new();
    public int Hull { get; set; }
    public int Shield { get; set; }
    public int Fuel { get; set; }
    public int CrewCapacity { get; set; }
    public string SizeClass { get; set; } = "M";
    public int HyperdriveClass { get; set; } = 3;
    public string Weapon { get; set; } = "Laser";
    public string Description { get; set; } = "";
    public bool IsCapital { get; set; }
}

public class ShipUpgradeDefinition
{
    public string Name { get; set; } = "";
    public string UpgradeKind { get; set; } = "utility";
    public string MinimumShipSize { get; set; } = "S";
    public string Era { get; set; } = "Old Republic";
    public int HyperdriveClassTarget { get; set; }
    public int FuelCapacityBonus { get; set; }
    public int FuelEfficiencyDeltaPercent { get; set; }
    public int TravelHoursModifier { get; set; }
    public int ShieldBonus { get; set; }
    public int HullBonus { get; set; }
    public int RefuelAmount { get; set; }
    public string WeaponOverride { get; set; } = "";
    public bool Consumable { get; set; }
    public bool Unique { get; set; } = true;
    public string Description { get; set; } = "";
    public string SourceUrl { get; set; } = "";
}

public class ShipArmamentData
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "laser";
    public string HardpointSize { get; set; } = "S";
    public string Era { get; set; } = "Old Republic";
    public int DamageRating { get; set; } = 8;
    public int FuelDraw { get; set; }
    public string Description { get; set; } = "";
    public string SourceUrl { get; set; } = "";
}

public class ShipArmamentImportEntry
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "laser";
    public string HardpointSize { get; set; } = "S";
    public string Era { get; set; } = "Old Republic";
    public int DamageRating { get; set; } = 8;
    public int FuelDraw { get; set; }
    public string Description { get; set; } = "";
    public string SourceUrl { get; set; } = "";
}

public class VehicleBlueprint
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "speeder";
    public string Era { get; set; } = "Old Republic";
    public List<string> Ascii { get; set; } = new();
    // Zone types this vehicle unlocks access to
    public List<string> UnlocksZones { get; set; } = new();
}

public class WeaponBlueprint
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "blaster";
    /// <summary>pistol | blaster_rifle | sniper_rifle | heavy_blaster | rotary_cannon | lightsaber | shoto_lightsaber | vibroblade | melee | ion | disruptor_rifle | explosive</summary>
    public string WeaponSubtype { get; set; } = "";
    /// <summary>True = can be equipped as off-hand or tool slot holder.</summary>
    public bool IsOneHanded { get; set; } = true;
    public string Era { get; set; } = "Old Republic";
    public int Damage { get; set; }
    public string Description { get; set; } = "";
}

public class WeaponCombatAbility
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> ApplicableSubtypes { get; set; } = new();
    public string RequiresSkill { get; set; } = "";
    public int StaminaCost { get; set; } = 5;
    public int DamageMultPercent { get; set; } = 100;  // 100=normal, 200=double
    public int FlatDamageBonus { get; set; }
    public bool IsMultiHit { get; set; }
    public int HitCount { get; set; } = 1;
    public bool ArmorPiercing { get; set; }
    public bool CanStun { get; set; }
    public bool IsSuppressive { get; set; }
    public bool RequiresDualWield { get; set; }
}

public class BlasterMod
{
    public string Name          { get; set; } = "";
    /// <summary>Scope | Barrel | Grip | Stock | Underbarrel | Power Cell</summary>
    public string Slot          { get; set; } = "";
    /// <summary>Legal | Illegal | Crafted</summary>
    public string Category      { get; set; } = "Legal";
    public string Description   { get; set; } = "";
    public int    DamageBonus   { get; set; }
    public string SpecialEffect { get; set; } = "";
    public bool   HasExplosionRisk  { get; set; }
    public double ExplosionChance   { get; set; }
    public int    Price         { get; set; }
}

public class CreatureData
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "fauna";
    public string SizeClass { get; set; } = "medium"; // tiny, small, medium, large, huge, colossal
    public string Habitat { get; set; } = "general";
    public int DangerRating { get; set; } = 2; // 1..10
    public string SourceUrl { get; set; } = "";
}

public class ItemBlueprint
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "gear";
    public string Description { get; set; } = "";
    public string Skill { get; set; } = "Crafting";
    public int Cost { get; set; }
}

public class RecipeComponent
{
    public string Item { get; set; } = "";
    public int Quantity { get; set; } = 1;
}

public class CraftRecipe
{
    public string Output { get; set; } = "";
    public int OutputQuantity { get; set; } = 1;
    public int CreditCost { get; set; }
    public int TimeHours { get; set; } = 1;
    public string Skill { get; set; } = "Crafting";
    public bool RequiresShipyard { get; set; }
    public bool RequiresIndustrialFurnace { get; set; }
    public bool RequiresBlueprint { get; set; }
    public string BlueprintName { get; set; } = "";
    public string BlueprintSourceHint { get; set; } = "";
    public bool HideUntilBlueprintUnlocked { get; set; }
    public List<RecipeComponent> Inputs { get; set; } = new();
    public string Notes { get; set; } = "";
}

public class Quest
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string TargetPlanet { get; set; } = "";
    public string Faction { get; set; } = "Independent";
    public string Status { get; set; } = "active";
    public int RewardCredits { get; set; }
    public int RewardXp { get; set; }
    public string RewardItem { get; set; } = "";
    public string RewardBlueprint { get; set; } = "";
    public string ObjectiveType { get; set; } = "generic";
    public string ObjectiveTarget { get; set; } = "";
    public string ObjectiveZone { get; set; } = "";
    public int ObjectiveRequired { get; set; } = 1;
    public int ObjectiveProgress { get; set; }
    public string IssuerName { get; set; } = "";
    public string IssuerSpecies { get; set; } = "";
    public string IssuerPlanet { get; set; } = "";
    public bool IsNpcGenerated { get; set; }
    // ── Quest chain fields ──────────────────────────────────────────────
    public string ChainId   { get; set; } = "";   // empty = standalone
    public int    ChainStep { get; set; }          // 1-based step index
    public string ChainNextStep { get; set; } = ""; // Id of quest to unlock on completion
}

public class QuestChain
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Faction { get; set; } = "";
    public string Description { get; set; } = "";
    public List<Quest> Steps { get; set; } = new();
}

public class EncounterResult
{
    public string Summary { get; set; } = "";
    public string Dialogue { get; set; } = "";
    public string Outcome { get; set; } = "";
    public int RewardCredits { get; set; }
    public bool IsHostile { get; set; }
    public string ThreatLevel { get; set; } = "Moderate";
    public string Zone { get; set; } = "market";
    public string NpcName { get; set; } = "";
    public string NpcSpecies { get; set; } = "";
    public int SpeciesRelation { get; set; }
}

public class CombatEncounter
{
    public string Title { get; set; } = "";
    public string PlanetName { get; set; } = "";
    public string Zone { get; set; } = "";
    public string EnemyName { get; set; } = "";
    public int EnemyHp { get; set; }
    public int EnemyMaxHp { get; set; }
    public int EnemyArmor { get; set; }
    public int EnemyDamage { get; set; }
    public int PlayerHp { get; set; }
    public int PlayerMaxHp { get; set; }
    public bool IsOver { get; set; }
    public bool PlayerWon { get; set; }
    public int RewardCredits { get; set; }
    public int RewardXp { get; set; }
    public string RewardItem { get; set; } = "";
    public bool RewardGranted { get; set; }
    public List<string> Log { get; set; } = new();
    // ── Extended combat state ────────────────────────────────────────────────
    public int EnemyStunned { get; set; }           // rounds remaining stunned
    public bool EnemyDisarmed { get; set; }
    public int PlayerDefenseBonus { get; set; }     // from deflect / guard actions
    public int BleedDamage { get; set; }            // DoT per round
    public int Round { get; set; }
}

public class NpcConversationSession
{
    public string Id { get; set; } = "";
    public string NpcName { get; set; } = "";
    public string NpcSpecies { get; set; } = "";
    public bool IsGroupConversation { get; set; }
    public List<string> ParticipantNames { get; set; } = new();
    public List<string> ParticipantSpecies { get; set; } = new();
    public string PlanetName { get; set; } = "";
    public string Zone { get; set; } = "market";
    public int Trust { get; set; }
    public int Turns { get; set; }
    public string Mood { get; set; } = "neutral";
    public List<string> Transcript { get; set; } = new();
}

public class NpcConversationTurn
{
    public string SessionId { get; set; } = "";
    public string NpcName { get; set; } = "";
    public string NpcSpecies { get; set; } = "";
    public string NpcLine { get; set; } = "";
    public string WorldContext { get; set; } = "";
    public string Mood { get; set; } = "neutral";
    public int Trust { get; set; }
    /// <summary>Set when the NPC agrees to offer a contract in response to a work-seeking keyword.</summary>
    public Quest? QuestOffer { get; set; }
    public string QuestOfferMessage { get; set; } = "";
}

public class MerchantListing
{
    public string ItemName { get; set; } = "";
    public int Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = "misc";
    public bool IsLocked { get; set; }
    public string LockReason { get; set; } = "";
}

public class JediAwakeningEvent
{
    public string MasterName { get; set; } = "";
    public string Dialogue { get; set; } = "";
    public string OfferText { get; set; } = "";
    public string TransportTo { get; set; } = "Coruscant";
}

public class RomanceEncounterEvent
{
    public string NpcName    { get; set; } = "";
    public string NpcSpecies { get; set; } = "";
    public string Planet     { get; set; } = "";
    public string Zone       { get; set; } = "";
    public string OpeningLine { get; set; } = "";
}

// ── Armor system ─────────────────────────────────────────────────────────────
public class ArmorBlueprint
{
    public string Name           { get; set; } = "";
    public string Category       { get; set; } = "light";  // light / medium / heavy / exotic
    public string Description    { get; set; } = "";
    public int ArmorRating       { get; set; }              // damage reduction per hit
    public int HpBonus           { get; set; }              // max HP boost when equipped
    public int StaminaBonus      { get; set; }              // max stamina boost
    public int MobilityPenalty   { get; set; }              // penalty to agility rolls
    public bool HeatResistance   { get; set; }
    public bool ColdResistance   { get; set; }
    public bool AcidResistance   { get; set; }
    public bool ToxinResistance  { get; set; }
    public bool VacuumSealed     { get; set; }              // survive in vacuum/space
    public bool RadiationShielded { get; set; }
    public bool WaterResistant   { get; set; }
    public bool StealthBonus     { get; set; }
    public bool LightsaberDampening { get; set; }           // halves lightsaber damage
    public bool LifeSupport      { get; set; }              // full environmental immunity
    public int BaseValue         { get; set; }
    public int ForagingBonus     { get; set; }              // bonus harvest rolls from foraging/gathering
    public string Slot           { get; set; } = "Full Suit"; // Helmet/Chest/Arms/Legs/Boots/Belt/Full Suit
    public string EraAvailable   { get; set; } = "All";
    public string FactionRequired { get; set; } = "";       // e.g. "Imperial" = faction-gated
    public bool Craftable        { get; set; } = true;
}

// ── Backpacks ────────────────────────────────────────────────────────────────────────
public class BackpackData
{
    public string Name              { get; set; } = "";
    public string Description       { get; set; } = "";
    public string MinChestType      { get; set; } = "light"; // light=any, medium=med+heavy, heavy=heavy only
    public long   CapacityMicroScu  { get; set; }            // μSCU added to inventory capacity
    public int    ArmorBonus        { get; set; }            // small AR bonus
    public int    BaseValue         { get; set; }
    public long   ItemSizeMicroScu  { get; set; } = 250_000L; // how much the pack itself takes up
}

// ── Housing ────────────────────────────────────────────────────────────────────────
public class PlayerHome
{
    public string Name                { get; set; } = "";
    public string Planet              { get; set; } = "";
    public long   StorageCapacityScu  { get; set; }          // in whole SCU
    public List<string> StorageItems  { get; set; } = new();
    public int    PurchasedFor        { get; set; }
}

public class HouseListing
{
    public string Name               { get; set; } = "";
    public string Planet             { get; set; } = "";
    public string Description        { get; set; } = "";
    public int    Price              { get; set; }
    public long   StorageCapacityScu { get; set; }
}

// ── Food & Cooking ────────────────────────────────────────────────────────────
public class FoodItem
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "prepared"; // raw / cooked / prepared / luxury
    public int HungerValue { get; set; } = 20;         // hunger restored on eating
    public int EnergyValue { get; set; } = 10;         // energy restored on eating
    public int HpBonus { get; set; }                   // HP healed on eating
    public bool RequiresCooking { get; set; }          // must be cooked before eating
    public int BuyPrice { get; set; }                  // 0 = not purchasable
    public string Description { get; set; } = "";
}

public class CookingRecipe
{
    public string Output { get; set; } = "";
    public string Description { get; set; } = "";
    public int TimeHours { get; set; } = 1;
    public List<RecipeComponent> Inputs { get; set; } = new();
}

// ── Resource Node Depletion ───────────────────────────────────────────────────
public class ResourceNodeState
{
    public string Key { get; set; } = "";
    public int TimesHarvested { get; set; }
    public int MaxHarvests { get; set; } = 5;
    public int DepletedAtRotation { get; set; } = -1;  // -1 = not depleted
    public int RespawnRotations { get; set; } = 5;

    public bool IsDepleted(int currentRotation)
        => DepletedAtRotation >= 0 && currentRotation < DepletedAtRotation + RespawnRotations;

    public int RotationsUntilRespawn(int currentRotation)
        => IsDepleted(currentRotation) ? (DepletedAtRotation + RespawnRotations - currentRotation) : 0;
}

// ── Harvesting Tools ──────────────────────────────────────────────────────────
public class HarvestingTool
{
    public string Name { get; set; } = "";
    public string ToolType { get; set; } = "mining";   // mining / woodcutting
    public int YieldBonus { get; set; }                // extra yield rolls per operation
    public int ChanceBonus { get; set; }               // bonus % added to each vein chance
    public bool RareMaterialBonus { get; set; }        // unlocks chance of rare material finds
    public int Tier { get; set; } = 1;                 // 1=basic → 4=legendary (used to pick best)
    public string Description { get; set; } = "";
    public int BuyPrice { get; set; }
}

// ── SCU unit-conversion helpers ─────────────────────────────────────────────────
public static class ScuConversion
{
    // 1 SCU = 100 cSCU = 1 000 mSCU = 1 000 000 μSCU
    public const long MicroPerScu   = 1_000_000L;
    public const long MicroPerMilli = 1_000L;
    public const long MicroPerCenti = 10_000L;   // 100 cSCU = 1 SCU → 1 cSCU = 10 000 μSCU

    public static string FormatMicroScu(long micro)
    {
        if (micro >= MicroPerScu)   return $"{micro / (double)MicroPerScu:F2} SCU";
        if (micro >= MicroPerCenti) return $"{micro / (double)MicroPerCenti:F1} cSCU";
        if (micro >= MicroPerMilli) return $"{micro / (double)MicroPerMilli:F1} mSCU";
        return $"{micro} μSCU";
    }
    public static string FormatScu(long scu) => $"{scu} SCU";
}

public class ArmorRecipe
{
    public string ArmorName       { get; set; } = "";
    public List<(string item, int qty)> Materials { get; set; } = new();
    public int CreditCost         { get; set; }
    public bool RequiresForge     { get; set; }
    public bool RequiresBlueprint { get; set; }
}

// ── Refining system ───────────────────────────────────────────────────────────
public class RefiningRecipe
{
    public string RawMaterial     { get; set; } = "";
    public string RefinedOutput   { get; set; } = "";
    public int Quantity           { get; set; } = 1;        // how many refined per 1 raw
    public string LocationRequired { get; set; } = "";      // "" = any forge; specific = location-locked
    public string FacilityRequired { get; set; } = "forge"; // forge / lab / gas_processor / spice_mill
    public int TimeCost           { get; set; } = 1;        // rotation cost
    public int CreditCost         { get; set; }
}

// ── Sub-zone travel ───────────────────────────────────────────────────────────
public class SubZoneDestination
{
    public string Name            { get; set; } = "";
    public string Planet          { get; set; } = "";
    public string ParentZone      { get; set; } = "";       // which zone this branches from
    public string Description     { get; set; } = "";
    public List<string> Actions   { get; set; } = new();
    public string ShipUpgradeRequired { get; set; } = "";   // e.g. "Gas Harvesting Laser"
    public string ShipModuleRequired  { get; set; } = "";   // e.g. "Gas Hold"
    public string FactionRequired { get; set; } = "";
    public string ThreatLevel     { get; set; } = "Moderate";
}

// ── Mining scan ───────────────────────────────────────────────────────────────
public class OreVeinNode
{
    public string OreType         { get; set; } = "";       // display name (always the refined/output form)
    public string RawMaterialKey  { get; set; } = "";       // inventory key added on mine
    public string NodeId          { get; set; } = "";       // stable per-vein depletion key
    public int ChancePercent      { get; set; }             // 0-100
    public bool IsRare            { get; set; }
    public int VeinSize           { get; set; } = 1;        // max yield per node
    public string Icon            { get; set; } = "◆";      // display char
}

// ── Wood types ────────────────────────────────────────────────────────────────
public class WoodTypeData
{
    public string Name            { get; set; } = "";
    public string Planet          { get; set; } = "";
    public string Description     { get; set; } = "";
    public int Hardness           { get; set; } = 5;        // 1-10
    public string RawMaterialKey  { get; set; } = "";       // inventory key when cut
    public int BaseValue          { get; set; }
}

// ── Mining vehicles ───────────────────────────────────────────────────────────
public class MiningVehicleBlueprint
{
    public string Name            { get; set; } = "";
    public string Description     { get; set; } = "";
    public int CargoSlotsRequired { get; set; } = 2;        // ship cargo slots to transport
    public int MiningBonus        { get; set; }             // extra yield rolls
    public bool HasScanner        { get; set; }
    public bool HasGasHarvester   { get; set; }
    public bool RequiresOperator  { get; set; } = true;
    public int BaseValue          { get; set; }
    public string Era             { get; set; } = "All";
}

// ── Planet location hierarchy ─────────────────────────────────────────────────
public class PlanetZoneLocation
{
    public string Region      { get; set; } = "";   // top tier: Wilderness, Urban, Industrial, Special
    public string District    { get; set; } = "";   // e.g. Mos Eisley, Dune Sea, Senate District
    public string Name        { get; set; } = "";   // e.g. Cantina, Sarlacc Pit
    public string Description { get; set; } = "";
    public string ThreatLevel { get; set; } = "Moderate"; // Low | Moderate | High | Extreme
    public string EncounterZone { get; set; } = "contact"; // maps to GenerateEncounter zone param
    public List<string> Actions { get; set; } = new();
    public bool HasMerchant   { get; set; }
    public bool IsMineZone    { get; set; }
    public bool IsHarvestZone { get; set; }
    public bool IsWoodZone    { get; set; }
}

// ── Quest encounter ───────────────────────────────────────────────────────────
public class QuestEncounterEvent
{
    public Quest Quest              { get; set; } = new();
    public string HintDialogue      { get; set; } = "";      // NPC hint about where/how
    public string ZoneHint          { get; set; } = "";      // suggested zone to visit
    public bool SpawnsCombat        { get; set; }
    public string EnemyFaction      { get; set; } = "";
}

public class PlanetEconomyStatus
{
    public string PlanetName { get; set; } = "";
    public int ResourceLevel { get; set; } = 100; // 0..100
    public bool ImperialExtraction { get; set; }
    public int TradeHealth { get; set; } = 60; // 0..100
    public string StatusText { get; set; } = "Stable";
}

public class GameSaveData
{
    public GameCharacter? Character { get; set; }
    public int Rotation { get; set; }
    public int Hour { get; set; }
    public List<Quest> Quests { get; set; } = new();
    public Dictionary<string, int> FactionStandings { get; set; } = new();
    public HashSet<string> DiscoveredPlanets { get; set; } = new();
    public Dictionary<string, int> PlanetRotations { get; set; } = new();
    public List<Dictionary<string, object>> ConstructionQueue { get; set; } = new();
    public List<ResourceNodeState> ResourceNodeStates { get; set; } = new();
}

public class GameEngine
{
    private readonly Random random = new();
    private readonly Dictionary<string, int> interspeciesRelations = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, (string Faction, int MinStanding, string Note)> factionUnlocks = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, NpcConversationSession> activeConversations = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> merchantRestockRotation = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, List<MerchantListing>>> merchantInventories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Queue<string> galaxyNews = new();
    private int creatureEncounterCursor;
    public class ConstructionProject
    {
        public GameCharacter Owner { get; set; } = null!;
        public ShipBlueprint Blueprint { get; set; } = null!;
        public int RemainingHours { get; set; }
    }
    public List<ConstructionProject> ConstructionQueue { get; } = new();

    public GameEngine()
    {
        foreach (var planet in Planets.Values)
        {
            planet.DayEvents ??= new List<string>();
            planet.NightEvents ??= new List<string>();
            PlanetClocks[planet.Name] = new WorldClock();
        }

        FactionStandings["Rebels"] = 0;
        FactionStandings["Empire"] = 0;
        FactionStandings["Smugglers"] = 0;
        FactionStandings["Guilds"] = 0;
        FactionStandings["Jedi"] = 0;
        FactionStandings["Mandalorians"] = 0;
        FactionStandings["Sith"] = 0;

        InitExtendedData();
    }

    private void InitExtendedData()
    {
        InitArmors();
        InitIndividualArmorPieces();
        InitRefiningRecipes();
        InitSubZones();
        InitMiningVehicles();
        InitWoodTypes();
        InitBackpacks();
        InitHouseListings();
        InitFoodData();
        InitHarvestingTools();
        InitWeaponAbilities();
        InitBlasterMods();
        InitFactionBlueprintPools();
        InitStorylineChains();
        InitPlanetLocations();
    }

    public Dictionary<string, RaceData> Races { get; } = new();
    public Dictionary<string, EraData> Eras { get; } = new();
    public Dictionary<string, PlanetData> Planets { get; } = new();
    /// <summary>Hierarchical planet location database: planet → list of zone locations.</summary>
    public Dictionary<string, List<PlanetZoneLocation>> PlanetLocations { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, SpaceStation> SpaceStations { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, ShipBlueprint> ShipCatalog { get; } = new();
    public Dictionary<string, ShipUpgradeDefinition> ShipUpgradeCatalog { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, ShipArmamentData> ShipArmaments { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, VehicleBlueprint> Vehicles { get; } = new();
    public Dictionary<string, WeaponBlueprint> Weapons { get; } = new();
    public Dictionary<string, CreatureData> Creatures { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, ItemBlueprint> CraftableItems { get; } = new();
    public Dictionary<string, CraftRecipe> Recipes { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, ArmorBlueprint> Armors { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, ArmorRecipe> ArmorRecipes { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, RefiningRecipe> RefiningRecipes { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, List<SubZoneDestination>> SubZones { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, MiningVehicleBlueprint> MiningVehicles { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, WoodTypeData> WoodTypes { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, BackpackData> Backpacks { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, HouseListing> HouseListings { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, FoodItem> FoodItems { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, CookingRecipe> CookingRecipes { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, HarvestingTool> HarvestingTools { get; } = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ResourceNodeState> resourceNodeStates = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, WeaponCombatAbility> WeaponAbilities { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, BlasterMod> BlasterMods { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, List<string>> PlanetRawMaterials { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, List<string>> PlanetCreatureTerritories { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, PlanetEconomyStatus> PlanetEconomyStates { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> RefiningMap { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> RawMaterials { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> RefinedMaterials { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, List<string>> CapitalShipPartRequirements { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, WorldClock> PlanetClocks { get; } = new();
    public WorldClock Clock { get; } = new();
    public List<Quest> ActiveQuests { get; } = new();
    public Dictionary<string, int> FactionStandings { get; } = new();

    // faction A hurts faction B when A gains standing
    private static readonly Dictionary<string, string[]> FactionRivalries = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Empire"]        = new[] { "Rebels", "Jedi", "Guilds" },
        ["Rebels"]        = new[] { "Empire", "Sith" },
        ["Jedi"]          = new[] { "Sith", "Empire" },
        ["Sith"]          = new[] { "Jedi", "Rebels" },
        ["Mandalorians"]  = new[] { "Empire" },
        ["Smugglers"]     = new[] { "Empire" },
        ["Guilds"]        = new[] { "Empire" },
    };

    // pre-built storyline chains keyed by chain id
    private Dictionary<string, QuestChain> StorylineChains { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> DiscoveredPlanets { get; } = new();
    public string SavePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savegame.json");

    public string GetPlanetFacilitySummary(string planetName)
    {
        if (!Planets.TryGetValue(planetName, out var planet)) return "Facilities unknown";

        var facilities = new List<string>();
        if (planet.HasDockyard) facilities.Add("Shipyard");
        if (planet.HasIndustrialFurnace) facilities.Add("Forge");

        var economy = (planet.Economy + " " + planet.Description).ToLowerInvariant();
        if (economy.Contains("speeder")) facilities.Add("Speeder Factory access");
        if (economy.Contains("walker") || economy.Contains("armor") || economy.Contains("armament")) facilities.Add("Walker Factory access");
        if (economy.Contains("crawler")) facilities.Add("Crawler Factory access");
        if (economy.Contains("transport")) facilities.Add("Transport Factory access");
        if (economy.Contains("refinery")) facilities.Add("Refinery");

        return facilities.Count == 0 ? "No major facilities" : string.Join(", ", facilities.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    public IReadOnlyList<string> GetStarterHomeworldOptions()
    {
        var curated = new[] { "Coruscant", "Corellia", "Alderaan", "Kuat", "Tatooine" }
            .Where(Planets.ContainsKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (curated.Count > 0)
        {
            return curated;
        }

        return Planets.Keys
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();
    }

    public string GetTravelRegionGroup(string planetName)
    {
        if (!Planets.TryGetValue(planetName, out var planet))
        {
            return "Unknown Space";
        }

        var searchableText = string.Join(" ", new[]
        {
            planetName,
            planet.Region,
            planet.Sector,
            planet.Description,
            planet.Economy
        }.Where(x => !string.IsNullOrWhiteSpace(x))).ToLowerInvariant();

        if (searchableText.Contains("chiss") || searchableText.Contains("ascendancy") || searchableText.Contains("csilla") || searchableText.Contains("rentor") || searchableText.Contains("primea"))
        {
            return "Chiss Ascendancy";
        }

        if (searchableText.Contains("core world")) return "Core Worlds";
        if (searchableText.Contains("colonies")) return "Colonies";
        if (searchableText.Contains("expansion region") || searchableText.Contains("expansion")) return "Expansion Region";
        if (searchableText.Contains("inner rim")) return "Inner Rim";
        if (searchableText.Contains("mid rim")) return "Mid Rim";
        if (searchableText.Contains("corporate sector")) return "Corporate Sector";
        if (searchableText.Contains("hutt")) return "Hutt Space";
        if (searchableText.Contains("outer rim")) return "Outer Rim";
        if (searchableText.Contains("wild space")) return "Wild Space";
        if (searchableText.Contains("unknown")) return "Unknown Regions";

        if (!string.IsNullOrWhiteSpace(planet.Sector) && !planet.Sector.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
        {
            return planet.Sector;
        }

        if (!string.IsNullOrWhiteSpace(planet.Region))
        {
            return planet.Region;
        }

        return "Frontier Space";
    }

    public IReadOnlyList<string> ConsumeGalaxyNews(int maxItems = 6)
    {
        var results = new List<string>();
        var take = Math.Max(1, maxItems);
        while (results.Count < take && galaxyNews.Count > 0)
        {
            results.Add(galaxyNews.Dequeue());
        }
        return results;
    }

    public GameEngine InitializeCatalogs()
    {
        Races["Human"] = new RaceData { Name = "Human", Description = "Adaptable and common across the galaxy.", EraUnlock = "Old Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 2, ["agility"] = 2, ["intellect"] = 2, ["presence"] = 2, ["vitality"] = 2 }, StartingSkills = new List<string> { "Negotiation" } };
        Races["Twi'lek"] = new RaceData { Name = "Twi'lek", Description = "Agile and expressive, common in the Outer Rim.", EraUnlock = "Old Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 1, ["agility"] = 3, ["intellect"] = 2, ["presence"] = 2, ["vitality"] = 2 }, StartingSkills = new List<string> { "Charm" } };
        Races["Miraluka"] = new RaceData { Name = "Miraluka", Description = "Force-sensitive and perceptive.", EraUnlock = "Old Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 1, ["agility"] = 2, ["intellect"] = 3, ["presence"] = 2, ["vitality"] = 2 }, StartingSkills = new List<string> { "Perception" } };
        Races["Droid"] = new RaceData { Name = "Droid", Description = "Machine intelligence with exceptional resilience.", EraUnlock = "Old Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 2, ["agility"] = 2, ["intellect"] = 3, ["presence"] = 1, ["vitality"] = 3 }, StartingSkills = new List<string> { "Computers" } };
        Races["Wookiee"] = new RaceData { Name = "Wookiee", Description = "Powerful and loyal, famous for their strength.", EraUnlock = "Old Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 4, ["agility"] = 2, ["intellect"] = 2, ["presence"] = 2, ["vitality"] = 3 }, StartingSkills = new List<string> { "Brawl" } };
        Races["Bothan"] = new RaceData { Name = "Bothan", Description = "Cunning and observant.", EraUnlock = "Old Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 2, ["agility"] = 2, ["intellect"] = 3, ["presence"] = 2, ["vitality"] = 2 }, StartingSkills = new List<string> { "Investigation" } };
        Races["Rodian"] = new RaceData { Name = "Rodian", Description = "Fast and sharp-eyed hunters.", EraUnlock = "Old Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 2, ["agility"] = 3, ["intellect"] = 2, ["presence"] = 2, ["vitality"] = 2 }, StartingSkills = new List<string> { "Tracking" } };
        Races["Zabrak"] = new RaceData { Name = "Zabrak", Description = "Fierce warriors with a deep connection to tradition.", EraUnlock = "Clone Wars", BaseStats = new Dictionary<string, int> { ["strength"] = 3, ["agility"] = 2, ["intellect"] = 2, ["presence"] = 2, ["vitality"] = 3 }, StartingSkills = new List<string> { "Brawl" } };
        Races["Chiss"] = new RaceData { Name = "Chiss", Description = "Strategic and disciplined, tied to the Unknown Regions.", EraUnlock = "Clone Wars", BaseStats = new Dictionary<string, int> { ["strength"] = 2, ["agility"] = 3, ["intellect"] = 3, ["presence"] = 2, ["vitality"] = 2 }, StartingSkills = new List<string> { "Astrogation" } };
        Races["Mon Calamari"] = new RaceData { Name = "Mon Calamari", Description = "Brilliant tacticians from the watery worlds of Mon Cala.", EraUnlock = "Clone Wars", BaseStats = new Dictionary<string, int> { ["strength"] = 1, ["agility"] = 2, ["intellect"] = 3, ["presence"] = 2, ["vitality"] = 2 }, StartingSkills = new List<string> { "Leadership" } };
        Races["Nautolan"] = new RaceData { Name = "Nautolan", Description = "Aquatic and resilient, tied to the Outer Rim.", EraUnlock = "Clone Wars", BaseStats = new Dictionary<string, int> { ["strength"] = 2, ["agility"] = 2, ["intellect"] = 2, ["presence"] = 2, ["vitality"] = 3 }, StartingSkills = new List<string> { "Survival" } };
        Races["Keldor"] = new RaceData { Name = "Kel Dor", Description = "Adept at survival and environmental adaptation.", EraUnlock = "Clone Wars", BaseStats = new Dictionary<string, int> { ["strength"] = 2, ["agility"] = 2, ["intellect"] = 3, ["presence"] = 2, ["vitality"] = 2 }, StartingSkills = new List<string> { "Medicine" } };
        Races["Mandalorian"] = new RaceData { Name = "Mandalorian", Description = "A warrior culture of armor, honor, and resolve.", EraUnlock = "Original Trilogy", RequiresMarriage = true, BaseStats = new Dictionary<string, int> { ["strength"] = 3, ["agility"] = 2, ["intellect"] = 2, ["presence"] = 3, ["vitality"] = 3 }, StartingSkills = new List<string> { "Leadership", "Brawl" } };
        Races["Trandoshan"] = new RaceData { Name = "Trandoshan", Description = "Savagely durable hunters from the Outer Rim.", EraUnlock = "Original Trilogy", BaseStats = new Dictionary<string, int> { ["strength"] = 3, ["agility"] = 2, ["intellect"] = 2, ["presence"] = 1, ["vitality"] = 3 }, StartingSkills = new List<string> { "Survival" } };
        Races["Cerean"] = new RaceData { Name = "Cerean", Description = "A calm and analytical species of the Core Worlds.", EraUnlock = "New Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 1, ["agility"] = 2, ["intellect"] = 3, ["presence"] = 2, ["vitality"] = 2 }, StartingSkills = new List<string> { "Computers" } };
        Races["Toydarian"] = new RaceData { Name = "Toydarian", Description = "Small but clever traders of the Outer Rim.", EraUnlock = "New Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 1, ["agility"] = 2, ["intellect"] = 3, ["presence"] = 2, ["vitality"] = 2 }, StartingSkills = new List<string> { "Negotiation" } };
        Races["Sith"] = new RaceData { Name = "Sith", Description = "Dark-side warriors from a fallen order.", EraUnlock = "Old Republic", RequiresMarriage = true, BaseStats = new Dictionary<string, int> { ["strength"] = 3, ["agility"] = 2, ["intellect"] = 3, ["presence"] = 3, ["vitality"] = 2 }, StartingSkills = new List<string> { "Force Discipline", "Lightsaber" } };
        // Additional humanoid races commonly referenced in lore
        Races["Togruta"] = new RaceData { Name = "Togruta", Description = "A head-tented species with strong community bonds.", EraUnlock = "Old Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 2, ["agility"] = 3, ["intellect"] = 2, ["presence"] = 3, ["vitality"] = 2 }, StartingSkills = new List<string> { "Stealth", "Perception" } };
        Races["Sullustan"] = new RaceData { Name = "Sullustan", Description = "Skilled pilots and navigators from the Outer Rim.", EraUnlock = "Old Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 1, ["agility"] = 3, ["intellect"] = 3, ["presence"] = 2, ["vitality"] = 2 }, StartingSkills = new List<string> { "Piloting" } };
        Races["Mirialan"] = new RaceData { Name = "Mirialan", Description = "Spiritual and disciplined people, often Force-sensitive.", EraUnlock = "Old Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 1, ["agility"] = 2, ["intellect"] = 3, ["presence"] = 3, ["vitality"] = 2 }, StartingSkills = new List<string> { "Meditation" } };
        Races["Kiffar"] = new RaceData { Name = "Kiffar", Description = "Cunning trackers with a warrior tradition.", EraUnlock = "Old Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 2, ["agility"] = 3, ["intellect"] = 2, ["presence"] = 2, ["vitality"] = 2 }, StartingSkills = new List<string> { "Tracking" } };
        Races["Chagrian"] = new RaceData { Name = "Chagrian", Description = "Diplomatic and imposing, from watery worlds.", EraUnlock = "Clone Wars", BaseStats = new Dictionary<string, int> { ["strength"] = 2, ["agility"] = 1, ["intellect"] = 3, ["presence"] = 3, ["vitality"] = 2 }, StartingSkills = new List<string> { "Diplomacy" } };
        Races["Mirialan"] = Races.GetValueOrDefault("Mirialan") ?? new RaceData { Name = "Mirialan", Description = "Spiritual and disciplined people, often Force-sensitive.", EraUnlock = "Old Republic", BaseStats = new Dictionary<string, int> { ["strength"] = 1, ["agility"] = 2, ["intellect"] = 3, ["presence"] = 3, ["vitality"] = 2 }, StartingSkills = new List<string> { "Meditation" } };

        Eras["Old Republic"] = new EraData { Name = "Old Republic", StartRotation = 1, Description = "The age of Jedi Order, republic worlds, and early trade routes." };
        Eras["Prequel Era"] = new EraData { Name = "Prequel Era", StartRotation = 120, Description = "The rise of the Republic, trade disputes, and the Clone Wars." };
        Eras["Clone Wars"] = new EraData { Name = "Clone Wars", StartRotation = 220, Description = "The galaxy is consumed by war and shifting allegiances." };
        Eras["Original Trilogy"] = new EraData { Name = "Original Trilogy", StartRotation = 340, Description = "The Empire rises, rebels fight, and heroes endure." };
        Eras["New Republic"] = new EraData { Name = "New Republic", StartRotation = 460, Description = "Hope returns as the galaxy rebuilds after Imperial collapse." };
        Eras["Sequel Trilogy"] = new EraData { Name = "Sequel Trilogy", StartRotation = 560, Description = "The galaxy faces new threats as old powers fade." };

        Planets["Alderaan"] = new PlanetData { Name = "Alderaan", Description = "A refined world of noble houses and culture.", Economy = "Diplomacy, medicine, and luxury goods", Region = "Core Worlds", Era = "Old Republic", ThreatLevel = "Low", DayEvents = new List<string> { "gallery opening", "trade summit", "scholar debate" }, NightEvents = new List<string> { "moonlit promenade", "hidden meeting" }, TravelCost = 12 };
        Planets["Corellia"] = new PlanetData { Name = "Corellia", Description = "An industrial world of shipyards, gangs, and desperate districts.", Economy = "Shipyards, trade, and cargo", Region = "Core Worlds", Sector = "Core Worlds", Era = "Old Republic", ThreatLevel = "High", DayEvents = new List<string> { "dock work", "trade", "shipwright contract" }, NightEvents = new List<string> { "slum raid", "street duel", "smuggling" }, TravelCost = 18, HasDockyard = true };
        Planets["Coruscant"] = new PlanetData { Name = "Coruscant", Description = "The shining ecumenopolis at the heart of the galaxy.", Economy = "Finance, government, and trade", Region = "Core Worlds", Sector = "Core Worlds", Era = "Old Republic", ThreatLevel = "Moderate", DayEvents = new List<string> { "senate rally", "market deal", "parade" }, NightEvents = new List<string> { "night market", "underworld rumor" }, TravelCost = 16, HasDockyard = true };
        Planets["Tatooine"] = new PlanetData { Name = "Tatooine", Description = "A desert world of moisture farms, smugglers, and danger.", Economy = "Spice, salvage, and podracing", Region = "Outer Rim", Era = "Old Republic", ThreatLevel = "High", DayEvents = new List<string> { "podrace", "trade", "scavenger hunt" }, NightEvents = new List<string> { "street chase", "hideout rumor", "dust storm" }, TravelCost = 14 };
        Planets["Naboo"] = new PlanetData { Name = "Naboo", Description = "A peaceful and elegant world of lakes, palaces, and diplomacy.", Economy = "Agriculture, diplomacy, and luxury goods", Region = "Mid Rim", Era = "Old Republic", ThreatLevel = "Low", DayEvents = new List<string> { "festival", "trade", "courtly intrigue" }, NightEvents = new List<string> { "quiet patrol", "theater performance" }, TravelCost = 13 };
        Planets["Mandalore"] = new PlanetData { Name = "Mandalore", Description = "A warlike world of clans, armor, and contracts.", Economy = "Armaments and mercenary work", Region = "Outer Rim", Era = "Clone Wars", ThreatLevel = "High", DayEvents = new List<string> { "forge work", "clan challenge", "contract" }, NightEvents = new List<string> { "raid", "sentry ambush" }, TravelCost = 22 };
        Planets["Kashyyyk"] = new PlanetData { Name = "Kashyyyk", Description = "A towering forest world of Wookiee cities and ancient trees.", Economy = "Timber, ship components, and hunting", Region = "Mid Rim", Era = "Clone Wars", ThreatLevel = "High", DayEvents = new List<string> { "tree-city trade", "hunting party" }, NightEvents = new List<string> { "night raid", "shadow hunt" }, TravelCost = 20 };
        Planets["Kamino"] = new PlanetData { Name = "Kamino", Description = "An ocean planet of clones, laboratories, and serene waters.", Economy = "Cloning and biotech", Region = "Outer Rim", Era = "Clone Wars", ThreatLevel = "Moderate", DayEvents = new List<string> { "research tour", "waterway delivery" }, NightEvents = new List<string> { "storm coverage", "deep-sea salvage" }, TravelCost = 21 };
        Planets["Geonosis"] = new PlanetData { Name = "Geonosis", Description = "A rocky desert world of droid foundries and arena culture.", Economy = "Droids and spectacle", Region = "Outer Rim", Era = "Clone Wars", ThreatLevel = "High", DayEvents = new List<string> { "arena match", "factory inspection" }, NightEvents = new List<string> { "execution pit", "underground revolt" }, TravelCost = 24 };
        Planets["Mustafar"] = new PlanetData { Name = "Mustafar", Description = "A volcanic world of black rock and heavy industry.", Economy = "Mining and refinery", Region = "Outer Rim", Era = "Clone Wars", ThreatLevel = "High", DayEvents = new List<string> { "lava refinery", "merchant caravan" }, NightEvents = new List<string> { "ash storm", "forge raid" }, TravelCost = 25 };
        Planets["Endor"] = new PlanetData { Name = "Endor", Description = "A forest moon alive with towering trees and hidden bases.", Economy = "Timber and ecology", Region = "Outer Rim", Era = "Original Trilogy", ThreatLevel = "Moderate", DayEvents = new List<string> { "forest hunt", "tribal market" }, NightEvents = new List<string> { "night patrol", "wild beast encounter" }, TravelCost = 19 };
        Planets["Hoth"] = new PlanetData { Name = "Hoth", Description = "A frozen world of blizzards and rebel bases.", Economy = "Ice, research, and defense", Region = "Outer Rim", Era = "Original Trilogy", ThreatLevel = "High", DayEvents = new List<string> { "blizzard patrol", "supply run" }, NightEvents = new List<string> { "ice cave ambush", "aurora watch" }, TravelCost = 26 };
        Planets["Bespin"] = new PlanetData { Name = "Bespin", Description = "A gas giant world with floating cities and high-altitude commerce.", Economy = "Mining and luxury trade", Region = "Outer Rim", Era = "Original Trilogy", ThreatLevel = "Moderate", DayEvents = new List<string> { "cloud city market", "tethered transport" }, NightEvents = new List<string> { "storm chase", "smuggler meetup" }, TravelCost = 20 };
        Planets["Dagobah"] = new PlanetData { Name = "Dagobah", Description = "A murky swamp world steeped in the Force.", Economy = "Rare herbs and relic salvage", Region = "Outer Rim", Era = "Original Trilogy", ThreatLevel = "High", DayEvents = new List<string> { "mire search", "sacred meditation" }, NightEvents = new List<string> { "swamp ambush", "ghostly whisper" }, TravelCost = 24 };
        Planets["Lothal"] = new PlanetData { Name = "Lothal", Description = "A rugged world of grass plains, ancient temples, and hidden rebels.", Economy = "Agriculture and mining", Region = "Outer Rim", Era = "Original Trilogy", ThreatLevel = "Moderate", DayEvents = new List<string> { "market road", "farm exchange" }, NightEvents = new List<string> { "temple watch", "rebel relay" }, TravelCost = 18 };
        Planets["Dantooine"] = new PlanetData { Name = "Dantooine", Description = "A calm world of open plains and old Jedi ruins.", Economy = "Farming and archaeology", Region = "Outer Rim", Era = "New Republic", ThreatLevel = "Low", DayEvents = new List<string> { "ruin survey", "trade caravan" }, NightEvents = new List<string> { "stargazer camp", "wildlife crossing" }, TravelCost = 15 };
        Planets["Utapau"] = new PlanetData { Name = "Utapau", Description = "A rocky world of sinkholes and giant beasts.", Economy = "Energy and livestock", Region = "Outer Rim", Era = "Original Trilogy", ThreatLevel = "Moderate", DayEvents = new List<string> { "city market", "terrain survey" }, NightEvents = new List<string> { "sinkhole patrol", "night predator" }, TravelCost = 21 };
        Planets["Ryloth"] = new PlanetData { Name = "Ryloth", Description = "A harsh desert world of slave markets and resistance.", Economy = "Mining and spice", Region = "Outer Rim", Era = "Clone Wars", ThreatLevel = "High", DayEvents = new List<string> { "market exchange", "resistance aide" }, NightEvents = new List<string> { "raid alarm", "shuttle chase" }, TravelCost = 23 };
        Planets["Jakku"] = new PlanetData { Name = "Jakku", Description = "A wreck-strewn desert world of scavengers and relic hunters.", Economy = "Salvage and relics", Region = "Outer Rim", Era = "Sequel Trilogy", ThreatLevel = "High", DayEvents = new List<string> { "junk market", "ship salvage" }, NightEvents = new List<string> { "sandstorm chase", "ruin survey" }, TravelCost = 17 };
        Planets["Ahch-To"] = new PlanetData { Name = "Ahch-To", Description = "A remote island world of ancient Jedi ruins and deep silence.", Economy = "Pilgrimage and relic study", Region = "Unknown Regions", Era = "Sequel Trilogy", ThreatLevel = "Low", DayEvents = new List<string> { "pilgrimage", "shore gathering" }, NightEvents = new List<string> { "misty watch", "sea cave descent" }, TravelCost = 28 };
        Planets["Exegol"] = new PlanetData { Name = "Exegol", Description = "A hidden world of ancient Sith power and ruin.", Economy = "Dark-side relics and shadow trade", Region = "Unknown Regions", Era = "Sequel Trilogy", ThreatLevel = "Very High", DayEvents = new List<string> { "ritual site", "sith shrine" }, NightEvents = new List<string> { "night raid", "death cult gathering" }, TravelCost = 31 };

        // Additional planets and sectors
        Planets["Kuat"] = new PlanetData { Name = "Kuat", Description = "A major shipbuilding world and naval yard, home of Kuat Drive Yards.", Economy = "Shipyards and manufacturing", Region = "Core Worlds", Sector = "Core Worlds", Era = "Old Republic", ThreatLevel = "Moderate", DayEvents = new List<string> { "ship launch", "yard tour" }, NightEvents = new List<string> { "overwatch patrol" }, TravelCost = 20, HasDockyard = true, ShipyardManufacturer = "Kuat Drive Yards", ShipyardName = "KDY Surface Yards", OrbitalStations = new List<string> { "Kuat Drive Yards Orbital Ring" } };
        Planets["Corellia"] = new PlanetData { Name = "Corellia", Description = "An industrial world of shipyards, gangs, and desperate districts.", Economy = "Shipyards, trade, and cargo", Region = "Core Worlds", Sector = "Core Worlds", Era = "Old Republic", ThreatLevel = "High", DayEvents = new List<string> { "dock work", "trade", "shipwright contract" }, NightEvents = new List<string> { "slum raid", "street duel", "smuggling" }, TravelCost = 18, HasDockyard = true, ShipyardManufacturer = "Corellian Engineering Corporation", ShipyardName = "CEC Orbital Shipyards", OrbitalStations = new List<string> { "CEC Orbital Shipyards" } };
        Planets["Mon Cala"] = new PlanetData { Name = "Mon Cala", Description = "A watery planet famed for ship construction and naval power.", Economy = "Shipbuilding and fisheries", Region = "Mid Rim", Sector = "Mid Rim", Era = "Clone Wars", ThreatLevel = "Moderate", DayEvents = new List<string> { "dock trade", "ship fitting" }, NightEvents = new List<string> { "lantern festival" }, TravelCost = 20, HasDockyard = true, ShipyardManufacturer = "Mon Calamari Shipyards", ShipyardName = "MC Deepwater Yards", OrbitalStations = new List<string> { "Mon Calamari Orbital Platform" } };
        Planets["Nar Shaddaa"] = new PlanetData { Name = "Nar Shaddaa", Description = "A vertical smuggler's paradise with endless levels.", Economy = "Smuggling and entertainment", Region = "Corporate Sector", Sector = "Corporate Sector", Era = "Original Trilogy", ThreatLevel = "High", DayEvents = new List<string> { "black market", "docking bustle" }, NightEvents = new List<string> { "gang treaty", "smuggler meetup" }, TravelCost = 18, HasDockyard = true };
        Planets["Kessel"] = new PlanetData { Name = "Kessel", Description = "A mining world of spice refineries and dangerous routes.", Economy = "Spice mining", Region = "Outer Rim", Sector = "Outer Rim", Era = "Old Republic", ThreatLevel = "High", DayEvents = new List<string> { "mine run", "slave labor" }, NightEvents = new List<string> { "escape attempt" }, TravelCost = 24, HasDockyard = true };
        Planets["Kashyyyk"] = Planets.GetValueOrDefault("Kashyyyk") ?? new PlanetData { Name = "Kashyyyk", Sector = "Mid Rim", HasDockyard = true };

        ShipCatalog["Skiff"]                  = new ShipBlueprint { Name = "Skiff",                  Model = "Skiff",                  Era = "Old Republic",     Cost =  70, PurchasePrice =   4_200, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "thrusters" },                                                                         Ascii = new List<string> { " /|\\", " /_|_\\", "|___|" },         Hull =  8, Shield =  2, Fuel =  40, HyperdriveClass = 3, CrewCapacity =  2, Weapon = "Light blaster",       Description = "A cheap hauler for short-range operations." };
        ShipCatalog["YT-1300"]                = new ShipBlueprint { Name = "YT-1300",                Model = "YT-1300",                Era = "Old Republic",     Cost = 220, PurchasePrice = 115_000, SizeClass = "M", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "reactor", "navicomputer" },                                   Ascii = new List<string> { " /\\", " /  \\", "/____\\", "|__||__|" }, Hull = 24, Shield = 12, Fuel = 110, HyperdriveClass = 2, CrewCapacity =  6, Weapon = "Laser cannon",       Description = "A rugged freighter that can survive deep space runs." };
        ShipCatalog["X-Wing"]                 = new ShipBlueprint { Name = "X-Wing",                 Model = "X-Wing",                 Era = "Original Trilogy", Cost = 180, PurchasePrice =  44_000, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "thrusters", "sensors" },                                     Ascii = new List<string> { " /\\", " /  \\", "|  |", "|_|_" },    Hull = 18, Shield = 10, Fuel =  90, HyperdriveClass = 2, CrewCapacity =  2, Weapon = "Proton torpedo",      Description = "A nimble starfighter used by the Rebel Alliance." };
        ShipCatalog["Millennium Falcon"]      = new ShipBlueprint { Name = "Millennium Falcon",      Model = "Millennium Falcon",      Era = "Original Trilogy", Cost = 360, PurchasePrice = 850_000, SizeClass = "M", ShipyardTier = "orbital",   RequiredParts = new List<string> { "hyperdrive", "reactor", "navicomputer", "shield generator" },               Ascii = new List<string> { " /\\", "/  \\", "/____\\", "|__||__|" }, Hull = 34, Shield = 16, Fuel = 140, HyperdriveClass = 1, CrewCapacity =  8, Weapon = "Quad laser",          Description = "A legendary freighter with exceptional speed and durability." };
        ShipCatalog["Imperial Star Destroyer"]= new ShipBlueprint { Name = "Imperial Star Destroyer",Model = "Imperial Star Destroyer",Era = "Original Trilogy", Cost = 800, PurchasePrice = 38_000_000, SizeClass = "C", ShipyardTier = "orbital", RequiredParts = new List<string> { "hyperdrive", "reactor", "navicomputer", "shield generator", "turbolaser" }, Ascii = new List<string> { "[====|====]", "|__|_|__|" },         Hull = 60, Shield = 30, Fuel = 220, HyperdriveClass = 2, CrewCapacity = 24, Weapon = "Turbolaser battery", Description = "A massive capital ship for fleet-scale operations.", IsCapital = true };
        ShipCatalog["A-Wing"]                 = new ShipBlueprint { Name = "A-Wing",                 Model = "A-Wing",                 Era = "Original Trilogy", Cost = 240, PurchasePrice =  28_500, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "thrusters", "sensors" },                                     Ascii = new List<string> { " /\\", "  \\//", " /__\\" },           Hull = 14, Shield =  9, Fuel =  85, HyperdriveClass = 2, CrewCapacity =  1, Weapon = "Axe head blaster",   Description = "A fast interceptor built for dogfights." };
        ShipCatalog["B-Wing"]                 = new ShipBlueprint { Name = "B-Wing",                 Model = "B-Wing",                 Era = "Original Trilogy", Cost = 320, PurchasePrice =  86_000, SizeClass = "M", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "reactor", "sensors" },                                       Ascii = new List<string> { "[||]", "[__]" },                        Hull = 24, Shield = 14, Fuel = 100, HyperdriveClass = 2, CrewCapacity =  2, Weapon = "Ion cannon",          Description = "A heavily armed bomber used in fleet engagements." };
        ShipCatalog["Venator"]                = new ShipBlueprint { Name = "Venator",                Model = "Venator",                Era = "Clone Wars",       Cost = 650, PurchasePrice = 14_800_000, SizeClass = "C", ShipyardTier = "orbital", RequiredParts = new List<string> { "hyperdrive", "reactor", "navicomputer", "shield generator", "turbolaser" }, Ascii = new List<string> { "|=|", "|_|" },                          Hull = 46, Shield = 22, Fuel = 200, HyperdriveClass = 2, CrewCapacity = 20, Weapon = "Turbo laser",        Description = "A mighty Republic capital ship of the Clone Wars.", IsCapital = true };
        ShipCatalog["Naboo Royal Starship"]   = new ShipBlueprint { Name = "Naboo Royal Starship",   Model = "Naboo Royal Starship",   Era = "Prequel Era",      Cost = 300, PurchasePrice = 290_000, SizeClass = "M", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "navicomputer", "shield generator" },                         Ascii = new List<string> { "[[ ]]", "|__|" },                       Hull = 20, Shield = 11, Fuel = 110, HyperdriveClass = 2, CrewCapacity =  5, Weapon = "Twin laser",          Description = "A glamorous luxury vessel from Naboo." };
        ShipCatalog["Jedi Starfighter"]       = new ShipBlueprint { Name = "Jedi Starfighter",       Model = "Jedi Starfighter",       Era = "Prequel Era",      Cost = 260, PurchasePrice =  16_500, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "thrusters", "sensors" },                                     Ascii = new List<string> { " /\\", "_||_" },                        Hull = 16, Shield = 10, Fuel =  95, HyperdriveClass = 2, CrewCapacity =  1, Weapon = "Light blaster",      Description = "A sleek starfighter crafted for Jedi Knights." };
        ShipCatalog["TIE Fighter"]            = new ShipBlueprint { Name = "TIE Fighter",            Model = "TIE Fighter",            Era = "Original Trilogy", Cost = 140, PurchasePrice =   8_500, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "thrusters" },                                                Ascii = new List<string> { " /\\", "|_|" },                         Hull = 12, Shield =  6, Fuel =  70, HyperdriveClass = 3, CrewCapacity =  1, Weapon = "Twin laser",          Description = "A standard Imperial interceptor." };
        ShipCatalog["Slave I"]                = new ShipBlueprint { Name = "Slave I",                Model = "Slave I",                Era = "Original Trilogy", Cost = 280, PurchasePrice =  62_000, SizeClass = "M", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "reactor", "shield generator" },                              Ascii = new List<string> { "<|>", "|_|" },                          Hull = 16, Shield =  9, Fuel = 100, HyperdriveClass = 2, CrewCapacity =  3, Weapon = "Concussion missile", Description = "A feared bounty hunter ship." };
        ShipCatalog["Upsilon-class shuttle"]  = new ShipBlueprint { Name = "Upsilon-class shuttle",  Model = "Upsilon-class shuttle",  Era = "Clone Wars",       Cost = 240, PurchasePrice =  46_500, SizeClass = "M", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "navicomputer" },                                            Ascii = new List<string> { "[=]", "|_|" },                          Hull = 15, Shield =  8, Fuel =  90, HyperdriveClass = 2, CrewCapacity =  6, Weapon = "Blaster turret",     Description = "A flexible military shuttle." };
        ShipCatalog["Razor Crest"]            = new ShipBlueprint { Name = "Razor Crest",            Model = "Razor Crest",            Era = "Sequel Trilogy",   Cost = 260, PurchasePrice =  71_000, SizeClass = "M", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "reactor" },                                                 Ascii = new List<string> { "[--]", "|_|" },                         Hull = 18, Shield = 10, Fuel =  96, HyperdriveClass = 2, CrewCapacity =  4, Weapon = "Blaster cannon",     Description = "A rugged independent ship from the New Republic era." };
        ShipCatalog["First Order TIE Fighter"]= new ShipBlueprint { Name = "First Order TIE Fighter",Model = "First Order TIE Fighter",Era = "Sequel Trilogy",   Cost = 200, PurchasePrice =  11_500, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "thrusters" },                                                Ascii = new List<string> { " /\\", "|_|" },                         Hull = 14, Shield =  7, Fuel =  80, HyperdriveClass = 3, CrewCapacity =  1, Weapon = "Plasma cannon",      Description = "A modernized Imperial starfighter." };

        Vehicles["Landspeeder"] = new VehicleBlueprint { Name = "Landspeeder", Type = "speeder", Era = "Old Republic", Ascii = new List<string> { " __/\\__", " /  o  \\", "/__/|\\__\\" } };
        Vehicles["Speeder Bike"] = new VehicleBlueprint { Name = "Speeder Bike", Type = "speeder", Era = "Original Trilogy", Ascii = new List<string> { " /\\/\\", "(  o )", " \\_\\/" } };
        Vehicles["AT-AT"] = new VehicleBlueprint { Name = "AT-AT", Type = "walker", Era = "Original Trilogy", Ascii = new List<string> { " /|\\", "/_|_\\", "|_|_|" } };
        Vehicles["AT-ST"] = new VehicleBlueprint { Name = "AT-ST", Type = "walker", Era = "Original Trilogy", Ascii = new List<string> { " /|\\", "|_|_|" } };
        Vehicles["MSE-6"] = new VehicleBlueprint { Name = "MSE-6", Type = "crawler", Era = "Clone Wars", Ascii = new List<string> { "[=]", "|_|" } };
        Vehicles["Sith Speeder"] = new VehicleBlueprint { Name = "Sith Speeder", Type = "speeder", Era = "Old Republic", Ascii = new List<string> { " /\\", "|_|" } };
        Vehicles["Swoop Bike"] = new VehicleBlueprint { Name = "Swoop Bike", Type = "swoop", Era = "Old Republic", Ascii = new List<string> { " /\\", "(o)" } };
        Vehicles["T-16 Skyhopper"] = Vehicles.GetValueOrDefault("T-16 Skyhopper") ?? new VehicleBlueprint { Name = "T-16 Skyhopper", Type = "speeder", Era = "Old Republic", Ascii = new List<string> { " /\\", "|_|" } };
        Vehicles["YT-2400"] = new VehicleBlueprint { Name = "YT-2400", Type = "freighter", Era = "Original Trilogy", Ascii = new List<string> { "[~]", "|_|" } };
        Vehicles["Sail Barge"] = new VehicleBlueprint { Name = "Sail Barge", Type = "barge", Era = "Original Trilogy", Ascii = new List<string> { "[===]", "|__|" } };
        Vehicles["Hammerhead Corvette"] = new VehicleBlueprint { Name = "Hammerhead Corvette", Type = "corvette", Era = "Original Trilogy", Ascii = new List<string> { "[=H=]", "|__|" } };

        Weapons["Blaster Pistol"]    = new WeaponBlueprint { Name = "Blaster Pistol",    Category = "blaster",   WeaponSubtype = "pistol",         IsOneHanded = true,  Era = "Old Republic",     Damage = 7,  Description = "A common sidearm with dependable stopping power." };
        Weapons["Heavy Blaster"]      = new WeaponBlueprint { Name = "Heavy Blaster",      Category = "blaster",   WeaponSubtype = "heavy_blaster",  IsOneHanded = false, Era = "Clone Wars",       Damage = 11, Description = "A robust weapon for infantry and rough encounters." };
        Weapons["Lightsaber"]         = new WeaponBlueprint { Name = "Lightsaber",          Category = "melee",     WeaponSubtype = "lightsaber",     IsOneHanded = true,  Era = "Old Republic",     Damage = 15, Description = "A legendary weapon of the Force." };
        Weapons["Shoto Lightsaber"]   = new WeaponBlueprint { Name = "Shoto Lightsaber",   Category = "melee",     WeaponSubtype = "shoto_lightsaber",IsOneHanded = true, Era = "Old Republic",     Damage = 10, Description = "A short-bladed training saber. Perfect as an off-hand blade." };
        Weapons["Ion Rifle"]          = new WeaponBlueprint { Name = "Ion Rifle",            Category = "energy",    WeaponSubtype = "blaster_rifle",  IsOneHanded = false, Era = "Original Trilogy", Damage = 13, Description = "A strong energy weapon that can disable shields." };
        Weapons["Thermal Detonator"]  = new WeaponBlueprint { Name = "Thermal Detonator",  Category = "explosive", WeaponSubtype = "explosive",      IsOneHanded = true,  Era = "Original Trilogy", Damage = 18, Description = "A dangerous explosive for close quarters." };
        Weapons["Bowcaster"]          = new WeaponBlueprint { Name = "Bowcaster",            Category = "projectile",WeaponSubtype = "bowcaster",      IsOneHanded = false, Era = "Old Republic",     Damage = 10, Description = "The favored ranged weapon of the Wookiees." };
        Weapons["Electrostaff"]       = new WeaponBlueprint { Name = "Electrostaff",         Category = "melee",     WeaponSubtype = "melee",          IsOneHanded = false, Era = "Clone Wars",       Damage = 12, Description = "A brutal close-combat weapon." };
        Weapons["Sniper Rifle"]       = new WeaponBlueprint { Name = "Sniper Rifle",         Category = "blaster",   WeaponSubtype = "sniper_rifle",   IsOneHanded = false, Era = "Old Republic",     Damage = 16, Description = "A long-range precision rifle. High damage, requires a steady hand." };
        Weapons["NT-242 Sniper Rifle"]= new WeaponBlueprint { Name = "NT-242 Sniper Rifle", Category = "blaster",   WeaponSubtype = "sniper_rifle",   IsOneHanded = false, Era = "Original Trilogy", Damage = 18, Description = "A Merr-Sonn precision rifle favored by bounty hunters." };
        Weapons["Ion Blaster"]        = new WeaponBlueprint { Name = "Ion Blaster",           Category = "energy",    WeaponSubtype = "ion",            IsOneHanded = true,  Era = "Clone Wars",       Damage = 9,  Description = "A handheld ion weapon that devastates droids and shields." };
        Weapons["Disruptor Rifle"]    = new WeaponBlueprint { Name = "Disruptor Rifle",      Category = "energy",    WeaponSubtype = "disruptor_rifle",IsOneHanded = false, Era = "Old Republic",     Damage = 17, Description = "A terrifying weapon that disintegrates matter. Illegal in most systems." };

        CraftableItems["repair kit"] = new ItemBlueprint { Name = "repair kit", Category = "gear", Description = "Restores ship hull and shields", Skill = "Engineering", Cost = 30 };
        CraftableItems["shield booster"] = new ItemBlueprint { Name = "shield booster", Category = "ship", Description = "Improves shield strength", Skill = "Engineering", Cost = 40 };
        CraftableItems["hyperdrive part"] = new ItemBlueprint { Name = "hyperdrive part", Category = "ship", Description = "Improves travel efficiency", Skill = "Engineering", Cost = 45 };
        CraftableItems["laser upgrade"] = new ItemBlueprint { Name = "laser upgrade", Category = "ship", Description = "Upgrades the ship weapon", Skill = "Engineering", Cost = 55 };
        CraftableItems["scrap armor"] = new ItemBlueprint { Name = "scrap armor", Category = "armor", Description = "Basic personal protection", Skill = "Crafting", Cost = 25 };
        CraftableItems["field medpack"] = new ItemBlueprint { Name = "field medpack", Category = "medicine", Description = "Heals the wearer", Skill = "Medicine", Cost = 24 };
        CraftableItems["fuel cell"] = new ItemBlueprint { Name = "fuel cell", Category = "ship", Description = "Adds fuel", Skill = "Engineering", Cost = 35 };
        CraftableItems["sensor array"] = new ItemBlueprint { Name = "sensor array", Category = "ship", Description = "Improves planet scanning", Skill = "Engineering", Cost = 50 };
        CraftableItems["smuggler cache"] = new ItemBlueprint { Name = "smuggler cache", Category = "trade", Description = "A hidden haul of goods", Skill = "Slicing", Cost = 60 };
        CraftableItems["lightsaber crystal"] = new ItemBlueprint { Name = "lightsaber crystal", Category = "weapon", Description = "A rare Force-attuned component", Skill = "Force Discipline", Cost = 80 };
        CraftableItems["hyperdrive stabilizer"] = new ItemBlueprint { Name = "hyperdrive stabilizer", Category = "ship", Description = "Improves jump reliability", Skill = "Engineering", Cost = 55 };
        CraftableItems["armor plating"] = new ItemBlueprint { Name = "armor plating", Category = "armor", Description = "Adds hull and protection to gear", Skill = "Crafting", Cost = 45 };
        CraftableItems["ion capacitor"] = new ItemBlueprint { Name = "ion capacitor", Category = "ship", Description = "Boosts ship systems", Skill = "Engineering", Cost = 48 };
        CraftableItems["power cell"] = new ItemBlueprint { Name = "power cell", Category = "ship", Description = "Provides extra energy for equipment", Skill = "Engineering", Cost = 36 };
        CraftableItems["stasis field"] = new ItemBlueprint { Name = "stasis field", Category = "gear", Description = "Temporarily immobilizes nearby foes", Skill = "Slicing", Cost = 70 };
        CraftableItems["healing stim"] = new ItemBlueprint { Name = "healing stim", Category = "medicine", Description = "Restores health quickly", Skill = "Medicine", Cost = 32 };
        CraftableItems["microdroid"] = new ItemBlueprint { Name = "microdroid", Category = "gear", Description = "A tiny maintenance drone", Skill = "Computers", Cost = 40 };
        CraftableItems["thermal detonator"] = new ItemBlueprint { Name = "thermal detonator", Category = "weapon", Description = "A powerful explosive charge", Skill = "Explosives", Cost = 90 };
        CraftableItems["vibroblade"] = new ItemBlueprint { Name = "vibroblade", Category = "weapon", Description = "A high-frequency energy blade", Skill = "Blades", Cost = 78 };
        CraftableItems["beskar plate"] = new ItemBlueprint { Name = "beskar plate", Category = "armor", Description = "A legendary armor insert", Skill = "Crafting", Cost = 95 };
        CraftableItems["sensorscope"] = new ItemBlueprint { Name = "sensorscope", Category = "ship", Description = "Magnifies sensor range", Skill = "Engineering", Cost = 58 };
        CraftableItems["sentry drone"] = new ItemBlueprint { Name = "sentry drone", Category = "gear", Description = "A compact defensive automaton", Skill = "Computers", Cost = 62 };

        ShipCatalog["Consular-class Cruiser"]    = new ShipBlueprint { Name = "Consular-class Cruiser",    Model = "Consular-class Cruiser",    Era = "Old Republic",     Cost = 420, PurchasePrice =  580_000, SizeClass = "L", ShipyardTier = "orbital",   RequiredParts = new List<string> { "hyperdrive", "reactor", "navicomputer", "shield generator" },               Ascii = new List<string> { "[=]", "|_|" },  Hull = 26, Shield = 14, Fuel = 130, HyperdriveClass = 2, CrewCapacity = 10, Weapon = "Twin turbolasers", Description = "A classic Republic vessel of diplomacy and travel." };
        ShipCatalog["ARC-170"]                   = new ShipBlueprint { Name = "ARC-170",                   Model = "ARC-170",                   Era = "Clone Wars",       Cost = 260, PurchasePrice =   36_500, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "thrusters", "sensors", "ion capacitor" },                             Ascii = new List<string> { " /\\", "|_|" }, Hull = 22, Shield = 12, Fuel =  95, HyperdriveClass = 2, CrewCapacity =  2, Weapon = "Rotary blaster", Description = "A rugged Republic bomber from the Clone Wars." };
        ShipCatalog["Republic Gunship"]          = new ShipBlueprint { Name = "Republic Gunship",          Model = "Republic Gunship",          Era = "Clone Wars",       Cost = 320, PurchasePrice =  195_000, SizeClass = "L", ShipyardTier = "orbital",   RequiredParts = new List<string> { "hyperdrive", "reactor", "navicomputer", "power cell" },                             Ascii = new List<string> { "[__]", "|_|" }, Hull = 26, Shield = 13, Fuel = 110, HyperdriveClass = 2, CrewCapacity = 12, Weapon = "Heavy cannon",    Description = "A transport and gunship used by Republic troops." };
        ShipCatalog["Delta-7 Jedi Starfighter"]  = new ShipBlueprint { Name = "Delta-7 Jedi Starfighter",  Model = "Delta-7",                   Era = "Clone Wars",       Cost = 240, PurchasePrice =   20_500, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "thrusters", "sensors" },                                             Ascii = new List<string> { " /\\", "|_|" }, Hull = 16, Shield = 10, Fuel =  90, HyperdriveClass = 2, CrewCapacity =  1, Weapon = "Light blaster",   Description = "A swift Jedi starfighter." };
        ShipCatalog["Eta-2 Actis"]               = new ShipBlueprint { Name = "Eta-2 Actis",               Model = "Eta-2",                     Era = "Clone Wars",       Cost = 260, PurchasePrice =   23_500, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "thrusters", "sensors" },                                             Ascii = new List<string> { " /\\", "|_|" }, Hull = 14, Shield =  9, Fuel =  86, HyperdriveClass = 2, CrewCapacity =  1, Weapon = "Twin saber",      Description = "An elegant Jedi interceptor." };
        ShipCatalog["Y-Wing"]                    = new ShipBlueprint { Name = "Y-Wing",                    Model = "Y-Wing",                    Era = "Original Trilogy", Cost = 220, PurchasePrice =   38_000, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "thrusters", "sensors", "ion capacitor" },                             Ascii = new List<string> { " /\\", "|_|" }, Hull = 20, Shield = 10, Fuel =  95, HyperdriveClass = 2, CrewCapacity =  2, Weapon = "Proton torpedo",  Description = "A dependable Rebel bomber." };
        ShipCatalog["TIE Bomber"]                = new ShipBlueprint { Name = "TIE Bomber",                Model = "TIE Bomber",                Era = "Original Trilogy", Cost = 180, PurchasePrice =   10_200, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "thrusters" },                                                          Ascii = new List<string> { " /\\", "|_|" }, Hull = 16, Shield =  8, Fuel =  80, HyperdriveClass = 3, CrewCapacity =  1, Weapon = "Bomb launcher",   Description = "An Imperial bomber designed for siege work." };
        ShipCatalog["TIE Interceptor"]           = new ShipBlueprint { Name = "TIE Interceptor",           Model = "TIE Interceptor",           Era = "Original Trilogy", Cost = 200, PurchasePrice =   13_500, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "thrusters", "sensors" },                                             Ascii = new List<string> { " /\\", "|_|" }, Hull = 14, Shield =  7, Fuel =  78, HyperdriveClass = 3, CrewCapacity =  1, Weapon = "Ion cannon",       Description = "A swift Imperial fighter." };
        ShipCatalog["Lambda-class Shuttle"]      = new ShipBlueprint { Name = "Lambda-class Shuttle",      Model = "Lambda-class Shuttle",      Era = "Original Trilogy", Cost = 250, PurchasePrice =   42_000, SizeClass = "M", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "navicomputer", "shield generator" },                               Ascii = new List<string> { "[=]", "|_|" },  Hull = 20, Shield = 11, Fuel = 100, HyperdriveClass = 2, CrewCapacity =  6, Weapon = "Twin blaster",    Description = "An Imperial shuttle used for command missions." };
        ShipCatalog["Imperial Shuttle"]          = new ShipBlueprint { Name = "Imperial Shuttle",          Model = "Imperial Shuttle",          Era = "Original Trilogy", Cost = 240, PurchasePrice =   34_000, SizeClass = "M", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "navicomputer" },                                                    Ascii = new List<string> { "[=]", "|_|" },  Hull = 18, Shield =  9, Fuel =  95, HyperdriveClass = 2, CrewCapacity =  6, Weapon = "Blaster turret",  Description = "A compact transport favored by Imperial officers." };
        ShipCatalog["TIE Advanced x1"]           = new ShipBlueprint { Name = "TIE Advanced x1",           Model = "TIE Advanced x1",           Era = "Original Trilogy", Cost = 260, PurchasePrice =   19_500, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "thrusters", "sensors" },                                             Ascii = new List<string> { " /\\", "|_|" }, Hull = 15, Shield =  8, Fuel =  82, HyperdriveClass = 2, CrewCapacity =  1, Weapon = "Ion cannon",       Description = "A feared Imperial elite fighter." };
        ShipCatalog["Snowspeeder"]               = new ShipBlueprint { Name = "Snowspeeder",               Model = "Snowspeeder",               Era = "Original Trilogy", Cost = 220, PurchasePrice =    9_500, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "thrusters", "sensors" },                                                          Ascii = new List<string> { " /\\", "|_|" }, Hull = 16, Shield =  8, Fuel =  88, HyperdriveClass = 3, CrewCapacity =  2, Weapon = "Laser cannon",    Description = "A repulsor craft built for icy battlefields." };
        ShipCatalog["X-34 Landspeeder"]          = new ShipBlueprint { Name = "X-34 Landspeeder",          Model = "X-34",                      Era = "New Republic",     Cost = 140, PurchasePrice =    3_800, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "thrusters" },                                                                     Ascii = new List<string> { " /\\", "|_|" }, Hull = 10, Shield =  4, Fuel =  70, HyperdriveClass = 3, CrewCapacity =  1, Weapon = "Light blaster",   Description = "A light civilian speeder for local travel." };
        ShipCatalog["U-Wing"]                    = new ShipBlueprint { Name = "U-Wing",                    Model = "U-Wing",                    Era = "New Republic",     Cost = 320, PurchasePrice =  180_000, SizeClass = "L", ShipyardTier = "orbital",   RequiredParts = new List<string> { "hyperdrive", "reactor", "navicomputer", "sensors" },                             Ascii = new List<string> { "[=]", "|_|" },  Hull = 24, Shield = 10, Fuel = 105, HyperdriveClass = 2, CrewCapacity =  8, Weapon = "Rotary cannon",   Description = "A versatile transport used by New Republic forces." };
        ShipCatalog["TIE Silencer"]              = new ShipBlueprint { Name = "TIE Silencer",              Model = "TIE Silencer",              Era = "Sequel Trilogy",   Cost = 300, PurchasePrice =   24_000, SizeClass = "S", ShipyardTier = "planetary", RequiredParts = new List<string> { "hyperdrive", "thrusters", "sensors", "shield generator" },                       Ascii = new List<string> { " /\\", "|_|" }, Hull = 18, Shield = 10, Fuel =  92, HyperdriveClass = 2, CrewCapacity =  1, Weapon = "Plasma cannon",   Description = "A sleek First Order starfighter." };
        ShipCatalog["Corellian Corvette"] = new ShipBlueprint { Name = "Corellian Corvette", Model = "Corellian Corvette", Era = "New Republic", Cost = 520, RequiredParts = new List<string> { "hyperdrive", "reactor", "navicomputer", "shield generator", "turbolaser" }, Ascii = new List<string> { "[====]", "|__|" }, Hull = 36, Shield = 18, Fuel = 170, CrewCapacity = 20, Weapon = "Turbo laser", Description = "A durable warship suited for long-range patrols." };

        Vehicles["T-16 Skyhopper"] = new VehicleBlueprint { Name = "T-16 Skyhopper", Type = "speeder", Era = "Old Republic", Ascii = new List<string> { " /\\", "|_|" }, UnlocksZones = new List<string> { "dune sea", "wastes", "open terrain" } };
        Vehicles["Dewback"] = new VehicleBlueprint { Name = "Dewback", Type = "mount", Era = "Old Republic", Ascii = new List<string> { " /\\", "|_|" }, UnlocksZones = new List<string> { "deep wilderness", "mountain pass", "dune sea" } };
        Vehicles["Bantha"] = new VehicleBlueprint { Name = "Bantha", Type = "mount", Era = "Old Republic", Ascii = new List<string> { " /\\", "|_|" }, UnlocksZones = new List<string> { "deep wilderness", "mountain pass", "dune sea" } };
        Vehicles["AT-TE"] = new VehicleBlueprint { Name = "AT-TE", Type = "walker", Era = "Clone Wars", Ascii = new List<string> { " /|\\", "|_|_|" }, UnlocksZones = new List<string> { "industrial platform", "volcanic flats", "battlefield" } };
        Vehicles["STAP"] = new VehicleBlueprint { Name = "STAP", Type = "walker", Era = "Clone Wars", Ascii = new List<string> { " /\\", "|_|" }, UnlocksZones = new List<string> { "industrial platform", "forest" } };
        Vehicles["Repulsor Sled"] = new VehicleBlueprint { Name = "Repulsor Sled", Type = "sled", Era = "Original Trilogy", Ascii = new List<string> { " /\\", "|_|" }, UnlocksZones = new List<string> { "dune sea", "wastes" } };
        Vehicles["BARC Speeder"] = new VehicleBlueprint { Name = "BARC Speeder", Type = "speeder", Era = "Clone Wars", Ascii = new List<string> { " /\\", "|_|" }, UnlocksZones = new List<string> { "dune sea", "wastes", "open terrain", "forest" } };
        Vehicles["AAT"] = new VehicleBlueprint { Name = "AAT", Type = "walker", Era = "Clone Wars", Ascii = new List<string> { " /|\\", "|_|_|" }, UnlocksZones = new List<string> { "industrial platform", "battlefield", "volcanic flats" } };
        Vehicles["Snowspeeder"] = new VehicleBlueprint { Name = "Snowspeeder", Type = "speeder", Era = "Original Trilogy", Ascii = new List<string> { " /\\", "|_|" } };
        Vehicles["U-Wing"] = new VehicleBlueprint { Name = "U-Wing", Type = "transport", Era = "New Republic", Ascii = new List<string> { "[=]", "|_|" } };

        Weapons["DL-44 Heavy Blaster Pistol"]  = new WeaponBlueprint { Name = "DL-44 Heavy Blaster Pistol",  Category = "blaster",   WeaponSubtype = "pistol",        IsOneHanded = true,  Era = "Old Republic",     Damage = 9,  Description = "A famous sidearm from the Outer Rim." };
        Weapons["DC-15A Blaster Rifle"]        = new WeaponBlueprint { Name = "DC-15A Blaster Rifle",        Category = "blaster",   WeaponSubtype = "blaster_rifle", IsOneHanded = false, Era = "Clone Wars",       Damage = 11, Description = "A reliable Republic infantry rifle." };
        Weapons["DC-17 Hand Blaster"]          = new WeaponBlueprint { Name = "DC-17 Hand Blaster",          Category = "blaster",   WeaponSubtype = "pistol",        IsOneHanded = true,  Era = "Clone Wars",       Damage = 10, Description = "A compact blaster favored by clone commandos." };
        Weapons["E-11 Blaster Rifle"]          = new WeaponBlueprint { Name = "E-11 Blaster Rifle",          Category = "blaster",   WeaponSubtype = "blaster_rifle", IsOneHanded = false, Era = "Original Trilogy", Damage = 10, Description = "A standard Imperial infantry weapon." };
        Weapons["SE-14r Blaster Pistol"] = new WeaponBlueprint { Name = "SE-14r Blaster Pistol", Category = "blaster", Era = "Original Trilogy", Damage = 8, Description = "A compact Imperial sidearm." };
        Weapons["Westar-35 Blaster"] = new WeaponBlueprint { Name = "Westar-35 Blaster", Category = "blaster", Era = "Original Trilogy", Damage = 12, Description = "A powerful Mandalorian-style sidearm." };
        Weapons["Z-6 Rotary Blaster Cannon"] = new WeaponBlueprint { Name = "Z-6 Rotary Blaster Cannon", Category = "blaster", Era = "Clone Wars", Damage = 14, Description = "A heavy rotary weapon used in close support." };
        Weapons["SE-14r Blaster Pistol"]       = new WeaponBlueprint { Name = "SE-14r Blaster Pistol",       Category = "blaster",    WeaponSubtype = "pistol",        IsOneHanded = true,  Era = "Original Trilogy", Damage = 8,  Description = "A compact Imperial sidearm." };
        Weapons["Westar-35 Blaster"]           = new WeaponBlueprint { Name = "Westar-35 Blaster",            Category = "blaster",    WeaponSubtype = "pistol",        IsOneHanded = true,  Era = "Original Trilogy", Damage = 12, Description = "A powerful Mandalorian-style sidearm." };
        Weapons["Z-6 Rotary Blaster Cannon"]   = new WeaponBlueprint { Name = "Z-6 Rotary Blaster Cannon",   Category = "blaster",    WeaponSubtype = "rotary_cannon", IsOneHanded = false, Era = "Clone Wars",       Damage = 14, Description = "A heavy rotary weapon used in close support." };
        Weapons["Vibroblade"]                  = new WeaponBlueprint { Name = "Vibroblade",                   Category = "melee",      WeaponSubtype = "vibroblade",    IsOneHanded = true,  Era = "Old Republic",     Damage = 12, Description = "A sharp energy edge for precise strikes." };
        Weapons["Force Pike"]                  = new WeaponBlueprint { Name = "Force Pike",                   Category = "melee",      WeaponSubtype = "melee",         IsOneHanded = false, Era = "Old Republic",     Damage = 13, Description = "A ceremonial weapon of the ancient Jedi and Sith." };
        Weapons["Gaffi Stick"]                 = new WeaponBlueprint { Name = "Gaffi Stick",                  Category = "melee",      WeaponSubtype = "melee",         IsOneHanded = false, Era = "Old Republic",     Damage = 10, Description = "A brutal club of the Tusken Raiders." };
        Weapons["Droid Burst Pistol"]          = new WeaponBlueprint { Name = "Droid Burst Pistol",           Category = "blaster",    WeaponSubtype = "pistol",        IsOneHanded = true,  Era = "Clone Wars",       Damage = 9,  Description = "A short-range weapon used by battle droids." };
        Weapons["Relby-v10 Mortar"]            = new WeaponBlueprint { Name = "Relby-v10 Mortar",             Category = "explosive",  WeaponSubtype = "explosive",     IsOneHanded = false, Era = "Original Trilogy", Damage = 16, Description = "A devastating support weapon." };
        Weapons["T-21 Light Repeating Blaster"]= new WeaponBlueprint { Name = "T-21 Light Repeating Blaster",Category = "blaster",    WeaponSubtype = "blaster_rifle", IsOneHanded = false, Era = "Original Trilogy", Damage = 13, Description = "A flexible ranged weapon for blaster warfare." };
        Weapons["Wrist Blaster"]               = new WeaponBlueprint { Name = "Wrist Blaster",                Category = "blaster",    WeaponSubtype = "pistol",        IsOneHanded = true,  Era = "Sequel Trilogy",   Damage = 8,  Description = "A compact weapon built for quick draw attacks." };
        Weapons["Bowcaster"]                   = new WeaponBlueprint { Name = "Bowcaster",                    Category = "projectile", WeaponSubtype = "bowcaster",     IsOneHanded = false, Era = "Old Republic",     Damage = 10, Description = "The favored ranged weapon of the Wookiees." };
        Weapons["Electrostaff"]                = new WeaponBlueprint { Name = "Electrostaff",                 Category = "melee",      WeaponSubtype = "melee",         IsOneHanded = false, Era = "Clone Wars",       Damage = 12, Description = "A brutal close-combat weapon." };

        InitializeShipSystems();
        InitializeSpaceStations();

        InitializeDefaultCreatures();
        RebuildCreatureTerritories();
        InitializeInterspeciesRelations();
        InitializePlanetEconomyStates();

        InitializeMaterialEconomyAndRecipes();
        InitializeFactionUnlocks();

        return this;
    }

    private void InitializeFactionUnlocks()
    {
        factionUnlocks.Clear();

        SetFactionUnlock("Imperial Star Destroyer", "Empire", 14, "Imperial high-command hull access");
        SetFactionUnlock("TIE Fighter", "Empire", 8, "Imperial pilot clearance");
        SetFactionUnlock("TIE Bomber", "Empire", 9, "Imperial bomber division clearance");
        SetFactionUnlock("TIE Interceptor", "Empire", 10, "Imperial interceptor wing clearance");
        SetFactionUnlock("TIE Advanced x1", "Empire", 12, "Restricted elite prototype access");
        SetFactionUnlock("Lambda-class Shuttle", "Empire", 8, "Imperial transport authorization");
        SetFactionUnlock("Imperial Shuttle", "Empire", 8, "Imperial transport authorization");
        SetFactionUnlock("First Order TIE Fighter", "Empire", 10, "First Order military access");
        SetFactionUnlock("TIE Silencer", "Empire", 12, "First Order elite craft access");
        SetFactionUnlock("E-11 Blaster Rifle", "Empire", 7, "Imperial infantry armory access");
        SetFactionUnlock("SE-14r Blaster Pistol", "Empire", 6, "Imperial sidearm access");
        SetFactionUnlock("military spec reactor", "Empire", 12, "Restricted naval engineering license");
        SetFactionUnlock("military spec powerplant", "Empire", 14, "Restricted naval engineering license");
        SetFactionUnlock("military spec shield lattice", "Empire", 15, "Restricted fleet shield matrix access");

        SetFactionUnlock("X-Wing", "Rebels", 6, "Alliance pilot approval");
        SetFactionUnlock("A-Wing", "Rebels", 7, "Alliance interceptor approval");
        SetFactionUnlock("B-Wing", "Rebels", 8, "Alliance heavy strike approval");
        SetFactionUnlock("Y-Wing", "Rebels", 6, "Alliance bomber approval");
        SetFactionUnlock("U-Wing", "Rebels", 6, "Alliance transport approval");
        SetFactionUnlock("Corellian Corvette", "Rebels", 10, "Alliance fleet command approval");
        SetFactionUnlock("military spec reactor", "Rebels", 12, "Alliance engineering trust");
        SetFactionUnlock("military spec powerplant", "Rebels", 14, "Alliance engineering trust");
        SetFactionUnlock("military spec shield lattice", "Rebels", 15, "Alliance fleet shield trust");

        SetFactionUnlock("Jedi Starfighter", "Jedi", 6, "Jedi Order authorization");
        SetFactionUnlock("Delta-7 Jedi Starfighter", "Jedi", 7, "Jedi starfighter authorization");
        SetFactionUnlock("Eta-2 Actis", "Jedi", 7, "Jedi interceptor authorization");
        SetFactionUnlock("Lightsaber", "Jedi", 5, "Jedi weapon authorization");
        SetFactionUnlock("lightsaber crystal", "Jedi", 5, "Jedi kyber authorization");
        SetFactionUnlock("cut kyber crystal", "Jedi", 5, "Jedi kyber authorization");
        SetFactionUnlock("military spec reactor", "Jedi", 12, "Jedi logistics clearance");
        SetFactionUnlock("military spec powerplant", "Jedi", 14, "Jedi logistics clearance");
        SetFactionUnlock("military spec shield lattice", "Jedi", 15, "Jedi fleet shield clearance");

        SetFactionUnlock("Westar-35 Blaster", "Mandalorians", 6, "Mandalorian forge access");
        SetFactionUnlock("beskar plate", "Mandalorians", 7, "Mandalorian beskar trust");
    }

    public void SetFactionUnlock(string itemName, string faction, int minStanding, string note = "")
    {
        if (string.IsNullOrWhiteSpace(itemName) || string.IsNullOrWhiteSpace(faction)) return;
        factionUnlocks[itemName] = (faction, Math.Max(0, minStanding), note);
        if (!FactionStandings.ContainsKey(faction))
        {
            FactionStandings[faction] = 0;
        }
    }

    private bool TryGetFactionUnlock(string itemName, out string faction, out int minStanding, out string note)
    {
        faction = string.Empty;
        minStanding = 0;
        note = string.Empty;

        if (string.IsNullOrWhiteSpace(itemName)) return false;

        if (factionUnlocks.TryGetValue(itemName, out var explicitRule))
        {
            faction = explicitRule.Faction;
            minStanding = explicitRule.MinStanding;
            note = explicitRule.Note;
            return true;
        }

        var key = itemName.ToLowerInvariant();
        if (key.Contains("imperial") || key.Contains("first order") || key.Contains("star destroyer") || key.Contains("tie "))
        {
            faction = "Empire";
            minStanding = key.Contains("star destroyer") ? 14 : 8;
            note = "Military procurement restrictions";
            return true;
        }

        if (key.Contains("jedi") || key.Contains("lightsaber") || key.Contains("kyber"))
        {
            faction = "Jedi";
            minStanding = 5;
            note = "Jedi order restrictions";
            return true;
        }

        if (key.Contains("mandal") || key.Contains("beskar") || key.Contains("westar"))
        {
            faction = "Mandalorians";
            minStanding = 6;
            note = "Clan trust restrictions";
            return true;
        }

        if (key.Contains("x-wing") || key.Contains("a-wing") || key.Contains("b-wing") || key.Contains("y-wing") || key.Contains("u-wing") || key.Contains("corellian corvette"))
        {
            faction = "Rebels";
            minStanding = 6;
            note = "Alliance procurement restrictions";
            return true;
        }

        return false;
    }

    private bool CanAccessFactionLockedAsset(GameCharacter character, string itemName, out string denial)
    {
        denial = string.Empty;
        if (!TryGetFactionUnlock(itemName, out var faction, out var minStanding, out var note))
        {
            return true;
        }

        var standing = FactionStandings.GetValueOrDefault(faction);
        if (standing >= minStanding)
        {
            return true;
        }

        denial = string.IsNullOrWhiteSpace(note)
            ? $"Access denied: {itemName} requires {faction} standing {minStanding} (current {standing})."
            : $"Access denied: {itemName} requires {faction} standing {minStanding} (current {standing}). {note}.";
        return false;
    }

    private bool TryGetEraUnlockForAsset(string itemName, out string eraName)
    {
        eraName = string.Empty;
        if (string.IsNullOrWhiteSpace(itemName)) return false;

        if (ShipCatalog.TryGetValue(itemName, out var directShip))
        {
            eraName = directShip.Era;
            return !string.IsNullOrWhiteSpace(eraName);
        }

        var byModel = ShipCatalog.Values.FirstOrDefault(x => string.Equals(x.Model, itemName, StringComparison.OrdinalIgnoreCase) || string.Equals(x.Name, itemName, StringComparison.OrdinalIgnoreCase));
        if (byModel is not null && !string.IsNullOrWhiteSpace(byModel.Era))
        {
            eraName = byModel.Era;
            return true;
        }

        if (Weapons.TryGetValue(itemName, out var weapon) && !string.IsNullOrWhiteSpace(weapon.Era))
        {
            eraName = weapon.Era;
            return true;
        }

        if (ShipArmaments.TryGetValue(itemName, out var armament) && !string.IsNullOrWhiteSpace(armament.Era))
        {
            eraName = armament.Era;
            return true;
        }

        if (ShipUpgradeCatalog.TryGetValue(itemName, out var upgrade) && !string.IsNullOrWhiteSpace(upgrade.Era))
        {
            eraName = upgrade.Era;
            return true;
        }

        if (Vehicles.TryGetValue(itemName, out var vehicle) && !string.IsNullOrWhiteSpace(vehicle.Era))
        {
            eraName = vehicle.Era;
            return true;
        }

        var lower = itemName.ToLowerInvariant();
        if (lower.Contains("first order") || lower.Contains("silencer") || lower.Contains("sequel"))
        {
            eraName = "Sequel Trilogy";
            return true;
        }
        if (lower.Contains("new republic"))
        {
            eraName = "New Republic";
            return true;
        }
        if (lower.Contains("imperial") || lower.Contains("tie ") || lower.Contains("x-wing") || lower.Contains("a-wing") || lower.Contains("b-wing") || lower.Contains("y-wing") || lower.Contains("u-wing"))
        {
            eraName = "Original Trilogy";
            return true;
        }
        if (lower.Contains("clone") || lower.Contains("venator") || lower.Contains("arc-170") || lower.Contains("delta-7") || lower.Contains("eta-2") || lower.Contains("republic gunship"))
        {
            eraName = "Clone Wars";
            return true;
        }

        return false;
    }

    private bool CanAccessEraLockedAsset(string itemName, out string denial)
    {
        denial = string.Empty;
        if (!TryGetEraUnlockForAsset(itemName, out var requiredEra)) return true;

        var required = Eras.GetValueOrDefault(requiredEra)?.StartRotation ?? int.MaxValue;
        if (Clock.Rotation >= required) return true;

        denial = $"Access denied: {itemName} requires era {requiredEra}. Current era is {GetCurrentEraName()} (rotation {Clock.Rotation}).";
        return false;
    }

    private bool CanAccessAsset(GameCharacter character, string itemName, out string denial)
    {
        if (!CanCraftBlueprint(character, itemName, out denial)) return false;
        if (!CanAccessFactionLockedAsset(character, itemName, out denial)) return false;
        if (!CanAccessEraLockedAsset(itemName, out denial)) return false;
        denial = string.Empty;
        return true;
    }

    private bool CanCraftBlueprint(GameCharacter character, string itemName, out string denial)
    {
        denial = string.Empty;
        if (!TryGetRecipe(itemName, out var recipe) || !recipe.RequiresBlueprint)
        {
            return true;
        }

        var blueprintName = string.IsNullOrWhiteSpace(recipe.BlueprintName) ? itemName : recipe.BlueprintName;
        if (KnowsBlueprint(character, blueprintName))
        {
            return true;
        }

        var sourceHint = string.IsNullOrWhiteSpace(recipe.BlueprintSourceHint)
            ? "Blueprint not yet acquired."
            : recipe.BlueprintSourceHint;
        denial = $"Access denied: {itemName} requires blueprint {blueprintName}. {sourceHint}";
        return false;
    }

    public bool KnowsBlueprint(GameCharacter character, string blueprintName)
        => character.KnownBlueprints.Any(x => string.Equals(x, blueprintName, StringComparison.OrdinalIgnoreCase));

    public string UnlockBlueprint(GameCharacter character, string blueprintName, string source = "")
    {
        if (string.IsNullOrWhiteSpace(blueprintName)) return string.Empty;
        if (KnowsBlueprint(character, blueprintName)) return string.Empty;

        character.KnownBlueprints.Add(blueprintName);
        character.KnownBlueprints = character.KnownBlueprints
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var note = string.IsNullOrWhiteSpace(source)
            ? $"Unlocked blueprint: {blueprintName}."
            : $"Unlocked blueprint: {blueprintName} via {source}.";
        character.Notes.Add(note);
        return note;
    }

    public bool IsRecipeVisible(GameCharacter character, string itemName)
    {
        if (!TryGetRecipe(itemName, out var recipe)) return false;
        if (!recipe.HideUntilBlueprintUnlocked) return true;

        var blueprintName = string.IsNullOrWhiteSpace(recipe.BlueprintName) ? itemName : recipe.BlueprintName;
        return KnowsBlueprint(character, blueprintName);
    }

    public IReadOnlyList<string> GetVisibleCraftingOptions(GameCharacter character, string? category = null)
    {
        return Recipes.Keys
            .Where(name => IsRecipeVisible(character, name))
            .Where(name => string.IsNullOrWhiteSpace(category)
                || (CraftableItems.TryGetValue(name, out var bp)
                    && bp.Category.Contains(category, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public bool IsHangarAccessible(GameCharacter character)
    {
        if (Planets.TryGetValue(character.Location, out var planet) && planet.HasDockyard) return true;
        if (SpaceStations.TryGetValue(character.Location, out var station) && station.HasShipyard) return true;
        return false;
    }

    public string MoveItemToHangar(GameCharacter character, string itemName)
    {
        if (!IsHangarAccessible(character)) return "You must be at a hangar-enabled world to store ship gear.";
        if (!character.Inventory.Any(x => string.Equals(x, itemName, StringComparison.OrdinalIgnoreCase))) return "That item is not in your inventory.";
        if (character.HangarInventory.Any(x => string.Equals(x, itemName, StringComparison.OrdinalIgnoreCase))) return $"{itemName} is already stored in the hangar.";

        character.Inventory.RemoveAll(x => string.Equals(x, itemName, StringComparison.OrdinalIgnoreCase));
        character.HangarInventory.Add(itemName);
        return $"Transferred {itemName} to hangar storage.";
    }

    public string MoveItemFromHangar(GameCharacter character, string itemName)
    {
        if (!IsHangarAccessible(character)) return "You must be at a hangar-enabled world to retrieve ship gear.";
        if (!character.HangarInventory.Any(x => string.Equals(x, itemName, StringComparison.OrdinalIgnoreCase))) return "That item is not stored in the hangar.";
        if (character.Inventory.Any(x => string.Equals(x, itemName, StringComparison.OrdinalIgnoreCase))) return $"{itemName} is already in your inventory.";

        character.HangarInventory.RemoveAll(x => string.Equals(x, itemName, StringComparison.OrdinalIgnoreCase));
        character.Inventory.Add(itemName);
        return $"Retrieved {itemName} from hangar storage.";
    }

    public bool TryGetAssetLockReason(GameCharacter character, string itemName, out string reason)
    {
        var allowed = CanAccessAsset(character, itemName, out reason);
        return !allowed;
    }

    public string GetAssetAccessCategory(GameCharacter? character, string itemName, out string reason)
    {
        reason = string.Empty;

        if (character is not null && TryGetRecipe(itemName, out var recipe) && recipe.RequiresBlueprint)
        {
            var blueprintName = string.IsNullOrWhiteSpace(recipe.BlueprintName) ? itemName : recipe.BlueprintName;
            if (!KnowsBlueprint(character, blueprintName))
            {
                reason = $"Access denied: {itemName} requires blueprint {blueprintName}.";
                return "blueprint-locked";
            }
        }

        if (TryGetEraUnlockForAsset(itemName, out var requiredEra))
        {
            var required = Eras.GetValueOrDefault(requiredEra)?.StartRotation ?? int.MaxValue;
            if (Clock.Rotation < required)
            {
                reason = $"Access denied: {itemName} requires era {requiredEra}. Current era is {GetCurrentEraName()} (rotation {Clock.Rotation}).";
                return "era-locked";
            }
        }

        if (character is not null && TryGetFactionUnlock(itemName, out var faction, out var minStanding, out var note))
        {
            var standing = FactionStandings.GetValueOrDefault(faction);
            if (standing < minStanding)
            {
                reason = string.IsNullOrWhiteSpace(note)
                    ? $"Access denied: {itemName} requires {faction} standing {minStanding} (current {standing})."
                    : $"Access denied: {itemName} requires {faction} standing {minStanding} (current {standing}). {note}.";
                return "faction-locked";
            }
        }

        return "unlocked";
    }

    public string GetAssetRequirementReport(GameCharacter? character, string assetName)
    {
        if (string.IsNullOrWhiteSpace(assetName))
        {
            return "Select an item or ship to view requirements.";
        }

        var lines = new List<string> { $"Asset: {assetName}" };

        if (TryGetEraUnlockForAsset(assetName, out var requiredEra))
        {
            var requiredRotation = Eras.GetValueOrDefault(requiredEra)?.StartRotation ?? 0;
            lines.Add($"Era unlock: {requiredEra} (rotation {requiredRotation})");
            lines.Add($"Current era: {GetCurrentEraName()} (rotation {Clock.Rotation})");
        }

        if (TryGetFactionUnlock(assetName, out var faction, out var minStanding, out var note))
        {
            var currentStanding = FactionStandings.GetValueOrDefault(faction);
            lines.Add($"Faction unlock: {faction} standing {minStanding} (current {currentStanding})");
            if (!string.IsNullOrWhiteSpace(note))
            {
                lines.Add($"Faction note: {note}");
            }
        }

        if (character is not null)
        {
            var allowed = CanAccessAsset(character, assetName, out var denial);
            lines.Add(allowed ? "Access: Unlocked" : $"Access: Locked - {denial}");
        }

        if (ShipUpgradeCatalog.TryGetValue(assetName, out var shipUpgrade))
        {
            lines.Add($"Ship upgrade: {shipUpgrade.UpgradeKind}");
            lines.Add($"Minimum ship size: {shipUpgrade.MinimumShipSize}");
            if (shipUpgrade.HyperdriveClassTarget > 0) lines.Add($"Hyperdrive target: Class {shipUpgrade.HyperdriveClassTarget}");
            if (shipUpgrade.FuelCapacityBonus > 0) lines.Add($"Fuel capacity: +{shipUpgrade.FuelCapacityBonus}");
            if (shipUpgrade.FuelEfficiencyDeltaPercent != 0) lines.Add($"Fuel efficiency delta: {shipUpgrade.FuelEfficiencyDeltaPercent}%");
            if (shipUpgrade.TravelHoursModifier != 0) lines.Add($"Travel time modifier: {shipUpgrade.TravelHoursModifier}h");
            if (shipUpgrade.ShieldBonus > 0) lines.Add($"Shield bonus: +{shipUpgrade.ShieldBonus}");
            if (shipUpgrade.HullBonus > 0) lines.Add($"Hull bonus: +{shipUpgrade.HullBonus}");
            if (shipUpgrade.RefuelAmount > 0) lines.Add($"Refuel amount: +{shipUpgrade.RefuelAmount}");
            if (character?.Ship is not null)
            {
                lines.Add($"Current ship: {character.Ship.Name} [{character.Ship.SizeClass}] | Hyperdrive {character.Ship.HyperdriveClass} | Fuel {character.Ship.Fuel}/{character.Ship.MaxFuel}");
            }
        }

        if (ShipArmaments.TryGetValue(assetName, out var shipArmament))
        {
            lines.Add($"Ship armament: {shipArmament.Category}");
            lines.Add($"Required hardpoint: {shipArmament.HardpointSize}");
            lines.Add($"Damage rating: {shipArmament.DamageRating}");
            lines.Add($"Fuel draw: {shipArmament.FuelDraw}");
            if (character?.Ship is not null)
            {
                lines.Add($"Current ship: {character.Ship.Name} [{character.Ship.SizeClass}] | Hardpoints {GetShipHardpointSummary(character.Ship)}");
            }
        }

        if (TryGetRecipe(assetName, out var recipe))
        {
            lines.Add($"Recipe cost: {recipe.CreditCost} credits | Time: {recipe.TimeHours}h | Skill: {recipe.Skill}");
            if (recipe.RequiresBlueprint)
            {
                var blueprintName = string.IsNullOrWhiteSpace(recipe.BlueprintName) ? assetName : recipe.BlueprintName;
                lines.Add($"Blueprint required: {blueprintName}");
                if (!string.IsNullOrWhiteSpace(recipe.BlueprintSourceHint)) lines.Add($"Blueprint source: {recipe.BlueprintSourceHint}");
                if (character is not null)
                {
                    lines.Add(KnowsBlueprint(character, blueprintName) ? "Blueprint status: Known" : "Blueprint status: Unknown");
                }
            }
            foreach (var facility in GetFacilityRequirementsForAsset(assetName, recipe))
            {
                lines.Add($"Facility: {facility} required");
            }

            if (recipe.Inputs.Count > 0)
            {
                lines.Add("Ingredients:");
                foreach (var input in recipe.Inputs.Where(x => x.Quantity > 0))
                {
                    lines.Add($"- {input.Item} x{input.Quantity}");
                }
            }
        }

        var ship = ShipCatalog.Values.FirstOrDefault(x => string.Equals(x.Name, assetName, StringComparison.OrdinalIgnoreCase) || string.Equals(x.Model, assetName, StringComparison.OrdinalIgnoreCase));
        if (ship is not null)
        {
            lines.Add($"Ship cost: {ship.Cost} credits | Era: {ship.Era}");
            lines.Add($"Base parts: {string.Join(", ", ship.RequiredParts)}");
            if (ship.IsCapital && CapitalShipPartRequirements.TryGetValue(ship.Model, out var capitalParts))
            {
                lines.Add("Capital assembly parts:");
                foreach (var part in capitalParts)
                {
                    lines.Add($"- {part} x1");
                }
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private IReadOnlyList<string> GetFacilityRequirementsForAsset(string assetName, CraftRecipe? recipe = null)
    {
        var facilities = new List<string>();
        var key = assetName.Trim();

        if (Vehicles.TryGetValue(key, out var vehicle))
        {
            facilities.Add(GetVehicleFactoryName(vehicle));
            if ((recipe?.RequiresIndustrialFurnace ?? false) && !facilities.Contains("Forge", StringComparer.OrdinalIgnoreCase))
            {
                facilities.Add("Forge");
            }
            return facilities;
        }

        if (ShipCatalog.Values.Any(x => string.Equals(x.Name, key, StringComparison.OrdinalIgnoreCase) || string.Equals(x.Model, key, StringComparison.OrdinalIgnoreCase)))
        {
            facilities.Add("Shipyard");
            if ((recipe?.RequiresIndustrialFurnace ?? false)) facilities.Add("Forge");
            return facilities;
        }

        if (RefiningMap.Values.Any(x => string.Equals(x, key, StringComparison.OrdinalIgnoreCase)))
        {
            var rawInput = recipe?.Inputs.FirstOrDefault()?.Item ?? string.Empty;
            facilities.Add(RequiresIndustrialFurnaceForRaw(rawInput) ? "Forge" : "Refinery");
            return facilities;
        }

        if (recipe?.RequiresShipyard == true)
        {
            facilities.Add("Shipyard");
        }

        if (recipe?.RequiresIndustrialFurnace == true)
        {
            facilities.Add("Forge");
        }

        return facilities;
    }

    private static string GetVehicleFactoryName(VehicleBlueprint vehicle)
    {
        var type = vehicle.Type.ToLowerInvariant();
        return type switch
        {
            "speeder" => "Speeder Factory",
            "walker" => "Walker Factory",
            "crawler" => "Crawler Factory",
            "swoop" => "Swoop Factory",
            "sled" => "Sled Factory",
            "barge" => "Barge Factory",
            "mount" => "Stable",
            "transport" => "Transport Factory",
            _ => $"{char.ToUpper(vehicle.Type[0])}{vehicle.Type[1..]} Factory"
        };
    }

    private void InitializeDefaultCreatures()
    {
        Creatures["Sarlacc"] = new CreatureData { Name = "Sarlacc", Description = "A massive pit predator of desert worlds.", Category = "beast", SizeClass = "colossal", Habitat = "desert", DangerRating = 10, SourceUrl = "manual" };
        Creatures["Rancor"] = new CreatureData { Name = "Rancor", Description = "A towering apex predator known for brute strength.", Category = "beast", SizeClass = "huge", Habitat = "cave", DangerRating = 8, SourceUrl = "manual" };
        Creatures["Krayt Dragon"] = new CreatureData { Name = "Krayt Dragon", Description = "A giant desert predator from the Dune Seas.", Category = "beast", SizeClass = "colossal", Habitat = "desert", DangerRating = 9, SourceUrl = "manual" };
        Creatures["Nexu"] = new CreatureData { Name = "Nexu", Description = "A swift feline hunter with vicious claws.", Category = "predator", SizeClass = "large", Habitat = "jungle", DangerRating = 6, SourceUrl = "manual" };
        Creatures["Wampa"] = new CreatureData { Name = "Wampa", Description = "An icy cave predator from frozen worlds.", Category = "predator", SizeClass = "large", Habitat = "ice", DangerRating = 7, SourceUrl = "manual" };
        Creatures["Mynock"] = new CreatureData { Name = "Mynock", Description = "A flying parasite that feeds on power systems.", Category = "fauna", SizeClass = "small", Habitat = "asteroid", DangerRating = 3, SourceUrl = "manual" };
        Creatures["Exogorth"] = new CreatureData { Name = "Exogorth", Description = "A giant space-dwelling slug.", Category = "space-fauna", SizeClass = "colossal", Habitat = "asteroid", DangerRating = 9, SourceUrl = "manual" };
        Creatures["Purrgil"] = new CreatureData { Name = "Purrgil", Description = "Migratory space whales capable of hyperspace travel.", Category = "space-fauna", SizeClass = "colossal", Habitat = "space", DangerRating = 5, SourceUrl = "manual" };
        Creatures["Dianoga"] = new CreatureData { Name = "Dianoga", Description = "A sewer-dwelling tentacled scavenger.", Category = "fauna", SizeClass = "medium", Habitat = "sewer", DangerRating = 4, SourceUrl = "manual" };
        Creatures["Maw Entity"] = new CreatureData { Name = "Maw Entity", Description = "A gigantic gravity-well creature from the Maw region.", Category = "space-fauna", SizeClass = "colossal", Habitat = "nebula", DangerRating = 10, SourceUrl = "manual" };
    }

    private void RebuildCreatureTerritories()
    {
        PlanetCreatureTerritories.Clear();
        if (Creatures.Count == 0) return;

        foreach (var planet in Planets.Values)
        {
            var habitatHint = InferPlanetHabitatHint(planet.Name, planet.Economy);
            var matching = Creatures.Values
                .Where(c => c.Habitat.Contains(habitatHint, StringComparison.OrdinalIgnoreCase)
                    || c.Habitat.Equals("general", StringComparison.OrdinalIgnoreCase)
                    || (habitatHint == "space" && (c.Habitat.Contains("nebula", StringComparison.OrdinalIgnoreCase) || c.Habitat.Contains("asteroid", StringComparison.OrdinalIgnoreCase))))
                .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .Select(c => c.Name)
                .Take(40)
                .ToList();

            if (matching.Count == 0)
            {
                matching = Creatures.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).Take(20).ToList();
            }

            PlanetCreatureTerritories[planet.Name] = matching;
        }
    }

    private void InitializePlanetEconomyStates()
    {
        PlanetEconomyStates.Clear();
        foreach (var planet in Planets.Values)
        {
            var eco = (planet.Economy + " " + planet.Region + " " + planet.Sector).ToLowerInvariant();
            var resourceLevel = eco.Contains("mining") || eco.Contains("refinery") ? 75 : eco.Contains("agric") || eco.Contains("farming") ? 88 : 82;
            var tradeHealth = eco.Contains("trade") || eco.Contains("finance") || eco.Contains("ship") ? 72 : 58;
            PlanetEconomyStates[planet.Name] = new PlanetEconomyStatus
            {
                PlanetName = planet.Name,
                ResourceLevel = Math.Clamp(resourceLevel, 0, 100),
                ImperialExtraction = false,
                TradeHealth = Math.Clamp(tradeHealth, 0, 100),
                StatusText = "Stable"
            };
        }
    }

    private void AdvancePlanetEconomies(int hours)
    {
        if (hours <= 0) return;
        var empirePressure = FactionStandings.GetValueOrDefault("Empire");
        foreach (var planet in Planets.Values)
        {
            if (!PlanetEconomyStates.TryGetValue(planet.Name, out var state)) continue;

            var eco = (planet.Economy + " " + planet.Region + " " + planet.Sector).ToLowerInvariant();
            var extractionProne = eco.Contains("mining") || eco.Contains("refinery") || eco.Contains("industry") || eco.Contains("ship");
            var extractionChance = extractionProne ? 0.04 : 0.015;
            if (empirePressure >= 6) extractionChance += 0.02;

            if (!state.ImperialExtraction && random.NextDouble() < extractionChance)
            {
                state.ImperialExtraction = true;
            }
            else if (state.ImperialExtraction && random.NextDouble() < 0.01)
            {
                state.ImperialExtraction = false;
            }

            var depletion = state.ImperialExtraction ? random.Next(1, 4) : random.Next(0, 2);
            if (extractionProne) depletion += random.Next(0, 2);
            depletion = (int)Math.Ceiling(depletion * Math.Max(1, hours) / 6.0);

            var recovery = state.ImperialExtraction ? 0 : random.Next(0, 2);
            if (eco.Contains("agric") || eco.Contains("farming")) recovery += 1;
            recovery = (int)Math.Ceiling(recovery * Math.Max(1, hours) / 8.0);

            state.ResourceLevel = Math.Clamp(state.ResourceLevel - depletion + recovery, 0, 100);

            var tradeDelta = random.Next(-2, 3) + (state.ImperialExtraction ? -2 : 0) + (state.ResourceLevel < 25 ? -2 : state.ResourceLevel > 70 ? 1 : 0);
            state.TradeHealth = Math.Clamp(state.TradeHealth + tradeDelta, 0, 100);

            state.StatusText = state.ResourceLevel switch
            {
                <= 10 => state.ImperialExtraction ? "Imperial Strip-Mined" : "Resource Collapse",
                <= 25 => state.ImperialExtraction ? "Empire Extraction Crisis" : "Resource Scarcity",
                <= 45 => "Strained",
                >= 80 => "Prosperous",
                _ => "Stable"
            };

            // If state changed heavily, allow merchant refresh next inventory query.
            if (state.ResourceLevel <= 25 || state.ImperialExtraction)
            {
                merchantRestockRotation[planet.Name] = Math.Min(merchantRestockRotation.GetValueOrDefault(planet.Name, Clock.Rotation), Clock.Rotation - 2);
            }
        }
    }

    private void SimulateGalaxyBackground(int hours, string focusPlanet)
    {
        if (hours <= 0 || Planets.Count == 0) return;

        // Faction drift: universe evolves regardless of direct player actions.
        var factions = FactionStandings.Keys.ToList();
        foreach (var faction in factions)
        {
            var drift = random.NextDouble() < 0.30 ? random.Next(-1, 2) : 0;
            FactionStandings[faction] = Math.Clamp(FactionStandings[faction] + drift, -50, 50);
        }

        // Planet threat and economy interplay.
        foreach (var planet in Planets.Values)
        {
            var state = GetPlanetEconomyStatus(planet.Name);
            if (state.ImperialExtraction && random.NextDouble() < 0.35)
            {
                planet.ThreatLevel = planet.ThreatLevel == "Low" ? "Moderate" : planet.ThreatLevel == "Moderate" ? "High" : planet.ThreatLevel;
            }
            else if (!state.ImperialExtraction && state.ResourceLevel > 75 && random.NextDouble() < 0.20)
            {
                planet.ThreatLevel = planet.ThreatLevel == "High" ? "Moderate" : planet.ThreatLevel == "Moderate" ? "Low" : planet.ThreatLevel;
            }
        }

        EmitGalaxyNews(hours, focusPlanet);
    }

    private void EmitGalaxyNews(int hours, string focusPlanet)
    {
        if (random.NextDouble() > 0.75) return;

        var planetName = Planets.Keys.OrderBy(_ => random.Next()).First();
        var state = GetPlanetEconomyStatus(planetName);
        var entry = state switch
        {
            { ImperialExtraction: true, ResourceLevel: <= 20 } => $"Galaxy feed: {planetName} is being strip-mined by Imperial contracts; raw trade has collapsed.",
            { ImperialExtraction: true } => $"Galaxy feed: Imperial extraction teams expanded operations on {planetName}.",
            { ResourceLevel: <= 25 } => $"Galaxy feed: {planetName} merchants report severe resource scarcity.",
            { ResourceLevel: >= 80, TradeHealth: >= 70 } => $"Galaxy feed: {planetName} enters a trade boom with stable supply lanes.",
            _ => $"Galaxy feed: shipping lanes around {planetName} remain active under shifting faction pressure."
        };

        if (planetName.Equals(focusPlanet, StringComparison.OrdinalIgnoreCase))
        {
            entry += " (local impact)";
        }

        galaxyNews.Enqueue(entry);
        while (galaxyNews.Count > 40) galaxyNews.Dequeue();
    }

    private static string InferPlanetHabitatHint(string planetName, string economy)
    {
        var lower = (planetName + " " + economy).ToLowerInvariant();
        if (lower.Contains("hoth") || lower.Contains("ice") || lower.Contains("frozen")) return "ice";
        if (lower.Contains("tatooine") || lower.Contains("jakku") || lower.Contains("geonosis") || lower.Contains("desert")) return "desert";
        if (lower.Contains("dagobah") || lower.Contains("endor") || lower.Contains("kashyyyk") || lower.Contains("forest") || lower.Contains("swamp")) return "jungle";
        if (lower.Contains("kamino") || lower.Contains("mon cala") || lower.Contains("ocean") || lower.Contains("water")) return "ocean";
        if (lower.Contains("bespin") || lower.Contains("space") || lower.Contains("asteroid") || lower.Contains("nebula")) return "space";
        if (lower.Contains("mustafar") || lower.Contains("volcan")) return "volcanic";
        return "general";
    }

    private void InitializeMaterialEconomyAndRecipes()
    {
        InitializeMaterialCatalog();
        InitializeIndustrialFurnaces();
        AssignPlanetMaterialSources();
        GenerateBalancedRecipes();
        RefreshShipUpgradeRecipes();
        TagBlueprintRecipe("durasteel ingot", "durasteel metallurgy blueprint", "Earned through industrial quests or forge-aligned factions.", hideUntilKnown: true);
        TagBlueprintRecipe("reactor tuning kit", "reactor tuning kit", "Often awarded by engineering contracts.", hideUntilKnown: true);
        TagBlueprintRecipe("powerplant booster", "powerplant booster", "Recovered from advanced shipwright work.", hideUntilKnown: true);
        // Lock everything except starter whitelist — must run AFTER all recipes are generated
        TagAllItemsBlueprintLocked();
    }

    private void InitializeShipSystems()
    {
        ShipUpgradeCatalog.Clear();
        ShipArmaments.Clear();

        EnsureCraftableItem("expanded fuel tank", "ship-upgrade", "Adds deep-space fuel reserves.", "Engineering", 72);
        EnsureCraftableItem("auxiliary fuel pod", "ship-upgrade", "Carries extra reserve fuel for long jumps.", "Engineering", 58);
        EnsureCraftableItem("class 2 hyperdrive retrofit", "ship-upgrade", "Improves your hyperdrive for more efficient long-range travel.", "Engineering", 95);
        EnsureCraftableItem("class 1 hyperdrive retrofit", "ship-upgrade", "A top-tier hyperdrive core for elite jump performance.", "Engineering", 140);
        EnsureCraftableItem("navicomputer overclock", "ship-upgrade", "Improves route plotting and reduces jump time.", "Engineering", 78);
        EnsureCraftableItem("reactor tuning kit", "ship-upgrade", "Improves baseline reactor stability and power routing.", "Engineering", 86);
        EnsureCraftableItem("powerplant booster", "ship-upgrade", "Adds stronger powerplant output for heavy subsystems.", "Engineering", 98);
        EnsureCraftableItem("military spec reactor", "ship-upgrade", "A restricted military reactor with hardened output rails.", "Engineering", 155);
        EnsureCraftableItem("military spec powerplant", "ship-upgrade", "Frontline-grade powerplant hardware for combat ships.", "Engineering", 168);
        EnsureCraftableItem("military spec shield lattice", "ship-upgrade", "Combat shield lattice reserved for trusted fleets.", "Engineering", 150);

        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "shield booster",
            UpgradeKind = "defense",
            MinimumShipSize = "S",
            ShieldBonus = 6,
            Description = "Reinforces the shield envelope with a stronger projector bank."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "hyperdrive part",
            UpgradeKind = "fuel",
            MinimumShipSize = "S",
            FuelEfficiencyDeltaPercent = -8,
            Description = "Replaces worn hyperdrive assemblies to reduce fuel waste."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "laser upgrade",
            UpgradeKind = "weapon",
            MinimumShipSize = "S",
            WeaponOverride = "Enhanced laser battery",
            Description = "Improves your ship's stock laser emitters."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "fuel cell",
            UpgradeKind = "fuel",
            MinimumShipSize = "S",
            RefuelAmount = 30,
            Consumable = true,
            Unique = false,
            Description = "A replaceable fuel pack for short-range refueling."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "sensor array",
            UpgradeKind = "utility",
            MinimumShipSize = "S",
            TravelHoursModifier = -1,
            Description = "Improves ship tracking and route confirmation."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "hyperdrive stabilizer",
            UpgradeKind = "hyperdrive",
            MinimumShipSize = "S",
            FuelEfficiencyDeltaPercent = -10,
            Description = "Smooths hyperspace vector entry and lowers wasted fuel."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "armor plating",
            UpgradeKind = "defense",
            MinimumShipSize = "S",
            HullBonus = 8,
            Description = "Adds extra hull armor and reinforcement ribs."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "ion capacitor",
            UpgradeKind = "utility",
            MinimumShipSize = "S",
            FuelEfficiencyDeltaPercent = -5,
            ShieldBonus = 2,
            Description = "Balances reactor output across shields and engines."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "sensorscope",
            UpgradeKind = "utility",
            MinimumShipSize = "S",
            TravelHoursModifier = -1,
            Description = "Extends sensor reach for safer route planning."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "expanded fuel tank",
            UpgradeKind = "fuel",
            MinimumShipSize = "S",
            FuelCapacityBonus = 35,
            Description = "Adds an enlarged internal tank section for more range."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "auxiliary fuel pod",
            UpgradeKind = "fuel",
            MinimumShipSize = "S",
            FuelCapacityBonus = 20,
            FuelEfficiencyDeltaPercent = -5,
            Description = "A detachable reserve pod for extra fuel and better endurance."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "class 2 hyperdrive retrofit",
            UpgradeKind = "hyperdrive",
            MinimumShipSize = "S",
            HyperdriveClassTarget = 2,
            FuelEfficiencyDeltaPercent = -8,
            Description = "Refits your drive core to Class 2 jump performance."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "class 1 hyperdrive retrofit",
            UpgradeKind = "hyperdrive",
            MinimumShipSize = "S",
            HyperdriveClassTarget = 1,
            FuelEfficiencyDeltaPercent = -12,
            TravelHoursModifier = -1,
            Description = "An elite hyperdrive refit for top-tier jump speed."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "navicomputer overclock",
            UpgradeKind = "utility",
            MinimumShipSize = "S",
            TravelHoursModifier = -2,
            Description = "Overclocks astrogation calculations to shave hours off long jumps."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "reactor tuning kit",
            UpgradeKind = "reactor",
            MinimumShipSize = "S",
            FuelEfficiencyDeltaPercent = -6,
            ShieldBonus = 2,
            Description = "Tunes the ship reactor for steadier output under load."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "powerplant booster",
            UpgradeKind = "powerplant",
            MinimumShipSize = "M",
            FuelCapacityBonus = 16,
            ShieldBonus = 3,
            Description = "Bolsters powerplant reserves for heavier systems."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "military spec reactor",
            UpgradeKind = "reactor",
            MinimumShipSize = "M",
            FuelEfficiencyDeltaPercent = -12,
            TravelHoursModifier = -1,
            ShieldBonus = 4,
            Description = "A sealed military reactor core with high-output redundancy."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "military spec powerplant",
            UpgradeKind = "powerplant",
            MinimumShipSize = "M",
            FuelCapacityBonus = 26,
            ShieldBonus = 4,
            HullBonus = 4,
            Description = "Military surplus powerplant hardware for sustained combat operations."
        });
        RegisterShipUpgrade(new ShipUpgradeDefinition
        {
            Name = "military spec shield lattice",
            UpgradeKind = "defense",
            MinimumShipSize = "M",
            ShieldBonus = 9,
            HullBonus = 2,
            Description = "Layered combat shield webbing trusted by fleet captains."
        });

        RegisterShipArmament(new ShipArmamentData
        {
            Name = "Light Laser Cannon",
            Category = "laser",
            HardpointSize = "S",
            Era = "Old Republic",
            DamageRating = 8,
            Description = "A compact starfighter cannon for small hardpoints."
        });
        RegisterShipArmament(new ShipArmamentData
        {
            Name = "Twin Laser Cannons",
            Category = "laser",
            HardpointSize = "S",
            Era = "Old Republic",
            DamageRating = 10,
            Description = "Paired laser barrels for agile dogfighters and interceptors."
        });
        RegisterShipArmament(new ShipArmamentData
        {
            Name = "Autoblaster Turret",
            Category = "autoblaster",
            HardpointSize = "S",
            Era = "Original Trilogy",
            DamageRating = 11,
            FuelDraw = 1,
            Description = "A defensive turret built for anti-fighter suppression."
        });
        RegisterShipArmament(new ShipArmamentData
        {
            Name = "Proton Torpedo Pod",
            Category = "torpedo",
            HardpointSize = "M",
            Era = "Original Trilogy",
            DamageRating = 14,
            FuelDraw = 1,
            Description = "A medium hardpoint torpedo package for strike craft."
        });
        RegisterShipArmament(new ShipArmamentData
        {
            Name = "Concussion Missile Rack",
            Category = "missile",
            HardpointSize = "M",
            Era = "Clone Wars",
            DamageRating = 15,
            FuelDraw = 1,
            Description = "A concussion missile rack for assault ships and bombers."
        });
        RegisterShipArmament(new ShipArmamentData
        {
            Name = "Ion Pulse Turret",
            Category = "ion",
            HardpointSize = "M",
            Era = "Original Trilogy",
            DamageRating = 13,
            FuelDraw = 1,
            Description = "A disabling ion turret for medium hulls."
        });
        RegisterShipArmament(new ShipArmamentData
        {
            Name = "Heavy Turbolaser Battery",
            Category = "turbolaser",
            HardpointSize = "L",
            Era = "Original Trilogy",
            DamageRating = 22,
            FuelDraw = 3,
            Description = "A capital-scale heavy battery for corvettes and larger warships."
        });
        RegisterShipArmament(new ShipArmamentData
        {
            Name = "Quad Turbolaser Cluster",
            Category = "turbolaser",
            HardpointSize = "L",
            Era = "Clone Wars",
            DamageRating = 24,
            FuelDraw = 3,
            Description = "A quad mount for sustained broadside fire on large ships."
        });

        foreach (var ship in ShipCatalog.Values)
        {
            ship.SizeClass = InferShipSizeClass(ship);
            ship.HyperdriveClass = InferBaseHyperdriveClass(ship);
        }

        TagBlueprintRecipe("durasteel ingot", "durasteel metallurgy blueprint", "Earned through industrial quests or forge-aligned factions.", hideUntilKnown: true);
        TagBlueprintRecipe("reactor tuning kit", "reactor tuning kit", "Often awarded by engineering contracts.", hideUntilKnown: true);
        TagBlueprintRecipe("powerplant booster", "powerplant booster", "Recovered from advanced shipwright work.", hideUntilKnown: true);
        TagBlueprintRecipe("military spec reactor", "military spec reactor", "Military Spec blueprints come from elite contracts and trusted reputations.", hideUntilKnown: true);
        TagBlueprintRecipe("military spec powerplant", "military spec powerplant", "Military Spec blueprints come from elite contracts and trusted reputations.", hideUntilKnown: true);
        TagBlueprintRecipe("military spec shield lattice", "military spec shield lattice", "Military Spec blueprints come from elite contracts and trusted reputations.", hideUntilKnown: true);
    }

    private void EnsureCraftableItem(string name, string category, string description, string skill, int cost)
    {
        if (CraftableItems.ContainsKey(name)) return;
        CraftableItems[name] = new ItemBlueprint
        {
            Name = name,
            Category = category,
            Description = description,
            Skill = skill,
            Cost = cost
        };
    }

    private void RegisterShipUpgrade(ShipUpgradeDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.Name)) return;
        ShipUpgradeCatalog[definition.Name] = definition;
        if (!CraftableItems.ContainsKey(definition.Name))
        {
            CraftableItems[definition.Name] = new ItemBlueprint
            {
                Name = definition.Name,
                Category = "ship-upgrade",
                Description = definition.Description,
                Skill = "Engineering",
                Cost = Math.Max(30, definition.FuelCapacityBonus + definition.ShieldBonus + definition.HullBonus + (definition.HyperdriveClassTarget > 0 ? 40 : 20))
            };
        }
    }

    private void RegisterShipArmament(ShipArmamentData definition)
    {
        if (string.IsNullOrWhiteSpace(definition.Name)) return;
        var normalizedSize = NormalizeShipSize(definition.HardpointSize);
        definition.HardpointSize = normalizedSize;
        ShipArmaments[definition.Name] = definition;
        if (!CraftableItems.ContainsKey(definition.Name))
        {
            var baseCost = normalizedSize switch
            {
                "S" => 70,
                "M" => 120,
                _ => 220
            };

            CraftableItems[definition.Name] = new ItemBlueprint
            {
                Name = definition.Name,
                Category = "ship-armament",
                Description = definition.Description,
                Skill = "Engineering",
                Cost = Math.Max(baseCost, definition.DamageRating * 8)
            };
        }
    }

    private void RefreshShipUpgradeRecipes()
    {
        foreach (var upgrade in ShipUpgradeCatalog.Values)
        {
            var blueprint = CraftableItems[upgrade.Name];
            var inputs = new List<RecipeComponent>
            {
                new() { Item = "durasteel ingot", Quantity = Math.Max(1, blueprint.Cost / 45) },
                new() { Item = "power cell", Quantity = 1 }
            };

            if (upgrade.HyperdriveClassTarget > 0 || upgrade.Name.Contains("hyperdrive", StringComparison.OrdinalIgnoreCase))
            {
                inputs.Add(new RecipeComponent { Item = "hyperdrive part", Quantity = 1 });
                inputs.Add(new RecipeComponent { Item = "circuit board", Quantity = 1 });
            }

            if (upgrade.FuelCapacityBonus > 0 || upgrade.RefuelAmount > 0)
            {
                inputs.Add(new RecipeComponent { Item = "stabilized coaxium", Quantity = 1 });
            }

            if (upgrade.ShieldBonus > 0)
            {
                inputs.Add(new RecipeComponent { Item = "shield generator", Quantity = 1 });
            }

            if (upgrade.Name.Contains("reactor", StringComparison.OrdinalIgnoreCase) || upgrade.Name.Contains("powerplant", StringComparison.OrdinalIgnoreCase))
            {
                inputs.Add(new RecipeComponent { Item = "reactor", Quantity = 1 });
                inputs.Add(new RecipeComponent { Item = "ion capacitor core", Quantity = 1 });
            }

            Recipes[upgrade.Name] = new CraftRecipe
            {
                Output = upgrade.Name,
                CreditCost = Math.Max(8, blueprint.Cost / 5),
                TimeHours = Math.Max(1, blueprint.Cost / 22),
                Skill = blueprint.Skill,
                RequiresShipyard = true,
                RequiresIndustrialFurnace = upgrade.HullBonus > 0 || upgrade.ShieldBonus > 0,
                Inputs = inputs
                    .GroupBy(x => x.Item, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new RecipeComponent { Item = g.Key, Quantity = g.Sum(x => x.Quantity) })
                    .ToList(),
                Notes = "Ship upgrade recipe"
            };

            if (upgrade.Name.Contains("military spec", StringComparison.OrdinalIgnoreCase))
            {
                TagBlueprintRecipe(upgrade.Name, upgrade.Name, "Military Spec blueprints come from elite contracts and trusted reputations.", hideUntilKnown: true);
            }
        }

        foreach (var armament in ShipArmaments.Values)
        {
            var craftItem = CraftableItems[armament.Name];
            var sizeFactor = armament.HardpointSize switch
            {
                "S" => 1,
                "M" => 2,
                _ => 4
            };

            Recipes[armament.Name] = new CraftRecipe
            {
                Output = armament.Name,
                CreditCost = Math.Max(12, craftItem.Cost / 4),
                TimeHours = Math.Max(2, sizeFactor * 3),
                Skill = "Engineering",
                RequiresShipyard = armament.HardpointSize is "M" or "L",
                RequiresIndustrialFurnace = true,
                Inputs = new List<RecipeComponent>
                {
                    new() { Item = "durasteel ingot", Quantity = 2 * sizeFactor },
                    new() { Item = "power cell", Quantity = sizeFactor },
                    new() { Item = "pressurized tibanna", Quantity = Math.Max(1, sizeFactor - 1) },
                    new() { Item = "ion capacitor core", Quantity = armament.Category.Contains("ion", StringComparison.OrdinalIgnoreCase) ? 1 : 0 }
                }.Where(x => x.Quantity > 0).ToList(),
                Notes = $"Ship armament recipe ({armament.HardpointSize} hardpoint)"
            };
        }
    }

    // ── Items that are visible without any blueprint (starter knowledge) ────
    private static readonly HashSet<string> BlueprintWhitelist = new(StringComparer.OrdinalIgnoreCase)
    {
        "repair kit", "field medpack", "medpack", "emergency medpack",
        "ration bar", "dried rations", "bantha steak", "bantha jerky",
        "power cell", "refined metal", "hyperdrive part", "sensor array",
        "scrap metal", "junk parts",
        // Starter ship — always purchasable/craftable without blueprint
        "Skiff",
    };

    private void TagAllItemsBlueprintLocked()
    {
        // Tag every recipe that is NOT in the whitelist as hidden-until-blueprint-owned.
        foreach (var kv in Recipes)
        {
            if (BlueprintWhitelist.Contains(kv.Key)) continue;
            var recipe = kv.Value;
            recipe.RequiresBlueprint = true;
            if (string.IsNullOrWhiteSpace(recipe.BlueprintName))
                recipe.BlueprintName = kv.Key;
            if (string.IsNullOrWhiteSpace(recipe.BlueprintSourceHint))
                recipe.BlueprintSourceHint = "Acquire via quests, merchants, or exploration.";
            recipe.HideUntilBlueprintUnlocked = true;
        }
    }

    // ── Storyline quest chains ────────────────────────────────────────────
    private void InitStorylineChains()
    {
        StorylineChains.Clear();

        // ── Rebel Alliance chain ────────────────────────────────────────
        AddChain("rebel-intro", "A Spark of Rebellion", "Rebels", new[]
        {
            MakeChainStep("rebel-intro", 1, "Contact in the Shadows",
                "Rebels", "travel", "Dantooine", "Rebel Liaison", "Human", "Dantooine",
                200, 40, "", "blaster rifle blueprint",
                "A contact has surfaced on Dantooine. Get there quietly — Imperial patrols are active."),
            MakeChainStep("rebel-intro", 2, "Supply Run",
                "Rebels", "fetch", "Hoth", "Rebel Quartermaster", "Mon Calamari", "Hoth",
                250, 50, "ration bar", "ion grenade blueprint",
                "The base on Hoth is running low. Procure 2 supply crates and deliver them before the next patrol sweep.",
                objectiveTarget: "supply crate", objectiveRequired: 2),
            MakeChainStep("rebel-intro", 3, "Disruption Strike",
                "Rebels", "combat", "Lothal", "Commander Vela", "Human", "Lothal",
                400, 80, "", "x-wing fighter blueprint",
                "Strike the Imperial garrison on Lothal. Take down 4 stormtrooper squads to disrupt their operations.",
                objectiveTarget: "stormtrooper", objectiveRequired: 4),
        });

        // ── Smuggler's Guild chain ──────────────────────────────────────
        AddChain("smuggler-intro", "Running the Gauntlet", "Smugglers", new[]
        {
            MakeChainStep("smuggler-intro", 1, "First Job",
                "Smugglers", "fetch", "Corellia", "Dockmaster Greev", "Rodian", "Corellia",
                150, 30, "credits chip", "vibroblade blueprint",
                "Greev needs a crate of tibanna gas moved through Corellian customs, no questions asked.",
                objectiveTarget: "tibanna gas canister", objectiveRequired: 1),
            MakeChainStep("smuggler-intro", 2, "Hot Cargo",
                "Smugglers", "travel", "Bespin", "Lady Mira", "Twi'lek", "Bespin",
                300, 55, "", "YT-1300 freighter blueprint",
                "Mira has something big waiting at Bespin. Get there before the Empire does."),
            MakeChainStep("smuggler-intro", 3, "The Heist",
                "Smugglers", "combat", "Tatooine", "Cutter", "Zabrak", "Tatooine",
                500, 90, "", "smuggler holdout pistol blueprint",
                "Jabba's men jumped the deal. Fight off 3 Hutt enforcers and recover the cargo.",
                objectiveTarget: "Hutt enforcer", objectiveRequired: 3),
        });

        // ── Mandalorian clan chain ──────────────────────────────────────
        AddChain("mandalorian-intro", "The Way of the Mandalore", "Mandalorians", new[]
        {
            MakeChainStep("mandalorian-intro", 1, "Prove Your Worth",
                "Mandalorians", "combat", "Mandalore", "Alor Kaden", "Mandalorian", "Mandalore",
                180, 35, "", "beskad blueprint",
                "To earn clan trust you must defeat 3 challengers in the proving grounds.",
                objectiveTarget: "challenger", objectiveRequired: 3),
            MakeChainStep("mandalorian-intro", 2, "The Forge Test",
                "Mandalorians", "fetch", "Mandalore", "Armorsmith Draal", "Mandalorian", "Mandalore",
                220, 45, "", "beskar pauldron blueprint",
                "Bring 2 beskar ore samples to the forge. The armorsmith will evaluate your commitment.",
                objectiveTarget: "beskar ore", objectiveRequired: 2),
            MakeChainStep("mandalorian-intro", 3, "Clan Hunt",
                "Mandalorians", "combat", "Tatooine", "Alor Kaden", "Mandalorian", "Tatooine",
                450, 85, "beskar ingot", "mandalorian helmet blueprint",
                "Track and defeat 4 Sand People warlords terrorising the clan's trade route.",
                objectiveTarget: "Sand People warlord", objectiveRequired: 4),
        });

        // ── Imperial contracts chain ────────────────────────────────────
        AddChain("imperial-intro", "Service to the Empire", "Empire", new[]
        {
            MakeChainStep("imperial-intro", 1, "Imperial Requisition",
                "Empire", "fetch", "Coruscant", "Moff Brennan", "Human", "Coruscant",
                160, 30, "", "E-11 blaster rifle blueprint",
                "The Moff requires 2 power cell modules delivered to the garrison before the inspection.",
                objectiveTarget: "power cell module", objectiveRequired: 2),
            MakeChainStep("imperial-intro", 2, "Pacification Order",
                "Empire", "combat", "Ryloth", "Lieutenant Vaas", "Human", "Ryloth",
                320, 60, "", "stormtrooper armor blueprint",
                "Crush the Ryloth insurgency — eliminate 3 rebel cells operating in the mining districts.",
                objectiveTarget: "rebel cell", objectiveRequired: 3),
            MakeChainStep("imperial-intro", 3, "Classified Retrieval",
                "Empire", "travel", "Dantooine", "Moff Brennan", "Human", "Dantooine",
                500, 95, "", "imperial TIE fighter blueprint",
                "A classified data core went missing near the old rebel base on Dantooine. Retrieve it."),
        });

        // ── Jedi order chain ───────────────────────────────────────────
        AddChain("jedi-intro", "The Force Calls", "Jedi", new[]
        {
            MakeChainStep("jedi-intro", 1, "Awakening",
                "Jedi", "travel", "Dagobah", "Master Elrin", "Mirialan", "Dagobah",
                100, 50, "", "lightsaber hilt blueprint",
                "A Jedi Master senses something in you. Journey to Dagobah to meet them."),
            MakeChainStep("jedi-intro", 2, "Finding the Crystal",
                "Jedi", "fetch", "Dantooine", "Master Elrin", "Mirialan", "Dantooine",
                150, 60, "", "kyber crystal blueprint",
                "The ruins near Dantooine hold a kyber crystal aligned to your Force signature. Recover it.",
                objectiveTarget: "kyber crystal", objectiveRequired: 1),
            MakeChainStep("jedi-intro", 3, "The Trial of Skill",
                "Jedi", "combat", "Dagobah", "Master Elrin", "Mirialan", "Dagobah",
                200, 100, "", "lightsaber assembly blueprint",
                "Face 3 Force-enhanced guardian constructs in the cave — a final test before you are acknowledged as a Padawan.",
                objectiveTarget: "guardian construct", objectiveRequired: 3),
        });
    }

    private void AddChain(string chainId, string title, string faction, Quest[] steps)
    {
        var chain = new QuestChain { Id = chainId, Title = title, Faction = faction };
        for (int i = 0; i < steps.Length; i++)
        {
            steps[i].Id = $"{chainId}-step{i + 1}";
            if (i < steps.Length - 1)
                steps[i].ChainNextStep = $"{chainId}-step{i + 2}";
        }
        chain.Steps.AddRange(steps);
        StorylineChains[chainId] = chain;
    }

    private static Quest MakeChainStep(string chainId, int step, string title,
        string faction, string objectiveType, string targetPlanet,
        string issuerName, string issuerSpecies, string issuerPlanet,
        int credits, int xp, string rewardItem, string rewardBlueprint,
        string description, string objectiveTarget = "", int objectiveRequired = 1)
        => new Quest
        {
            ChainId = chainId, ChainStep = step, Title = title,
            Faction = faction, ObjectiveType = objectiveType, TargetPlanet = targetPlanet,
            IssuerName = issuerName, IssuerSpecies = issuerSpecies, IssuerPlanet = issuerPlanet,
            RewardCredits = credits, RewardXp = xp,
            RewardItem = rewardItem, RewardBlueprint = rewardBlueprint,
            Description = description,
            ObjectiveTarget = objectiveTarget, ObjectiveRequired = Math.Max(1, objectiveRequired),
            Status = "inactive",
        };

    /// <summary>Offer the first step of a storyline chain, or the next pending step if already started.</summary>
    public bool TryOfferChainQuest(string chainId, GameCharacter character, out Quest? quest, out string message)
    {
        quest = null; message = string.Empty;
        if (!StorylineChains.TryGetValue(chainId, out var chain)) return false;

        // find already-active chain step
        var active = ActiveQuests.FirstOrDefault(q => q.ChainId == chainId && q.Status is "active" or "ready");
        if (active != null) { message = $"You already have an active step in \"{chain.Title}\"."; return false; }

        // find next step to offer
        var completed = ActiveQuests.Where(q => q.ChainId == chainId && q.Status == "completed").Select(q => q.ChainStep).ToHashSet();
        var nextStep  = chain.Steps.OrderBy(s => s.ChainStep).FirstOrDefault(s => !completed.Contains(s.ChainStep));
        if (nextStep is null) { message = $"You have completed the chain \"{chain.Title}\"."; return false; }

        if (ActiveQuests.Count(q => q.Status is "active" or "ready") >= 5)
        { message = "Datapad full — finish current missions first."; return false; }

        var clone = CloneQuest(nextStep);
        clone.Status = "active";
        ActiveQuests.Add(clone);
        AdjustFactionStanding(clone.Faction, 1);
        quest = clone;
        message = $"[CHAIN — {chain.Title} Step {clone.ChainStep}] {clone.IssuerName}: \"{clone.Description}\"";
        return true;
    }

    private Quest CloneQuest(Quest src) => new Quest
    {
        Id = src.Id, Title = src.Title, Description = src.Description,
        TargetPlanet = src.TargetPlanet, Faction = src.Faction, Status = src.Status,
        RewardCredits = src.RewardCredits, RewardXp = src.RewardXp,
        RewardItem = src.RewardItem, RewardBlueprint = src.RewardBlueprint,
        ObjectiveType = src.ObjectiveType, ObjectiveTarget = src.ObjectiveTarget,
        ObjectiveZone = src.ObjectiveZone, ObjectiveRequired = src.ObjectiveRequired,
        ObjectiveProgress = 0, IssuerName = src.IssuerName,
        IssuerSpecies = src.IssuerSpecies, IssuerPlanet = src.IssuerPlanet,
        IsNpcGenerated = false, ChainId = src.ChainId, ChainStep = src.ChainStep,
        ChainNextStep = src.ChainNextStep
    };

    public string GetQuestLogDisplay(GameCharacter character)
    {
        var sb = new System.Text.StringBuilder();

        // ── active / ready ──────────────────────────────────────────────
        var active = ActiveQuests.Where(q => q.Status is "active" or "ready").OrderBy(q => q.ChainId).ThenBy(q => q.ChainStep).ToList();
        if (active.Count == 0)
            sb.AppendLine("  (no active quests — speak with NPCs to receive contracts)");
        else
        {
            foreach (var q in active)
            {
                var chainTag = string.IsNullOrEmpty(q.ChainId) ? "" : $"  [Chain: {q.ChainId} Step {q.ChainStep}]";
                sb.AppendLine($"▶ {q.Title}{chainTag}");
                sb.AppendLine($"   {q.Description}");
                var prog = q.ObjectiveType == "travel"
                    ? (string.Equals(character.Location, q.TargetPlanet, StringComparison.OrdinalIgnoreCase) ? "Arrived ✓" : $"Travel to {q.TargetPlanet}")
                    : $"{q.ObjectiveProgress}/{q.ObjectiveRequired}";
                var locationHint = new List<string>();
                if (!string.IsNullOrWhiteSpace(q.TargetPlanet))  locationHint.Add($"Planet: {q.TargetPlanet}");
                if (!string.IsNullOrWhiteSpace(q.ObjectiveZone)) locationHint.Add($"Zone: {q.ObjectiveZone}");
                sb.AppendLine($"   Objective ({q.ObjectiveType}): {prog}   Faction: {q.Faction}");
                if (locationHint.Count > 0) sb.AppendLine($"   Location → {string.Join("  |  ", locationHint)}");
                var rewards = new List<string>();
                if (q.RewardCredits > 0) rewards.Add($"{q.RewardCredits} credits");
                if (q.RewardXp > 0)     rewards.Add($"{q.RewardXp} XP");
                if (!string.IsNullOrEmpty(q.RewardItem))      rewards.Add(q.RewardItem);
                if (!string.IsNullOrEmpty(q.RewardBlueprint)) rewards.Add($"Blueprint: {q.RewardBlueprint}");
                sb.AppendLine($"   Rewards: {string.Join(" | ", rewards)}");
                sb.AppendLine();
            }
        }

        // ── available chain starts ──────────────────────────────────────
        sb.AppendLine("── Available Story Chains ──────────────────────────────────");
        foreach (var chain in StorylineChains.Values)
        {
            bool anyActive    = active.Any(q => q.ChainId == chain.Id);
            bool anyCompleted = ActiveQuests.Any(q => q.ChainId == chain.Id && q.Status == "completed");
            bool allDone      = chain.Steps.All(s => ActiveQuests.Any(q => q.ChainId == chain.Id && q.ChainStep == s.ChainStep && q.Status == "completed"));
            string state = allDone ? "✓ Completed" : anyActive ? "In Progress" : anyCompleted ? "Next step available" : "Not started";
            sb.AppendLine($"  {chain.Title}  [{chain.Faction}]  — {state}");
        }

        // ── recently completed ──────────────────────────────────────────
        var recent = ActiveQuests.Where(q => q.Status == "completed").TakeLast(5).ToList();
        if (recent.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("── Recently Completed ───────────────────────────────────────");
            foreach (var q in recent)
                sb.AppendLine($"  ✓ {q.Title}   [{q.Faction}]");
        }

        return sb.ToString();
    }

    public IReadOnlyList<QuestChain> GetAllChains() => StorylineChains.Values.ToList();

    private void TagBlueprintRecipe(string outputName, string blueprintName, string sourceHint, bool hideUntilKnown)    {
        if (!Recipes.TryGetValue(outputName, out var recipe)) return;
        recipe.RequiresBlueprint = true;
        recipe.BlueprintName = blueprintName;
        recipe.BlueprintSourceHint = sourceHint;
        recipe.HideUntilBlueprintUnlocked = hideUntilKnown;
    }

    private void InitializeSpaceStations()
    {
        SpaceStations.Clear();

        // Kuat Drive Yards — Imperial/Republic capital ships
        SpaceStations["Kuat Drive Yards Orbital Ring"] = new SpaceStation
        {
            Name = "Kuat Drive Yards Orbital Ring",
            OrbitingPlanet = "Kuat",
            Description = "The most famous orbital shipyard in the galaxy. Produces Star Destroyers, Victory-class warships, and heavy military vessels under contract to the Empire and Republic alike.",
            Region = "Core Worlds",
            Manufacturer = "Kuat Drive Yards",
            HasShipyard = true,
            HasMerchant = true,
            TravelCostFromPlanet = 4,
            LinerFeeFromGalaxy = 320
        };

        // CEC Orbital Shipyards — Corellian freighters and fighters
        SpaceStations["CEC Orbital Shipyards"] = new SpaceStation
        {
            Name = "CEC Orbital Shipyards",
            OrbitingPlanet = "Corellia",
            Description = "The production home of the iconic YT-series freighters and CR-series corvettes. Attracts independent captains and smugglers galaxy-wide.",
            Region = "Core Worlds",
            Manufacturer = "Corellian Engineering Corporation",
            HasShipyard = true,
            HasMerchant = true,
            TravelCostFromPlanet = 3,
            LinerFeeFromGalaxy = 200
        };

        // Mon Calamari — cruisers and capital ships for the Rebellion
        SpaceStations["Mon Calamari Orbital Platform"] = new SpaceStation
        {
            Name = "Mon Calamari Orbital Platform",
            OrbitingPlanet = "Mon Cala",
            Description = "The Mon Calamari's proudly-built orbital shipyard, home of the MC80 and MC90 cruisers that formed the backbone of the Rebel fleet.",
            Region = "Mid Rim",
            Manufacturer = "Mon Calamari Shipyards",
            HasShipyard = true,
            HasMerchant = true,
            TravelCostFromPlanet = 4,
            LinerFeeFromGalaxy = 280
        };

        // Rendili StarDrive — Mid Rim neutral manufacturer
        SpaceStations["Rendili StarDrive Platform"] = new SpaceStation
        {
            Name = "Rendili StarDrive Platform",
            OrbitingPlanet = "Coruscant",
            Description = "A mid-tier shipyard operated by Rendili StarDrive, producing frigates and mid-class warships. Open to independent contracts.",
            Region = "Core Worlds",
            Manufacturer = "Rendili StarDrive",
            HasShipyard = true,
            HasMerchant = false,
            TravelCostFromPlanet = 5,
            LinerFeeFromGalaxy = 240
        };

        // Sienar Fleet Systems — TIE variants and Imperial fighters
        SpaceStations["Sienar Fleet Systems Depot"] = new SpaceStation
        {
            Name = "Sienar Fleet Systems Depot",
            OrbitingPlanet = "Coruscant",
            Description = "Production depot for TIE fighters and Imperial starfighters. Heavily militarized — faction access required for most models.",
            Region = "Core Worlds",
            Manufacturer = "Sienar Fleet Systems",
            HasShipyard = true,
            HasMerchant = false,
            TravelCostFromPlanet = 6,
            LinerFeeFromGalaxy = 260
        };

        // Incom Corporation — X-Wing and civilian craft
        SpaceStations["Incom Corporation Yard"] = new SpaceStation
        {
            Name = "Incom Corporation Yard",
            OrbitingPlanet = "Corellia",
            Description = "Builder of the T-65 X-Wing and other civilian/military fighters. Accessible to most factions.",
            Region = "Core Worlds",
            Manufacturer = "Incom Corporation",
            HasShipyard = true,
            HasMerchant = true,
            TravelCostFromPlanet = 5,
            LinerFeeFromGalaxy = 220
        };

        // Register orbital station links on parent planets
        foreach (var station in SpaceStations.Values)
        {
            if (!string.IsNullOrWhiteSpace(station.OrbitingPlanet)
                && Planets.TryGetValue(station.OrbitingPlanet, out var planet))
            {
                if (!planet.OrbitalStations.Contains(station.Name))
                    planet.OrbitalStations.Add(station.Name);
            }
        }
    }

    // Returns the ships available for purchase at this location (planet or space station).
    // Filters by Manufacturer if the location has one; otherwise returns the full catalog.
    public IReadOnlyList<ShipBlueprint> GetShipyardCatalog(string locationName)
    {
        bool isOrbital = SpaceStations.TryGetValue(locationName, out var station);
        string manufacturer = "";

        if (isOrbital)
        {
            if (!station!.HasShipyard) return Array.Empty<ShipBlueprint>();
            manufacturer = station.Manufacturer;
        }
        else if (Planets.TryGetValue(locationName, out var planet))
        {
            if (!planet.HasDockyard) return Array.Empty<ShipBlueprint>();
            manufacturer = planet.ShipyardManufacturer;
        }
        else
        {
            return Array.Empty<ShipBlueprint>();
        }

        // Orbital stations stock the full range; planetary dockyards only carry planetary-tier ships
        var pool = ShipCatalog.Values
            .Where(s => isOrbital || string.Equals(s.ShipyardTier, "planetary", StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.PurchasePrice > 0 ? s.PurchasePrice : s.Cost);

        if (string.IsNullOrWhiteSpace(manufacturer))
            return pool.ToList();

        var filtered = pool.Where(s => ManufacturerMatchesShip(manufacturer, s)).ToList();
        return filtered.Count > 0 ? filtered : pool.ToList();
    }

    private static bool ManufacturerMatchesShip(string manufacturer, ShipBlueprint ship)
    {
        var mfr = manufacturer.ToLowerInvariant();
        var name = (ship.Name + " " + ship.Description + " " + ship.Model).ToLowerInvariant();

        return mfr switch
        {
            var m when m.Contains("kuat") =>
                name.Contains("star destroyer") || name.Contains("victory") || name.Contains("acclamator") || name.Contains("imperial") || name.Contains("venator"),

            var m when m.Contains("corellian engineering") || m.Contains("cec") =>
                name.Contains("yt-") || name.Contains("corellian") || name.Contains("millennium falcon") || name.Contains("razor crest") || name.Contains("skiff"),

            var m when m.Contains("mon calamari") =>
                name.Contains("mon cal") || name.Contains("mc80") || name.Contains("mc90") || name.Contains("b-wing"),

            var m when m.Contains("sienar") =>
                name.Contains("tie") || name.Contains("sienar"),

            var m when m.Contains("incom") =>
                name.Contains("x-wing") || name.Contains("u-wing") || name.Contains("t-65") || name.Contains("t-16") || name.Contains("snowspeeder"),

            var m when m.Contains("rendili") =>
                name.Contains("corvette") || name.Contains("frigate") || name.Contains("hammerhead") || name.Contains("rendili"),

            _ => false
        };
    }

    // Travel to a space station (requires a ship and fuel like planet travel)
    public (bool success, string message) TravelToStation(GameCharacter character, string stationName)
    {
        if (character.Ship is null)
            return (false, "You need a ship to travel to a space station.");

        if (!SpaceStations.TryGetValue(stationName, out var station))
            return (false, "That space station is not on record.");

        // Cost is base planet travel to the orbiting planet + small additional hop
        var orbitPlanet = station.OrbitingPlanet;
        var baseCost = Planets.TryGetValue(orbitPlanet, out _)
            ? GetTravelFuelCost(character, orbitPlanet) + station.TravelCostFromPlanet
            : station.TravelCostFromPlanet * 3;

        if (character.Ship.Fuel < baseCost)
            return (false, $"Not enough fuel to reach {stationName} (need {baseCost}, have {character.Ship.Fuel}).");

        character.Ship.Fuel -= baseCost;
        character.Credits = Math.Max(0, character.Credits - Math.Max(1, baseCost / 8));
        var hours = Math.Max(2, GetTravelHours(character, orbitPlanet));
        AdvanceWorldTime(hours, orbitPlanet, character);
        character.Location = stationName;
        character.Experience += 5;
        MarkPlanetDiscovered(stationName);
        return (true, $"You dock at {stationName} after {hours}h. Fuel remaining: {character.Ship.Fuel}/{character.Ship.MaxFuel}.");
    }

    // Travel via commercial liner — no ship required, costs credits, longer trip
    public (bool success, string message) TravelByLiner(GameCharacter character, string destination)
    {
        int fee;
        int hours;

        if (SpaceStations.TryGetValue(destination, out var station))
        {
            fee = station.LinerFeeFromGalaxy;
            hours = 24 + random.Next(0, 13); // 24–36 hours
        }
        else if (Planets.TryGetValue(destination, out var planet))
        {
            fee = Math.Max(80, planet.TravelCost * 8); // scaled commercial fare
            hours = 18 + random.Next(0, 19); // 18–36 hours
        }
        else
        {
            return (false, "That destination is not known to commercial liner services.");
        }

        if (character.Credits < fee)
            return (false, $"The liner fare to {destination} is {fee} credits. You only have {character.Credits}.");

        character.Credits -= fee;
        AdvanceWorldTime(hours, destination, character);
        character.Location = destination;
        character.Experience += 3;
        MarkPlanetDiscovered(destination);
        return (true, $"You board a commercial liner and arrive at {destination} after {hours}h. Fare paid: {fee} credits.");
    }

    private static string NormalizeShipSize(string sizeClass)
    {
        var normalized = (sizeClass ?? "S").Trim().ToUpperInvariant();
        return normalized is "S" or "M" or "L" ? normalized : "S";
    }

    private static int GetShipSizeRank(string sizeClass)
        => NormalizeShipSize(sizeClass) switch
        {
            "M" => 2,
            "L" => 3,
            _ => 1
        };

    private static string InferShipSizeClass(ShipBlueprint ship)
    {
        if (ship.IsCapital || ship.CrewCapacity >= 12 || ship.Hull >= 36 || ship.Cost >= 500) return "L";
        if (ship.CrewCapacity >= 4 || ship.Hull >= 20 || ship.Cost >= 240) return "M";
        return "S";
    }

    private static int InferBaseHyperdriveClass(ShipBlueprint ship)
        => InferShipSizeClass(ship) switch
        {
            "S" => 2,
            "M" => 3,
            _ => 4
        };

    private static Dictionary<string, int> GetHardpointCapacity(string sizeClass)
    {
        return NormalizeShipSize(sizeClass) switch
        {
            "M" => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["S"] = 2, ["M"] = 2, ["L"] = 0 },
            "L" => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["S"] = 2, ["M"] = 3, ["L"] = 2 },
            _ => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["S"] = 2, ["M"] = 0, ["L"] = 0 }
        };
    }

    private Dictionary<string, int> GetHardpointUsage(Ship ship)
    {
        EnsureShipSystemsInitialized(ship);
        var usage = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["S"] = 0,
            ["M"] = 0,
            ["L"] = 0
        };

        foreach (var slot in ship.HardpointSlots.Where(x => !string.IsNullOrWhiteSpace(x.MountedArmament)))
        {
            usage[NormalizeShipSize(slot.SlotSize)] += 1;
        }

        return usage;
    }

    public string GetShipHardpointSummary(Ship? ship)
    {
        if (ship is null) return "No ship";
        EnsureShipSystemsInitialized(ship);
        var capacity = GetHardpointCapacity(ship.SizeClass);
        var usage = GetHardpointUsage(ship);
        var slotSummary = ship.HardpointSlots.Count == 0
            ? string.Empty
            : " | " + string.Join(", ", ship.HardpointSlots.Select(slot =>
            {
                var slotDisplay = string.IsNullOrWhiteSpace(slot.MountedArmament) ? "empty" : slot.MountedArmament;
                return $"{slot.SlotName}:{slotDisplay}";
            }));
        var s = usage["S"]; var sc = capacity["S"];
        var m = usage["M"]; var mc = capacity["M"];
        var l = usage["L"]; var lc = capacity["L"];
        return $"S {s}/{sc} | M {m}/{mc} | L {l}/{lc}{slotSummary}";
    }

    private string BuildShipWeaponSummary(Ship ship)
    {
        EnsureShipSystemsInitialized(ship);
        var mounted = ship.HardpointSlots
            .Where(x => !string.IsNullOrWhiteSpace(x.MountedArmament))
            .Select(x => $"{x.Position}:{x.MountedArmament}")
            .ToList();
        return mounted.Count == 0
            ? ship.BaseWeapon
            : ship.BaseWeapon + " + " + string.Join(", ", mounted);
    }

    public void EnsureShipSystemsInitialized(Ship ship)
    {
        ship.Upgrades ??= new List<string>();
        ship.Armaments ??= new List<string>();
        ship.Parts ??= new List<string>();
        ship.HardpointSlots ??= new List<ShipHardpointSlot>();

        if (ship.HardpointSlots.Count > 0) return;

        ship.HardpointSlots = BuildHardpointSlots(ship.SizeClass, ship.Model);
        var pendingArmaments = ship.Armaments.ToList();
        ship.Armaments.Clear();
        foreach (var armament in pendingArmaments)
        {
            var slot = GetCompatibleHardpointSlots(ship, armament).FirstOrDefault();
            if (slot is null) continue;
            slot.MountedArmament = armament;
            ship.Armaments.Add(armament);
        }
    }

    private List<ShipHardpointSlot> BuildHardpointSlots(string sizeClass, string model)
    {
        var slots = new List<ShipHardpointSlot>
        {
            new() { SlotName = "Nose-1", Position = "Nose", SlotSize = "S", AllowedCategories = new List<string> { "laser", "ion", "autoblaster", "blaster" } },
            new() { SlotName = "Dorsal-1", Position = "Dorsal", SlotSize = "S", AllowedCategories = new List<string> { "laser", "autoblaster", "ion", "turret" } }
        };

        if (NormalizeShipSize(sizeClass) is "M" or "L")
        {
            slots.Add(new ShipHardpointSlot { SlotName = "Port-1", Position = "Port", SlotSize = "M", AllowedCategories = new List<string> { "torpedo", "missile", "ion", "laser" } });
            slots.Add(new ShipHardpointSlot { SlotName = "Starboard-1", Position = "Starboard", SlotSize = "M", AllowedCategories = new List<string> { "torpedo", "missile", "ion", "laser" } });
        }

        if (NormalizeShipSize(sizeClass) == "L")
        {
            slots.Add(new ShipHardpointSlot { SlotName = "Ventral-1", Position = "Ventral", SlotSize = "L", AllowedCategories = new List<string> { "turbolaser", "ion", "battery" } });
            slots.Add(new ShipHardpointSlot { SlotName = "Broadside-1", Position = "Broadside", SlotSize = "L", AllowedCategories = new List<string> { "turbolaser", "battery" } });
            slots.Add(new ShipHardpointSlot { SlotName = "Broadside-2", Position = "Broadside", SlotSize = "M", AllowedCategories = new List<string> { "laser", "ion", "missile" } });
        }

        if (model.Contains("Falcon", StringComparison.OrdinalIgnoreCase) || model.Contains("YT-1300", StringComparison.OrdinalIgnoreCase))
        {
            slots.Add(new ShipHardpointSlot { SlotName = "Turret-Top", Position = "Dorsal", SlotSize = "M", AllowedCategories = new List<string> { "laser", "ion", "autoblaster" } });
            slots.Add(new ShipHardpointSlot { SlotName = "Turret-Bottom", Position = "Ventral", SlotSize = "M", AllowedCategories = new List<string> { "laser", "ion", "autoblaster" } });
        }

        return slots;
    }

    public IReadOnlyList<ShipHardpointSlot> GetCompatibleHardpointSlots(Ship ship, string armamentName)
    {
        if (!ShipArmaments.TryGetValue(armamentName, out var armament)) return Array.Empty<ShipHardpointSlot>();
        EnsureShipSystemsInitialized(ship);

        return ship.HardpointSlots
            .Where(slot => string.IsNullOrWhiteSpace(slot.MountedArmament))
            .Where(slot => GetShipSizeRank(slot.SlotSize) >= GetShipSizeRank(armament.HardpointSize))
            .Where(slot => slot.AllowedCategories.Count == 0 || slot.AllowedCategories.Any(cat => string.Equals(cat, armament.Category, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private Ship CreateShipFromBlueprint(ShipBlueprint blueprint, string ownerSeed)
    {
        var ship = new Ship
        {
            Name = blueprint.Name,
            Model = blueprint.Model,
            Ascii = new List<string>(blueprint.Ascii),
            Hull = blueprint.Hull,
            Shield = blueprint.Shield,
            CrewCapacity = blueprint.CrewCapacity,
            Fuel = blueprint.Fuel,
            MaxFuel = blueprint.Fuel,
            SizeClass = blueprint.SizeClass,
            HyperdriveClass = blueprint.HyperdriveClass,
            FuelEfficiencyPercent = 100,
            BaseWeapon = blueprint.Weapon,
            Weapon = blueprint.Weapon,
            Parts = new List<string>(blueprint.RequiredParts),
            Upgrades = new List<string>(),
            Armaments = new List<string>(),
            HardpointSlots = BuildHardpointSlots(blueprint.SizeClass, blueprint.Model)
        };

        RollShipStats(ship, ownerSeed);
        ship.Weapon = BuildShipWeaponSummary(ship);
        return ship;
    }

    private void RollShipStats(Ship ship, string ownerSeed)
    {
        var seed = Math.Abs(StringComparer.OrdinalIgnoreCase.GetHashCode($"{ownerSeed}|{ship.Model}|{Clock.Rotation}|{ship.SizeClass}"));
        var variance = new Random(seed);
        switch (NormalizeShipSize(ship.SizeClass))
        {
            case "S":
                ship.Hull = Math.Max(8, ship.Hull + variance.Next(-2, 3));
                ship.Shield = Math.Max(2, ship.Shield + variance.Next(-1, 3));
                ship.MaxFuel = Math.Max(35, ship.MaxFuel + variance.Next(-8, 10));
                ship.HyperdriveClass = Math.Clamp(ship.HyperdriveClass + variance.Next(-1, 1), 1, 4);
                ship.FuelEfficiencyPercent = Math.Clamp(100 + variance.Next(-6, 7), 82, 112);
                ship.TravelHoursModifier = variance.Next(-1, 2);
                break;
            case "M":
                ship.Hull = Math.Max(16, ship.Hull + variance.Next(-3, 5));
                ship.Shield = Math.Max(6, ship.Shield + variance.Next(-2, 4));
                ship.MaxFuel = Math.Max(80, ship.MaxFuel + variance.Next(-12, 16));
                ship.HyperdriveClass = Math.Clamp(ship.HyperdriveClass + variance.Next(-1, 2), 2, 5);
                ship.FuelEfficiencyPercent = Math.Clamp(100 + variance.Next(-8, 9), 78, 118);
                ship.TravelHoursModifier = variance.Next(-1, 3);
                break;
            default:
                ship.Hull = Math.Max(30, ship.Hull + variance.Next(-4, 7));
                ship.Shield = Math.Max(12, ship.Shield + variance.Next(-3, 5));
                ship.MaxFuel = Math.Max(140, ship.MaxFuel + variance.Next(-18, 24));
                ship.HyperdriveClass = Math.Clamp(ship.HyperdriveClass + variance.Next(0, 2), 3, 6);
                ship.FuelEfficiencyPercent = Math.Clamp(100 + variance.Next(-10, 11), 74, 120);
                ship.TravelHoursModifier = variance.Next(0, 4);
                break;
        }

        ship.Fuel = ship.MaxFuel;
    }

    public int GetTravelFuelCost(GameCharacter character, string planetName)
    {
        if (character.Ship is null || !Planets.ContainsKey(planetName)) return 0;
        var baseCost = Planets[planetName].TravelCost;
        var scaled = baseCost * (character.Ship.HyperdriveClass / 3.0) * (character.Ship.FuelEfficiencyPercent / 100.0);
        return Math.Max(4, (int)Math.Ceiling(scaled));
    }

    public int GetTravelHours(GameCharacter character, string planetName)
    {
        if (character.Ship is null || !Planets.ContainsKey(planetName)) return 0;
        return Math.Max(2, 4 + character.Ship.HyperdriveClass + character.Ship.TravelHoursModifier);
    }

    private void InitializeIndustrialFurnaces()
    {
        foreach (var planet in Planets.Values)
        {
            var eco = planet.Economy ?? string.Empty;
            var heavyIndustry = eco.Contains("refinery", StringComparison.OrdinalIgnoreCase)
                || eco.Contains("mining", StringComparison.OrdinalIgnoreCase)
                || eco.Contains("manufacturing", StringComparison.OrdinalIgnoreCase)
                || eco.Contains("industry", StringComparison.OrdinalIgnoreCase)
                || eco.Contains("foundry", StringComparison.OrdinalIgnoreCase)
                || eco.Contains("shipyard", StringComparison.OrdinalIgnoreCase);

            planet.HasIndustrialFurnace = planet.HasDockyard || heavyIndustry;
        }
    }

    private void InitializeMaterialCatalog()
    {
        RawMaterials.Clear();
        RefinedMaterials.Clear();
        RefiningMap.Clear();

        var pairs = new (string raw, string refined)[]
        {
            ("raw ore", "refined metal"),
            ("raw durasteel", "durasteel ingot"),
            ("raw tibanna gas", "pressurized tibanna"),
            ("raw coaxium", "stabilized coaxium"),
            ("raw kyber shard", "cut kyber crystal"),
            ("raw neutronium", "neutronium plate"),
            ("raw phrik", "phrik alloy"),
            ("raw beskar ore", "beskar alloy"),
            ("raw carbonite", "carbonite slab"),
            ("raw ionite", "ion capacitor core"),
            ("raw crystal fiber", "composite weave"),
            ("raw bacta reagents", "refined bacta"),
            ("raw polymer sap", "polymer resin"),
            ("raw power salts", "energy compound")
        };

        foreach (var (raw, refined) in pairs)
        {
            RawMaterials.Add(raw);
            RefinedMaterials.Add(refined);
            RefiningMap[raw] = refined;

            if (!CraftableItems.ContainsKey(raw))
            {
                CraftableItems[raw] = new ItemBlueprint
                {
                    Name = raw,
                    Category = "raw-material",
                    Description = "Unrefined resource harvested from planetary biomes.",
                    Skill = "Survival",
                    Cost = 4
                };
            }

            if (!CraftableItems.ContainsKey(refined))
            {
                CraftableItems[refined] = new ItemBlueprint
                {
                    Name = refined,
                    Category = "refined-material",
                    Description = "Processed material used for higher-tier crafting.",
                    Skill = "Engineering",
                    Cost = 12
                };
            }
        }
    }

    private void AssignPlanetMaterialSources()
    {
        PlanetRawMaterials.Clear();
        var pool = RawMaterials.OrderBy(x => x).ToList();
        if (pool.Count == 0) return;

        foreach (var planet in Planets.Values)
        {
            var idx = Math.Abs(StringComparer.OrdinalIgnoreCase.GetHashCode(planet.Name));
            var sources = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                pool[idx % pool.Count],
                pool[(idx / 3 + 3) % pool.Count],
                pool[(idx / 7 + 5) % pool.Count]
            };

            if (planet.HasDockyard || planet.Economy.Contains("ship", StringComparison.OrdinalIgnoreCase))
            {
                sources.Add("raw durasteel");
                sources.Add("raw ionite");
            }

            if (planet.Economy.Contains("mining", StringComparison.OrdinalIgnoreCase) || planet.Economy.Contains("refinery", StringComparison.OrdinalIgnoreCase))
            {
                sources.Add("raw ore");
                sources.Add("raw carbonite");
            }

            if (planet.Name.Equals("Ilum", StringComparison.OrdinalIgnoreCase) || planet.Name.Equals("Jedha", StringComparison.OrdinalIgnoreCase) || planet.Name.Equals("Lothal", StringComparison.OrdinalIgnoreCase))
            {
                sources.Add("raw kyber shard");
            }

            PlanetRawMaterials[planet.Name] = sources.OrderBy(x => x).ToList();
        }
    }

    private void GenerateBalancedRecipes()
    {
        Recipes.Clear();
        CapitalShipPartRequirements.Clear();

        // Base refining recipes
        foreach (var pair in RefiningMap)
        {
            Recipes[pair.Value] = new CraftRecipe
            {
                Output = pair.Value,
                CreditCost = 6,
                TimeHours = 2,
                Skill = "Engineering",
                RequiresShipyard = false,
                RequiresIndustrialFurnace = RequiresIndustrialFurnaceForRaw(pair.Key),
                Inputs = new List<RecipeComponent> { new() { Item = pair.Key, Quantity = 2 } },
                Notes = "Refining recipe"
            };
        }

        // Generic craftable item recipes
        foreach (var item in CraftableItems.Values)
        {
            if (RawMaterials.Contains(item.Name) || RefinedMaterials.Contains(item.Name)) continue;

            var baseCost = Math.Max(8, item.Cost);
            var refinedQty = Math.Max(1, baseCost / 35);
            var powerQty = Math.Max(1, baseCost / 60);
            var inputs = new List<RecipeComponent>
            {
                new() { Item = "refined metal", Quantity = refinedQty },
                new() { Item = "power cell", Quantity = powerQty }
            };

            if (item.Category.Contains("weapon", StringComparison.OrdinalIgnoreCase))
            {
                inputs.Add(new RecipeComponent { Item = "pressurized tibanna", Quantity = 1 });
            }
            if (item.Category.Contains("medicine", StringComparison.OrdinalIgnoreCase))
            {
                inputs.Add(new RecipeComponent { Item = "refined bacta", Quantity = 1 });
            }
            if (item.Name.Contains("hyperdrive", StringComparison.OrdinalIgnoreCase) || item.Name.Contains("sensor", StringComparison.OrdinalIgnoreCase))
            {
                inputs.Add(new RecipeComponent { Item = "circuit board", Quantity = 1 });
            }

            Recipes[item.Name] = new CraftRecipe
            {
                Output = item.Name,
                CreditCost = Math.Max(4, item.Cost / 5),
                TimeHours = Math.Max(1, item.Cost / 20),
                Skill = item.Skill,
                RequiresShipyard = item.Category.Contains("ship", StringComparison.OrdinalIgnoreCase),
                RequiresIndustrialFurnace = item.Category.Contains("weapon", StringComparison.OrdinalIgnoreCase),
                Inputs = inputs,
                Notes = "Auto-balanced item recipe"
            };
        }

        // Weapon recipes
        foreach (var weapon in Weapons.Values)
        {
            Recipes[weapon.Name] = new CraftRecipe
            {
                Output = weapon.Name,
                CreditCost = Math.Max(8, weapon.Damage * 2),
                TimeHours = Math.Max(2, weapon.Damage / 2),
                Skill = weapon.Category.Contains("melee", StringComparison.OrdinalIgnoreCase) ? "Blades" : "Engineering",
                RequiresShipyard = false,
                RequiresIndustrialFurnace = weapon.Damage >= 12,
                Inputs = new List<RecipeComponent>
                {
                    new() { Item = "refined metal", Quantity = Math.Max(1, weapon.Damage / 4) },
                    new() { Item = "power cell", Quantity = 1 },
                    new() { Item = "pressurized tibanna", Quantity = weapon.Category.Contains("blaster", StringComparison.OrdinalIgnoreCase) ? 1 : 0 }
                }.Where(x => x.Quantity > 0).ToList(),
                Notes = "Auto-balanced weapon recipe"
            };
        }

        // Vehicle recipes
        foreach (var vehicle in Vehicles.Values)
        {
            var sizeFactor = vehicle.Type.Contains("walker", StringComparison.OrdinalIgnoreCase) ? 5 : vehicle.Type.Contains("transport", StringComparison.OrdinalIgnoreCase) ? 4 : 3;
            Recipes[vehicle.Name] = new CraftRecipe
            {
                Output = vehicle.Name,
                CreditCost = 40 * sizeFactor,
                TimeHours = 6 * sizeFactor,
                Skill = "Engineering",
                RequiresShipyard = true,
                RequiresIndustrialFurnace = sizeFactor >= 4,
                Inputs = new List<RecipeComponent>
                {
                    new() { Item = "durasteel ingot", Quantity = 2 * sizeFactor },
                    new() { Item = "ion capacitor core", Quantity = Math.Max(1, sizeFactor - 1) },
                    new() { Item = "power cell", Quantity = sizeFactor }
                },
                Notes = "Auto-balanced vehicle recipe"
            };
        }

        // Ship recipes and capital ship part chains
        foreach (var ship in ShipCatalog.Values)
        {
            if (!ship.IsCapital)
            {
                Recipes[ship.Model] = new CraftRecipe
                {
                    Output = ship.Model,
                    CreditCost = Math.Max(40, ship.Cost / 3),
                    TimeHours = Math.Max(8, ship.Cost / 20),
                    Skill = "Engineering",
                    RequiresShipyard = ship.Cost >= 260,
                    RequiresIndustrialFurnace = ship.Cost >= 220,
                    Inputs = ship.RequiredParts
                        .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                        .Select(g => new RecipeComponent { Item = g.Key, Quantity = g.Count() })
                        .Concat(new[]
                        {
                            new RecipeComponent { Item = "durasteel ingot", Quantity = Math.Max(2, ship.Cost / 120) },
                            new RecipeComponent { Item = "ion capacitor core", Quantity = Math.Max(1, ship.Cost / 220) }
                        })
                        .ToList(),
                    Notes = "Auto-balanced ship recipe"
                };
                continue;
            }

            var partNames = new List<string>
            {
                $"{ship.Model} keel section",
                $"{ship.Model} reactor spine",
                $"{ship.Model} command core",
                $"{ship.Model} armor lattice",
                $"{ship.Model} weapons grid"
            };
            CapitalShipPartRequirements[ship.Model] = partNames;

            for (int i = 0; i < partNames.Count; i++)
            {
                var part = partNames[i];
                if (!CraftableItems.ContainsKey(part))
                {
                    CraftableItems[part] = new ItemBlueprint
                    {
                        Name = part,
                        Category = "capital-part",
                        Description = $"Capital ship assembly part for {ship.Model}.",
                        Skill = "Engineering",
                        Cost = Math.Max(90, ship.Cost / 8)
                    };
                }

                Recipes[part] = new CraftRecipe
                {
                    Output = part,
                    CreditCost = Math.Max(40, ship.Cost / 14),
                    TimeHours = Math.Max(18, ship.Cost / 30),
                    Skill = "Engineering",
                    RequiresShipyard = true,
                    RequiresIndustrialFurnace = true,
                    Inputs = new List<RecipeComponent>
                    {
                        new() { Item = "durasteel ingot", Quantity = Math.Max(2, ship.Cost / 200) + i },
                        new() { Item = "neutronium plate", Quantity = Math.Max(1, ship.Cost / 320) },
                        new() { Item = "ion capacitor core", Quantity = Math.Max(1, ship.Cost / 260) },
                        new() { Item = "stabilized coaxium", Quantity = Math.Max(1, ship.Cost / 350) }
                    },
                    Notes = "Shipyard-only capital part recipe"
                };
            }
        }
    }

    // Random name generation for races (simple syllable combiner)
    public string GenerateNameForRace(string raceName)
    {
        var namePools = new Dictionary<string, string[]>
        {
            ["Human"] = new[] { "ka", "li", "ra", "tor", "voss", "mar", "den", "kel", "san", "ryn" },
            ["Twi'lek"] = new[] { "lek", "ri", "sha", "lek", "tal", "ree", "nya" },
            ["Miraluka"] = new[] { "sa", "na", "el", "ri", "thu", "vo" },
            ["Wookiee"] = new[] { "ro", "ga", "wok", "har", "chuk" },
            ["Mandalorian"] = new[] { "din", "jyn", "val", "kar", "nor" },
            ["Togruta"] = new[] { "sha", "tal", "ahr", "nya", "oka" },
            ["Sullustan"] = new[] { "oll", "rus", "tan", "bir" },
            ["Bothan"] = new[] { "bo", "than", "rik", "fel" },
            ["Rodian"] = new[] { "gre", "nik", "tar", "zun" },
            ["Zabrak"] = new[] { "zha", "rak", "tar", "vok" },
            ["Chiss"] = new[] { "th", "ri", "lan", "xis" },
            ["Mon Calamari"] = new[] { "mok", "tam", "uri", "cal" },
            ["Nautolan"] = new[] { "nok", "tar", "sul" },
            ["Sith"] = new[] { "dar", "vak", "ren", "nos" },
        };

        var pool = namePools.GetValueOrDefault(raceName) ?? namePools["Human"];
        var parts = random.Next(2, 4);
        var name = string.Concat(Enumerable.Range(0, parts).Select(_ => pool[random.Next(pool.Length)]));
        return char.ToUpper(name[0]) + name.Substring(1);
    }

    public string EquipItem(GameCharacter character, string itemName)
    {
        if (!character.Inventory.Any(x => string.Equals(x, itemName, StringComparison.OrdinalIgnoreCase)))
            return "You do not own that item.";

        // Vibroblade is a craftable component / hand-tool — not a proper combat weapon
        if (itemName.Contains("vibroblade", StringComparison.OrdinalIgnoreCase))
            return "Vibroblades are a craft component and cannot be equipped as a primary weapon.";

        if (Weapons.TryGetValue(itemName, out var weaponDef)
            || Weapons.Keys.Any(k => string.Equals(k, itemName, StringComparison.OrdinalIgnoreCase))
            || itemName.Contains("Blaster", StringComparison.OrdinalIgnoreCase)
            || itemName.Contains("Lightsaber", StringComparison.OrdinalIgnoreCase)
            || itemName.Contains("Rifle", StringComparison.OrdinalIgnoreCase)
            || itemName.Contains("Pistol", StringComparison.OrdinalIgnoreCase)
            || itemName.Contains("Sword", StringComparison.OrdinalIgnoreCase)
            || itemName.Contains("Knife", StringComparison.OrdinalIgnoreCase)
            || itemName.Contains("Spear", StringComparison.OrdinalIgnoreCase))
        {
            character.EquippedWeapon = itemName;
            return $"Equipped {itemName} as your weapon.";
        }

        if (itemName.Contains("armor", StringComparison.OrdinalIgnoreCase) || itemName.Contains("beskar", StringComparison.OrdinalIgnoreCase))
        {
            character.Inventory.RemoveAll(x => string.Equals(x, itemName, StringComparison.OrdinalIgnoreCase));
            character.Armor += 2;
            return $"Installed {itemName} improving armor.";
        }

        return "That item cannot be equipped.";
    }

    public string CraftLightsaber(GameCharacter character)
    {
        var required = new[] { "lightsaber hilt", "lightsaber crystal" };
        var missing = required.Where(r => !character.Inventory.Any(x => string.Equals(x, r, StringComparison.OrdinalIgnoreCase))).ToList();
        if (missing.Count > 0) return $"Missing parts: {string.Join(", ", missing)}.";
        foreach (var p in required) character.Inventory.RemoveAll(x => string.Equals(x, p, StringComparison.OrdinalIgnoreCase));
        TryAddInventoryItem(character, "Lightsaber");
        if (!character.Skills.Contains("Lightsaber")) character.Skills.Add("Lightsaber");
        character.Experience += 10;
        return "You assemble a Lightsaber. Skill Lightsaber learned.";
    }

    public string CraftMedkit(GameCharacter character)
    {
        var required = new[] { "medkit components", "power cell" };
        var missing = required.Where(r => !character.Inventory.Any(x => string.Equals(x, r, StringComparison.OrdinalIgnoreCase))).ToList();
        if (missing.Count > 0) return $"Missing medkit parts: {string.Join(", ", missing)}.";
        foreach (var p in required) character.Inventory.RemoveAll(x => string.Equals(x, p, StringComparison.OrdinalIgnoreCase));
        TryAddInventoryItem(character, "field medpack");
        character.Experience += 4;
        return "Crafted a field medpack.";
    }

    public string CraftWeapon(GameCharacter character, string weaponName)
    {
        var part = weaponName + " parts";
        if (!character.Inventory.Any(x => string.Equals(x, part, StringComparison.OrdinalIgnoreCase))) return $"Missing {part}.";
        character.Inventory.RemoveAll(x => string.Equals(x, part, StringComparison.OrdinalIgnoreCase));
        TryAddInventoryItem(character, weaponName);
        character.Experience += 6;
        return $"Crafted {weaponName} from parts.";
    }

    public string GetCurrencyTypeForEra(string eraName, int? rotation = null)
    {
        var currentRotation = rotation ?? Clock.Rotation;
        return eraName switch
        {
            "Old Republic" => currentRotation < 120 ? "Old Republic Credits" : "Galactic Credits",
            "Prequel Era" => "Galactic Credits",
            "Clone Wars" => currentRotation < 340 ? "Galactic Credits" : "Imperial Credits",
            "Original Trilogy" => currentRotation >= 340 ? "Imperial Credits" : "Galactic Credits",
            "New Republic" => currentRotation >= 460 ? "New Republic Credits" : "Imperial Credits",
            "Sequel Trilogy" => "Sequel Credits",
            _ => "Galactic Credits"
        };
    }

    public string GetCurrencyTypeForCurrentEra() => GetCurrencyTypeForEra(GetCurrentEraName());

    public int GetCurrencyRarityValue(int rotation)
    {
        if (rotation >= 560) return 3;
        if (rotation >= 460) return 2;
        if (rotation >= 340) return 1;
        return 0;
    }

    public bool IsInventoryEligible(string itemName) =>
        CraftableItems.ContainsKey(itemName) ||
        Weapons.ContainsKey(itemName) ||
        Vehicles.ContainsKey(itemName) ||
        Armors.ContainsKey(itemName) ||
        Recipes.ContainsKey(itemName) ||
        WoodTypes.ContainsKey(itemName) ||
        RefiningRecipes.ContainsKey(itemName) ||       // raw ore is always eligible
        itemName.Contains("part", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("cell", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("crystal", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("armor", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("blaster", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("saber", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("raw", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("refined", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("alloy", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("ingot", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("ore", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("wood", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("log", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("plank", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("gas", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("shard", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("compound", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("resin", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("fragment", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("dust", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("mineral", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("stone", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("rock", StringComparison.OrdinalIgnoreCase) ||
        itemName.Contains("core", StringComparison.OrdinalIgnoreCase);

    public int GetInventoryCapacity(GameCharacter character)
    {
        var cap = character.MaxInventorySlots;
        // Role bonuses
        if (character.Role.Equals("Smuggler", StringComparison.OrdinalIgnoreCase)) cap += 4;
        if (character.Role.Equals("Bounty Hunter", StringComparison.OrdinalIgnoreCase)) cap += 2;
        // Equipped backpack / belt
        if (character.Inventory.Any(x => x.Contains("backpack", StringComparison.OrdinalIgnoreCase))) cap += 8;
        if (character.Inventory.Any(x => x.Contains("cargo belt", StringComparison.OrdinalIgnoreCase))) cap += 4;
        return cap;
    }

    public string TryAddInventoryItem(GameCharacter character, string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName)) return "Nothing to add.";
        if (!IsInventoryEligible(itemName)) return $"{itemName} cannot be carried in this inventory.";

        // Raw ore is stackable — no duplicate check, but capacity enforced
        bool isRawOre = RefiningRecipes.ContainsKey(itemName) ||
                        itemName.Contains("ore", StringComparison.OrdinalIgnoreCase) ||
                        itemName.Contains("raw", StringComparison.OrdinalIgnoreCase) ||
                        itemName.Contains("gas canister", StringComparison.OrdinalIgnoreCase) ||
                        itemName.Contains("wood", StringComparison.OrdinalIgnoreCase) ||
                        itemName.Contains("shard", StringComparison.OrdinalIgnoreCase) ||
                        itemName.Contains("mineral", StringComparison.OrdinalIgnoreCase);

        if (!isRawOre && character.Inventory.Any(x => string.Equals(x, itemName, StringComparison.OrdinalIgnoreCase)))
            return $"You already hold {itemName}.";

        var used = GetInventoryUsedMicroScu(character);
        var cap  = GetInventoryCapacityMicroScu(character);
        var size = GetItemMicroScuSize(itemName);
        if (used + size > cap)
            return $"Inventory full ({ScuConversion.FormatMicroScu(used)}/{ScuConversion.FormatMicroScu(cap)} used).";

        character.Inventory.Add(itemName);
        return $"Added {itemName} to your inventory.";
    }

    private int CountInventoryItem(GameCharacter character, string itemName)
        => character.Inventory.Count(x => string.Equals(x, itemName, StringComparison.OrdinalIgnoreCase));

    private bool ConsumeInventoryItem(GameCharacter character, string itemName, int quantity)
    {
        if (quantity <= 0) return true;
        if (CountInventoryItem(character, itemName) < quantity) return false;
        for (int i = 0; i < quantity; i++)
        {
            var idx = character.Inventory.FindIndex(x => string.Equals(x, itemName, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return false;
            character.Inventory.RemoveAt(idx);
        }
        return true;
    }

    private bool TryGetRecipe(string outputName, out CraftRecipe recipe)
    {
        if (Recipes.TryGetValue(outputName, out recipe!)) return true;
        var key = Recipes.Keys.FirstOrDefault(k => string.Equals(k, outputName, StringComparison.OrdinalIgnoreCase));
        if (key is null)
        {
            recipe = new CraftRecipe();
            return false;
        }
        recipe = Recipes[key];
        return true;
    }

    public IReadOnlyList<string> GetPlanetRawMaterials(string planetName)
        => PlanetRawMaterials.TryGetValue(planetName, out var mats) ? mats : Array.Empty<string>();

    public string HarvestRawMaterial(GameCharacter character, string materialName, int amount = 1)
    {
        if (amount <= 0) return "Specify a positive amount to gather.";
        if (!PlanetRawMaterials.TryGetValue(character.Location, out var available) || !available.Any(x => string.Equals(x, materialName, StringComparison.OrdinalIgnoreCase)))
        {
            return $"{materialName} is not available on {character.Location}.";
        }

        var gathered = 0;
        for (int i = 0; i < amount; i++)
        {
            if (random.NextDouble() < 0.75)
            {
                if (TryAddInventoryItem(character, materialName).StartsWith("Added", StringComparison.OrdinalIgnoreCase))
                {
                    gathered++;
                }
            }
        }

        if (gathered == 0) return "No usable material was gathered this run.";
        character.Experience += gathered;
        return $"Gathered {gathered}x {materialName} from {character.Location}.";
    }

    // RefineMaterial legacy stub — full logic moved to RefineRawMaterial
    public string RefineMaterial(GameCharacter character, string rawMaterial, int amount = 1)
    {
        var (ok, msg) = RefineRawMaterial(character, rawMaterial, amount);
        return msg;
    }

    public string SellInventoryToMerchant(GameCharacter character, string itemName, int? priceOverride = null)
    {
        if (!character.Inventory.Any(x => string.Equals(x, itemName, StringComparison.OrdinalIgnoreCase)))
        {
            return "You do not have that item to sell.";
        }

        var blueprint = CraftableItems.TryGetValue(itemName, out var craftItem) ? craftItem : null;
        var value = priceOverride ?? (blueprint?.Cost ?? 15) / 2;
        character.Inventory.RemoveAll(x => string.Equals(x, itemName, StringComparison.OrdinalIgnoreCase));
        character.Credits += value;
        return $"You sold {itemName} for {value} credits.";
    }

    // ─── Mining ───────────────────────────────────────────────────────────────
    // Each entry is two tiers: [common...] then ["|"] then [rare...].
    // Items before "|" appear on every roll; items after "|" have a 20% chance per slot.
    private static readonly Dictionary<string, string[]> PlanetMineralYield = new(StringComparer.OrdinalIgnoreCase)
    {
        // ── Core Worlds ────────────────────────────────────────────────────────
        ["Alderaan"]       = new[] { "white marble slab", "cultivite crystal", "noble clay",
                                     "|", "resonite ore", "alderaan chalk" },
        ["Coruscant"]      = new[] { "durasteel scrap", "ferrocrete chunk", "recycled alloy",
                                     "|", "cortosis trace", "refined scrap" },
        ["Corellia"]       = new[] { "durasteel scrap", "corusca fragment", "raw ore",
                                     "|", "coaxium trace", "carbonite slab" },
        ["Kuat"]           = new[] { "durasteel ingot", "duraplast slab", "titanite ore",
                                     "|", "transparisteel fragment", "hull alloy" },

        // ── Mid Rim ────────────────────────────────────────────────────────────
        ["Naboo"]          = new[] { "plasma mineral", "rock salt", "silicite ore",
                                     "|", "naboo plasma crystal", "gemstone shard" },
        ["Kashyyyk"]       = new[] { "wroshyr resin", "ironwood shard", "silicate crystal",
                                     "|", "kshyyyk amber", "symbiote moss" },
        ["Ryloth"]         = new[] { "ryll spice trace", "iron ore", "raw mineral",
                                     "|", "pure ryll crystal", "twilek clay" },
        ["Mon Cala"]       = new[] { "sea mineral", "aquite crystal", "brine salt",
                                     "|", "deep-sea ore", "nautolan coral" },
        ["Dantooine"]      = new[] { "topsoil mineral", "sediment ore", "jedi ruin dust",
                                     "|", "adegan crystal fragment", "ancient stone" },
        ["Utapau"]         = new[] { "sinkhole mineral", "porous rock", "silica dust",
                                     "|", "utapaun geode", "energy crystal" },

        // ── Outer Rim ──────────────────────────────────────────────────────────
        ["Tatooine"]       = new[] { "silicite ore", "silica sand", "mesa rock",
                                     "|", "krayt dragon pearl fragment", "sand glass" },
        ["Mandalore"]      = new[] { "iron ore", "titanite ore", "refined ore",
                                     "|", "beskar fragment", "beskar-laced clay" },
        ["Geonosis"]       = new[] { "doonium ore", "silicite ore", "ferrite dust",
                                     "|", "geonosian resin", "arena clay" },
        ["Mustafar"]       = new[] { "obsidian shard", "volcanic ash", "magma stone",
                                     "|", "cortosis ore", "fire gem" },
        ["Hoth"]           = new[] { "permafrost crystal", "ice mineral", "tundra rock",
                                     "|", "varium ore", "glacial shard" },
        ["Endor"]          = new[] { "forest resin", "hardwood chip", "loam mineral",
                                     "|", "ewok amber", "ancient bark" },
        ["Dagobah"]        = new[] { "bog root", "mire mineral", "dark clay",
                                     "|", "force-imbued sediment", "swamp crystal" },
        ["Lothal"]         = new[] { "raw ore", "prairie mineral", "iron deposit",
                                     "|", "coaxium trace", "loth-cat claw mineral" },
        ["Kessel"]         = new[] { "raw mineral", "glitterstim residue", "carbon deposit",
                                     "|", "spice trace", "pure glitterstim shard" },
        ["Jakku"]          = new[] { "ship hull scrap", "sand mineral", "ferrous dust",
                                     "|", "battle relic fragment", "stardust crystal" },
        ["Bespin"]         = new[] { "tibanna gas canister", "cloud sediment", "gas mineral",
                                     "|", "refined tibanna", "cloudium crystal" },
        ["Kamino"]         = new[] { "sea mineral", "storm sediment", "brine crystal",
                                     "|", "kaminoan bio-resin", "sea glass" },

        // ── Unknown Regions / Sequel ───────────────────────────────────────────
        ["Ilum"]           = new[] { "pure silicate", "cold quartz", "ice mineral",
                                     "|", "kyber crystal shard", "white kyber fragment" },
        ["Ahch-To"]        = new[] { "sea stone", "ancient island rock", "tidal mineral",
                                     "|", "first jedi relic dust", "porg bone chip" },
        ["Exegol"]         = new[] { "dark obsidian", "sith-tainted stone", "shadow mineral",
                                     "|", "sith holocron fragment", "void crystal" },

        // ── Corporate / Hutt Space ─────────────────────────────────────────────
        ["Nar Shaddaa"]    = new[] { "salvage chunk", "scrap alloy", "used power cell",
                                     "|", "black-market circuit board", "illicit coolant" },
    };

    /// <summary>
    /// Infer a contextual mineral pool for planets not in the explicit catalog,
    /// based on their Economy and Region fields.
    /// </summary>
    private string[] InferMineralYield(string planetName)
    {
        if (!Planets.TryGetValue(planetName, out var planet))
            return new[] { "raw ore", "mineral sample", "trace metal" };

        var eco = planet.Economy.ToLowerInvariant();
        var region = planet.Region.ToLowerInvariant();

        // Economy-driven pools
        if (eco.Contains("spice") || eco.Contains("drug"))
            return new[] { "spice trace", "raw mineral", "carbon deposit", "|", "pure spice crystal" };

        if (eco.Contains("mining") || eco.Contains("refinery") || eco.Contains("industry"))
            return new[] { "raw ore", "titanite ore", "metal ore", "|", "durasteel ingot", "cortosis trace" };

        if (eco.Contains("droid") || eco.Contains("foundry") || eco.Contains("manufactur"))
            return new[] { "ferrite dust", "doonium ore", "silicite ore", "|", "durasteel scrap" };

        if (eco.Contains("ship") || eco.Contains("naval") || eco.Contains("cargo"))
            return new[] { "hull alloy", "durasteel scrap", "refined ore", "|", "transparisteel fragment" };

        if (eco.Contains("timber") || eco.Contains("ecolog") || eco.Contains("hunt"))
            return new[] { "hardwood chip", "forest resin", "loam mineral", "|", "ancient bark" };

        if (eco.Contains("farm") || eco.Contains("agric"))
            return new[] { "topsoil mineral", "sediment ore", "raw mineral", "|", "fertile clay" };

        if (eco.Contains("gas") || eco.Contains("tibanna"))
            return new[] { "tibanna gas canister", "gas mineral", "cloud sediment", "|", "refined tibanna" };

        if (eco.Contains("relic") || eco.Contains("archaeo") || eco.Contains("pilgrim"))
            return new[] { "ancient stone", "ruin dust", "fossilised mineral", "|", "force-imbued sediment", "ancient relic fragment" };

        if (eco.Contains("salvage") || eco.Contains("scaveng"))
            return new[] { "salvage chunk", "scrap alloy", "ferrous dust", "|", "rare circuit board" };

        // Region-driven fallback
        if (region.Contains("outer rim"))
            return new[] { "raw ore", "silicite ore", "titanite ore", "|", "trace metal" };
        if (region.Contains("core"))
            return new[] { "refined ore", "durasteel scrap", "ferrocrete chunk", "|", "coaxium trace" };
        if (region.Contains("mid rim"))
            return new[] { "silicate crystal", "raw mineral", "sediment ore", "|", "refined ore" };
        if (region.Contains("unknown"))
            return new[] { "dark mineral", "void stone", "ancient rock", "|", "dark crystal shard" };

        return new[] { "raw ore", "mineral sample", "trace metal" };
    }

    public string MinePlanet(GameCharacter character, string zone)
    {
        const int miningCooldownRotations = 4;
        if (Clock.Rotation - character.LastMiningRotation < miningCooldownRotations)
        {
            var wait = miningCooldownRotations - (Clock.Rotation - character.LastMiningRotation);
            return $"The vein is picked over — wait {wait} more rotation(s) before mining again.";
        }

        var planet = character.Location;
        // Stations — resolve to orbiting planet
        if (!Planets.ContainsKey(planet) && SpaceStations.TryGetValue(planet, out var st) && Planets.ContainsKey(st.OrbitingPlanet))
            planet = st.OrbitingPlanet;

        // Get explicit yield or infer from planet data
        string[] rawPool;
        if (!PlanetMineralYield.TryGetValue(planet, out rawPool!))
            rawPool = InferMineralYield(planet);

        // Split pool into common / rare tiers on the "|" sentinel
        var separatorIdx = Array.IndexOf(rawPool, "|");
        string[] commonPool = separatorIdx > 0 ? rawPool[..separatorIdx] : rawPool;
        string[] rarePool   = separatorIdx > 0 && separatorIdx < rawPool.Length - 1
                              ? rawPool[(separatorIdx + 1)..] : Array.Empty<string>();

        // Zone modifiers — how many common rolls
        var rolls = zone.ToLowerInvariant() switch
        {
            "caves" or "mine shaft" or "volcanic flats" or "industrial platform" => random.Next(2, 4),
            "ruins" or "wilderness" or "forest" or "swamp"                       => random.Next(1, 3),
            _                                                                     => 1
        };

        var yield = new List<string>();
        for (int i = 0; i < rolls; i++)
            yield.Add(commonPool[random.Next(commonPool.Length)]);

        // 20% chance per rare slot
        foreach (var rare in rarePool)
            if (random.NextDouble() < 0.20)
                yield.Add(rare);

        // 8% chance at a universal tech bonus drop
        if (random.NextDouble() < 0.08)
        {
            var bonusPool = new[] { "ancient relic fragment", "power cell", "circuit board", "coolant unit" };
            yield.Add(bonusPool[random.Next(bonusPool.Length)]);
        }

        character.LastMiningRotation = Clock.Rotation;
        character.Inventory.AddRange(yield);
        character.Experience += 4;
        AdvanceWorldTime(2, character.Location, character);

        return $"You spend time mining the {zone} of {planet} and extract: {string.Join(", ", yield)}. ({yield.Count} item(s) added to inventory.)";
    }

    public string LootTarget(GameCharacter character, string targetName, int credits = 0)
    {
        // Loot cooldown: 5 rotations between loots
        const int lootCooldownRotations = 5;
        if (Clock.Rotation - character.LastLootRotation < lootCooldownRotations)
        {
            var wait = lootCooldownRotations - (Clock.Rotation - character.LastLootRotation);
            return $"You need to wait {wait} more rotation(s) before looting again. Take your time.";
        }
        character.LastLootRotation = Clock.Rotation;

        // Build a loot pool that excludes vehicles
        var vehicleNames = new HashSet<string>(Vehicles.Keys, StringComparer.OrdinalIgnoreCase);
        var craftPool = CraftableItems.Keys
            .Where(k => !vehicleNames.Contains(k)
                && !k.Contains("speeder", StringComparison.OrdinalIgnoreCase)
                && !k.Contains("walker", StringComparison.OrdinalIgnoreCase)
                && !k.Contains("ship", StringComparison.OrdinalIgnoreCase)
                && !k.Contains("freighter", StringComparison.OrdinalIgnoreCase)
                && !k.Contains("corvette", StringComparison.OrdinalIgnoreCase)
                && !k.Contains("starship", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var weaponPool = Weapons.Keys
            .Where(k => !vehicleNames.Contains(k))
            .ToList();

        var combinedPool = craftPool.Concat(weaponPool).ToList();
        if (combinedPool.Count == 0) combinedPool.Add("power cell");

        var rewardItem = combinedPool[random.Next(combinedPool.Count)];
        var reward = credits > 0 ? credits : 8 + random.Next(0, 12);
        character.Credits += reward;
        character.Reputation += 1;
        return TryAddInventoryItem(character, rewardItem) + $" You also looted {reward} credits from {targetName}.";
    }

    public List<string> GetEncounterActionCatalog(string planetName)
    {
        // Legacy overload — returns all possible actions. Use GetZoneActions for zone-filtered lists.
        return new List<string>
        {
            "Visit Merchant",
            "Scavenge Ruins",
            "Talk to Contact",
            "Raid Slums",
            "Loot Body",
            "Inspect Dock",
            "Track Wildlife",
            "Start Long Chat"
        };
    }

    // Returns valid zones for a given planet, taking into account the planet's terrain/culture
    // ═══════════════════════════════════════════════════════════════════════════
    // PLANET LOCATION HIERARCHY
    // ═══════════════════════════════════════════════════════════════════════════

    public IReadOnlyList<string> GetLocationRegions(string planet)
    {
        if (PlanetLocations.TryGetValue(planet, out var locs))
            return locs.Select(l => l.Region).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return GenerateFallbackRegions(planet);
    }

    public IReadOnlyList<string> GetLocationDistricts(string planet, string region)
    {
        if (PlanetLocations.TryGetValue(planet, out var locs))
            return locs.Where(l => l.Region.Equals(region, StringComparison.OrdinalIgnoreCase))
                       .Select(l => l.District).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return FallbackDistricts(region);
    }

    public IReadOnlyList<PlanetZoneLocation> GetLocationsByDistrict(string planet, string region, string district)
    {
        if (PlanetLocations.TryGetValue(planet, out var locs))
            return locs.Where(l => l.Region.Equals(region, StringComparison.OrdinalIgnoreCase)
                                && l.District.Equals(district, StringComparison.OrdinalIgnoreCase)).ToList();
        return GenerateFallbackLocations(planet, region, district);
    }

    private IReadOnlyList<string> GenerateFallbackRegions(string planet)
    {
        var regions = new List<string> { "Wilderness" };
        if (Planets.TryGetValue(planet, out var p))
        {
            if (!string.IsNullOrWhiteSpace(p.Economy)) regions.Insert(0, "Urban");
            if (p.HasDockyard || p.Economy.Contains("ship", StringComparison.OrdinalIgnoreCase)) regions.Add("Industrial");
        }
        return regions;
    }

    private static IReadOnlyList<string> FallbackDistricts(string region) => region.ToLowerInvariant() switch
    {
        "urban"      => new[] { "Market Quarter", "Docking Ring", "Outskirts" },
        "industrial" => new[] { "Shipyards", "Factory District" },
        _            => new[] { "Open Terrain", "Deep Wilderness" }
    };

    private IReadOnlyList<PlanetZoneLocation> GenerateFallbackLocations(string planet, string region, string district)
    {
        var r = region.ToLowerInvariant();
        if (r == "urban")
            return new[]
            {
                new PlanetZoneLocation { Region=region, District=district, Name="Cantina",
                    Description=$"The local cantina in {district}.", ThreatLevel="Moderate", EncounterZone="market",
                    Actions=new(){"Talk to Contact","Start Long Chat","Visit Merchant","Gather Intel"}, HasMerchant=true },
                new PlanetZoneLocation { Region=region, District=district, Name="Market",
                    Description=$"A market in {district}.", ThreatLevel="Low", EncounterZone="market",
                    Actions=new(){"Visit Merchant","Talk to Contact"}, HasMerchant=true },
                new PlanetZoneLocation { Region=region, District=district, Name="Back Alleys",
                    Description=$"The rougher side of {district}.", ThreatLevel="High", EncounterZone="slums",
                    Actions=new(){"Raid Slums","Gather Intel","Talk to Contact"} },
                new PlanetZoneLocation { Region=region, District=district, Name="Docking Bay",
                    Description=$"A docking facility in {district}.", ThreatLevel="Low", EncounterZone="dock",
                    Actions=new(){"Inspect Dock","Visit Merchant","Talk to Contact"}, HasMerchant=true },
            };
        if (r == "industrial")
            return new[]
            {
                new PlanetZoneLocation { Region=region, District=district, Name="Docking Bay",
                    Description=$"Industrial docking in {district}.", ThreatLevel="Low", EncounterZone="dock",
                    Actions=new(){"Inspect Dock","Visit Merchant"}, HasMerchant=true },
                new PlanetZoneLocation { Region=region, District=district, Name="Factory Floor",
                    Description=$"Industrial machinery in {district}.", ThreatLevel="Moderate", EncounterZone="contact",
                    Actions=new(){"Talk to Contact","Mine Resources","Gather Intel"}, IsMineZone=true },
            };
        return new[]
        {
            new PlanetZoneLocation { Region=region, District=district, Name="Open Terrain",
                Description=$"The wilderness of {district} on {planet}.", ThreatLevel="Moderate", EncounterZone="wilderness",
                Actions=new(){"Harvest","Mine Resources","Track Wildlife"}, IsHarvestZone=true, IsMineZone=true },
            new PlanetZoneLocation { Region=region, District=district, Name="Ruins",
                Description=$"Ancient ruins in {district}.", ThreatLevel="High", EncounterZone="ruins",
                Actions=new(){"Scavenge Ruins","Investigate","Mine Resources"}, IsMineZone=true },
        };
    }

    private void InitPlanetLocations()
    {
        PlanetLocations.Clear();
        static List<string> A(params string[] a) => new(a);

        void L(string planet, string region, string district, string name, string desc,
               string threat, string ez, List<string> actions,
               bool merchant=false, bool mine=false, bool harvest=false, bool wood=false)
        {
            if (!PlanetLocations.ContainsKey(planet)) PlanetLocations[planet] = new();
            PlanetLocations[planet].Add(new PlanetZoneLocation {
                Region=region, District=district, Name=name, Description=desc,
                ThreatLevel=threat, EncounterZone=ez, Actions=actions,
                HasMerchant=merchant, IsMineZone=mine, IsHarvestZone=harvest, IsWoodZone=wood });
        }

        // ── TATOOINE ──────────────────────────────────────────────────────────
        L("Tatooine","Wilderness","Dune Sea","Sarlacc Pit",
          "The Great Pit of Carkoon. The sarlacc digests victims over millennia.",
          "Extreme","wilderness",A("Investigate","Scavenge Ruins","Raid Slums"));
        L("Tatooine","Wilderness","Dune Sea","Krayt Dragon Lair",
          "Skeletal remains and active krayt territories dot the sea.",
          "High","wilderness",A("Track Wildlife","Scavenge Ruins","Mine Resources"),mine:true);
        L("Tatooine","Wilderness","Dune Sea","Ancient Ruins",
          "Pre-Republic ruins half-buried by drifting sand.",
          "High","ruins",A("Scavenge Ruins","Mine Resources","Investigate"),mine:true);
        L("Tatooine","Wilderness","Jundland Wastes","Tusken Encampment",
          "Rough terrain. Aggressive Tusken Raiders. Travel in numbers.",
          "Extreme","slums",A("Raid Slums","Track Wildlife","Gather Intel"));
        L("Tatooine","Wilderness","Jundland Wastes","Krayt Pass",
          "A treacherous canyon pass through the rocky Jundland Wastes.",
          "High","wilderness",A("Investigate","Track Wildlife","Scavenge Ruins"));
        L("Tatooine","Wilderness","Jundland Wastes","Obi-Wan's Hut",
          "Remote hermit dwelling on the edge of the Jundland Wastes.",
          "Low","contact",A("Talk to Contact","Gather Intel","Investigate"));
        L("Tatooine","Wilderness","Outskirts","Lars Moisture Farm",
          "A moisture farm on the outer edge. Quiet and isolated.",
          "Low","wilderness",A("Talk to Contact","Investigate","Harvest"),harvest:true);
        L("Tatooine","Wilderness","Outskirts","Beggar's Canyon",
          "Canyon famous for T-16 skyhopper runs and daredevils.",
          "Moderate","wilderness",A("Investigate","Track Wildlife","Scavenge Ruins"));
        L("Tatooine","Wilderness","Outskirts","Tosche Station",
          "A small power station and general store near Anchorhead.",
          "Low","market",A("Visit Merchant","Talk to Contact"),merchant:true);
        // Mos Eisley
        L("Tatooine","Towns","Mos Eisley","Cantina",
          "Wretched hive. Great place to hire a pilot — or get shot.",
          "High","market",A("Talk to Contact","Start Long Chat","Gather Intel","Visit Merchant"),merchant:true);
        L("Tatooine","Towns","Mos Eisley","Spaceport",
          "Busy spaceport. Imperial checkpoints are common.",
          "Moderate","dock",A("Inspect Dock","Visit Merchant","Talk to Contact"),merchant:true);
        L("Tatooine","Towns","Mos Eisley","Docking Bay 94",
          "A docking bay used for discreet departures.",
          "Moderate","dock",A("Inspect Dock","Talk to Contact"));
        L("Tatooine","Towns","Mos Eisley","Market District",
          "Sun-scorched market full of moisture farmers and traders.",
          "Low","market",A("Visit Merchant","Talk to Contact","Gather Intel"),merchant:true);
        L("Tatooine","Towns","Mos Eisley","Back Alleys",
          "The shadier side of Mos Eisley. Imperials rarely patrol here.",
          "High","slums",A("Gather Intel","Raid Slums","Talk to Contact"));
        // Mos Espa
        L("Tatooine","Towns","Mos Espa","Slave Quarter",
          "Crowded residential area where slaves like young Anakin once lived.",
          "High","slums",A("Talk to Contact","Gather Intel","Raid Slums"));
        L("Tatooine","Towns","Mos Espa","Podracing Circuit",
          "The Boonta Eve Classic course. Dangerous at speed.",
          "Moderate","wilderness",A("Investigate","Talk to Contact","Scavenge Ruins"));
        L("Tatooine","Towns","Mos Espa","Watto's Shop",
          "Junk shop dealing in spare parts, droids, and salvage.",
          "Low","market",A("Visit Merchant","Talk to Contact"),merchant:true);
        L("Tatooine","Towns","Mos Espa","Jabba's Palace",
          "Fortified stronghold of Jabba the Hutt in the Dune Sea outskirts. Heavily guarded.",
          "Extreme","slums",A("Talk to Contact","Gather Intel","Raid Slums"));
        // Anchorhead
        L("Tatooine","Towns","Anchorhead","Trade Office",
          "Regional hub for moisture farmers and travelling merchants.",
          "Low","market",A("Visit Merchant","Talk to Contact"),merchant:true);
        L("Tatooine","Towns","Anchorhead","Tatooine Security Post",
          "A local constabulary outpost. Occasionally helpful.",
          "Low","contact",A("Talk to Contact","Gather Intel"));

        // ── CORUSCANT ─────────────────────────────────────────────────────────
        L("Coruscant","Federal District","Senate District","Senate Rotunda",
          "Central legislative chamber of the Galactic Republic, now Imperial Senate.",
          "Moderate","contact",A("Talk to Contact","Gather Intel"));
        L("Coruscant","Federal District","Jedi Temple District","Jedi Temple",
          "The ancient fortress-temple of the Jedi. Abandoned after Order 66.",
          "High","ruins",A("Investigate","Scavenge Ruins","Gather Intel"));
        L("Coruscant","Federal District","Imperial Quarter","Imperial Palace",
          "The Emperor's palace, built over the old Jedi Temple.",
          "Extreme","slums",A("Gather Intel","Raid Slums"));
        L("Coruscant","Federal District","Banking District","Banking Clan HQ",
          "The powerful InterGalactic Banking Clan's Coruscant offices.",
          "Moderate","market",A("Visit Merchant","Talk to Contact","Gather Intel"),merchant:true);
        L("Coruscant","Upper City","Entertainment District","Coruscant Cantina",
          "Upscale cantina frequented by senators and lobbyists.",
          "Low","market",A("Talk to Contact","Start Long Chat","Gather Intel","Visit Merchant"),merchant:true);
        L("Coruscant","Upper City","Coruscant Way","Commerce Plaza",
          "A massive commercial hub serving the upper-city population.",
          "Low","market",A("Visit Merchant","Talk to Contact"),merchant:true);
        L("Coruscant","Upper City","Coruscant Way","Transport Hub",
          "Airbus and transport depot connecting upper city sectors.",
          "Low","contact",A("Talk to Contact","Gather Intel"));
        L("Coruscant","Mid-Levels","Lower Market","Grey Market",
          "Semi-legitimate trading district where upper rules don't apply.",
          "Moderate","market",A("Visit Merchant","Talk to Contact","Gather Intel"),merchant:true);
        L("Coruscant","Mid-Levels","Lower Market","Industrial Platform",
          "A work platform linking industrial and residential sectors.",
          "Moderate","contact",A("Talk to Contact","Gather Intel","Mine Resources"),mine:true);
        L("Coruscant","Underworld","Level 1313","1313 Slums",
          "Infamous subterranean criminal haven. Black Sun and worse.",
          "Extreme","slums",A("Raid Slums","Gather Intel","Talk to Contact"));
        L("Coruscant","Underworld","Level 1313","Security Bureau 1313",
          "An ISB law enforcement node embedded in the underworld.",
          "High","slums",A("Raid Slums","Gather Intel","Investigate"));
        L("Coruscant","Underworld","Level 1313","Smuggler Dens",
          "Black market trading, fencing stolen goods, discreet meetings.",
          "High","slums",A("Talk to Contact","Visit Merchant","Gather Intel"),merchant:true);
        L("Coruscant","Underworld","Dex's Sector","Dex's Diner",
          "A diner run by an old Besalisk. Good food, good rumours.",
          "Low","market",A("Talk to Contact","Start Long Chat","Gather Intel"),merchant:true);

        // ── NABOO ────────────────────────────────────────────────────────────
        L("Naboo","Theed","Royal Palace","Throne Room",
          "The seat of Naboo's monarchy. Grand, ornate, politically active.",
          "Low","contact",A("Talk to Contact","Gather Intel"));
        L("Naboo","Theed","Royal Palace","Hangar Bay",
          "Royal starship hangar. N-1 starfighters patrol the skies.",
          "Low","dock",A("Inspect Dock","Talk to Contact"));
        L("Naboo","Theed","Theed Centre","Marketplace",
          "Beautiful open marketplace in the heart of the capital.",
          "Low","market",A("Visit Merchant","Talk to Contact"),merchant:true);
        L("Naboo","Theed","Theed Centre","Plasma Refinery",
          "The plasma energy facility powering Theed's infrastructure.",
          "Moderate","contact",A("Mine Resources","Gather Intel"),mine:true);
        L("Naboo","Lake Country","Varykino","Varykino Retreat",
          "An island retreat on Naboo's lake district. Serene and remote.",
          "Low","wilderness",A("Harvest","Investigate","Talk to Contact"),harvest:true);
        L("Naboo","Lake Country","Keren","Keren Market",
          "A relaxed trading town on the shores of Naboo's lakes.",
          "Low","market",A("Visit Merchant","Talk to Contact"),merchant:true);
        L("Naboo","Wilderness","Naboo Plains","Grassy Plains",
          "Rolling grasslands and rivers. Home to large fauna.",
          "Low","wilderness",A("Harvest","Track Wildlife","Mine Resources"),harvest:true);
        L("Naboo","Wilderness","Gungan Territory","Gungan Sacred Place",
          "A holy site used by Gungans for meetings and ceremonies.",
          "Low","contact",A("Talk to Contact","Gather Intel","Investigate"));

        // ── HOTH ─────────────────────────────────────────────────────────────
        L("Hoth","Rebel Base","Echo Base","Command Centre",
          "The Rebel Alliance's icy command bunker. Tactical maps inside.",
          "Moderate","contact",A("Talk to Contact","Gather Intel"));
        L("Hoth","Rebel Base","Echo Base","Hangar Bay",
          "Rebel snowspeeders and transports in the cavernous ice hangar.",
          "Low","dock",A("Inspect Dock","Talk to Contact","Visit Merchant"),merchant:true);
        L("Hoth","Rebel Base","Echo Base","Medical Bay",
          "Bacta tanks and field surgery stations.",
          "Low","contact",A("Talk to Contact","Gather Intel"));
        L("Hoth","Wilderness","Frozen Wastes","Wampa Territory",
          "Open ice flats where wampas hunt. Do not travel alone.",
          "Extreme","wilderness",A("Track Wildlife","Investigate","Mine Resources"),mine:true);
        L("Hoth","Wilderness","Frozen Wastes","South Ridge",
          "Rebel patrol ridge line. Crashed equipment here.",
          "High","wilderness",A("Scavenge Ruins","Investigate","Mine Resources"),mine:true);
        L("Hoth","Ice Caves","Deep Caverns","Crystal Deposits",
          "Deep ice caves with natural crystal formations and mineral deposits.",
          "High","caves",A("Mine Resources","Investigate"),mine:true);

        // ── DAGOBAH ──────────────────────────────────────────────────────────
        L("Dagobah","Swamp","Yoda's Dwelling","Yoda's Hut",
          "A tiny mud dwelling deep in the swamp.",
          "Low","contact",A("Talk to Contact","Gather Intel","Investigate"));
        L("Dagobah","Swamp","Yoda's Dwelling","Dark Side Cave",
          "A place strong in the dark side. Visions linger here.",
          "High","ruins",A("Investigate","Scavenge Ruins"));
        L("Dagobah","Wilderness","Swamp Interior","River Delta",
          "A tangle of roots, fog, and murky waterways.",
          "Moderate","swamp",A("Harvest","Track Wildlife"),harvest:true);
        L("Dagobah","Wilderness","Deep Wilderness","Ancient Ruins",
          "Ruins from a forgotten civilisation beneath the swamp.",
          "High","ruins",A("Scavenge Ruins","Investigate","Mine Resources"),mine:true);

        // ── KASHYYYK ─────────────────────────────────────────────────────────
        L("Kashyyyk","Kachirho","Tree City","Great Hall",
          "The central gathering place of the Wookiee city in the canopy.",
          "Low","contact",A("Talk to Contact","Gather Intel","Visit Merchant"),merchant:true);
        L("Kashyyyk","Kachirho","Tree City","Docking Platform",
          "Elevated docking area for visiting ships far above the forest floor.",
          "Low","dock",A("Inspect Dock","Talk to Contact"));
        L("Kashyyyk","Wroshyr Canopy","Upper Branches","Treetop Village",
          "Wookiee settlements built into the highest branches.",
          "Low","wilderness",A("Talk to Contact","Harvest","Chop Wood"),harvest:true,wood:true);
        L("Kashyyyk","Wroshyr Canopy","Mid-Canopy","Forest Walkways",
          "Suspended bridges connecting the mid-canopy settlements.",
          "Moderate","forest",A("Harvest","Track Wildlife","Chop Wood"),harvest:true,wood:true);
        L("Kashyyyk","Shadowlands","Lower Levels","Dark Territory",
          "The ground level of Kashyyyk. Terrifying predators. Few return.",
          "Extreme","wilderness",A("Track Wildlife","Scavenge Ruins","Mine Resources"),mine:true);
        L("Kashyyyk","Shadowlands","Lower Levels","Old Republic Ruins",
          "Ancient ruins from before the Empire, submerged in the undergrowth.",
          "High","ruins",A("Scavenge Ruins","Investigate","Mine Resources"),mine:true);

        // ── BESPIN ───────────────────────────────────────────────────────────
        L("Bespin","Cloud City","Administrator Level","Lando's Offices",
          "Administrative suites of Cloud City's baron-administrator.",
          "Moderate","contact",A("Talk to Contact","Gather Intel"));
        L("Bespin","Cloud City","Upper City","Carbon-Freezing Chamber",
          "Industrial facility used for tibanna processing and darker purposes.",
          "High","slums",A("Investigate","Gather Intel","Raid Slums"));
        L("Bespin","Cloud City","Casino District","Sabacc Tables",
          "The famous Cloud City casino. Cards, credits, conversations.",
          "Low","market",A("Talk to Contact","Start Long Chat","Gather Intel","Visit Merchant"),merchant:true);
        L("Bespin","Cloud City","Market Level","Cloud City Market",
          "Commercial heart selling tibanna products and luxury goods.",
          "Low","market",A("Visit Merchant","Talk to Contact"),merchant:true);
        L("Bespin","Mining Platforms","Tibanna Platform","Gas Mining Rigs",
          "Massive tibanna gas extraction platforms in the cloud layers.",
          "Moderate","contact",A("Mine Resources","Gather Intel","Investigate"),mine:true);
        L("Bespin","Mining Platforms","Ugnaught Warrens","Industrial Quarter",
          "The Ugnaught workers' living areas below the main city.",
          "Moderate","slums",A("Talk to Contact","Gather Intel","Mine Resources"),mine:true);

        // ── CORELLIA ─────────────────────────────────────────────────────────
        L("Corellia","Coronet City","Treasure Ship Row","Trade Cantina",
          "A sailors' cantina near Corellia's capital docking ports.",
          "Moderate","market",A("Talk to Contact","Start Long Chat","Visit Merchant","Gather Intel"),merchant:true);
        L("Corellia","Coronet City","Docking Ring","Trade Port",
          "The main commercial docking port of Coronet City.",
          "Low","dock",A("Inspect Dock","Visit Merchant","Talk to Contact"),merchant:true);
        L("Corellia","Coronet City","CorSec District","CorSec Headquarters",
          "The Corellian Security Force's main offices.",
          "Moderate","contact",A("Talk to Contact","Gather Intel"));
        L("Corellia","Coronet City","Commerce District","Commerce Plaza",
          "The main commercial district of Coronet City.",
          "Low","market",A("Visit Merchant","Talk to Contact"),merchant:true);
        L("Corellia","Wilderness","Corellian Countryside","Forest Trails",
          "The rolling wooded countryside outside Coronet City.",
          "Low","forest",A("Harvest","Chop Wood","Track Wildlife"),harvest:true,wood:true);
        L("Corellia","Industrial","Corellian Shipyards","Shipyard Floor",
          "One of the most famous shipyards in the galaxy.",
          "Moderate","dock",A("Inspect Dock","Visit Merchant","Gather Intel"),merchant:true);

        // ── MANDALORE ────────────────────────────────────────────────────────
        L("Mandalore","Sundari","Capital City","Royal Palace",
          "The seat of Mandalore's ruler. Warrior customs and politics.",
          "High","contact",A("Talk to Contact","Gather Intel","Investigate"));
        L("Mandalore","Sundari","Capital City","Mandalorian Market",
          "Commercial district dealing in beskar and warrior gear.",
          "Low","market",A("Visit Merchant","Talk to Contact"),merchant:true);
        L("Mandalore","Wilderness","Mandalorian Desert","Ancient Mines",
          "Old beskar ore mines in the Mandalorian wastelands.",
          "High","wilderness",A("Mine Resources","Scavenge Ruins"),mine:true);
        L("Mandalore","Wilderness","Mandalorian Desert","Death Watch Ruins",
          "Abandoned Death Watch outpost in the desert.",
          "High","ruins",A("Scavenge Ruins","Investigate","Mine Resources"),mine:true);

        // ── JAKKU ────────────────────────────────────────────────────────────
        L("Jakku","Outpost","Niima Outpost","Trading Post",
          "The only trading hub on Jakku. Unkar Plutt's domain.",
          "High","market",A("Visit Merchant","Talk to Contact","Gather Intel"),merchant:true);
        L("Jakku","Outpost","Niima Outpost","Junkboss Territory",
          "Enforcer-patrolled area around the trading post.",
          "High","slums",A("Raid Slums","Gather Intel","Talk to Contact"));
        L("Jakku","Starship Graveyard","Graveyard","Imperial Wreck Sites",
          "Fallen Imperial Star Destroyers from the Battle of Jakku.",
          "High","ruins",A("Scavenge Ruins","Investigate","Mine Resources"),mine:true);
        L("Jakku","Starship Graveyard","Graveyard","Scavenger Turf",
          "Contested scavenging grounds. Good salvage, dangerous rivals.",
          "High","wilderness",A("Scavenge Ruins","Mine Resources","Track Wildlife"),mine:true);
        L("Jakku","Wilderness","Carbon Ridge","Desert Wastes",
          "Vast desert wastes between settlements.",
          "Moderate","wilderness",A("Harvest","Mine Resources","Investigate"),mine:true);

        // ── YAVIN IV ─────────────────────────────────────────────────────────
        L("Yavin IV","Rebel Base","Great Temple","Rebel Command",
          "The Rebel Alliance's primary base in a Massassi temple.",
          "Moderate","contact",A("Talk to Contact","Gather Intel"));
        L("Yavin IV","Rebel Base","Great Temple","Hangar Bay",
          "X-Wings and Y-Wings lined up for sorties.",
          "Low","dock",A("Inspect Dock","Talk to Contact"));
        L("Yavin IV","Jungle","Massassi Ruins","Ancient Temple Ruins",
          "Thousand-year-old Massassi temples deep in the jungle.",
          "High","ruins",A("Scavenge Ruins","Investigate","Mine Resources"),mine:true);
        L("Yavin IV","Jungle","Deep Jungle","Jungle Interior",
          "Dense hostile jungle. Ancient ruins and dangerous fauna.",
          "High","forest",A("Harvest","Chop Wood","Track Wildlife","Mine Resources"),harvest:true,wood:true);

        // ── ENDOR ────────────────────────────────────────────────────────────
        L("Endor","Forest","Bright Tree Village","Ewok Village",
          "Treetop village of the Bright Tree tribe of Ewoks.",
          "Low","contact",A("Talk to Contact","Gather Intel","Visit Merchant"),merchant:true);
        L("Endor","Forest","Imperial Outpost","Shield Generator Bunker",
          "Imperial facility housing the Death Star II's shield generator.",
          "Extreme","slums",A("Raid Slums","Investigate","Gather Intel"));
        L("Endor","Wilderness","Forest Floor","Ancient Forest",
          "The ancient forest floor of Endor, alive with wildlife.",
          "Moderate","forest",A("Harvest","Chop Wood","Track Wildlife"),harvest:true,wood:true);

        // ── JEDHA ────────────────────────────────────────────────────────────
        L("Jedha","Jedha City","Holy City","Temple of the Kyber",
          "Sacred Jedi temple, heavily looted by Imperial occupation.",
          "High","ruins",A("Investigate","Scavenge Ruins","Gather Intel"));
        L("Jedha","Jedha City","Holy City","Cadranese District",
          "Civilian market ground down by Imperial extraction.",
          "High","slums",A("Talk to Contact","Visit Merchant","Gather Intel"),merchant:true);
        L("Jedha","Jedha City","Saw's Hideout","Partisans' Base",
          "Saw Gerrera's Partisans operate from hidden cells in the city.",
          "High","slums",A("Talk to Contact","Gather Intel","Raid Slums"));
        L("Jedha","Desert","Kyber Deposits","Crystal Mines",
          "Devastated kyber crystal mining fields stripped by the Empire.",
          "High","wilderness",A("Mine Resources","Scavenge Ruins","Investigate"),mine:true);

        // ── MUSTAFAR ─────────────────────────────────────────────────────────
        L("Mustafar","Fortress","Fortress Vader","Outer Ramparts",
          "The imposing black fortress of Darth Vader. Dark side strong here.",
          "Extreme","slums",A("Raid Slums","Gather Intel","Investigate"));
        L("Mustafar","Lava Flats","Klegger Corp Facility","Mining Platform",
          "Old mining platform scarred by lava flows.",
          "Extreme","volcanic flats",A("Mine Resources","Investigate","Scavenge Ruins"),mine:true);
        L("Mustafar","Lava Flats","Open Lava Fields","Lava Rivers",
          "Flowing rivers of molten rock. Extreme heat, extreme danger.",
          "Extreme","volcanic flats",A("Mine Resources","Investigate"),mine:true);

        // ── GEONOSIS ─────────────────────────────────────────────────────────
        L("Geonosis","Surface","Rocky Terrain","Hive Spires",
          "Towering spire constructions of the Geonosian hive.",
          "High","wilderness",A("Investigate","Track Wildlife","Gather Intel"));
        L("Geonosis","Surface","Separatist Command","Command Centre",
          "Separatist military command during the Clone Wars.",
          "Extreme","slums",A("Raid Slums","Gather Intel","Investigate"));
        L("Geonosis","Catacombs","Battle Droid Factory","Assembly Lines",
          "Underground droid production facilities, now silent.",
          "High","ruins",A("Scavenge Ruins","Mine Resources","Investigate"),mine:true);
        L("Geonosis","Catacombs","Execution Arena","Petranaki Arena",
          "The arena where Obi-Wan, Anakin, and Padmé were nearly executed.",
          "High","ruins",A("Investigate","Scavenge Ruins"));
    }

    public List<string> GetPlanetZones(string planetName)
    {
        // Space stations get a fixed dock-oriented zone list
        if (SpaceStations.ContainsKey(planetName))
            return new List<string> { "dock", "market", "research platform" };

        if (Planets.TryGetValue(planetName, out var planet) && planet.ValidZones.Count > 0)
            return new List<string>(planet.ValidZones);

        // Default zone sets per planet type (inferred from name or description)
        var lower = planetName.ToLowerInvariant();

        // Desert/Outer Rim worlds — no slums, no forest/swamp
        if (lower is "tatooine" or "geonosis" or "jakku" or "jedha")
            return new List<string> { "marketplace", "cantina", "dune sea", "ruins", "dock" };

        // Jungle/Forest worlds
        if (lower is "kashyyyk" or "endor" or "felucia" or "dagobah")
            return new List<string> { "outpost", "forest", "ruins", "wilderness", "caves" };

        // Urban / Ecumenopolis
        if (lower is "coruscant" or "nar shaddaa")
        {
            var zones = new List<string> { "market", "dock", "slums", "undercity", "government district" };
            // Jedi Temple zones are always available on Coruscant
            if (lower == "coruscant") zones.AddRange(GetJediTempleZones());
            return zones;
        }

        // Ocean/Aquatic
        if (lower is "kamino" or "mon calamari" or "manaan")
            return new List<string> { "dock", "research platform", "open ocean", "market" };

        // Ice/Arctic
        if (lower is "hoth" or "ilum")
            return new List<string> { "wastes", "caves", "ruins", "wilderness" };

        // Volcanic
        if (lower is "mustafar" or "malachor")
            return new List<string> { "ruins", "caves", "industrial platform", "volcanic flats", "wilderness" };

        // Mining worlds
        if (lower is "kessel" or "geonosis" or "lothal" or "mandalore")
            return new List<string> { "mine shaft", "dock", "market", "wilderness", "ruins" };

        // Standard Core / Mid Rim planet with dockyard
        if (Planets.TryGetValue(planetName, out var pd) && pd.HasDockyard)
            return new List<string> { "market", "dock", "slums", "ruins", "wilderness" };

        // Generic fallback
        return new List<string> { "market", "ruins", "wilderness", "dock" };
    }

    // Returns valid encounter actions for a specific zone on a given planet
    public List<string> GetZoneActions(string zone, string planetName)
    {
        var lower = zone.ToLowerInvariant();
        return lower switch
        {
            "market" or "marketplace" or "cantina" =>
                new List<string> { "Visit Merchant", "Talk to Contact", "Gather Intel", "Start Long Chat" },

            "dock" =>
                new List<string> { "Inspect Dock", "Visit Merchant", "Talk to Contact", "Start Long Chat" },

            "slums" or "undercity" =>
                new List<string> { "Raid Slums", "Loot Body", "Talk to Contact", "Scavenge Ruins" },

            "ruins" =>
                new List<string> { "Scavenge Ruins", "Loot Body", "Talk to Contact", "Harvest" },

            "wilderness" or "dune sea" or "wastes" =>
                new List<string> { "Track Wildlife", "Scavenge Ruins", "Loot Body", "Harvest" },

            "forest" =>
                new List<string> { "Track Wildlife", "Scavenge Ruins", "Chop Wood", "Harvest" },

            "swamp" =>
                new List<string> { "Track Wildlife", "Loot Body", "Harvest" },

            "caves" or "mine shaft" =>
                new List<string> { "Scavenge Ruins", "Track Wildlife", "Loot Body", "Mine Resources", "Harvest" },

            "government district" or "research platform" or "industrial platform" =>
                new List<string> { "Talk to Contact", "Gather Intel", "Start Long Chat" },

            "volcanic flats" =>
                new List<string> { "Talk to Contact", "Gather Intel", "Mine Resources", "Harvest" },

            "open ocean" =>
                new List<string> { "Track Wildlife", "Loot Body", "Harvest" },

            "jedi temple" or "council chamber" or "archive library" or "training hall"
                or "meditation garden" or "temple hangar" =>
                GetJediTempleActions(lower),

            _ => new List<string> { "Visit Merchant", "Talk to Contact", "Scavenge Ruins" }
        };
    }

    // Returns zones that require a specific vehicle type to access
    public bool CanAccessZone(GameCharacter character, string zone)
    {
        var lower = zone.ToLowerInvariant();
        // Zones that need a speeder or better
        var speedersRequired = new HashSet<string> { "dune sea", "wastes", "open ocean" };
        // Zones that need a walker
        var walkerRequired = new HashSet<string> { "industrial platform", "volcanic flats" };
        // Mount-accessible terrain
        var mountRequired = new HashSet<string> { "deep wilderness", "mountain pass" };

        if (speedersRequired.Contains(lower))
        {
            var hasVehicle = character.OwnedVehicles.Any(v =>
                Vehicles.TryGetValue(v, out var vd) &&
                vd.Type is "speeder" or "swoop" or "barge" or "walker");
            if (!hasVehicle) return false;
        }

        if (walkerRequired.Contains(lower))
        {
            var hasWalker = character.OwnedVehicles.Any(v =>
                Vehicles.TryGetValue(v, out var vd) && vd.Type is "walker");
            if (!hasWalker) return false;
        }

        if (mountRequired.Contains(lower))
        {
            var hasMount = character.OwnedVehicles.Any(v =>
                Vehicles.TryGetValue(v, out var vd) && vd.Type is "mount" or "speeder" or "walker");
            if (!hasMount) return false;
        }

        return true;
    }

    public string SetActiveVehicle(GameCharacter character, string vehicleName)
    {
        if (string.IsNullOrWhiteSpace(vehicleName))
        {
            character.ActiveVehicle = "";
            return "Active vehicle cleared.";
        }
        if (!character.OwnedVehicles.Any(v => string.Equals(v, vehicleName, StringComparison.OrdinalIgnoreCase)))
            return "You do not own that vehicle.";
        character.ActiveVehicle = vehicleName;
        var vehicleType = Vehicles.TryGetValue(vehicleName, out var vd) ? vd.Type : "vehicle";
        return $"You set your active {vehicleType}: {vehicleName}.";
    }

    public bool IsRaceAvailable(string raceName, string eraName, bool isMarried)
    {
        if (!Races.TryGetValue(raceName, out var race))
        {
            return false;
        }

        if (string.Compare(eraName, race.EraUnlock, StringComparison.OrdinalIgnoreCase) < 0)
        {
            return false;
        }

        return !race.RequiresMarriage || isMarried;
    }

    public string GetRaceAvailabilityMessage(string raceName, string eraName, bool isMarried)
    {
        if (!Races.TryGetValue(raceName, out var race))
        {
            return "That race is not recognized in this galaxy model.";
        }

        if (string.Compare(eraName, race.EraUnlock, StringComparison.OrdinalIgnoreCase) < 0)
        {
            return $"{race.Name} is locked to {race.EraUnlock} or later.";
        }

        if (race.RequiresMarriage && !isMarried)
        {
            return $"{race.Name} is only available to characters who are married.";
        }

        return "Available.";
    }

    public string GetCurrentEraName(int? rotation = null)
    {
        var currentRotation = rotation ?? Clock.Rotation;
        return Eras.Values.Where(era => currentRotation >= era.StartRotation).OrderByDescending(era => era.StartRotation).FirstOrDefault()?.Name ?? "Old Republic";
    }

    public string GetEraName() => GetCurrentEraName();

    public string GetPlanetEra(string planetName) => Planets[planetName].Era;

    public int GetPlanetRotation(string planetName) => PlanetClocks.TryGetValue(planetName, out var clock) ? clock.Rotation : Clock.Rotation;

    public WorldClock GetPlanetClock(string planetName) => PlanetClocks.TryGetValue(planetName, out var clock) ? clock : Clock;

    public void AdvanceWorldTime(int hours, string planetName, GameCharacter? character = null)
    {
        Clock.Advance(hours);
        AdvancePlanetEconomies(hours);
        foreach (var worldClock in PlanetClocks.Values)
        {
            worldClock.Advance(hours);
        }

        SimulateGalaxyBackground(hours, planetName);

        if (character is not null)
        {
            if (hours >= 24)
            {
                character.Age += 1;
            }

            ApplySurvivalTick(character, hours);

            var eraCurrency = GetCurrencyTypeForEra(GetCurrentEraName());
            if (string.IsNullOrWhiteSpace(character.CurrencyType) || character.CurrencyType is "Galactic Credits" or "Imperial Credits" or "Mandalorian Credits" or "New Republic Credits" or "Sequel Credits" or "Old Republic Credits")
            {
                character.CurrencyType = eraCurrency;
            }

            if (character.Age >= 90)
            {
                character.IsAlive = false;
                character.CurrentState = "Elderly";
                character.Condition = "Fading";
                if (character.Family.Children.Count > 0)
                {
                    character.Notes.Add("Legacy transferred to child after old age.");
                }
            }
        }

        // Progress any ongoing ship constructions
        if (ConstructionQueue.Count > 0)
        {
            var completed = new List<ConstructionProject>();
            foreach (var proj in ConstructionQueue)
            {
                proj.RemainingHours -= hours;
                if (proj.RemainingHours <= 0)
                {
                    var bp = proj.Blueprint;
                    proj.Owner.Ship = CreateShipFromBlueprint(bp, proj.Owner.Name);
                    proj.Owner.Experience += 30;
                    completed.Add(proj);
                }
            }

            foreach (var c in completed) ConstructionQueue.Remove(c);
        }
    }

    public void MarkPlanetDiscovered(string planetName) => DiscoveredPlanets.Add(planetName);

    public GameCharacter CreateCharacter(string name, string species, string role, string homeworld, string background, int age = 22)
    {
        var eraName = GetCurrentEraName();
        var raceName = IsRaceAvailable(species, eraName, false) ? species : "Human";
        var raceData = Races[raceName];

        // Jedi is no longer a selectable role — ensure it can't slip through
        if (role.Equals("Jedi", StringComparison.OrdinalIgnoreCase))
            role = "Smuggler";

        var roleStats = new Dictionary<string, int>
        {
            ["strength"] = 1, ["agility"] = 1, ["intellect"] = 1, ["presence"] = 1, ["vitality"] = 1
        };
        switch (role)
        {
            case "Engineer":  roleStats["intellect"] = 2; break;
            case "Pilot":     roleStats["agility"]   = 2; break;
            case "Scout":     roleStats["agility"]   = 2; break;
            case "Smuggler":  roleStats["presence"]  = 1; roleStats["agility"] = 1; break;
            case "Soldier":   roleStats["strength"]  = 2; break;
            case "Bounty Hunter": roleStats["strength"] = 1; roleStats["agility"] = 1; break;
        }

        var stats = new Dictionary<string, int>();
        foreach (var stat in new[] { "strength", "agility", "intellect", "presence", "vitality" })
            stats[stat] = raceData.BaseStats[stat] + roleStats[stat];

        var character = new GameCharacter
        {
            Name = name,
            Species = raceName,
            Role = role,
            Homeworld = homeworld,
            Background = background,
            Stats = stats,
            Skills = new List<string>(raceData.StartingSkills) { "Piloting", "Negotiation" },
            Inventory = new List<string> { "repair kit", "ration bar", "ration bar" },
            HangarInventory = new List<string>(),
            KnownBlueprints = new List<string>(),
            Crafting = new List<string> { "repair kit", "field medpack" },
            EquippedWeapon = "Blaster Pistol",
            Location = homeworld,
            Credits = role switch { "Smuggler" => 85, "Pilot" => 95, "Bounty Hunter" => 120, _ => 70 },
            CurrencyType = GetCurrencyTypeForEra(eraName),
            Armor = Math.Max(0, stats["vitality"] - 2),
            Hp = 24 + stats["vitality"] * 6,
            MaxHp = 24 + stats["vitality"] * 6,
            Stamina = 16 + stats["agility"] * 4,
            MaxStamina = 16 + stats["agility"] * 4,
            Stress = 0,
            Morale = 50 + stats["presence"] * 5,
            Condition = "Healthy",
            CurrentState = "Steady",
            Reputation = 0,
            Faction = "Independent",
            Experience = 0,
            Age = age,
            ForceResistance = raceName == "Miraluka" || raceName == "Sith",
            ForcePoints = 0,
            MaxForcePoints = 0,
            Hunger = 100,
            Energy = 100,
            LastMealRotation = 0
        };

        ConfigureLatentForcePotential(character);
        SeedStartingBlueprints(character);
        ApplySpeciesCharacterVariance(character);
        PopulateKnownSpeciesRelations(character);

        return character;
    }

    public Dictionary<string, object> GetPlanetState(string planetName, string timeOfDay)
    {
        var planet = Planets[planetName];
        var events = timeOfDay == "day" ? planet.DayEvents : planet.NightEvents;
        var eco = GetPlanetEconomyStatus(planetName);
        return new Dictionary<string, object>
        {
            ["name"] = planetName,
            ["description"] = planet.Description,
            ["economy"] = planet.Economy,
            ["economyStatus"] = eco.StatusText,
            ["resourceLevel"] = eco.ResourceLevel,
            ["imperialExtraction"] = eco.ImperialExtraction,
            ["timeOfDay"] = timeOfDay,
            ["events"] = new List<string>(events),
            ["era"] = planet.Era,
            ["threat"] = planet.ThreatLevel,
            ["facilities"] = GetPlanetFacilitySummary(planetName)
        };
    }

    public Dictionary<string, object> GetPlanetSnapshot(string planetName)
    {
        var clock = GetPlanetClock(planetName);
        return new Dictionary<string, object>
        {
            ["rotation"] = clock.Rotation,
            ["hour"] = clock.Hour,
            ["timeOfDay"] = clock.TimeOfDay
        };
    }

    public string GetStateSummary(GameCharacter character)
    {
        if (character.IsAlive == false)
        {
            return "Deceased";
        }

        // Survival states take priority
        if (!character.Species.Equals("Droid", StringComparison.OrdinalIgnoreCase))
        {
            if (character.Hunger <= 5)  return "Dying of Hunger";
            if (character.Energy <= 10) return "Exhausted";
            if (character.Hunger <= 20) return "Starving";
            if (character.Energy <= 25) return "Fatigued";
            if (character.Hunger <= 40) return "Hungry";
        }

        if (character.Reputation >= 35 && FactionStandings.Values.Any(value => value > 8))
        {
            return "Celebrated";
        }

        if (character.Reputation <= -20 || FactionStandings.GetValueOrDefault("Empire") > 10)
        {
            return "Under pressure";
        }

        if (character.Hp < character.MaxHp / 2)
        {
            return "Wounded";
        }

        if (character.HasLatentForcePotential && !character.ForcePotentialKnown)
        {
            return "Unsettled";
        }

        return "Steady";
    }

    private void ConfigureLatentForcePotential(GameCharacter character)
    {
        // Force sensitivity is always random — Jedi is never a starting role
        var chance = 0.10; // base 10% chance
        var sp = character.Species.ToLowerInvariant();
        if (sp is "miraluka" or "mirialan" or "togruta") chance += 0.12;
        if (sp is "human" or "zabrak")                    chance += 0.05;
        if (sp is "sith")                                 chance += 0.15;

        character.HasLatentForcePotential = random.NextDouble() < chance;
        character.ForcePotentialKnown     = false;
        character.ForceTrainingDeclined   = false;
        character.ForceSeekerFaction      = string.Empty;
        character.IsForceUser             = false;
        character.JediRank                = "";
        character.JediXp                  = 0;
        character.LightsaberCrafted       = false;
        character.ForceAbilities          = new List<string>();
        character.ForcePoints             = 0;
        character.MaxForcePoints          = 0;

        if (character.HasLatentForcePotential)
        {
            // Schedule the Jedi encounter between rotation 20 and 30
            character.ForceAwakeningRotation = 20 + random.Next(0, 11);
        }
    }

    /// <summary>Called every rotation tick — returns true if the awakening popup should show now.</summary>
    public bool ShouldTriggerJediAwakening(GameCharacter character)
    {
        if (!character.HasLatentForcePotential) return false;
        if (character.JediAwakeningTriggered)   return false;
        if (character.JediTrainingDeclined)     return false;
        if (character.IsForceUser)              return false;
        if (character.ForceAwakeningRotation < 0) return false;
        return Clock.Rotation >= character.ForceAwakeningRotation;
    }

    public JediAwakeningEvent BuildJediAwakeningEvent(GameCharacter character)
    {
        var masterName    = GenerateNameForRace("Human");
        character.JediMasterName = masterName;
        character.JediAwakeningTriggered = true;

        var lines = new[]
        {
            $"A cloaked figure steps from the shadows of the {character.Location} dock. \"{character.Name}. I have searched the galaxy for you for years. The Force is strong in you — I felt it across three systems.\"",
            $"A Jedi scout named {masterName} finds you in a quiet corner of {character.Location}. \"The Order sensed a tremor in the Force when you were born. You have a gift — and a choice.\"",
            $"\"{character.Name},\" says a calm voice. {masterName}, a Jedi Knight, steps into the light. \"We do not approach lightly. The Force called us here. What will you do with that power?\""
        };

        return new JediAwakeningEvent
        {
            MasterName  = masterName,
            Dialogue    = lines[random.Next(lines.Length)],
            OfferText   = "Will you travel to Coruscant and join the Jedi Order? Your path — and perhaps the galaxy's — depends on it.",
            TransportTo = "Coruscant"
        };
    }

    public string AcceptJediTraining(GameCharacter character)
    {
        character.IsForceUser          = true;
        character.ForcePotentialKnown  = true;
        character.ForceTrainingDeclined = false;
        character.ForceSeekerFaction   = "Jedi";
        character.JediRank             = "Initiate";
        character.JediXp               = 0;
        character.MaxForcePoints       = 10;
        character.ForcePoints          = 10;
        character.Location             = "Coruscant";
        if (!character.Skills.Contains("Force Discipline")) character.Skills.Add("Force Discipline");
        if (!character.Skills.Contains("Lightsaber"))       character.Skills.Add("Lightsaber");
        character.ForceAbilities = new List<string> { "Force Push", "Force Sense" };
        character.Notes.Add($"Accepted Jedi training under {character.JediMasterName}. Transported to Coruscant.");
        FactionStandings["Jedi"] = Math.Max(FactionStandings.GetValueOrDefault("Jedi"), 1);
        return $"You accept. {character.JediMasterName} nods quietly. \"Welcome, Initiate.\" A Jedi transport carries you to Coruscant.";
    }

    public string DeclineJediTraining(GameCharacter character)
    {
        character.JediTrainingDeclined  = true;
        character.ForcePotentialKnown   = true;
        character.IsForceUser           = false;
        character.Notes.Add("Declined Jedi Order recruitment.");
        return "You decline. The Jedi nods with a hint of sorrow. \"The offer stands if you change your mind.\" They disappear into the crowd.";
    }

    // ─── Jedi Rank & Progression ──────────────────────────────────────────────
    private static readonly Dictionary<string, (int xpNeeded, string[] newAbilities, int fpGain)> JediRankThresholds = new()
    {
        ["Initiate"] = (0,   new[] { "Force Push", "Force Sense" },                      10),
        ["Padawan"]  = (80,  new[] { "Force Pull", "Mind Trick", "Force Speed" },         18),
        ["Knight"]   = (220, new[] { "Force Leap", "Saber Throw", "Force Barrier" },      28),
        ["Master"]   = (450, new[] { "Force Storm", "Battle Meditation", "Force Drain" }, 40),
    };

    private static readonly string[] JediRankOrder = { "Initiate", "Padawan", "Knight", "Master" };

    public string AwardJediXp(GameCharacter character, int xp)
    {
        if (!character.IsForceUser) return "";
        character.JediXp  += xp;
        character.Experience += xp / 2;
        return CheckJediRankPromotion(character);
    }

    private string CheckJediRankPromotion(GameCharacter character)
    {
        var currentIdx = Array.IndexOf(JediRankOrder, character.JediRank);
        if (currentIdx < 0) currentIdx = 0;
        if (currentIdx >= JediRankOrder.Length - 1) return "";

        var nextRank = JediRankOrder[currentIdx + 1];
        if (!JediRankThresholds.TryGetValue(nextRank, out var threshold)) return "";
        if (character.JediXp < threshold.xpNeeded) return "";

        character.JediRank      = nextRank;
        character.MaxForcePoints = threshold.fpGain;
        character.ForcePoints   = threshold.fpGain;
        FactionStandings["Jedi"] = FactionStandings.GetValueOrDefault("Jedi") + 2;

        foreach (var ability in threshold.newAbilities)
            if (!character.ForceAbilities.Contains(ability))
                character.ForceAbilities.Add(ability);

        // Knight gets a Jedi starfighter
        var shipGift = "";
        if (nextRank == "Knight" && character.Ship is null)
        {
            if (ShipCatalog.TryGetValue("Jedi Starfighter", out var starfighter))
            {
                character.Ship = new Ship
                {
                    Name = $"The {character.Name} Wing",
                    Model = starfighter.Model,
                    SizeClass = "S",
                    HyperdriveClass = 1,
                    Hull = starfighter.Hull,
                    Shield = starfighter.Shield,
                    Fuel = starfighter.Fuel,
                    MaxFuel = starfighter.Fuel,
                    Weapon = starfighter.Weapon,
                    Armaments = new List<string>(),
                    Parts = new List<string> { "navicomputer", "shield generator" },
                    Upgrades = new List<string>()
                };
                shipGift = " The Order grants you a Jedi Starfighter.";
            }
        }

        character.Notes.Add($"Promoted to Jedi {nextRank}.");
        return $"The Jedi Council recognises your growth. You are now a Jedi {nextRank}!{shipGift}";
    }

    // ─── Lightsaber Crafting Quest ────────────────────────────────────────────
    private static readonly string[] LightsaberColors = { "blue", "green", "yellow", "white", "purple" };

    public (bool canCraft, string message) CheckLightsaberCraftRequirements(GameCharacter character)
    {
        if (!character.IsForceUser)
            return (false, "Only Force users may craft a lightsaber.");
        if (character.LightsaberCrafted)
            return (false, "You already carry a lightsaber.");
        if (character.JediRank is "" or "Initiate")
            return (false, "You must reach Padawan rank before crafting your lightsaber.");

        var inv = character.Inventory.Select(x => x.ToLowerInvariant()).ToList();
        var missingParts = new List<string>();
        if (!inv.Any(x => x.Contains("kyber crystal")))     missingParts.Add("kyber crystal shard (mine on Ilum)");
        if (!inv.Any(x => x.Contains("durasteel")))         missingParts.Add("durasteel ingot/scrap");
        if (!inv.Any(x => x.Contains("power cell")))        missingParts.Add("power cell");

        if (missingParts.Count > 0)
            return (false, $"Missing components: {string.Join(", ", missingParts)}.");

        return (true, "You have all the parts.");
    }

    public string CraftLightsaber(GameCharacter character, string? preferredColor = null)
    {
        var (canCraft, msg) = CheckLightsaberCraftRequirements(character);
        if (!canCraft) return msg;

        // Consume components
        RemoveFirstMatchingItem(character.Inventory, x => x.Contains("kyber crystal"));
        RemoveFirstMatchingItem(character.Inventory, x => x.Contains("durasteel"));
        RemoveFirstMatchingItem(character.Inventory, x => x.Contains("power cell"));

        var color = preferredColor ?? LightsaberColors[random.Next(LightsaberColors.Length)];
        character.LightsaberCrafted = true;
        character.LightsaberColor   = color;
        character.EquippedWeapon    = $"{color} lightsaber";
        character.Inventory.Add($"{color} lightsaber");
        character.JediXp += 30;
        character.Notes.Add($"Crafted a {color} lightsaber on {character.Location}.");
        var rankMsg = CheckJediRankPromotion(character);
        return $"In meditation, you pour your will into the crystal. The {color} blade ignites — yours to wield.{(rankMsg.Length > 0 ? " " + rankMsg : "")}";
    }

    private static void RemoveFirstMatchingItem(List<string> list, Func<string, bool> predicate)
    {
        var idx = list.FindIndex(x => predicate(x.ToLowerInvariant()));
        if (idx >= 0) list.RemoveAt(idx);
    }

    // ─── Coruscant Jedi Temple Zones ─────────────────────────────────────────
    public List<string> GetJediTempleZones()
        => new() { "Jedi Temple", "Council Chamber", "Archive Library", "Training Hall", "Meditation Garden", "Temple Hangar" };

    public List<string> GetJediTempleActions(string zone)
        => zone.ToLowerInvariant() switch
        {
            "jedi temple"       => new List<string> { "Meditate", "Speak with Master", "Review Mission Board" },
            "council chamber"   => new List<string> { "Attend Council", "Request Promotion", "Report Mission" },
            "archive library"   => new List<string> { "Research Force Lore", "Study Holocron", "Learn Lightsaber Form" },
            "training hall"     => new List<string> { "Spar with Initiate", "Spar with Padawan", "Kata Practice" },
            "meditation garden" => new List<string> { "Deep Meditation", "Commune with Force", "Seek Vision" },
            "temple hangar"     => new List<string> { "Inspect Assigned Ship", "Prepare for Mission" },
            _ => new List<string> { "Meditate" }
        };

    public string ExecuteJediTempleAction(GameCharacter character, string zone, string action)
    {
        if (!character.IsForceUser) return "You are not attuned to the Force.";
        var lower = action.ToLowerInvariant();

        if (lower == "meditate" || lower == "deep meditation")
        {
            character.ForcePoints = Math.Min(character.MaxForcePoints, character.ForcePoints + 4);
            character.Stress      = Math.Max(0, character.Stress - 5);
            AdvanceWorldTime(2, "Coruscant", character);
            return "You sit in stillness. The Force flows back into you. (+4 FP, -5 stress)";
        }

        if (lower == "commune with force" || lower == "seek vision")
        {
            character.ForcePoints = Math.Min(character.MaxForcePoints, character.ForcePoints + 6);
            var visions = new[]
            {
                "You see fragments of a coming threat in the Outer Rim.",
                "A face appears — someone important to your path.",
                "The Force shows you Ilum, its crystal caves glowing.",
                "A vision of your future self, standing at a crossroads."
            };
            var xpGain = AwardJediXp(character, 8);
            return $"{visions[random.Next(visions.Length)]} (+8 Jedi XP){(xpGain.Length > 0 ? " " + xpGain : "")}";
        }

        if (lower.Contains("spar"))
        {
            var xpGain = AwardJediXp(character, 12);
            character.ForcePoints = Math.Max(0, character.ForcePoints - 2);
            return $"Training ends with both combatants breathing hard. (+12 Jedi XP){(xpGain.Length > 0 ? " " + xpGain : "")}";
        }

        if (lower == "study holocron" || lower == "research force lore" || lower == "learn lightsaber form")
        {
            var knowledge = new[]
            {
                "You learn Form II: Makashi — precise, elegant.",
                "You study Ataru — acrobatic and aggressive.",
                "The holocron reveals ancient Force healing techniques.",
                "You deepen your understanding of Shii-Cho fundamentals."
            };
            var xpGain = AwardJediXp(character, 10);
            return $"{knowledge[random.Next(knowledge.Length)]} (+10 Jedi XP){(xpGain.Length > 0 ? " " + xpGain : "")}";
        }

        if (lower == "request promotion")
        {
            var rankMsg = CheckJediRankPromotion(character);
            return rankMsg.Length > 0 ? rankMsg : $"The Council evaluates your record. You are at {character.JediXp} XP — keep training, {character.JediRank}.";
        }

        if (lower == "attend council")
        {
            character.Reputation += 2;
            FactionStandings["Jedi"] = FactionStandings.GetValueOrDefault("Jedi") + 1;
            return "The Council acknowledges your presence. Your standing with the Order grows.";
        }

        return "You spend time in the temple, letting the Force guide your thoughts.";
    }

    // ─── Keep backward compatibility hook ────────────────────────────────────
    // ═══════════════════════════════════════════════════════════════════════════
    // ARMOR DATABASE
    // ═══════════════════════════════════════════════════════════════════════════
    private void InitArmors()
    {
        void A(string name, string cat, string desc, int ar, int hp, int stam, int mob,
               bool heat=false, bool cold=false, bool acid=false, bool tox=false,
               bool vac=false, bool rad=false, bool water=false, bool stealth=false,
               bool saber=false, bool life=false, int val=0, bool craft=true,
               string era="All", string faction="")
        {
            Armors[name] = new ArmorBlueprint
            {
                Name=name, Category=cat, Description=desc, ArmorRating=ar, HpBonus=hp,
                StaminaBonus=stam, MobilityPenalty=mob, HeatResistance=heat,
                ColdResistance=cold, AcidResistance=acid, ToxinResistance=tox,
                VacuumSealed=vac, RadiationShielded=rad, WaterResistant=water,
                StealthBonus=stealth, LightsaberDampening=saber, LifeSupport=life,
                BaseValue=val, Craftable=craft, EraAvailable=era, FactionRequired=faction
            };
        }

        // ── Light Armor ────────────────────────────────────────────────────────
        A("Jedi Robes",           "light",  "Traditional robes worn by Force-sensitive Jedi.",       ar:2,  hp:5,  stam:2, mob:0, val:80,  craft:true);
        A("Sith Acolyte Robes",   "light",  "Dark-dyed robes that dampen Force detection.",           ar:3,  hp:8,  stam:2, mob:0, val:100, craft:true);
        A("Rebel Soldier Vest",   "light",  "Padded vest issued to Alliance troopers.",               ar:3,  hp:8,  stam:3, mob:0, val:120, craft:true);
        A("Smuggler's Jacket",    "light",  "Reinforced jacket with concealed compartments.",         ar:2,  hp:5,  stam:4, mob:0, stealth:true, val:90, craft:true);
        A("Pilot Flight Suit",    "light",  "Pressurised suit for starfighter pilots.",               ar:3,  hp:8,  stam:2, mob:0, vac:true, val:140, craft:true);
        A("Jawa Robe",            "light",  "Coarse sand-cloth robe of the Jawa scavengers.",        ar:1,  hp:2,  stam:1, mob:0, heat:true, stealth:true, val:20, craft:true);
        A("Tusken Raider Wraps",  "light",  "Layered cloth strips protecting from desert heat.",     ar:3,  hp:6,  stam:2, mob:0, heat:true, val:50, craft:true);
        A("Padawan Robes",        "light",  "Simple Jedi training attire.",                          ar:2,  hp:4,  stam:3, mob:0, val:60,  craft:true);
        A("Handmaiden Bodysuit",  "light",  "Elegant bodysuit worn by Naboo handmaidens.",           ar:2,  hp:5,  stam:4, mob:0, stealth:true, val:95, craft:true);
        A("Resistance Vest",      "light",  "Lightweight armour vest of the Resistance.",            ar:4,  hp:10, stam:3, mob:0, val:150, craft:true, era:"Sequel Trilogy");
        A("Scout Cloak",          "light",  "A hooded cloak used for reconnaissance.",               ar:2,  hp:4,  stam:2, mob:0, stealth:true, val:80, craft:true);
        A("Zygerrian Slaver Gear","light",  "Light leather armour of Zygerrian slave masters.",     ar:3,  hp:6,  stam:3, mob:0, val:70,  craft:true);

        // ── Medium Armor ───────────────────────────────────────────────────────
        A("Clone Trooper Armor Phase I",  "medium","First-generation Kaminoan clone battle armour.", ar:8,  hp:15, stam:5, mob:1, heat:true, val:360, craft:true, era:"Clone Wars");
        A("Clone Trooper Armor Phase II", "medium","Refined clone armour with improved ergonomics.", ar:10, hp:18, stam:5, mob:1, heat:true, cold:true, val:440, craft:true, era:"Clone Wars");
        A("ARC Trooper Armor",     "medium","Elite Advanced Recon Commando plate.",                  ar:11, hp:20, stam:5, mob:1, heat:true, cold:true, vac:true, val:520, craft:true, era:"Clone Wars");
        A("Republic Commando Suit","medium","Hardened battle-suit of the Republic commandos.",       ar:12, hp:22, stam:6, mob:2, heat:true, cold:true, rad:true, val:600, craft:true, era:"Clone Wars");
        A("Stormtrooper Armor",    "medium","Standard Imperial stormtrooper battle armour.",         ar:8,  hp:14, stam:4, mob:1, val:280, craft:true, era:"Original Trilogy", faction:"Empire");
        A("Scout Trooper Armor",   "medium","Lightweight armour for Imperial scouts.",               ar:6,  hp:12, stam:5, mob:0, stealth:true, val:220, craft:true, era:"Original Trilogy");
        A("Snowtrooper Armor",     "medium","Environmentally sealed armour for arctic conditions.",  ar:8,  hp:15, stam:4, mob:1, cold:true, vac:true, val:320, craft:true, era:"Original Trilogy");
        A("Sandtrooper Armor",     "medium","Stormtrooper armour modified for desert operations.",   ar:8,  hp:14, stam:4, mob:1, heat:true, val:300, craft:true, era:"Original Trilogy");
        A("Mudtrooper Armor",      "medium","Heavy-duty armour for swamp/mud warfare.",              ar:6,  hp:10, stam:3, mob:2, water:true, acid:true, val:240, craft:true, era:"Original Trilogy");
        A("Rebel Alliance Armor",  "medium","Heterogeneous rebel trooper battle plate.",             ar:7,  hp:13, stam:5, mob:1, val:240, craft:true, era:"Original Trilogy");
        A("Mandalorian Armor",     "medium","Iron Mandalorian full-body armour.",                    ar:10, hp:20, stam:5, mob:1, val:480, craft:true, faction:"Mandalorians");
        A("Bounty Hunter Armor",   "medium","Customised plate worn by hunters.",                     ar:9,  hp:16, stam:5, mob:1, stealth:true, val:400, craft:true);
        A("Shadow Guard Armor",    "medium","Crimson-trimmed Imperial Force guard plate.",            ar:10, hp:18, stam:4, mob:1, val:450, craft:true, era:"Original Trilogy", faction:"Empire");
        A("Sabine Wren Armor",     "medium","Painted custom Mandalorian plate.",                     ar:9,  hp:16, stam:6, mob:0, val:420, craft:true, faction:"Mandalorians");
        A("Inquisitor Armor",      "medium","Armour of the Emperor's dark-side Inquisitors.",        ar:10, hp:18, stam:4, mob:1, saber:true, val:500, craft:true, era:"Original Trilogy");
        A("First Order Armor",     "medium","Modernised stormtrooper armour of the First Order.",    ar:9,  hp:15, stam:4, mob:1, val:350, craft:true, era:"Sequel Trilogy");
        A("Purge Trooper Armor",   "medium","Specialised armour for hunting Force-users.",           ar:13, hp:22, stam:4, mob:2, saber:true, val:580, craft:true, era:"Original Trilogy");
        A("Imperial Officer Coat", "medium","Formal Imperial officer field coat.",                   ar:4,  hp:10, stam:5, mob:0, val:200, craft:true, era:"Original Trilogy");
        A("Weequay Battle Armor",  "medium","Battered pirate plate worn by Weequay mercenaries.",   ar:6,  hp:12, stam:4, mob:1, val:160, craft:true);
        A("Nikto Battle Armor",    "medium","Rough-hewn armour of Hutt clan enforcers.",             ar:6,  hp:11, stam:4, mob:1, val:150, craft:true);

        // ── Heavy Armor ────────────────────────────────────────────────────────
        A("Beskar Mandalorian Armor","heavy","Armour forged from near-indestructible beskar iron.", ar:18, hp:30, stam:6, mob:2, heat:true, cold:true, acid:true, tox:true, vac:true, saber:true, val:1800, craft:true, faction:"Mandalorians");
        A("Boba Fett Armor",        "heavy","Iconic green battle armour worn by Boba Fett.",        ar:16, hp:28, stam:6, mob:2, heat:true, cold:true, saber:false, val:1400, craft:false);
        A("Jango Fett Armor",       "heavy","Beskar-reinforced armour of the original Mandalorian template.", ar:15, hp:26, stam:6, mob:2, val:1200, craft:false);
        A("Death Trooper Armor",    "heavy","Experimental black armour with stealth coating.",      ar:14, hp:25, stam:5, mob:2, tox:true, rad:true, stealth:true, val:900, craft:true, era:"Original Trilogy");
        A("Imperial Royal Guard Armor","heavy","The crimson full-plate armour of the Emperor's Guard.", ar:12, hp:22, stam:4, mob:2, val:700, craft:true, era:"Original Trilogy");
        A("Sith Battle Armor",      "heavy","Dark-alloy plate imbued with Sith alchemy.",           ar:15, hp:28, stam:5, mob:2, saber:true, val:1200, craft:true);
        A("Vonduun Crab Armor",     "heavy","Living armour grown from Vonduun crab shell.",         ar:17, hp:30, stam:5, mob:2, acid:true, tox:true, water:true, val:2000, craft:false);
        A("Dark Trooper Phase III", "heavy","Fully mechanised droid-worn superarmour.",             ar:20, hp:35, stam:0, mob:3, heat:true, cold:true, acid:true, tox:true, vac:true, rad:true, val:2500, craft:false);
        A("Bo-Katan Armor",         "heavy","Blue Mandalorian armour fit for a leader.",            ar:16, hp:28, stam:6, mob:1, saber:false, val:1300, craft:false);
        A("Din Djarin Beskar",      "heavy","Full beskar armour forged by the Armorer.",            ar:18, hp:32, stam:6, mob:2, heat:true, cold:true, saber:true, val:2000, craft:false);

        // ── Exotic ────────────────────────────────────────────────────────────
        A("Darth Vader's Armor",    "exotic","Life-supporting armour that sustains a broken body.", ar:22, hp:40, stam:6, mob:3, heat:true, cold:true, acid:true, tox:true, vac:true, rad:true, life:true, saber:false, val:9999, craft:false);
        A("Kylo Ren's Armor",       "exotic","Shattered-and-repaired plate of the dark side aspirant.", ar:16, hp:30, stam:5, mob:2, val:3000, craft:false);

        // ── Armor recipes ──────────────────────────────────────────────────────
        void R(string name, bool forge, bool bp, int credits, params (string item, int qty)[] mats)
        {
            ArmorRecipes[name] = new ArmorRecipe
            {
                ArmorName=name, RequiresForge=forge, RequiresBlueprint=bp, CreditCost=credits,
                Materials=mats.ToList()
            };
        }

        R("Jedi Robes",            false, false, 40,  ("rough cloth", 3), ("dye kit", 1));
        R("Sith Acolyte Robes",    false, false, 50,  ("rough cloth", 3), ("black dye", 1), ("leather strip", 1));
        R("Rebel Soldier Vest",    true,  false, 80,  ("durasteel scrap", 2), ("rubber seal compound", 1));
        R("Smuggler's Jacket",     false, false, 60,  ("leather strip", 3), ("durasteel scrap", 1));
        R("Pilot Flight Suit",     true,  false, 110, ("rubber seal compound", 2), ("durasteel scrap", 2), ("transparisteel fragment", 1));
        R("Tusken Raider Wraps",   false, false, 30,  ("rough cloth", 4), ("sand mineral", 2));
        R("Scout Cloak",           false, false, 50,  ("rough cloth", 3), ("dye kit", 1));
        R("Rebel Alliance Armor",  true,  false, 160, ("durasteel ingot", 2), ("rubber seal compound", 1), ("plastoid alloy", 1));
        R("Clone Trooper Armor Phase I", true, true, 280, ("plastoid alloy", 3), ("durasteel ingot", 2), ("transparisteel fragment", 1), ("rubber seal compound", 2));
        R("Clone Trooper Armor Phase II", true, true, 340, ("plastoid alloy", 4), ("durasteel ingot", 3), ("transparisteel fragment", 1), ("rubber seal compound", 2), ("cooling unit", 1));
        R("ARC Trooper Armor",     true,  true,  420, ("plastoid alloy", 4), ("durasteel ingot", 3), ("transparisteel fragment", 2), ("rubber seal compound", 3), ("vacuum seal kit", 1));
        R("Republic Commando Suit",true,  true,  500, ("plastoid alloy", 5), ("titanite ingot", 2), ("transparisteel fragment", 2), ("rubber seal compound", 3), ("vacuum seal kit", 1), ("radiation liner", 1));
        R("Stormtrooper Armor",    true,  true,  200, ("plastoid alloy", 3), ("durasteel ingot", 2), ("transparisteel fragment", 1));
        R("Scout Trooper Armor",   true,  true,  170, ("plastoid alloy", 2), ("durasteel ingot", 2));
        R("Snowtrooper Armor",     true,  true,  260, ("plastoid alloy", 3), ("durasteel ingot", 2), ("cooling unit", 2), ("vacuum seal kit", 1));
        R("Sandtrooper Armor",     true,  true,  240, ("plastoid alloy", 3), ("durasteel ingot", 2), ("cooling unit", 1));
        R("Mudtrooper Armor",      true,  false, 190, ("durasteel ingot", 2), ("rubber seal compound", 3), ("acid liner", 1));
        R("Mandalorian Armor",     true,  true,  400, ("iron ingot", 4), ("durasteel ingot", 2), ("transparisteel fragment", 2), ("rubber seal compound", 2));
        R("Bounty Hunter Armor",   true,  false, 320, ("durasteel ingot", 3), ("titanite ingot", 1), ("rubber seal compound", 2));
        R("Beskar Mandalorian Armor", true, true, 1400, ("beskar ingot", 4), ("titanite ingot", 2), ("transparisteel fragment", 2), ("rubber seal compound", 3), ("vacuum seal kit", 2), ("cortosis weave", 1));
        R("Death Trooper Armor",   true,  true,  700, ("durasteel ingot", 3), ("cortosis weave", 1), ("toxin liner", 1), ("radiation liner", 1), ("stealth coating", 1));
        R("Imperial Royal Guard Armor", true, true, 550, ("durasteel ingot", 4), ("titanite ingot", 2), ("transparisteel fragment", 1));
        R("Sith Battle Armor",     true,  true,  900, ("durasteel ingot", 4), ("cortosis weave", 2), ("beskar fragment", 1), ("sith alchemy resin", 1));
        R("Inquisitor Armor",      true,  true,  420, ("durasteel ingot", 3), ("cortosis weave", 1), ("transparisteel fragment", 1));
        R("Purge Trooper Armor",   true,  true,  460, ("durasteel ingot", 4), ("cortosis weave", 2), ("transparisteel fragment", 1));
        R("Jawa Robe",             false, false, 15,  ("rough cloth", 4));
        R("Handmaiden Bodysuit",   false, false, 70,  ("silk cloth", 2), ("durasteel scrap", 1));
        R("Shadow Guard Armor",    true,  true,  380, ("durasteel ingot", 3), ("cortosis weave", 1));
        R("Sabine Wren Armor",     true,  true,  350, ("iron ingot", 3), ("durasteel ingot", 2), ("transparisteel fragment", 1), ("paint kit", 2));

        // ── Foraging / Scout Armor ──────────────────────────────────────────────
        // These require direct dictionary writes to carry ForagingBonus
        Armors["Scout Ranger Vest"] = new ArmorBlueprint { Name = "Scout Ranger Vest", Category = "light", Description = "Lightweight vest with hidden pouches for gathered materials.", ArmorRating = 2, HpBonus = 4, StaminaBonus = 3, MobilityPenalty = 0, StealthBonus = true, ForagingBonus = 1, BaseValue = 80, Craftable = true, Slot = "Chest" };
        Armors["Rebel Scout Gear"] = new ArmorBlueprint { Name = "Rebel Scout Gear", Category = "light", Description = "Standard Alliance scout kit with foraging pouches and camouflage.", ArmorRating = 3, HpBonus = 6, StaminaBonus = 4, MobilityPenalty = 0, StealthBonus = true, ForagingBonus = 2, BaseValue = 150, Craftable = true, EraAvailable = "Original Trilogy", Slot = "Full Suit" };
        Armors["Endor Scout Armor"] = new ArmorBlueprint { Name = "Endor Scout Armor", Category = "light", Description = "Forest camouflage worn by Endor rebels. Excellent for gathering and foraging.", ArmorRating = 4, HpBonus = 8, StaminaBonus = 5, MobilityPenalty = 0, StealthBonus = true, ForagingBonus = 3, BaseValue = 200, Craftable = true, EraAvailable = "Original Trilogy", Slot = "Full Suit" };
        Armors["Clone Scout Plate"] = new ArmorBlueprint { Name = "Clone Scout Plate", Category = "medium", Description = "Lightweight clone reconnaissance armor for long wilderness operations.", ArmorRating = 7, HpBonus = 12, StaminaBonus = 5, MobilityPenalty = 0, StealthBonus = true, ForagingBonus = 2, BaseValue = 340, Craftable = true, EraAvailable = "Clone Wars", Slot = "Full Suit" };
        Armors["Mandalorian Tracker Plate"] = new ArmorBlueprint { Name = "Mandalorian Tracker Plate", Category = "medium", Description = "Hunter's plate with integrated wilderness sensor suites for tracking and foraging.", ArmorRating = 9, HpBonus = 16, StaminaBonus = 5, MobilityPenalty = 1, ForagingBonus = 3, BaseValue = 480, Craftable = true, FactionRequired = "Mandalorians", Slot = "Full Suit" };
        Armors["Ghost Company Scout Armor"] = new ArmorBlueprint { Name = "Ghost Company Scout Armor", Category = "medium", Description = "Modified clone scout armor used by Ghost Company on Kashyyyk — optimised for forest gathering.", ArmorRating = 8, HpBonus = 14, StaminaBonus = 6, MobilityPenalty = 0, StealthBonus = true, ForagingBonus = 4, BaseValue = 550, Craftable = true, EraAvailable = "Clone Wars", Slot = "Full Suit" };

        // Armor recipes for foraging suits
        ArmorRecipes["Scout Ranger Vest"] = new ArmorRecipe { ArmorName = "Scout Ranger Vest", CreditCost = 40, Materials = new List<(string, int)> { ("rough cloth", 3), ("leather strip", 2) } };
        ArmorRecipes["Rebel Scout Gear"] = new ArmorRecipe { ArmorName = "Rebel Scout Gear", CreditCost = 80, Materials = new List<(string, int)> { ("rough cloth", 4), ("durasteel scrap", 1), ("dye kit", 1) } };
        ArmorRecipes["Endor Scout Armor"] = new ArmorRecipe { ArmorName = "Endor Scout Armor", CreditCost = 110, Materials = new List<(string, int)> { ("rough cloth", 4), ("durasteel scrap", 2), ("forest mushroom", 1) } };
        ArmorRecipes["Clone Scout Plate"] = new ArmorRecipe { ArmorName = "Clone Scout Plate", RequiresForge = true, CreditCost = 220, Materials = new List<(string, int)> { ("plastoid alloy", 2), ("durasteel ingot", 2) } };
        ArmorRecipes["Mandalorian Tracker Plate"] = new ArmorRecipe { ArmorName = "Mandalorian Tracker Plate", RequiresForge = true, CreditCost = 320, Materials = new List<(string, int)> { ("iron ingot", 3), ("durasteel ingot", 2), ("sensor array", 1) } };
        ArmorRecipes["Ghost Company Scout Armor"] = new ArmorRecipe { ArmorName = "Ghost Company Scout Armor", RequiresForge = true, CreditCost = 360, Materials = new List<(string, int)> { ("plastoid alloy", 3), ("durasteel ingot", 2), ("stealth coating", 1) } };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REFINING RECIPES
    // ═══════════════════════════════════════════════════════════════════════════
    private void InitRefiningRecipes()
    {
        void Ref(string raw, string refined, int qty=1, string loc="", string fac="forge", int time=1, int credits=0)
        {
            RefiningRecipes[raw] = new RefiningRecipe
            {
                RawMaterial=raw, RefinedOutput=refined, Quantity=qty,
                LocationRequired=loc, FacilityRequired=fac,
                TimeCost=time, CreditCost=credits
            };
        }

        // ── Common metals ──────────────────────────────────────────────────────
        Ref("iron ore",          "iron ingot",            qty:2);
        Ref("durasteel scrap",   "durasteel ingot",       qty:1);
        Ref("titanite ore",      "titanite ingot",        qty:1, fac:"forge", time:2);
        Ref("raw ore",           "refined ore",           qty:1);
        Ref("doonium ore",       "plastoid alloy",        qty:1, fac:"industrial forge", time:2, credits:10);
        Ref("silicite ore",      "transparisteel fragment", qty:1, fac:"industrial forge", time:2, credits:15);

        // ── Rare / location-locked ─────────────────────────────────────────────
        Ref("beskar fragment",   "beskar ingot",          qty:1, loc:"Mandalore", fac:"mandalorian forge", time:3, credits:50);
        Ref("beskar-laced clay", "beskar fragment",       qty:1, loc:"Mandalore", fac:"mandalorian forge", time:2, credits:20);
        Ref("cortosis ore",      "cortosis weave",        qty:1, fac:"industrial forge", time:3, credits:40);
        Ref("tibanna gas canister","refined tibanna",     qty:2, loc:"Bespin",    fac:"gas_processor", time:1, credits:20);
        Ref("gas mineral",       "refined tibanna",       qty:1, loc:"Bespin",    fac:"gas_processor", time:1, credits:30);
        Ref("kyber crystal shard","kyber crystal",        qty:1, loc:"Ilum",      fac:"jedi_sanctum",  time:2);
        Ref("white kyber fragment","kyber crystal",       qty:1, loc:"Ilum",      fac:"jedi_sanctum",  time:2);
        Ref("glitterstim residue","pure glitterstim shard",qty:1,loc:"Kessel",    fac:"spice_mill",    time:1, credits:30);
        Ref("spice trace",       "pure spice crystal",   qty:1, loc:"Kessel",    fac:"spice_mill",    time:1, credits:25);
        Ref("ryll spice trace",  "pure ryll crystal",    qty:1, loc:"Ryloth",    fac:"spice_mill",    time:1, credits:20);
        Ref("coaxium trace",     "processed coaxium",    qty:1, fac:"shipyard",  time:2, credits:60);
        Ref("obsidian shard",    "volcanic glass pane",  qty:1, fac:"forge",     time:1);
        Ref("permafrost crystal","cryogenic compound",   qty:1, fac:"lab",       time:2, credits:25);
        Ref("naboo plasma crystal","refined plasma cell",qty:2, fac:"lab",       time:1, credits:15);
        Ref("adegan crystal fragment","adegan crystal",  qty:1, fac:"jedi_sanctum", time:2);
        Ref("dark obsidian",     "sith-tainted shard",   qty:1, fac:"forge",     time:1);
        Ref("sith holocron fragment","sith-tainted shard",qty:1,fac:"forge",     time:1);
        Ref("cortosis trace",    "cortosis weave",       qty:1, fac:"industrial forge", time:2, credits:50);
        Ref("force-imbued sediment","dark side crystal", qty:1, loc:"Dagobah",   fac:"jedi_sanctum",  time:3);
        Ref("sea mineral",       "brine salt",           qty:2);
        Ref("brine crystal",     "kaminoan bio-resin",   qty:1, loc:"Kamino",    fac:"lab",           time:2, credits:20);
        Ref("bog root",          "symbiote moss extract",qty:1, fac:"lab",       time:1);
        Ref("volcanic ash",      "cooling unit",         qty:1, fac:"forge",     time:1);
        Ref("magma stone",       "heat-treated alloy",   qty:1, fac:"forge",     time:2);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SUB-ZONE DESTINATIONS
    // ═══════════════════════════════════════════════════════════════════════════
    private void InitSubZones()
    {
        void SZ(string planet, string name, string parentZone, string desc, string threat, string shipUpg, string shipMod, string faction, params string[] actions)
        {
            if (!SubZones.ContainsKey(planet)) SubZones[planet] = new List<SubZoneDestination>();
            SubZones[planet].Add(new SubZoneDestination
            {
                Name=name, Planet=planet, ParentZone=parentZone, Description=desc,
                ThreatLevel=threat, ShipUpgradeRequired=shipUpg, ShipModuleRequired=shipMod,
                FactionRequired=faction, Actions=actions.ToList()
            });
        }

        SZ("Tatooine",   "Krayt Dragon Caves",      "wilderness", "Ancient caves where krayt dragons nest.", "Deadly", "", "", "", "Mine Resources", "Scan Veins", "Hunt");
        SZ("Tatooine",   "Dune Sea Depths",          "wilderness", "Open desert riddled with undiscovered ruins.", "High", "", "", "", "Mine Resources", "Scan Veins", "Scavenge");
        SZ("Tatooine",   "Jawa Sandcrawler Yard",   "market",     "Jawa merchants trading in salvage.",          "Low",  "", "", "", "Visit Merchant", "Barter");

        SZ("Kashyyyk",   "Shadowlands Lower Level", "forest",     "The pitch-dark forest floor teeming with predators.", "Deadly", "", "", "", "Mine Resources", "Chop Wood", "Hunt", "Scan Veins");
        SZ("Kashyyyk",   "Wroshyr Tree Canopy",     "forest",     "Ancient canopy; wroshyr wood harvestable here.",       "Moderate","","","", "Chop Wood", "Harvest", "Scout");

        SZ("Bespin",     "Tibanna Gas Clouds",       "dock",       "Pressurised gas layers with tibanna deposits.", "Moderate", "Gas Harvesting Laser", "Gas Hold", "", "Harvest Gas", "Scan Gas Pockets");
        SZ("Bespin",     "Gas Processing Plant",     "market",     "Cloud City's tibanna refinery.",                "Low", "", "", "", "Refine Materials", "Visit Merchant");
        SZ("Bespin",     "Cloud City Casino",        "market",     "A luxurious casino district.",                  "Low", "", "", "", "Visit Merchant", "Gamble", "Talk to Contact");

        SZ("Mandalore",  "Sundari Ruins",            "ruins",      "Bombed ruins of the pacifist capital.",         "High", "", "", "", "Scavenge", "Mine Resources", "Scan Veins");
        SZ("Mandalore",  "Mandalorian Forge",        "market",     "Sacred forge for shaping beskar.",              "Moderate","","","Mandalorians", "Refine Materials", "Craft Armor");

        SZ("Mustafar",   "Lava Mines",               "industrial platform","Active lava tubes with cortosis deposits.", "Deadly","","","",  "Mine Resources", "Scan Veins");
        SZ("Mustafar",   "Obsidian Caldera",         "volcanic flats","A vast caldera of cooling obsidian.",          "High",   "","","",  "Mine Resources", "Scan Veins", "Harvest");

        SZ("Hoth",       "Wampa Ice Caves",          "caves",      "Frozen caverns home to wampa beasts.",           "Deadly","","","", "Mine Resources", "Scan Veins", "Hunt");
        SZ("Hoth",       "Perma-Frost Glacier",      "wilderness", "Miles of solid glacier with mineral deposits.",  "High",  "","","", "Mine Resources", "Scan Veins");

        SZ("Kessel",     "Spice Mines of Kessel",    "mine shaft", "The treacherous spice mines.",                  "Deadly","","","", "Mine Resources", "Scan Veins");
        SZ("Kessel",     "Kessel Spice Refinery",    "market",     "Processing facility for raw spice.",             "High",  "","","", "Refine Materials");

        SZ("Ilum",       "Kyber Crystal Cave",       "caves",      "The sacred cave where Jedi find their kyber crystals.", "High", "","","", "Mine Resources", "Scan Veins", "Meditate");

        SZ("Coruscant",  "Coruscant Underworld",     "undercity",  "The lawless depths below the city crust.",      "Deadly","","","", "Raid Slums", "Scavenge", "Black Market");
        SZ("Coruscant",  "Senate District",          "government district","The political heart of the galaxy.",    "Moderate","","","", "Talk to Contact", "Gather Intel");

        SZ("Geonosis",   "Geonosis Catacombs",       "caves",      "Deep catacombs riddled with doonium ore.",       "High","","","", "Mine Resources", "Scan Veins");
        SZ("Dagobah",    "Force Nexus Cave",          "swamp",      "A dark side vergence hidden in the bog.",        "Deadly","","","", "Meditate", "Harvest");
        SZ("Endor",      "Ewok Village",              "forest",     "Treehouse village of the Ewok people.",          "Low",  "","","", "Trade", "Gather Intel");
        SZ("Endor",      "Ancient Petrified Forest", "forest",      "Old-growth timber heavy with ancient wood.",     "Moderate","","","","Chop Wood", "Scan Veins", "Harvest");
        SZ("Naboo",      "Underwater City",           "open ocean", "Gungan city beneath the lakes.",                "Moderate","","","","Visit Merchant","Talk to Contact");
        SZ("Naboo",      "Plasma Refinery",           "market",     "Theed's plasma processing plant.",              "Low",  "","","", "Refine Materials");
        SZ("Ryloth",     "Spice Caves",               "caves",      "Ryll spice mines beneath the surface.",          "High","","","", "Mine Resources", "Scan Veins");
        SZ("Ryloth",     "Ryll Refinery",             "market",     "Ryloth's spice refinery district.",              "Moderate","","","", "Refine Materials");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MINING VEHICLES
    // ═══════════════════════════════════════════════════════════════════════════
    private void InitMiningVehicles()
    {
        void MV(string name, string desc, int cargo, int bonus, bool scanner, bool gas, int val, string era)
        {
            MiningVehicles[name] = new MiningVehicleBlueprint
            {
                Name=name, Description=desc, CargoSlotsRequired=cargo, MiningBonus=bonus,
                HasScanner=scanner, HasGasHarvester=gas, BaseValue=val, Era=era
            };
        }

        MV("Digger Crawler",        "A massive treaded crawler used by the Mining Guild.",       6, 3, true,  false, 14000, "Old Republic");
        MV("GX8 Ore Drill",         "Industrial rotary drill for mining hard rock ore.",         3, 2, false, false, 6500,  "All");
        MV("Tibanna Tanker",        "A repulsor-lifted tanker for harvesting gas clouds.",       4, 2, false, true,  9000,  "Original Trilogy");
        MV("Moisture Vaporator",    "A tall vaporator used on desert worlds to extract moisture.",1, 0, false, false, 800,   "All");
        MV("T-47 Mining Speeder",   "A modified airspeeder fitted with ore collection arms.",   2, 1, true,  false, 3500,  "Original Trilogy");
        MV("RockerMelt Drill",      "A stationary thermal drill that melts rock faces.",         2, 2, false, false, 4200,  "All");
        MV("Ore Hauler",            "A flatbed repulsor-truck for hauling raw ore.",             5, 0, false, false, 2800,  "All");
        MV("Mining Guild Scout",    "A compact craft used for prospecting new veins.",           2, 1, true,  false, 5500,  "Original Trilogy");
        MV("Separatist Harvester",  "A Techno Union harvester used during the Clone Wars.",     4, 2, true,  false, 7000,  "Clone Wars");
        MV("Jawas' Sandcrawler",    "A huge mobile base and ore processor.",                    8, 3, true,  false, 18000, "All");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WOOD TYPES
    // ═══════════════════════════════════════════════════════════════════════════
    private void InitWoodTypes()
    {
        void WT(string name, string planet, string desc, int hard, string key, int val)
        {
            WoodTypes[name] = new WoodTypeData { Name=name, Planet=planet, Description=desc, Hardness=hard, RawMaterialKey=key, BaseValue=val };
        }

        WT("Wroshyr Wood",        "Kashyyyk",  "Bark of the colossal wroshyr trees; near-metal hardness.",   9, "wroshyr wood plank",     180);
        WT("Ancient Bark",        "Endor",     "Dense bark from old-growth Endor trees.",                    7, "ancient bark plank",     90);
        WT("Massassi Wood",       "Yavin IV",  "Amber-tinged hardwood from Massassi jungle.",                7, "massassi wood plank",    100);
        WT("Noj Wood",            "Naboo",     "Light-weight wood from Naboo wetland trees.",                4, "noj wood plank",         50);
        WT("Petrified Driftwood", "Dagobah",   "Waterlogged, semi-fossilised driftwood.",                    5, "petrified driftwood plank",60);
        WT("Ironwood",            "Dantooine", "Dense iron-rich wood from Dantooine grassland trees.",       8, "ironwood plank",         120);
        WT("Shadowbark",          "Lothal",    "Dark wood harvested from Lothal's sparse forests.",          5, "shadowbark plank",       70);
        WT("Sith Oak",            "Korriban",  "Twisted, dark-grain wood from Korriban's cursed tombs.",     6, "sith oak plank",         110);
        WT("Lumber Pine",         "Hoth",      "Frozen conifer timber from Hoth's ice-buried forests.",      4, "lumber pine plank",      40);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ARMOR OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    // Delegates to slot-based system (backward-compat wrapper)
    public string EquipArmor(GameCharacter character, string armorName)
        => EquipArmorPiece(character, armorName);

    public string UnequipArmor(GameCharacter character)
    {
        if (character.EquippedArmorPieces.Count == 0 && string.IsNullOrEmpty(character.EquippedArmor))
            return "No armor equipped.";
        // Unequip everything via slot system
        if (character.EquippedArmorPieces.Count > 0)
        {
            UnequipAllArmor(character);
            return "All armor removed.";
        }
        // Legacy fallback (EquippedArmorPieces was empty but EquippedArmor had a value)
        var prev = character.EquippedArmor;
        if (Armors.TryGetValue(prev, out var old))
        {
            character.MaxHp      = Math.Max(10, character.MaxHp - old.HpBonus);
            character.MaxStamina = Math.Max(5, character.MaxStamina - old.StaminaBonus);
            character.Armor      = Math.Max(0, character.Armor - old.ArmorRating);
            character.Hp         = Math.Min(character.Hp, character.MaxHp);
        }
        character.EquippedArmor = "";
        return $"You remove {prev}.";
    }

    public (bool ok, string message) CraftArmor(GameCharacter character, string armorName)
    {
        if (!Armors.TryGetValue(armorName, out var armor))
            return (false, $"Unknown armor: {armorName}.");
        if (!armor.Craftable)
            return (false, $"{armorName} cannot be crafted — it must be found or obtained.");
        if (!ArmorRecipes.TryGetValue(armorName, out var recipe))
            return (false, $"No recipe known for {armorName}.");
        if (recipe.RequiresBlueprint &&
            !character.KnownBlueprints.Any(b => b.Equals(armorName + " blueprint", StringComparison.OrdinalIgnoreCase)))
            return (false, $"You need the '{armorName} blueprint' to craft this.");
        if (recipe.RequiresForge && !HasForgeAccess(character))
            return (false, "This armor requires a forge or industrial furnace.");
        if (character.Credits < recipe.CreditCost)
            return (false, $"Not enough credits. Need {recipe.CreditCost}.");

        foreach (var (item, qty) in recipe.Materials)
        {
            if (CountInventoryItem(character, item) < qty)
                return (false, $"Missing material: {qty}x {item}.");
        }

        // Consume materials
        foreach (var (item, qty) in recipe.Materials)
            ConsumeInventoryItem(character, item, qty);
        character.Credits -= recipe.CreditCost;

        // Check inventory space via SCU system
        var used = GetInventoryUsedMicroScu(character);
        var cap  = GetInventoryCapacityMicroScu(character);
        if (used + GetItemMicroScuSize(armorName) > cap)
            return (false, $"Inventory full ({ScuConversion.FormatMicroScu(used)}/{ScuConversion.FormatMicroScu(cap)}).");

        character.Inventory.Add(armorName);
        character.Experience += 10;
        return (true, $"You craft {armorName} successfully.");
    }

    private bool HasForgeAccess(GameCharacter character)
    {
        if (!Planets.TryGetValue(character.Location, out var planet)) return false;
        return planet.HasIndustrialFurnace ||
               character.Location.Equals("Mandalore", StringComparison.OrdinalIgnoreCase) ||
               GetPlanetFacilitySummary(character.Location).Contains("Forge", StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REFINING OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    public bool CanRefineAt(string rawMaterial, string location)
    {
        if (!RefiningRecipes.TryGetValue(rawMaterial, out var rec)) return false;
        if (string.IsNullOrWhiteSpace(rec.LocationRequired)) return true;
        return rec.LocationRequired.Equals(location, StringComparison.OrdinalIgnoreCase);
    }

    public List<string> GetRefinableAtLocation(GameCharacter character)
    {
        return character.Inventory
            .Where(item => RefiningRecipes.ContainsKey(item) && CanRefineAt(item, character.Location))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public (bool ok, string message) RefineRawMaterial(GameCharacter character, string rawMaterial, int quantity = 1)
    {
        if (!RefiningRecipes.TryGetValue(rawMaterial, out var rec))
            return (false, $"'{rawMaterial}' has no refining recipe.");
        if (!CanRefineAt(rawMaterial, character.Location))
            return (false, $"{rawMaterial} can only be refined at {rec.LocationRequired}.");

        var facilityOk = rec.FacilityRequired switch
        {
            "forge" or "mandalorian forge" => HasForgeAccess(character) || character.Location.Equals("Mandalore", StringComparison.OrdinalIgnoreCase),
            "industrial forge"             => Planets.TryGetValue(character.Location, out var p) && p.HasIndustrialFurnace,
            "gas_processor"                => character.Location.Equals("Bespin", StringComparison.OrdinalIgnoreCase) || character.UnlockedSubZones.Contains("Gas Processing Plant"),
            "spice_mill"                   => character.UnlockedSubZones.Contains("Kessel Spice Refinery") || character.UnlockedSubZones.Contains("Ryll Refinery"),
            "jedi_sanctum"                 => character.UnlockedSubZones.Contains("Kyber Crystal Cave") || character.UnlockedSubZones.Contains("Jedi Temple"),
            "lab"                          => GetPlanetFacilitySummary(character.Location).Contains("Lab", StringComparison.OrdinalIgnoreCase),
            "shipyard"                     => Planets.TryGetValue(character.Location, out var ps) && ps.HasDockyard,
            _ => true
        };
        if (!facilityOk)
            return (false, $"Refining {rawMaterial} requires a {rec.FacilityRequired} at this location.");

        var have = CountInventoryItem(character, rawMaterial);
        var toProcess = Math.Min(quantity, have);
        if (toProcess <= 0)
            return (false, $"You don't have any {rawMaterial} to refine.");
        if (character.Credits < rec.CreditCost * toProcess)
            return (false, $"Need {rec.CreditCost * toProcess} credits to process {toProcess}x {rawMaterial}.");

        ConsumeInventoryItem(character, rawMaterial, toProcess);
        character.Credits -= rec.CreditCost * toProcess;

        var produced = toProcess * rec.Quantity;
        for (int i = 0; i < produced; i++) character.Inventory.Add(rec.RefinedOutput);
        AdvanceWorldTime(rec.TimeCost, character.Location, character);
        character.Experience += produced;
        return (true, $"Refined {toProcess}x {rawMaterial} → {produced}x {rec.RefinedOutput}.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MINING SCAN SYSTEM
    // ═══════════════════════════════════════════════════════════════════════════

    public List<OreVeinNode> ScanMiningZone(GameCharacter character, string zone)
    {
        var planet = character.Location;
        if (!Planets.ContainsKey(planet) && SpaceStations.TryGetValue(planet, out var st))
            planet = st.OrbitingPlanet;

        string[] rawPool;
        if (!PlanetMineralYield.TryGetValue(planet, out rawPool!))
            rawPool = InferMineralYield(planet);

        var sepIdx = Array.IndexOf(rawPool, "|");
        var common = sepIdx > 0 ? rawPool[..sepIdx] : rawPool;
        var rare   = sepIdx > 0 && sepIdx < rawPool.Length - 1 ? rawPool[(sepIdx+1)..] : Array.Empty<string>();

        // Zone multipliers
        var nodeCount = zone.ToLowerInvariant() switch
        {
            "caves" or "mine shaft"           => 6,
            "volcanic flats" or "lava mines"  => 5,
            "wilderness" or "forest" or "ruins"=>4,
            "shadowlands lower level"         => 6,
            "geonosis catacombs"              => 5,
            "kyber crystal cave"              => 4,
            "spice mines of kessel"           => 5,
            _                                 => 3
        };

        var nodes = new List<OreVeinNode>();
        var icons = new[] { "◆", "◈", "⬡", "◉", "◎", "▲", "●" };
        int iconIdx = 0;

        var zoneLow = zone.ToLowerInvariant();
        foreach (var ore in common)
        {
            var nodeId  = $"{zoneLow}:{ore.ToLowerInvariant()}";
            var chance  = random.Next(45, 85);
            nodes.Add(new OreVeinNode
            {
                OreType = ore, RawMaterialKey = ore, NodeId = nodeId,
                ChancePercent = chance, IsRare = false,
                VeinSize = zone.Contains("cave") || zone.Contains("mine") ? random.Next(2,4) : 1,
                Icon = icons[iconIdx++ % icons.Length]
            });
            if (nodes.Count >= nodeCount) break;
        }
        foreach (var ore in rare)
        {
            if (nodes.Count >= nodeCount) break;
            var nodeId  = $"{zoneLow}:{ore.ToLowerInvariant()}:rare";
            var chance  = random.Next(10, 28);
            nodes.Add(new OreVeinNode
            {
                OreType = ore, RawMaterialKey = ore, NodeId = nodeId,
                ChancePercent = chance, IsRare = true,
                VeinSize = 1,
                Icon = "✦"
            });
        }
        return nodes;
    }

    /// <summary>If the item is a raw material with a refining recipe, returns the refined version. Otherwise returns the item unchanged.</summary>
    private string AutoRefineItem(string item)
        => RefiningMap.TryGetValue(item, out var refined) ? refined : item;

    public (bool ok, string message, List<string> obtained) ExecuteMine(GameCharacter character, string zone, List<OreVeinNode> veins)
    {
        const int cooldown = 4;
        if (Clock.Rotation - character.LastMiningRotation < cooldown)
        {
            var wait = cooldown - (Clock.Rotation - character.LastMiningRotation);
            return (false, $"You need to wait for the dust to settle — {wait} more rotation(s).", new());
        }

        // Mining tool bonus
        var tool = GetBestMiningTool(character);
        var yieldBonus  = tool?.YieldBonus  ?? 0;
        var chanceBonus = tool?.ChanceBonus ?? 0;

        var cap = GetInventoryCapacity(character);
        var obtained  = new List<string>();
        var deplNodes = new List<string>();

        foreach (var vein in veins)
        {
            // Per-vein depletion — each ore vein has its own counter
            var nodeId = string.IsNullOrEmpty(vein.NodeId)
                ? $"{zone.ToLowerInvariant()}:{vein.RawMaterialKey.ToLowerInvariant()}"
                : vein.NodeId;
            var (veinDepleted, _) = ConsumeVeinHarvest(character.Location, nodeId, 3, 8);
            if (veinDepleted) { deplNodes.Add(vein.OreType); continue; }

            var effectiveChance = Math.Min(98, vein.ChancePercent + chanceBonus);
            for (int i = 0; i < vein.VeinSize + yieldBonus; i++)
            {
                if (random.Next(100) < effectiveChance)
                {
                    if (character.Inventory.Count + obtained.Count >= cap) break;
                    obtained.Add(vein.RawMaterialKey);
                }
            }
        }

        if (obtained.Count == 0 && deplNodes.Count == veins.Count)
            return (false, $"All selected vein(s) are exhausted — respawn in 8 rotations.", new());
        if (obtained.Count == 0)
            return (false, "You mine the rock face but find nothing useful this time.", new());

        character.Inventory.AddRange(obtained);
        character.LastMiningRotation = Clock.Rotation;
        character.Experience += 4 + obtained.Count;
        AdvanceWorldTime(2, character.Location, character);

        var discoveryMsgs = OnItemDiscovered(character, obtained);
        var deplNote = deplNodes.Count > 0 ? $"  [{string.Join(", ", deplNodes)} depleted]" : "";
        var toolNote = tool is not null ? $" (⛏ {tool.Name})" : " (no mining tool — a pick helps)";
        var discNote = discoveryMsgs.Count > 0 ? "\r\n" + string.Join("\r\n", discoveryMsgs) : "";
        return (true, $"Extracted: {string.Join(", ", obtained)}.{toolNote}{deplNote}{discNote}", obtained);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HARVESTING
    // ═══════════════════════════════════════════════════════════════════════════

    private static readonly Dictionary<string, string[]> ZoneHarvestables = new(StringComparer.OrdinalIgnoreCase)
    {
        ["forest"]          = new[] { "medicinal herb", "forest mushroom", "symbiote moss", "wroshyr resin", "tree sap", "jungle root", "nerf cut (raw)" },
        ["swamp"]           = new[] { "bog root", "swamp fungus", "dark clay", "mire mineral", "swamp reed", "jungle root" },
        ["wilderness"]      = new[] { "prairie grass bundle", "topsoil mineral", "wild berry cluster", "loam mineral", "moisture fruit", "bantha jerky (raw)" },
        ["jungle"]          = new[] { "jungle vine", "tropical fruit", "bark resin", "insect chitin", "jungle root", "nerf cut (raw)" },
        ["open ocean"]      = new[] { "sea mineral", "brine crystal", "aquite crystal", "nautolan coral", "sea glass", "algae pack" },
        ["caves"]           = new[] { "cave moss", "bat guano", "mineral deposit", "stalactite shard" },
        ["volcanic flats"]  = new[] { "volcanic ash", "obsidian shard", "magma stone", "cooling unit" },
        ["ruins"]           = new[] { "ancient relic fragment", "ruin dust", "fossilised mineral", "carved stone" },
        ["grasslands"]      = new[] { "prairie grass bundle", "wild grain", "sand mineral", "loam mineral", "moisture fruit" },
        ["tundra"]          = new[] { "permafrost crystal", "tundra rock", "ice mineral", "frozen herb" },
        ["desert"]          = new[] { "silica sand", "mesa rock", "heat crystal", "dried root", "moisture fruit" },
        ["market"]          = new[] { "misc goods", "datapad", "power cell" },
        ["dune sea"]        = new[] { "silica sand", "mesa rock", "moisture fruit", "dried root" },
        ["wastes"]          = new[] { "topsoil mineral", "wild berry cluster", "moisture fruit", "bantha jerky (raw)" },
    };

    public (bool ok, string message, List<string> obtained) HarvestZone(GameCharacter character, string zone, string targetType = "")
    {
        // Node depletion check (5 uses, 5-rotation respawn)
        var (nodeDepleted, nodeMsg) = ConsumeNodeHarvest(character.Location, zone, "harvest", 5, 5);
        if (nodeDepleted) return (false, nodeMsg, new());

        const int cooldown = 3;
        if (Clock.Rotation - character.LastHarvestRotation < cooldown)
        {
            var wait = cooldown - (Clock.Rotation - character.LastHarvestRotation);
            return (false, $"Area is picked over — wait {wait} more rotation(s).", new());
        }

        var cap = GetInventoryCapacity(character);
        if (character.Inventory.Count >= cap)
            return (false, "Inventory full.", new());

        var zLow = zone.ToLowerInvariant();
        string[] pool;
        if (!ZoneHarvestables.TryGetValue(zLow, out pool!))
        {
            // Fallback from planet biome
            pool = InferMineralYield(character.Location).Where(x => x != "|").ToArray();
            if (pool.Length == 0) pool = new[] { "misc organic", "root bundle" };
        }

        // Filter by target type if specified
        if (!string.IsNullOrWhiteSpace(targetType))
            pool = pool.Where(p => p.Contains(targetType, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (pool.Length == 0) pool = ZoneHarvestables.TryGetValue(zLow, out var def) ? def : new[] { "misc organic" };

        // Foraging armor bonus adds extra rolls
        var foragingBonus = GetForagingBonus(character);
        var rolls = random.Next(1, 4) + foragingBonus;
        var obtained = new List<string>();
        for (int i = 0; i < rolls && character.Inventory.Count + obtained.Count < cap; i++)
            obtained.Add(AutoRefineItem(pool[random.Next(pool.Length)]));

        character.Inventory.AddRange(obtained);
        character.LastHarvestRotation = Clock.Rotation;
        character.Experience += 3 + obtained.Count;
        AdvanceWorldTime(1, character.Location, character);

        var discoveryMsgs = OnItemDiscovered(character, obtained);
        var (_, remaining, _) = GetNodeStatus(character.Location, zone, "harvest");
        var nodeNote = remaining > 0 ? $" [{remaining} gather(s) left]" : " [area now depleted — respawns in 5 rotations]";
        var armorNote = foragingBonus > 0 ? $" (✿ +{foragingBonus} rolls from armor)" : "";
        var discNote  = discoveryMsgs.Count > 0 ? "\r\n" + string.Join("\r\n", discoveryMsgs) : "";
        return (true, $"Harvested: {string.Join(", ", obtained)}.{armorNote}{nodeNote}{discNote}", obtained);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SURVIVAL — EATING, COOKING, HUNGER & ENERGY
    // ═══════════════════════════════════════════════════════════════════════════

    public (bool ok, string message) EatFood(GameCharacter character, string foodItemName)
    {
        if (!FoodItems.TryGetValue(foodItemName, out var food))
            return (false, $"'{foodItemName}' is not a recognised food item.");

        if (!character.Inventory.Any(x => x.Equals(foodItemName, StringComparison.OrdinalIgnoreCase)))
            return (false, $"You don't have '{foodItemName}' in your inventory.");

        if (food.RequiresCooking)
            return (false, $"{foodItemName} must be cooked before eating. Use the cook action first.");

        character.Inventory.Remove(
            character.Inventory.First(x => x.Equals(foodItemName, StringComparison.OrdinalIgnoreCase)));

        var hungerBefore = character.Hunger;
        var energyBefore = character.Energy;
        character.Hunger = Math.Min(100, character.Hunger + food.HungerValue);
        character.Energy = Math.Min(100, character.Energy + food.EnergyValue);
        if (food.HpBonus > 0)
            character.Hp = Math.Min(character.MaxHp, character.Hp + food.HpBonus);

        character.LastMealRotation = Clock.Rotation;

        // Remove hunger/starving status effects now that the character has eaten
        character.StatusEffects.Remove("Hungry");
        character.StatusEffects.Remove("Starving");

        var hungerGained = character.Hunger - hungerBefore;
        var energyGained = character.Energy - energyBefore;
        var msg = $"You eat {foodItemName}. Hunger +{hungerGained} → {character.Hunger}/100. Energy +{energyGained} → {character.Energy}/100.";
        if (food.HpBonus > 0) msg += $" HP +{food.HpBonus} → {character.Hp}/{character.MaxHp}.";
        return (true, msg);
    }

    public (bool ok, string message) CookFood(GameCharacter character, string recipeName)
    {
        if (!CookingRecipes.TryGetValue(recipeName, out var recipe))
            return (false, $"No cooking recipe known for '{recipeName}'.");

        foreach (var input in recipe.Inputs)
        {
            var count = character.Inventory.Count(x => x.Equals(input.Item, StringComparison.OrdinalIgnoreCase));
            if (count < input.Quantity)
                return (false, $"Need {input.Quantity}× {input.Item} to cook {recipeName} (you have {count}).");
        }

        // Consume ingredients
        foreach (var input in recipe.Inputs)
        {
            int removed = 0;
            for (int i = character.Inventory.Count - 1; i >= 0 && removed < input.Quantity; i--)
            {
                if (character.Inventory[i].Equals(input.Item, StringComparison.OrdinalIgnoreCase))
                {
                    character.Inventory.RemoveAt(i);
                    removed++;
                }
            }
        }

        character.Inventory.Add(recipe.Output);
        AdvanceWorldTime(recipe.TimeHours, character.Location, character);
        character.Experience += 3;
        return (true, $"You cook {recipe.Output}. ({recipe.Description}) Time spent: {recipe.TimeHours}h.");
    }

    public void ApplySurvivalTick(GameCharacter character, int hours)
    {
        // Droids don't need food
        if (character.Species.Equals("Droid", StringComparison.OrdinalIgnoreCase)) return;

        // Hunger decreases: ~1 per 4 hours (8 per day)
        var hungerLoss = Math.Max(0, hours / 4);
        if (hungerLoss > 0)
            character.Hunger = Math.Max(0, character.Hunger - hungerLoss);

        // Energy decreases: ~1 per 6 hours when well-fed, faster when hungry
        var baseEnergyLoss = Math.Max(0, hours / 6);
        if (baseEnergyLoss > 0)
        {
            var actualLoss = character.Hunger < 30 ? baseEnergyLoss * 2 : baseEnergyLoss;
            // Slight energy recovery if well-fed and resting
            if (character.Hunger >= 60 && baseEnergyLoss > 0)
                actualLoss = Math.Max(0, actualLoss - 1);
            character.Energy = Math.Clamp(character.Energy - actualLoss, 0, 100);
        }

        // Starvation causes HP damage
        if (character.Hunger <= 5)
        {
            var hpLoss = Math.Max(1, hours / 12);
            character.Hp = Math.Max(1, character.Hp - hpLoss);
            if (!character.StatusEffects.Contains("Starving"))
                character.StatusEffects.Add("Starving");
        }
        else
        {
            character.StatusEffects.Remove("Starving");
        }

        // Update status effects for hunger
        if (character.Hunger <= 20 && !character.StatusEffects.Contains("Hungry"))
            character.StatusEffects.Add("Hungry");
        else if (character.Hunger > 20)
            character.StatusEffects.Remove("Hungry");

        // Update status effects for energy/fatigue
        if (character.Energy <= 10 && !character.StatusEffects.Contains("Exhausted"))
            character.StatusEffects.Add("Exhausted");
        else if (character.Energy > 10)
            character.StatusEffects.Remove("Exhausted");

        if (character.Energy <= 25 && character.Energy > 10 && !character.StatusEffects.Contains("Fatigued"))
            character.StatusEffects.Add("Fatigued");
        else if (character.Energy > 25)
            character.StatusEffects.Remove("Fatigued");
    }

    public string GetNutritionState(GameCharacter character)
    {
        if (character.Species.Equals("Droid", StringComparison.OrdinalIgnoreCase)) return "Powered";
        return character.Hunger switch
        {
            >= 80 => "Well Fed",
            >= 50 => "Satisfied",
            >= 30 => "Hungry",
            >= 10 => "Starving",
            _     => "Dying of Hunger"
        };
    }

    public string GetEnergyState(GameCharacter character)
    {
        if (character.Species.Equals("Droid", StringComparison.OrdinalIgnoreCase)) return "Charged";
        return character.Energy switch
        {
            >= 75 => "Energized",
            >= 45 => "Normal",
            >= 25 => "Tired",
            >= 10 => "Fatigued",
            _     => "Exhausted"
        };
    }

    /// <summary>
    /// Returns true if fatigue from low energy blocks this combat ability.
    /// </summary>
    public bool IsBlockedByFatigue(GameCharacter character, string ability)
    {
        if (character.Species.Equals("Droid", StringComparison.OrdinalIgnoreCase)) return false;

        // Exhausted: only basic attack allowed
        if (character.Energy <= 10)
            return !ability.Equals("attack", StringComparison.OrdinalIgnoreCase)
                && !ability.Equals("flee", StringComparison.OrdinalIgnoreCase)
                && !ability.Equals("use item", StringComparison.OrdinalIgnoreCase);

        // Fatigued: advanced abilities locked
        if (character.Energy <= 25)
        {
            var basicAllowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "attack", "guard", "flee", "use item" };
            return !basicAllowed.Contains(ability);
        }

        // Tired: force abilities and very expensive actions locked
        if (character.Energy <= 40)
        {
            // Only Force abilities and special skill-based attacks are blocked
            return ability.StartsWith("force", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public IReadOnlyList<FoodItem> GetFoodInInventory(GameCharacter character)
        => character.Inventory
            .Where(x => FoodItems.ContainsKey(x))
            .Select(x => FoodItems[x])
            .ToList();

    private void InitFoodData()
    {
        // ── Buyable prepared foods ──────────────────────────────────────────
        FoodItems["ration bar"]       = new FoodItem { Name = "ration bar",       Category = "prepared", HungerValue = 20, EnergyValue = 10, BuyPrice = 5,  Description = "A compact military ration. Not tasty, but filling enough." };
        FoodItems["ration pack"]      = new FoodItem { Name = "ration pack",      Category = "prepared", HungerValue = 40, EnergyValue = 20, BuyPrice = 12, Description = "Standard field ration pack — feeds a soldier for a day." };
        FoodItems["cantina meal"]     = new FoodItem { Name = "cantina meal",     Category = "luxury",   HungerValue = 65, EnergyValue = 40, BuyPrice = 25, Description = "A hot meal from the cantina. Surprisingly good." };
        FoodItems["medicated ration"] = new FoodItem { Name = "medicated ration", Category = "prepared", HungerValue = 30, EnergyValue = 20, HpBonus = 10, BuyPrice = 18, Description = "Ration supplemented with stim compounds. Heals minor wounds." };

        // ── Raw / forageable foods ──────────────────────────────────────────
        FoodItems["moisture fruit"]       = new FoodItem { Name = "moisture fruit",       Category = "raw",  HungerValue = 15, EnergyValue = 8,  RequiresCooking = false, Description = "Desert fruit gathered near moisture farms. Sweet and hydrating." };
        FoodItems["jungle root"]          = new FoodItem { Name = "jungle root",          Category = "raw",  HungerValue = 12, EnergyValue = 5,  RequiresCooking = true,  Description = "A starchy root from deep jungle. Must be cooked to be safe." };
        FoodItems["bantha jerky (raw)"]   = new FoodItem { Name = "bantha jerky (raw)",   Category = "raw",  HungerValue = 8,  EnergyValue = 5,  RequiresCooking = true,  Description = "Raw bantha meat — cook it before eating." };
        FoodItems["algae pack"]           = new FoodItem { Name = "algae pack",           Category = "raw",  HungerValue = 10, EnergyValue = 6,  RequiresCooking = false, Description = "Ocean algae compressed into edible blocks. Nutritious if unpleasant." };
        FoodItems["nerf cut (raw)"]       = new FoodItem { Name = "nerf cut (raw)",       Category = "raw",  HungerValue = 10, EnergyValue = 8,  RequiresCooking = true,  Description = "Raw nerf meat. Best cooked." };

        // ── Cooked foods ────────────────────────────────────────────────────
        FoodItems["ration stew"]   = new FoodItem { Name = "ration stew",   Category = "cooked", HungerValue = 40, EnergyValue = 22, Description = "Watery stew from field rations. Warm and comforting." };
        FoodItems["bantha stew"]   = new FoodItem { Name = "bantha stew",   Category = "cooked", HungerValue = 60, EnergyValue = 35, HpBonus = 5, Description = "Rich stew from bantha meat. Hearty and restorative." };
        FoodItems["spiced jerky"]  = new FoodItem { Name = "spiced jerky",  Category = "cooked", HungerValue = 35, EnergyValue = 18, Description = "Dried and spiced meat. Keeps well in the field." };
        FoodItems["roasted tuber"] = new FoodItem { Name = "roasted tuber", Category = "cooked", HungerValue = 38, EnergyValue = 20, Description = "Fire-roasted jungle root. Rich and earthy." };
        FoodItems["grilled nerf"]  = new FoodItem { Name = "grilled nerf",  Category = "cooked", HungerValue = 55, EnergyValue = 30, HpBonus = 3, Description = "Grilled nerf steak. A classic Outer Rim meal." };

        // ── Cooking recipes ─────────────────────────────────────────────────
        CookingRecipes["ration stew"] = new CookingRecipe
        {
            Output = "ration stew", TimeHours = 1,
            Description = "Heat moisture fruit with water into a simple filling stew.",
            Inputs = new List<RecipeComponent> { new() { Item = "moisture fruit", Quantity = 2 } }
        };
        CookingRecipes["bantha stew"] = new CookingRecipe
        {
            Output = "bantha stew", TimeHours = 2,
            Description = "Simmer bantha meat with spices for a restorative stew.",
            Inputs = new List<RecipeComponent>
            {
                new() { Item = "bantha jerky (raw)", Quantity = 2 },
                new() { Item = "moisture fruit",     Quantity = 1 }
            }
        };
        CookingRecipes["spiced jerky"] = new CookingRecipe
        {
            Output = "spiced jerky", TimeHours = 1,
            Description = "Dry and cure bantha meat with spices.",
            Inputs = new List<RecipeComponent> { new() { Item = "bantha jerky (raw)", Quantity = 1 } }
        };
        CookingRecipes["roasted tuber"] = new CookingRecipe
        {
            Output = "roasted tuber", TimeHours = 1,
            Description = "Fire-roast jungle roots until soft and safe to eat.",
            Inputs = new List<RecipeComponent> { new() { Item = "jungle root", Quantity = 2 } }
        };
        CookingRecipes["grilled nerf"] = new CookingRecipe
        {
            Output = "grilled nerf", TimeHours = 1,
            Description = "Grill nerf cuts over an open flame.",
            Inputs = new List<RecipeComponent> { new() { Item = "nerf cut (raw)", Quantity = 1 } }
        };
    }

    private void InitHarvestingTools()
    {
        // ── Mining picks ────────────────────────────────────────────────────────
        HarvestingTools["mining pick"]        = new HarvestingTool { Name = "mining pick",        ToolType = "mining",      YieldBonus = 1, ChanceBonus = 5,  Tier = 1, BuyPrice = 30,  Description = "A sturdy metal pick for ore mining. +1 yield roll, +5% vein hit chance." };
        HarvestingTools["vibro-pick"]         = new HarvestingTool { Name = "vibro-pick",         ToolType = "mining",      YieldBonus = 2, ChanceBonus = 10, Tier = 2, BuyPrice = 80,  Description = "Vibro-powered pick that shatters rock efficiently. +2 yield rolls, +10% chance." };
        HarvestingTools["plasma drill"]       = new HarvestingTool { Name = "plasma drill",       ToolType = "mining",      YieldBonus = 3, ChanceBonus = 15, Tier = 3, BuyPrice = 200, Description = "Handheld plasma-torch drill for deep veins. +3 yield rolls, +15% chance." };
        HarvestingTools["cortosis excavator"] = new HarvestingTool { Name = "cortosis excavator", ToolType = "mining",      YieldBonus = 4, ChanceBonus = 20, RareMaterialBonus = true, Tier = 4, BuyPrice = 600, Description = "Military-grade cortosis-tipped excavator. +4 yield rolls, +20% chance, unlocks rare ore finds." };

        // ── Woodcutting axes ────────────────────────────────────────────────────
        HarvestingTools["woodcutter's axe"] = new HarvestingTool { Name = "woodcutter's axe", ToolType = "woodcutting", YieldBonus = 1, Tier = 1, BuyPrice = 25,  Description = "A basic steel woodcutting axe. +1 yield roll." };
        HarvestingTools["vibro-axe"]        = new HarvestingTool { Name = "vibro-axe",        ToolType = "woodcutting", YieldBonus = 2, Tier = 2, BuyPrice = 75,  Description = "Vibro-powered axe that cleaves through hardwood. +2 yield rolls." };
        HarvestingTools["arc-cutter"]       = new HarvestingTool { Name = "arc-cutter",       ToolType = "woodcutting", YieldBonus = 3, Tier = 3, BuyPrice = 180, Description = "Arc-powered rotary saw for felling large trees. +3 yield rolls." };
        HarvestingTools["plasma chainsaw"]  = new HarvestingTool { Name = "plasma chainsaw",  ToolType = "woodcutting", YieldBonus = 4, Tier = 4, BuyPrice = 500, Description = "Plasma-edged chainsaw for industrial logging. +4 yield rolls." };
    }

    // ── Node depletion helpers ─────────────────────────────────────────────────

    private string GetNodeKey(string planet, string zone, string nodeType)
        => $"{planet}:{zone}:{nodeType}".ToLowerInvariant();

    private ResourceNodeState GetOrCreateNodeState(string planet, string zone, string nodeType, int maxHarvests, int respawnRotations)
    {
        var key = GetNodeKey(planet, zone, nodeType);
        if (!resourceNodeStates.TryGetValue(key, out var state))
        {
            state = new ResourceNodeState { Key = key, MaxHarvests = maxHarvests, RespawnRotations = respawnRotations };
            resourceNodeStates[key] = state;
        }
        return state;
    }

    private (bool depleted, string message) ConsumeNodeHarvest(string planet, string zone, string nodeType, int maxHarvests, int respawnRotations)
    {
        var state = GetOrCreateNodeState(planet, zone, nodeType, maxHarvests, respawnRotations);
        if (state.IsDepleted(Clock.Rotation))
        {
            var wait = state.RotationsUntilRespawn(Clock.Rotation);
            return (true, $"This area is exhausted — resources here are regenerating. Return in {wait} more rotation(s).");
        }
        state.TimesHarvested++;
        if (state.TimesHarvested >= state.MaxHarvests)
        {
            state.DepletedAtRotation = Clock.Rotation;
            state.TimesHarvested = 0;
        }
        return (false, "");
    }

    /// <summary>Returns (isDepleted, remainingUses, rotationsUntilRespawn) for a zone-level node.</summary>
    public (bool isDepleted, int remaining, int respawnIn) GetNodeStatus(string planet, string zone, string nodeType)
    {
        var (maxH, respawnR) = nodeType switch
        {
            "mine"  => (3, 8),
            "wood"  => (4, 7),
            _       => (5, 5)   // "harvest"
        };
        var key = GetNodeKey(planet, zone, nodeType);
        if (!resourceNodeStates.TryGetValue(key, out var state))
            return (false, maxH, 0);
        var depleted = state.IsDepleted(Clock.Rotation);
        var remaining = depleted ? 0 : Math.Max(0, state.MaxHarvests - state.TimesHarvested);
        return (depleted, remaining, depleted ? state.RotationsUntilRespawn(Clock.Rotation) : 0);
    }

    // ── Per-vein (per-node) depletion ─────────────────────────────────────────

    private (bool depleted, string message) ConsumeVeinHarvest(string planet, string nodeId, int maxHarvests, int respawnRotations)
    {
        var key = $"{planet}:{nodeId}:vein".ToLowerInvariant();
        if (!resourceNodeStates.TryGetValue(key, out var state))
        {
            state = new ResourceNodeState { Key = key, MaxHarvests = maxHarvests, RespawnRotations = respawnRotations };
            resourceNodeStates[key] = state;
        }
        if (state.IsDepleted(Clock.Rotation))
        {
            var wait = state.RotationsUntilRespawn(Clock.Rotation);
            return (true, $"Vein exhausted \u2014 respawns in {wait} rotation(s).");
        }
        state.TimesHarvested++;
        if (state.TimesHarvested >= state.MaxHarvests)
        {
            state.DepletedAtRotation = Clock.Rotation;
            state.TimesHarvested = 0;
        }
        return (false, "");
    }

    /// <summary>Returns per-vein depletion status by nodeId.</summary>
    public (bool isDepleted, int remaining, int respawnIn) GetVeinStatus(string planet, string nodeId)
    {
        const int maxH = 3;
        var key = $"{planet}:{nodeId}:vein".ToLowerInvariant();
        if (!resourceNodeStates.TryGetValue(key, out var state))
            return (false, maxH, 0);
        var depleted = state.IsDepleted(Clock.Rotation);
        var remaining = depleted ? 0 : Math.Max(0, state.MaxHarvests - state.TimesHarvested);
        return (depleted, remaining, depleted ? state.RotationsUntilRespawn(Clock.Rotation) : 0);
    }

    // ── Tool helpers ────────────────────────────────────────────────────────────

    public HarvestingTool? GetBestMiningTool(GameCharacter character)
    {
        // Prefer the explicitly equipped tool slot first
        if (!string.IsNullOrEmpty(character.EquippedTool) &&
            HarvestingTools.TryGetValue(character.EquippedTool, out var et) && et.ToolType == "mining")
            return et;
        return character.Inventory
            .Where(i => HarvestingTools.TryGetValue(i, out var t) && t.ToolType == "mining")
            .Select(i => HarvestingTools[i])
            .OrderByDescending(t => t.Tier)
            .FirstOrDefault();
    }

    public HarvestingTool? GetBestWoodcuttingTool(GameCharacter character)
    {
        if (!string.IsNullOrEmpty(character.EquippedTool) &&
            HarvestingTools.TryGetValue(character.EquippedTool, out var et) && et.ToolType == "woodcutting")
            return et;
        return character.Inventory
            .Where(i => HarvestingTools.TryGetValue(i, out var t) && t.ToolType == "woodcutting")
            .Select(i => HarvestingTools[i])
            .OrderByDescending(t => t.Tier)
            .FirstOrDefault();
    }

    /// <summary>Sums ForagingBonus from all equipped armor pieces.</summary>
    public int GetForagingBonus(GameCharacter character)
    {
        var total = 0;
        foreach (var slot in character.EquippedArmorPieces.Values)
        {
            if (Armors.TryGetValue(slot, out var armor))
                total += armor.ForagingBonus;
        }
        if (!string.IsNullOrWhiteSpace(character.EquippedArmor) && Armors.TryGetValue(character.EquippedArmor, out var full))
            total += full.ForagingBonus;
        return total;
    }

    /// <summary>Returns a summary string of active gathering tools for display.</summary>
    public string GetGatheringToolStatus(GameCharacter character)
    {
        var parts = new List<string>();
        var mineTool = GetBestMiningTool(character);
        var woodTool = GetBestWoodcuttingTool(character);
        var foragingBonus = GetForagingBonus(character);

        parts.Add(mineTool is not null ? $"⛏ {mineTool.Name} (+{mineTool.YieldBonus} yield, +{mineTool.ChanceBonus}% chance)" : "⛏ No mining tool");
        parts.Add(woodTool is not null ? $"🪓 {woodTool.Name} (+{woodTool.YieldBonus} yield)" : "🪓 No woodcutting axe");
        parts.Add(foragingBonus > 0 ? $"✿ Foraging bonus +{foragingBonus} (from armor)" : "✿ No foraging armor");

        return string.Join("  |  ", parts);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WOODCUTTING
    // ═══════════════════════════════════════════════════════════════════════════

    public List<WoodTypeData> GetPlanetWoodTypes(string planet)
        => WoodTypes.Values.Where(w => w.Planet.Equals(planet, StringComparison.OrdinalIgnoreCase)).ToList();

    public (bool ok, string message, List<string> obtained) CutWood(GameCharacter character, string zone, string woodTypeName = "")
    {
        // Node depletion check (4 uses, 7-rotation respawn)
        var (nodeDepleted, nodeMsg) = ConsumeNodeHarvest(character.Location, zone, "wood", 4, 7);
        if (nodeDepleted) return (false, nodeMsg, new());

        const int cooldown = 3;
        if (Clock.Rotation - character.LastWoodcutRotation < cooldown)
        {
            var wait = cooldown - (Clock.Rotation - character.LastWoodcutRotation);
            return (false, $"You need to find more trees — wait {wait} more rotation(s).", new());
        }

        var validWood = GetPlanetWoodTypes(character.Location);
        if (validWood.Count == 0)
            return (false, $"No harvestable wood found on {character.Location}.", new());

        // Zone check — woodcutting only in appropriate zones
        var woodZones = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "forest", "jungle", "wilderness", "shadowlands lower level", "wroshyr tree canopy", "ancient petrified forest" };
        if (!woodZones.Contains(zone))
            return (false, "Woodcutting requires a forest or jungle zone.", new());

        // Require an axe (HarvestingTools woodcutting entries) or vibroblade
        bool hasAxe = character.Inventory.Any(x => HarvestingTools.TryGetValue(x, out var ht) && ht.ToolType == "woodcutting") ||
                      character.Inventory.Any(x => x.Contains("axe", StringComparison.OrdinalIgnoreCase)) ||
                      character.Inventory.Any(x => x.Contains("vibro", StringComparison.OrdinalIgnoreCase));
        if (!hasAxe)
            return (false, "You need an axe or vibroblade to cut wood.", new());

        WoodTypeData? wood;
        if (!string.IsNullOrWhiteSpace(woodTypeName))
            wood = validWood.FirstOrDefault(w => w.Name.Equals(woodTypeName, StringComparison.OrdinalIgnoreCase));
        else
            wood = validWood[random.Next(validWood.Count)];
        if (wood == null) return (false, "That wood type is not available here.", new());

        var tool = GetBestWoodcuttingTool(character);
        var yieldBonus = tool?.YieldBonus ?? 0;

        var cap = GetInventoryCapacity(character);
        var rolls = random.Next(1, 4) + yieldBonus;
        var obtained = new List<string>();
        for (int i = 0; i < rolls && character.Inventory.Count + obtained.Count < cap; i++)
            obtained.Add(wood.RawMaterialKey);

        character.Inventory.AddRange(obtained);
        character.LastWoodcutRotation = Clock.Rotation;
        character.Experience += 4 + obtained.Count;
        AdvanceWorldTime(1, character.Location, character);

        var (_, remaining, _) = GetNodeStatus(character.Location, zone, "wood");
        var nodeNote = remaining > 0 ? $" [{remaining} cut(s) left]" : " [grove now felled — respawns in 7 rotations]";
        var toolNote = tool is not null ? $" (🪓 {tool.Name})" : "";
        return (true, $"You cut {obtained.Count}x {wood.RawMaterialKey} from {wood.Name} trees.{toolNote}{nodeNote}", obtained);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GAS HARVESTING (Bespin / gas giants)
    // ═══════════════════════════════════════════════════════════════════════════

    public (bool ok, string message, List<string> obtained) HarvestGas(GameCharacter character)
    {
        if (character.Ship == null) return (false, "You need a ship to harvest gas.", new());
        if (!character.Ship.HasGasHarvester) return (false, "Your ship needs a Gas Harvesting Laser upgrade.", new());
        if (character.Ship.GasHoldCount < 1) return (false, "Your ship needs at least one Gas Hold module.", new());
        if (!character.UnlockedSubZones.Contains("Tibanna Gas Clouds"))
            return (false, "Travel to the Tibanna Gas Clouds sub-zone first.", new());

        var maxGas = character.Ship.GasHoldCount * 8;
        var current = character.Ship.CargoItems.Count(x => x.Contains("tibanna", StringComparison.OrdinalIgnoreCase) || x.Contains("gas canister", StringComparison.OrdinalIgnoreCase));
        var space = Math.Max(0, maxGas - current);
        if (space <= 0) return (false, "Gas Holds are full. Refine or sell before harvesting more.", new());

        var rolls = Math.Min(space, random.Next(2, 5));
        var obtained = new List<string>();
        for (int i = 0; i < rolls; i++) obtained.Add("tibanna gas canister");
        character.Ship.CargoItems.AddRange(obtained);
        AdvanceWorldTime(2, character.Location, character);
        character.Experience += 5 + rolls;
        return (true, $"Harvested {rolls}x tibanna gas canister into ship cargo.", obtained);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SUB-ZONE TRAVEL
    // ═══════════════════════════════════════════════════════════════════════════

    public List<SubZoneDestination> GetSubZoneDestinations(string planet, string zone = "")
    {
        if (!SubZones.TryGetValue(planet, out var list)) return new();
        if (string.IsNullOrWhiteSpace(zone)) return list;
        return list.Where(s => string.IsNullOrWhiteSpace(s.ParentZone) ||
                               s.ParentZone.Equals(zone, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public (bool ok, string message) TravelToSubZone(GameCharacter character, string subZoneName)
    {
        var dest = SubZones.Values.SelectMany(l => l)
            .FirstOrDefault(s => s.Name.Equals(subZoneName, StringComparison.OrdinalIgnoreCase));
        if (dest == null) return (false, $"Unknown destination: {subZoneName}.");

        if (!dest.Planet.Equals(character.Location, StringComparison.OrdinalIgnoreCase))
            return (false, $"You must be on {dest.Planet} to access {subZoneName}.");

        if (!string.IsNullOrWhiteSpace(dest.ShipUpgradeRequired))
        {
            if (character.Ship == null) return (false, $"You need a ship with {dest.ShipUpgradeRequired} to reach {subZoneName}.");
            bool hasUpg = dest.ShipUpgradeRequired.Equals("Gas Harvesting Laser", StringComparison.OrdinalIgnoreCase)
                ? character.Ship.HasGasHarvester
                : character.Ship.Upgrades.Any(u => u.Equals(dest.ShipUpgradeRequired, StringComparison.OrdinalIgnoreCase));
            if (!hasUpg) return (false, $"Your ship needs the '{dest.ShipUpgradeRequired}' upgrade to reach {subZoneName}.");
        }

        if (!string.IsNullOrWhiteSpace(dest.ShipModuleRequired))
        {
            if (character.Ship == null || character.Ship.GasHoldCount < 1)
                return (false, $"Your ship needs at least one '{dest.ShipModuleRequired}' module.");
        }

        if (!string.IsNullOrWhiteSpace(dest.FactionRequired) &&
            !character.Faction.Equals(dest.FactionRequired, StringComparison.OrdinalIgnoreCase) &&
            !(FactionStandings.TryGetValue(dest.FactionRequired, out var fs) && fs >= 10))
            return (false, $"Access to {subZoneName} requires {dest.FactionRequired} standing.");

        if (!character.UnlockedSubZones.Contains(subZoneName, StringComparer.OrdinalIgnoreCase))
            character.UnlockedSubZones.Add(subZoneName);

        AdvanceWorldTime(1, character.Location, character);
        return (true, $"You arrive at {subZoneName}. {dest.Description}");
    }

    public List<string> GetUnlockedSubZoneActions(GameCharacter character)
    {
        var actions = new List<string>();
        foreach (var szName in character.UnlockedSubZones)
        {
            var dest = SubZones.Values.SelectMany(l => l)
                .FirstOrDefault(s => s.Name.Equals(szName, StringComparison.OrdinalIgnoreCase));
            if (dest != null && dest.Planet.Equals(character.Location, StringComparison.OrdinalIgnoreCase))
                actions.AddRange(dest.Actions);
        }
        return actions.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // QUEST ENCOUNTERS
    // ═══════════════════════════════════════════════════════════════════════════

    public QuestEncounterEvent BuildQuestEncounterHint(Quest quest)
    {
        var spawnsCombat = quest.ObjectiveType.Equals("attack", StringComparison.OrdinalIgnoreCase) ||
                           quest.ObjectiveType.Equals("eliminate", StringComparison.OrdinalIgnoreCase);

        var hints = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["fetch"]     = new[] { $"The item should be somewhere in the {quest.ObjectiveZone} district of {quest.TargetPlanet}.",
                                    $"I heard a local contact spotted it near {quest.TargetPlanet}.",
                                    $"Last I heard it passed through {quest.TargetPlanet}. Check the markets." },
            ["travel"]    = new[] { $"You need to be seen at {quest.TargetPlanet} for this to count.",
                                    $"Get yourself to {quest.TargetPlanet}. I'll know when you arrive." },
            ["attack"]    = new[] { $"The target operates out of {quest.ObjectiveZone} on {quest.TargetPlanet}. Be careful.",
                                    $"Imperial patrols have been seen at {quest.TargetPlanet}. That's where your quarry is.",
                                    $"Find them in {quest.ObjectiveZone} — they won't come quietly." },
            ["eliminate"] = new[] { $"The mark was last seen on {quest.TargetPlanet}, {quest.ObjectiveZone} zone.",
                                    $"Your target is a dangerous {quest.ObjectiveTarget}. Don't underestimate them." },
            ["generic"]   = new[] { $"Head to {quest.TargetPlanet} and see what you find.",
                                    $"Keep your eyes open on {quest.TargetPlanet}." },
        };

        var typeKey = hints.ContainsKey(quest.ObjectiveType) ? quest.ObjectiveType : "generic";
        var arr = hints[typeKey];
        var hint = arr[random.Next(arr.Length)];

        return new QuestEncounterEvent
        {
            Quest = quest,
            HintDialogue = hint,
            ZoneHint = quest.ObjectiveZone,
            SpawnsCombat = spawnsCombat,
            EnemyFaction = quest.Faction
        };
    }

    /// <summary>
    /// Returns a list of active quest titles whose TargetPlanet + ObjectiveZone match
    /// the current encounter location, so the UI can show a quest context banner.
    /// </summary>
    public List<(string Title, string ObjectiveType, string ChainId)> GetQuestEncounterContext(string planet, string zone)
    {
        return ActiveQuests
            .Where(q => q.Status is "active" or "ready"
                && string.Equals(q.TargetPlanet, planet, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(q.ObjectiveZone)
                    || string.Equals(q.ObjectiveZone, zone, StringComparison.OrdinalIgnoreCase)))
            .Select(q => (q.Title, q.ObjectiveType, q.ChainId))
            .ToList();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INDIVIDUAL ARMOR PIECES  (slot-based mix-and-match)
    // ═══════════════════════════════════════════════════════════════════════════
    private void InitIndividualArmorPieces()
    {
        // Explicit array helper avoids CS8323 (named arg followed by unnamed params element)
        static (string item, int qty)[] M(params (string item, int qty)[] x) => x;

        void P(string name, string slot, string cat, string desc,
               int ar, int hp, int stam, int val,
               bool heat=false, bool cold=false, bool acid=false, bool tox=false,
               bool vac=false, bool rad=false, bool water=false, bool stealth=false, bool saber=false,
               bool forge=false, bool bp=false, int credits=0,
               (string item, int qty)[]? mats = null)
        {
            Armors[name] = new ArmorBlueprint
            {
                Name=name, Slot=slot, Category=cat, Description=desc,
                ArmorRating=ar, HpBonus=hp, StaminaBonus=stam, BaseValue=val,
                HeatResistance=heat, ColdResistance=cold, AcidResistance=acid,
                ToxinResistance=tox, VacuumSealed=vac, RadiationShielded=rad,
                WaterResistant=water, StealthBonus=stealth, LightsaberDampening=saber,
                Craftable=true
            };
            if ((mats != null && mats.Length > 0) || credits > 0)
                ArmorRecipes[name] = new ArmorRecipe
                    { ArmorName=name, RequiresForge=forge, RequiresBlueprint=bp,
                      CreditCost=credits, Materials=mats?.ToList() ?? new() };
        }

        // ── Generic light pieces ───────────────────────────────────────────────
        P("Scout Hood",         "Helmet","light","A lightweight hood for scouts.",            1,2,1,40,  credits:20, mats:M(("rough cloth",2)));
        P("Scout Vest",         "Chest", "light","Padded scout vest.",                        2,5,2,80,  credits:30, mats:M(("rough cloth",3),("leather strip",1)));
        P("Scout Sleeves",      "Arms",  "light","Light padded arm wraps.",                   1,2,1,35,  credits:15, mats:M(("rough cloth",2)));
        P("Scout Trousers",     "Legs",  "light","Durable scout leggings.",                   1,3,1,50,  credits:20, mats:M(("rough cloth",3)));
        P("Scout Boots",        "Boots", "light","Soft soled boots.",                         0,2,1,30,  credits:10, mats:M(("leather strip",2)));
        P("Utility Belt",       "Belt",  "light","A belt with pockets for tools.",            0,0,2,50,  credits:20, mats:M(("leather strip",3)));

        // ── Generic medium pieces ──────────────────────────────────────────────
        P("Blast Helmet",       "Helmet","medium","Reinforced trooper helmet.",               2,5,1,120, credits:50, mats:M(("durasteel scrap",2)));
        P("Combat Vest",        "Chest", "medium","Standard combat chest armour.",            4,8,2,200, credits:80, mats:M(("durasteel scrap",3),("rubber seal compound",1)));
        P("Combat Pauldrons",   "Arms",  "medium","Shoulder and arm guards.",                 1,3,1,90,  credits:35, mats:M(("durasteel scrap",2)));
        P("Combat Greaves",     "Legs",  "medium","Metal-reinforced leg armour.",             2,5,1,140, credits:55, mats:M(("durasteel scrap",2),("rubber seal compound",1)));
        P("Combat Boots",       "Boots", "medium","Steel-toed military boots.",               1,3,1,80,  credits:30, mats:M(("durasteel scrap",1),("leather strip",2)));
        P("Trooper Belt",       "Belt",  "medium","Heavy-duty equipment belt.",               0,2,2,70,  credits:25, mats:M(("durasteel scrap",1),("leather strip",1)));

        // ── Generic heavy pieces ───────────────────────────────────────────────
        P("Heavy Battle Helm",  "Helmet","heavy","Full-coverage battle helmet.",              4,8,1,300, forge:true, credits:100, mats:M(("durasteel ingot",2)));
        P("Heavy Breastplate",  "Chest", "heavy","Solid steel breastplate.",                 6,14,2,500, forge:true, credits:160, mats:M(("durasteel ingot",3),("titanite ore",1)));
        P("Heavy Vambraces",    "Arms",  "heavy","Thick gauntlets and arm guards.",           3,5,1,220, forge:true, credits:80,  mats:M(("durasteel ingot",2)));
        P("Heavy Cuisses",      "Legs",  "heavy","Full metal leg armour.",                   4,8,1,350, forge:true, credits:120, mats:M(("durasteel ingot",2),("titanite ore",1)));
        P("Heavy Sabatons",     "Boots", "heavy","Plated metal boots.",                      2,4,0,180, forge:true, credits:60,  mats:M(("durasteel ingot",1)));

        // ── Clone Trooper Phase I set ──────────────────────────────────────────
        P("CT Phase I Helmet",     "Helmet","medium","Clone Phase I bucket.",                2,4,1,130, forge:true, bp:true, credits:55, mats:M(("plastoid alloy",1),("transparisteel fragment",1)));
        P("CT Phase I Chest",      "Chest", "medium","Clone Phase I torso plate.",           4,8,2,220, forge:true, bp:true, credits:90, mats:M(("plastoid alloy",2)));
        P("CT Phase I Pauldrons",  "Arms",  "medium","Clone Phase I arm guards.",            1,3,1, 90, forge:true, bp:true, credits:40, mats:M(("plastoid alloy",1)));
        P("CT Phase I Greaves",    "Legs",  "medium","Clone Phase I leg guards.",            2,5,1,150, forge:true, bp:true, credits:65, mats:M(("plastoid alloy",1)));
        P("CT Phase I Boots",      "Boots", "medium","Clone Phase I boots.",                 1,2,1, 70, forge:true, bp:true, credits:30, mats:M(("plastoid alloy",1)));

        // ── Clone Trooper Phase II set ─────────────────────────────────────────
        P("CT Phase II Helmet",    "Helmet","medium","Personalised Phase II helmet.",        3,5,1,170, cold:true, forge:true, bp:true, credits:70,  mats:M(("plastoid alloy",2),("transparisteel fragment",1)));
        P("CT Phase II Chest",     "Chest", "medium","Reinforced Phase II torso.",           5,10,2,280, cold:true, forge:true, bp:true, credits:110, mats:M(("plastoid alloy",3)));
        P("CT Phase II Pauldrons", "Arms",  "medium","Phase II shoulder plates.",            2,4,1,120, forge:true, bp:true, credits:50,  mats:M(("plastoid alloy",2)));
        P("CT Phase II Greaves",   "Legs",  "medium","Phase II leg guard.",                  3,6,1,190, forge:true, bp:true, credits:80,  mats:M(("plastoid alloy",2)));
        P("CT Phase II Boots",     "Boots", "medium","Phase II sealed boots.",               1,3,1, 90, vac:true, forge:true, bp:true, credits:40, mats:M(("plastoid alloy",1),("rubber seal compound",1)));

        // ── Stormtrooper set ───────────────────────────────────────────────────
        P("Stormtrooper Helmet",   "Helmet","medium","Imperial white bucket helmet.",        2,3,1,100, forge:true, bp:true, credits:45, mats:M(("plastoid alloy",1)));
        P("Stormtrooper Chest",    "Chest", "medium","Stormtrooper torso plate.",            3,6,2,160, forge:true, bp:true, credits:70, mats:M(("plastoid alloy",2)));
        P("Stormtrooper Arms",     "Arms",  "medium","Stormtrooper arm guards.",             1,2,1, 70, forge:true, bp:true, credits:30, mats:M(("plastoid alloy",1)));
        P("Stormtrooper Legs",     "Legs",  "medium","Stormtrooper leg armour.",             1,4,1,100, forge:true, bp:true, credits:45, mats:M(("plastoid alloy",1)));
        P("Stormtrooper Boots",    "Boots", "medium","Standard stormtrooper boots.",         1,2,1, 60, forge:true, bp:true, credits:25, mats:M(("plastoid alloy",1)));

        // ── Mandalorian iron set ───────────────────────────────────────────────
        P("Mandalorian Helmet",    "Helmet","heavy","Iconic T-visor helmet.",                3,6,1,380, heat:true, forge:true, bp:true, credits:100, mats:M(("iron ingot",2)));
        P("Mandalorian Chest",     "Chest", "heavy","Mandalorian beskar-framed cuirass.",    5,12,2,600, heat:true, forge:true, bp:true, credits:160, mats:M(("iron ingot",3),("durasteel ingot",1)));
        P("Mandalorian Vambraces", "Arms",  "heavy","Mandalorian arm guards with tools.",    2,5,1,250, forge:true, bp:true, credits:80,  mats:M(("iron ingot",2)));
        P("Mandalorian Greaves",   "Legs",  "heavy","Mandalorian leg armour.",               3,8,1,380, forge:true, bp:true, credits:110, mats:M(("iron ingot",2),("durasteel ingot",1)));
        P("Mandalorian Sabatons",  "Boots", "heavy","Mandalorian armoured boots.",           2,4,0,200, forge:true, bp:true, credits:70,  mats:M(("iron ingot",1)));

        // ── Beskar set ─────────────────────────────────────────────────────────
        P("Beskar Helmet",    "Helmet","heavy","Near-indestructible beskar visor helmet.",   4,8,1,700,  heat:true,cold:true,saber:true, forge:true, bp:true, credits:200, mats:M(("beskar ingot",1),("iron ingot",1)));
        P("Beskar Chest",     "Chest", "heavy","Full beskar breastplate.",                   6,14,2,1100, heat:true,cold:true,acid:true,saber:true, forge:true, bp:true, credits:320, mats:M(("beskar ingot",2),("titanite ingot",1)));
        P("Beskar Vambraces", "Arms",  "heavy","Beskar forearm plates.",                    3,6,1,480,  saber:true, forge:true, bp:true, credits:150, mats:M(("beskar ingot",1)));
        P("Beskar Greaves",   "Legs",  "heavy","Beskar leg armour.",                         4,9,1,700,  saber:true, forge:true, bp:true, credits:220, mats:M(("beskar ingot",1),("iron ingot",1)));
        P("Beskar Sabatons",  "Boots", "heavy","Beskar boots.",                              2,4,0,360,  forge:true, bp:true, credits:120, mats:M(("beskar ingot",1)));

        // ── Jedi Robes set ─────────────────────────────────────────────────────
        P("Jedi Hood",         "Helmet","light","Jedi training hood.",                       1,2,1,50,   credits:15, mats:M(("rough cloth",2)));
        P("Jedi Robe Chest",   "Chest", "light","Traditional Jedi robe upper.",              2,5,3,90,   credits:30, mats:M(("rough cloth",3)));
        P("Jedi Robe Sleeves", "Arms",  "light","Jedi robe long sleeves.",                   0,2,1,35,   credits:10, mats:M(("rough cloth",2)));
        P("Jedi Robe Leggings","Legs",  "light","Jedi robe lower garment.",                  1,3,2,60,   credits:20, mats:M(("rough cloth",2)));
        P("Jedi Sandals",      "Boots", "light","Simple Force-user footwear.",               0,1,1,25,   credits:5,  mats:M(("leather strip",1)));

        // ── Sith set ───────────────────────────────────────────────────────────
        P("Sith Mask",         "Helmet","medium","Dark-side mask that hides identity.",      2,4,1,200,  stealth:true, forge:true, credits:60, mats:M(("durasteel scrap",2),("black dye",1)));
        P("Sith Robe Chest",   "Chest", "medium","Dark-woven robe with hidden plates.",      3,8,2,280,  forge:true, credits:90, mats:M(("rough cloth",2),("durasteel scrap",2)));
        P("Sith Robe Sleeves", "Arms",  "medium","Armoured Sith sleeves.",                   1,3,1,120,  credits:40, mats:M(("rough cloth",2),("durasteel scrap",1)));
        P("Sith Robe Leggings","Legs",  "medium","Heavy Sith leg wraps.",                    2,5,1,180,  credits:60, mats:M(("rough cloth",2),("durasteel scrap",2)));
        P("Sith Boots",        "Boots", "medium","Heavy dark boots.",                        1,3,0,100,  credits:35, mats:M(("leather strip",2),("durasteel scrap",1)));

        // ── Rebel Alliance set ─────────────────────────────────────────────────
        P("Rebel Helmet",      "Helmet","medium","Rebel Alliance combat helmet.",            2,4,1,120,  credits:45, mats:M(("durasteel scrap",1),("rubber seal compound",1)));
        P("Rebel Body Armour", "Chest", "medium","Heterogeneous rebel vest.",                3,7,2,190,  credits:70, mats:M(("durasteel scrap",2),("rubber seal compound",1)));
        P("Rebel Pauldrons",   "Arms",  "medium","Rebel shoulder pads.",                     1,3,1, 80,  credits:30, mats:M(("durasteel scrap",1)));
        P("Rebel Trousers",    "Legs",  "medium","Reinforced rebel combat trousers.",        2,4,1,110,  credits:45, mats:M(("durasteel scrap",1),("rubber seal compound",1)));
        P("Rebel Boots",       "Boots", "medium","Standard rebel lace-ups.",                 1,2,1, 65,  credits:25, mats:M(("leather strip",2)));

        // ── Bounty Hunter set ──────────────────────────────────────────────────
        P("Hunter's Helm",     "Helmet","medium","Scarred and dented hunter's helmet.",      2,4,1,200,  stealth:true, credits:70, mats:M(("durasteel scrap",2),("leather strip",1)));
        P("Hunter's Vest",     "Chest", "medium","Multi-pocketed hunter's vest.",            4,8,2,350,  stealth:true, credits:120, mats:M(("durasteel scrap",3),("rubber seal compound",1)));
        P("Hunter's Gauntlets","Arms",  "medium","Reinforced gauntlets with weapon mounts.", 2,3,1,160,  credits:55, mats:M(("durasteel scrap",2)));
        P("Hunter's Trousers", "Legs",  "medium","Armoured bounty hunter trousers.",         2,5,1,230,  stealth:true, credits:80, mats:M(("durasteel scrap",2),("leather strip",1)));
        P("Hunter's Boots",    "Boots", "medium","Mag-locked boots for ship boarding.",      1,3,1,130,  credits:45, mats:M(("durasteel scrap",1),("rubber seal compound",1)));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BACKPACKS
    // ═══════════════════════════════════════════════════════════════════════════
    private void InitBackpacks()
    {
        Backpacks["Light Backpack"]       = new BackpackData { Name="Light Backpack",       Description="A small scout satchel. Fits any armor type.",              MinChestType="light",  CapacityMicroScu=500_000,   ArmorBonus=0, BaseValue=120,  ItemSizeMicroScu=200_000 };
        Backpacks["Medic Satchel"]        = new BackpackData { Name="Medic Satchel",         Description="A medical kit carrier. Fits any armor type.",             MinChestType="light",  CapacityMicroScu=400_000,   ArmorBonus=0, BaseValue=180,  ItemSizeMicroScu=200_000 };
        Backpacks["Explorer Pack"]        = new BackpackData { Name="Explorer Pack",         Description="Rugged field pack. Requires medium or heavy chest.",       MinChestType="medium", CapacityMicroScu=1_000_000, ArmorBonus=0, BaseValue=280,  ItemSizeMicroScu=300_000 };
        Backpacks["Military Backpack"]    = new BackpackData { Name="Military Backpack",     Description="Tactical pack for medium or heavy armor.",                 MinChestType="medium", CapacityMicroScu=1_500_000, ArmorBonus=1, BaseValue=420,  ItemSizeMicroScu=400_000 };
        Backpacks["Smuggler's Satchel"]   = new BackpackData { Name="Smuggler's Satchel",   Description="Inconspicuous bag. Fits any armor.",                       MinChestType="light",  CapacityMicroScu=600_000,   ArmorBonus=0, BaseValue=200,  ItemSizeMicroScu=250_000 };
        Backpacks["Heavy Field Pack"]     = new BackpackData { Name="Heavy Field Pack",      Description="Massive pack for heavy armor only.",                       MinChestType="heavy",  CapacityMicroScu=2_000_000, ArmorBonus=2, BaseValue=600,  ItemSizeMicroScu=500_000 };
        Backpacks["Mandalorian Jetpack"]  = new BackpackData { Name="Mandalorian Jetpack",   Description="Jetpack with storage compartment. Heavy armor only.",      MinChestType="heavy",  CapacityMicroScu=800_000,   ArmorBonus=3, BaseValue=1500, ItemSizeMicroScu=600_000 };
        Backpacks["Clone Rations Pack"]   = new BackpackData { Name="Clone Rations Pack",    Description="Republic clone field ration pack. Medium or heavy.",       MinChestType="medium", CapacityMicroScu=900_000,   ArmorBonus=0, BaseValue=250,  ItemSizeMicroScu=350_000 };
        Backpacks["Mining Satchel"]       = new BackpackData { Name="Mining Satchel",        Description="Reinforced ore-carrying satchel. Medium or heavy.",        MinChestType="medium", CapacityMicroScu=1_200_000, ArmorBonus=0, BaseValue=320,  ItemSizeMicroScu=380_000 };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HOUSE LISTINGS
    // ═══════════════════════════════════════════════════════════════════════════
    private void InitHouseListings()
    {
        void H(string name, string planet, string desc, int price, long scu) =>
            HouseListings[name] = new HouseListing { Name=name, Planet=planet, Description=desc, Price=price, StorageCapacityScu=scu };

        H("Corellian Apartment",     "Corellia",   "A modest urban flat in CorNET district.",         5_000,  10);
        H("Tatooine Moisture Farm",  "Tatooine",   "A working moisture farm with sand cellar.",        3_000,   8);
        H("Nar Shaddaa Hab Unit",    "Nar Shaddaa","A cramped hab on the Smuggler's Moon.",            4_000,  12);
        H("Naboo Villa",             "Naboo",      "A beautiful villa with Theed lake views.",        15_000,  30);
        H("Mandalorian Bunker",      "Mandalore",  "A fortified underground stronghold.",             20_000,  40);
        H("Lothal Farmstead",        "Lothal",     "A quiet prairie farmhouse.",                       6_000,  15);
        H("Hoth Outpost",            "Hoth",       "A survival outpost buried under snow.",            8_000,  18);
        H("Coruscant Tower Suite",   "Coruscant",  "Luxury suite in the upper city levels.",          30_000,  25);
        H("Dagobah Hut",             "Dagobah",    "A primitive hut in the bog.",                      1_000,   5);
        H("Bespin Cloud Loft",       "Bespin",     "A floating loft above the gas clouds.",           18_000,  22);
        H("Kashyyyk Treetop Lodge",  "Kashyyyk",   "A treehouse lodge in the Shadowlands.",            9_000,  20);
        H("Endor Ewok Hut",          "Endor",      "A wooden hut in an Ewok tree village.",            4_000,  10);
        H("Kessel Miners' Quarters", "Kessel",     "Rough quarters near the spice mines.",             2_500,   9);
        H("Kamino Research Flat",    "Kamino",     "A sterile flat above the ocean platform.",        12_000,  16);
        H("Mandalorian Covert",      "Mandalore",  "Hidden quarters in a Mandalorian covert.",        12_000,  35);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SCU CAPACITY + ITEM SIZES
    // ═══════════════════════════════════════════════════════════════════════════

    // Default item μSCU sizes for specific item names
    private static readonly Dictionary<string, long> ItemMicroScuSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Consumables
        ["field medpack"]        = 40_000,
        ["stim pack"]            = 20_000,
        ["power cell"]           = 15_000,
        ["fuel cell"]            = 30_000,
        ["circuit board"]        = 12_000,
        ["coolant unit"]         = 25_000,
        ["repair kit"]           = 50_000,
        ["thermal grenade"]      = 50_000,
        ["frag grenade"]         = 50_000,
        ["thermal detonator"]    = 60_000,
        // Weapons
        ["blaster pistol"]       = 60_000,
        ["heavy blaster"]        = 120_000,
        ["blaster rifle"]        = 150_000,
        ["ion rifle"]            = 150_000,
        ["lightsaber"]           = 80_000,
        ["electrostaff"]         = 120_000,
        ["bowcaster"]            = 130_000,
        // Misc gear
        ["smuggler cache"]       = 80_000,
        ["sensor array"]         = 60_000,
        ["hyperdrive part"]      = 200_000,
        ["hyperdrive stabilizer"]= 180_000,
        ["shield booster"]       = 100_000,
        ["laser upgrade"]        = 80_000,
    };

    public long GetItemMicroScuSize(string itemName)
    {
        if (ItemMicroScuSizes.TryGetValue(itemName, out var exact)) return exact;
        // Armor pieces by slot
        if (Armors.TryGetValue(itemName, out var armor))
            return armor.Slot switch
            {
                "Helmet"   => 200_000L,
                "Chest"    => 400_000L,
                "Arms"     => 150_000L,
                "Legs"     => 280_000L,
                "Boots"    => 100_000L,
                "Belt"     => 80_000L,
                "Full Suit"=> 800_000L,
                _ => 200_000L
            };
        // Backpacks
        if (Backpacks.TryGetValue(itemName, out var bp)) return bp.ItemSizeMicroScu;
        var lower = itemName.ToLowerInvariant();
        // Weapons
        if (lower.Contains("lightsaber"))                    return 80_000L;
        if (lower.Contains("rifle") || lower.Contains("carbine")) return 150_000L;
        if (lower.Contains("pistol") || lower.Contains("blaster")) return 60_000L;
        if (lower.Contains("grenade") || lower.Contains("detonator")) return 50_000L;
        if (lower.Contains("staff")  || lower.Contains("spear"))  return 120_000L;
        if (lower.Contains("sword")  || lower.Contains("vibro"))  return 80_000L;
        if (lower.Contains("bow")    || lower.Contains("caster")) return 130_000L;
        // Armor keywords
        if (lower.Contains("helmet") || lower.Contains("hood") || lower.Contains("mask")) return 200_000L;
        if (lower.Contains("chest")  || lower.Contains("vest") || lower.Contains("cuirass") || lower.Contains("breastplate") || lower.Contains("robe chest")) return 400_000L;
        if (lower.Contains("vambrace") || lower.Contains("gauntlet") || lower.Contains("pauldron") || lower.Contains("sleeve") || lower.Contains("arms")) return 150_000L;
        if (lower.Contains("greave") || lower.Contains("trouser") || lower.Contains("legging") || lower.Contains("cuisses") || lower.Contains("legs")) return 280_000L;
        if (lower.Contains("boot") || lower.Contains("sabaton") || lower.Contains("sandal")) return 100_000L;
        if (lower.Contains("belt"))  return 80_000L;
        if (lower.Contains("suit") || lower.Contains("armour") || lower.Contains("armor")) return 800_000L;
        if (lower.Contains("backpack") || lower.Contains("pack") || lower.Contains("satchel") || lower.Contains("jetpack")) return 300_000L;
        // Raw materials (small)
        if (lower.Contains("ore")  || lower.Contains("mineral") || lower.Contains("rock")) return 25_000L;
        if (lower.Contains("shard")|| lower.Contains("fragment")|| lower.Contains("dust")) return 10_000L;
        if (lower.Contains("ingot")|| lower.Contains("alloy")   || lower.Contains("compound")) return 40_000L;
        if (lower.Contains("plank")|| lower.Contains("wood")    || lower.Contains("log")) return 40_000L;
        if (lower.Contains("gas canister") || lower.Contains("tibanna")) return 60_000L;
        if (lower.Contains("resin")|| lower.Contains("crystal") || lower.Contains("shard")) return 15_000L;
        if (lower.Contains("cell") || lower.Contains("chip")    || lower.Contains("circuit")) return 15_000L;
        if (lower.Contains("medpack") || lower.Contains("stim"))  return 40_000L;
        return 100_000L; // default: 100 mSCU
    }

    public long GetInventoryUsedMicroScu(GameCharacter character)
        => character.Inventory.Sum(GetItemMicroScuSize);

    public long GetInventoryCapacityMicroScu(GameCharacter character)
        => character.BaseInventoryMicroScu;

    public long GetShipPersonalStorageCapacity(Ship ship)
    {
        if (ship.PersonalStorageCapacityMicroScu > 0) return ship.PersonalStorageCapacityMicroScu;
        return ship.SizeClass switch
        {
            "S"  => 1_000_000L,
            "M"  => 5_000_000L,
            "L"  => 20_000_000L,
            "XL" => 50_000_000L,
            _ => 2_000_000L
        };
    }

    public long GetShipPersonalStorageUsed(Ship ship)
        => ship.PersonalStorageItems.Sum(GetItemMicroScuSize);

    public long GetCargoUsedScu(Ship ship)
        => ship.CargoItems.Count; // each cargo item = 1 SCU

    // ═══════════════════════════════════════════════════════════════════════════
    // SLOT-BASED ARMOR EQUIP / UNEQUIP
    // ═══════════════════════════════════════════════════════════════════════════

    public void RecalcArmorRating(GameCharacter character)
    {
        int total = 0;
        // Unique piece names only (guard against duplicate slot references)
        foreach (var name in character.EquippedArmorPieces.Values
                     .Where(v => !string.IsNullOrEmpty(v))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (Armors.TryGetValue(name, out var a)) total += a.ArmorRating;
        }
        // Backpack AR bonus
        if (!string.IsNullOrEmpty(character.EquippedBackpack) &&
            Backpacks.TryGetValue(character.EquippedBackpack, out var bp))
            total += bp.ArmorBonus;
        character.Armor = total;
    }

    public string EquipArmorPiece(GameCharacter character, string armorName)
    {
        if (!Armors.TryGetValue(armorName, out var armor))
            return $"Unknown armor piece: {armorName}.";
        if (!character.Inventory.Any(x => x.Equals(armorName, StringComparison.OrdinalIgnoreCase)))
            return $"You don't have '{armorName}' in your inventory.";
        if (!string.IsNullOrWhiteSpace(armor.FactionRequired) &&
            !character.Faction.Equals(armor.FactionRequired, StringComparison.OrdinalIgnoreCase))
            return $"{armorName} requires {armor.FactionRequired} faction.";

        var slot = armor.Slot;

        if (slot == "Full Suit")
        {
            // Remove all individual pieces first
            UnequipAllArmor(character);
            character.EquippedArmorPieces["Full Suit"] = armorName;
            character.EquippedArmor = armorName;
            character.MaxHp       += armor.HpBonus;
            character.MaxStamina  += armor.StaminaBonus;
            character.Hp           = Math.Min(character.Hp + armor.HpBonus, character.MaxHp);
        }
        else
        {
            // Can't add individual pieces if a full suit is equipped
            if (character.EquippedArmorPieces.TryGetValue("Full Suit", out var fs) && !string.IsNullOrEmpty(fs))
                return "Remove your full suit first before equipping individual pieces.";
            // Unequip any existing piece in this slot
            if (character.EquippedArmorPieces.TryGetValue(slot, out var existing) && !string.IsNullOrEmpty(existing))
                UnequipArmorSlot(character, slot);
            character.EquippedArmorPieces[slot] = armorName;
            if (slot == "Chest") character.EquippedArmor = armorName;
            character.MaxHp       += armor.HpBonus;
            character.MaxStamina  += armor.StaminaBonus;
            character.Hp           = Math.Min(character.Hp + armor.HpBonus, character.MaxHp);
        }
        RecalcArmorRating(character);

        var envPerks = new List<string>();
        if (armor.HeatResistance)      envPerks.Add("Heat");
        if (armor.ColdResistance)      envPerks.Add("Cold");
        if (armor.VacuumSealed)        envPerks.Add("Vacuum");
        if (armor.LightsaberDampening) envPerks.Add("Saber-damp");
        var perkStr = envPerks.Count > 0 ? $"  |  {string.Join(", ", envPerks)}" : "";
        return $"Equipped [{slot}] {armorName} → AR +{armor.ArmorRating}, HP +{armor.HpBonus}, Stamina +{armor.StaminaBonus}{perkStr}. Total AR: {character.Armor}.";
    }

    public string UnequipArmorSlot(GameCharacter character, string slot)
    {
        if (!character.EquippedArmorPieces.TryGetValue(slot, out var armorName) || string.IsNullOrEmpty(armorName))
            return $"Nothing equipped in the {slot} slot.";
        if (Armors.TryGetValue(armorName, out var armor))
        {
            character.MaxHp      = Math.Max(10, character.MaxHp - armor.HpBonus);
            character.MaxStamina = Math.Max(5, character.MaxStamina - armor.StaminaBonus);
            character.Hp         = Math.Min(character.Hp, character.MaxHp);
        }
        var removed = character.EquippedArmorPieces[slot];
        character.EquippedArmorPieces.Remove(slot);
        if (slot is "Chest" or "Full Suit") character.EquippedArmor = "";
        RecalcArmorRating(character);
        return $"Removed {removed} from {slot} slot.";
    }

    public void UnequipAllArmor(GameCharacter character)
    {
        foreach (var slot in character.EquippedArmorPieces.Keys.ToList())
            UnequipArmorSlot(character, slot);
        character.EquippedArmorPieces.Clear();
        character.EquippedArmor = "";
        RecalcArmorRating(character);
    }

    public string GetArmorSummary(GameCharacter character)
    {
        if (character.EquippedArmorPieces.Count == 0) return "No armor equipped.";
        var parts = character.EquippedArmorPieces
            .Select(kvp => $"{kvp.Key}: {kvp.Value}");
        return $"AR:{character.Armor}  |  {string.Join("  |  ", parts)}";
    }

    // ── Update legacy EquipArmor/UnequipArmor to delegate to new system ────────
    // (existing EquipArmor + UnequipArmor methods below are replaced here)

    // ═══════════════════════════════════════════════════════════════════════════
    // BACKPACK EQUIP / UNEQUIP
    // ═══════════════════════════════════════════════════════════════════════════

    public string EquipBackpack(GameCharacter character, string backpackName)
    {
        if (!Backpacks.TryGetValue(backpackName, out var bp))
            return $"Unknown backpack: {backpackName}.";
        if (!character.Inventory.Any(x => x.Equals(backpackName, StringComparison.OrdinalIgnoreCase)))
            return $"You don't have '{backpackName}' in your inventory.";

        // Determine chest type
        var chestName = character.EquippedArmorPieces.TryGetValue("Chest", out var cn) ? cn :
                        (character.EquippedArmorPieces.TryGetValue("Full Suit", out var fs) ? fs : "");
        var chestType = !string.IsNullOrEmpty(chestName) && Armors.TryGetValue(chestName, out var ca)
                        ? ca.Category : "light";

        bool ok = bp.MinChestType switch
        {
            "heavy"  => chestType == "heavy",
            "medium" => chestType is "medium" or "heavy",
            _        => true // "light" = any or no chest
        };
        if (!ok)
        {
            var req = bp.MinChestType == "heavy" ? "a heavy chest piece" : "a medium or heavy chest piece";
            return $"{backpackName} requires {req} to attach to.";
        }

        // Unequip existing backpack
        if (!string.IsNullOrEmpty(character.EquippedBackpack))
            UnequipBackpack(character);

        character.EquippedBackpack = backpackName;
        character.BaseInventoryMicroScu += bp.CapacityMicroScu;
        RecalcArmorRating(character);
        return $"Attached {backpackName} (+{ScuConversion.FormatMicroScu(bp.CapacityMicroScu)} inventory capacity).";
    }

    public string UnequipBackpack(GameCharacter character)
    {
        if (string.IsNullOrEmpty(character.EquippedBackpack)) return "No backpack equipped.";
        if (Backpacks.TryGetValue(character.EquippedBackpack, out var bp))
            character.BaseInventoryMicroScu = Math.Max(2_000_000L, character.BaseInventoryMicroScu - bp.CapacityMicroScu);
        var prev = character.EquippedBackpack;
        character.EquippedBackpack = "";
        RecalcArmorRating(character);
        return $"Removed {prev}.";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STORAGE TRANSFER HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    public (bool ok, string msg) TransferToShipPersonal(GameCharacter character, string itemName)
    {
        if (character.Ship == null) return (false, "No ship docked.");
        var idx = character.Inventory.FindIndex(x => x.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return (false, $"'{itemName}' not in your bag.");
        var size = GetItemMicroScuSize(itemName);
        var cap  = GetShipPersonalStorageCapacity(character.Ship);
        var used = GetShipPersonalStorageUsed(character.Ship);
        if (used + size > cap)
            return (false, $"Personal Storage Full ({ScuConversion.FormatMicroScu(used)}/{ScuConversion.FormatMicroScu(cap)}).");
        character.Inventory.RemoveAt(idx);
        character.Ship.PersonalStorageItems.Add(itemName);
        return (true, $"Moved '{itemName}' → Ship Locker.");
    }

    public (bool ok, string msg) TransferFromShipPersonal(GameCharacter character, string itemName)
    {
        if (character.Ship == null) return (false, "No ship docked.");
        var idx = character.Ship.PersonalStorageItems.FindIndex(x => x.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return (false, $"'{itemName}' not in ship locker.");
        var size = GetItemMicroScuSize(itemName);
        var cap  = GetInventoryCapacityMicroScu(character);
        var used = GetInventoryUsedMicroScu(character);
        if (used + size > cap)
            return (false, $"Inventory full ({ScuConversion.FormatMicroScu(used)}/{ScuConversion.FormatMicroScu(cap)}).");
        character.Ship.PersonalStorageItems.RemoveAt(idx);
        character.Inventory.Add(itemName);
        return (true, $"Retrieved '{itemName}' from Ship Locker.");
    }

    public (bool ok, string msg) TransferToShipCargo(GameCharacter character, string itemName)
    {
        if (character.Ship == null) return (false, "No ship docked.");
        var idx = character.Inventory.FindIndex(x => x.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return (false, $"'{itemName}' not in your bag.");
        if (GetCargoUsedScu(character.Ship) >= character.Ship.CargoCapacity)
            return (false, $"Cargo Bay Full ({character.Ship.CargoItems.Count}/{character.Ship.CargoCapacity} SCU).");
        character.Inventory.RemoveAt(idx);
        character.Ship.CargoItems.Add(itemName);
        return (true, $"Loaded '{itemName}' into Cargo Bay.");
    }

    public (bool ok, string msg) TransferFromShipCargo(GameCharacter character, string itemName)
    {
        if (character.Ship == null) return (false, "No ship docked.");
        var idx = character.Ship.CargoItems.FindIndex(x => x.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return (false, $"'{itemName}' not in cargo.");
        var size = GetItemMicroScuSize(itemName);
        var cap  = GetInventoryCapacityMicroScu(character);
        var used = GetInventoryUsedMicroScu(character);
        if (used + size > cap)
            return (false, $"Inventory full ({ScuConversion.FormatMicroScu(used)}/{ScuConversion.FormatMicroScu(cap)}).");
        character.Ship.CargoItems.RemoveAt(idx);
        character.Inventory.Add(itemName);
        return (true, $"Unloaded '{itemName}' from Cargo Bay.");
    }

    public (bool ok, string msg) TransferToHome(GameCharacter character, string itemName)
    {
        if (character.Home == null) return (false, "You don't own a home.");
        var idx = character.Inventory.FindIndex(x => x.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return (false, $"'{itemName}' not in your bag.");
        if (character.Home.StorageItems.Count >= character.Home.StorageCapacityScu)
            return (false, $"Home Storage Full ({character.Home.StorageItems.Count}/{character.Home.StorageCapacityScu} SCU).");
        character.Inventory.RemoveAt(idx);
        character.Home.StorageItems.Add(itemName);
        return (true, $"Stored '{itemName}' at home.");
    }

    public (bool ok, string msg) TransferFromHome(GameCharacter character, string itemName)
    {
        if (character.Home == null) return (false, "You don't own a home.");
        var idx = character.Home.StorageItems.FindIndex(x => x.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return (false, $"'{itemName}' not in home storage.");
        var size = GetItemMicroScuSize(itemName);
        var cap  = GetInventoryCapacityMicroScu(character);
        var used = GetInventoryUsedMicroScu(character);
        if (used + size > cap)
            return (false, $"Inventory full ({ScuConversion.FormatMicroScu(used)}/{ScuConversion.FormatMicroScu(cap)}).");
        character.Home.StorageItems.RemoveAt(idx);
        character.Inventory.Add(itemName);
        return (true, $"Retrieved '{itemName}' from home.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HOUSING
    // ═══════════════════════════════════════════════════════════════════════════

    public (bool ok, string msg) BuyHouse(GameCharacter character, string listingName)
    {
        if (character.Home != null)
            return (false, $"You already own {character.Home.Name}. Sell it first.");
        if (!HouseListings.TryGetValue(listingName, out var listing))
            return (false, $"Unknown property: {listingName}.");
        if (!listing.Planet.Equals(character.Location, StringComparison.OrdinalIgnoreCase))
            return (false, $"Travel to {listing.Planet} first to purchase {listing.Name}.");
        if (character.Credits < listing.Price)
            return (false, $"Need {listing.Price} credits (have {character.Credits}).");
        character.Credits -= listing.Price;
        character.Home = new PlayerHome
        {
            Name = listing.Name, Planet = listing.Planet,
            StorageCapacityScu = listing.StorageCapacityScu,
            PurchasedFor = listing.Price, StorageItems = new()
        };
        return (true, $"You purchase {listing.Name} on {listing.Planet}. Storage: {listing.StorageCapacityScu} SCU.");
    }

    public (bool ok, string msg) SellHouse(GameCharacter character)
    {
        if (character.Home == null) return (false, "You don't own a home.");
        var price = character.Home.PurchasedFor / 2;
        character.Credits += price;
        // Move items to cargo or drop
        if (character.Ship != null)
            foreach (var item in character.Home.StorageItems.ToList())
                if (GetCargoUsedScu(character.Ship) < character.Ship.CargoCapacity)
                    character.Ship.CargoItems.Add(item);
        var name = character.Home.Name;
        character.Home = null;
        return (true, $"Sold {name} for {price} credits.");
    }

    public List<HouseListing> GetHouseListingsAtLocation(string planet)
        => HouseListings.Values.Where(h => h.Planet.Equals(planet, StringComparison.OrdinalIgnoreCase)).ToList();

    public bool TryTriggerLatentForceEncounter(GameCharacter character, out string seekerFaction, out string message, bool forceTrigger = false)
    {
        seekerFaction = "Jedi"; message = "";
        // Legacy hook — new system uses ShouldTriggerJediAwakening
        return false;
    }

    public string ResolveLatentForceChoice(GameCharacter character, bool acceptTraining, string seekerFaction)
        => acceptTraining ? AcceptJediTraining(character) : DeclineJediTraining(character);

    public Quest GenerateQuest(string planetName, GameCharacter character)
    {
        if (ActiveQuests.Count(q => q.Status == "active") >= 3)
        {
            return new Quest { Id = "none", Title = "No more assignments", Description = "You already have several active missions.", TargetPlanet = planetName, Faction = "Independent", Status = "blocked" };
        }

        var quest = BuildProceduralQuest(character, planetName, "market", "Job Broker", "Human", forcedObjectiveType: null);
        quest.IsNpcGenerated = false;
        quest.IssuerName = "Job Broker";
        quest.IssuerSpecies = "Human";
        quest.IssuerPlanet = planetName;

        ActiveQuests.Add(quest);
        AdjustFactionStanding(quest.Faction, 2);
        return quest;
    }

    public bool TryOfferNpcQuest(GameCharacter character, string planetName, string zone, string npcName, string npcSpecies, out Quest? quest, out string message, bool forceOffer = false, string? forcedObjectiveType = null)
    {
        quest = null;
        message = string.Empty;

        // ── Faction standing lock check ───────────────────────────────────
        // Pick a faction for this NPC, then verify the player isn't locked out
        var candidateFaction = PickNpcFaction(planetName);
        var standing = FactionStandings.GetValueOrDefault(candidateFaction);
        if (standing <= FactionQuestLockThreshold)
        {
            // Try a neutral/independent faction instead
            candidateFaction = "Independent";
        }

        var activeCount = ActiveQuests.Count(q => q.Status is "active" or "ready");
        if (activeCount >= 5)
        {
            message = "Your datapad is full. Finish current contracts before taking another mission.";
            return false;
        }

        var offerChance = 0.52;
        if (!forceOffer && random.NextDouble() > offerChance)
            return false;

        quest = BuildProceduralQuest(character, planetName, zone, npcName, npcSpecies, forcedObjectiveType);
        // Override faction with our standing-checked faction
        if (!quest.Faction.Equals("Independent", StringComparison.OrdinalIgnoreCase))
        {
            var qStanding = FactionStandings.GetValueOrDefault(quest.Faction);
            if (qStanding <= FactionQuestLockThreshold)
                quest.Faction = candidateFaction;
        }
        quest.IsNpcGenerated = true;
        AdjustFactionStanding(quest.Faction, 2);

        // 15 % chance: the random encounter seeds a full procedural chain.
        if (random.NextDouble() < 0.15)
        {
            var chain = GenerateProceduralChain(quest.Faction, character, planetName);
            // GenerateProceduralChain already adds all steps to ActiveQuests.
            quest = ActiveQuests.First(q => q.ChainId == chain.Id && q.ChainStep == 1);
            message = $"{npcName} has ongoing work — a contract chain has begun: \"{chain.Title}\" ({chain.Steps.Count} steps). Check the Quest tab.";
        }
        else
        {
            ActiveQuests.Add(quest);
            message = $"{npcName} offers a contract: {quest.Description}";
        }
        return true;
    }

    private string PickNpcFaction(string planetName)
    {
        // Bias faction selection by planet characteristics
        if (Planets.TryGetValue(planetName, out var planet))
        {
            var name = planetName.ToLowerInvariant();
            if (name is "coruscant" or "geonosis") return random.NextDouble() < 0.6 ? "Empire" : "Guilds";
            if (name is "dagobah" or "dantooine" or "ahch-to") return "Jedi";
            if (name is "mandalore") return "Mandalorians";
            if (name is "hoth" or "yavin" or "lothal" or "endor") return random.NextDouble() < 0.7 ? "Rebels" : "Independent";
            if (name is "tatooine" or "corellia" or "bespin") return random.NextDouble() < 0.5 ? "Smugglers" : "Guilds";
        }
        var factions = new[] { "Rebels", "Empire", "Smugglers", "Guilds", "Mandalorians", "Independent", "Jedi" };
        return factions[random.Next(factions.Length)];
    }

    public IReadOnlyList<string> RefreshQuestProgress(GameCharacter character, string context = "")
    {
        var updates = new List<string>();
        foreach (var quest in ActiveQuests.Where(q => q.Status is "active" or "ready"))
        {
            var before = quest.ObjectiveProgress;
            var required = Math.Max(1, quest.ObjectiveRequired);

            if (quest.ObjectiveType.Equals("fetch", StringComparison.OrdinalIgnoreCase))
            {
                quest.ObjectiveProgress = character.Inventory.Any(x => string.Equals(x, quest.ObjectiveTarget, StringComparison.OrdinalIgnoreCase)) ? 1 : 0;
            }
            else if (quest.ObjectiveType.Equals("travel", StringComparison.OrdinalIgnoreCase))
            {
                quest.ObjectiveProgress = string.Equals(character.Location, quest.TargetPlanet, StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            }

            quest.ObjectiveProgress = Math.Clamp(quest.ObjectiveProgress, 0, required);
            if (before != quest.ObjectiveProgress)
            {
                updates.Add($"Quest progress [{quest.Title}]: {quest.ObjectiveProgress}/{required}.");
            }

            if (quest.ObjectiveProgress >= required && quest.Status == "active")
            {
                quest.Status = "ready";
                updates.Add($"Quest ready to turn in: {quest.Title}.");
            }
            else if (quest.ObjectiveProgress < required && quest.Status == "ready")
            {
                quest.Status = "active";
            }
        }

        return updates;
    }

    public string GetQuestJournal(GameCharacter character)
    {
        var active = ActiveQuests.Where(q => q.Status is "active" or "ready").ToList();
        if (active.Count == 0)
        {
            return "No active quests. Encounter NPCs to receive procedural contracts.";
        }

        var lines = new List<string>();
        foreach (var quest in active)
        {
            var required = Math.Max(1, quest.ObjectiveRequired);
            lines.Add($"- {quest.Title} [{quest.Status}] {quest.ObjectiveProgress}/{required} | Type: {quest.ObjectiveType} | Target: {quest.ObjectiveTarget} | Planet: {quest.TargetPlanet}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    public string CompleteQuest(GameCharacter character, string questId)
    {
        RefreshQuestProgress(character, "turn-in");
        var quest = ActiveQuests.FirstOrDefault(q => q.Id == questId && q.Status is "active" or "ready");
        if (quest is null)
        {
            return "No active quest matches that request.";
        }

        var required = Math.Max(1, quest.ObjectiveRequired);
        if (quest.ObjectiveProgress < required)
        {
            return $"Quest objective is incomplete: {quest.ObjectiveProgress}/{required}.";
        }

        if (quest.ObjectiveType.Equals("fetch", StringComparison.OrdinalIgnoreCase))
        {
            if (!ConsumeInventoryItem(character, quest.ObjectiveTarget, required))
            {
                return $"You still need {required}x {quest.ObjectiveTarget} to complete this quest.";
            }
        }

        quest.Status = "completed";
        character.Credits += quest.RewardCredits;
        character.Experience += quest.RewardXp;
        character.Reputation += 6;
        AdjustFactionStanding(quest.Faction, 5);
        if (!string.IsNullOrWhiteSpace(quest.RewardItem))
            TryAddInventoryItem(character, quest.RewardItem);

        var rewardNotes = new List<string>();
        if (!string.IsNullOrWhiteSpace(quest.RewardBlueprint))
        {
            var unlockText = UnlockBlueprint(character, quest.RewardBlueprint, $"quest {quest.Title}");
            if (!string.IsNullOrWhiteSpace(unlockText)) rewardNotes.Add(unlockText);
        }
        rewardNotes.AddRange(GrantReputationBlueprintRewards(character, quest.Faction));

        // ── auto-advance chain: queue the next step as inactive until player picks it up ──
        string chainNote = "";
        if (!string.IsNullOrEmpty(quest.ChainNextStep) && StorylineChains.TryGetValue(quest.ChainId, out var chain))
        {
            var nextDef = chain.Steps.FirstOrDefault(s => s.Id == quest.ChainNextStep);
            if (nextDef != null && !ActiveQuests.Any(q => q.Id == nextDef.Id))
            {
                var nextClone = CloneQuest(nextDef);
                nextClone.Status = "inactive"; // becomes active when player chooses to start it
                ActiveQuests.Add(nextClone);
                chainNote = $" Next chain step unlocked: \"{nextClone.Title}\".";
            }
        }

        var suffix = rewardNotes.Count == 0 ? "" : " " + string.Join(" ", rewardNotes);
        return $"Quest completed: {quest.Title}. You earned {quest.RewardCredits} credits and {quest.RewardXp} XP.{suffix}{chainNote}";
    }

    public string ActivateChainStep(GameCharacter character, string questId)
    {
        var q = ActiveQuests.FirstOrDefault(q => q.Id == questId && q.Status == "inactive");
        if (q is null) return "No inactive chain step with that ID.";
        if (ActiveQuests.Count(x => x.Status is "active" or "ready") >= 5) return "Datapad full — finish current missions first.";
        q.Status = "active";
        return $"Started chain quest: {q.Title}";
    }

    /// <summary>
    /// Advance combat kill progress for a specific quest by ID (called from QuestCompletionWindow).
    /// </summary>
    public void RegisterCombatQuestKill(GameCharacter character, string questId)
    {
        var q = ActiveQuests.FirstOrDefault(x => x.Id == questId && x.Status is "active" or "ready");
        if (q is null) return;
        var required = Math.Max(1, q.ObjectiveRequired);
        q.ObjectiveProgress = Math.Min(required, q.ObjectiveProgress + 1);
        if (q.ObjectiveProgress >= required)
            q.Status = "ready";
    }

    /// <summary>
    /// Simulate a faction-spy ambush when the player's standing with an enemy faction crosses a hostility threshold.
    /// Returns a message if an ambush triggers, or empty string if nothing happens.
    /// </summary>
    public string TryTriggerFactionAmbush(GameCharacter character, string zone)
    {
        // For each faction the player has positive standing with, check if its rivals might send a spy
        foreach (var kv in FactionStandings.Where(k => k.Value >= 5))
        {
            if (!FactionRivalries.TryGetValue(kv.Key, out var rivals)) continue;
            foreach (var rival in rivals)
            {
                var rivalStanding = FactionStandings.GetValueOrDefault(rival);
                // rival is hostile toward player if player has low or negative rival standing
                if (rivalStanding > -5) continue;
                if (random.NextDouble() > 0.08) continue; // 8% chance per check

                var spyName = $"{rival} Operative";
                var ambushDmg = Math.Max(2, Math.Abs(rivalStanding) / 5);
                character.Hp = Math.Max(1, character.Hp - ambushDmg);
                AdjustFactionStanding(rival, -1); // spy got what they came for
                return $"[AMBUSH] A {spyName} ambushed you in the {zone}! You took {ambushDmg} damage. " +
                       $"Your reputation with {rival} drops further.";
            }
        }
        return "";
    }

    private Quest BuildProceduralQuest(GameCharacter character, string planetName, string zone, string npcName, string npcSpecies, string? forcedObjectiveType)
    {
        var availableFactions = new[] { "Smugglers", "Rebels", "Guilds", "Independent", "Jedi", "Empire" };
        var faction = availableFactions[random.Next(availableFactions.Length)];

        var objectiveType = forcedObjectiveType?.ToLowerInvariant();
        if (objectiveType is not "fetch" and not "combat" and not "travel")
        {
            objectiveType = random.NextDouble() switch
            {
                < 0.36 => "fetch",
                < 0.73 => "combat",
                _ => "travel"
            };
        }

        var opLead = new[] { "Directive", "Contract", "Protocol", "Signal", "Run", "Vector", "Cipher", "Operation" };
        var opColor = new[] { "Cinder", "Dust", "Obsidian", "Ion", "Ghost", "Nova", "Ember", "Horizon" };
        var opTail = new[] { "Ledger", "Spoke", "Lane", "Beacon", "Drift", "Node", "Pulse", "Relay" };
        var title = $"{opLead[random.Next(opLead.Length)]} {opColor[random.Next(opColor.Length)]} {opTail[random.Next(opTail.Length)]} {random.Next(10, 99)}-{(char)('A' + random.Next(0, 26))}";

        var quest = new Quest
        {
            Id = $"Q{ActiveQuests.Count + 1}-{DateTime.Now.Ticks}",
            Title = title,
            TargetPlanet = planetName,
            Faction = faction,
            Status = "active",
            ObjectiveType = objectiveType,
            ObjectiveRequired = 1,
            ObjectiveProgress = 0,
            IssuerName = npcName,
            IssuerSpecies = npcSpecies,
            IssuerPlanet = planetName
        };

        if (objectiveType == "fetch")
        {
            var itemPool = new List<string>();
            if (PlanetRawMaterials.TryGetValue(planetName, out var materials)) itemPool.AddRange(materials);
            itemPool.AddRange(RefiningMap.Values.Take(20));
            itemPool.AddRange(CraftableItems.Keys.Take(20));
            itemPool.AddRange(new[] { "power cell", "repair kit", "field medpack", "sensor array", "durasteel ingot" });
            itemPool = itemPool.Where(IsInventoryEligible).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (itemPool.Count == 0) itemPool.Add("repair kit");

            quest.ObjectiveTarget = itemPool[random.Next(itemPool.Count)];
            quest.ObjectiveRequired = 1;
            quest.Description = $"{npcName} needs a fast pickup: acquire {quest.ObjectiveTarget} and return for discreet transfer on {planetName}.";
        }
        else if (objectiveType == "combat")
        {
            var zones = new[] { "slums", "dock", "ruins", "wilderness", "market" };
            quest.ObjectiveZone = zones[random.Next(zones.Length)];
            quest.ObjectiveTarget = "hostiles";
            quest.ObjectiveRequired = random.Next(2, 5);
            quest.Description = $"{npcName} marks threats in the {quest.ObjectiveZone}. Defeat {quest.ObjectiveRequired} hostile targets to stabilize local routes.";
        }
        else
        {
            var destinations = Planets.Keys.Where(x => !string.Equals(x, planetName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (destinations.Count == 0) destinations.Add(planetName);
            quest.TargetPlanet = destinations[random.Next(destinations.Count)];
            quest.ObjectiveTarget = quest.TargetPlanet;
            quest.ObjectiveRequired = 1;
            quest.Description = $"{npcName} needs a courier run. Travel to {quest.TargetPlanet} and confirm delivery to complete the contract.";
        }

        var difficulty = objectiveType == "combat" ? quest.ObjectiveRequired + 1 : objectiveType == "travel" ? 3 : 2;
        quest.RewardCredits = 35 + difficulty * random.Next(14, 22);
        quest.RewardXp = 8 + difficulty * random.Next(3, 6);
        quest.RewardItem = PickCombatRewardItem(planetName, quest.ObjectiveTarget);

        // Blueprint tier: fetch=1 (basic crafting), travel=2 (mid), combat scales with kill count.
        int bpTier = objectiveType switch
        {
            "fetch"  => 1,
            "travel" => 2,
            "combat" => quest.ObjectiveRequired >= 4 ? 3 : 2,
            _        => 1
        };
        // Try faction pool first; fall back to generic crafting blueprint.
        quest.RewardBlueprint = PickFactionBlueprintReward(faction, difficulty, character);
        if (string.IsNullOrWhiteSpace(quest.RewardBlueprint))
            quest.RewardBlueprint = PickGenericCraftingBlueprint(character, bpTier);
        return quest;
    }

    // ── Faction blueprint pool ────────────────────────────────────────────────
    // Each entry: (minStanding, blueprintName, isCapitalShip, isPartBlueprint)
    private record FactionBpEntry(int MinStanding, string Blueprint, bool IsCapital = false, bool IsPart = false);

    private readonly Dictionary<string, List<FactionBpEntry>> FactionBlueprintPools = new(StringComparer.OrdinalIgnoreCase);

    private void InitFactionBlueprintPools()
    {
        // ── Rebels ─────────────────────────────────────────────────────────
        FactionBlueprintPools["Rebels"] = new()
        {
            new(5,  "X-Wing hyperdrive blueprint",   IsPart: true),
            new(5,  "X-Wing thrusters blueprint",    IsPart: true),
            new(5,  "X-Wing sensors blueprint",      IsPart: true),
            new(10, "Y-Wing ion capacitor blueprint", IsPart: true),
            new(12, "X-Wing",          IsCapital: false),
            new(14, "Y-Wing",          IsCapital: false),
            new(18, "A-Wing",          IsCapital: false),
            new(20, "B-Wing",          IsCapital: false),
            new(22, "U-Wing",          IsCapital: false),
            new(30, "Corellian Corvette", IsCapital: true),
        };
        // ── Empire ─────────────────────────────────────────────────────────
        FactionBlueprintPools["Empire"] = new()
        {
            new(5,  "TIE Fighter thrusters blueprint", IsPart: true),
            new(5,  "TIE Fighter hyperdrive blueprint", IsPart: true),
            new(10, "TIE Bomber bomb launcher blueprint", IsPart: true),
            new(12, "TIE Fighter",        IsCapital: false),
            new(14, "TIE Bomber",         IsCapital: false),
            new(18, "TIE Interceptor",    IsCapital: false),
            new(20, "TIE Advanced x1",   IsCapital: false),
            new(22, "Lambda-class Shuttle", IsCapital: false),
            new(30, "Imperial Star Destroyer", IsCapital: true),
        };
        // ── Smugglers ───────────────────────────────────────────────────────
        FactionBlueprintPools["Smugglers"] = new()
        {
            new(5,  "YT-1300 hyperdrive blueprint", IsPart: true),
            new(5,  "YT-1300 reactor blueprint",    IsPart: true),
            new(10, "YT-1300 navicomputer blueprint", IsPart: true),
            new(12, "YT-1300",         IsCapital: false),
            new(14, "Razor Crest",     IsCapital: false),
            new(18, "Slave I",         IsCapital: false),
            new(22, "Millennium Falcon", IsCapital: false),
            new(30, "Corellian Corvette", IsCapital: true),
        };
        // ── Mandalorians ────────────────────────────────────────────────────
        FactionBlueprintPools["Mandalorians"] = new()
        {
            new(5,  "beskar pauldron blueprint",   IsPart: true),
            new(5,  "mandalorian helmet blueprint", IsPart: true),
            new(8,  "mandalorian vambrace blueprint", IsPart: true),
            new(10, "beskad blueprint",            IsPart: true),
            new(12, "Razor Crest",     IsCapital: false),
            new(20, "Upsilon-class shuttle", IsCapital: false),
            new(30, "Republic Gunship", IsCapital: true),
        };
        // ── Jedi ────────────────────────────────────────────────────────────
        FactionBlueprintPools["Jedi"] = new()
        {
            new(5,  "lightsaber hilt blueprint",     IsPart: true),
            new(5,  "kyber crystal blueprint",       IsPart: true),
            new(8,  "lightsaber assembly blueprint", IsPart: true),
            new(12, "Jedi Starfighter",  IsCapital: false),
            new(14, "Delta-7 Jedi Starfighter", IsCapital: false),
            new(18, "Eta-2 Actis",       IsCapital: false),
            new(25, "Consular-class Cruiser", IsCapital: false),
            new(35, "Venator",           IsCapital: true),
        };
        // ── Guilds ──────────────────────────────────────────────────────────
        FactionBlueprintPools["Guilds"] = new()
        {
            new(5,  "Slave I reactor blueprint",      IsPart: true),
            new(5,  "Slave I shield generator blueprint", IsPart: true),
            new(10, "Slave I concussion missile blueprint", IsPart: true),
            new(12, "Slave I",           IsCapital: false),
            new(14, "Upsilon-class shuttle", IsCapital: false),
            new(18, "ARC-170",           IsCapital: false),
            new(25, "Republic Gunship",  IsCapital: false),
            new(30, "Corellian Corvette", IsCapital: true),
        };
        // ── Sith ────────────────────────────────────────────────────────────
        FactionBlueprintPools["Sith"] = new()
        {
            new(5,  "sith saber hilt blueprint",  IsPart: true),
            new(8,  "red kyber crystal blueprint", IsPart: true),
            new(12, "TIE Silencer",      IsCapital: false),
            new(18, "TIE Advanced x1",  IsCapital: false),
            new(30, "Imperial Star Destroyer", IsCapital: true),
        };
    }

    /// <summary>Minimum standing required to receive quests from a faction. Below this: locked out.</summary>
    public const int FactionQuestLockThreshold = -10;
    /// <summary>Minimum standing for faction-exclusive blueprint quests (parts).</summary>
    public const int FactionPartBpMinStanding = 5;

    public string GetFactionQuestAccessStatus(string faction)
    {
        var standing = FactionStandings.GetValueOrDefault(faction);
        if (standing <= FactionQuestLockThreshold)
            return $"LOCKED — standing {standing} (need > {FactionQuestLockThreshold} to receive quests)";
        if (standing < 0)
            return $"Cautious — standing {standing} (neutral/generic quests only)";
        var nextBp = FactionBlueprintPools.GetValueOrDefault(faction)?
            .Where(e => e.MinStanding > standing).OrderBy(e => e.MinStanding).FirstOrDefault();
        var nextStr = nextBp is null ? "all blueprints unlocked" : $"next unlock at standing {nextBp.MinStanding}: {nextBp.Blueprint}";
        return $"Trusted — standing {standing} | {nextStr}";
    }

    /// <summary>Picks a random locked crafting blueprint matching the tier (1=basic items, 2=weapons/armor, 3=vehicles/ship parts).</summary>
    private string PickGenericCraftingBlueprint(GameCharacter character, int tier)
    {
        var candidates = Recipes
            .Where(kv => kv.Value.HideUntilBlueprintUnlocked && !KnowsBlueprint(character, kv.Key))
            .Select(kv => kv.Key)
            .Where(name =>
            {
                bool isShip    = ShipCatalog.ContainsKey(name);
                bool isVehicle = Vehicles.ContainsKey(name);
                bool isArmament= ShipArmaments.ContainsKey(name);
                return tier switch
                {
                    1 => !isShip && !isVehicle && !isArmament && !Weapons.ContainsKey(name),
                    2 => !isShip && !isVehicle,
                    _ => true   // tier 3: anything including ships
                };
            })
            .ToList();
        if (candidates.Count == 0) return string.Empty;
        return candidates[random.Next(candidates.Count)];
    }

    /// <summary>Called after the character obtains new items; unlocks basic processing blueprints on first discovery.</summary>
    public IReadOnlyList<string> OnItemDiscovered(GameCharacter character, IEnumerable<string> items)
    {
        var unlocked = new List<string>();
        foreach (var item in items)
        {
            if (!character.DiscoveredItems.Add(item)) continue; // already known

            // Raw material first-find → unlock its refining/smelting blueprint.
            if (RefiningMap.TryGetValue(item, out var refinedOutput))
            {
                var msg = UnlockBlueprint(character, refinedOutput, $"discovered {item}");
                if (!string.IsNullOrWhiteSpace(msg))
                    unlocked.Add($"[Discovery] Learned to process {item} → {refinedOutput}. {msg}");
            }
        }
        return unlocked;
    }

    private string PickFactionBlueprintReward(string faction, int difficulty, GameCharacter character)
    {
        var standing = FactionStandings.GetValueOrDefault(faction);
        // Map difficulty to a rough standing tier
        int maxTier = difficulty switch
        {
            <= 2 => 8,
            <= 3 => 14,
            <= 4 => 20,
            _    => 30
        };
        if (!FactionBlueprintPools.TryGetValue(faction, out var pool)) return string.Empty;
        var eligible = pool
            .Where(e => e.MinStanding <= standing && e.MinStanding <= maxTier && !KnowsBlueprint(character, e.Blueprint))
            .ToList();
        if (eligible.Count == 0) return string.Empty;
        // Weight toward part blueprints for lower difficulties
        var candidates = difficulty <= 3
            ? eligible.Where(e => e.IsPart).ToList()
            : eligible;
        if (candidates.Count == 0) candidates = eligible;
        return candidates[random.Next(candidates.Count)].Blueprint;
    }

    /// <summary>Procedurally generate a quest chain for the given faction (3–5 steps).</summary>
    public QuestChain GenerateProceduralChain(string faction, GameCharacter character, string planetName)
    {
        if (!FactionBlueprintPools.ContainsKey(faction))
            faction = "Independent";

        var standing = FactionStandings.GetValueOrDefault(faction);
        var steps    = new List<Quest>();
        var chainId  = $"proc-{faction.ToLower()}-{DateTime.Now.Ticks}";

        // ── Faction flavour tables ─────────────────────────────────────────
        var (npcNames, zones, descs) = GetFactionFlavour(faction);
        int stepCount = standing >= 20 ? 5 : standing >= 10 ? 4 : 3;

        // Destination planets for travel steps
        var destPlanets = Planets.Keys
            .Where(p => !string.Equals(p, planetName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(_ => random.Next()).ToList();

        // Step objective cycling: fetch → combat → travel (repeat/extend for longer chains)
        var objCycle = new[] { "fetch", "combat", "travel", "combat", "fetch" };

        for (int i = 0; i < stepCount; i++)
        {
            int stepNum   = i + 1;
            var objType   = objCycle[i];
            var npc       = npcNames[random.Next(npcNames.Length)];
            var planet    = objType == "travel" && destPlanets.Count > 0
                            ? destPlanets[i % destPlanets.Count]
                            : planetName;
            var zone      = zones[random.Next(zones.Length)];
            int difficulty = stepNum + (standing >= 10 ? 1 : 0);

            // Blueprint reward: escalate across chain
            int bpTierCap = stepNum switch { 1 => 8, 2 => 14, 3 => 20, 4 => 28, _ => 40 };
            string bp = "";
            if (FactionBlueprintPools.TryGetValue(faction, out var pool))
            {
                var eligible = pool
                    .Where(e => e.MinStanding <= standing && e.MinStanding <= bpTierCap && !KnowsBlueprint(character, e.Blueprint))
                    .ToList();
                // Later steps can award ship/capital blueprints
                if (stepNum >= 3) eligible = eligible.Where(e => !e.IsPart || stepNum == 3).ToList();
                if (stepNum == stepCount) eligible = pool
                    .Where(e => e.MinStanding <= standing && !KnowsBlueprint(character, e.Blueprint) && (e.IsCapital || !e.IsPart))
                    .ToList();
                if (eligible.Count > 0)
                    bp = eligible[eligible.Count > 1 ? random.Next(eligible.Count) : 0].Blueprint;
            }

            int credits = 80 + stepNum * difficulty * random.Next(20, 35);
            int xp      = 20 + stepNum * difficulty * random.Next(5, 10);

            string objTarget = "";
            int objReq = 1;
            string desc = "";

            if (objType == "fetch")
            {
                objTarget = PickFetchTarget(faction, planetName);
                objReq    = Math.Min(stepNum + 1, 4);
                desc = $"{npc} requires {objReq}x {objTarget}. {descs[0]}";
            }
            else if (objType == "combat")
            {
                objTarget = PickCombatTarget(faction);
                objReq    = Math.Min(2 + stepNum, 5);
                desc = $"{npc}: Eliminate {objReq} {objTarget} in the {zone}. {descs[1]}";
            }
            else
            {
                desc = $"{npc} needs a contact on {planet}. {descs[2]}";
            }

            var suffix  = stepNum == stepCount ? " [FINAL]" : "";
            var step = new Quest
            {
                Id             = $"{chainId}-step{stepNum}",
                ChainId        = chainId,
                ChainStep      = stepNum,
                ChainNextStep  = stepNum < stepCount ? $"{chainId}-step{stepNum + 1}" : "",
                Title          = GenerateChainTitle(faction, stepNum, stepCount) + suffix,
                Description    = desc,
                Faction        = faction,
                ObjectiveType  = objType,
                ObjectiveTarget = objTarget,
                ObjectiveZone  = zone,
                ObjectiveRequired = objReq,
                TargetPlanet   = planet,
                RewardCredits  = credits,
                RewardXp       = xp,
                RewardBlueprint = bp,
                IssuerName     = npc,
                IssuerSpecies  = PickFactionSpecies(faction),
                IssuerPlanet   = planetName,
                Status         = stepNum == 1 ? "active" : "inactive",
            };
            steps.Add(step);
        }

        var chainTitle = GenerateChainTitle(faction, 0, 0); // chain-level title
        var chain = new QuestChain { Id = chainId, Title = chainTitle, Faction = faction, Steps = steps };
        StorylineChains[chainId] = chain;
        // Add first step to active quests immediately
        var first = steps[0];
        if (!ActiveQuests.Any(q => q.Id == first.Id))
            ActiveQuests.Add(first);
        // Queue remaining steps as inactive
        foreach (var s in steps.Skip(1))
            if (!ActiveQuests.Any(q => q.Id == s.Id))
                ActiveQuests.Add(s);

        AdjustFactionStanding(faction, 1);
        return chain;
    }

    private string GenerateChainTitle(string faction, int step, int total)
    {
        var factionTitles = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Rebels"]       = new[] { "Operation: Bright Dawn", "Shadow Network", "Rebel Cause", "Freedom's Price", "Spark Ignition" },
            ["Empire"]       = new[] { "Imperial Directive", "Order's Reach", "Iron Protocol", "Subjugation Vector", "Peacekeeping Mandate" },
            ["Smugglers"]    = new[] { "The Quiet Run", "Contraband Relay", "Gray Market", "The Clean Slate", "Dead Drop" },
            ["Mandalorians"] = new[] { "This is the Way", "Clan's Honor", "The Iron Covert", "Beskar Proven", "Forge & Fire" },
            ["Jedi"]         = new[] { "Echoes in the Force", "Balance Restored", "The Living Force", "Serenity Protocol", "Light of the Order" },
            ["Guilds"]       = new[] { "The Bounty Circuit", "Target Acquired", "Collector's Run", "Iron Curtain", "Payment in Full" },
            ["Sith"]         = new[] { "Power's Shadow", "The Dark Chain", "Corrupted Signal", "Breaking Point", "Abyss Protocol" },
        };
        var pool = factionTitles.GetValueOrDefault(faction) ?? new[] { "Unknown Contract", "Gray Signal", "Dark Run" };
        return pool[random.Next(pool.Length)];
    }

    private (string[] NpcNames, string[] Zones, string[] DescTemplates) GetFactionFlavour(string faction)
    {
        return faction switch
        {
            "Rebels"  => (
                new[] { "Commander Vela", "Lt. Kira Stenn", "Corporal Desh", "Agent Mira Donn", "Sergeant Aryn" },
                new[] { "rebel base", "transport hangar", "forest outpost", "mountain pass", "ruins" },
                new[] { "The Alliance needs this desperately.", "Strike fast — time is against us.", "Get to the rendezvous." }),
            "Empire"  => (
                new[] { "Moff Dravek", "Lieutenant Vaas", "Commander Rohl", "ISB Agent Crell", "Captain Merrik" },
                new[] { "garrison", "outpost", "dock quarter", "industrial zone", "checkpoint" },
                new[] { "This is an Imperial order.", "Non-compliance is treason.", "The Empire requires efficiency." }),
            "Smugglers" => (
                new[] { "Greev", "Lady Mira", "Cutter", "Parro Sleen", "Zix the Quick" },
                new[] { "black market", "back alley", "smuggler den", "abandoned dock", "spice run" },
                new[] { "No questions asked.", "Keep it quiet.", "The Hutts don't need to know." }),
            "Mandalorians" => (
                new[] { "Alor Kaden", "Verd Raan", "Dar'yaim", "Bes'uliik", "Ori'ramikad" },
                new[] { "forge", "proving grounds", "clan hall", "battlefield", "armory" },
                new[] { "Prove your valor.", "The clan watches.", "Strength earns trust." }),
            "Jedi" => (
                new[] { "Master Elrin", "Padawan Sola", "Knight Tessek", "Archivist Ven", "Guardian Cayne" },
                new[] { "temple ruins", "sacred cave", "meditation grove", "ancient library", "force nexus" },
                new[] { "The Force guides you.", "Trust your instincts.", "Patience and strength." }),
            "Guilds" => (
                new[] { "The Broker", "Vizier Hox", "Contractor Nyr", "Handler Bel", "Acquisition Agent" },
                new[] { "cantina", "bounty board", "underground market", "freight yard", "secure vault" },
                new[] { "Payment on delivery.", "Don't miss.", "The contract is clear." }),
            _ => (
                new[] { "Contact", "Handler", "Broker", "Agent", "Liaison" },
                new[] { "market", "dock", "slums", "ruins", "wilderness" },
                new[] { "No further details.", "Complete and return.", "Standard contract terms." })
        };
    }

    private string PickFetchTarget(string faction, string planetName)
    {
        var factionItems = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Rebels"]       = new[] { "rebel intel chip", "encrypted data core", "supply crate", "medical stim", "comms relay module" },
            ["Empire"]       = new[] { "power cell module", "imperial data card", "equipment manifest", "fuel canister", "targeting array" },
            ["Smugglers"]    = new[] { "tibanna gas canister", "spice packet", "unmarked crate", "contraband module", "stolen part" },
            ["Mandalorians"] = new[] { "beskar ore", "armor plate", "forge fuel", "clan relic", "weapon cartridge" },
            ["Jedi"]         = new[] { "kyber crystal", "holocron shard", "ancient scroll", "force artifact", "meditation crystal" },
            ["Guilds"]       = new[] { "bounty token", "target dossier", "transponder chip", "credits chip", "evidence chip" },
        };
        var pool = factionItems.GetValueOrDefault(faction) ?? new[] { "supply crate", "repair kit", "data chip" };
        // Also sprinkle in planet materials
        if (PlanetRawMaterials.TryGetValue(planetName, out var mats) && mats.Count > 0 && random.NextDouble() < 0.3)
            return mats[random.Next(mats.Count)];
        return pool[random.Next(pool.Length)];
    }

    private string PickCombatTarget(string faction)
    {
        var targets = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Rebels"]       = new[] { "stormtrooper squad", "Imperial probe droid", "Imperial patrol", "ISB agent", "TIE escort" },
            ["Empire"]       = new[] { "rebel cell", "insurgent squad", "pirate crew", "dissident cell", "rebel spy" },
            ["Smugglers"]    = new[] { "Hutt enforcer", "rival smuggler", "customs officer", "hired merc", "rival crew" },
            ["Mandalorians"] = new[] { "enemy clan warrior", "Death Watch raider", "rival hunter", "warlord guard", "challenger" },
            ["Jedi"]         = new[] { "dark side agent", "Sith acolyte", "inquisitor scout", "fallen knight", "shadow operative" },
            ["Guilds"]       = new[] { "rival hunter", "target bodyguard", "protection detail", "rival contractor", "hostile informant" },
        };
        var pool = targets.GetValueOrDefault(faction) ?? new[] { "hostile", "raider", "enemy patrol" };
        return pool[random.Next(pool.Length)];
    }

    private string PickFactionSpecies(string faction)
    {
        var species = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Rebels"]       = new[] { "Human", "Mon Calamari", "Bothan", "Sullustan", "Twi'lek" },
            ["Empire"]       = new[] { "Human" },
            ["Smugglers"]    = new[] { "Rodian", "Twi'lek", "Human", "Zabrak", "Toydarian" },
            ["Mandalorians"] = new[] { "Mandalorian", "Human" },
            ["Jedi"]         = new[] { "Human", "Mirialan", "Togruta", "Nautolan", "Miraluka" },
            ["Guilds"]       = new[] { "Rodian", "Trandoshan", "Human", "Zabrak", "Bounty Hunter" },
        };
        var pool = species.GetValueOrDefault(faction) ?? new[] { "Human" };
        return pool[random.Next(pool.Length)];
    }
    private IReadOnlyList<string> GrantReputationBlueprintRewards(GameCharacter character, string faction)
    {
        var unlocked = new List<string>();
        var standing = FactionStandings.GetValueOrDefault(faction);
        if (!FactionBlueprintPools.TryGetValue(faction, out var pool)) return unlocked;
        foreach (var entry in pool.Where(e => e.MinStanding <= standing))
        {
            var msg = UnlockBlueprint(character, entry.Blueprint, $"{faction} rep {standing}");
            if (!string.IsNullOrWhiteSpace(msg)) unlocked.Add(msg);
        }
        return unlocked;
    }

    private void SeedStartingBlueprints(GameCharacter character)
    {
        foreach (var blueprint in new[]
        {
            "repair kit",
            "field medpack",
            "refined metal",
            "power cell",
            "sensor array",
            "hyperdrive part"
        })
        {
            UnlockBlueprint(character, blueprint, "starting knowledge");
        }
    }

    private void NormalizeCharacterProgressionState(GameCharacter? character)
    {
        if (character is null) return;

        character.Inventory ??= new List<string>();
        character.HangarInventory ??= new List<string>();
        character.KnownBlueprints ??= new List<string>();
        character.Crafting ??= new List<string>();
        SeedStartingBlueprints(character);

        if (character.Ship is not null)
        {
            EnsureShipSystemsInitialized(character.Ship);
            character.Ship.Weapon = BuildShipWeaponSummary(character.Ship);
        }
    }

    private List<string> RegisterCombatQuestProgress(GameCharacter character, string zone)
    {
        var updates = new List<string>();
        foreach (var quest in ActiveQuests.Where(q => q.Status == "active" && q.ObjectiveType.Equals("combat", StringComparison.OrdinalIgnoreCase)))
        {
            if (!string.IsNullOrWhiteSpace(quest.ObjectiveZone) && !string.Equals(quest.ObjectiveZone, zone, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var required = Math.Max(1, quest.ObjectiveRequired);
            quest.ObjectiveProgress = Math.Min(required, quest.ObjectiveProgress + 1);
            updates.Add($"Quest progress [{quest.Title}]: {quest.ObjectiveProgress}/{required}.");
            if (quest.ObjectiveProgress >= required)
            {
                quest.Status = "ready";
                updates.Add($"Quest ready to turn in: {quest.Title}.");
            }
        }

        return updates;
    }

    public void AdjustFactionStanding(string faction, int amount)
    {
        if (!FactionStandings.ContainsKey(faction))
            FactionStandings[faction] = 0;
        FactionStandings[faction] = Math.Clamp(FactionStandings[faction] + amount, -50, 50);

        // Rival factions lose standing proportionally when enemy gains
        if (amount > 0 && FactionRivalries.TryGetValue(faction, out var rivals))
        {
            foreach (var rival in rivals)
            {
                if (!FactionStandings.ContainsKey(rival)) FactionStandings[rival] = 0;
                FactionStandings[rival] = Math.Clamp(FactionStandings[rival] - Math.Max(1, amount / 2), -50, 50);
            }
        }
    }

    public string GetFactionSummary() => string.Join(Environment.NewLine, FactionStandings.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

    public string GenerateWorldEvent(string planetName, string timeOfDay)
    {
        var planet = Planets[planetName];
        var events = timeOfDay == "day" ? planet.DayEvents : planet.NightEvents;
        var eventName = events[random.Next(events.Count)];
        var reward = random.Next(8, 24);
        AdjustFactionStanding("Smugglers", 1);
        var eco = GetPlanetEconomyStatus(planetName);
        var econNote = eco.ImperialExtraction
            ? "Imperial crews are stripping local resources."
            : eco.ResourceLevel <= 25
                ? "Merchants report major raw material shortages."
                : eco.ResourceLevel >= 75
                    ? "Supply lanes are healthy and markets are flush."
                    : "Local trade remains uneven.";
        return $"A {eventName} unfolds on {planetName}. You gain {reward} credits and a fresh rumor. {econNote}";
    }

    public EncounterResult GenerateEncounter(string planetName, string zone, GameCharacter? character = null, string? encounterType = null)
    {
        // Space stations are not in the Planets dict — resolve to orbiting planet or a generic fallback
        if (!Planets.ContainsKey(planetName))
        {
            if (SpaceStations.TryGetValue(planetName, out var st) && Planets.ContainsKey(st.OrbitingPlanet))
                planetName = st.OrbitingPlanet;
            else
                planetName = Planets.Keys.FirstOrDefault(k => k.Equals("Corellia", StringComparison.OrdinalIgnoreCase)) ?? Planets.Keys.First();
        }

        var planet = Planets[planetName];

        // Encounter cooldown — 3 rotations between credit-bearing encounters (combat bypass allowed)
        const int encounterCooldown = 3;
        if (character is not null && encounterType != "attack"
            && Clock.Rotation - character.LastEncounterRotation < encounterCooldown)
        {
            var wait = encounterCooldown - (Clock.Rotation - character.LastEncounterRotation);
            return new EncounterResult
            {
                Summary = $"Still too soon — locals haven't forgotten you. Come back in {wait} rotation(s).",
                Dialogue = "The contact shakes their head. \"Give it some time.\"",
                Outcome = "No reward.",
                RewardCredits = 0, IsHostile = false, ThreatLevel = planet.ThreatLevel,
                Zone = zone, NpcName = "", NpcSpecies = "", SpeciesRelation = 0
            };
        }
        if (character is not null && encounterType != "attack")
            character.LastEncounterRotation = Clock.Rotation;

        var resolvedType = encounterType ?? zone switch
        {
            "slums" => "attack",
            "dock" => "merchant",
            "market" => "merchant",
            "ruins" => "scavenger",
            _ => "contact"
        };

        // If this planet has a dockyard, there's a chance of pirate activity
        if (zone == "dock" && planet.HasDockyard && encounterType is null)
        {
            var roll = random.NextDouble();
            if (roll < 0.25) resolvedType = "attack"; // pirate raid
            else if (roll < 0.6) resolvedType = "merchant";
            else resolvedType = "contact";
        }

        var npcSpecies = SelectEncounterNpcSpecies(planetName, zone, character?.Species);
        var npcName = GenerateNameForRace(npcSpecies);
        var speciesRelation = character is null ? 0 : GetInterspeciesRelation(character.Species, npcSpecies);

        var reward = 12 + random.Next(1, 21);
        var threatLevel = planet.ThreatLevel;
        if (speciesRelation >= 4) reward += 6;
        if (speciesRelation <= -4) reward = Math.Max(5, reward - 4);
        if (planetName == "Corellia" && zone == "slums")
        {
            threatLevel = "High";
            reward = Math.Min(35, reward + 8);
        }

        if (encounterType is null && speciesRelation <= -6 && resolvedType != "merchant")
        {
            resolvedType = "attack";
        }

        var dialogue = ComposeEncounterDialogue(planetName, zone, resolvedType, npcName, npcSpecies, speciesRelation, character?.Reputation ?? 0);

        var result = new EncounterResult
        {
            Summary = $"Encounter in the {zone} of {planetName}",
            Dialogue = dialogue,
            Outcome = resolvedType == "attack" ? "The scene becomes hostile." : "The encounter ends in profit or rumor.",
            RewardCredits = reward,
            IsHostile = resolvedType == "attack",
            ThreatLevel = threatLevel,
            Zone = zone,
            NpcName = npcName,
            NpcSpecies = npcSpecies,
            SpeciesRelation = speciesRelation
        };

        if (character is not null)
        {
            if (result.IsHostile)
            {
                if (character.Reputation < -10 || speciesRelation <= -6)
                {
                    character.Credits = Math.Max(0, character.Credits - 5);
                    character.Reputation -= 2;
                    result.Outcome = "Your reputation and species tensions turn a skirmish into a costly ambush.";
                }
                else
                {
                    character.Credits += reward;
                    character.Reputation += 2;
                    result.Outcome = "You survive the skirmish and collect a reward.";
                }

                ShiftCharacterSpeciesRelation(character, npcSpecies, -1);
            }
            else
            {
                character.Credits += reward;
                character.Reputation += speciesRelation >= 3 ? 2 : 1;
                ShiftCharacterSpeciesRelation(character, npcSpecies, speciesRelation >= 2 ? 1 : 0);
            }
        }

        return result;
    }

    public CombatEncounter CreateCombatEncounter(GameCharacter character, string planetName, string zone)
    {
        var planet = Planets[planetName];
        var enemyName = zone switch
        {
            "slums" => "Gang enforcer",
            "dock" => "Pirate cutthroat",
            "ruins" => "Ruin scavenger",
            _ => "Hostile mercenary"
        };

        // Economy and region pressure change which hostile archetypes appear.
        var eco = (planet.Economy + " " + planet.Region + " " + planet.Sector).ToLowerInvariant();
        if (eco.Contains("smuggl") || eco.Contains("spice")) enemyName = zone == "dock" ? "Smuggler corsair" : "Syndicate enforcer";
        else if (eco.Contains("mining") || eco.Contains("refinery") || eco.Contains("industry")) enemyName = "Mine raider";
        else if (eco.Contains("finance") || eco.Contains("luxury") || eco.Contains("diplom")) enemyName = "Corporate hit squad";
        else if (eco.Contains("agric") || eco.Contains("farming")) enemyName = "Highway marauder";

        // Wildlife-heavy and high-threat areas can spawn creatures from the imported creature catalog.
        var canSpawnCreature = zone is "ruins" or "wilds" or "wilderness" or "forest" or "swamp" or "caves"
            || planet.Economy.Contains("hunting", StringComparison.OrdinalIgnoreCase)
            || planet.Economy.Contains("ecology", StringComparison.OrdinalIgnoreCase)
            || planet.ThreatLevel is "High" or "Very High";
        var guaranteedCreature = zone is "wilds" or "wilderness" or "forest" or "swamp" or "caves";
        if (canSpawnCreature && Creatures.Count > 0 && (guaranteedCreature || random.NextDouble() < 0.55))
        {
            var creature = SelectCreatureForPlanet(planetName, guaranteedCreature);
            if (creature is not null)
            {
                enemyName = creature.Name;
            }
        }
        else
        {
            var species = SelectEncounterNpcSpecies(planetName, zone, character.Species);
            enemyName = $"{species} raider";
        }

        var enemyHp = 16 + (planet.ThreatLevel == "High" ? 12 : 6) + character.Reputation / 3;
        var economyPressure = eco.Contains("smuggl") || eco.Contains("spice") || eco.Contains("mining") ? 3 : eco.Contains("industry") || eco.Contains("refinery") ? 2 : 0;
        enemyHp += economyPressure;
        if (Creatures.TryGetValue(enemyName, out var creatureData))
        {
            enemyHp += CreatureHpBonus(creatureData.SizeClass) + creatureData.DangerRating;
        }

        var rewardCredits = 20 + random.Next(8, 28) + (Creatures.ContainsKey(enemyName) ? 12 : 0) + economyPressure * 2;
        var rewardXp = 10 + random.Next(3, 10) + (Creatures.ContainsKey(enemyName) ? 5 : 0);
        var rewardItem = PickCombatRewardItem(planetName, enemyName);

        var encounter = new CombatEncounter
        {
            Title = $"Combat on {planetName}",
            PlanetName = planetName,
            Zone = zone,
            EnemyName = enemyName,
            EnemyHp = enemyHp,
            EnemyMaxHp = enemyHp,
            EnemyArmor = (planet.ThreatLevel == "High" ? 3 : 1) + (Creatures.TryGetValue(enemyName, out var c) ? CreatureArmorBonus(c.SizeClass) : 0),
            EnemyDamage = 6 + (planet.ThreatLevel == "Very High" ? 4 : 2) + economyPressure + (Creatures.TryGetValue(enemyName, out var c2) ? CreatureDamageBonus(c2.SizeClass, c2.DangerRating) : 0),
            PlayerHp = character.Hp,
            PlayerMaxHp = character.MaxHp,
            RewardCredits = rewardCredits,
            RewardXp = rewardXp,
            RewardItem = rewardItem,
            Log = new List<string> { $"A {enemyName} blocks your path in the {zone}." }
        };

        return encounter;
    }

    private CreatureData? SelectCreatureForPlanet(string planetName, bool forceWilderness)
    {
        if (Creatures.Count == 0) return null;
        if (PlanetCreatureTerritories.TryGetValue(planetName, out var territory) && territory.Count > 0)
        {
            var available = territory.Where(name => Creatures.ContainsKey(name)).ToList();
            if (available.Count > 0)
            {
                var index = creatureEncounterCursor % available.Count;
                creatureEncounterCursor = (creatureEncounterCursor + 1) % Math.Max(1, Creatures.Count * 6);
                var mapped = Creatures[available[index]];
                if (!forceWilderness || !mapped.SizeClass.Equals("tiny", StringComparison.OrdinalIgnoreCase))
                {
                    return mapped;
                }
            }
        }

        var lower = planetName.ToLowerInvariant();

        var habitatHint = lower switch
        {
            var p when p.Contains("hoth") => "ice",
            var p when p.Contains("tatooine") || p.Contains("jakku") || p.Contains("geonosis") => "desert",
            var p when p.Contains("dagobah") || p.Contains("endor") || p.Contains("kashyyyk") => "jungle",
            var p when p.Contains("bespin") => "atmosphere",
            var p when p.Contains("mustafar") => "volcanic",
            _ => "general"
        };

        var matching = Creatures.Values.Where(c =>
            c.Habitat.Contains(habitatHint, StringComparison.OrdinalIgnoreCase)
            || c.Habitat.Equals("general", StringComparison.OrdinalIgnoreCase)).ToList();
        if (matching.Count == 0) matching = Creatures.Values.ToList();

        // Rare chance to roll colossal monsters like Sarlacc/Maw entities.
        var colossal = matching.Where(c => c.SizeClass.Equals("colossal", StringComparison.OrdinalIgnoreCase)).ToList();
        if (colossal.Count > 0 && random.NextDouble() < (forceWilderness ? 0.15 : 0.08))
        {
            return colossal[random.Next(colossal.Count)];
        }

        var ordered = matching.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToList();
        var baseIndex = Math.Abs(StringComparer.OrdinalIgnoreCase.GetHashCode($"{planetName}|{Clock.Rotation}|{clockSeed()}") );
        var selectedIndex = (baseIndex + creatureEncounterCursor) % ordered.Count;
        creatureEncounterCursor = (creatureEncounterCursor + 1) % Math.Max(1, Creatures.Count * 4);
        return ordered[selectedIndex];

        int clockSeed() => PlanetClocks.TryGetValue(planetName, out var wc) ? wc.Rotation + wc.Hour : Clock.Rotation + Clock.Hour;
    }

    public IReadOnlyList<string> GetCreatureTerritoryForPlanet(string planetName)
        => PlanetCreatureTerritories.TryGetValue(planetName, out var list) ? list : Array.Empty<string>();

    private static int CreatureHpBonus(string sizeClass) => sizeClass.ToLowerInvariant() switch
    {
        "tiny" => -2,
        "small" => 0,
        "medium" => 4,
        "large" => 10,
        "huge" => 18,
        "colossal" => 28,
        _ => 3
    };

    private static int CreatureArmorBonus(string sizeClass) => sizeClass.ToLowerInvariant() switch
    {
        "tiny" => 0,
        "small" => 0,
        "medium" => 1,
        "large" => 2,
        "huge" => 3,
        "colossal" => 4,
        _ => 1
    };

    private static int CreatureDamageBonus(string sizeClass, int danger) => sizeClass.ToLowerInvariant() switch
    {
        "tiny" => Math.Max(0, danger / 4),
        "small" => Math.Max(1, danger / 3),
        "medium" => Math.Max(1, danger / 2),
        "large" => Math.Max(2, danger / 2 + 1),
        "huge" => Math.Max(3, danger / 2 + 2),
        "colossal" => Math.Max(4, danger / 2 + 4),
        _ => Math.Max(1, danger / 3)
    };

    public string ResolveCombatAction(GameCharacter character, CombatEncounter encounter, string action, string? itemName = null)
    {
        if (encounter.IsOver)
            return "Combat has already ended.";

        encounter.Round++;
        var lower = action.ToLowerInvariant();

        // ── Tick status effects ───────────────────────────────────────────────
        if (encounter.EnemyStunned > 0) encounter.EnemyStunned--;
        if (encounter.BleedDamage > 0)
        {
            encounter.EnemyHp = Math.Max(0, encounter.EnemyHp - encounter.BleedDamage);
            encounter.Log.Add($"The enemy bleeds for {encounter.BleedDamage} damage.");
        }
        encounter.PlayerDefenseBonus = 0; // reset each round

        // ── Player action ─────────────────────────────────────────────────────
        int playerDamage = 0;
        bool skipEnemyTurn = false;

        switch (lower)
        {
            // General actions
            case "attack":
            {
                var base_dmg = 6 + character.Stats["strength"];
                var weaponBonus = character.EquippedWeapon?.ToLowerInvariant() switch
                {
                    string w when w.Contains("lightsaber") => character.Stats["presence"] + 4,
                    string w when w.Contains("rifle")      => character.Stats["agility"] + 3,
                    string w when w.Contains("sword")      => character.Stats["strength"] + 2,
                    string w when w.Contains("vibro")      => character.Stats["strength"] + 2,
                    _ => 1
                };
                var modBonus = GetWeaponModDamageBonus(character, character.EquippedWeapon ?? "");
                playerDamage = base_dmg + weaponBonus + modBonus + random.Next(0, 5);
                encounter.Log.Add($"You strike for {playerDamage} damage." + (modBonus > 0 ? $" [+{modBonus} mods]" : ""));
                break;
            }
            case "heavy strike":
            {
                playerDamage = 10 + character.Stats["strength"] * 2 + random.Next(0, 6);
                character.Stamina = Math.Max(0, character.Stamina - 4);
                encounter.Log.Add($"Heavy strike! {playerDamage} damage. (-4 stamina)");
                break;
            }
            case "aimed shot":
            {
                playerDamage = 8 + character.Stats["agility"] * 2 + random.Next(2, 8);
                character.Stamina = Math.Max(0, character.Stamina - 2);
                encounter.Log.Add($"Aimed shot lands for {playerDamage} damage.");
                break;
            }
            case "disarm":
            {
                var success = random.NextDouble() < 0.45 + character.Stats["agility"] * 0.05;
                if (success)
                {
                    encounter.EnemyDisarmed = true;
                    encounter.EnemyDamage   = Math.Max(1, encounter.EnemyDamage / 2);
                    encounter.Log.Add("You disarm the enemy! Their damage is halved.");
                }
                else encounter.Log.Add("Disarm attempt failed.");
                playerDamage = 2;
                break;
            }
            case "guard" or "defend":
            {
                encounter.PlayerDefenseBonus = 5 + character.Stats["vitality"];
                encounter.Log.Add($"You take a defensive stance. (+{encounter.PlayerDefenseBonus} armor this round)");
                playerDamage = 0;
                break;
            }
            case "taunt":
            {
                // Force enemy to deal -20% this round, player gets small advantage next
                encounter.EnemyStunned = Math.Max(encounter.EnemyStunned, 1);
                encounter.Log.Add("You taunt the enemy — they lose their cool.");
                playerDamage = 0;
                break;
            }
            case "use item" when itemName is not null:
            {
                if (character.Inventory.Any(x => x.Equals(itemName, StringComparison.OrdinalIgnoreCase)))
                {
                    character.Inventory.Remove(character.Inventory.First(x =>
                        x.Equals(itemName, StringComparison.OrdinalIgnoreCase)));
                    var itemLower = itemName.ToLowerInvariant();
                    if (itemLower.Contains("medpack") || itemLower.Contains("stim"))
                    {
                        var heal = 14 + character.Stats["vitality"] * 2;
                        character.Hp = Math.Min(character.MaxHp, character.Hp + heal);
                        encounter.PlayerHp = character.Hp;
                        encounter.Log.Add($"You use {itemName} and recover {heal} HP.");
                    }
                    else if (itemLower.Contains("grenade"))
                    {
                        playerDamage = 18 + random.Next(0, 10);
                        encounter.Log.Add($"Grenade blast! {playerDamage} damage.");
                    }
                    else
                    {
                        encounter.Log.Add($"You use {itemName}.");
                    }
                }
                else encounter.Log.Add("That item is not in your inventory.");
                break;
            }
            case "flee":
            {
                encounter.IsOver     = true;
                encounter.PlayerWon  = false;
                character.Reputation = Math.Max(-50, character.Reputation - 2);
                encounter.Log.Add("You break away and retreat. Your reputation takes a small hit.");
                return string.Join(Environment.NewLine, encounter.Log);
            }

            // ── Weapon abilities ──────────────────────────────────────────────
            case var weapAbilAction when WeaponAbilities.TryGetValue(weapAbilAction, out var wAbility):
            {
                if (character.Stamina < wAbility.StaminaCost)
                {
                    encounter.Log.Add($"Not enough stamina to use {wAbility.Name} (need {wAbility.StaminaCost}, have {character.Stamina}).");
                    break;
                }
                character.Stamina = Math.Max(0, character.Stamina - wAbility.StaminaCost);

                var weapDmgBase = 6 + character.Stats.GetValueOrDefault("strength", 1) + character.Stats.GetValueOrDefault("agility", 1);
                if (Weapons.TryGetValue(character.EquippedWeapon, out var wBp)) weapDmgBase += wBp.Damage;
                weapDmgBase += GetWeaponModDamageBonus(character, character.EquippedWeapon ?? "");

                if (wAbility.IsMultiHit)
                {
                    var totalDmg = 0;
                    var hitLog = new List<string>();
                    for (int h = 0; h < wAbility.HitCount; h++)
                    {
                        var hit = Math.Max(1, (int)(weapDmgBase * wAbility.DamageMultPercent / 100.0) + wAbility.FlatDamageBonus + random.Next(0, 5));
                        if (!wAbility.ArmorPiercing)
                        {
                            var eff = Math.Max(0, encounter.EnemyArmor - (encounter.EnemyDisarmed ? 2 : 0));
                            hit = Math.Max(1, hit - eff);
                        }
                        totalDmg += hit;
                        hitLog.Add(hit.ToString());
                    }
                    playerDamage = totalDmg;
                    encounter.Log.Add($"{wAbility.Name}: {wAbility.HitCount} hits — {string.Join(" + ", hitLog)} = {totalDmg} total damage. {wAbility.Description}");
                }
                else
                {
                    playerDamage = Math.Max(1, (int)(weapDmgBase * wAbility.DamageMultPercent / 100.0) + wAbility.FlatDamageBonus + random.Next(0, 8));
                    if (wAbility.ArmorPiercing)
                    {
                        encounter.Log.Add($"{wAbility.Name}: {playerDamage} damage (armor bypassed). {wAbility.Description}");
                        // Skip normal armor reduction later — we still want the enemy hit block to run cleanly
                        var directHit = Math.Max(1, playerDamage);
                        encounter.EnemyHp = Math.Max(0, encounter.EnemyHp - directHit);
                        playerDamage = 0; // already applied
                    }
                    else
                        encounter.Log.Add($"{wAbility.Name}: {playerDamage} damage. {wAbility.Description}");
                }

                if (wAbility.CanStun && random.NextDouble() < 0.55)
                {
                    encounter.EnemyStunned = Math.Max(encounter.EnemyStunned, 1 + (wAbility.IsSuppressive ? 1 : 0));
                    skipEnemyTurn = encounter.EnemyStunned > 0;
                    encounter.Log.Add($"Enemy stunned for {encounter.EnemyStunned} round(s).");
                }
                else if (wAbility.IsSuppressive)
                {
                    encounter.EnemyStunned = Math.Max(encounter.EnemyStunned, 2);
                    skipEnemyTurn = true;
                    encounter.Log.Add("Suppressive fire forces the enemy into cover — they skip their next turn.");
                }

                // Deflect special: redirect some damage back
                if (wAbility.Name == "Deflect")
                {
                    var redirected = 5 + character.Stats.GetValueOrDefault("presence", 1) * 2;
                    encounter.EnemyHp = Math.Max(0, encounter.EnemyHp - redirected);
                    encounter.Log.Add($"You deflect incoming blaster bolts, redirecting {redirected} damage back at the attacker.");
                    skipEnemyTurn = true;
                    playerDamage = 0;
                }
                break;
            }
            case "force push":
            {
                if (!CheckForceAbility(character, "Force Push", encounter)) break;
                playerDamage = 8 + character.Stats["presence"] * 2;
                encounter.EnemyStunned = Math.Max(encounter.EnemyStunned, 1);
                encounter.Log.Add($"Force Push slams the enemy for {playerDamage} damage and stuns them.");
                skipEnemyTurn = true; // stunned enemies skip attack
                break;
            }
            case "force pull":
            {
                if (!CheckForceAbility(character, "Force Pull", encounter)) break;
                encounter.EnemyDisarmed = true;
                encounter.EnemyDamage   = Math.Max(1, encounter.EnemyDamage / 2);
                encounter.Log.Add("Force Pull strips their weapon. Enemy damage halved.");
                playerDamage = 0;
                break;
            }
            case "mind trick":
            {
                if (!CheckForceAbility(character, "Mind Trick", encounter)) break;
                var chance = 0.55 + character.Stats["presence"] * 0.06;
                if (random.NextDouble() < chance)
                {
                    encounter.EnemyStunned = 2 + random.Next(0, 2);
                    skipEnemyTurn = true;
                    encounter.Log.Add($"Mind Trick takes hold — enemy stunned for {encounter.EnemyStunned} round(s).");
                }
                else encounter.Log.Add("Mind Trick fails — the enemy resists.");
                playerDamage = 0;
                break;
            }
            case "force speed":
            {
                if (!CheckForceAbility(character, "Force Speed", encounter)) break;
                // Double attack
                var d1 = 5 + character.Stats["agility"] + random.Next(0, 4);
                var d2 = 5 + character.Stats["agility"] + random.Next(0, 4);
                playerDamage = d1 + d2;
                encounter.Log.Add($"Force Speed — two rapid strikes: {d1} + {d2} = {playerDamage} damage.");
                break;
            }
            case "force leap":
            {
                if (!CheckForceAbility(character, "Force Leap", encounter)) break;
                playerDamage = 14 + character.Stats["strength"] + character.Stats["agility"] + random.Next(0, 6);
                encounter.Log.Add($"Force Leap — devastating overhead blow for {playerDamage} damage.");
                break;
            }
            case "saber throw":
            {
                if (!CheckForceAbility(character, "Saber Throw", encounter)) break;
                playerDamage = 10 + character.Stats["presence"] * 2 + random.Next(0, 8);
                encounter.BleedDamage = 3; // spinning saber causes bleed
                encounter.Log.Add($"Saber Throw strikes for {playerDamage} damage and causes bleeding (3/round).");
                break;
            }
            case "force barrier":
            {
                if (!CheckForceAbility(character, "Force Barrier", encounter)) break;
                encounter.PlayerDefenseBonus = 10 + character.Stats["presence"] * 2;
                encounter.Log.Add($"Force Barrier surrounds you. (+{encounter.PlayerDefenseBonus} armor this round)");
                playerDamage = 0;
                break;
            }
            case "force storm":
            {
                if (!CheckForceAbility(character, "Force Storm", encounter)) break;
                playerDamage = 22 + character.Stats["presence"] * 3 + random.Next(0, 12);
                encounter.EnemyStunned = 1;
                encounter.BleedDamage  = 5;
                character.ForcePoints = Math.Max(0, character.ForcePoints - 6);
                encounter.Log.Add($"Force Storm erupts! {playerDamage} damage, bleed 5/round, enemy stunned.");
                break;
            }
            case "battle meditation":
            {
                if (!CheckForceAbility(character, "Battle Meditation", encounter)) break;
                var heal = 8 + character.Stats["presence"];
                character.Hp = Math.Min(character.MaxHp, character.Hp + heal);
                encounter.PlayerHp = character.Hp;
                encounter.PlayerDefenseBonus = 6;
                character.ForcePoints = Math.Max(0, character.ForcePoints - 4);
                encounter.Log.Add($"Battle Meditation — healed {heal} HP and boosted defence.");
                playerDamage = 0;
                break;
            }
            case "force drain":
            {
                if (!CheckForceAbility(character, "Force Drain", encounter)) break;
                playerDamage = 12 + character.Stats["presence"] * 2;
                var drain    = Math.Min(playerDamage / 3, encounter.EnemyHp);
                character.Hp = Math.Min(character.MaxHp, character.Hp + drain);
                encounter.PlayerHp = character.Hp;
                character.ForcePoints = Math.Max(0, character.ForcePoints - 5);
                encounter.Log.Add($"Force Drain — {playerDamage} damage, absorbed {drain} HP.");
                break;
            }
            case "force sense":
            {
                if (!CheckForceAbility(character, "Force Sense", encounter)) break;
                // Reveal enemy stats
                encounter.Log.Add($"Force Sense: enemy HP {encounter.EnemyHp}/{encounter.EnemyMaxHp}, armor {encounter.EnemyArmor}, damage {encounter.EnemyDamage}.");
                playerDamage = 0;
                break;
            }
            default:
            {
                encounter.Log.Add($"Unknown action: {action}.");
                break;
            }
        }

        // Apply player damage
        if (playerDamage > 0)
        {
            var effectiveArmor = Math.Max(0, encounter.EnemyArmor - (encounter.EnemyDisarmed ? 2 : 0));
            var dealt = Math.Max(1, playerDamage - effectiveArmor);
            encounter.EnemyHp = Math.Max(0, encounter.EnemyHp - dealt);
        }

        // Check enemy death
        if (encounter.EnemyHp <= 0)
        {
            encounter.IsOver    = true;
            encounter.PlayerWon = true;
            GrantCombatRewards(character, encounter);
            character.Reputation  += 4;
            character.Experience  += encounter.RewardXp;
            if (character.IsForceUser)
            {
                var rankMsg = AwardJediXp(character, encounter.RewardXp / 2);
                if (rankMsg.Length > 0) encounter.Log.Add(rankMsg);
            }
            encounter.Log.Add("The enemy falls and the field is yours.");
            encounter.Log.Add($"Rewards: {encounter.RewardCredits} credits, {encounter.RewardXp} XP, {encounter.RewardItem}.");
            foreach (var questUpdate in RegisterCombatQuestProgress(character, encounter.Zone))
                encounter.Log.Add(questUpdate);
            return string.Join(Environment.NewLine, encounter.Log);
        }

        // ── Enemy counter-attack ──────────────────────────────────────────────
        if (!skipEnemyTurn && encounter.EnemyStunned == 0)
        {
            var baseEnemyDmg = Math.Max(1, encounter.EnemyDamage - (character.Armor + encounter.PlayerDefenseBonus) / 2);
            encounter.PlayerHp = Math.Max(0, encounter.PlayerHp - baseEnemyDmg);
            character.Hp       = encounter.PlayerHp;
            encounter.Log.Add($"The enemy hits you for {baseEnemyDmg} damage. (HP: {character.Hp}/{character.MaxHp})");
        }
        else if (encounter.EnemyStunned > 0)
        {
            encounter.Log.Add("The enemy is stunned and cannot act this round.");
        }

        // Check player death
        if (encounter.PlayerHp <= 0)
        {
            encounter.IsOver    = true;
            character.Hp        = 0;
            character.IsAlive   = false;
            character.Condition = "Critical";
            character.CurrentState = "Downed";
            encounter.Log.Add("You collapse from your wounds.");
        }

        encounter.PlayerHp  = Math.Max(0, encounter.PlayerHp);
        encounter.PlayerMaxHp = character.MaxHp;
        character.Hp        = encounter.PlayerHp;
        return string.Join(Environment.NewLine, encounter.Log);
    }

    private bool CheckForceAbility(GameCharacter character, string ability, CombatEncounter encounter)
    {
        if (!character.IsForceUser)
        { encounter.Log.Add("You are not attuned to the Force."); return false; }
        if (!character.ForceAbilities.Contains(ability))
        { encounter.Log.Add($"You have not learned {ability} yet."); return false; }
        var cost = ability switch
        {
            "Force Storm" or "Force Drain" or "Battle Meditation" => 5,
            "Force Speed" or "Force Leap" or "Saber Throw"        => 3,
            _ => 2
        };
        if (character.ForcePoints < cost)
        { encounter.Log.Add($"Not enough Force Points (need {cost}, have {character.ForcePoints})."); return false; }
        character.ForcePoints -= cost;
        return true;
    }

    public List<string> GetCombatActions(GameCharacter character)
    {
        var actions = new List<string> { "Attack", "Heavy Strike", "Aimed Shot", "Disarm", "Guard", "Taunt", "Use Item", "Flee" };

        // Add weapon-specific abilities
        var weaponAbils = GetWeaponAbilities(character);
        if (weaponAbils.Count > 0)
        {
            actions.Add("── Weapon ──");
            actions.AddRange(weaponAbils);
        }

        if (character.IsForceUser && character.ForceAbilities.Count > 0)
        {
            actions.Add("── Force ──");
            actions.AddRange(character.ForceAbilities);
        }
        return actions;
    }

    // ── Weapon ability helpers ────────────────────────────────────────────────

    /// <summary>Returns the WeaponSubtype for the named weapon, using name-based fallbacks.</summary>
    public string GetWeaponSubtype(string weaponName)
    {
        if (Weapons.TryGetValue(weaponName, out var w) && !string.IsNullOrEmpty(w.WeaponSubtype))
            return w.WeaponSubtype;
        var n = weaponName.ToLowerInvariant();
        if (n.Contains("shoto"))   return "shoto_lightsaber";
        if (n.Contains("lightsaber") || n.Contains("light saber")) return "lightsaber";
        if (n.Contains("sniper"))  return "sniper_rifle";
        if (n.Contains("rotary") || (n.Contains("cannon") && !n.Contains("laser"))) return "rotary_cannon";
        if (n.Contains("pistol") || n.Contains("hand blaster") || n.Contains("sidearm")) return "pistol";
        if (n.Contains("rifle") || n.Contains("repeating")) return "blaster_rifle";
        if (n.Contains("vibroblade") || n.Contains("vibro-blade")) return "vibroblade";
        if (n.Contains("ion") && !n.Contains("rifle")) return "ion";
        if (n.Contains("disruptor")) return "disruptor_rifle";
        if (n.Contains("detonator") || n.Contains("grenade") || n.Contains("mortar")) return "explosive";
        return "";
    }

    /// <summary>Returns applicable weapon abilities for the character's equipped weapons.</summary>
    public List<string> GetWeaponAbilities(GameCharacter character)
    {
        var abilities = new List<string>();
        var subtype = GetWeaponSubtype(character.EquippedWeapon);
        if (string.IsNullOrEmpty(subtype)) return abilities;

        bool hasDualWield = !string.IsNullOrEmpty(character.OffHandWeapon);

        foreach (var ability in WeaponAbilities.Values.OrderBy(a => a.Name))
        {
            if (!ability.ApplicableSubtypes.Any(s => s.Equals(subtype, StringComparison.OrdinalIgnoreCase))) continue;
            if (ability.RequiresDualWield && !hasDualWield) continue;
            if (!string.IsNullOrEmpty(ability.RequiresSkill) &&
                !character.Skills.Any(s => s.Contains(ability.RequiresSkill, StringComparison.OrdinalIgnoreCase))) continue;
            abilities.Add(ability.Name);
        }

        // Off-hand shoto unlocks shoto abilities even if main is full lightsaber
        if (!string.IsNullOrEmpty(character.OffHandWeapon))
        {
            var offSubtype = GetWeaponSubtype(character.OffHandWeapon);
            if (offSubtype == "shoto_lightsaber")
            {
                foreach (var ability in WeaponAbilities.Values.Where(a => a.ApplicableSubtypes.Contains("shoto_lightsaber")))
                    if (!abilities.Contains(ability.Name)) abilities.Add(ability.Name);
            }
        }

        return abilities;
    }

    /// <summary>Validates whether an off-hand weapon can be equipped, returning false + reason if not.</summary>
    public bool CanEquipOffHand(GameCharacter character, string weaponName, out string reason)
    {
        reason = "";
        if (!Weapons.TryGetValue(weaponName, out var w))
        {
            reason = $"'{weaponName}' is not a recognised weapon.";
            return false;
        }
        if (!w.IsOneHanded)
        {
            reason = $"{weaponName} is two-handed and cannot be held in the off-hand.";
            return false;
        }
        var offSubtype  = w.WeaponSubtype;
        var mainSubtype = GetWeaponSubtype(character.EquippedWeapon);

        if (offSubtype == "pistol") return true;
        if (offSubtype == "shoto_lightsaber" && (mainSubtype == "lightsaber" || mainSubtype == "shoto_lightsaber")) return true;
        if (offSubtype == "lightsaber" && mainSubtype == "lightsaber")
        {
            if (character.Skills.Any(s => s.Contains("Jar'kai", StringComparison.OrdinalIgnoreCase) || s.Contains("Dual Wield", StringComparison.OrdinalIgnoreCase)))
                return true;
            reason = "Wielding a full lightsaber in your off-hand requires the Jar'kai training skill.";
            return false;
        }
        reason = $"{weaponName} cannot be held in the off-hand slot.";
        return false;
    }

    public string EquipOffHandWeapon(GameCharacter character, string weaponName)
    {
        if (string.IsNullOrWhiteSpace(weaponName)) return UnequipOffHand(character);
        if (!character.Inventory.Any(x => x.Equals(weaponName, StringComparison.OrdinalIgnoreCase)))
            return "That weapon is not in your inventory.";
        if (!CanEquipOffHand(character, weaponName, out var reason)) return reason;
        character.OffHandWeapon = weaponName;
        return $"Equipped {weaponName} in your off-hand.";
    }

    public string UnequipOffHand(GameCharacter character)
    {
        if (string.IsNullOrEmpty(character.OffHandWeapon)) return "Nothing equipped in off-hand.";
        var old = character.OffHandWeapon; character.OffHandWeapon = ""; return $"Removed {old} from off-hand.";
    }

    public string EquipTool(GameCharacter character, string toolName)
    {
        if (string.IsNullOrWhiteSpace(toolName)) return UnequipTool(character);
        if (!character.Inventory.Any(x => x.Equals(toolName, StringComparison.OrdinalIgnoreCase)))
            return "That tool is not in your inventory.";
        if (!HarvestingTools.ContainsKey(toolName))
            return $"'{toolName}' is not a recognised harvesting tool.";
        character.EquippedTool = toolName;
        return $"Equipped {toolName} as your active tool.";
    }

    public string UnequipTool(GameCharacter character)
    {
        if (string.IsNullOrEmpty(character.EquippedTool)) return "No tool equipped.";
        var old = character.EquippedTool; character.EquippedTool = ""; return $"Removed {old} from tool slot.";
    }

    private void InitWeaponAbilities()
    {
        void WA(string name, string desc, string[] subtypes, int stamCost, int multPct, int flatBonus,
                bool multi = false, int hits = 1, bool piercing = false, bool stun = false,
                bool suppressive = false, bool dualReq = false, string skill = "")
        {
            WeaponAbilities[name] = new WeaponCombatAbility
            {
                Name = name, Description = desc, StaminaCost = stamCost,
                DamageMultPercent = multPct, FlatDamageBonus = flatBonus,
                IsMultiHit = multi, HitCount = hits,
                ArmorPiercing = piercing, CanStun = stun, IsSuppressive = suppressive,
                RequiresDualWield = dualReq, RequiresSkill = skill,
                ApplicableSubtypes = subtypes.ToList()
            };
        }

        // ── Pistols ─────────────────────────────────────────────────────────────
        WA("Quick Draw",        "Fire twice in rapid succession with lightning speed.",
            new[]{"pistol"}, 5, 90, 0, multi:true, hits:2);
        WA("Aimed Blaster",     "Take careful aim for a high-damage, near-certain hit.",
            new[]{"pistol"}, 8, 160, 0);
        WA("Twin Blasters",     "Fire both pistols simultaneously for a devastating burst.",
            new[]{"pistol"}, 12, 100, 5, multi:true, hits:3, dualReq:true);

        // ── Blaster Rifles ──────────────────────────────────────────────────────
        WA("Precision Shot",    "A carefully aligned bolt that ignores half armor.",
            new[]{"blaster_rifle"}, 6, 130, 0, piercing:true);
        WA("Suppressive Fire",  "Lay down covering fire, forcing the enemy to keep their head down.",
            new[]{"blaster_rifle"}, 10, 75, 0, multi:true, hits:2, suppressive:true);
        WA("Burst Fire",        "A short controlled burst for increased damage.",
            new[]{"blaster_rifle"}, 8, 110, 0, multi:true, hits:2);

        // ── Sniper Rifle ────────────────────────────────────────────────────────
        WA("Snipe",             "Line up a killing shot from range. High damage, armor-piercing.",
            new[]{"sniper_rifle"}, 10, 260, 6, piercing:true);
        WA("Leg Shot",          "Target the legs. Low damage but immobilizes the enemy for 2 rounds.",
            new[]{"sniper_rifle"}, 8, 55, 0, stun:true);
        WA("Overwatch",         "Set up an overwatch shot — deals extra damage if enemy attacks this round.",
            new[]{"sniper_rifle"}, 12, 200, 8, piercing:true);

        // ── Heavy Blaster ────────────────────────────────────────────────────────
        WA("Power Shot",        "An overcharged bolt that knocks back and stuns the target.",
            new[]{"heavy_blaster"}, 8, 180, 4, stun:true);
        WA("Scatter Shot",      "A wide-angle blast that hits multiple times.",
            new[]{"heavy_blaster"}, 12, 90, 0, multi:true, hits:3);

        // ── Rotary Blaster Cannon ────────────────────────────────────────────────
        WA("Spray",             "Unleash a rotary cannon volley — 4-6 rapid-fire hits at reduced damage.",
            new[]{"rotary_cannon"}, 15, 65, 0, multi:true, hits:5);
        WA("Suppressive Spray", "Full-auto suppression — enemy is stunned for 2 rounds.",
            new[]{"rotary_cannon"}, 20, 55, 0, multi:true, hits:4, suppressive:true, stun:true);
        WA("Concentrated Burst","Aim the cannon for a single devastating concentrated blast.",
            new[]{"rotary_cannon"}, 12, 200, 6);

        // ── Ion ─────────────────────────────────────────────────────────────────
        WA("Ion Burst",         "Supercharge the ion cell — double damage vs droids, shields, and vehicles.",
            new[]{"ion"}, 8, 200, 0);

        // ── Disruptor ───────────────────────────────────────────────────────────
        WA("Disruptor Shot",    "Fire a disruptor beam that bypasses all armor entirely.",
            new[]{"disruptor_rifle"}, 10, 150, 6, piercing:true);

        // ── Vibroblade ──────────────────────────────────────────────────────────
        WA("Riposte",           "Parry and counter-strike with surgical precision.",
            new[]{"vibroblade"}, 4, 130, 3);
        WA("Execution Strike",  "A powerful finisher that deals double damage to weakened foes.",
            new[]{"vibroblade"}, 10, 200, 0);
        WA("Pommel Strike",     "Strike with the hilt for stun damage.",
            new[]{"vibroblade", "melee"}, 5, 80, 4, stun:true);

        // ── Lightsaber ──────────────────────────────────────────────────────────
        WA("Saber Rush",        "Dash forward and deliver a powerful overhead strike.",
            new[]{"lightsaber", "shoto_lightsaber"}, 6, 145, 3, stun:true);
        WA("Deflect",           "Deflect incoming blaster bolts and redirect some back at the attacker.",
            new[]{"lightsaber", "shoto_lightsaber"}, 5, 50, 0);

        // ── Shoto / Dual Saber ──────────────────────────────────────────────────
        WA("Twin Fangs",        "Two rapid alternating strikes with blade and shoto.",
            new[]{"shoto_lightsaber"}, 10, 90, 0, multi:true, hits:2);
        WA("Jar'kai Flourish",  "A lightning-fast three-hit Jar'Kai combo that overwhelms defenses.",
            new[]{"shoto_lightsaber"}, 14, 85, 0, multi:true, hits:3);
    }

    private void GrantCombatRewards(GameCharacter character, CombatEncounter encounter)
    {
        if (encounter.RewardGranted) return;
        character.Credits += encounter.RewardCredits;
        if (!string.IsNullOrWhiteSpace(encounter.RewardItem))
        {
            character.Inventory.Add(encounter.RewardItem);
        }
        encounter.RewardGranted = true;
    }

    private string PickCombatRewardItem(string planetName, string enemyName)
    {
        var pool = new List<string>();
        var planet = Planets.GetValueOrDefault(planetName);
        var eco = (planet?.Economy ?? string.Empty).ToLowerInvariant();

        if (eco.Contains("mining") || eco.Contains("refinery"))
        {
            pool.AddRange(new[] { "raw ore", "raw carbonite", "durasteel ingot", "ion capacitor core" });
        }
        if (eco.Contains("ship") || eco.Contains("industry") || eco.Contains("manufacturing"))
        {
            pool.AddRange(new[] { "power cell", "sensor array", "hyperdrive part", "repair kit" });
        }
        if (eco.Contains("smugg") || eco.Contains("spice"))
        {
            pool.AddRange(new[] { "smuggler cache", "pressurized tibanna", "thermal detonator" });
        }
        if (enemyName.Contains("creature", StringComparison.OrdinalIgnoreCase) || Creatures.ContainsKey(enemyName))
        {
            pool.AddRange(new[] { "raw polymer sap", "composite weave", "field medpack" });
        }

        if (pool.Count == 0)
        {
            pool.AddRange(new[] { "repair kit", "power cell", "field medpack" });
        }

        return pool[random.Next(pool.Count)];
    }

    public string CraftItem(GameCharacter character, string itemName)
    {
        if (!CanAccessAsset(character, itemName, out var accessDeniedReason))
        {
            return accessDeniedReason;
        }

        if (TryGetRecipe(itemName, out var visibilityRecipe) && visibilityRecipe.HideUntilBlueprintUnlocked && !IsRecipeVisible(character, itemName))
        {
            return "That recipe remains hidden until its blueprint is unlocked.";
        }

        if (TryGetRecipe(itemName, out var recipe))
        {
            var planet = Planets.GetValueOrDefault(character.Location);
            if (recipe.RequiresShipyard && (planet is null || !planet.HasDockyard))
            {
                var facility = GetFacilityRequirementsForAsset(itemName, recipe).FirstOrDefault() ?? "Shipyard";
                return $"This recipe requires {facility} access.";
            }

            if (recipe.RequiresIndustrialFurnace && (planet is null || !planet.HasIndustrialFurnace))
            {
                var facility = GetFacilityRequirementsForAsset(itemName, recipe).LastOrDefault() ?? "Forge";
                return $"This recipe requires {facility} access.";
            }

            if (character.Credits < recipe.CreditCost)
            {
                return $"You need {recipe.CreditCost} credits to craft {itemName}.";
            }

            var missing = recipe.Inputs
                .Where(i => CountInventoryItem(character, i.Item) < i.Quantity)
                .Select(i => $"{i.Item} x{i.Quantity}")
                .ToList();
            if (missing.Count > 0)
            {
                return $"Missing ingredients: {string.Join(", ", missing)}.";
            }

            foreach (var input in recipe.Inputs)
            {
                if (!ConsumeInventoryItem(character, input.Item, input.Quantity))
                {
                    return "Failed to consume required inputs.";
                }
            }

            character.Credits -= recipe.CreditCost;
            var added = 0;
            for (int i = 0; i < Math.Max(1, recipe.OutputQuantity); i++)
            {
                if (TryAddInventoryItem(character, recipe.Output).StartsWith("Added", StringComparison.OrdinalIgnoreCase)) added++;
            }

            character.Crafting.Add(recipe.Output);
            character.Experience += Math.Max(2, recipe.TimeHours / 2);
            if (recipe.TimeHours > 1) AdvanceWorldTime(Math.Min(recipe.TimeHours, 24), character.Location, character);
            return $"Crafted {recipe.Output} ({added}x). Used {recipe.CreditCost} credits.";
        }

        // allow crafting certain weapons via legacy parts rule
        var matchedWeapon = Weapons.Keys.FirstOrDefault(k => string.Equals(k, itemName, StringComparison.OrdinalIgnoreCase));
        if (matchedWeapon is not null)
        {
            return CraftWeapon(character, matchedWeapon);
        }
        return "That crafting recipe is unknown.";
    }

    public (bool success, string message) CraftShip(GameCharacter character, string shipModel)
    {
        if (!ShipCatalog.TryGetValue(shipModel, out var blueprint))
        {
            return (false, "That ship is not available in the catalog.");
        }

        if (!CanAccessAsset(character, blueprint.Name, out var accessDeniedReason))
        {
            return (false, accessDeniedReason);
        }

        var currentPlanet = Planets.GetValueOrDefault(character.Location);
        if ((blueprint.IsCapital || blueprint.Cost >= 260) && (currentPlanet is null || !currentPlanet.HasDockyard))
        {
            return (false, "This class of ship requires a dockyard planet to assemble.");
        }

        if (character.Credits < blueprint.Cost)
        {
            return (false, $"You need {blueprint.Cost} credits to build that ship.");
        }

        var missingParts = blueprint.RequiredParts.Where(required => CountInventoryItem(character, required) < 1).ToList();
        if (missingParts.Count > 0)
        {
            return (false, $"You are missing the required parts: {string.Join(", ", missingParts)}.");
        }

        if (blueprint.IsCapital && CapitalShipPartRequirements.TryGetValue(blueprint.Model, out var capitalParts))
        {
            var missingCapitalParts = capitalParts.Where(p => CountInventoryItem(character, p) < 1).ToList();
            if (missingCapitalParts.Count > 0)
            {
                return (false, $"Capital assembly requires shipyard parts: {string.Join(", ", missingCapitalParts)}.");
            }
            foreach (var cp in capitalParts)
            {
                if (!ConsumeInventoryItem(character, cp, 1))
                {
                    return (false, $"Failed to consume required capital part: {cp}.");
                }
            }
        }

        character.Credits -= blueprint.Cost;
        foreach (var part in blueprint.RequiredParts)
        {
            if (!ConsumeInventoryItem(character, part, 1))
            {
                return (false, $"Failed to consume required part: {part}.");
            }
        }

        // Capital ships take time to construct
        if (blueprint.IsCapital)
        {
            var hours = Math.Max(72, blueprint.Cost / 4);
            ConstructionQueue.Add(new ConstructionProject { Owner = character, Blueprint = blueprint, RemainingHours = hours });
            return (true, $"Construction started on {blueprint.Name}. It will take approximately {hours} hours to complete.");
        }

        character.Ship = CreateShipFromBlueprint(blueprint, character.Name);

        character.Experience += 15;
        return (true, $"You assemble a {shipModel} using your parts and credits.");
    }

    public (bool success, string message) TryPurchaseShip(GameCharacter character, string shipModel)
    {
        if (character.Ship is not null)
            return (false, "You already own a ship. Sell or decommission it before purchasing another.");

        if (!ShipCatalog.TryGetValue(shipModel, out var blueprint))
            return (false, "That ship is not in the current market catalog.");

        // Tier check — orbital-only ships require an orbital station shipyard
        bool atOrbital = SpaceStations.TryGetValue(character.Location, out var st) && st.HasShipyard;
        bool atPlanetary = Planets.TryGetValue(character.Location, out var pl) && pl.HasDockyard;
        if (!atOrbital && !atPlanetary)
            return (false, "You must be at a shipyard to purchase a ship.");

        if (string.Equals(blueprint.ShipyardTier, "orbital", StringComparison.OrdinalIgnoreCase) && !atOrbital)
            return (false, $"The {shipModel} is a {blueprint.SizeClass}-class vessel only available through an orbital shipyard. Travel to an orbital station to purchase it.");

        // Faction/era locks still apply to purchases (some ships are faction-restricted)
        if (!CanAccessFactionLockedAsset(character, blueprint.Name, out var factionDenial))
            return (false, factionDenial);
        if (!CanAccessEraLockedAsset(blueprint.Name, out var eraDenial))
            return (false, eraDenial);

        var price = blueprint.PurchasePrice > 0 ? blueprint.PurchasePrice : blueprint.Cost;
        if (character.Credits < price)
            return (false, $"You need {price:N0} credits to buy the {shipModel} (you have {character.Credits:N0} cr).");

        character.Credits -= price;
        character.Ship = CreateShipFromBlueprint(blueprint, character.Name);
        character.Experience += 10;
        return (true, $"You purchased a {shipModel} for {price:N0} credits. Welcome aboard, Captain.");
    }

    public string InstallShipArmamentFromHangar(GameCharacter character, string armamentName, string? slotName = null)
    {
        if (character.Ship is null) return "You do not own a ship.";
        if (!IsHangarAccessible(character)) return "Ship armaments can only be changed in a hangar.";
        if (!character.HangarInventory.Any(x => string.Equals(x, armamentName, StringComparison.OrdinalIgnoreCase))) return "That armament must be stored in the hangar before mounting.";
        if (!ShipArmaments.TryGetValue(armamentName, out var armament)) return "That item is not a ship armament.";

        EnsureShipSystemsInitialized(character.Ship);
        if (character.Ship.Armaments.Any(x => string.Equals(x, armamentName, StringComparison.OrdinalIgnoreCase)))
        {
            return $"{armamentName} is already mounted on your ship.";
        }

        var compatibleSlots = GetCompatibleHardpointSlots(character.Ship, armamentName).ToList();
        if (compatibleSlots.Count == 0)
        {
            return $"No compatible hardpoint slot is available for {armamentName}. Hardpoints: {GetShipHardpointSummary(character.Ship)}.";
        }

        ShipHardpointSlot? slot = null;
        if (!string.IsNullOrWhiteSpace(slotName))
        {
            slot = compatibleSlots.FirstOrDefault(x => string.Equals(x.SlotName, slotName, StringComparison.OrdinalIgnoreCase));
            if (slot is null)
            {
                return $"{slotName} cannot accept {armamentName}.";
            }
        }
        else
        {
            slot = compatibleSlots[0];
        }

        slot.MountedArmament = armamentName;
        character.Ship.Armaments.Add(armamentName);
        character.HangarInventory.RemoveAll(x => string.Equals(x, armamentName, StringComparison.OrdinalIgnoreCase));
        character.Ship.Weapon = BuildShipWeaponSummary(character.Ship);
        character.Experience += 6;
        return $"Mounted {armamentName} to {slot.SlotName} ({slot.Position}). Hardpoints: {GetShipHardpointSummary(character.Ship)}.";
    }

    public string RemoveShipArmamentToHangar(GameCharacter character, string armamentName)
    {
        if (character.Ship is null) return "You do not own a ship.";
        if (!IsHangarAccessible(character)) return "Ship armaments can only be changed in a hangar.";
        EnsureShipSystemsInitialized(character.Ship);

        var slot = character.Ship.HardpointSlots.FirstOrDefault(x => string.Equals(x.MountedArmament, armamentName, StringComparison.OrdinalIgnoreCase));
        if (slot is null)
        {
            return "That armament is not mounted on your ship.";
        }

        slot.MountedArmament = string.Empty;
        character.Ship.Armaments.RemoveAll(x => string.Equals(x, armamentName, StringComparison.OrdinalIgnoreCase));
        if (!character.HangarInventory.Any(x => string.Equals(x, armamentName, StringComparison.OrdinalIgnoreCase)))
        {
            character.HangarInventory.Add(armamentName);
        }
        character.Ship.Weapon = BuildShipWeaponSummary(character.Ship);
        return $"Removed {armamentName} from {slot.SlotName} and stored it in the hangar.";
    }

    public bool TravelTo(GameCharacter character, string planetName)
    {
        if (character.Ship is null)
        {
            return false;
        }

        var planet = Planets[planetName];
        var fuelCost = GetTravelFuelCost(character, planetName);
        if (character.Ship.Fuel < fuelCost)
        {
            return false;
        }

        character.Ship.Fuel -= fuelCost;
        character.Credits = Math.Max(0, character.Credits - Math.Max(1, fuelCost / 6));
        AdvanceWorldTime(GetTravelHours(character, planetName), planetName, character);
        character.Location = planetName;
        character.Experience += 5;
        character.Stamina = Math.Max(0, character.Stamina - 2);
        return true;
    }

    public string ApplyShipUpgrade(GameCharacter character, string upgradeName)
    {
        if (character.Ship is null)
        {
            return "You do not own a ship.";
        }

        if (!character.Inventory.Any(x => string.Equals(x, upgradeName, StringComparison.OrdinalIgnoreCase)))
        {
            return "You do not have that upgrade in your inventory.";
        }

        if (ShipArmaments.TryGetValue(upgradeName, out var armament))
        {
            return "Ship armaments can only be changed in a hangar. Transfer the armament to hangar storage first.";
        }

        if (ShipUpgradeCatalog.TryGetValue(upgradeName, out var catalogUpgrade))
        {
            if (GetShipSizeRank(character.Ship.SizeClass) < GetShipSizeRank(catalogUpgrade.MinimumShipSize))
            {
                return $"{upgradeName} requires a {catalogUpgrade.MinimumShipSize}-class ship or larger.";
            }

            if (catalogUpgrade.Unique && character.Ship.Upgrades.Any(x => string.Equals(x, upgradeName, StringComparison.OrdinalIgnoreCase)))
            {
                return $"{upgradeName} is already installed.";
            }

            if (catalogUpgrade.HyperdriveClassTarget > 0)
            {
                if (character.Ship.HyperdriveClass <= catalogUpgrade.HyperdriveClassTarget)
                {
                    return $"Your ship already has a Class {character.Ship.HyperdriveClass} hyperdrive or better.";
                }
                character.Ship.HyperdriveClass = catalogUpgrade.HyperdriveClassTarget;
            }

            if (catalogUpgrade.FuelCapacityBonus > 0)
            {
                character.Ship.MaxFuel += catalogUpgrade.FuelCapacityBonus;
                character.Ship.Fuel = Math.Min(character.Ship.MaxFuel, character.Ship.Fuel + catalogUpgrade.FuelCapacityBonus);
            }

            if (catalogUpgrade.FuelEfficiencyDeltaPercent != 0)
            {
                character.Ship.FuelEfficiencyPercent = Math.Clamp(character.Ship.FuelEfficiencyPercent + catalogUpgrade.FuelEfficiencyDeltaPercent, 55, 130);
            }

            if (catalogUpgrade.TravelHoursModifier != 0)
            {
                character.Ship.TravelHoursModifier += catalogUpgrade.TravelHoursModifier;
            }

            if (catalogUpgrade.ShieldBonus > 0) character.Ship.Shield += catalogUpgrade.ShieldBonus;
            if (catalogUpgrade.HullBonus > 0) character.Ship.Hull += catalogUpgrade.HullBonus;
            if (catalogUpgrade.RefuelAmount > 0) character.Ship.Fuel = Math.Min(character.Ship.MaxFuel, character.Ship.Fuel + catalogUpgrade.RefuelAmount);
            if (!string.IsNullOrWhiteSpace(catalogUpgrade.WeaponOverride)) character.Ship.BaseWeapon = catalogUpgrade.WeaponOverride;

            if (!catalogUpgrade.Consumable)
            {
                character.Ship.Upgrades.Add(upgradeName);
            }

            character.Ship.Weapon = BuildShipWeaponSummary(character.Ship);
            character.Inventory.RemoveAll(x => string.Equals(x, upgradeName, StringComparison.OrdinalIgnoreCase));
            character.Experience += 4;
            return $"Installed {upgradeName}. Hyperdrive Class {character.Ship.HyperdriveClass} | Fuel {character.Ship.Fuel}/{character.Ship.MaxFuel} | Hardpoints {GetShipHardpointSummary(character.Ship)}.";
        }

        switch (upgradeName.ToLowerInvariant())
        {
            case "shield booster":
                character.Ship.Shield += 6;
                break;
            case "hyperdrive part":
                character.Ship.Fuel = Math.Min(character.Ship.MaxFuel, character.Ship.Fuel + 20);
                break;
            case "laser upgrade":
                character.Ship.Weapon = "Enhanced laser";
                break;
            case "fuel cell":
                character.Ship.Fuel = Math.Min(character.Ship.MaxFuel, character.Ship.Fuel + 30);
                break;
            case "sensor array":
                character.Ship.Upgrades.Add("sensor array");
                break;
            default:
                character.Ship.Upgrades.Add(upgradeName);
                break;
        }

        character.Inventory.Remove(upgradeName);
        character.Experience += 4;
        return $"Installed {upgradeName}.";
    }

    // ─── Romance System ────────────────────────────────────────────────────────

    private static readonly string[] FlirtLines = new[]
    {
        "You catch their eye across the cantina and share a lingering look.",
        "You drop a witty remark and earn a genuine laugh.",
        "You ask about their homeworld and listen with real interest.",
        "You share a drink and the conversation flows easily.",
        "You defend their honor in a minor dispute — they notice.",
        "You trade stories of the outer rim. The hours slip by.",
        "You compliment their piloting skills. They smile.",
        "You find an excuse to brush shoulders. The air between you shifts.",
    };

    private static readonly string[] RejectedLines = new[]
    {
        "They seem distracted tonight. Try again another time.",
        "\"You are interesting,\" they say, \"but I have somewhere to be.\"",
        "Your approach is too bold too soon. They step back slightly.",
        "A cold shoulder — perhaps build trust with them first.",
    };

    private static readonly string[] GiftResponses = new[]
    {
        "Their eyes light up. \"You remembered.\"",
        "They accept it carefully. \"This is... thoughtful.\"",
        "A brief smile. \"You did not have to.\"",
        "They tuck it away and squeeze your hand.",
    };

    private static readonly string[] ProposalAccepted = new[]
    {
        "Tears in their eyes. \"Yes. A thousand times yes.\"",
        "They laugh, then cry, then take your hands. \"Of course.\"",
        "\"I thought you would never ask,\" they whisper.",
        "A quiet nod. Then an embrace that says everything.",
    };

    private static readonly string[] ProposalRejected = new[]
    {
        "\"Not yet. I need more time with you.\"",
        "\"I care for you, but I am not ready.\"",
        "\"Ask me again when the galaxy feels less uncertain.\"",
    };

    private static readonly Dictionary<string, int> GiftValues = new(StringComparer.OrdinalIgnoreCase)
    {
        ["corusca fragment"]              = 14,
        ["kyber crystal shard"]           = 16,
        ["beskar fragment"]               = 12,
        ["krayt dragon pearl fragment"]   = 18,
        ["fire gem"]                      = 10,
        ["gemstone shard"]                = 10,
        ["cloudium crystal"]              = 12,
        ["naboo plasma crystal"]          = 14,
        ["void crystal"]                  = 16,
        ["ancient relic fragment"]        = 8,
        ["white kyber fragment"]          = 14,
        ["kshyyyk amber"]                 = 8,
        ["ewok amber"]                    = 8,
        ["porg bone chip"]                = 4,
        ["power cell"]                    = 3,
        ["circuit board"]                 = 2,
        ["wroshyr resin"]                 = 6,
        ["tibanna gas canister"]          = 7,
        ["sea mineral"]                   = 5,
        ["permafrost crystal"]            = 8,
        ["doonium ore"]                   = 6,
    };

    // Species preferred gifts — give double ♥
    private static readonly Dictionary<string, string[]> SpeciesGiftPreferences = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Human"]        = new[] { "corusca fragment", "gemstone shard", "ancient relic fragment" },
        ["Twi'lek"]      = new[] { "naboo plasma crystal", "fire gem", "cloudium crystal" },
        ["Wookiee"]      = new[] { "wroshyr resin", "kshyyyk amber", "hardwood chip" },
        ["Zabrak"]       = new[] { "fire gem", "obsidian shard", "titanite ore" },
        ["Mirialan"]     = new[] { "kyber crystal shard", "white kyber fragment", "pure silicate" },
        ["Miraluka"]     = new[] { "kyber crystal shard", "white kyber fragment", "force-imbued sediment" },
        ["Togruta"]      = new[] { "sea mineral", "aquite crystal", "brine salt" },
        ["Nautolan"]     = new[] { "deep-sea ore", "sea mineral", "sea glass" },
        ["Mandalorian"]  = new[] { "beskar fragment", "beskar-laced clay", "titanite ore" },
        ["Chiss"]        = new[] { "void crystal", "cold quartz", "ice mineral" },
        ["Rodian"]       = new[] { "doonium ore", "circuit board", "power cell" },
        ["Bothan"]       = new[] { "ancient relic fragment", "corusca fragment", "rare circuit board" },
        ["Mon Calamari"] = new[] { "aquite crystal", "sea mineral", "deep-sea ore" },
        ["Duros"]        = new[] { "tibanna gas canister", "refined tibanna", "coaxium trace" },
        ["Sullustian"]   = new[] { "tibanna gas canister", "gas mineral", "power cell" },
        ["Sith"]         = new[] { "void crystal", "dark obsidian", "sith-tainted stone" },
        ["Kel Dor"]      = new[] { "permafrost crystal", "cold quartz", "ice mineral" },
    };

    public int GetGiftValue(string itemName, string targetSpecies)
    {
        GiftValues.TryGetValue(itemName, out var baseVal);
        if (baseVal == 0) baseVal = 3;
        if (SpeciesGiftPreferences.TryGetValue(targetSpecies, out var preferred)
            && preferred.Any(p => p.Equals(itemName, StringComparison.OrdinalIgnoreCase)))
            return baseVal * 2;
        return baseVal;
    }

    // ─── Random cantina romance encounter ────────────────────────────────────
    public bool TrySpawnCantinaRomance(GameCharacter character, string zone, out RomanceEncounterEvent? evt)
    {
        evt = null;
        if (character.Family.Married || character.Family.HeartbreakCooldown > 0) return false;
        if (!string.IsNullOrWhiteSpace(character.Family.RomanceTargetName)) return false;
        var socialZones = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "cantina", "market", "marketplace", "dock", "slums" };
        if (!socialZones.Contains(zone)) return false;
        if (random.NextDouble() > 0.08) return false;

        var npcSpecies = SelectEncounterNpcSpecies(character.Location, zone, character.Species);
        var npcName    = GenerateNameForRace(npcSpecies);
        var relation   = GetInterspeciesRelation(character.Species, npcSpecies);
        if (relation <= -6) return false;

        var openers = new[]
        {
            $"{npcName} catches your eye from across the cantina. They raise their glass slightly.",
            $"A {npcSpecies} named {npcName} leans against the bar beside you. \"You look like you've seen something interesting today.\"",
            $"{npcName} is sitting alone, studying a star map. They glance up and hold your gaze a moment longer than necessary.",
            $"\"You are not from this sector,\" says {npcName}, a {npcSpecies} with an unreadable smile.",
            $"{npcName} slides into the seat across from you uninvited. \"The galaxy is too small to sit alone in.\""
        };

        evt = new RomanceEncounterEvent
        {
            NpcName     = npcName,
            NpcSpecies  = npcSpecies,
            Planet      = character.Location,
            Zone        = zone,
            OpeningLine = openers[random.Next(openers.Length)]
        };

        character.Family.RomanceTargetName    = npcName;
        character.Family.RomanceTargetSpecies = npcSpecies;
        character.Family.RomanceTargetPlanet  = character.Location;
        character.Family.FlirtStage           = "acquaintance";
        character.Family.LovePoints           = random.Next(4, 10);
        return true;
    }

    public (bool success, int pointsGained, string message) FlirtWithNpc(GameCharacter character, string npcName, string npcSpecies, string planet)
    {
        var fam = character.Family;

        if (fam.Married)
            return (false, 0, $"You are already married to {fam.SpouseName}. Honour that bond.");

        if (fam.HeartbreakCooldown > 0)
        {
            fam.HeartbreakCooldown = Math.Max(0, fam.HeartbreakCooldown - 1);
            return (false, 0, $"Your heart is still healing. Give it {fam.HeartbreakCooldown + 1} more rotation(s).");
        }

        // Starting a new romance
        if (string.IsNullOrWhiteSpace(fam.RomanceTargetName))
        {
            fam.RomanceTargetName    = npcName;
            fam.RomanceTargetSpecies = npcSpecies;
            fam.RomanceTargetPlanet  = planet;
            fam.FlirtStage           = "acquaintance";
            fam.LovePoints           = 0;
        }

        // Must be with the same NPC
        if (!fam.RomanceTargetName.Equals(npcName, StringComparison.OrdinalIgnoreCase))
            return (false, 0, $"{fam.RomanceTargetName} would not appreciate you flirting with someone else right now.");

        var relation  = GetInterspeciesRelation(character.Species, npcSpecies);
        var baseGain  = random.Next(6, 13);
        var modifier  = relation >= 3 ? 3 : relation <= -3 ? -4 : 0;
        var repBonus  = character.Reputation >= 20 ? 2 : character.Reputation <= -10 ? -2 : 0;

        // Chance of rejection based on stage
        var rejectChance = fam.FlirtStage switch
        {
            "stranger"     => 0.45,
            "acquaintance" => 0.25,
            "crush"        => 0.12,
            _              => 0.05
        };

        if (random.NextDouble() < rejectChance)
        {
            var rejection = RejectedLines[random.Next(RejectedLines.Length)];
            return (false, 0, rejection);
        }

        var gained = Math.Max(1, baseGain + modifier + repBonus);
        fam.LovePoints = Math.Min(100, fam.LovePoints + gained);
        UpdateFlirtStage(fam);

        var line = FlirtLines[random.Next(FlirtLines.Length)];
        var stageNote = fam.LovePoints >= 70 ? " (You could propose now.)" : "";
        return (true, gained, $"{line} [{npcName} — {fam.FlirtStage}, ♥ {fam.LovePoints}/100]{stageNote}");
    }

    public (bool success, int pointsGained, string message) GiftNpc(GameCharacter character, string itemName)
    {
        var fam = character.Family;

        if (string.IsNullOrWhiteSpace(fam.RomanceTargetName))
            return (false, 0, "You have no one to gift. Start a romance first.");

        if (!character.Inventory.Any(x => x.Equals(itemName, StringComparison.OrdinalIgnoreCase)))
            return (false, 0, $"You do not have '{itemName}' in your inventory.");

        character.Inventory.Remove(
            character.Inventory.First(x => x.Equals(itemName, StringComparison.OrdinalIgnoreCase)));

        GiftValues.TryGetValue(itemName, out var giftValue);
        if (giftValue == 0) giftValue = 3;
        // Species-specific preferred gifts give double ♥
        var giftValueFinal = GetGiftValue(itemName, fam.RomanceTargetSpecies);

        fam.LovePoints = Math.Min(100, fam.LovePoints + giftValueFinal);
        var bonusNote = giftValueFinal > giftValue ? " ★ They love this kind of gift!" : "";
        fam.GiftLog.Add($"{itemName} → {fam.RomanceTargetName} (+{giftValueFinal} ♥){bonusNote}");
        UpdateFlirtStage(fam);

        var response = GiftResponses[random.Next(GiftResponses.Length)];
        return (true, giftValueFinal, $"{fam.RomanceTargetName}: {response} [+{giftValueFinal} ♥, now {fam.LovePoints}/100]{bonusNote}");
    }

    public (bool success, string message) ProposeToNpc(GameCharacter character)
    {
        var fam = character.Family;

        if (fam.Married)
            return (false, $"You are already married to {fam.SpouseName}.");

        if (string.IsNullOrWhiteSpace(fam.RomanceTargetName))
            return (false, "You have no romance to propose to.");

        if (fam.LovePoints < 70)
            return (false, $"Your bond with {fam.RomanceTargetName} is not strong enough yet (♥ {fam.LovePoints}/70 needed). Keep courting them.");

        // 85% accept at 70–89, guaranteed at 90+
        var acceptChance = fam.LovePoints >= 90 ? 1.0 : 0.85;
        if (random.NextDouble() > acceptChance)
        {
            var rejection = ProposalRejected[random.Next(ProposalRejected.Length)];
            fam.LovePoints = Math.Max(0, fam.LovePoints - 8);
            return (false, $"{fam.RomanceTargetName}: {rejection} [♥ {fam.LovePoints}/100]");
        }

        return TryMarry(character, fam.RomanceTargetName, fam.RomanceTargetSpecies);
    }

    public (bool success, string message) TryMarry(GameCharacter character, string spouseName, string spouseSpecies)
    {
        if (character.Family.Married)
            return (false, "You are already married.");

        character.Family.Married         = true;
        character.Family.SpouseName      = spouseName;
        character.Family.SpouseSpecies   = spouseSpecies;
        character.Family.FlirtStage      = "married";
        character.Family.LovePoints      = 100;
        character.Reputation            += 10;
        character.Morale                 = Math.Min(100, character.Morale + 20);
        character.Notes.Add($"Married {spouseName} ({spouseSpecies})");
        character.Family.RomanceHistory.Add($"Married {spouseName} ({spouseSpecies}) at rotation {Clock.Rotation}");

        var accepted = ProposalAccepted[random.Next(ProposalAccepted.Length)];
        return (true, $"{spouseName}: {accepted}\nYou are now married to {spouseName}. Your morale surges and your reputation grows.");
    }

    public (bool success, string message) DivorceSpouse(GameCharacter character)
    {
        if (!character.Family.Married)
            return (false, "You are not married.");

        var formerName    = character.Family.SpouseName ?? "your spouse";
        character.Family.RomanceHistory.Add($"Separated from {formerName} at rotation {Clock.Rotation}");
        character.Family.Married              = false;
        character.Family.SpouseName           = null;
        character.Family.SpouseSpecies        = null;
        character.Family.RomanceTargetName    = "";
        character.Family.RomanceTargetSpecies = "";
        character.Family.FlirtStage           = "stranger";
        character.Family.LovePoints           = 0;
        character.Family.HeartbreakCooldown   = 8;
        character.Reputation                 -= 5;
        character.Morale                      = Math.Max(0, character.Morale - 15);
        return (true, $"You and {formerName} part ways. Your heart needs time to heal (8 rotations).");
    }

    public string HaveChild(GameCharacter character)
    {
        if (!character.Family.Married || character.Family.SpouseName is null)
            return "You need a partner before you can have a child.";

        if (character.Family.LovePoints < 80)
            return $"Your bond with {character.Family.SpouseName} is not strong enough to start a family yet (♥ {character.Family.LovePoints}/80 needed).";

        var speciesParts  = new[] { character.Species[..Math.Min(3, character.Species.Length)],
                                    (character.Family.SpouseSpecies ?? "Hum")[..Math.Min(3, character.Family.SpouseSpecies?.Length ?? 3)] };
        var firstName     = GenerateNameForRace(character.Species);
        var childName     = $"{firstName}";
        character.Family.Children.Add(childName);
        character.Reputation += 5;
        character.Morale      = Math.Min(100, character.Morale + 10);
        character.Notes.Add($"Child born: {childName}");
        character.Family.RomanceHistory.Add($"Child {childName} born at rotation {Clock.Rotation}");
        return $"Your child {childName} is born. Your family line grows stronger.";
    }

    public string AdvanceLegacy(GameCharacter character)
    {
        if (character.Family.Children.Count == 0)
            return "No heirs remain to carry your legacy.";

        var heir = character.Family.Children[0];
        character.Family.Children.RemoveAt(0);
        character.Family.RomanceHistory.Add($"Legacy passed to {heir} at rotation {Clock.Rotation}");
        character.Name = heir;
        character.Notes.Add("Inherited from a previous generation");
        return $"Your heir {heir} now carries the mantle.";
    }

    private static void UpdateFlirtStage(FamilyRecord fam)
    {
        fam.FlirtStage = fam.LovePoints switch
        {
            >= 100 => "devoted",
            >= 70  => "devoted",
            >= 45  => "crush",
            >= 20  => "acquaintance",
            _      => "stranger"
        };
        if (fam.Married) fam.FlirtStage = "married";
    }

    public List<string> SimulateBackgroundTurn(GameCharacter character, string context)
    {
        var messages = new List<string>();
        if (character.IsAlive == false)
        {
            return new List<string> { "Your character is deceased and no longer reacts to the galaxy." };
        }

        if (character.Reputation >= 25)
        {
            character.Credits += 6;
            character.Morale += 2;
            messages.Add("Your reputation opens opportunities and you gain 6 credits.");
        }
        else if (character.Reputation <= -15)
        {
            character.Credits = Math.Max(0, character.Credits - 8);
            character.Hp = Math.Max(1, character.Hp - 3);
            messages.Add("Your reputation brings trouble, costing you credits and health.");
        }

        if (FactionStandings.GetValueOrDefault("Empire") > 8)
        {
            character.Reputation -= 2;
            character.Stress += 2;
            messages.Add("Imperial pressure closes around you and your standing slips.");
        }

        if (FactionStandings.GetValueOrDefault("Rebels") > 8)
        {
            character.Reputation += 2;
            messages.Add("Rebel sympathizers treat you as a trusted ally.");
        }

        if (character.Location is "Mustafar" or "Tatooine" or "Geonosis")
        {
            character.Armor = Math.Max(0, character.Armor - 1);
            messages.Add($"The harsh environment of {character.Location} wears on your gear.");
        }

        if (character.Ship is not null && character.Ship.Fuel < 25)
        {
            character.Ship.Fuel = Math.Max(0, character.Ship.Fuel - 3);
            messages.Add("Your ship is low on fuel and a jump will be risky.");
        }

        character.Stress = Math.Max(0, character.Stress + 1);
        character.Morale = Math.Clamp(character.Morale - 1, 0, 100);
        character.CurrentState = GetStateSummary(character);
        character.Condition = character.Hp < character.MaxHp / 2 ? "Wounded" : "Healthy";
        messages.AddRange(RefreshQuestProgress(character, context));
        character.Notes.Add($"Background pulse: {context}");
        return messages;
    }

    public void SaveGame(GameCharacter character)
    {
        var saveData = new GameSaveData
        {
            Character = character,
            Rotation = Clock.Rotation,
            Hour = Clock.Hour,
            Quests = new List<Quest>(ActiveQuests),
            FactionStandings = new Dictionary<string, int>(FactionStandings),
            DiscoveredPlanets = new HashSet<string>(DiscoveredPlanets),
            PlanetRotations = PlanetClocks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Rotation),
            ResourceNodeStates = resourceNodeStates.Values.ToList()
        };

        // Save construction queue as lightweight objects
        saveData.ConstructionQueue.Clear();
        foreach (var proj in ConstructionQueue)
        {
            saveData.ConstructionQueue.Add(new Dictionary<string, object>
            {
                ["owner"] = proj.Owner.Name,
                ["model"] = proj.Blueprint.Model,
                ["remainingHours"] = proj.RemainingHours
            });
        }

        var json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SavePath, json);
    }

    public int ImportRecipesFromJson(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath)) return 0;

        var raw = File.ReadAllText(jsonFilePath);
        using var doc = JsonDocument.Parse(raw);
        if (doc.RootElement.ValueKind != JsonValueKind.Array) return 0;

        var imported = 0;
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (!element.TryGetProperty("Name", out var nameProp)) continue;
            var name = nameProp.GetString();
            if (string.IsNullOrWhiteSpace(name)) continue;

            var matCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (element.TryGetProperty("Materials", out var materialsProp) && materialsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var m in materialsProp.EnumerateArray())
                {
                    var mat = m.GetString();
                    if (string.IsNullOrWhiteSpace(mat)) continue;
                    mat = mat.Trim();

                    // simple quantity parser for forms like "durasteel (x3)"
                    var qty = 1;
                    var lower = mat.ToLowerInvariant();
                    var ix = lower.IndexOf("(x", StringComparison.Ordinal);
                    if (ix >= 0)
                    {
                        var end = lower.IndexOf(')', ix);
                        if (end > ix + 2)
                        {
                            var rawQty = lower.Substring(ix + 2, end - (ix + 2));
                            if (int.TryParse(rawQty, out var parsed) && parsed > 0) qty = parsed;
                            mat = mat.Substring(0, ix).Trim();
                        }
                    }

                    var normalized = NormalizeImportedMaterialToken(mat);
                    if (string.IsNullOrWhiteSpace(normalized)) continue;
                    if (!matCounts.ContainsKey(normalized)) matCounts[normalized] = 0;
                    matCounts[normalized] += qty;
                }
            }

            if (matCounts.Count == 0)
            {
                matCounts["refined metal"] = 2;
                matCounts["power cell"] = 1;
            }

            if (!CraftableItems.ContainsKey(name))
            {
                CraftableItems[name] = new ItemBlueprint
                {
                    Name = name,
                    Category = "imported",
                    Description = "Imported from external dataset.",
                    Skill = "Crafting",
                    Cost = Math.Max(20, matCounts.Values.Sum() * 8)
                };
            }

            Recipes[name] = new CraftRecipe
            {
                Output = name,
                OutputQuantity = 1,
                CreditCost = Math.Max(6, matCounts.Values.Sum() * 2),
                TimeHours = Math.Max(1, matCounts.Values.Sum() / 2),
                Skill = CraftableItems[name].Skill,
                RequiresShipyard = name.Contains("star destroyer", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("cruiser", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("capital", StringComparison.OrdinalIgnoreCase),
                RequiresIndustrialFurnace = name.Contains("cannon", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("rifle", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("alloy", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("reactor", StringComparison.OrdinalIgnoreCase),
                Inputs = matCounts.Select(kvp => new RecipeComponent { Item = kvp.Key, Quantity = kvp.Value }).ToList(),
                Notes = "Imported recipe"
            };

            var lowerName = name.ToLowerInvariant();
            var isBlasterLike = lowerName.Contains("blaster")
                || lowerName.Contains("rifle")
                || lowerName.Contains("pistol")
                || lowerName.Contains("cannon");
            if (isBlasterLike)
            {
                var tibanna = "pressurized tibanna";
                if (!Recipes[name].Inputs.Any(x => string.Equals(x.Item, tibanna, StringComparison.OrdinalIgnoreCase)))
                {
                    Recipes[name].Inputs.Add(new RecipeComponent { Item = tibanna, Quantity = 1 });
                }
                Recipes[name].RequiresIndustrialFurnace = true;
            }

            imported++;
        }

        // Re-run blueprint locking so imported recipes obey the same hide rules.
        if (imported > 0) TagAllItemsBlueprintLocked();

        return imported;
    }

    private string? NormalizeImportedMaterialToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var t = token.Trim();
        var lower = t.ToLowerInvariant();

        // Drop reference/citation/textual noise often scraped from wiki pages.
        if (lower.Contains("first appearance") ||
            lower.Contains("sourcebook") ||
            lower.Contains("audiobook") ||
            lower.Contains("wizards.com") ||
            lower.Contains("star wars:") ||
            lower.Contains("episode") ||
            lower.Contains("insider") ||
            lower.Contains("http") ||
            lower.Contains("backup link") ||
            lower.Contains("↑") ||
            lower.Contains("legends"))
        {
            return null;
        }

        if (lower.Contains("kyber")) return "cut kyber crystal";
        if (lower.Contains("bacta")) return "refined bacta";
        if (lower.Contains("coaxium")) return "stabilized coaxium";
        if (lower.Contains("durasteel") || lower.Contains("steel") || lower.Contains("alloy")) return "durasteel ingot";
        if (lower.Contains("power") || lower.Contains("battery") || lower.Contains("cell")) return "power cell";
        if (lower.Contains("ion")) return "ion capacitor core";
        if (lower.Contains("gas") || lower.Contains("tibanna")) return "pressurized tibanna";
        if (lower.Contains("crystal")) return "cut kyber crystal";
        if (lower.Contains("ore") || lower.Contains("metal")) return "raw ore";

        // Unknown tokens are reduced to a generic balanced crafting input.
        return "raw ore";
    }

    public int ImportRacesFromJson(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath)) return 0;

        var raw = File.ReadAllText(jsonFilePath);
        using var doc = JsonDocument.Parse(raw);
        if (doc.RootElement.ValueKind != JsonValueKind.Array) return 0;

        var imported = 0;
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (!element.TryGetProperty("Name", out var nameProp)) continue;
            var name = nameProp.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (name.Length < 2 || name.Length > 70) continue;
            if (Races.ContainsKey(name)) continue;

            var description = element.TryGetProperty("Description", out var descProp)
                ? (descProp.GetString() ?? "A species known throughout the galaxy.").Trim()
                : "A species known throughout the galaxy.";
            if (string.IsNullOrWhiteSpace(description)) description = "A species known throughout the galaxy.";

            var era = InferEraFromRaceText(name, description);
            var skills = InferSkillsFromRaceText(description);
            var stats = GenerateRaceStatsFromName(name, description);

            Races[name] = new RaceData
            {
                Name = name,
                Description = description,
                EraUnlock = era,
                BaseStats = stats,
                StartingSkills = skills
            };
            imported++;
        }

        return imported;
    }

    private Dictionary<string, int> GenerateRaceStatsFromName(string name, string description)
    {
        var seed = Math.Abs(StringComparer.OrdinalIgnoreCase.GetHashCode(name + "|" + description));
        var stats = new Dictionary<string, int>
        {
            ["strength"] = 1 + (seed % 4),
            ["agility"] = 1 + ((seed / 3) % 4),
            ["intellect"] = 1 + ((seed / 5) % 4),
            ["presence"] = 1 + ((seed / 7) % 4),
            ["vitality"] = 1 + ((seed / 11) % 4)
        };

        var desc = description.ToLowerInvariant();
        if (desc.Contains("strong") || desc.Contains("warrior") || desc.Contains("hunter")) stats["strength"] = Math.Min(5, stats["strength"] + 1);
        if (desc.Contains("agile") || desc.Contains("swift") || desc.Contains("fast")) stats["agility"] = Math.Min(5, stats["agility"] + 1);
        if (desc.Contains("intelligent") || desc.Contains("scholar") || desc.Contains("engineer")) stats["intellect"] = Math.Min(5, stats["intellect"] + 1);
        if (desc.Contains("diplomatic") || desc.Contains("charismatic") || desc.Contains("merchant")) stats["presence"] = Math.Min(5, stats["presence"] + 1);
        if (desc.Contains("resilient") || desc.Contains("hardy") || desc.Contains("survive")) stats["vitality"] = Math.Min(5, stats["vitality"] + 1);

        return stats;
    }

    private List<string> InferSkillsFromRaceText(string description)
    {
        var desc = description.ToLowerInvariant();
        var skills = new List<string>();
        if (desc.Contains("pilot") || desc.Contains("navigate")) skills.Add("Piloting");
        if (desc.Contains("trade") || desc.Contains("merchant") || desc.Contains("diplomat")) skills.Add("Negotiation");
        if (desc.Contains("warrior") || desc.Contains("hunter") || desc.Contains("martial")) skills.Add("Brawl");
        if (desc.Contains("force")) skills.Add("Force Discipline");
        if (desc.Contains("stealth") || desc.Contains("shadow")) skills.Add("Stealth");
        if (desc.Contains("tech") || desc.Contains("engineer") || desc.Contains("droid")) skills.Add("Computers");
        if (skills.Count == 0) skills.Add("Perception");
        return skills.Distinct(StringComparer.OrdinalIgnoreCase).Take(2).ToList();
    }

    private string InferEraFromRaceText(string name, string description)
    {
        var text = (name + " " + description).ToLowerInvariant();
        if (text.Contains("first order") || text.Contains("sequel")) return "Sequel Trilogy";
        if (text.Contains("new republic")) return "New Republic";
        if (text.Contains("empire") || text.Contains("imperial")) return "Original Trilogy";
        if (text.Contains("clone") || text.Contains("separat")) return "Clone Wars";
        if (text.Contains("high republic") || text.Contains("old republic") || text.Contains("ancient")) return "Old Republic";
        return "Old Republic";
    }

    public int ImportCreaturesFromJson(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath)) return 0;

        var raw = File.ReadAllText(jsonFilePath);
        using var doc = JsonDocument.Parse(raw);
        if (doc.RootElement.ValueKind != JsonValueKind.Array) return 0;

        var imported = 0;
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (!element.TryGetProperty("Name", out var nameProp)) continue;
            var name = nameProp.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (name.Length < 2 || name.Length > 90) continue;
            if (Creatures.ContainsKey(name)) continue;

            var desc = element.TryGetProperty("Description", out var descProp)
                ? (descProp.GetString() ?? "A dangerous creature from the Star Wars galaxy.").Trim()
                : "A dangerous creature from the Star Wars galaxy.";
            if (string.IsNullOrWhiteSpace(desc)) desc = "A dangerous creature from the Star Wars galaxy.";

            var category = element.TryGetProperty("Category", out var catProp)
                ? (catProp.GetString() ?? "fauna")
                : "fauna";
            var size = element.TryGetProperty("SizeClass", out var sizeProp)
                ? (sizeProp.GetString() ?? "medium")
                : InferCreatureSize(name, desc);
            var habitat = element.TryGetProperty("Habitat", out var habProp)
                ? (habProp.GetString() ?? "general")
                : InferCreatureHabitat(name, desc);
            var danger = element.TryGetProperty("DangerRating", out var dangerProp) && dangerProp.TryGetInt32(out var d)
                ? Math.Clamp(d, 1, 10)
                : InferCreatureDanger(size, desc);
            var source = element.TryGetProperty("SourceUrl", out var srcProp) ? (srcProp.GetString() ?? "") : "";

            Creatures[name] = new CreatureData
            {
                Name = name,
                Description = desc,
                Category = category,
                SizeClass = size,
                Habitat = habitat,
                DangerRating = danger,
                SourceUrl = source
            };
            imported++;
        }

        if (imported > 0)
        {
            RebuildCreatureTerritories();
        }

        return imported;
    }

    public int ImportPlanetsFromJson(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath)) return 0;

        var raw = File.ReadAllText(jsonFilePath);
        var entries = JsonSerializer.Deserialize<List<PlanetImportEntry>>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (entries is null || entries.Count == 0) return 0;

        var imported = 0;
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Name)) continue;
            var name = entry.Name.Trim();
            if (Planets.ContainsKey(name)) continue;

            Planets[name] = new PlanetData
            {
                Name = name,
                Description = string.IsNullOrWhiteSpace(entry.Description) ? "A world somewhere in the galaxy." : entry.Description.Trim(),
                Economy = string.IsNullOrWhiteSpace(entry.Economy) ? "Trade and survival" : entry.Economy.Trim(),
                Region = string.IsNullOrWhiteSpace(entry.Region) ? "Unknown Regions" : entry.Region.Trim(),
                Sector = string.IsNullOrWhiteSpace(entry.Sector) ? "Unknown" : entry.Sector.Trim(),
                Era = string.IsNullOrWhiteSpace(entry.Era) ? "Old Republic" : entry.Era.Trim(),
                ThreatLevel = string.IsNullOrWhiteSpace(entry.ThreatLevel) ? "Moderate" : entry.ThreatLevel.Trim(),
                TravelCost = Math.Clamp(entry.TravelCost, 10, 40),
                HasDockyard = entry.HasDockyard,
                HasIndustrialFurnace = entry.HasIndustrialFurnace,
                DayEvents = entry.DayEvents.Count > 0 ? entry.DayEvents.Distinct(StringComparer.OrdinalIgnoreCase).Take(6).ToList() : new List<string> { "market trade", "survey mission" },
                NightEvents = entry.NightEvents.Count > 0 ? entry.NightEvents.Distinct(StringComparer.OrdinalIgnoreCase).Take(6).ToList() : new List<string> { "night patrol", "shadow exchange" }
            };

            PlanetClocks[name] = new WorldClock();
            imported++;
        }

        if (imported > 0)
        {
            InitializePlanetEconomyStates();
            InitializeIndustrialFurnaces();
            AssignPlanetMaterialSources();
            RebuildCreatureTerritories();
        }

        return imported;
    }

    public int ImportShipArmamentsFromJson(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath)) return 0;

        var raw = File.ReadAllText(jsonFilePath);
        var entries = JsonSerializer.Deserialize<List<ShipArmamentImportEntry>>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (entries is null || entries.Count == 0) return 0;

        var imported = 0;
        foreach (var entry in entries)
        {
            var name = entry.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 120) continue;

            RegisterShipArmament(new ShipArmamentData
            {
                Name = name,
                Category = string.IsNullOrWhiteSpace(entry.Category) ? "laser" : entry.Category.Trim(),
                HardpointSize = NormalizeShipSize(entry.HardpointSize),
                Era = string.IsNullOrWhiteSpace(entry.Era) ? "Old Republic" : entry.Era.Trim(),
                DamageRating = Math.Clamp(entry.DamageRating, 6, 30),
                FuelDraw = Math.Clamp(entry.FuelDraw, 0, 5),
                Description = string.IsNullOrWhiteSpace(entry.Description) ? "Imported ship armament." : entry.Description.Trim(),
                SourceUrl = entry.SourceUrl ?? string.Empty
            });
            imported++;
        }

        if (imported > 0)
        {
            RefreshShipUpgradeRecipes();
            // Re-run blueprint locking so newly imported armament recipes are hidden correctly.
            TagAllItemsBlueprintLocked();
        }

        return imported;
    }

    private bool RequiresIndustrialFurnaceForRaw(string rawMaterial)
    {
        var raw = rawMaterial.ToLowerInvariant();
        return raw.Contains("beskar")
            || raw.Contains("phrik")
            || raw.Contains("neutronium")
            || raw.Contains("coaxium")
            || raw.Contains("carbonite")
            || raw.Contains("ionite");
    }

    private void InitializeInterspeciesRelations()
    {
        interspeciesRelations.Clear();
        var species = Races.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
        for (int i = 0; i < species.Count; i++)
        {
            for (int j = i; j < species.Count; j++)
            {
                var score = species[i].Equals(species[j], StringComparison.OrdinalIgnoreCase)
                    ? 6
                    : ComputeBaseSpeciesRelation(species[i], species[j]);
                interspeciesRelations[SpeciesPairKey(species[i], species[j])] = score;
            }
        }
    }

    private static string SpeciesPairKey(string a, string b)
        => string.Compare(a, b, StringComparison.OrdinalIgnoreCase) <= 0 ? $"{a}|{b}" : $"{b}|{a}";

    private static int ComputeBaseSpeciesRelation(string a, string b)
    {
        var seed = Math.Abs(StringComparer.OrdinalIgnoreCase.GetHashCode($"{a}|{b}"));
        var relation = (seed % 11) - 5;

        var text = (a + " " + b).ToLowerInvariant();
        if (text.Contains("sith") && text.Contains("jedi")) relation -= 2;
        if (text.Contains("mandalorian") && text.Contains("wookiee")) relation += 1;
        if (text.Contains("droid")) relation -= 1;
        if (text.Contains("human")) relation += 1;

        return Math.Clamp(relation, -8, 8);
    }

    public int GetInterspeciesRelation(string sourceSpecies, string targetSpecies)
    {
        var key = SpeciesPairKey(sourceSpecies, targetSpecies);
        if (!interspeciesRelations.TryGetValue(key, out var score))
        {
            score = sourceSpecies.Equals(targetSpecies, StringComparison.OrdinalIgnoreCase)
                ? 6
                : ComputeBaseSpeciesRelation(sourceSpecies, targetSpecies);
            interspeciesRelations[key] = score;
        }
        return score;
    }

    private void PopulateKnownSpeciesRelations(GameCharacter character)
    {
        character.SpeciesRelations.Clear();
        foreach (var race in Races.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).Take(30))
        {
            character.SpeciesRelations[race] = GetInterspeciesRelation(character.Species, race);
        }
    }

    private void ApplySpeciesCharacterVariance(GameCharacter character)
    {
        var seed = Math.Abs(StringComparer.OrdinalIgnoreCase.GetHashCode($"{character.Name}|{character.Species}|{character.Role}|{Clock.Rotation}"));
        var baselineShift = (seed % 7) - 3;

        character.Morale = Math.Clamp(character.Morale + baselineShift * 2, 0, 100);
        character.Stress = Math.Max(0, character.Stress + Math.Max(0, -baselineShift));
        character.Reputation += baselineShift > 0 ? 1 : 0;

        var speciesText = character.Species.ToLowerInvariant();
        if (speciesText.Contains("wookiee") || speciesText.Contains("trandoshan"))
        {
            character.Hp += 6;
            character.MaxHp += 6;
            character.CurrentState = "Ferocious";
        }
        else if (speciesText.Contains("droid"))
        {
            character.Armor += 2;
            character.Stress = Math.Max(0, character.Stress - 1);
            character.CurrentState = "Calculated";
        }
        else if (speciesText.Contains("twi") || speciesText.Contains("chiss") || speciesText.Contains("bothan"))
        {
            character.Morale = Math.Clamp(character.Morale + 5, 0, 100);
            character.CurrentState = "Composed";
        }
        else
        {
            character.CurrentState = baselineShift <= -2 ? "Uneasy" : "Steady";
        }

        if (GetInterspeciesRelation(character.Species, "Human") <= -4)
        {
            character.Reputation -= 1;
            character.Stress += 1;
        }

        character.Condition = character.Hp < character.MaxHp / 2 ? "Wounded" : "Healthy";
    }

    private string SelectEncounterNpcSpecies(string planetName, string zone, string? playerSpecies)
    {
        if (Races.Count == 0) return "Human";

        var pool = Races.Keys.ToList();
        var lowerPlanet = planetName.ToLowerInvariant();
        var lowerZone = zone.ToLowerInvariant();

        if (lowerPlanet.Contains("kashyyyk"))
        {
            pool = pool.Where(s => s.Contains("Wookiee", StringComparison.OrdinalIgnoreCase) || s.Contains("Trandoshan", StringComparison.OrdinalIgnoreCase) || s.Contains("Human", StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else if (lowerPlanet.Contains("ryloth"))
        {
            pool = pool.Where(s => s.Contains("Twi", StringComparison.OrdinalIgnoreCase) || s.Contains("Human", StringComparison.OrdinalIgnoreCase) || s.Contains("Rodian", StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else if (lowerPlanet.Contains("mandalore"))
        {
            pool = pool.Where(s => s.Contains("Mandalorian", StringComparison.OrdinalIgnoreCase) || s.Contains("Human", StringComparison.OrdinalIgnoreCase) || s.Contains("Zabrak", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (lowerZone is "dock" or "market")
        {
            pool = pool.Where(s => !s.Contains("Sith", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (pool.Count == 0) pool = Races.Keys.ToList();
        var pick = pool[random.Next(pool.Count)];

        if (!string.IsNullOrWhiteSpace(playerSpecies) && random.NextDouble() < 0.22)
        {
            pick = playerSpecies;
        }

        return pick;
    }

    private string ComposeEncounterDialogue(string planetName, string zone, string resolvedType, string npcName, string npcSpecies, int speciesRelation, int reputation)
    {
        var relationTone = speciesRelation switch
        {
            >= 5 => "with immediate warmth",
            >= 2 => "with guarded friendliness",
            <= -5 => "with open distrust",
            <= -2 => "with suspicion",
            _ => "with neutral curiosity"
        };

        var reputationTone = reputation switch
        {
            >= 25 => "Your reputation precedes you.",
            >= 10 => "The locals seem aware of your name.",
            <= -15 => "Whispers warn others to keep distance.",
            <= -5 => "People keep one hand near a holster.",
            _ => "You are still a relative unknown here."
        };

        var actionLine = resolvedType switch
        {
            "attack" => $"{npcName}, a {npcSpecies}, steps out in the {zone} of {planetName} and the scene snaps hostile.",
            "merchant" => $"{npcName}, a {npcSpecies} quartermaster, offers trade terms in the {zone}.",
            "scavenger" => $"{npcName}, a {npcSpecies} salvager, shares a cache rumor near the {zone}.",
            "loot" => $"{npcName}, a {npcSpecies} spotter, points at abandoned valuables in the {zone}.",
            _ => $"{npcName}, a {npcSpecies} contact, opens conversation in the {zone} of {planetName}."
        };

        return $"{actionLine} They assess you {relationTone}. {reputationTone}";
    }

    public NpcConversationTurn StartNpcConversation(GameCharacter character, string planetName, string zone)
    {
        var species = SelectEncounterNpcSpecies(planetName, zone, character.Species);
        var name = GenerateNameForRace(species);
        var relation = GetInterspeciesRelation(character.Species, species);
        var trust = Math.Clamp(relation + character.Reputation / 6 + random.Next(-2, 3), -20, 20);

        var session = new NpcConversationSession
        {
            Id = Guid.NewGuid().ToString("N"),
            NpcName = name,
            NpcSpecies = species,
            PlanetName = planetName,
            Zone = zone,
            Trust = trust,
            Mood = MoodFromTrust(trust),
            Turns = 0
        };

        var opener = BuildNpcLine(character, session, "");
        session.Transcript.Add($"{name}: {opener}");
        activeConversations[session.Id] = session;

        return new NpcConversationTurn
        {
            SessionId = session.Id,
            NpcName = session.NpcName,
            NpcSpecies = session.NpcSpecies,
            NpcLine = opener,
            WorldContext = GetWorldRumorSnippet(planetName),
            Mood = session.Mood,
            Trust = session.Trust
        };
    }

    public NpcConversationTurn StartNpcGroupConversation(GameCharacter character, string planetName, string zone)
    {
        var memberCount = 2 + random.Next(0, 2);
        var names = new List<string>();
        var speciesList = new List<string>();

        for (int i = 0; i < memberCount; i++)
        {
            var species = SelectEncounterNpcSpecies(planetName, zone, character.Species);
            speciesList.Add(species);
            names.Add(GenerateNameForRace(species));
        }

        var leadIndex = random.Next(names.Count);
        var leadName = names[leadIndex];
        var leadSpecies = speciesList[leadIndex];
        var relation = GetInterspeciesRelation(character.Species, leadSpecies);
        var trust = Math.Clamp(relation + character.Reputation / 6 + random.Next(-2, 3), -20, 20);

        var session = new NpcConversationSession
        {
            Id = Guid.NewGuid().ToString("N"),
            NpcName = leadName,
            NpcSpecies = leadSpecies,
            IsGroupConversation = true,
            ParticipantNames = names,
            ParticipantSpecies = speciesList,
            PlanetName = planetName,
            Zone = zone,
            Trust = trust,
            Mood = MoodFromTrust(trust),
            Turns = 0
        };

        var opener = BuildNpcLine(character, session, "") + $" Group present: {string.Join(", ", names)}.";
        session.Transcript.Add($"{leadName}: {opener}");
        activeConversations[session.Id] = session;

        return new NpcConversationTurn
        {
            SessionId = session.Id,
            NpcName = leadName,
            NpcSpecies = leadSpecies,
            NpcLine = opener,
            WorldContext = GetWorldRumorSnippet(planetName),
            Mood = session.Mood,
            Trust = session.Trust
        };
    }

    public NpcConversationTurn ContinueNpcConversation(GameCharacter character, string sessionId, string playerMessage)
    {
        if (!activeConversations.TryGetValue(sessionId, out var session))
        {
            return new NpcConversationTurn
            {
                SessionId = sessionId,
                NpcLine = "The contact is gone. Start a new conversation.",
                Mood = "neutral",
                Trust = 0
            };
        }

        var tone = ScorePlayerMessageTone(playerMessage);
        session.Turns += 1;
        session.Trust = Math.Clamp(session.Trust + tone + random.Next(-1, 2) + (character.Reputation >= 20 ? 1 : 0), -25, 25);
        session.Mood = MoodFromTrust(session.Trust);
        session.Transcript.Add($"You: {playerMessage}");

        // ── Work/job keyword detection ─────────────────────────────────────────
        // Player is explicitly asking for work — try to generate a quest offer.
        Quest? questOffer = null;
        string questOfferMsg = "";
        var pLow = playerMessage.ToLowerInvariant();
        bool asksForWork = pLow.Contains("got any work") || pLow.Contains("any work for me")
            || pLow.Contains("looking for work") || pLow.Contains("need work")
            || pLow.Contains("got a job") || pLow.Contains("any jobs") || pLow.Contains("have work")
            || pLow.Contains("looking for someone to help") || pLow.Contains("help out")
            || pLow.Contains("need a hand") || pLow.Contains("hire me") || pLow.Contains("any contracts")
            || pLow.Contains("need someone") || pLow.Contains("looking for a hand")
            || pLow.Contains("got a contract") || pLow.Contains("any jobs around here");
        if (asksForWork && session.Trust >= -5)
        {
            // Force an offer — the player explicitly asked; also raise chance of chain start.
            if (TryOfferNpcQuest(character, session.PlanetName, session.Zone,
                                  session.NpcName, session.NpcSpecies,
                                  out questOffer, out questOfferMsg, forceOffer: true))
            {
                session.Trust = Math.Clamp(session.Trust + 2, -25, 25);
            }
        }

        if (session.ParticipantSpecies.Count > 0)
        {
            foreach (var species in session.ParticipantSpecies)
            {
                ShiftCharacterSpeciesRelation(character, species, tone >= 2 ? 1 : tone <= -2 ? -1 : 0);
            }
        }
        else
        {
            ShiftCharacterSpeciesRelation(character, session.NpcSpecies, tone >= 2 ? 1 : tone <= -2 ? -1 : 0);
        }

        var speakerName = session.NpcName;
        var speakerSpecies = session.NpcSpecies;
        if (session.IsGroupConversation && session.ParticipantNames.Count > 0)
        {
            var idx = random.Next(session.ParticipantNames.Count);
            speakerName = session.ParticipantNames[idx];
            speakerSpecies = idx < session.ParticipantSpecies.Count ? session.ParticipantSpecies[idx] : session.NpcSpecies;
        }

        var reply = BuildNpcLine(character, session, playerMessage);
        session.Transcript.Add($"{speakerName}: {reply}");

        if (session.Trust >= 18)
        {
            character.Reputation += 1;
            character.Notes.Add($"{session.NpcName} shared high-value intel on {session.PlanetName}.");
        }

        var npcReply = questOffer is not null
            ? questOfferMsg   // quest offer overrides generic NPC line
            : reply;

        return new NpcConversationTurn
        {
            SessionId = session.Id,
            NpcName = speakerName,
            NpcSpecies = speakerSpecies,
            NpcLine = npcReply,
            WorldContext = GetWorldRumorSnippet(session.PlanetName),
            Mood = session.Mood,
            Trust = session.Trust,
            QuestOffer = questOffer,
            QuestOfferMessage = questOfferMsg
        };
    }

    public IReadOnlyList<KeyValuePair<string, int>> GetSpeciesRelationshipJournal(GameCharacter character, int take = 40)
    {
        if (character.SpeciesRelations.Count == 0)
        {
            PopulateKnownSpeciesRelations(character);
        }

        return character.SpeciesRelations
            .OrderByDescending(kvp => Math.Abs(kvp.Value))
            .ThenBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(1, take))
            .ToList();
    }

    public IReadOnlyList<MerchantListing> GetMerchantInventory(string planetName, string merchantType = "general")
    {
        RestockMerchantIfNeeded(planetName);
        if (!merchantInventories.TryGetValue(planetName, out var byType)) return Array.Empty<MerchantListing>();
        if (!byType.TryGetValue(merchantType, out var stock)) return Array.Empty<MerchantListing>();
        return stock
            .Where(x => x.Stock > 0)
            .OrderBy(x => x.Price)
            .Select(x => new MerchantListing { ItemName = x.ItemName, Price = x.Price, Stock = x.Stock, Category = x.Category })
            .ToList();
    }

    /// <summary>Returns true if this planet has an accessible black-market vendor.</summary>
    public bool HasBlackMarket(string planetName)
    {
        if (!Planets.TryGetValue(planetName, out var planet)) return false;
        var eco = (planet.Economy + " " + planet.Region + " " + planet.Sector).ToLowerInvariant();
        return eco.Contains("smugg") || eco.Contains("crime") || eco.Contains("black market")
            || eco.Contains("spice") || eco.Contains("pirate")
            || new HashSet<string>(StringComparer.OrdinalIgnoreCase)
               { "Nar Shaddaa", "Tatooine", "Kessel", "Jakku", "Corellia", "Mandalore", "Ryloth", "Nal Hutta" }
               .Contains(planetName);
    }

    public PlanetEconomyStatus GetPlanetEconomyStatus(string planetName)
    {
        if (!PlanetEconomyStates.TryGetValue(planetName, out var state))
        {
            state = new PlanetEconomyStatus { PlanetName = planetName, ResourceLevel = 70, TradeHealth = 60, StatusText = "Stable" };
            PlanetEconomyStates[planetName] = state;
        }
        return state;
    }

    public void SetPlanetEconomicState(string planetName, int resourceLevel, bool imperialExtraction, int tradeHealth)
    {
        var state = GetPlanetEconomyStatus(planetName);
        state.ResourceLevel = Math.Clamp(resourceLevel, 0, 100);
        state.ImperialExtraction = imperialExtraction;
        state.TradeHealth = Math.Clamp(tradeHealth, 0, 100);
        state.StatusText = state.ResourceLevel <= 25
            ? (state.ImperialExtraction ? "Empire Extraction Crisis" : "Resource Scarcity")
            : state.ResourceLevel >= 80
                ? "Prosperous"
                : "Stable";
        merchantRestockRotation[planetName] = Math.Min(merchantRestockRotation.GetValueOrDefault(planetName, Clock.Rotation), Clock.Rotation - 2);
    }

    public string BuyFromMerchant(GameCharacter character, string planetName, string itemName, string merchantType = "general")
    {
        RestockMerchantIfNeeded(planetName);
        if (!merchantInventories.TryGetValue(planetName, out var byType)) return "No merchant inventory available here.";
        if (!byType.TryGetValue(merchantType, out var stock)) return "That merchant type is not available here.";

        var listing = stock.FirstOrDefault(x => string.Equals(x.ItemName, itemName, StringComparison.OrdinalIgnoreCase));
        if (listing is null || listing.Stock <= 0) return "That item is out of stock.";
        if (!CanAccessAsset(character, listing.ItemName, out var accessDeniedReason)) return accessDeniedReason;
        if (character.Credits < listing.Price) return $"You need {listing.Price} credits for {listing.ItemName}.";

        character.Credits -= listing.Price;
        listing.Stock -= 1;
        character.Inventory.Add(listing.ItemName);
        character.Experience += 1;
        return $"Purchased {listing.ItemName} for {listing.Price} credits.";
    }

    private void RestockMerchantIfNeeded(string planetName)
    {
        if (!Planets.ContainsKey(planetName)) return;
        var currentRotation = GetPlanetClock(planetName).Rotation;
        if (!merchantInventories.ContainsKey(planetName))
        {
            merchantInventories[planetName] = BuildAllMerchantStocks(planetName);
            merchantRestockRotation[planetName] = currentRotation;
            return;
        }
        var last = merchantRestockRotation.GetValueOrDefault(planetName, currentRotation);
        if (currentRotation - last >= 2)
        {
            merchantInventories[planetName] = BuildAllMerchantStocks(planetName);
            merchantRestockRotation[planetName] = currentRotation;
        }
    }

    private Dictionary<string, List<MerchantListing>> BuildAllMerchantStocks(string planetName)
    {
        var result = new Dictionary<string, List<MerchantListing>>(StringComparer.OrdinalIgnoreCase)
        {
            ["general"] = BuildGeneralMerchantStock(planetName),
            ["armor"]   = BuildArmorMerchantStock(planetName),
            ["weapons"] = BuildWeaponsMerchantStock(planetName),
        };
        if (HasBlackMarket(planetName))
            result["black_market"] = BuildBlackMarketStock(planetName);
        return result;
    }

    // ── GENERAL MERCHANT (food, med, gear, harvesting tools) ────────────────

    private List<MerchantListing> BuildGeneralMerchantStock(string planetName)
    {
        var planet = Planets[planetName];
        var state  = GetPlanetEconomyStatus(planetName);
        var eco    = (planet.Economy + " " + planet.Region + " " + planet.Sector).ToLowerInvariant();

        var picks = new List<string> { "repair kit", "field medpack", "power cell", "sensor array", "ration bar", "ration pack" };

        if (eco.Contains("agric") || eco.Contains("farming") || eco.Contains("medicine"))
            picks.AddRange(new[] { "healing stim", "refined bacta" });
        if (eco.Contains("luxury") || eco.Contains("tourism") || eco.Contains("trade"))
            picks.Add("cantina meal");
        if (eco.Contains("medicine") || eco.Contains("bacta") || eco.Contains("medical"))
            picks.Add("medicated ration");
        if (eco.Contains("ship") || eco.Contains("manufacturing") || eco.Contains("industry"))
            picks.AddRange(new[] { "hyperdrive part", "shield booster", "armor plating" });
        if (eco.Contains("mining") || eco.Contains("refinery"))
            picks.AddRange(new[] { "durasteel ingot", "ion capacitor core" });

        // Harvesting tools — basic always, advanced by economy
        picks.AddRange(new[] { "mining pick", "woodcutter's axe" });
        if (eco.Contains("mining") || eco.Contains("refinery") || eco.Contains("ore") || eco.Contains("industri"))
            picks.AddRange(new[] { "vibro-pick", "plasma drill" });
        if (eco.Contains("timber") || eco.Contains("forest") || eco.Contains("jungle") || eco.Contains("ecology"))
            picks.AddRange(new[] { "vibro-axe", "arc-cutter" });

        // Legal blaster mods — general tier
        picks.AddRange(new[] { "Targeting Scope", "Extended Barrel", "Custom Grip", "High-Capacity Cell" });

        return BuildStock(picks, eco, state, "misc");
    }

    // ── ARMOR MERCHANT ────────────────────────────────────────────────────────

    private List<MerchantListing> BuildArmorMerchantStock(string planetName)
    {
        var planet = Planets[planetName];
        var state  = GetPlanetEconomyStatus(planetName);
        var eco    = (planet.Economy + " " + planet.Region + " " + planet.Sector).ToLowerInvariant();

        // Pull armor names from the Armors + individual pieces dictionaries
        var armorNames = Armors.Keys.ToList();
        var picks = armorNames.OrderBy(_ => random.Next()).Take(10).ToList();

        // Add backpacks
        picks.AddRange(Backpacks.Keys.OrderBy(_ => random.Next()).Take(3));

        // Specialty armor by economy
        if (eco.Contains("military") || eco.Contains("mandal") || eco.Contains("imperial") || eco.Contains("republic"))
        {
            var heavy = armorNames.Where(a => Armors.TryGetValue(a, out var ar) && ar.ArmorRating >= 5).OrderBy(_ => random.Next()).Take(4).ToList();
            picks.AddRange(heavy);
        }

        return BuildStock(picks, eco, state, "armor");
    }

    // ── WEAPONS MERCHANT ─────────────────────────────────────────────────────

    private List<MerchantListing> BuildWeaponsMerchantStock(string planetName)
    {
        var planet = Planets[planetName];
        var state  = GetPlanetEconomyStatus(planetName);
        var eco    = (planet.Economy + " " + planet.Region + " " + planet.Sector).ToLowerInvariant();

        var picks = Weapons.Keys.OrderBy(_ => random.Next()).Take(8).ToList();

        // Advanced tools and mod tier
        picks.AddRange(new[] { "cortosis excavator", "plasma chainsaw" });
        picks.AddRange(new[] { "Macro-Binocular Scope", "Reinforced Stock", "Bipod",
                                "Targeting Laser", "Electronic Grip", "Ion Cell Conversion",
                                "Plasma Bore Barrel", "Precision Stock", "Combat Grip",
                                "Compressed Gas Cartridge", "Thermal Imaging Scope", "Vibroblade Bayonet" });

        if (eco.Contains("military") || eco.Contains("mandal") || eco.Contains("bounty"))
        {
            var heavy = Weapons.Keys.Where(w => Weapons.TryGetValue(w, out var wb) && wb.Damage >= 12).OrderBy(_ => random.Next()).Take(4).ToList();
            picks.AddRange(heavy);
        }

        return BuildStock(picks, eco, state, "weapon");
    }

    // ── BLACK MARKET ─────────────────────────────────────────────────────────

    private List<MerchantListing> BuildBlackMarketStock(string planetName)
    {
        var planet = Planets[planetName];
        var state  = GetPlanetEconomyStatus(planetName);
        var eco    = (planet.Economy + " " + planet.Region + " " + planet.Sector).ToLowerInvariant();

        var picks = new List<string>
        {
            // Illegal mods
            "Assassin's Scope", "Scatter Barrel", "Sith Ergonomic Grip",
            "Overcharged Power Cell", "Grenade Launcher", "Sith Rangefinder",
            "Military Grade Stock", "Ion Barrel Conversion", "Unstable Crystal Cell",
            // Contraband items
            "smuggler cache", "pressurized tibanna", "thermal detonator",
            "glitterstim residue", "spice trace",
            // Black-market weapons & tools
            "Disruptor Rifle", "cortosis excavator", "plasma chainsaw",
        };

        // Extra illegal weapons by planet
        var illegalWeapons = Weapons.Keys
            .Where(w => Weapons.TryGetValue(w, out var wb) && (wb.Category == "energy" || wb.Damage >= 15))
            .OrderBy(_ => random.Next()).Take(4).ToList();
        picks.AddRange(illegalWeapons);

        return BuildStock(picks, eco, state, "illegal", scarcityMult: 1.4);
    }

    // ── SHARED STOCK BUILDER ──────────────────────────────────────────────────

    private List<MerchantListing> BuildStock(List<string> picks, string eco, PlanetEconomyStatus state,
                                             string defaultCategory, double scarcityMult = 1.0, int maxItems = 18)
    {
        var rawSaleBlocked = state.ImperialExtraction || state.ResourceLevel <= 25;
        if (rawSaleBlocked)
            picks.RemoveAll(item => RawMaterials.Contains(item) || item.StartsWith("raw ", StringComparison.OrdinalIgnoreCase));

        var distinct = picks
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(_ => random.Next())
            .Take(maxItems)
            .ToList();

        double scarcity = eco.Contains("smugg") || eco.Contains("spice") ? 1.15 :
                          eco.Contains("industry") || eco.Contains("manufacturing") ? 0.9 : 1.0;
        scarcity *= scarcityMult;
        if (state.ImperialExtraction) scarcity += 0.18;
        if (state.ResourceLevel <= 25) scarcity += 0.22;
        if (state.ResourceLevel >= 80) scarcity -= 0.08;

        var stock = new List<MerchantListing>();
        foreach (var item in distinct)
        {
            var isWeapon     = Weapons.TryGetValue(item, out var weapon);
            var isFood       = FoodItems.TryGetValue(item, out var food);
            var isHarvestTool= HarvestingTools.TryGetValue(item, out var harvestTool);
            var isBlasterMod = BlasterMods.TryGetValue(item, out var bmod);
            var isArmor      = Armors.TryGetValue(item, out var armor);
            var isBackpack   = Backpacks.TryGetValue(item, out var backpack);
            var blueprint    = CraftableItems.TryGetValue(item, out var bp) ? bp : null;

            var baseCost = isFood        ? food!.BuyPrice
                         : isHarvestTool ? harvestTool!.BuyPrice
                         : isBlasterMod  ? bmod!.Price
                         : isArmor       ? armor!.BaseValue > 0 ? armor.BaseValue : armor.ArmorRating * 40 + 60
                         : isBackpack    ? 120
                         : blueprint?.Cost ?? (isWeapon ? 20 + (weapon?.Damage ?? 0) * 3 : 26);

            var price = Math.Max(6, (int)Math.Round(baseCost * scarcity));
            var baseStockCount = isWeapon || isArmor ? random.Next(1, 3) : random.Next(2, 7);
            var stockCount = Math.Max(1, baseStockCount
                + (state.ResourceLevel <= 25 ? -2 : state.ResourceLevel >= 75 ? 1 : 0)
                + (state.ImperialExtraction ? -1 : 0));

            var category = isFood ? "food" : isWeapon ? "weapon" : isBlasterMod ? "mod"
                         : isArmor ? "armor" : isBackpack ? "armor" : blueprint?.Category ?? defaultCategory;

            stock.Add(new MerchantListing { ItemName = item, Price = price, Stock = stockCount, Category = category });
        }
        return stock.OrderBy(x => x.Price).ToList();
    }

    private void ShiftCharacterSpeciesRelation(GameCharacter character, string targetSpecies, int delta)
    {
        if (delta == 0 || string.IsNullOrWhiteSpace(targetSpecies)) return;
        if (!character.SpeciesRelations.ContainsKey(targetSpecies))
        {
            character.SpeciesRelations[targetSpecies] = GetInterspeciesRelation(character.Species, targetSpecies);
        }

        character.SpeciesRelations[targetSpecies] = Math.Clamp(character.SpeciesRelations[targetSpecies] + delta, -10, 10);
    }

    public IReadOnlyList<string> GetConversationTranscript(string sessionId)
        => activeConversations.TryGetValue(sessionId, out var session) ? session.Transcript : Array.Empty<string>();

    private string BuildNpcLine(GameCharacter character, NpcConversationSession session, string playerMessage)
    {
        var world = GetWorldRumorSnippet(session.PlanetName);
        var mood = session.Mood;
        var relation = GetInterspeciesRelation(character.Species, session.NpcSpecies);
        var era = GetCurrentEraName();
        var playerLower = playerMessage.ToLowerInvariant();
        var topFaction = FactionStandings.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
        var dominantFaction = topFaction.Key ?? "no dominant faction";
        var factionScore = topFaction.Value;

        // --- Keyword-driven contextual replies (checked first) ---
        if (!string.IsNullOrWhiteSpace(playerMessage))
        {
            // Ship / travel topics
            if (playerLower.Contains("ship") || playerLower.Contains("hyperdrive") || playerLower.Contains("travel"))
            {
                var shipLines = new List<string>
                {
                    $"Ships? {session.PlanetName} dockyards have been crawling with {dominantFaction} patrols since the last rotation. Check manifests before you fly.",
                    $"I know a slicer who can wipe a ship registry if the price is right. Not cheap, mind you.",
                    $"Fuel prices in this sector went up fifteen percent. {era} economics hit spacers hardest.",
                    $"The Kessel Run record got broken again. Nobody talks about it openly but half this cantina knows.",
                    $"If you're looking for a new hull, the shipyard here charges above-market during {era} tensions."
                };
                return shipLines[random.Next(shipLines.Count)];
            }

            // Faction / politics topics
            if (playerLower.Contains("empire") || playerLower.Contains("rebel") || playerLower.Contains("republic")
                || playerLower.Contains("faction") || playerLower.Contains("war") || playerLower.Contains("imperial"))
            {
                var factionLines = new List<string>
                {
                    $"Keep that talk quiet. {dominantFaction} informants are thick as flies at this {session.Zone}. Score: {factionScore:+#;-#;0}.",
                    $"The {era} brought nothing but checkpoints and tribute demands. Whole sectors are bleeding credits.",
                    $"I saw a {dominantFaction} garrison double in size overnight here. Something is moving.",
                    $"Political opinions are how people disappear on {session.PlanetName}. I am saying nothing.",
                    $"Resistance cells pass through here sometimes. I do not know their names. That is intentional."
                };
                return factionLines[random.Next(factionLines.Count)];
            }

            // Credits / trade / money topics
            if (playerLower.Contains("credit") || playerLower.Contains("trade") || playerLower.Contains("buy")
                || playerLower.Contains("sell") || playerLower.Contains("money") || playerLower.Contains("pay"))
            {
                var ecoStatus = GetPlanetEconomyStatus(session.PlanetName);
                var tradeLines = new List<string>
                {
                    $"Economy here is {ecoStatus.StatusText.ToLower()}. A {character.Species} with {character.Credits} credits gets polite service, barely.",
                    $"Black market spice routes dried up last rotation. Legitimate trade is all that's left — barely.",
                    $"Durasteel ingots are moving high. If you have a ship and a cargo hold, that is a hint.",
                    $"Somebody flooded the market with fake Republic scrip. Merchants are only taking hard credits.",
                    $"Best rate I know: refinery work pays steady. Risky, but steady."
                };
                return tradeLines[random.Next(tradeLines.Count)];
            }

            // Quest / job / work topics
            if (playerLower.Contains("job") || playerLower.Contains("quest") || playerLower.Contains("mission")
                || playerLower.Contains("work") || playerLower.Contains("hire") || playerLower.Contains("contract"))
            {
                var questLines = new List<string>
                {
                    $"Work? There is always someone who needs a discreet pilot on {session.PlanetName}.",
                    $"Check the dock postings. Half of them are legitimate. The other half pay better.",
                    $"A local contact owes me a favor. He runs salvage out of the {session.Zone}. I can mention your name.",
                    $"I heard the {dominantFaction} is hiring auxiliary scouts. Only fools take that work.",
                    $"If you want clean credits, merchant escort runs through this sector pay well this rotation."
                };
                return questLines[random.Next(questLines.Count)];
            }

            // Creature / wildlife topics
            if (playerLower.Contains("creature") || playerLower.Contains("beast") || playerLower.Contains("hunt")
                || playerLower.Contains("wildlife") || playerLower.Contains("animal"))
            {
                var creatureName = SelectCreatureForPlanet(session.PlanetName, true)?.Name ?? "apex predators";
                var creatureLines = new List<string>
                {
                    $"Wilderness reports mention {creatureName} activity in the outer zones. Do not go alone.",
                    $"Hunters out of {session.PlanetName} have been tracking {creatureName}. Season is brutal right now.",
                    $"The {creatureName} population exploded since {dominantFaction} cleared the garrisons. Nature fills the gap.",
                    $"Bounty board has creature trophies listed. High risk, decent payout."
                };
                return creatureLines[random.Next(creatureLines.Count)];
            }

            // Planet / location topics
            if (playerLower.Contains("planet") || playerLower.Contains("sector") || playerLower.Contains("region")
                || playerLower.Contains(session.PlanetName.ToLowerInvariant()))
            {
                var planetLines = new List<string>
                {
                    $"{session.PlanetName} has seen better rotations. {world}",
                    $"This place has a long memory. The scars of {era} are still visible in the {session.Zone}.",
                    $"I grew up three sectors from here. {session.PlanetName} used to be safe. Used to be.",
                    $"Rumor says a major operation is planned near the outer zones of {session.PlanetName}. Military traffic is up."
                };
                return planetLines[random.Next(planetLines.Count)];
            }

            // Mining / ore / resources topics
            if (playerLower.Contains("mine") || playerLower.Contains("ore") || playerLower.Contains("mineral")
                || playerLower.Contains("dig") || playerLower.Contains("resource"))
            {
                var mineLines = new List<string>
                {
                    $"The mines here run deep. Some shafts go back to the Old Republic.",
                    $"Corusca gem runners used to pass through here. Not anymore — {dominantFaction} claimed the routes.",
                    $"I know a surveyor who mapped a vein nobody has touched. The location will cost you.",
                    $"Mining guilds and faction interests clash here every rotation. Stay out of that mess.",
                    $"Tibanna gas, cortosis, rare ore — if you know where to look, {session.PlanetName} is still generous."
                };
                return mineLines[random.Next(mineLines.Count)];
            }

            // Greeting / small talk / vague
            if (playerLower.Contains("hello") || playerLower.Contains("hey") || playerLower.Contains("hi")
                || playerLower.Contains("greet") || playerLower.Contains("how are"))
            {
                var greetLines = new List<string>
                {
                    $"Not dead yet. That counts for something in the {era}.",
                    $"Surviving. {world}",
                    $"You picked an interesting rotation to show up. Things are moving.",
                    mood == "friendly"
                        ? $"Good to see a {character.Species}. Not many of your kind come through {session.Zone}."
                        : $"Keep one hand on your credits and we can talk."
                };
                return greetLines[random.Next(greetLines.Count)];
            }

            // Threat / intimidation
            if (playerLower.Contains("threat") || playerLower.Contains("kill") || playerLower.Contains("fight")
                || playerLower.Contains("die") || playerLower.Contains("hurt"))
            {
                var threatLines = new List<string>
                {
                    "Half this district has heard worse from better. Lower your voice.",
                    "You pull that thread and you will not like what unravels.",
                    $"I have survived {era} skirmishes. You are not the sharpest threat I have faced today.",
                    "If you are serious, you are in the wrong zone for that conversation."
                };
                return threatLines[random.Next(threatLines.Count)];
            }
        }

        // --- Mood-based fallback replies (opener turn or unrecognized topic) ---
        var repLine = character.Reputation >= 20
            ? "People mention your name with some weight here."
            : character.Reputation <= -10
                ? "Your name draws stares in this zone."
                : "You are still establishing yourself.";

        var fallbackLines = mood switch
        {
            "friendly" => new List<string>
            {
                $"You picked the right booth. {world}",
                $"For a {character.Species}, you carry yourself well. {repLine}",
                $"The {session.Zone} is quiet for now. It never stays that way. {world}",
                $"I like directness. Ask me something specific and I will tell you what I know.",
                $"Between us? {dominantFaction} grip on this sector is weaker than it looks. {repLine}"
            },
            "hostile" => new List<string>
            {
                $"I do not trust this conversation. {world}",
                $"Your species does not earn easy words in this {session.Zone}.",
                $"Keep it short. I have other business and yours does not rank high.",
                $"You want information, you pay. That is how things run here in the {era}.",
                $"One wrong word and this turns into something neither of us planned."
            },
            _ => new List<string>
            {
                $"Depends what you are asking. {world}",
                $"Intel has a cost, and so does silence. {repLine}",
                $"The {session.Zone} reacts to power shifts faster than governments do.",
                $"Ask something specific. I do not give speeches.",
                $"{dominantFaction} is active in this sector. That affects everything you might want to ask about."
            }
        };

        // High-trust bonus intel
        if (session.Turns > 3 && session.Trust >= 10)
        {
            fallbackLines.Add($"Fine — real intel: there is a cache of unregistered cargo moving through the dockside routes this rotation. Grade-A durasteel.");
            fallbackLines.Add($"Off the record: {dominantFaction} has a listening post two systems over. Pilots are reporting unusual vector scans.");
        }

        if (relation <= -5)
            fallbackLines.Add($"Your kind and mine rarely leave talks without blood. Prove this rotation is different.");

        return fallbackLines[random.Next(fallbackLines.Count)];
    }

    private int ScorePlayerMessageTone(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return 0;
        var lower = message.ToLowerInvariant();
        var score = 0;

        if (lower.Contains("please") || lower.Contains("thanks") || lower.Contains("help") || lower.Contains("trade")) score += 2;
        if (lower.Contains("ally") || lower.Contains("peace") || lower.Contains("trust")) score += 2;
        if (lower.Contains("threat") || lower.Contains("kill") || lower.Contains("pay up") || lower.Contains("stupid")) score -= 3;
        if (lower.Contains("lie") || lower.Contains("betray")) score -= 2;

        return Math.Clamp(score, -4, 4);
    }

    private static string MoodFromTrust(int trust)
    {
        if (trust >= 10) return "friendly";
        if (trust <= -8) return "hostile";
        return "neutral";
    }

    private string GetWorldRumorSnippet(string planetName)
    {
        var era = GetCurrentEraName();
        var topFaction = FactionStandings.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
        var clock = GetPlanetClock(planetName);
        var time = clock.TimeOfDay;
        var factionText = string.IsNullOrWhiteSpace(topFaction.Key)
            ? "No faction clearly dominates the street right now."
            : $"{topFaction.Key} influence is surging ({topFaction.Value:+#;-#;0}).";
        var ecoStatus = GetPlanetEconomyStatus(planetName);
        var ecoText = ecoStatus.StatusText switch
        {
            "Prosperous" => "Trade is booming and credits flow freely.",
            "Resource Scarcity" => "Shortages are biting hard — prices are up across the board.",
            "Empire Extraction Crisis" => "Imperial levies have stripped this world to its bones.",
            _ => "The economy creaks along under the weight of galactic events."
        };

        var rumors = new List<string>
        {
            $"{planetName} at {time} (rotation {clock.Rotation}) during the {era}. {factionText}",
            $"The {era} weighs heavy here. {ecoText}",
            $"Word is {topFaction.Key ?? "powerful forces"} are tightening their grip on supply routes out of {planetName}.",
            $"Cantina talk: a Republic-era cache was spotted near the outer {planetName} zones. Nobody has claimed it yet.",
            $"Rotation {clock.Rotation}: patrols are heavier than normal. Something big is moving through this sector."
        };
        return rumors[random.Next(rumors.Count)];
    }

    private string InferCreatureSize(string name, string description)
    {
        var t = (name + " " + description).ToLowerInvariant();
        if (t.Contains("tiny") || t.Contains("small")) return "small";
        if (t.Contains("colossal") || t.Contains("gigantic") || t.Contains("leviathan") || t.Contains("sarlacc") || t.Contains("maw")) return "colossal";
        if (t.Contains("huge") || t.Contains("giant")) return "huge";
        if (t.Contains("large")) return "large";
        return "medium";
    }

    private string InferCreatureHabitat(string name, string description)
    {
        var t = (name + " " + description).ToLowerInvariant();
        if (t.Contains("desert") || t.Contains("dune") || t.Contains("sand")) return "desert";
        if (t.Contains("ice") || t.Contains("snow") || t.Contains("frozen")) return "ice";
        if (t.Contains("swamp") || t.Contains("jungle") || t.Contains("forest")) return "jungle";
        if (t.Contains("space") || t.Contains("vacuum") || t.Contains("asteroid") || t.Contains("nebula")) return "space";
        if (t.Contains("cave") || t.Contains("underground") || t.Contains("pit")) return "cave";
        if (t.Contains("ocean") || t.Contains("sea") || t.Contains("water")) return "ocean";
        return "general";
    }

    private int InferCreatureDanger(string sizeClass, string description)
    {
        var baseDanger = sizeClass.ToLowerInvariant() switch
        {
            "small" => 2,
            "medium" => 4,
            "large" => 6,
            "huge" => 8,
            "colossal" => 10,
            _ => 4
        };

        var d = description.ToLowerInvariant();
        if (d.Contains("apex") || d.Contains("deadly") || d.Contains("predator")) baseDanger++;
        if (d.Contains("docile") || d.Contains("passive")) baseDanger--;
        return Math.Clamp(baseDanger, 1, 10);
    }

    public GameSaveData LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            return new GameSaveData();
        }

        var json = File.ReadAllText(SavePath);
        return JsonSerializer.Deserialize<GameSaveData>(json) ?? new GameSaveData();
    }

    public void ApplySaveData(GameSaveData data)
    {
        NormalizeCharacterProgressionState(data.Character);
        ActiveQuests.Clear();
        foreach (var quest in data.Quests) ActiveQuests.Add(quest);
        FactionStandings.Clear();
        foreach (var entry in data.FactionStandings) FactionStandings[entry.Key] = entry.Value;
        DiscoveredPlanets.Clear();
        foreach (var planet in data.DiscoveredPlanets) DiscoveredPlanets.Add(planet);
        Clock.SetTime(data.Rotation, data.Hour);
        foreach (var entry in data.PlanetRotations)
        {
            if (PlanetClocks.ContainsKey(entry.Key))
            {
                PlanetClocks[entry.Key].SetTime(entry.Value, 8);
            }
        }

        // Restore construction queue
        ConstructionQueue.Clear();
        foreach (var raw in data.ConstructionQueue)
        {
            try
            {
                if (!raw.TryGetValue("owner", out var ownerObj) || !raw.TryGetValue("model", out var modelObj) || !raw.TryGetValue("remainingHours", out var hoursObj)) continue;
                var ownerName = ownerObj?.ToString() ?? "";
                var modelName = modelObj?.ToString() ?? "";
                var remaining = Convert.ToInt32(hoursObj);
                var ownerChar = data.Character is not null && string.Equals(data.Character.Name, ownerName, StringComparison.OrdinalIgnoreCase) ? data.Character : null;
                if (ownerChar is null) continue;
                if (!ShipCatalog.TryGetValue(modelName, out var bp)) continue;
                ConstructionQueue.Add(new ConstructionProject { Owner = ownerChar, Blueprint = bp, RemainingHours = remaining });
            }
            catch
            {
                // ignore malformed entries
            }
        }

        // Restore resource node depletion states
        resourceNodeStates.Clear();
        foreach (var state in data.ResourceNodeStates)
        {
            if (!string.IsNullOrWhiteSpace(state.Key))
                resourceNodeStates[state.Key] = state;
        }
    }

    public string SmeltMaterials(GameCharacter character, int rawCount)
    {
        return RefineMaterial(character, "raw ore", rawCount);
    }

    public string ApplyWeaponMod(GameCharacter character, string weaponName, string modName, bool blackmarket = false)
    {
        if (!character.Inventory.Any(x => string.Equals(x, modName, StringComparison.OrdinalIgnoreCase))) return "You do not have that mod.";
        if (!character.Inventory.Any(x => string.Equals(x, weaponName, StringComparison.OrdinalIgnoreCase))) return "You do not have that weapon to modify.";

        // chance of catastrophic failure for blackmarket mods
        if (blackmarket && random.NextDouble() < 0.12)
        {
            // detonate: remove weapon and mod, damage character
            character.Inventory.RemoveAll(x => string.Equals(x, modName, StringComparison.OrdinalIgnoreCase));
            character.Inventory.RemoveAll(x => string.Equals(x, weaponName, StringComparison.OrdinalIgnoreCase));
            var dmg = Math.Max(6, character.MaxHp / 4);
            character.Hp = Math.Max(0, character.Hp - dmg);
            if (character.Hp == 0) { character.IsAlive = false; character.Condition = "Dead"; }
            return $"The blackmarket mod detonated during installation, destroying your {weaponName} and injuring you for {dmg} HP.";
        }

        // success: mark weapon as modded
        character.Inventory.RemoveAll(x => string.Equals(x, modName, StringComparison.OrdinalIgnoreCase));
        // represent mod as suffix on weapon name when equipped or in inventory
        var newWeaponName = weaponName + " (modded)";
        character.Inventory.RemoveAll(x => string.Equals(x, weaponName, StringComparison.OrdinalIgnoreCase));
        character.Inventory.Add(newWeaponName);
        if (string.Equals(character.EquippedWeapon, weaponName, StringComparison.OrdinalIgnoreCase)) character.EquippedWeapon = newWeaponName;
        character.Experience += 6;
        return $"Successfully installed {modName} on {weaponName}. It's now {newWeaponName}.";
    }

    // ── Blaster Mod Slot System ─────────────────────────────────────────────

    /// <summary>Returns the set of attachment slots this weapon supports. Empty = not moddable.</summary>
    public HashSet<string> GetModSlotsForWeapon(string weaponName)
    {
        var subtype = GetWeaponSubtype(weaponName);
        return subtype switch
        {
            "pistol"          => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Scope", "Barrel", "Grip", "Power Cell" },
            "blaster_rifle"   => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Scope", "Barrel", "Grip", "Stock", "Underbarrel", "Power Cell" },
            "sniper_rifle"    => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Scope", "Barrel", "Grip", "Stock", "Power Cell" },
            "heavy_blaster"   => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Scope", "Barrel", "Grip", "Stock", "Underbarrel", "Power Cell" },
            "rotary_cannon"   => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Barrel", "Stock", "Power Cell" },
            "ion"             => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Scope", "Barrel", "Grip", "Power Cell" },
            "disruptor_rifle" => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Scope", "Barrel", "Grip", "Stock", "Power Cell" },
            "bowcaster"       => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Scope", "Stock", "Power Cell" },
            _                 => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        };
    }

    /// <summary>Installs a mod from the character's inventory into the specified slot of a weapon they own.</summary>
    public string InstallMod(GameCharacter character, string weaponName, string slot, string modName)
    {
        bool hasWeapon = character.Inventory.Any(x => x.Equals(weaponName, StringComparison.OrdinalIgnoreCase))
                      || character.EquippedWeapon.Equals(weaponName, StringComparison.OrdinalIgnoreCase)
                      || character.OffHandWeapon.Equals(weaponName, StringComparison.OrdinalIgnoreCase);
        if (!hasWeapon) return $"You don't have a {weaponName}.";

        if (!character.Inventory.Any(x => x.Equals(modName, StringComparison.OrdinalIgnoreCase)))
            return $"'{modName}' is not in your inventory.";

        var slots = GetModSlotsForWeapon(weaponName);
        if (slots.Count == 0) return $"{weaponName} cannot accept blaster mods.";
        if (!slots.Contains(slot))      return $"{weaponName} has no {slot} slot.";

        if (!BlasterMods.TryGetValue(modName, out var mod)) return $"Unknown mod: {modName}.";
        if (!mod.Slot.Equals(slot, StringComparison.OrdinalIgnoreCase))
            return $"{modName} is a {mod.Slot} mod and cannot go in the {slot} slot.";

        // Explosion risk for illegal high-power mods
        if (mod.HasExplosionRisk && random.NextDouble() < mod.ExplosionChance)
        {
            character.Inventory.RemoveAll(x => x.Equals(modName, StringComparison.OrdinalIgnoreCase));
            var dmg = Math.Max(8, character.MaxHp / 5);
            character.Hp = Math.Max(0, character.Hp - dmg);
            if (character.Hp == 0) { character.IsAlive = false; character.Condition = "Dead"; }
            return $"\u26a0 The {modName} DETONATED during installation! Mod destroyed. You took {dmg} damage.";
        }

        // Swap out any existing mod in the slot
        if (!character.InstalledMods.ContainsKey(weaponName))
            character.InstalledMods[weaponName] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (character.InstalledMods[weaponName].TryGetValue(slot, out var oldMod))
        {
            character.Inventory.Add(oldMod);
            character.InstalledMods[weaponName].Remove(slot);
        }

        character.InstalledMods[weaponName][slot] = modName;
        character.Inventory.RemoveAll(x => x.Equals(modName, StringComparison.OrdinalIgnoreCase));
        character.Experience += 4;

        var bonusTxt = mod.DamageBonus > 0 ? $" (+{mod.DamageBonus} damage)" : "";
        var illTxt   = mod.Category == "Illegal" ? " [ILLEGAL]" : "";
        return $"Installed {modName} in {weaponName}'s {slot} slot.{bonusTxt}{illTxt}";
    }

    /// <summary>Removes a mod from a weapon slot and returns it to inventory.</summary>
    public string RemoveMod(GameCharacter character, string weaponName, string slot)
    {
        if (!character.InstalledMods.TryGetValue(weaponName, out var slotMap)
            || !slotMap.TryGetValue(slot, out var modName))
            return $"No mod in the {slot} slot of {weaponName}.";
        slotMap.Remove(slot);
        if (slotMap.Count == 0) character.InstalledMods.Remove(weaponName);
        character.Inventory.Add(modName);
        return $"Removed {modName} from {weaponName}. Returned to inventory.";
    }

    /// <summary>Returns the total flat damage bonus from all mods installed on a weapon.</summary>
    public int GetWeaponModDamageBonus(GameCharacter character, string weaponName)
    {
        if (string.IsNullOrEmpty(weaponName) || !character.InstalledMods.TryGetValue(weaponName, out var slotMap))
            return 0;
        var total = 0;
        foreach (var mn in slotMap.Values)
            if (BlasterMods.TryGetValue(mn, out var m)) total += m.DamageBonus;
        return total;
    }

    /// <summary>Returns a human-readable summary of all mods on a weapon.</summary>
    public string GetModSummary(GameCharacter character, string weaponName)
    {
        if (!character.InstalledMods.TryGetValue(weaponName, out var slotMap) || slotMap.Count == 0)
            return "No mods installed.";
        var lines = slotMap.Select(kv =>
        {
            var extra = BlasterMods.TryGetValue(kv.Value, out var m) && m.DamageBonus > 0 ? $" +{m.DamageBonus} dmg" : "";
            return $"  {kv.Key}: {kv.Value}{extra}";
        });
        return string.Join("\n", lines);
    }

    private void InitBlasterMods()
    {
        // ── SCOPE ─────────────────────────────────────────────────────────────
        BlasterMods["Targeting Scope"]          = new BlasterMod { Name = "Targeting Scope",          Slot = "Scope",       Category = "Legal",   DamageBonus = 1, Price = 120, Description = "A compact optical scope. Improves target acquisition at range." };
        BlasterMods["Macro-Binocular Scope"]    = new BlasterMod { Name = "Macro-Binocular Scope",    Slot = "Scope",       Category = "Legal",   DamageBonus = 2, Price = 200, Description = "Military-grade magnification. Excellent for medium to long range." };
        BlasterMods["Thermal Imaging Scope"]    = new BlasterMod { Name = "Thermal Imaging Scope",    Slot = "Scope",       Category = "Legal",   DamageBonus = 1, Price = 280, Description = "Detects heat signatures. Effective against droids and armored targets.", SpecialEffect = "Ion Detect" };
        BlasterMods["Assassin's Scope"]         = new BlasterMod { Name = "Assassin's Scope",         Slot = "Scope",       Category = "Illegal", DamageBonus = 3, Price = 400, Description = "A modified rangefinder used by bounty hunters. Banned on most core worlds." };
        BlasterMods["Sith Rangefinder"]         = new BlasterMod { Name = "Sith Rangefinder",         Slot = "Scope",       Category = "Illegal", DamageBonus = 4, Price = 600, Description = "Recovered Sith technology. Pinpoints weak points automatically." };

        // ── BARREL ────────────────────────────────────────────────────────────
        BlasterMods["Extended Barrel"]          = new BlasterMod { Name = "Extended Barrel",          Slot = "Barrel",      Category = "Legal",   DamageBonus = 1, Price = 80,  Description = "Lengthened barrel improves bolt velocity and effective range." };
        BlasterMods["Suppressor Barrel"]        = new BlasterMod { Name = "Suppressor Barrel",        Slot = "Barrel",      Category = "Legal",   DamageBonus = 0, Price = 150, Description = "Dampens shot report. Favored by assassins and infiltrators.", SpecialEffect = "Stealth" };
        BlasterMods["Plasma Bore Barrel"]       = new BlasterMod { Name = "Plasma Bore Barrel",       Slot = "Barrel",      Category = "Crafted", DamageBonus = 2, Price = 220, Description = "Plasma-lined bore that superheats bolts. Armor-piercing effect.", SpecialEffect = "Armor Pierce" };
        BlasterMods["Scatter Barrel"]           = new BlasterMod { Name = "Scatter Barrel",           Slot = "Barrel",      Category = "Illegal", DamageBonus = 2, Price = 260, Description = "Widens bolt dispersion for short-range devastation." };
        BlasterMods["Ion Barrel Conversion"]    = new BlasterMod { Name = "Ion Barrel Conversion",    Slot = "Barrel",      Category = "Illegal", DamageBonus = 1, Price = 350, Description = "Converts output to ionic discharge. Lethal vs droids and electronics.", SpecialEffect = "Ion" };

        // ── GRIP ──────────────────────────────────────────────────────────────
        BlasterMods["Custom Grip"]              = new BlasterMod { Name = "Custom Grip",              Slot = "Grip",        Category = "Legal",   DamageBonus = 1, Price = 60,  Description = "Ergonomic grip reduces weapon sway and improves handling." };
        BlasterMods["Electronic Grip"]          = new BlasterMod { Name = "Electronic Grip",          Slot = "Grip",        Category = "Legal",   DamageBonus = 1, Price = 110, Description = "Grip with built-in trigger assist and recoil feedback." };
        BlasterMods["Combat Grip"]              = new BlasterMod { Name = "Combat Grip",              Slot = "Grip",        Category = "Crafted", DamageBonus = 2, Price = 180, Description = "Cortosis-reinforced grip with full recoil absorption. Allows faster follow-up shots." };
        BlasterMods["Sith Ergonomic Grip"]      = new BlasterMod { Name = "Sith Ergonomic Grip",      Slot = "Grip",        Category = "Illegal", DamageBonus = 3, Price = 320, Description = "Reverse-engineered from Sith weapons. Dark-side-enhanced targeting algorithms." };

        // ── STOCK ─────────────────────────────────────────────────────────────
        BlasterMods["Folding Stock"]            = new BlasterMod { Name = "Folding Stock",            Slot = "Stock",       Category = "Legal",   DamageBonus = 0, Price = 90,  Description = "Lightweight collapsible stock. Improves mobility and draw speed.", SpecialEffect = "Mobility" };
        BlasterMods["Reinforced Stock"]         = new BlasterMod { Name = "Reinforced Stock",         Slot = "Stock",       Category = "Legal",   DamageBonus = 1, Price = 130, Description = "Heavy stock reduces muzzle climb. Can also serve as an emergency bludgeon." };
        BlasterMods["Precision Stock"]          = new BlasterMod { Name = "Precision Stock",          Slot = "Stock",       Category = "Crafted", DamageBonus = 2, Price = 200, Description = "Gyro-stabilized stock engineered for precision long-range fire." };
        BlasterMods["Military Grade Stock"]     = new BlasterMod { Name = "Military Grade Stock",     Slot = "Stock",       Category = "Illegal", DamageBonus = 3, Price = 320, Description = "Surplus military stock salvaged from Stormtrooper rifles. Suppressed signature." };

        // ── UNDERBARREL ───────────────────────────────────────────────────────
        BlasterMods["Bipod"]                    = new BlasterMod { Name = "Bipod",                    Slot = "Underbarrel", Category = "Legal",   DamageBonus = 1, Price = 100, Description = "Deployable bipod for braced, sustained fire. Excellent stability bonus." };
        BlasterMods["Targeting Laser"]          = new BlasterMod { Name = "Targeting Laser",          Slot = "Underbarrel", Category = "Legal",   DamageBonus = 1, Price = 160, Description = "Underbarrel laser sight. Improves shot placement under stress." };
        BlasterMods["Vibroblade Bayonet"]       = new BlasterMod { Name = "Vibroblade Bayonet",       Slot = "Underbarrel", Category = "Crafted", DamageBonus = 2, Price = 250, Description = "Vibroblade attached below the barrel. Enables close-quarters melee strikes.", SpecialEffect = "Melee" };
        BlasterMods["Grenade Launcher"]         = new BlasterMod { Name = "Grenade Launcher",         Slot = "Underbarrel", Category = "Illegal", DamageBonus = 4, Price = 500, Description = "Single-shot micro-grenade launcher. Highly illegal on most civilized worlds." };

        // ── POWER CELL ────────────────────────────────────────────────────────
        BlasterMods["High-Capacity Cell"]       = new BlasterMod { Name = "High-Capacity Cell",       Slot = "Power Cell",  Category = "Legal",   DamageBonus = 1, Price = 80,  Description = "Extended energy cell for sustained fire output." };
        BlasterMods["Ion Cell Conversion"]      = new BlasterMod { Name = "Ion Cell Conversion",      Slot = "Power Cell",  Category = "Legal",   DamageBonus = 1, Price = 180, Description = "Pulsed ion discharge cell. Highly effective against mechanical targets.", SpecialEffect = "Ion" };
        BlasterMods["Compressed Gas Cartridge"] = new BlasterMod { Name = "Compressed Gas Cartridge", Slot = "Power Cell",  Category = "Crafted", DamageBonus = 2, Price = 220, Description = "High-pressure gas propulsion dramatically increases bolt velocity." };
        BlasterMods["Overcharged Power Cell"]   = new BlasterMod { Name = "Overcharged Power Cell",   Slot = "Power Cell",  Category = "Illegal", DamageBonus = 3, Price = 260, HasExplosionRisk = true, ExplosionChance = 0.10, Description = "Dangerously overloaded cell. Massive power. 10% explosion chance on install." };
        BlasterMods["Unstable Crystal Cell"]    = new BlasterMod { Name = "Unstable Crystal Cell",    Slot = "Power Cell",  Category = "Illegal", DamageBonus = 5, Price = 500, HasExplosionRisk = true, ExplosionChance = 0.20, Description = "Forbidden crystal-matrix cell. Extreme output, extreme risk. 20% explosion chance." };
    }
}
