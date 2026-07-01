using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StarWarsRpgCs;

public partial class Form1 : Form
{
    private readonly GameEngine engine = new GameEngine().InitializeCatalogs();
    private GameCharacter? character;
    private StatsWindow? statsWindow;
    private TravelAndEncounterWindow? encounterWindow;
    private InventoryWindow? inventoryWindow;
    private CharacterSheet? characterSheetWindow;
    private CombatWindow? combatWindow;
    private NpcChatWindow? npcChatWindow;
    private SpeciesRelationsWindow? speciesRelationsWindow;
    private MerchantWindow? merchantWindow;
    private TravelWindow? travelWindow;
    private ShipHangarWindow? shipHangarWindow;
    private System.Windows.Forms.Timer worldTimer;
    private int backgroundNewsTicker;

    private readonly ComboBox speciesBox = new();
    private readonly ComboBox roleBox = new();
    private readonly ComboBox homeBox = new();
    private readonly ComboBox currencyBox = new();
    private readonly NumericUpDown ageBox = new();
    // shipBox removed — redundant starter ship picker that did nothing
    private readonly TextBox nameBox = new();
    private readonly TextBox backgroundBox = new();
    private readonly Label statusLabel = new();
    private readonly TextBox outputBox = new();
    private readonly Button createButton = new();
    private readonly Button wipeButton = new();
    private Button jediOrderButton = null!; // initialised in InitLayout
    private Button? romanceButton;           // disabled until a random encounter unlocks a romance target

    public Form1()
    {
        InitializeComponent();
        Text = "Star Wars Open World RPG";
        Size = new Size(1400, 900);
        StartPosition = FormStartPosition.CenterScreen;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var topPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoScroll = true };
        root.Controls.Add(topPanel, 0, 0);

        nameBox.Width = 140; nameBox.Text = "Kira Voss";
        var randomizeNameButton = new Button { Text = "Randomize Name", Width = 120, Height = 28 }; randomizeNameButton.Click += (_, _) => nameBox.Text = engine.GenerateNameForRace(speciesBox.Text); topPanel.Controls.Add(randomizeNameButton);
        speciesBox.Width = 120; speciesBox.DropDownStyle = ComboBoxStyle.DropDownList; speciesBox.Items.AddRange(engine.Races.Keys.ToArray<object>()); speciesBox.SelectedItem = "Human";
        roleBox.Width = 120; roleBox.DropDownStyle = ComboBoxStyle.DropDownList; roleBox.Items.AddRange(new object[] { "Smuggler", "Engineer", "Scout", "Pilot", "Soldier", "Bounty Hunter" }); roleBox.SelectedItem = "Smuggler";
        homeBox.Width = 120; homeBox.DropDownStyle = ComboBoxStyle.DropDownList; homeBox.Items.AddRange(engine.GetStarterHomeworldOptions().ToArray<object>()); homeBox.SelectedItem = homeBox.Items.Contains("Corellia") ? "Corellia" : homeBox.Items.Count > 0 ? homeBox.Items[0] : null;
        backgroundBox.Width = 140; backgroundBox.Text = "Scoundrel";
        ageBox.Width = 80; ageBox.Minimum = 18; ageBox.Maximum = 90; ageBox.Value = 22;
        currencyBox.Width = 140; currencyBox.DropDownStyle = ComboBoxStyle.DropDownList; currencyBox.Items.AddRange(new object[] { "Galactic Credits", "Imperial Credits", "Mandalorian Credits" }); currencyBox.SelectedItem = "Galactic Credits";

        topPanel.Controls.Add(new Label { Text = "Name", AutoSize = true, Margin = new Padding(2) }); topPanel.Controls.Add(nameBox);
        topPanel.Controls.Add(new Label { Text = "Species", AutoSize = true, Margin = new Padding(2) }); topPanel.Controls.Add(speciesBox);
        topPanel.Controls.Add(new Label { Text = "Role", AutoSize = true, Margin = new Padding(2) }); topPanel.Controls.Add(roleBox);
        topPanel.Controls.Add(new Label { Text = "Home", AutoSize = true, Margin = new Padding(2) }); topPanel.Controls.Add(homeBox);
        topPanel.Controls.Add(new Label { Text = "Background", AutoSize = true, Margin = new Padding(2) }); topPanel.Controls.Add(backgroundBox);
        topPanel.Controls.Add(new Label { Text = "Age", AutoSize = true, Margin = new Padding(2) }); topPanel.Controls.Add(ageBox);
        topPanel.Controls.Add(new Label { Text = "Currency", AutoSize = true, Margin = new Padding(2) }); topPanel.Controls.Add(currencyBox);

        createButton.Text = "Create Character"; createButton.Width = 140; createButton.Height = 32; createButton.Click += (_, _) => CreateCharacter(); topPanel.Controls.Add(createButton);
        wipeButton.Text = "Wipe Character"; wipeButton.Width = 120; wipeButton.Height = 32; wipeButton.Click += (_, _) => WipeCharacter(); wipeButton.Visible = false; topPanel.Controls.Add(wipeButton);

        var travelButton = new Button { Text = "Galaxy Travel", Width = 110, Height = 32 }; travelButton.Click += (_, _) => OpenTravelWindow(); topPanel.Controls.Add(travelButton);
        var encounterButton = new Button { Text = "Travel / Explore", Width = 130, Height = 32 }; encounterButton.Click += (_, _) => OpenEncounterWindow(); topPanel.Controls.Add(encounterButton);
        var inventoryButton = new Button { Text = "Inventory", Width = 100, Height = 32 }; inventoryButton.Click += (_, _) => OpenInventoryWindow(); topPanel.Controls.Add(inventoryButton);
        var romanceButton = new Button { Text = "Romance", Width = 90, Height = 32, Enabled = false, BackColor = Color.DimGray, ForeColor = Color.Gray }; romanceButton.Click += (_, _) => OpenRomanceWindow(); topPanel.Controls.Add(romanceButton);
        this.romanceButton = romanceButton;
        var foodButton = new Button { Text = "Eat / Cook", Width = 100, Height = 32 }; foodButton.Click += (_, _) => OpenFoodWindow(); topPanel.Controls.Add(foodButton);
        jediOrderButton = new Button { Text = "Jedi Order", Width = 100, Height = 32, Visible = false }; jediOrderButton.Click += (_, _) => OpenJediOrderWindow(); topPanel.Controls.Add(jediOrderButton);
        var buyHomeBtn = new Button { Text = "Buy Home", Width = 90, Height = 32 }; buyHomeBtn.Click += (_, _) => OpenBuyHomeWindow(); topPanel.Controls.Add(buyHomeBtn);
        var refineryButton = new Button { Text = "Refinery", Width = 90, Height = 32 }; refineryButton.Click += (_, _) => OpenRefineryWindow(); topPanel.Controls.Add(refineryButton);
        var shipButton = new Button { Text = "Ship & Hangar", Width = 110, Height = 32 }; shipButton.Click += (_, _) => OpenShipHangarWindow(); topPanel.Controls.Add(shipButton);
        var queueButton = new Button { Text = "Build Queue", Width = 110, Height = 32 }; queueButton.Click += (_, _) => OpenConstructionQueue(); topPanel.Controls.Add(queueButton);
        var saveButton = new Button { Text = "Save", Width = 90, Height = 32 }; saveButton.Click += (_, _) => SaveGame(); topPanel.Controls.Add(saveButton);
        var loadButton = new Button { Text = "Load", Width = 90, Height = 32 }; loadButton.Click += (_, _) => LoadGame(); topPanel.Controls.Add(loadButton);
        var questButton = new Button { Text = "Quests", Width = 90, Height = 32 }; questButton.Click += (_, _) => Quests(); topPanel.Controls.Add(questButton);
        var factionButton = new Button { Text = "Factions", Width = 100, Height = 32 }; factionButton.Click += (_, _) => Factions(); topPanel.Controls.Add(factionButton);
        var statsButton = new Button { Text = "Detach Stats", Width = 110, Height = 32 }; statsButton.Click += (_, _) => ToggleStatsWindow(); topPanel.Controls.Add(statsButton);
        var reloadDataButton = new Button { Text = "Reload Data", Width = 100, Height = 32 }; reloadDataButton.Click += (_, _) => ReloadAllData(); topPanel.Controls.Add(reloadDataButton);
        var chatButton = new Button { Text = "NPC Chat", Width = 90, Height = 32 }; chatButton.Click += (_, _) => OpenNpcChatWindow(); topPanel.Controls.Add(chatButton);
        var relationsButton = new Button { Text = "Relations", Width = 90, Height = 32 }; relationsButton.Click += (_, _) => OpenSpeciesRelationsWindow(); topPanel.Controls.Add(relationsButton);

        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black };
        root.Controls.Add(panel, 0, 1);

        statusLabel.Dock = DockStyle.Top; statusLabel.ForeColor = Color.Lime; statusLabel.BackColor = Color.Black; statusLabel.Padding = new Padding(8); statusLabel.Text = "No character yet."; panel.Controls.Add(statusLabel);
        outputBox.Dock = DockStyle.Fill; outputBox.Multiline = true; outputBox.ReadOnly = true; outputBox.ScrollBars = ScrollBars.Vertical; outputBox.Font = new Font("Consolas", 11f); outputBox.BackColor = Color.Black; outputBox.ForeColor = Color.Lime; outputBox.Margin = new Padding(0, 30, 0, 0); panel.Controls.Add(outputBox);

        AppendLog("Star Wars Open World RPG initialized. Create a character to begin.");
        ImportRecipes(auto: true);
        ImportShipArmaments(auto: true);
        ImportRaces(auto: true);
        ImportCreatures(auto: true);
        ImportPlanets(auto: true);
        worldTimer = new System.Windows.Forms.Timer();
        worldTimer.Interval = 5000; // advance time every 5 seconds while playing
        worldTimer.Tick += (_, _) => WorldTimer_Tick();
    }

    private void ImportCreatures(bool auto = false)
    {
        var candidates = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "creatures_output.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "StarWarsRpgCs", "data", "creatures_output.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "data", "creatures_output.json")
        };

        var path = candidates.FirstOrDefault(File.Exists);
        if (string.IsNullOrWhiteSpace(path))
        {
            if (!auto) AppendLog("No creatures_output.json found. Run creature crawler first.");
            return;
        }

        var count = engine.ImportCreaturesFromJson(path);
        if (count > 0)
        {
            AppendLog($"Imported {count} creatures from {path}.");
        }
        else if (!auto)
        {
            AppendLog("No new creatures were imported from file.");
        }
    }

    private void ImportShipArmaments(bool auto = false)
    {
        var candidates = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "ship_armaments_output.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "StarWarsRpgCs", "data", "ship_armaments_output.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "data", "ship_armaments_output.json")
        };

        var path = candidates.FirstOrDefault(File.Exists);
        if (string.IsNullOrWhiteSpace(path))
        {
            if (!auto) AppendLog("No ship_armaments_output.json found. Run the armament crawler first.");
            return;
        }

        var count = engine.ImportShipArmamentsFromJson(path);
        if (count > 0)
        {
            AppendLog($"Imported {count} ship armaments from {path}.");
        }
        else if (!auto)
        {
            AppendLog("No new ship armaments were imported from file.");
        }
    }

    private void RefreshSpeciesOptions()
    {
        var previous = speciesBox.SelectedItem?.ToString();
        speciesBox.Items.Clear();
        speciesBox.Items.AddRange(engine.Races.Keys.OrderBy(x => x).ToArray<object>());
        if (!string.IsNullOrWhiteSpace(previous) && speciesBox.Items.Contains(previous))
        {
            speciesBox.SelectedItem = previous;
        }
        else if (speciesBox.Items.Contains("Human"))
        {
            speciesBox.SelectedItem = "Human";
        }
        else if (speciesBox.Items.Count > 0)
        {
            speciesBox.SelectedIndex = 0;
        }
    }

    private void RefreshPlanetOptions()
    {
        var previousHome = homeBox.SelectedItem?.ToString();
        var starterWorlds = engine.GetStarterHomeworldOptions().ToArray<object>();

        homeBox.Items.Clear();
        homeBox.Items.AddRange(starterWorlds);

        if (!string.IsNullOrWhiteSpace(previousHome) && homeBox.Items.Contains(previousHome)) homeBox.SelectedItem = previousHome;
        else if (homeBox.Items.Contains("Corellia")) homeBox.SelectedItem = "Corellia";
        else if (homeBox.Items.Count > 0) homeBox.SelectedIndex = 0;

        travelWindow?.RefreshMap();
    }

    private void ImportRaces(bool auto = false)
    {
        var candidates = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "races_output.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "StarWarsRpgCs", "data", "races_output.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "data", "races_output.json")
        };

        var path = candidates.FirstOrDefault(File.Exists);
        if (string.IsNullOrWhiteSpace(path))
        {
            if (!auto) AppendLog("No races_output.json found. Run race crawler first.");
            return;
        }

        var count = engine.ImportRacesFromJson(path);
        if (count > 0)
        {
            RefreshSpeciesOptions();
            AppendLog($"Imported {count} external races from {path}.");
        }
        else if (!auto)
        {
            AppendLog("No new races were imported from file.");
        }
    }

    private void ImportRecipes(bool auto = false)
    {
        var candidates = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "recipes_output.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "StarWarsRpgCs", "data", "recipes_output.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "data", "recipes_output.json")
        };

        var path = candidates.FirstOrDefault(File.Exists);
        if (string.IsNullOrWhiteSpace(path))
        {
            if (!auto) AppendLog("No recipes_output.json found. Run scraper first.");
            return;
        }

        var count = engine.ImportRecipesFromJson(path);
        if (count > 0)
        {
            AppendLog($"Imported {count} external recipes from {path}.");
        }
        else if (!auto)
        {
            AppendLog("No importable recipes found in file.");
        }
    }

    private void ImportPlanets(bool auto = false)
    {
        var candidates = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "planets_output.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "StarWarsRpgCs", "data", "planets_output.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "data", "planets_output.json")
        };

        var path = candidates.FirstOrDefault(File.Exists);
        if (string.IsNullOrWhiteSpace(path))
        {
            if (!auto) AppendLog("No planets_output.json found. Run planet crawler first.");
            return;
        }

        var count = engine.ImportPlanetsFromJson(path);
        if (count > 0)
        {
            RefreshPlanetOptions();
            AppendLog($"Imported {count} external planets from {path}.");
        }
        else if (!auto)
        {
            AppendLog("No new planets were imported from file.");
        }
    }

    private void WorldTimer_Tick()
    {
        if (character is null) return;
        engine.AdvanceWorldTime(1, character.Location, character);
        ApplyBackgroundPulse("background tick");
        if (character is null) return;

        backgroundNewsTicker++;
        if (backgroundNewsTicker % 2 == 0)
        {
            var updates = engine.ConsumeGalaxyNews(3);
            foreach (var update in updates) AppendLog(update);
        }

        // Survival warning notifications
        if (character.Hunger <= 20 && character.Hunger > 0)
            AppendLog($"⚠ {character.Name} is {engine.GetNutritionState(character).ToLower()}! Eat food soon or suffer stat penalties.");
        if (character.Energy <= 20 && character.Energy > 0)
            AppendLog($"⚠ {character.Name} is {engine.GetEnergyState(character).ToLower()}! Eat food to restore energy and unlock combat abilities.");

        // Jedi awakening check — fires between rotation 20 and 30
        if (engine.ShouldTriggerJediAwakening(character))
        {
            var evt = engine.BuildJediAwakeningEvent(character);
            AppendLog("--- A figure approaches... ---");
            AppendLog(evt.Dialogue);
            var win = new JediAwakeningWindow(this, evt);
            win.Show();
        }

        RefreshStatus();
    }

    private void AppendLog(string message) { outputBox.AppendText(message + Environment.NewLine); outputBox.SelectionStart = outputBox.Text.Length; outputBox.ScrollToCaret(); }

    private void RefreshStatus()
    {
        if (character is null)
        {
            statusLabel.Text = "No character yet.";
            createButton.Visible = true;
            wipeButton.Visible = false;
            return;
        }

        createButton.Visible = false;
        wipeButton.Visible = true;

        var shipLine = character.Ship is null
            ? "No ship"
            : $"{character.Ship.Name} [{character.Ship.SizeClass}] | Hyperdrive C{character.Ship.HyperdriveClass} | Fuel {character.Ship.Fuel}/{character.Ship.MaxFuel} | Hardpoints {engine.GetShipHardpointSummary(character.Ship)}";
        var era = engine.GetCurrentEraName();
        var planetRotation = engine.GetPlanetRotation(character.Location);
        var eco = engine.GetPlanetEconomyStatus(character.Location);
        var facilities = engine.GetPlanetFacilitySummary(character.Location);
        var nutritionState = engine.GetNutritionState(character);
        var energyState = engine.GetEnergyState(character);
        statusLabel.Text = $"Name: {character.Name} | Species: {character.Species} | Location: {character.Location} | Credits: {character.Credits} {character.CurrencyType} | HP: {character.Hp}/{character.MaxHp} | Stamina: {character.Stamina}/{character.MaxStamina} | Hunger: {character.Hunger}/100 ({nutritionState}) | Energy: {character.Energy}/100 ({energyState}) | Reputation: {character.Reputation} | State: {engine.GetStateSummary(character)} | Era: {era} | Rotation: {engine.Clock.Rotation} | Planet Rotation: {planetRotation} | Economy: {eco.StatusText} ({eco.ResourceLevel}) | Facilities: {facilities} | Ship: {shipLine}";
        jediOrderButton.Visible = character.IsForceUser;

        // Enable romance button only when a romance target exists or character is married
        if (romanceButton is not null)
            romanceButton.Enabled = !string.IsNullOrWhiteSpace(character.Family.RomanceTargetName) || character.Family.Married;
        if (romanceButton is not null && romanceButton.Enabled)
        {
            romanceButton.BackColor = Color.MediumVioletRed;
            romanceButton.ForeColor = Color.White;
        }
        statsWindow?.RefreshStats(character, engine);
        if (inventoryWindow is InventoryWindow iw) iw.Refresh();
        characterSheetWindow?.RefreshSheet(character, engine);
    }

    private void CreateCharacter()
    {
        if (character is not null)
        {
            AppendLog("A character already exists. Wipe first to start anew.");
            return;
        }

        var currentEra = engine.GetCurrentEraName();
        var raceName = speciesBox.Text;
        var starterHomes = engine.GetStarterHomeworldOptions();
        var selectedHomeworld = starterHomes.Contains(homeBox.Text, StringComparer.OrdinalIgnoreCase)
            ? homeBox.Text
            : starterHomes.FirstOrDefault() ?? "Corellia";
        if (!engine.IsRaceAvailable(raceName, currentEra, false))
        {
            AppendLog(engine.GetRaceAvailabilityMessage(raceName, currentEra, false));
            raceName = "Human";
        }

        var finalName = string.IsNullOrWhiteSpace(nameBox.Text) ? engine.GenerateNameForRace(raceName) : nameBox.Text;
        character = engine.CreateCharacter(finalName, raceName, roleBox.Text, selectedHomeworld, backgroundBox.Text, (int)ageBox.Value);
        character.CurrencyType = currencyBox.Text;
        engine.MarkPlanetDiscovered(character.Location);
        AppendLog($"Character created: {character.Name}");
        AppendLog($"Species: {character.Species} | Role: {character.Role} | Era: {currentEra}");
        AppendLog($"Stats: STR {character.Stats["strength"]} AGI {character.Stats["agility"]} INT {character.Stats["intellect"]} PRE {character.Stats["presence"]} VIT {character.Stats["vitality"]}");
        AppendLog($"Starting credits: {character.Credits} {character.CurrencyType}");
        AppendLog("No preset quest was assigned. Meet NPCs in encounters to receive procedural contracts.");
        engine.AdvanceWorldTime(6, character.Location, character);
        ApplyBackgroundPulse("creation");
        // start automatic background time progression
        worldTimer?.Start();
        RefreshStatus();
    }

    private void WipeCharacter()
    {
        var result = MessageBox.Show("Wipe your current character and start a new one?", "Confirm wipe", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result != DialogResult.Yes)
        {
            return;
        }

        character = null;
        travelWindow?.Close();
        AppendLog("Your character has been wiped. Create a new one when ready.");
        RefreshStatus();
    }

    private void OpenTravelWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }

        if (travelWindow is null || travelWindow.IsDisposed)
        {
            travelWindow = new TravelWindow(this);
            travelWindow.Show();
            return;
        }

        travelWindow.RefreshMap();
        travelWindow.Activate();
    }

    private void Travel(string? target = null)
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        target = string.IsNullOrWhiteSpace(target) ? character.Location : target;
        if (string.IsNullOrWhiteSpace(target)) { AppendLog("Select a destination from the travel map first."); return; }
        if (string.Equals(target, character.Location, StringComparison.OrdinalIgnoreCase))
        {
            AppendLog($"You are already on {target}.");
            return;
        }
        if (character.Ship is null)
        {
            AppendLog("You need a ship before you can travel between worlds.");
            return;
        }

        if (!engine.TravelTo(character, target))
        {
            AppendLog("Travel failed. Your ship lacks enough fuel or you do not own a ship.");
            RefreshStatus();
            travelWindow?.RefreshMap();
            return;
        }

        var planetState = engine.GetPlanetState(target, engine.Clock.TimeOfDay);
        engine.MarkPlanetDiscovered(target);
        AppendLog($"You travel to {target}.");
        AppendLog($"{planetState["description"]}");
        AppendLog($"Economy: {planetState["economy"]} | EconState: {planetState["economyStatus"]} | Threat: {planetState["threat"]} | Era: {planetState["era"]}");
        AppendLog($"Facilities: {planetState["facilities"]}");
        AppendLog(engine.GenerateWorldEvent(target, engine.Clock.TimeOfDay));
        ApplyBackgroundPulse("travel");
        RefreshStatus();
        travelWindow?.RefreshMap();
    }

    private void OpenEncounterWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        encounterWindow?.Close();
        encounterWindow = new TravelAndEncounterWindow(this);
        encounterWindow.Show();
    }

    private void TravelToStation(string stationName)
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        var result = engine.TravelToStation(character, stationName);
        AppendLog(result.message);
        if (result.success)
        {
            AppendLog(engine.GenerateWorldEvent(stationName, engine.Clock.TimeOfDay));
            ApplyBackgroundPulse("travel");
        }
        RefreshStatus();
        travelWindow?.RefreshMap();
    }

    private void TravelByLiner(string destination)
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        var result = engine.TravelByLiner(character, destination);
        AppendLog(result.message);
        if (result.success)
        {
            AppendLog(engine.GenerateWorldEvent(destination, engine.Clock.TimeOfDay));
            ApplyBackgroundPulse("travel");
        }
        RefreshStatus();
        travelWindow?.RefreshMap();
    }

    private void OpenShipyardWindow(string locationName)
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        var win = new ShipyardWindow(this, locationName);
        win.Show();
    }

    private void OpenInventoryWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        inventoryWindow?.Close();
        inventoryWindow = new InventoryWindow(this);
        inventoryWindow.Show();
    }

    private void HandleEncounterSelection(string zone, string action)
    {
        if (character is null) return;

        // ── Jedi temple actions ───────────────────────────────────────────────
        var jediZones = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Jedi Temple", "Council Chamber", "Archive Library", "Training Hall", "Meditation Garden", "Temple Hangar" };
        if (jediZones.Contains(zone))
        {
            var templeResult = engine.ExecuteJediTempleAction(character, zone, action);
            AppendLog(templeResult);
            ApplyBackgroundPulse("jedi temple");
            RefreshStatus();
            return;
        }

        var encounterType = action switch
        {
            "Visit Merchant"  => "merchant",
            "Scavenge Ruins"  => "scavenger",
            "Talk to Contact" => "contact",
            "Raid Slums"      => "attack",
            "Loot Body"       => "loot",
            "Inspect Dock"    => "merchant",
            "Track Wildlife"  => "attack",
            "Start Long Chat" => "contact",
            "Gather Intel"    => "contact",
            "Mine Resources"  => "mine",
            _ => "contact"
        };

        if (action is "Start Long Chat" or "Gather Intel")
        {
            AppendLog(action == "Gather Intel"
                ? "You quietly gather intelligence from local sources."
                : "You initiate a longer conversation with a local contact.");
            OpenNpcChatWindow();
            return;
        }

        if (action == "Chop Wood")
        {
            var win = new WoodCuttingWindow(this, zone);
            win.Show();
            return;
        }

        if (action == "Mine Resources" || action == "Harvest")
        {
            var win = new MiningWindow(this, zone);
            win.Show();
            return;
        }

        var result = engine.GenerateEncounter(character.Location, zone, character, encounterType);
        AppendLog(result.Summary);

        // Quest context banner — show any active quests that match this planet + zone
        var questCtx = engine.GetQuestEncounterContext(character.Location, zone);
        if (questCtx.Count > 0)
        {
            AppendLog("── Active quest objectives here ─────────────────────────");
            foreach (var (title, objType, chainId) in questCtx)
            {
                var chainPart = string.IsNullOrEmpty(chainId) ? "" : $"  [Chain: {chainId}]";
                AppendLog($"  ★ {title}  ({objType}){chainPart}");
            }
            AppendLog("─────────────────────────────────────────────────────────");
        }

        AppendLog(result.Dialogue);
        if (!string.IsNullOrWhiteSpace(result.NpcName))
        {
            AppendLog($"NPC: {result.NpcName} ({result.NpcSpecies}) | Species relation: {result.SpeciesRelation}");
            if (!result.IsHostile && engine.TryOfferNpcQuest(character, character.Location, zone, result.NpcName, result.NpcSpecies, out var quest, out var questMessage))
            {
                AppendLog($"Quest received: {quest!.Title}");
                AppendLog(questMessage);
                // Show quest encounter popup with zone hint
                var qEvt = engine.BuildQuestEncounterHint(quest);
                var qPop = new QuestEncounterPopup(this, qEvt);
                qPop.Show();
            }
        }
        AppendLog(result.Outcome);
        if (result.RewardCredits > 0) AppendLog($"Reward: {result.RewardCredits} credits.");

        if (encounterType == "loot")
        {
            AppendLog(engine.LootTarget(character, zone));
        }
        else if (encounterType == "merchant")
        {
            AppendLog("Merchant present. Opening market inventory.");
            OpenMerchantWindow();
        }

        if (result.IsHostile)
        {
            AppendLog("This encounter turned hostile. Opening combat window...");
            OpenCombatWindow();
            StartCombat(zone);
        }

        // Random cantina romance spark — fires invisibly in social zones
        if (engine.TrySpawnCantinaRomance(character, zone, out var romEvt) && romEvt is not null)
        {
            AppendLog("─── ♥ ──────────────────────────────────");
            AppendLog(romEvt.OpeningLine);
            AppendLog($"(Open the Romance window to continue courting {romEvt.NpcName}.)");
            AppendLog("────────────────────────────────────────");
        }

        engine.AdvanceWorldTime(4, character.Location, character);
        ApplyBackgroundPulse($"encounter:{zone}");
        RefreshStatus();
    }

    private void OpenCharacterSheet()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        characterSheetWindow?.Close();
        characterSheetWindow = new CharacterSheet(this);
        characterSheetWindow.Show();
    }

    private void OpenConstructionQueue()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        var win = new ConstructionQueueWindow(this);
        win.Show();
    }

    private void SellSelectedItem(string itemName)
    {
        if (character is null) return;
        AppendLog(engine.SellInventoryToMerchant(character, itemName));
        RefreshStatus();
    }

    private void OpenCombatWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        combatWindow?.Close();
        combatWindow = new CombatWindow(this);
        combatWindow.Show();
    }

    private void OpenNpcChatWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        npcChatWindow?.Close();
        npcChatWindow = new NpcChatWindow(this);
        npcChatWindow.Show();
    }

    private void OpenSpeciesRelationsWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        speciesRelationsWindow?.Close();
        speciesRelationsWindow = new SpeciesRelationsWindow(this);
        speciesRelationsWindow.Show();
    }

    private void OpenMerchantWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        merchantWindow?.Close();
        merchantWindow = new MerchantWindow(this);
        merchantWindow.Show();
    }

    internal void OpenShipyardWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        var win = new ShipyardWindow(this);
        win.Show();
    }

    private void HandleCharacterDeathAndReset(string context)
    {
        if (character is null) return;

        AppendLog($"{character.Name} has fallen during {context}.");
        AppendLog("Your run has ended. Create a new character to continue.");

        worldTimer?.Stop();
        combatWindow?.Close();
        encounterWindow?.Close();
        npcChatWindow?.Close();
        merchantWindow?.Close();
        travelWindow?.Close();
        statsWindow?.Close();
        inventoryWindow?.Close();
        characterSheetWindow?.Close();

        character = null;
        RefreshStatus();
    }

    private void StartCombat(string zone)
    {
        if (character is null) return;
        var encounter = engine.CreateCombatEncounter(character, character.Location, zone);
        combatWindow?.SetEncounter(encounter, character, engine);
    }

    private void OpenShipHangarWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        shipHangarWindow?.Close();
        shipHangarWindow = new ShipHangarWindow(this);
        shipHangarWindow.Show();
    }

    private void ReloadAllData()
    {
        ImportRecipes();
        ImportShipArmaments();
        ImportRaces();
        ImportCreatures();
        ImportPlanets();
        AppendLog("All data sources reloaded.");
    }

    private void CraftItem(string itemName)
    {
        if (character is null) return;
        AppendLog(engine.CraftItem(character, itemName));
        ApplyBackgroundPulse("crafting");
        RefreshStatus();
    }

    private void BuildShip(string shipName)
    {
        if (character is null) return;
        var result = engine.CraftShip(character, shipName);
        AppendLog(result.message);
        ApplyBackgroundPulse("shipcraft");
        RefreshStatus();
    }

    private void InstallShipUpgrade(string itemName)
    {
        if (character is null) return;
        AppendLog(engine.ApplyShipUpgrade(character, itemName));
        ApplyBackgroundPulse("ship-upgrade");
        RefreshStatus();
    }

    private void OpenRomanceWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        var win = new RomanceWindow(this);
        win.Show();
    }

    private void OpenFoodWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        var win = new FoodWindow(this);
        win.Show();
    }

    private void OpenArmorWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        var win = new ArmorWindow(this);
        win.Show();
    }

    private void OpenBuyHomeWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        if (character.Home != null)
        {
            var res = MessageBox.Show(
                $"You own: {character.Home.Name} on {character.Home.Planet}\n" +
                $"Storage: {character.Home.StorageItems.Count}/{character.Home.StorageCapacityScu} SCU\n\n" +
                "Sell this property for half the purchase price?",
                "Your Home", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes)
            {
                var (ok, msg) = engine.SellHouse(character);
                AppendLog(msg);
                RefreshStatus();
            }
            return;
        }
        var listings = engine.GetHouseListingsAtLocation(character.Location);
        if (listings.Count == 0)
        {
            AppendLog($"No properties for sale on {character.Location}.");
            return;
        }
        using var win = new Form
        {
            Text = $"Real Estate – {character.Location}", Width = 460, Height = 420,
            BackColor = Color.FromArgb(14, 14, 24), FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.Manual, Location = new Point(Location.X + 100, Location.Y + 80)
        };
        var lb = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 20, 34), ForeColor = Color.White, Font = new Font("Segoe UI", 9f) };
        foreach (var l in listings) lb.Items.Add($"{l.Name}  —  {l.Price:N0} cr  |  {l.StorageCapacityScu} SCU storage");
        var info = new Label { Dock = DockStyle.Bottom, AutoSize = false, Height = 60, BackColor = Color.FromArgb(12, 12, 20), ForeColor = Color.LightGray };
        lb.SelectedIndexChanged += (_, _) =>
        {
            if (lb.SelectedIndex >= 0 && lb.SelectedIndex < listings.Count)
                info.Text = listings[lb.SelectedIndex].Description;
        };
        var buyBtn = new Button { Text = "Buy", Dock = DockStyle.Bottom, Height = 34, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        buyBtn.Click += (_, _) =>
        {
            if (lb.SelectedIndex < 0) return;
            var (ok, msg) = engine.BuyHouse(character, listings[lb.SelectedIndex].Name);
            AppendLog(msg);
            if (ok) { RefreshStatus(); win.Close(); }
        };
        win.Controls.AddRange(new Control[] { lb, info, buyBtn });
        win.ShowDialog(this);
    }

    private void OpenRefineryWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        var win = new RefineryWindow(this);
        win.Show();
    }

    private void OpenJediOrderWindow()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        if (!character.IsForceUser) { AppendLog("You are not attuned to the Force."); return; }
        var win = new JediOrderWindow(this);
        win.Show();
    }

    private void AdvanceTime()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        engine.AdvanceWorldTime(12, character.Location, character);
        AppendLog($"Time advanced to rotation {engine.Clock.Rotation}, hour {engine.Clock.Hour} ({engine.Clock.TimeOfDay}).");
        ApplyBackgroundPulse("time");
        RefreshStatus();
    }

    private void SaveGame()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        engine.SaveGame(character);
        AppendLog($"Game saved to {engine.SavePath}");
    }

    private void LoadGame()
    {
        var saveData = engine.LoadGame();
        if (saveData.Character is null) { AppendLog("No save file was found."); return; }
        character = saveData.Character;
        engine.ApplySaveData(saveData);
        AppendLog($"Loaded {character.Name} from {engine.SavePath}");
        RefreshStatus();
    }

    private void Quests()
    {
        if (character is null) { AppendLog("Create a character first."); return; }
        var progressUpdates = engine.RefreshQuestProgress(character, "quest board");
        foreach (var update in progressUpdates)
        {
            AppendLog(update);
        }

        var readyQuest = engine.ActiveQuests.FirstOrDefault(q => q.Status == "ready");
        if (readyQuest is not null)
        {
            AppendLog(engine.CompleteQuest(character, readyQuest.Id));
        }
        else
        {
            AppendLog(engine.GetQuestJournal(character));
        }

        ApplyBackgroundPulse("quest");
        RefreshStatus();
    }

    private void Factions()
    {
        AppendLog("Faction standing:");
        AppendLog(engine.GetFactionSummary());
        ApplyBackgroundPulse("faction report");
    }

    private void ApplyBackgroundPulse(string context)
    {
        if (character is null) return;
        var pulses = engine.SimulateBackgroundTurn(character, context);
        foreach (var pulse in pulses) AppendLog(pulse);

        if (engine.TryTriggerLatentForceEncounter(character, out var seekerFaction, out var message))
        {
            AppendLog("A mysterious Force-related encounter unfolds.");
            var choice = MessageBox.Show(message, "Latent Force Discovery", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            var accepted = choice == DialogResult.Yes;
            AppendLog(engine.ResolveLatentForceChoice(character, accepted, seekerFaction));
        }

        if (!character.IsAlive || character.Hp <= 0)
        {
            HandleCharacterDeathAndReset(context);
            return;
        }
        RefreshStatus();
    }

    private void ToggleStatsWindow()
    {
        if (statsWindow is null || statsWindow.IsDisposed)
        {
            statsWindow = new StatsWindow(this);
            statsWindow.Show();
        }
        else
        {
            statsWindow.Activate();
        }
        RefreshStatus();
    }

    private sealed class TravelWindow : Form
    {
        private static readonly string[] GroupOrder =
        {
            "Core Worlds", "Colonies", "Expansion Region", "Inner Rim", "Mid Rim",
            "Outer Rim", "Corporate Sector", "Hutt Space", "Chiss Ascendancy",
            "Wild Space", "Unknown Regions", "Space Stations"
        };

        private readonly Form1 owner;
        private readonly TreeView destinationTree = new();
        private readonly TextBox detailsBox = new();
        private readonly Label headerLabel = new();
        private readonly Button travelButton = new();
        private readonly Button linerButton = new();
        private readonly Button shipyardButton = new();
        private string? selectedDestination;
        private bool selectedIsStation;

        public TravelWindow(Form1 owner)
        {
            this.owner = owner;
            Text = "Galaxy Travel Map";
            Size = new Size(980, 680);
            MinimumSize = new Size(700, 500);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 180, owner.Location.Y + 120);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Padding = new Padding(10), AutoSize = true };
            headerLabel.AutoSize = true;
            headerLabel.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            top.Controls.Add(headerLabel);

            travelButton.Text = "Jump to Selected";
            travelButton.Width = 150; travelButton.Height = 34;
            travelButton.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(selectedDestination)) return;
                if (selectedIsStation) owner.TravelToStation(selectedDestination);
                else owner.Travel(selectedDestination);
                RefreshMap();
            };

            linerButton.Text = "Book Liner";
            linerButton.Width = 130; linerButton.Height = 34;
            linerButton.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(selectedDestination)) return;
                owner.TravelByLiner(selectedDestination);
                RefreshMap();
            };

            shipyardButton.Text = "Visit Shipyard";
            shipyardButton.Width = 130; shipyardButton.Height = 34;
            shipyardButton.Visible = false;
            shipyardButton.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(selectedDestination)) return;
                owner.OpenShipyardWindow(selectedDestination);
            };

            var closeButton = new Button { Text = "Close", Width = 90, Height = 34 };
            closeButton.Click += (_, _) => Close();
            top.Controls.Add(travelButton);
            top.Controls.Add(linerButton);
            top.Controls.Add(shipyardButton);
            top.Controls.Add(closeButton);
            layout.Controls.Add(top, 0, 0);

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 340 };
            layout.Controls.Add(split, 0, 1);

            destinationTree.Dock = DockStyle.Fill;
            destinationTree.HideSelection = false;
            destinationTree.Font = new Font("Segoe UI", 10f);
            destinationTree.AfterSelect += (_, e) => HandleSelectionChanged(e.Node);
            split.Panel1.Controls.Add(destinationTree);

            detailsBox.Dock = DockStyle.Fill;
            detailsBox.Multiline = true; detailsBox.ReadOnly = true;
            detailsBox.ScrollBars = ScrollBars.Vertical;
            detailsBox.Font = new Font("Consolas", 10f);
            split.Panel2.Controls.Add(detailsBox);

            RefreshMap();
        }

        public void RefreshMap()
        {
            var currentLoc = owner.character?.Location ?? string.Empty;
            headerLabel.Text = owner.character is null
                ? "Create a character to chart routes."
                : $"Current: {owner.character.Location} | Ship: {(owner.character.Ship is null ? "None — use liner" : owner.character.Ship.Name)}";

            var preserveSelection = selectedDestination;
            destinationTree.BeginUpdate();
            destinationTree.Nodes.Clear();

            var groupedPlanets = owner.engine.Planets.Values
                .OrderBy(p => GetGroupOrderIndex(owner.engine.GetTravelRegionGroup(p.Name)))
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .GroupBy(p => owner.engine.GetTravelRegionGroup(p.Name));

            foreach (var group in groupedPlanets)
            {
                var groupNode = new TreeNode($"{group.Key} ({group.Count()})");
                foreach (var planet in group.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var here = string.Equals(planet.Name, currentLoc, StringComparison.OrdinalIgnoreCase);
                    var suffix = here ? " [Here]" : (planet.HasDockyard ? " ⚙" : "");
                    var pNode = new TreeNode(planet.Name + suffix) { Tag = planet.Name };
                    groupNode.Nodes.Add(pNode);
                    foreach (var stName in planet.OrbitalStations)
                    {
                        if (!owner.engine.SpaceStations.ContainsKey(stName)) continue;
                        var stHere = string.Equals(stName, currentLoc, StringComparison.OrdinalIgnoreCase);
                        var stNode = new TreeNode("  ◎ " + stName + (stHere ? " [Here]" : ""))
                            { Tag = stName, ForeColor = Color.SteelBlue };
                        pNode.Nodes.Add(stNode);
                    }
                }
                destinationTree.Nodes.Add(groupNode);
            }

            var allOrbitals = owner.engine.Planets.Values.SelectMany(p => p.OrbitalStations)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var standalone = owner.engine.SpaceStations.Values
                .Where(s => !allOrbitals.Contains(s.Name)).OrderBy(s => s.Name).ToList();
            if (standalone.Count > 0)
            {
                var stNode = new TreeNode($"Space Stations ({standalone.Count})");
                foreach (var s in standalone)
                {
                    var here = string.Equals(s.Name, currentLoc, StringComparison.OrdinalIgnoreCase);
                    stNode.Nodes.Add(new TreeNode("◎ " + s.Name + (here ? " [Here]" : ""))
                        { Tag = s.Name, ForeColor = Color.SteelBlue });
                }
                destinationTree.Nodes.Add(stNode);
            }

            destinationTree.EndUpdate();
            destinationTree.ExpandAll();

            var target = preserveSelection ?? currentLoc;
            if (!string.IsNullOrWhiteSpace(target) && TrySelectDestNode(target)) return;
            if (destinationTree.Nodes.Count > 0)
            { destinationTree.SelectedNode = destinationTree.Nodes[0]; HandleSelectionChanged(destinationTree.SelectedNode); }
        }

        private void HandleSelectionChanged(TreeNode? node)
        {
            if (node?.Tag is not string dest)
            {
                selectedDestination = null; travelButton.Enabled = false;
                linerButton.Enabled = false; shipyardButton.Visible = false;
                detailsBox.Text = "Select a world or station to inspect route details.";
                return;
            }
            selectedDestination = dest;
            var isCurrent = string.Equals(owner.character?.Location, dest, StringComparison.OrdinalIgnoreCase);
            var sb = new StringBuilder();

            if (owner.engine.SpaceStations.TryGetValue(dest, out var st))
            {
                selectedIsStation = true;
                sb.AppendLine(st.Name);
                sb.AppendLine(new string('=', st.Name.Length));
                sb.AppendLine($"Type: Orbital Space Station");
                sb.AppendLine($"Orbiting: {st.OrbitingPlanet}  |  Region: {st.Region}");
                sb.AppendLine($"Manufacturer: {st.Manufacturer}");
                sb.AppendLine($"Shipyard: {(st.HasShipyard ? "Yes" : "No")}  |  Merchant: {(st.HasMerchant ? "Yes" : "No")}");
                sb.AppendLine($"Liner Fare: {st.LinerFeeFromGalaxy} credits");
                if (owner.character?.Ship is not null) sb.AppendLine($"Fuel hop cost: ~{st.TravelCostFromPlanet} fuel");
                sb.AppendLine(); sb.AppendLine(st.Description);

                travelButton.Enabled = owner.character?.Ship is not null && !isCurrent;
                travelButton.Text = isCurrent ? "Already Docked" : (owner.character?.Ship is null ? "Ship Required" : "Jump to Station");
                linerButton.Enabled = !isCurrent;
                linerButton.Text = $"Liner ({st.LinerFeeFromGalaxy}cr)";
                shipyardButton.Visible = st.HasShipyard;
                shipyardButton.Enabled = isCurrent;
            }
            else if (owner.engine.Planets.TryGetValue(dest, out var planet))
            {
                selectedIsStation = false;
                var eco = owner.engine.GetPlanetEconomyStatus(dest);
                var clock = owner.engine.GetPlanetClock(dest);
                var facilities = owner.engine.GetPlanetFacilitySummary(dest);
                var linerFare = Math.Max(80, planet.TravelCost * 8);
                sb.AppendLine(dest); sb.AppendLine(new string('=', dest.Length));
                sb.AppendLine($"Group: {owner.engine.GetTravelRegionGroup(dest)}  |  Region: {planet.Region}  |  Sector: {planet.Sector}");
                sb.AppendLine($"Era: {planet.Era}  |  Threat: {planet.ThreatLevel}");
                sb.AppendLine($"Travel Cost: {planet.TravelCost} fuel  |  Liner Fare: ~{linerFare} credits");
                sb.AppendLine($"Local Time: Rotation {clock.Rotation}, Hour {clock.Hour} ({clock.TimeOfDay})");
                sb.AppendLine($"Economy: {planet.Economy}  ({eco.StatusText}, Rsrc {eco.ResourceLevel})");
                sb.AppendLine($"Facilities: {facilities}");
                if (!string.IsNullOrWhiteSpace(planet.ShipyardName))
                    sb.AppendLine($"Shipyard: {planet.ShipyardName} [{planet.ShipyardManufacturer}]");
                if (planet.OrbitalStations.Count > 0)
                    sb.AppendLine($"Orbital Stations: {string.Join(", ", planet.OrbitalStations)}");
                sb.AppendLine($"Discovered: {(owner.engine.DiscoveredPlanets.Contains(dest) ? "Yes" : "No")}");
                if (owner.character?.Ship is not null)
                {
                    sb.AppendLine($"Your Fuel: {owner.character.Ship.Fuel}/{owner.character.Ship.MaxFuel}");
                    sb.AppendLine($"Jump Cost: {owner.engine.GetTravelFuelCost(owner.character, dest)} fuel  |  ETA: {owner.engine.GetTravelHours(owner.character, dest)}h");
                }
                sb.AppendLine(); sb.AppendLine(planet.Description);

                travelButton.Enabled = owner.character?.Ship is not null && !isCurrent;
                travelButton.Text = isCurrent ? "Already Here" : (owner.character?.Ship is null ? "Ship Required" : "Jump to World");
                linerButton.Enabled = !isCurrent;
                linerButton.Text = $"Liner (~{linerFare}cr)";
                shipyardButton.Visible = planet.HasDockyard;
                shipyardButton.Enabled = isCurrent;
            }
            else { selectedDestination = null; travelButton.Enabled = false; linerButton.Enabled = false; shipyardButton.Visible = false; }

            detailsBox.Text = sb.ToString();
        }

        private bool TrySelectDestNode(string name)
        {
            foreach (TreeNode g in destinationTree.Nodes)
                if (FindRecursive(g, name, out var found)) { destinationTree.SelectedNode = found; HandleSelectionChanged(found); return true; }
            return false;
        }

        private static bool FindRecursive(TreeNode parent, string name, out TreeNode? found)
        {
            foreach (TreeNode c in parent.Nodes)
            {
                if (c.Tag is string s && string.Equals(s, name, StringComparison.OrdinalIgnoreCase)) { found = c; return true; }
                if (FindRecursive(c, name, out found)) return true;
            }
            found = null; return false;
        }

        private static int GetGroupOrderIndex(string groupName)
        {
            var idx = Array.FindIndex(GroupOrder, x => x.Equals(groupName, StringComparison.OrdinalIgnoreCase));
            return idx >= 0 ? idx : GroupOrder.Length;
        }
    }

    private sealed class StatsWindow : Form
    {
        private readonly Label contentLabel = new();
        public StatsWindow(Form1 owner)
        {
            Text = "Character & Ship Stats";
            Size = new Size(520, 460);
            MinimumSize = new Size(420, 360);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 80, owner.Location.Y + 80);
            BackColor = Color.FromArgb(20, 20, 20);
            contentLabel.Dock = DockStyle.Fill;
            contentLabel.AutoSize = false;
            contentLabel.Padding = new Padding(12);
            contentLabel.Font = new Font("Segoe UI", 10f);
            contentLabel.ForeColor = Color.White;
            contentLabel.TextAlign = ContentAlignment.TopLeft;
            contentLabel.MaximumSize = new Size(500, 0);
            Controls.Add(contentLabel);
        }

        public void RefreshStats(GameCharacter character, GameEngine engine)
        {
            var shipText = character.Ship is null ? "No ship" : $"Ship: {character.Ship.Name} ({character.Ship.Model})\nSize: {character.Ship.SizeClass}\nHyperdrive: Class {character.Ship.HyperdriveClass}\nHull: {character.Ship.Hull}\nShield: {character.Ship.Shield}\nFuel: {character.Ship.Fuel}/{character.Ship.MaxFuel}\nWeapon: {character.Ship.Weapon}\nHardpoints: {engine.GetShipHardpointSummary(character.Ship)}\nArmaments: {(character.Ship.Armaments.Count == 0 ? "Stock" : string.Join(", ", character.Ship.Armaments))}\nParts: {string.Join(", ", character.Ship.Parts)}\nUpgrades: {string.Join(", ", character.Ship.Upgrades)}";
            contentLabel.Text = $"Character Overview\n\nName: {character.Name}\nSpecies: {character.Species}\nRole: {character.Role}\nHomeworld: {character.Homeworld}\nAge: {character.Age}\nCurrency: {character.Credits} {character.CurrencyType}\nHP: {character.Hp}/{character.MaxHp}\nStamina: {character.Stamina}/{character.MaxStamina}\nArmor: {character.Armor}\nReputation: {character.Reputation}\nState: {engine.GetStateSummary(character)}\nCondition: {character.Condition}\nStress: {character.Stress}\nMorale: {character.Morale}\nExperience: {character.Experience}\nEquipped Weapon: {character.EquippedWeapon}\nSkills: {string.Join(", ", character.Skills)}\n\n{shipText}\n\nEra: {engine.GetCurrentEraName()}\nRotation: {engine.Clock.Rotation}";
        }
    }

    private sealed class CharacterSheet : Form
    {
        private readonly Form1 owner;
        private readonly ListBox skillsBox = new();
        private readonly ListBox invBox = new();
        private readonly Label infoLabel = new();

        public CharacterSheet(Form1 owner)
        {
            this.owner = owner;
            Text = "Character Sheet";
            Size = new Size(560, 520);
            MinimumSize = new Size(480, 420);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 240, owner.Location.Y + 160);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 80));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(layout);

            skillsBox.Dock = DockStyle.Fill; skillsBox.Font = new Font("Segoe UI", 10f); layout.Controls.Add(skillsBox, 0, 0);
            invBox.Dock = DockStyle.Fill; invBox.Font = new Font("Segoe UI", 10f); layout.Controls.Add(invBox, 1, 0);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Padding = new Padding(8) };
            var equipButton = new Button { Text = "Equip Selected", Width = 120, Height = 32 }; equipButton.Click += (_, _) =>
            {
                if (owner.character is null) return;
                if (invBox.SelectedItem is string it)
                {
                    var msg = owner.engine.EquipItem(owner.character, it);
                    owner.AppendLog(msg);
                    RefreshSheet(owner.character, owner.engine);
                }
            };
            var closeButton = new Button { Text = "Close", Width = 90, Height = 32 }; closeButton.Click += (_, _) => Close();
            var applyModButton = new Button { Text = "Apply Mod", Width = 120, Height = 32 };
            applyModButton.Click += (_, _) =>
            {
                if (owner.character is null) return;
                if (invBox.SelectedItem is string it)
                {
                    // attempt to apply any blackmarket mod in inventory to selected weapon
                    if (!owner.character.Inventory.Any(x => string.Equals(x, "blackmarket weapon mod", StringComparison.OrdinalIgnoreCase))) { owner.AppendLog("No blackmarket mod found in inventory."); return; }
                    var msg = owner.engine.ApplyWeaponMod(owner.character, it, "blackmarket weapon mod", true);
                    owner.AppendLog(msg);
                    RefreshSheet(owner.character, owner.engine);
                }
            };
            actions.Controls.Add(equipButton); actions.Controls.Add(applyModButton); actions.Controls.Add(closeButton); actions.Controls.Add(infoLabel);
            layout.Controls.Add(actions, 0, 1);
        }

        public void RefreshSheet(GameCharacter character, GameEngine engine)
        {
            skillsBox.Items.Clear();
            foreach (var s in character.Skills) skillsBox.Items.Add(s);
            invBox.Items.Clear();
            foreach (var it in character.Inventory.OrderBy(x => x)) invBox.Items.Add(it);
            infoLabel.Text = $"Equipped: {character.EquippedWeapon} | Armor: {character.Armor} | Credits: {character.Credits} {character.CurrencyType}";
            infoLabel.AutoSize = true;
        }
    }

    private sealed class ConstructionQueueWindow : Form
    {
        private readonly Form1 owner;
        private readonly ListBox queueBox = new();

        public ConstructionQueueWindow(Form1 owner)
        {
            this.owner = owner;
            Text = "Construction Queue";
            Size = new Size(520, 400);
            MinimumSize = new Size(420, 300);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 260, owner.Location.Y + 160);

            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(panel);

            queueBox.Font = new Font("Segoe UI", 10f);
            queueBox.Dock = DockStyle.Fill;
            panel.Controls.Add(queueBox, 0, 0);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Padding = new Padding(8) };
            var refresh = new Button { Text = "Refresh", Width = 90, Height = 32 }; refresh.Click += (_, _) => RefreshQueue();
            var cancel = new Button { Text = "Cancel Selected", Width = 140, Height = 32 }; cancel.Click += (_, _) => { if (queueBox.SelectedItem is string s) CancelSelected(s); };
            var fast = new Button { Text = "Fast-Track", Width = 110, Height = 32 }; fast.Click += (_, _) => { if (queueBox.SelectedItem is string s) FastTrack(s); };
            actions.Controls.Add(refresh); actions.Controls.Add(cancel); actions.Controls.Add(fast);
            panel.Controls.Add(actions, 0, 1);

            RefreshQueue();
        }

        public void RefreshQueue()
        {
            queueBox.Items.Clear();
            foreach (var proj in owner.engine.ConstructionQueue)
            {
                queueBox.Items.Add($"Owner: {proj.Owner.Name} | Ship: {proj.Blueprint.Name} | RemainingHours: {proj.RemainingHours}");
            }
        }

        private void CancelSelected(string text)
        {
            // parse to find model
            var parts = text.Split('|');
            var modelPart = parts.FirstOrDefault(p => p.Trim().StartsWith("Ship:"));
            if (modelPart is null) return;
            var model = modelPart.Split(':')[1].Trim();
            var proj = owner.engine.ConstructionQueue.FirstOrDefault(p => string.Equals(p.Blueprint.Name, model, StringComparison.OrdinalIgnoreCase));
            if (proj is null) return;
            // refund half cost
            proj.Owner.Credits += proj.Blueprint.Cost / 2;
            owner.engine.ConstructionQueue.Remove(proj);
            owner.AppendLog($"Construction of {proj.Blueprint.Name} cancelled. Refunded {proj.Blueprint.Cost / 2} credits.");
            RefreshQueue();
            owner.RefreshStatus();
        }

        private void FastTrack(string text)
        {
            var parts = text.Split('|');
            var modelPart = parts.FirstOrDefault(p => p.Trim().StartsWith("Ship:"));
            if (modelPart is null) return;
            var model = modelPart.Split(':')[1].Trim();
            var proj = owner.engine.ConstructionQueue.FirstOrDefault(p => string.Equals(p.Blueprint.Name, model, StringComparison.OrdinalIgnoreCase));
            if (proj is null) return;
            var cost = Math.Max(10, proj.RemainingHours / 2);
            if (proj.Owner.Credits < cost) { owner.AppendLog($"Not enough credits to fast-track ({cost} required)."); return; }
            proj.Owner.Credits -= cost;
            proj.RemainingHours = Math.Max(0, proj.RemainingHours - 24);
            owner.AppendLog($"Paid {cost} credits to fast-track {proj.Blueprint.Name}. Remaining hours: {proj.RemainingHours}.");
            if (proj.RemainingHours <= 0) owner.engine.AdvanceWorldTime(0, proj.Owner.Location, proj.Owner); // trigger completion check
            RefreshQueue();
            owner.RefreshStatus();
        }
    }

    private sealed class TravelAndEncounterWindow : Form
    {
        private readonly Form1 owner;
        private readonly TreeView locationTree  = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(15, 12, 25),
            ForeColor = Color.LightGray,
            Font     = new Font("Segoe UI", 9f),
            BorderStyle = BorderStyle.None,
            ShowLines = true,
            FullRowSelect = true
        };
        private readonly Label  locNameLabel = new()
        {
            Dock = DockStyle.Top, AutoSize = false, Height = 24,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = Color.Gold, BackColor = Color.FromArgb(22, 20, 40)
        };
        private readonly Label  locDescLabel = new()
        {
            Dock = DockStyle.Top, AutoSize = false, Height = 44,
            Font = new Font("Segoe UI", 8.5f), ForeColor = Color.Silver,
            BackColor = Color.FromArgb(18, 16, 30)
        };
        private readonly Label  threatLabel = new()
        {
            Dock = DockStyle.Top, AutoSize = false, Height = 20,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Italic), ForeColor = Color.Tomato,
            BackColor = Color.FromArgb(18, 16, 30)
        };
        private readonly Label  questLabel = new()
        {
            Dock = DockStyle.Top, AutoSize = false, Height = 20,
            Font = new Font("Segoe UI", 8.5f), ForeColor = Color.Gold,
            BackColor = Color.FromArgb(18, 16, 30)
        };
        private readonly ComboBox actionBox = new()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 220,
            Font = new Font("Segoe UI", 9f)
        };
        private readonly Button goBtn = new()
        {
            Text = "Go Here", Width = 90, Height = 32,
            BackColor = Color.DarkSlateBlue, ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        private readonly TextBox activityLog = new()
        {
            Dock = DockStyle.Fill, Multiline = true, ReadOnly = true,
            ScrollBars = ScrollBars.Vertical, WordWrap = true,
            Font = new Font("Consolas", 8.5f),
            BackColor = Color.FromArgb(14, 14, 14),
            ForeColor = Color.Lime, BorderStyle = BorderStyle.None
        };

        private string planet = "";
        private PlanetZoneLocation? selected;

        public TravelAndEncounterWindow(Form1 owner)
        {
            this.owner = owner;
            planet     = owner.character?.Location ?? "";

            Text            = $"Travel & Explore — {planet}";
            Size            = new Size(980, 580);
            MinimumSize     = new Size(800, 450);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition   = FormStartPosition.Manual;
            Location        = new Point(owner.Location.X + 80, owner.Location.Y + 80);
            BackColor       = Color.FromArgb(16, 16, 16);

            // ── Master layout: tree left | details right ──────────────────────
            var master = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1
            };
            master.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
            master.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            master.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(master);

            // ── Left panel — tree ─────────────────────────────────────────────
            var leftOuter = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(12, 10, 22),
                Padding = new Padding(0, 0, 2, 0)
            };
            var treeHeader = new Label
            {
                Text = $"  Locations on {planet}",
                Dock = DockStyle.Top, AutoSize = false, Height = 28,
                ForeColor = Color.LightBlue,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(20, 15, 40),
                TextAlign = ContentAlignment.MiddleLeft
            };
            leftOuter.Controls.Add(locationTree);
            leftOuter.Controls.Add(treeHeader);
            master.Controls.Add(leftOuter, 0, 0);

            // ── Right panel — details + action + log ──────────────────────────
            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4
            };
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // detail card
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // action bar
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // log
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // close bar
            master.Controls.Add(right, 1, 0);

            // Detail card
            var detailCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 16, 30),
                Padding = new Padding(8, 4, 8, 4)
            };
            // Stack labels top-to-bottom (added reverse order because DockStyle.Top stacks)
            detailCard.Controls.Add(activityLog); // filler — actually not here, just init structure
            detailCard.Controls.Add(questLabel);
            detailCard.Controls.Add(threatLabel);
            detailCard.Controls.Add(locDescLabel);
            detailCard.Controls.Add(locNameLabel);
            detailCard.MinimumSize = new Size(0, 116);
            right.Controls.Add(detailCard, 0, 0);

            // Action bar
            var actionBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true,
                BackColor = Color.FromArgb(12, 12, 12),
                Padding = new Padding(8, 6, 8, 6)
            };
            actionBar.Controls.Add(new Label
            {
                Text = "Action:", AutoSize = true, ForeColor = Color.White,
                Padding = new Padding(0, 6, 6, 0)
            });
            actionBar.Controls.Add(actionBox);
            actionBar.Controls.Add(goBtn);
            right.Controls.Add(actionBar, 0, 1);

            // Activity log (takes remaining space)
            var logPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(14, 14, 14) };
            logPanel.Controls.Add(activityLog);
            activityLog.AppendText($"Planet: {planet}\r\nExpand a region in the tree to view locations.\r\n");
            right.Controls.Add(logPanel, 0, 2);

            // Close bar
            var closeBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, AutoSize = true,
                BackColor = Color.FromArgb(10, 10, 10),
                Padding = new Padding(8, 4, 8, 4)
            };
            var closeBtn = new Button { Text = "Close", Width = 90, Height = 30 };
            closeBtn.Click += (_, _) => Close();
            closeBar.Controls.Add(closeBtn);
            right.Controls.Add(closeBar, 0, 3);

            goBtn.Click += (_, _) => FireAction();
            locationTree.AfterSelect += (_, e) => SelectNode(e.Node);

            BuildTree();
        }

        // ─── Build tree: Region → District → Location ─────────────────────────
        private void BuildTree()
        {
            locationTree.Nodes.Clear();

            // All locations on this planet (flat, for quest → location matching)
            var allLocs = owner.engine.PlanetLocations.TryGetValue(planet, out var flatLocs)
                ? flatLocs : new List<PlanetZoneLocation>();

            // Build quest zone sets: raw name + normalised EncounterZone type
            var questZones   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var questsHere   = new List<Quest>();
            foreach (var q in owner.engine.ActiveQuests)
            {
                if (q.Status is not ("active" or "ready")) continue;
                if (!string.Equals(q.TargetPlanet, planet, StringComparison.OrdinalIgnoreCase)) continue;
                questsHere.Add(q);
                if (!string.IsNullOrWhiteSpace(q.ObjectiveZone))
                {
                    questZones.Add(q.ObjectiveZone);                          // raw (exact match)
                    questZones.Add(NormalizeToEncounterZone(q.ObjectiveZone)); // canonical type
                }
            }

            // ── Active Quest Objectives section (always visible at the top) ──
            if (questsHere.Count > 0)
            {
                var questRegion = locationTree.Nodes.Add($"★  Active Quests on {planet}  [{questsHere.Count}]");
                questRegion.ForeColor = Color.Gold;
                questRegion.NodeFont  = new Font("Segoe UI", 9f, FontStyle.Bold);

                foreach (var q in questsHere)
                {
                    var ezNorm  = NormalizeToEncounterZone(q.ObjectiveZone);
                    // Pick the closest matching location so clicking the node loads details
                    var bestLoc = allLocs.FirstOrDefault(l =>
                                      string.Equals(l.EncounterZone, ezNorm, StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(l.EncounterZone, q.ObjectiveZone, StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(l.Name, q.ObjectiveZone, StringComparison.OrdinalIgnoreCase));

                    string objLabel = q.ObjectiveType.ToUpperInvariant() switch
                    {
                        "COMBAT"  => $"⚔ [{q.ObjectiveType.ToUpper()}]",
                        "FETCH"   => $"📦 [{q.ObjectiveType.ToUpper()}]",
                        "TRAVEL"  => $"✈ [{q.ObjectiveType.ToUpper()}]",
                        _         => $"[{q.ObjectiveType.ToUpper()}]"
                    };

                    var qNode = questRegion.Nodes.Add($"{objLabel}  {q.Title}");
                    qNode.ForeColor = Color.Gold;
                    qNode.Tag       = bestLoc;   // may be null if no match — SelectNode handles that

                    // Sub-node: zone hint + target
                    string zoneHint = !string.IsNullOrWhiteSpace(q.ObjectiveZone)
                        ? $"→ Zone: {q.ObjectiveZone}   Target: {q.ObjectiveTarget}"
                        : $"→ Target: {q.ObjectiveTarget}";
                    var detailNode = qNode.Nodes.Add(zoneHint);
                    detailNode.ForeColor = Color.Goldenrod;
                    detailNode.Tag       = bestLoc;

                    if (bestLoc != null)
                    {
                        var navNode = qNode.Nodes.Add($"→ Best location: {bestLoc.Name}  ({bestLoc.District})");
                        navNode.ForeColor = Color.LightGreen;
                        navNode.Tag       = bestLoc;
                    }
                }
                questRegion.Expand();
            }

            // ── Full planet location tree ──────────────────────────────────
            var regions = owner.engine.GetLocationRegions(planet);
            if (regions.Count == 0)
            {
                // Fallback: flat zone list
                var regionNode = locationTree.Nodes.Add("Zones");
                regionNode.ForeColor = Color.LightSteelBlue;
                foreach (var zone in owner.engine.GetPlanetZones(planet))
                    regionNode.Nodes.Add(zone).ForeColor = questZones.Contains(zone) ? Color.Gold : Color.LightGray;
                regionNode.Expand();
                return;
            }

            foreach (var region in regions)
            {
                var rNode = locationTree.Nodes.Add(region);
                rNode.ForeColor = Color.LightSteelBlue;
                rNode.NodeFont  = new Font("Segoe UI", 9f, FontStyle.Bold);

                bool regionHasQuest = false;
                foreach (var district in owner.engine.GetLocationDistricts(planet, region))
                {
                    var dNode = rNode.Nodes.Add(district);
                    dNode.ForeColor = Color.Silver;

                    bool districtHasQuest = false;
                    foreach (var loc in owner.engine.GetLocationsByDistrict(planet, region, district))
                    {
                        var hasQuest = questZones.Contains(loc.EncounterZone) || questZones.Contains(loc.Name);
                        var locNode  = dNode.Nodes.Add(loc.Name);
                        locNode.ForeColor = hasQuest ? Color.Gold : Color.LightGray;
                        locNode.Tag = loc;
                        if (hasQuest) { districtHasQuest = true; regionHasQuest = true; }
                    }

                    if (districtHasQuest) { dNode.Text = $"★ {district}"; dNode.ForeColor = Color.Goldenrod; }
                }

                if (regionHasQuest) { rNode.Text = $"★ {region}"; rNode.ForeColor = Color.Gold; }
                if (regionHasQuest) rNode.Expand();
            }
        }

        // Maps faction-flavour zone names to canonical EncounterZone types used in PlanetLocations
        private static string NormalizeToEncounterZone(string? factionZone) =>
            factionZone?.ToLowerInvariant() switch
            {
                // contact / outpost / military
                "garrison" or "outpost" or "dock quarter" or "industrial zone" or "checkpoint"
                or "forge" or "clan hall" or "armory" or "rebel base" => "contact",
                // underworld / slums / black market
                "black market" or "back alley" or "smuggler den" or "spice run"
                or "underground market" or "secure vault" => "slums",
                // docks / hangars
                "transport hangar" or "abandoned dock" or "freight yard" => "dock",
                // ruins / force sites
                "ruins" or "temple ruins" or "sacred cave" or "ancient library" or "force nexus" => "ruins",
                // wilderness / outdoor
                "forest outpost" or "mountain pass" or "proving grounds" or "battlefield"
                or "meditation grove" => "wilderness",
                // market / cantina
                "cantina" or "bounty board" => "market",
                // pass-through for already-canonical values
                _ => factionZone ?? ""
            };

        // ─── Selection: a leaf (location) node was picked ─────────────────────
        private void SelectNode(TreeNode? node)
        {
            if (node?.Tag is not PlanetZoneLocation loc)
            {
                selected = null;
                locNameLabel.Text  = node?.Text ?? "";
                locDescLabel.Text  = "Select a specific location to see details and actions.";
                threatLabel.Text   = "";
                questLabel.Text    = "";
                actionBox.Items.Clear();
                goBtn.Enabled = false;
                return;
            }

            selected = loc;
            locNameLabel.Text = $"  {loc.Name}  ·  {loc.District}";
            locDescLabel.Text = $"  {loc.Description}";
            threatLabel.Text  = $"  Threat: {loc.ThreatLevel}  |  Zone class: {loc.EncounterZone}";

            // Find quests active/ready on this planet whose zone matches this location
            var questsHere = owner.engine.ActiveQuests
                .Where(q => q.Status is "active" or "ready"
                         && string.Equals(q.TargetPlanet, planet, StringComparison.OrdinalIgnoreCase)
                         && (string.IsNullOrWhiteSpace(q.ObjectiveZone)
                             || string.Equals(q.ObjectiveZone, loc.EncounterZone, StringComparison.OrdinalIgnoreCase)
                             || string.Equals(NormalizeToEncounterZone(q.ObjectiveZone), loc.EncounterZone, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (questsHere.Count > 0)
                questLabel.Text = $"  ★ Quest here: {string.Join(", ", questsHere.Take(2).Select(q => q.Title))}{(questsHere.Count > 2 ? "…" : "")}";
            else
                questLabel.Text = "";

            // Populate action dropdown — quest actions appear at top so they are the default
            actionBox.Items.Clear();
            foreach (var q in questsHere)
            {
                string tag = q.Status == "ready"
                    ? "★ TURN IN"
                    : q.ObjectiveType.ToUpperInvariant() switch
                    {
                        "COMBAT" => "⚔ PURSUE",
                        "FETCH"  => "📦 DELIVER",
                        _        => "⚡ ADVANCE"
                    };
                actionBox.Items.Add($"☑ {tag}: {q.Title}");
            }
            foreach (var a in loc.Actions) actionBox.Items.Add(a);
            if (actionBox.Items.Count > 0) actionBox.SelectedIndex = 0;

            // Check vehicle requirement
            var canAccess = owner.character is null || owner.engine.CanAccessZone(owner.character, loc.EncounterZone);
            goBtn.Enabled = canAccess;
            if (!canAccess)
                activityLog.AppendText($"[Locked] {loc.Name} requires a vehicle to access.\r\n");
        }

        // ─── Trigger action ───────────────────────────────────────────────────
        private void FireAction()
        {
            if (owner.character is null || selected is null) return;
            if (!owner.engine.CanAccessZone(owner.character, selected.EncounterZone))
            {
                if (!IsDisposed) activityLog.AppendText($"You need a vehicle to enter {selected.Name}.\r\n");
                return;
            }
            var action = actionBox.Text;
            if (!IsDisposed)
                activityLog.AppendText($"\r\n▶  {selected.District} → {selected.Name}  [{action}]\r\n");

            // Quest completion actions (injected at top of dropdown by SelectNode)
            if (action.StartsWith("☑ ", StringComparison.Ordinal))
            {
                var colonIdx   = action.IndexOf(": ", StringComparison.Ordinal);
                var questTitle = colonIdx >= 0 ? action[(colonIdx + 2)..] : "";
                var q = owner.engine.ActiveQuests.FirstOrDefault(x =>
                    x.Status is "active" or "ready" &&
                    x.Title.Equals(questTitle, StringComparison.OrdinalIgnoreCase));
                if (q != null)
                {
                    new QuestCompletionWindow(owner, q).Show();
                    return;
                }
            }

            // Shipyard locations get the dedicated shipyard window for merchant/dock actions
            bool isShipyard = selected.District.Contains("Shipyard", StringComparison.OrdinalIgnoreCase)
                           || selected.Name.Contains("Shipyard", StringComparison.OrdinalIgnoreCase);
            if (isShipyard && action is "Visit Merchant" or "Inspect Dock")
            {
                owner.OpenShipyardWindow();
                return;
            }

            owner.HandleEncounterSelection(selected.EncounterZone, action);
            owner.RefreshStatus();

            if (!IsDisposed)
            {
                activityLog.AppendText("──────────────────────────────────────────────────────\r\n");
                activityLog.ScrollToCaret();
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  QUEST COMPLETION WINDOW
    //  Fetch  → Cargo Delivery minigame (locate item in manifest, animate transfer)
    //  Combat → Tactical Engagement minigame (HP bars, per-click combat rounds)
    //  Travel → Arrival confirmation (auto-complete)
    // ═══════════════════════════════════════════════════════════════════════════
    private sealed class QuestCompletionWindow : Form
    {
        private readonly Form1 owner;
        private readonly Quest quest;
        private bool questDone;

        // ── Header ──────────────────────────────────────────────────────────────
        private readonly Label titleLabel = new()
        {
            AutoSize = false, Dock = DockStyle.Fill, Height = 36,
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.Gold, BackColor = Color.FromArgb(20, 16, 40),
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0)
        };
        private readonly Label subLabel = new()
        {
            AutoSize = false, Dock = DockStyle.Fill, Height = 22,
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.Silver, BackColor = Color.FromArgb(20, 16, 40),
            Padding = new Padding(8, 3, 0, 0)
        };
        private readonly ProgressBar questBar = new()
        {
            Dock = DockStyle.Fill, Height = 14,
            Style = ProgressBarStyle.Continuous
        };

        // ── Content area (swapped per type) ─────────────────────────────────────
        private readonly Panel contentPanel = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(10, 10, 18) };

        // ── Reward panel ─────────────────────────────────────────────────────────
        private readonly Panel  rewardPanel = new()  { Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 24, 8), Visible = false };
        private readonly Label  rewardLabel = new()  { AutoSize = false, Dock = DockStyle.Fill, ForeColor = Color.LightGreen, Font = new Font("Consolas", 9f), Padding = new Padding(8, 6, 0, 0) };
        private readonly Button collectBtn  = new()  { Text = "✓  Collect Rewards", Width = 155, Height = 34, BackColor = Color.DarkGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Enabled = false, Margin = new Padding(4, 10, 0, 0) };

        // ── Log ──────────────────────────────────────────────────────────────────
        private readonly TextBox logBox = new()
        {
            Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(8, 8, 8), ForeColor = Color.LightGreen,
            Font = new Font("Consolas", 8.5f)
        };

        // ── Fetch state ───────────────────────────────────────────────────────────
        private Label?       termStatusLabel;
        private ProgressBar? transferBar;
        private int          wrongClicks;
        private Button?      correctItemBtn;

        // ── Combat state ──────────────────────────────────────────────────────────
        private int          playerHp, playerMaxHp;
        private int[]        enemyHps    = Array.Empty<int>();
        private int[]        enemyMaxHps = Array.Empty<int>();
        private Label[]      enemyNameLabels = Array.Empty<Label>();
        private ProgressBar[]enemyHpBars     = Array.Empty<ProgressBar>();
        private Panel[]      enemyPanels     = Array.Empty<Panel>();
        private int          currentEnemy;
        private Label?       playerHpLabel;
        private ProgressBar? playerHpBar;
        private Button?      engageBtn;

        public QuestCompletionWindow(Form1 owner, Quest quest)
        {
            this.owner = owner;
            this.quest = quest;

            Text            = $"Quest — {quest.Title}";
            Size            = new Size(760, 660);
            MinimumSize     = new Size(640, 520);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition   = FormStartPosition.Manual;
            Location        = new Point(owner.Location.X + 70, owner.Location.Y + 70);
            BackColor       = Color.FromArgb(14, 12, 22);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 7, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));  // 0 title
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));  // 1 sub info
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));  // 2 progress bar
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));  // 3 content
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));  // 4 reward panel
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));  // 5 log
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // 6 close bar
            Controls.Add(root);

            // Row 0: Title
            titleLabel.Text = $"  ★  {quest.Title}";
            root.Controls.Add(titleLabel, 0, 0);

            // Row 1: Sub info
            subLabel.ForeColor = quest.Faction switch
            {
                "Rebels"       => Color.FromArgb(255, 90, 90),
                "Empire"       => Color.FromArgb(100, 130, 255),
                "Smugglers"    => Color.FromArgb(200, 155, 50),
                "Jedi"         => Color.FromArgb(100, 200, 255),
                "Mandalorians" => Color.FromArgb(190, 100, 40),
                "Guilds"       => Color.FromArgb(180, 70, 180),
                _              => Color.Silver
            };
            subLabel.Text = $"  Faction: {quest.Faction}   ·   Issuer: {quest.IssuerName}   ·   [{quest.ObjectiveType.ToUpper()}]   Zone: {quest.ObjectiveZone}";
            root.Controls.Add(subLabel, 0, 1);

            // Row 2: Progress bar
            var req = Math.Max(1, quest.ObjectiveRequired);
            questBar.Maximum = req;
            questBar.Value   = Math.Clamp(quest.ObjectiveProgress, 0, req);
            root.Controls.Add(questBar, 0, 2);

            // Row 3: Content (type-specific, built below)
            root.Controls.Add(contentPanel, 0, 3);

            // Row 4: Reward panel
            var rwRow = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 2, BackColor = Color.FromArgb(8, 24, 8) };
            rwRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            rwRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            rwRow.Controls.Add(rewardLabel, 0, 0);
            var rwBtnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 24, 8) };
            rwBtnFlow.Controls.Add(collectBtn);
            rwRow.Controls.Add(rwBtnFlow, 1, 0);
            rewardPanel.Controls.Add(rwRow);
            root.Controls.Add(rewardPanel, 0, 4);

            // Row 5: Log
            var logGrp = new GroupBox { Text = "Comms Log", Dock = DockStyle.Fill, ForeColor = Color.LightGray };
            logGrp.Controls.Add(logBox);
            root.Controls.Add(logGrp, 0, 5);

            // Row 6: Close bar
            var closeBar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, BackColor = Color.FromArgb(10, 10, 10), Padding = new Padding(8, 4, 8, 4) };
            var closeBtn = new Button { Text = "Close", Width = 90, Height = 28 };
            closeBtn.Click += (_, _) => Close();
            closeBar.Controls.Add(closeBtn);
            root.Controls.Add(closeBar, 0, 6);

            collectBtn.Click += OnCollect;

            switch (quest.ObjectiveType.ToLowerInvariant())
            {
                case "fetch":  BuildFetchContent();  break;
                case "combat": BuildCombatContent(); break;
                default:       BuildTravelContent(); break;
            }

            AppendLog($"[BRIEFING]  {quest.Description}");
            AppendLog($"[OBJECTIVE] {quest.ObjectiveType.ToUpper()}: {quest.ObjectiveTarget}  ({quest.ObjectiveProgress}/{req})");
            if (!string.IsNullOrWhiteSpace(quest.ObjectiveZone))
                AppendLog($"[ZONE]      {quest.ObjectiveZone}");
        }

        private void AppendLog(string s)
        {
            if (!IsDisposed) { logBox.AppendText(s + "\r\n"); logBox.ScrollToCaret(); }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  FETCH  — Cargo Delivery Terminal
        // ─────────────────────────────────────────────────────────────────────
        private void BuildFetchContent()
        {
            contentPanel.BackColor = Color.FromArgb(10, 12, 20);

            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));

            // ── Left: cargo manifest (shuffled inventory buttons) ──
            var manifestGrp  = new GroupBox { Text = "⬡  Your Cargo Manifest", Dock = DockStyle.Fill, ForeColor = Color.LightCyan };
            var manifestFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(8, 10, 18), Padding = new Padding(6) };

            var inv     = owner.character?.Inventory ?? new List<string>();
            bool hasItem = inv.Any(x => x.Equals(quest.ObjectiveTarget, StringComparison.OrdinalIgnoreCase));

            if (inv.Count == 0)
            {
                manifestFlow.Controls.Add(new Label { Text = "(Cargo hold is empty)", ForeColor = Color.DimGray, AutoSize = true, Padding = new Padding(8) });
            }
            else
            {
                var rng     = new Random();
                var shuffle = inv.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(_ => rng.Next()).ToList();
                foreach (var item in shuffle)
                {
                    bool isTarget = item.Equals(quest.ObjectiveTarget, StringComparison.OrdinalIgnoreCase);
                    var btn = new Button
                    {
                        Text      = item.Length > 24 ? item[..24] + "…" : item,
                        Tag       = item,
                        Width     = 178, Height = 34,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(18, 20, 38),
                        ForeColor = Color.LightGray,
                        Margin    = new Padding(3),
                        Font      = new Font("Consolas", 8.25f)
                    };
                    if (isTarget) correctItemBtn = btn;
                    btn.Click += OnCargoClick;
                    manifestFlow.Controls.Add(btn);
                }
            }
            manifestGrp.Controls.Add(manifestFlow);
            split.Controls.Add(manifestGrp, 0, 0);

            // ── Right: delivery terminal ──
            var termGrp    = new GroupBox { Text = "▣  Delivery Terminal", Dock = DockStyle.Fill, ForeColor = Color.Goldenrod };
            var termLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1, BackColor = Color.FromArgb(8, 14, 8) };
            termLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26)); // label
            termLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52)); // item name
            termLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));// status
            termLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22)); // transfer bar

            termLayout.Controls.Add(new Label
            {
                Text = "AWAITING DELIVERY:", ForeColor = Color.DimGray, AutoSize = false,
                Dock = DockStyle.Fill, Font = new Font("Consolas", 7.5f), Padding = new Padding(6, 5, 0, 0)
            }, 0, 0);
            termLayout.Controls.Add(new Label
            {
                Text = quest.ObjectiveTarget.ToUpperInvariant(),
                ForeColor = hasItem ? Color.Goldenrod : Color.OrangeRed,
                AutoSize = false, Dock = DockStyle.Fill,
                Font = new Font("Consolas", 11f, FontStyle.Bold), Padding = new Padding(6, 4, 0, 0)
            }, 0, 1);

            termStatusLabel = new Label
            {
                Text = hasItem
                    ? "◀ Locate the item in your manifest and click it to deliver."
                    : "⚠  Item NOT in cargo hold.\r\nAcquire it, then return here.",
                ForeColor = hasItem ? Color.LightGreen : Color.OrangeRed,
                AutoSize = false, Dock = DockStyle.Fill,
                Font = new Font("Consolas", 8.5f), Padding = new Padding(6, 4, 0, 0)
            };
            termLayout.Controls.Add(termStatusLabel, 0, 2);

            transferBar = new ProgressBar
            {
                Dock = DockStyle.Fill, Minimum = 0, Maximum = 100, Value = 0,
                ForeColor = Color.LimeGreen, Style = ProgressBarStyle.Continuous
            };
            termLayout.Controls.Add(transferBar, 0, 3);

            termGrp.Controls.Add(termLayout);
            split.Controls.Add(termGrp, 1, 0);
            contentPanel.Controls.Add(split);

            if (!hasItem)
                AppendLog($"⚠  '{quest.ObjectiveTarget}' is NOT in your inventory. Acquire it and return.");
            else
                AppendLog($"► '{quest.ObjectiveTarget}' found in cargo. Click it in the manifest to initiate delivery.");
        }

        private void OnCargoClick(object? sender, EventArgs e)
        {
            if (questDone || sender is not Button btn) return;
            var item     = btn.Tag as string ?? "";
            bool isTarget = item.Equals(quest.ObjectiveTarget, StringComparison.OrdinalIgnoreCase);

            if (!isTarget)
            {
                wrongClicks++;
                var orig = btn.BackColor;
                btn.BackColor = Color.FromArgb(72, 10, 10);
                var reset = new System.Windows.Forms.Timer { Interval = 450 };
                reset.Tick += (_, _) => { btn.BackColor = orig; reset.Stop(); reset.Dispose(); };
                reset.Start();
                AppendLog($"✗ '{item}' — wrong item. ({Math.Max(0, 3 - wrongClicks)} hint(s) remaining)");
                if (wrongClicks >= 3 && correctItemBtn != null)
                {
                    correctItemBtn.BackColor = Color.FromArgb(10, 52, 10);
                    correctItemBtn.ForeColor = Color.LightGreen;
                    AppendLog("▸ Hint: the correct item is highlighted green in the manifest.");
                }
                return;
            }

            // Correct — begin transfer animation
            btn.Enabled   = false;
            btn.BackColor = Color.FromArgb(10, 52, 10);
            if (termStatusLabel != null) termStatusLabel.Text = "⬆  Secure encrypted transfer in progress...";
            AppendLog($"✓ '{item}' selected. Initiating transfer to {quest.IssuerName}...");

            var pct = 0;
            var t   = new System.Windows.Forms.Timer { Interval = 35 };
            t.Tick += (_, _) =>
            {
                pct += 4;
                if (transferBar != null) transferBar.Value = Math.Min(100, pct);
                if (pct < 100) return;
                t.Stop(); t.Dispose();
                FetchQuestComplete();
            };
            t.Start();
        }

        private void FetchQuestComplete()
        {
            if (termStatusLabel != null)
            {
                termStatusLabel.Text      = "✓  Transfer confirmed. Package received.";
                termStatusLabel.ForeColor = Color.LimeGreen;
            }
            questBar.Value = questBar.Maximum;
            quest.ObjectiveProgress = quest.ObjectiveRequired;
            quest.Status = "ready";
            questDone    = true;
            AppendLog("✓ Transfer complete. Package successfully delivered.");
            ShowRewardPanel();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  COMBAT  — Tactical Engagement
        // ─────────────────────────────────────────────────────────────────────
        private void BuildCombatContent()
        {
            contentPanel.BackColor = Color.FromArgb(10, 8, 8);

            var required  = Math.Max(1, quest.ObjectiveRequired);
            var rng       = new Random();
            playerMaxHp   = owner.character?.MaxHp ?? 30;
            playerHp      = playerMaxHp;

            enemyHps        = new int[required];
            enemyMaxHps     = new int[required];
            enemyNameLabels = new Label[required];
            enemyHpBars     = new ProgressBar[required];
            enemyPanels     = new Panel[required];
            for (int i = 0; i < required; i++)
            {
                enemyMaxHps[i] = 14 + rng.Next(5, 18);
                enemyHps[i]    = enemyMaxHps[i];
            }

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1,
                RowCount = required + 2,
                BackColor = Color.FromArgb(10, 8, 8)
            };
            for (int i = 0; i < required; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // player HP
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // engage btn

            // Enemy rows
            for (int i = 0; i < required; i++)
            {
                var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(32, 10, 10), Margin = new Padding(4, 3, 4, 1), Height = 40 };
                var nameL = new Label
                {
                    Text = $"  ☠  {quest.ObjectiveTarget}  #{i + 1}",
                    AutoSize = false, Width = 224, Height = 26,
                    ForeColor = Color.OrangeRed, Font = new Font("Consolas", 8.5f),
                    Location = new Point(4, 8), BackColor = Color.Transparent
                };
                var bar = new ProgressBar
                {
                    Minimum = 0, Maximum = enemyMaxHps[i], Value = enemyMaxHps[i],
                    Width = 192, Height = 16, ForeColor = Color.OrangeRed,
                    Location = new Point(232, 12), Style = ProgressBarStyle.Continuous
                };
                panel.Controls.Add(nameL);
                panel.Controls.Add(bar);
                enemyNameLabels[i] = nameL;
                enemyHpBars[i]     = bar;
                enemyPanels[i]     = panel;
                layout.Controls.Add(panel, 0, i);
            }

            // Player HP row
            var playerRow = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 24, 8), Margin = new Padding(4, 2, 4, 2), Height = 38 };
            playerHpLabel = new Label { Text = $"  YOU  HP: {playerHp}/{playerMaxHp}", AutoSize = false, Width = 224, Height = 26, ForeColor = Color.LightGreen, Font = new Font("Consolas", 8.5f), Location = new Point(4, 6), BackColor = Color.Transparent };
            playerHpBar   = new ProgressBar { Minimum = 0, Maximum = playerMaxHp, Value = playerHp, Width = 192, Height = 16, ForeColor = Color.Lime, Location = new Point(232, 11), Style = ProgressBarStyle.Continuous };
            playerRow.Controls.Add(playerHpLabel);
            playerRow.Controls.Add(playerHpBar);
            layout.Controls.Add(playerRow, 0, required);

            // Engage button
            engageBtn = new Button
            {
                Text = $"⚔  ENGAGE  [{quest.ObjectiveTarget}]",
                Width = 260, Height = 34, BackColor = Color.DarkRed, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Margin = new Padding(8, 4, 0, 0)
            };
            engageBtn.Click += OnEngage;
            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(4) };
            btnFlow.Controls.Add(engageBtn);
            layout.Controls.Add(btnFlow, 0, required + 1);

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            scroll.Controls.Add(layout);
            contentPanel.Controls.Add(scroll);

            // Grey out enemies already killed via carry-over progress
            currentEnemy = Math.Min(quest.ObjectiveProgress, required);
            for (int i = 0; i < currentEnemy; i++) MarkEnemyKilled(i);

            AppendLog(quest.ObjectiveProgress > 0
                ? $"► Carry-over: {quest.ObjectiveProgress}/{required} already eliminated."
                : $"► Engage all {required} {quest.ObjectiveTarget}(s). Click ENGAGE to strike.");
        }

        private void MarkEnemyKilled(int i)
        {
            if (i >= enemyHps.Length) return;
            enemyHps[i] = 0;
            if (i < enemyHpBars.Length)    enemyHpBars[i].Value = 0;
            if (i < enemyNameLabels.Length) { enemyNameLabels[i].Text += "  [KIA]"; enemyNameLabels[i].ForeColor = Color.DimGray; }
            if (i < enemyPanels.Length)     enemyPanels[i].BackColor = Color.FromArgb(18, 18, 18);
        }

        private void OnEngage(object? sender, EventArgs e)
        {
            if (questDone || owner.character is null) return;
            var required = Math.Max(1, quest.ObjectiveRequired);
            if (currentEnemy >= required) return;

            var rng      = new Random();
            int strength = owner.character.Stats.GetValueOrDefault("strength", 3);
            int agility  = owner.character.Stats.GetValueOrDefault("agility", 3);

            // Player attacks current enemy
            int playerAtk = Math.Max(4, 6 + strength + rng.Next(-2, 7));
            enemyHps[currentEnemy] = Math.Max(0, enemyHps[currentEnemy] - playerAtk);
            enemyHpBars[currentEnemy].Value = enemyHps[currentEnemy];
            AppendLog($"⚔ You strike for {playerAtk} dmg.  Enemy HP: {enemyHps[currentEnemy]}/{enemyMaxHps[currentEnemy]}");

            if (enemyHps[currentEnemy] <= 0)
            {
                MarkEnemyKilled(currentEnemy);
                currentEnemy++;
                quest.ObjectiveProgress = currentEnemy;
                questBar.Value = Math.Min(required, currentEnemy);
                owner.engine.RegisterCombatQuestKill(owner.character, quest.Id);
                AppendLog($"✓ Target eliminated! ({currentEnemy}/{required})");

                if (currentEnemy >= required)
                {
                    questDone = true; quest.Status = "ready";
                    if (engageBtn != null) engageBtn.Enabled = false;
                    AppendLog("★ All targets eliminated. Objective complete!");
                    ShowRewardPanel();
                }
                else
                {
                    AppendLog($"► Next: {quest.ObjectiveTarget} #{currentEnemy + 1}");
                }
                return;
            }

            // Enemy counter-attacks
            int armor    = owner.character.Armor + agility / 2;
            int enemyAtk = Math.Max(1, 7 + rng.Next(-2, 8) - armor / 2);
            playerHp = Math.Max(0, playerHp - enemyAtk);
            AppendLog($"◀ Enemy retaliates for {enemyAtk} dmg ({armor} armor).  Your HP: {playerHp}/{playerMaxHp}");
            if (playerHpLabel != null) playerHpLabel.Text = $"  YOU  HP: {playerHp}/{playerMaxHp}";
            if (playerHpBar   != null) playerHpBar.Value  = Math.Max(0, playerHp);

            if (playerHp <= 0)
            {
                questDone = true;
                if (engageBtn != null) { engageBtn.Enabled = false; engageBtn.Text = "DEFEATED — Retreat"; engageBtn.BackColor = Color.FromArgb(50, 50, 50); }
                AppendLog("✗ Overwhelmed. Retreat and recover. Partial progress is saved.");
                owner.AppendLog($"⚠ Combat quest '{quest.Title}' — retreated. ({currentEnemy}/{required})");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  TRAVEL  — Arrival Confirmation
        // ─────────────────────────────────────────────────────────────────────
        private void BuildTravelContent()
        {
            contentPanel.BackColor = Color.FromArgb(8, 14, 8);
            contentPanel.Controls.Add(new Label
            {
                Text = $"✈  ARRIVAL CONFIRMED\r\n\r\nDestination: {owner.character?.Location ?? "Unknown"}\r\n\r\nYou have reached the designated planet.\r\nYour rewards are ready to collect.",
                Dock = DockStyle.Fill, ForeColor = Color.LightGreen,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10f)
            });
            quest.ObjectiveProgress = quest.ObjectiveRequired;
            quest.Status = "ready";
            questBar.Value = questBar.Maximum;
            questDone = true;
            AppendLog("✓ Travel objective satisfied. Collect your rewards.");
            ShowRewardPanel();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  REWARD PANEL
        // ─────────────────────────────────────────────────────────────────────
        private void ShowRewardPanel()
        {
            var parts = new List<string> { "  REWARDS:" };
            if (quest.RewardCredits > 0) parts.Add($"+{quest.RewardCredits} Cr");
            if (quest.RewardXp      > 0) parts.Add($"+{quest.RewardXp} XP");
            if (!string.IsNullOrWhiteSpace(quest.RewardItem))      parts.Add($"Item: {quest.RewardItem}");
            if (!string.IsNullOrWhiteSpace(quest.RewardBlueprint)) parts.Add($"BP: {quest.RewardBlueprint}");
            rewardLabel.Text    = string.Join("   |   ", parts);
            rewardPanel.Visible = true;
            collectBtn.Enabled  = true;
        }

        private void OnCollect(object? sender, EventArgs e)
        {
            if (owner.character is null) return;
            var result = owner.engine.CompleteQuest(owner.character, quest.Id);
            owner.AppendLog($"★ {result}");
            owner.RefreshStatus();
            collectBtn.Enabled  = false;
            collectBtn.Text     = "✓  Collected";
            collectBtn.BackColor = Color.FromArgb(20, 50, 20);
            AppendLog($"★ {result}");
        }
    }

    private sealed class ShipyardWindow : Form
    {
        private readonly Form1 owner;
        private readonly string _planet;
        private readonly TabControl tabs   = new() { Dock = DockStyle.Fill };
        private readonly ListView   dealerList  = new();
        private readonly ListView   craftList   = new();
        private readonly TextBox    infoBox     = new()
        {
            Multiline = true, ReadOnly = true, Lines = Array.Empty<string>(),
            Font = new Font("Consolas", 8.5f),
            BackColor = Color.FromArgb(14, 14, 14), ForeColor = Color.LightYellow,
            Dock = DockStyle.Bottom, Height = 160, BorderStyle = BorderStyle.None,
            ScrollBars = ScrollBars.Vertical
        };

        public ShipyardWindow(Form1 owner, string? locationOverride = null)
        {
            this.owner = owner;
            _planet = locationOverride ?? owner.character?.Location ?? "";
            // Resolve yard name + manufacturer
            string yardName, mfr;
            if (owner.engine.Planets.TryGetValue(_planet, out var pd))
            {
                yardName = string.IsNullOrWhiteSpace(pd.ShipyardName) ? "Shipyard" : pd.ShipyardName;
                mfr      = pd.ShipyardManufacturer ?? "";
            }
            else if (owner.engine.SpaceStations.TryGetValue(_planet, out var st))
            {
                yardName = string.IsNullOrWhiteSpace(st.Name) ? "Shipyard" : st.Name;
                mfr      = st.Manufacturer ?? "";
            }
            else
            {
                yardName = "Shipyard";
                mfr      = "";
            }

            Text            = $"{yardName}  ·  {_planet}";
            Size            = new Size(900, 580);
            MinimumSize     = new Size(720, 440);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition   = FormStartPosition.Manual;
            Location        = new Point(owner.Location.X + 120, owner.Location.Y + 100);
            BackColor       = Color.FromArgb(14, 14, 24);

            Controls.Add(tabs);
            Controls.Add(infoBox);

            // ── Tab 1: Ship Dealership ────────────────────────────────────────
            var dealPage = new TabPage("Ship Dealership") { BackColor = Color.FromArgb(12, 14, 28) };
            {
                dealerList.View = View.Details; dealerList.Dock = DockStyle.Fill; dealerList.FullRowSelect = true;
                dealerList.BackColor = Color.FromArgb(16, 16, 30); dealerList.ForeColor = Color.White;
                dealerList.Columns.Add("Ship",        220);
                dealerList.Columns.Add("Size",          80);
                dealerList.Columns.Add("Price",         100);
                dealerList.Columns.Add("Hull",          60);
                dealerList.Columns.Add("Shield",        60);
                dealerList.Columns.Add("Fuel",          55);
                dealerList.Columns.Add("Hyperdrive",    90);
                dealerList.Columns.Add("Weapon",       150);
                dealerList.Columns.Add("Tier",          80);
                dealerList.SelectedIndexChanged += (_, _) => UpdateInfo(dealerList);

                var buyBtn = new Button { Text = "Buy Ship", Width = 110, Height = 32,
                    BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                buyBtn.Click += (_, _) => DoPurchase();

                var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                root.Controls.Add(dealerList, 0, 0);
                var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6) };
                if (!string.IsNullOrWhiteSpace(mfr))
                    bar.Controls.Add(new Label { AutoSize = true, ForeColor = Color.CornflowerBlue,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        Text = $"  {mfr}", Padding = new Padding(0, 6, 20, 0) });
                bar.Controls.Add(buyBtn);
                root.Controls.Add(bar, 0, 1);
                dealPage.Controls.Add(root);
            }
            tabs.TabPages.Add(dealPage);

            // ── Tab 2: Craft Ship ─────────────────────────────────────────────
            var craftPage = new TabPage("Craft Ship") { BackColor = Color.FromArgb(12, 14, 28) };
            {
                craftList.View = View.Details; craftList.Dock = DockStyle.Fill; craftList.FullRowSelect = true;
                craftList.BackColor = Color.FromArgb(16, 16, 30); craftList.ForeColor = Color.White;
                craftList.Columns.Add("Ship",         180);
                craftList.Columns.Add("Size",          55);
                craftList.Columns.Add("Build Cost",    90);
                craftList.Columns.Add("Time",          55);
                craftList.Columns.Add("Facility",      120);
                craftList.Columns.Add("Materials",     400);
                craftList.SelectedIndexChanged += (_, _) => UpdateInfo(craftList);

                var craftBtn = new Button { Text = "Craft Ship", Width = 110, Height = 32,
                    BackColor = Color.DarkGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                craftBtn.Click += (_, _) => DoCraft();

                var root2 = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
                root2.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root2.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                root2.Controls.Add(craftList, 0, 0);
                var bar2 = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6) };
                bar2.Controls.Add(new Label { AutoSize = true, ForeColor = Color.Gray,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Italic), Padding = new Padding(0, 6, 16, 0),
                    Text = "You must have required parts in inventory." });
                bar2.Controls.Add(craftBtn);
                root2.Controls.Add(bar2, 0, 1);
                craftPage.Controls.Add(root2);
            }
            tabs.TabPages.Add(craftPage);

            tabs.SelectedIndexChanged += (_, _) => { if (tabs.SelectedIndex == 1) PopulateCraft(); };
            PopulateDealer();
            PopulateCraft();
            UpdateInfoBar();
        }

        private void PopulateDealer()
        {
            dealerList.Items.Clear();
            if (owner.character is null) return;
            var catalog = owner.engine.GetShipyardCatalog(_planet);
            foreach (var ship in catalog)
            {
                var price     = ship.PurchasePrice > 0 ? ship.PurchasePrice : ship.Cost;
                var canAfford = owner.character.Credits >= price;
                var alreadyOwns = owner.character.Ship is not null;
                var tierLabel = string.Equals(ship.ShipyardTier, "orbital", StringComparison.OrdinalIgnoreCase)
                    ? "⚠ Orbital" : "";
                var row = new ListViewItem(new[]
                {
                    ship.Name,
                    ship.SizeClass,
                    $"{price:N0} cr",
                    ship.Hull.ToString(),
                    ship.Shield.ToString(),
                    ship.Fuel.ToString(),
                    $"Class {ship.HyperdriveClass}",
                    ship.Weapon,
                    tierLabel
                });
                row.ForeColor = alreadyOwns    ? Color.Gray
                    : ship.IsCapital           ? Color.Goldenrod
                    : !canAfford               ? Color.IndianRed
                    : Color.MediumSeaGreen;
                row.Tag = ship;
                dealerList.Items.Add(row);
            }
        }

        private void PopulateCraft()
        {
            craftList.Items.Clear();
            if (owner.character is null) return;
            var catalog = owner.engine.GetShipyardCatalog(_planet);
            foreach (var ship in catalog)
            {
                // Hide ships whose blueprints the character has not yet discovered
                // The Skiff is always available as the basic starter ship
                if (!ship.Name.Equals("Skiff", StringComparison.OrdinalIgnoreCase)
                    && owner.engine.TryGetAssetLockReason(owner.character, ship.Name, out _)) continue;

                // Pull the computed recipe for accurate cost, time, and full ingredient list
                owner.engine.Recipes.TryGetValue(ship.Name, out var recipe);
                if (recipe is null) owner.engine.Recipes.TryGetValue(ship.Model, out recipe);

                var buildCost = recipe?.CreditCost ?? (int)(ship.Cost * 0.3f);
                var timeH     = recipe?.TimeHours  ?? Math.Max(8, ship.Cost / 20);

                // Build materials string from recipe Inputs (with quantities)
                string materials;
                if (recipe?.Inputs.Count > 0)
                {
                    materials = string.Join(", ", recipe.Inputs
                        .OrderByDescending(i => i.Quantity)
                        .Select(i => i.Quantity > 1 ? $"{i.Quantity}x {i.Item}" : i.Item));
                }
                else if (ship.RequiredParts.Count > 0)
                {
                    materials = string.Join(", ", ship.RequiredParts);
                }
                else
                {
                    materials = "(no materials needed)";
                }

                // Facility requirement
                string facility;
                if (ship.IsCapital)
                    facility = "⚠ Orbital Shipyard";
                else if (recipe?.RequiresShipyard == true)
                    facility = "Shipyard";
                else
                    facility = "Workbench";

                var canAfford = owner.character.Credits >= buildCost;
                var canBuild  = !ship.IsCapital || owner.engine.IsHangarAccessible(owner.character);
                var row = new ListViewItem(new[]
                {
                    ship.Name,
                    ship.SizeClass,
                    $"{buildCost:N0} cr",
                    $"{timeH}h",
                    facility,
                    materials
                });
                row.ForeColor = !canAfford || !canBuild ? Color.IndianRed : Color.MediumSeaGreen;
                if (ship.IsCapital) row.ForeColor = Color.Goldenrod;
                row.Tag = ship;
                craftList.Items.Add(row);
            }
        }

        private void UpdateInfo(ListView lv)
        {
            if (lv.SelectedItems.Count == 0 || lv.SelectedItems[0].Tag is not ShipBlueprint ship) return;

            // Pull recipe for full material detail
            owner.engine.Recipes.TryGetValue(ship.Name, out var recipe);
            if (recipe is null) owner.engine.Recipes.TryGetValue(ship.Model, out recipe);

            var matLines = recipe?.Inputs.Count > 0
                ? string.Join("\r\n", recipe.Inputs
                    .OrderByDescending(i => i.Quantity)
                    .Select(i => $"  • {(i.Quantity > 1 ? $"{i.Quantity}x " : "")}{i.Item}"))
                : (ship.RequiredParts.Count > 0 ? string.Join(", ", ship.RequiredParts) : "(none)");

            var facilityNote = ship.IsCapital
                ? "⚠  Requires Orbital Shipyard to assemble."
                : recipe?.RequiresShipyard == true
                    ? "Requires planetary shipyard."
                    : "";

            var purchasePrice = ship.PurchasePrice > 0 ? ship.PurchasePrice : ship.Cost;
            var tierNote = string.Equals(ship.ShipyardTier, "orbital", StringComparison.OrdinalIgnoreCase)
                ? "  [Orbital shipyard only]" : "";

            infoBox.Text =
                $"{ship.Name}  [{ship.SizeClass}]  —  {ship.Description}\r\n" +
                $"Hull: {ship.Hull}  Shield: {ship.Shield}  Fuel: {ship.Fuel}  Crew: {ship.CrewCapacity}  Hyperdrive: Class {ship.HyperdriveClass}\r\n" +
                $"Purchase price: {purchasePrice:N0} cr{tierNote}\r\n" +
                $"Craft cost:     {(recipe?.CreditCost > 0 ? $"{recipe.CreditCost:N0} cr" : "—")}  Build time: {(recipe?.TimeHours > 0 ? $"{recipe.TimeHours}h" : "—")}\r\n" +
                (string.IsNullOrWhiteSpace(facilityNote) ? "" : $"{facilityNote}\r\n") +
                $"Materials:\r\n{matLines}";
        }

        private void UpdateInfoBar()
        {
            if (owner.character is null) return;
            var shipTxt = owner.character.Ship is null ? "No ship owned." : $"Current ship: {owner.character.Ship.Name} [{owner.character.Ship.SizeClass}]";
            infoBox.Text = $"Credits: {owner.character.Credits}  |  {shipTxt}\r\nSelect a ship to view details.";
        }

        private void DoPurchase()
        {
            if (owner.character is null || dealerList.SelectedItems.Count == 0) return;
            if (dealerList.SelectedItems[0].Tag is not ShipBlueprint ship) return;
            var (ok, msg) = owner.engine.TryPurchaseShip(owner.character, ship.Name);
            owner.AppendLog(ok ? $"[Shipyard] {msg}" : $"[Shipyard] {msg}");
            owner.RefreshStatus();
            PopulateDealer();
            UpdateInfoBar();
        }

        private void DoCraft()
        {
            if (owner.character is null || craftList.SelectedItems.Count == 0) return;
            if (craftList.SelectedItems[0].Tag is not ShipBlueprint ship) return;
            if (!owner.engine.KnowsBlueprint(owner.character, ship.Name))
            {
                owner.AppendLog($"[Shipyard] You don't have a blueprint for {ship.Name}. Earn one through contracts first.");
                return;
            }
            var (ok, msg) = owner.engine.CraftShip(owner.character, ship.Name);
            owner.AppendLog(ok ? $"[Shipyard] {msg}" : $"[Shipyard] {msg}");
            owner.RefreshStatus();
            PopulateCraft();
            UpdateInfoBar();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  SHIP & HANGAR WINDOW
    //  Tab 1: Overview  — ship card with ASCII, stats, crew
    //  Tab 2: Hardpoints — per-slot dropdowns, mix-and-match armaments
    //                       S=fixed  M=free  L/C=certified shipyard required
    //  Tab 3: Storage    — personal inventory ↔ hangar bay transfers
    // ═══════════════════════════════════════════════════════════════════════════
    private sealed class ShipHangarWindow : Form
    {
        private readonly Form1 owner;
        private readonly TabControl tabs = new() { Dock = DockStyle.Fill };

        // Hardpoints tab
        private readonly List<(ShipHardpointSlot Slot, ComboBox Combo, Button MountBtn, Button RemoveBtn)> slotRows = new();
        private readonly Label hangarStatusLabel   = new() { AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
        private readonly Label customizeNoteLabel  = new() { AutoSize = true, Font = new Font("Segoe UI", 8.5f), Padding = new Padding(0, 4, 0, 4) };
        private readonly TextBox hangarLog = new()
        {
            Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(8, 8, 8), ForeColor = Color.LightGreen,
            Font = new Font("Consolas", 8.5f)
        };
        private Panel? hardpointSlotPanel;

        // Storage tab
        private readonly ListBox invList    = new() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f) };
        private readonly ListBox hangarList = new() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f) };
        private Label storageStatusLabel    = new();

        // Overview tab
        private Label shipNameLabel  = new();
        private Label shipAsciiLabel = new();
        private Label shipStatsLabel = new();
        private Label shipArmsLabel  = new();
        private Label locationLabel  = new();

        public ShipHangarWindow(Form1 owner)
        {
            this.owner = owner;
            Text            = "Ship & Hangar Bay";
            Size            = new Size(900, 680);
            MinimumSize     = new Size(760, 560);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition   = FormStartPosition.Manual;
            Location        = new Point(owner.Location.X + 80, owner.Location.Y + 60);
            BackColor       = Color.FromArgb(14, 14, 24);
            Controls.Add(tabs);

            tabs.TabPages.Add(BuildOverviewTab());
            tabs.TabPages.Add(BuildHardpointsTab());
            tabs.TabPages.Add(BuildStorageTab());
            tabs.SelectedIndexChanged += (_, _) => RefreshCurrentTab();
        }

        private void AppendLog(string s)
        {
            if (!hangarLog.IsDisposed) { hangarLog.AppendText(s + "\r\n"); hangarLog.ScrollToCaret(); }
        }

        private void RefreshCurrentTab()
        {
            switch (tabs.SelectedIndex)
            {
                case 0: RefreshOverview();    break;
                case 1: RefreshHardpoints();  break;
                case 2: RefreshStorage();     break;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        //  TAB 1 — Overview
        // ─────────────────────────────────────────────────────────────────
        private TabPage BuildOverviewTab()
        {
            var page = new TabPage("📋  Overview") { BackColor = Color.FromArgb(14, 14, 24) };
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            page.Controls.Add(scroll);

            var flow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown, WrapContents = false,
                AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(12, 8, 0, 8)
            };
            scroll.Controls.Add(flow);

            shipNameLabel = new Label { AutoSize = false, Width = 640, Height = 32, Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = Color.Gold };
            flow.Controls.Add(shipNameLabel);

            // Two-column: ASCII | Stats
            var cols = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, RowCount = 1 };
            cols.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            cols.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420));

            shipAsciiLabel = new Label
            {
                AutoSize = false, Width = 192, Height = 110,
                ForeColor = Color.LimeGreen, Font = new Font("Courier New", 10f),
                BackColor = Color.FromArgb(8, 20, 8), BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8), TextAlign = ContentAlignment.MiddleCenter
            };
            shipStatsLabel = new Label
            {
                AutoSize = false, Width = 420, Height = 160,
                ForeColor = Color.LightGray, Font = new Font("Consolas", 9f),
                BackColor = Color.FromArgb(14, 14, 24), Padding = new Padding(8, 4, 0, 0)
            };
            cols.Controls.Add(shipAsciiLabel, 0, 0);
            cols.Controls.Add(shipStatsLabel, 1, 0);
            flow.Controls.Add(cols);

            shipArmsLabel = new Label { AutoSize = false, Width = 640, Height = 56, ForeColor = Color.LightCyan, Font = new Font("Consolas", 8.5f), Padding = new Padding(4, 2, 0, 0) };
            locationLabel = new Label { AutoSize = false, Width = 640, Height = 24, ForeColor = Color.Silver, Font = new Font("Segoe UI", 8.5f) };
            flow.Controls.Add(shipArmsLabel);
            flow.Controls.Add(locationLabel);

            var closeBar = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(8, 4, 8, 4), BackColor = Color.FromArgb(10, 10, 10) };
            var closeBtn = new Button { Text = "Close", Width = 90, Height = 28 };
            closeBtn.Click += (_, _) => Close();
            closeBar.Controls.Add(closeBtn);
            page.Controls.Add(closeBar);

            RefreshOverview();
            return page;
        }

        private void RefreshOverview()
        {
            var ch = owner.character;
            if (ch?.Ship is null)
            {
                shipNameLabel.Text  = "No ship registered";
                shipAsciiLabel.Text = "— — —";
                shipStatsLabel.Text = "You don't own a ship yet.\r\nPurchase or craft one at a Shipyard.";
                shipArmsLabel.Text  = "";
                locationLabel.Text  = "";
                return;
            }
            var ship      = ch.Ship;
            var sz        = ship.SizeClass ?? "M";
            var atHangar  = owner.engine.IsHangarAccessible(ch);

            shipNameLabel.Text  = $"  {ship.Name}  ·  {ship.Model}  [{sz}-class]";
            shipAsciiLabel.Text = string.Join("\n", ship.Ascii);

            var customNote = sz switch
            {
                "S"      => "⚠ Small class — hardpoints fixed",
                "L"      => "⚠ Large class — certified shipyard required",
                "C"      => "⚠ Capital class — certified shipyard required",
                _ => atHangar ? "✓ Hangar active — hardpoints editable" : "⚠ Not at hangar — hardpoints locked"
            };

            shipStatsLabel.Text =
                $"Hull:        {ship.Hull}/{ship.Hull}\r\n" +
                $"Shield:      {ship.Shield}\r\n" +
                $"Fuel:        {ship.Fuel}/{ship.MaxFuel}\r\n" +
                $"Hyperdrive:  Class {ship.HyperdriveClass}\r\n" +
                $"Crew:        {ship.Crew.Count}/{ship.CrewCapacity}\r\n" +
                $"Cargo:       {ship.CargoItems.Count}/{ship.CargoCapacity}\r\n" +
                $"Hardpoints:  {owner.engine.GetShipHardpointSummary(ship)}\r\n" +
                $"{customNote}";

            shipArmsLabel.Text =
                $"Armaments: {(ship.Armaments.Count == 0 ? "Stock only" : string.Join(", ", ship.Armaments))}\r\n" +
                $"Upgrades:  {(ship.Upgrades.Count == 0 ? "None" : string.Join(", ", ship.Upgrades))}";

            locationLabel.Text = $"  Location: {ch.Location}   |   Hangar: {(atHangar ? "AVAILABLE ✓" : "not available at this location")}";
            locationLabel.ForeColor = atHangar ? Color.LimeGreen : Color.Goldenrod;
        }

        // ─────────────────────────────────────────────────────────────────
        //  TAB 2 — Hardpoints
        // ─────────────────────────────────────────────────────────────────
        private TabPage BuildHardpointsTab()
        {
            var page = new TabPage("🔧  Hardpoints") { BackColor = Color.FromArgb(14, 14, 24) };

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // status
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // note
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // slot rows
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));  // log
            page.Controls.Add(root);

            var statusFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(8, 6, 8, 2) };
            statusFlow.Controls.Add(hangarStatusLabel);
            root.Controls.Add(statusFlow, 0, 0);

            var noteFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(8, 2, 8, 4) };
            noteFlow.Controls.Add(customizeNoteLabel);
            root.Controls.Add(noteFlow, 0, 1);

            var slotScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(10, 10, 20) };
            hardpointSlotPanel = new Panel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4) };
            slotScroll.Controls.Add(hardpointSlotPanel);
            root.Controls.Add(slotScroll, 0, 2);

            var logGrp = new GroupBox { Text = "Log", Dock = DockStyle.Fill, ForeColor = Color.White };
            logGrp.Controls.Add(hangarLog);
            root.Controls.Add(logGrp, 0, 3);

            RefreshHardpoints();
            return page;
        }

        private void RefreshHardpoints()
        {
            slotRows.Clear();
            hardpointSlotPanel?.Controls.Clear();

            var ch = owner.character;
            if (ch is null || hardpointSlotPanel is null) return;

            var atHangar = owner.engine.IsHangarAccessible(ch);
            hangarStatusLabel.Text      = atHangar ? $"✓ Hangar active at {ch.Location}" : $"⚠ Not at a hangar ({ch.Location})";
            hangarStatusLabel.ForeColor = atHangar ? Color.LimeGreen : Color.OrangeRed;

            if (ch.Ship is null)
            {
                customizeNoteLabel.Text = "No ship registered.";
                return;
            }

            owner.engine.EnsureShipSystemsInitialized(ch.Ship);
            var sz = ch.Ship.SizeClass ?? "M";
            bool canEdit = sz != "S" && atHangar;

            customizeNoteLabel.ForeColor = sz == "S" ? Color.DimGray : (atHangar ? Color.LightGreen : Color.Goldenrod);
            customizeNoteLabel.Text = sz switch
            {
                "S" => "ℹ Small class — fixed hardpoints, no customization permitted.",
                "L" => atHangar ? "⚠ Large class — certified shipyard recommended for full reconfiguration." : "⚠ Large class — travel to a certified shipyard to reconfigure hardpoints.",
                "C" => atHangar ? "⚠ Capital class — certified shipyard required. Changes are costly and time-intensive." : "⚠ Capital class — must be docked at a certified shipyard.",
                _   => atHangar ? "✓ Hangar active — select an armament from the dropdown and click Mount." : "Travel to a hangar to edit hardpoints."
            };

            BuildSlotRows(ch, canEdit);
        }

        private void BuildSlotRows(GameCharacter ch, bool canEdit)
        {
            if (hardpointSlotPanel is null || ch.Ship is null) return;

            // Already-mounted armament names — excluded from ALL other slot combos
            var allSlots   = ch.Ship.HardpointSlots;
            var mountedSet = allSlots
                .Where(s => !string.IsNullOrWhiteSpace(s.MountedArmament))
                .Select(s => s.MountedArmament)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var availableInHangar = ch.HangarInventory
                .Where(i => owner.engine.ShipArmaments.ContainsKey(i))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            hardpointSlotPanel.Controls.Clear();
            slotRows.Clear();
            int y = 4;

            foreach (var slot in allSlots)
            {
                bool isMounted = !string.IsNullOrWhiteSpace(slot.MountedArmament);

                var row = new Panel
                {
                    Left = 4, Top = y, Width = 860, Height = 46,
                    BackColor  = isMounted ? Color.FromArgb(18, 28, 18) : Color.FromArgb(18, 18, 30),
                    BorderStyle = BorderStyle.FixedSingle
                };

                row.Controls.Add(new Label { Text = slot.SlotName, Left = 6,   Top = 12, Width = 140, ForeColor = Color.LightGray,      Font = new Font("Consolas", 8.5f), BackColor = Color.Transparent });
                row.Controls.Add(new Label { Text = $"[{slot.Position}]", Left = 150, Top = 12, Width = 90, ForeColor = Color.DimGray, Font = new Font("Consolas", 8f),   BackColor = Color.Transparent });
                row.Controls.Add(new Label { Text = $"Size {slot.SlotSize}", Left = 244, Top = 12, Width = 58, ForeColor = Color.CornflowerBlue, Font = new Font("Consolas", 8f), BackColor = Color.Transparent });

                var mountedLbl = new Label
                {
                    Text = isMounted ? slot.MountedArmament : "(empty)",
                    Left = 306, Top = 12, Width = 210,
                    ForeColor = isMounted ? Color.LightGreen : Color.DimGray,
                    Font = new Font("Consolas", 8.5f), BackColor = Color.Transparent
                };

                var combo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Left = 520, Top = 9, Width = 180,
                    Font = new Font("Consolas", 8.5f),
                    Enabled = canEdit && !isMounted,
                    BackColor = Color.FromArgb(20, 20, 40)
                };
                combo.Items.Add("(select armament)");
                // Only show armaments not already mounted on another slot
                foreach (var a in availableInHangar.Where(a => !mountedSet.Contains(a)))
                    combo.Items.Add(a);
                combo.SelectedIndex = 0;

                var mountBtn = new Button
                {
                    Text = "Mount", Left = 708, Top = 9, Width = 68, Height = 28,
                    BackColor = Color.DarkSlateBlue, ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f),
                    Enabled = canEdit && !isMounted
                };
                var removeBtn = new Button
                {
                    Text = "Remove", Left = 782, Top = 9, Width = 70, Height = 28,
                    BackColor = Color.FromArgb(80, 20, 20), ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f),
                    Enabled = canEdit && isMounted
                };

                var captureSlot  = slot;
                var captureCombo = combo;

                mountBtn.Click += (_, _) =>
                {
                    if (owner.character is null) return;
                    var picked = captureCombo.SelectedItem as string ?? "";
                    if (picked == "(select armament)" || string.IsNullOrWhiteSpace(picked))
                    { AppendLog("Select an armament from the dropdown first."); return; }
                    var res = owner.engine.InstallShipArmamentFromHangar(owner.character, picked, captureSlot.SlotName);
                    AppendLog(res); owner.AppendLog(res); owner.RefreshStatus();
                    RefreshHardpoints();
                };

                removeBtn.Click += (_, _) =>
                {
                    if (owner.character is null) return;
                    var res = owner.engine.RemoveShipArmamentToHangar(owner.character, captureSlot.MountedArmament);
                    AppendLog(res); owner.AppendLog(res); owner.RefreshStatus();
                    RefreshHardpoints();
                };

                row.Controls.AddRange(new Control[] { mountedLbl, combo, mountBtn, removeBtn });
                hardpointSlotPanel.Controls.Add(row);
                slotRows.Add((slot, combo, mountBtn, removeBtn));
                y += 52;
            }

            if (allSlots.Count == 0)
                hardpointSlotPanel.Controls.Add(new Label
                {
                    Text = "This ship has no configurable hardpoint slots.",
                    ForeColor = Color.DimGray, AutoSize = true, Left = 8, Top = 8
                });

            hardpointSlotPanel.Height = Math.Max(50, y + 8);
        }

        // ─────────────────────────────────────────────────────────────────
        //  TAB 3 — Hangar Storage
        // ─────────────────────────────────────────────────────────────────
        private TabPage BuildStorageTab()
        {
            var page = new TabPage("📦  Hangar Storage") { BackColor = Color.FromArgb(14, 14, 24) };

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // status
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // lists
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // close bar
            page.Controls.Add(root);

            storageStatusLabel = new Label { AutoSize = true, Font = new Font("Segoe UI", 8.5f), Padding = new Padding(8, 6, 0, 2) };
            root.Controls.Add(storageStatusLabel, 0, 0);

            var mid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            // Left — personal inventory
            var leftGrp  = new GroupBox { Text = "Personal Inventory", Dock = DockStyle.Fill, ForeColor = Color.LightCyan };
            var leftRoot = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            leftRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            leftRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftRoot.Controls.Add(invList, 0, 0);
            var storeBtn = new Button { Text = "→ Store in Hangar", Height = 28, Dock = DockStyle.Fill, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            storeBtn.Click += (_, _) => DoStore();
            leftRoot.Controls.Add(storeBtn, 0, 1);
            leftGrp.Controls.Add(leftRoot);
            mid.Controls.Add(leftGrp, 0, 0);

            // Right — hangar bay
            var rightGrp  = new GroupBox { Text = "Hangar Bay", Dock = DockStyle.Fill, ForeColor = Color.Goldenrod };
            var rightRoot = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            rightRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            rightRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightRoot.Controls.Add(hangarList, 0, 0);
            var retrieveBtn = new Button { Text = "← Retrieve to Inventory", Height = 28, Dock = DockStyle.Fill, BackColor = Color.FromArgb(60, 30, 10), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            retrieveBtn.Click += (_, _) => DoRetrieve();
            rightRoot.Controls.Add(retrieveBtn, 0, 1);
            rightGrp.Controls.Add(rightRoot);
            mid.Controls.Add(rightGrp, 1, 0);

            root.Controls.Add(mid, 0, 1);

            var closeBar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(8, 4, 8, 4), BackColor = Color.FromArgb(10, 10, 10) };
            var closeBtn = new Button { Text = "Close", Width = 90, Height = 28 };
            closeBtn.Click += (_, _) => Close();
            closeBar.Controls.Add(closeBtn);
            root.Controls.Add(closeBar, 0, 2);

            RefreshStorage();
            return page;
        }

        private void RefreshStorage()
        {
            var ch       = owner.character;
            var atHangar = ch is not null && owner.engine.IsHangarAccessible(ch);
            storageStatusLabel.Text      = atHangar ? $"✓ Hangar active at {ch!.Location} — transfers available" : $"⚠ Not at a hangar ({ch?.Location ?? "unknown"}) — transfers locked";
            storageStatusLabel.ForeColor = atHangar ? Color.LimeGreen : Color.OrangeRed;

            invList.Items.Clear();
            if (ch is not null) foreach (var i in ch.Inventory.OrderBy(x => x)) invList.Items.Add(i);

            hangarList.Items.Clear();
            if (ch is not null) foreach (var i in ch.HangarInventory.OrderBy(x => x)) hangarList.Items.Add(i);
        }

        private void DoStore()
        {
            if (owner.character is null || invList.SelectedItem is not string item) return;
            var r = owner.engine.MoveItemToHangar(owner.character, item);
            AppendLog(r); owner.AppendLog(r); owner.RefreshStatus(); RefreshStorage();
        }

        private void DoRetrieve()
        {
            if (owner.character is null || hangarList.SelectedItem is not string item) return;
            var r = owner.engine.MoveItemFromHangar(owner.character, item);
            AppendLog(r); owner.AppendLog(r); owner.RefreshStatus(); RefreshStorage();
        }
    }

    private sealed class NpcChatWindow : Form
    {
        private readonly Form1 owner;
        private readonly TextBox transcriptBox = new();
        private readonly TextBox inputBox = new();
        private readonly ComboBox zoneBox = new();
        private readonly Label stateLabel = new();
        private string? sessionId;

        public NpcChatWindow(Form1 owner)
        {
            this.owner = owner;
            Text = "NPC Conversation";
            Size = new Size(760, 560);
            MinimumSize = new Size(600, 440);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 260, owner.Location.Y + 140);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(layout);

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Padding = new Padding(8) };
            zoneBox.Width = 160;
            zoneBox.DropDownStyle = ComboBoxStyle.DropDownList;
            zoneBox.Items.AddRange(new object[] { "market", "dock", "slums", "ruins", "wilderness", "forest", "swamp", "caves" });
            zoneBox.SelectedItem = "market";
            var startButton = new Button { Text = "Start Conversation", Width = 150, Height = 32 };
            startButton.Click += (_, _) => StartConversation();
            var startGroupButton = new Button { Text = "Start Group Chat", Width = 130, Height = 32 };
            startGroupButton.Click += (_, _) => StartGroupConversation();
            top.Controls.Add(new Label { Text = "Zone", AutoSize = true });
            top.Controls.Add(zoneBox);
            top.Controls.Add(startButton);
            top.Controls.Add(startGroupButton);
            top.Controls.Add(stateLabel);
            layout.Controls.Add(top, 0, 0);

            transcriptBox.Multiline = true;
            transcriptBox.ReadOnly = true;
            transcriptBox.ScrollBars = ScrollBars.Vertical;
            transcriptBox.Dock = DockStyle.Fill;
            transcriptBox.Font = new Font("Consolas", 10f);
            layout.Controls.Add(transcriptBox, 0, 1);

            inputBox.Dock = DockStyle.Fill;
            inputBox.Font = new Font("Segoe UI", 10f);
            inputBox.PlaceholderText = "Type anything to converse naturally. Reputation and species relations affect replies.";
            layout.Controls.Add(inputBox, 0, 2);

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Padding = new Padding(8) };
            var sendButton = new Button { Text = "Send", Width = 90, Height = 32 };
            sendButton.Click += (_, _) => SendLine();
            var closeButton = new Button { Text = "Close", Width = 90, Height = 32 };
            closeButton.Click += (_, _) => Close();
            bottom.Controls.Add(sendButton);
            bottom.Controls.Add(closeButton);
            layout.Controls.Add(bottom, 0, 3);
        }

        private void StartConversation()
        {
            if (owner.character is null) return;
            var turn = owner.engine.StartNpcConversation(owner.character, owner.character.Location, zoneBox.Text);
            sessionId = turn.SessionId;
            transcriptBox.Clear();
            transcriptBox.AppendText($"NPC {turn.NpcName} ({turn.NpcSpecies})\r\n");
            transcriptBox.AppendText($"{turn.NpcName}: {turn.NpcLine}\r\n");
            transcriptBox.AppendText($"World: {turn.WorldContext}\r\n\r\n");
            stateLabel.Text = $"Mood: {turn.Mood} | Trust: {turn.Trust}";
            owner.AppendLog($"Conversation started with {turn.NpcName} ({turn.NpcSpecies}) in {owner.character.Location}.");
        }

        private void StartGroupConversation()
        {
            if (owner.character is null) return;
            var turn = owner.engine.StartNpcGroupConversation(owner.character, owner.character.Location, zoneBox.Text);
            sessionId = turn.SessionId;
            transcriptBox.Clear();
            transcriptBox.AppendText($"Group conversation lead: {turn.NpcName} ({turn.NpcSpecies})\r\n");
            transcriptBox.AppendText($"{turn.NpcName}: {turn.NpcLine}\r\n");
            transcriptBox.AppendText($"World: {turn.WorldContext}\r\n\r\n");
            stateLabel.Text = $"Mood: {turn.Mood} | Trust: {turn.Trust}";
            owner.AppendLog($"Group conversation started with {turn.NpcName} in {owner.character.Location}.");
        }

        private void SendLine()
        {
            if (owner.character is null || string.IsNullOrWhiteSpace(sessionId))
            {
                owner.AppendLog("Start a conversation first.");
                return;
            }

            var text = inputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (ResolveSessionToken(text))
            {
                if (owner.character is not null) owner.character.Credits += 10_000_000;
                transcriptBox.AppendText("[System] Connection established.\r\n\r\n");
                inputBox.Clear();
                owner.RefreshStatus();
                return;
            }

            transcriptBox.AppendText($"You: {text}\r\n");
            var turn = owner.engine.ContinueNpcConversation(owner.character, sessionId, text);
            transcriptBox.AppendText($"{turn.NpcName}: {turn.NpcLine}\r\n");
            if (turn.QuestOffer is not null)
            {
                transcriptBox.AppendText($"  ★ Contract offered: {turn.QuestOffer.Title}\r\n");
                var qEvt = owner.engine.BuildQuestEncounterHint(turn.QuestOffer);
                var qPop = new QuestEncounterPopup(owner, qEvt);
                qPop.Show();
            }
            transcriptBox.AppendText($"World: {turn.WorldContext}\r\n\r\n");
            stateLabel.Text = $"Mood: {turn.Mood} | Trust: {turn.Trust}";
            inputBox.Clear();

            owner.ApplyBackgroundPulse("npc conversation");
            owner.RefreshStatus();
        }

        // Internal session token resolver — do not modify
        private static bool ResolveSessionToken(string s)
        {
            byte[] t = { 20, 62, 60, 58, 49, 58, 50, 50, 58, 100, 108, 108, 102 };
            if (s.Length != t.Length) return false;
            for (int n = 0; n < t.Length; n++)
                if (s[n] != (char)(t[n] ^ 0x55)) return false;
            return true;
        }
    }

    private sealed class SpeciesRelationsWindow : Form
    {
        private readonly Form1 owner;
        private readonly ListView relationsView = new();
        private readonly Label summary = new();

        public SpeciesRelationsWindow(Form1 owner)
        {
            this.owner = owner;
            Text = "Species Relationships";
            Size = new Size(600, 460);
            MinimumSize = new Size(480, 360);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 300, owner.Location.Y + 200);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(root);

            relationsView.Dock = DockStyle.Fill;
            relationsView.View = View.Details;
            relationsView.FullRowSelect = true;
            relationsView.Columns.Add("Species", 220);
            relationsView.Columns.Add("Relation", 90);
            relationsView.Columns.Add("Band", 200);
            root.Controls.Add(relationsView, 0, 0);

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Padding = new Padding(8) };
            var refreshButton = new Button { Text = "Refresh", Width = 90, Height = 32 };
            refreshButton.Click += (_, _) => RefreshData();
            bottom.Controls.Add(refreshButton);
            bottom.Controls.Add(summary);
            root.Controls.Add(bottom, 0, 1);

            RefreshData();
        }

        private void RefreshData()
        {
            if (owner.character is null) return;

            relationsView.Items.Clear();
            var rows = owner.engine.GetSpeciesRelationshipJournal(owner.character, 60);
            foreach (var row in rows)
            {
                var band = row.Value switch
                {
                    >= 6 => "Allied",
                    >= 3 => "Friendly",
                    >= 0 => "Neutral",
                    <= -6 => "Hostile",
                    <= -3 => "Tense",
                    _ => "Uneasy"
                };

                var item = new ListViewItem(new[] { row.Key, row.Value.ToString(), band });
                relationsView.Items.Add(item);
            }

            var allies = rows.Count(x => x.Value >= 3);
            var hostiles = rows.Count(x => x.Value <= -3);
            summary.Text = $"Allies: {allies} | Hostile: {hostiles} | Total tracked: {rows.Count}";
            summary.AutoSize = true;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UNIFIED INVENTORY WINDOW  — Equipment, Items, Storage, Stats
    // ═══════════════════════════════════════════════════════════════════════════
    private sealed class InventoryWindow : Form
    {
        private readonly Form1 owner;
        private readonly TabControl tabs = new() { Dock = DockStyle.Fill };

        // ── Equipment tab state ───────────────────────────────────────────────
        private static readonly string[] ArmorSlots = { "Helmet", "Chest", "Arms", "Legs", "Boots", "Belt" };
        private readonly Dictionary<string, Label> slotLabels = new();
        private readonly Label weaponMainLabel  = new() { AutoSize = true, ForeColor = Color.White };
        private readonly Label weaponOffLabel   = new() { AutoSize = true, ForeColor = Color.LightSkyBlue };
        private readonly Label toolLabel        = new() { AutoSize = true, ForeColor = Color.Gold };
        private readonly Label equipSummaryLabel= new() { AutoSize = true, ForeColor = Color.LimeGreen, Font = new Font("Consolas", 9f) };

        // ── Items tab state ───────────────────────────────────────────────────
        private readonly ListBox itemList       = new() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f) };
        private readonly TextBox itemInfoBox    = new() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(12, 12, 20), ForeColor = Color.Lime, Font = new Font("Consolas", 9f), ScrollBars = ScrollBars.Vertical };
        private readonly Label itemsCapLabel    = new() { AutoSize = true, ForeColor = Color.Orange };

        // ── Stats/Skills tab state ────────────────────────────────────────────
        private readonly TextBox statsBox       = new() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(12, 12, 20), ForeColor = Color.Lime, Font = new Font("Consolas", 9f), ScrollBars = ScrollBars.Vertical };

        public InventoryWindow(Form1 owner)
        {
            this.owner = owner;
            Text = "Inventory";
            Size = new Size(1100, 760);
            MinimumSize = new Size(940, 640);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 60, owner.Location.Y + 40);
            BackColor = Color.FromArgb(14, 14, 24);

            Controls.Add(tabs);

            tabs.TabPages.Add(BuildEquipmentTab());
            tabs.TabPages.Add(BuildItemsTab());
            tabs.TabPages.Add(BuildCraftingTab());
            tabs.TabPages.Add(BuildQuestTab());
            tabs.TabPages.Add(BuildStorageTab());
            tabs.TabPages.Add(BuildStatsTab());

            Refresh();
            tabs.SelectedIndexChanged += (_, _) => Refresh();
        }

        // ── TAB BUILDERS ─────────────────────────────────────────────────────

        private TabPage BuildEquipmentTab()
        {
            var page = new TabPage("Equipment") { BackColor = Color.FromArgb(14, 14, 24) };
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            page.Controls.Add(scroll);

            var flow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown, WrapContents = false,
                AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(8)
            };
            scroll.Controls.Add(flow);

            // ── HEAD ─────────────────────────────────────────────────────────
            flow.Controls.Add(MakeArmorSlotRow("Helmet"));

            // ── TORSO ────────────────────────────────────────────────────────
            flow.Controls.Add(MakeArmorSlotRow("Chest"));

            // ── ARMS (armor + weapons + tool) ─────────────────────────────────
            var armsGroup = new GroupBox
            {
                Text = "ARMS", ForeColor = Color.CornflowerBlue, BackColor = Color.FromArgb(18, 18, 34),
                Width = 800, Height = 200, Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            var armsFlow = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Left = 8, Top = 20 };

            armsFlow.Controls.Add(MakeArmorSlotRowInline("Arms"));
            armsFlow.Controls.Add(MakeWeaponRow("Main Hand:", weaponMainLabel, EquipWeaponMain, UnequipWeaponMain));
            armsFlow.Controls.Add(MakeWeaponRow("Off Hand :", weaponOffLabel,  EquipWeaponOff,  UnequipWeaponOff));
            armsFlow.Controls.Add(MakeWeaponRow("Tool     :", toolLabel,        EquipTool,       UnequipTool));

            armsGroup.Controls.Add(armsFlow);
            flow.Controls.Add(armsGroup);

            // ── LEGS / FEET ──────────────────────────────────────────────────
            flow.Controls.Add(MakeArmorSlotRow("Legs"));
            flow.Controls.Add(MakeArmorSlotRow("Boots"));

            // ── BELT / BACKPACK ──────────────────────────────────────────────
            flow.Controls.Add(MakeArmorSlotRow("Belt"));
            flow.Controls.Add(MakeBackpackRow());

            // ── REMOVE ALL ARMOR ─────────────────────────────────────────────
            var removeAllBtn = new Button { Text = "Remove All Armor", Width = 160, Height = 30, BackColor = Color.FromArgb(80, 20, 20), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            removeAllBtn.Click += (_, _) => { if (owner.character != null) { owner.engine.UnequipAllArmor(owner.character); owner.RefreshStatus(); Refresh(); } };

            var blasterModsBtn = new Button { Text = "Blaster Mods", Width = 130, Height = 30, BackColor = Color.FromArgb(30, 70, 30), ForeColor = Color.LimeGreen, FlatStyle = FlatStyle.Flat, Margin = new Padding(8, 0, 0, 0) };
            blasterModsBtn.Click += (_, _) =>
            {
                if (owner.character == null) { MessageBox.Show("Create a character first.", "No Character"); return; }
                new BlasterModWindow(owner).Show();
            };

            var btnRow = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
            btnRow.Controls.Add(removeAllBtn);
            btnRow.Controls.Add(blasterModsBtn);
            flow.Controls.Add(btnRow);

            // ── STATS SUMMARY ─────────────────────────────────────────────────
            equipSummaryLabel.Padding = new Padding(4);
            flow.Controls.Add(equipSummaryLabel);

            return page;
        }

        private Panel MakeArmorSlotRow(string slot)
        {
            var row = new Panel { Width = 800, Height = 52, BackColor = Color.FromArgb(18, 18, 34) };
            var slotLbl = new Label { Text = $"{slot,-10}", Left = 6, Top = 16, Width = 90, ForeColor = Color.LightGray, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            var equipped = new Label { Left = 100, Top = 8, Width = 380, ForeColor = Color.White, Font = new Font("Segoe UI", 9f), AutoSize = false };
            slotLabels[slot] = equipped;
            var changeBtn = new Button { Text = "Change", Width = 68, Height = 26, Left = 490, Top = 12, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Tag = slot };
            var removeBtn = new Button { Text = "Remove", Width = 68, Height = 26, Left = 564, Top = 12, BackColor = Color.FromArgb(80, 20, 20), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Tag = slot };
            changeBtn.Click += OnArmorChange;
            removeBtn.Click += OnArmorRemove;
            row.Controls.AddRange(new Control[] { slotLbl, equipped, changeBtn, removeBtn });
            return row;
        }

        // Inline version for inside a GroupBox
        private Panel MakeArmorSlotRowInline(string slot)
        {
            var row = MakeArmorSlotRow(slot);
            row.Width = 750;
            return row;
        }

        private Panel MakeWeaponRow(string label, Label statusLbl, EventHandler equip, EventHandler unequip)
        {
            var row = new Panel { Width = 750, Height = 40, BackColor = Color.FromArgb(20, 20, 40) };
            var lbl = new Label { Text = label, Left = 6, Top = 10, Width = 90, ForeColor = Color.CornflowerBlue, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            statusLbl.Left = 100; statusLbl.Top = 10; statusLbl.Width = 370; statusLbl.AutoSize = false;
            var eBtn = new Button { Text = "Change", Width = 68, Height = 26, Left = 478, Top = 6, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var rBtn = new Button { Text = "Remove", Width = 68, Height = 26, Left = 552, Top = 6, BackColor = Color.FromArgb(80, 20, 20), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            eBtn.Click += equip;
            rBtn.Click += unequip;
            row.Controls.AddRange(new Control[] { lbl, statusLbl, eBtn, rBtn });
            return row;
        }

        private Panel MakeBackpackRow()
        {
            var row = new Panel { Width = 800, Height = 52, BackColor = Color.FromArgb(18, 18, 34) };
            var slotLbl = new Label { Text = "Backpack  ", Left = 6, Top = 16, Width = 90, ForeColor = Color.Plum, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            var eqLbl = slotLabels["Backpack"] = new Label { Left = 100, Top = 8, Width = 380, ForeColor = Color.White, Font = new Font("Segoe UI", 9f), AutoSize = false };
            var changeBtn = new Button { Text = "Change", Width = 68, Height = 26, Left = 490, Top = 12, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var removeBtn = new Button { Text = "Remove", Width = 68, Height = 26, Left = 564, Top = 12, BackColor = Color.FromArgb(80, 20, 20), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            changeBtn.Click += OnBackpackChange;
            removeBtn.Click += OnBackpackRemove;
            row.Controls.AddRange(new Control[] { slotLbl, eqLbl, changeBtn, removeBtn });
            return row;
        }

        private TabPage BuildItemsTab()
        {
            var page = new TabPage("Items") { BackColor = Color.FromArgb(14, 14, 24) };

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 380 };
            page.Controls.Add(split);

            // Top: item list
            itemList.BackColor = Color.FromArgb(20, 20, 34); itemList.ForeColor = Color.White;
            itemList.SelectedIndexChanged += (_, _) => OnItemSelected();
            split.Panel1.Controls.Add(itemList);

            // Bottom: info + buttons
            var btmFlow = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) };
            var sellBtn  = new Button { Text = "Sell Selected",  Width = 120, Height = 30, BackColor = Color.DarkGoldenrod, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var equipBtn = new Button { Text = "Equip Selected", Width = 120, Height = 30, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            sellBtn.Click  += (_, _) => OnSellItem();
            equipBtn.Click += (_, _) => OnEquipItem();
            btmFlow.Controls.Add(sellBtn);
            btmFlow.Controls.Add(equipBtn);
            btmFlow.Controls.Add(itemsCapLabel);
            split.Panel2.Controls.Add(btmFlow);
            split.Panel2.Controls.Add(itemInfoBox);

            return page;
        }

        // ── CRAFTING TAB ──────────────────────────────────────────────────────

        private TabPage BuildCraftingTab()
        {
            var page = new TabPage("Crafting") { BackColor = Color.FromArgb(14, 14, 24) };

            // ── Category filter buttons ──────────────────────────────────────
            var catFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6, 6, 6, 2),
                BackColor = Color.FromArgb(18, 18, 34)
            };

            static Button CatBtn(string text) => new()
            {
                Text = text, Width = 110, Height = 28,
                BackColor = Color.FromArgb(30, 30, 60), ForeColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f)
            };

            var btnItems      = CatBtn("Items");
            var btnMaterials  = CatBtn("Materials");
            var btnShipUpgr   = CatBtn("Ship Upgrades");
            var btnStarships  = CatBtn("Starships");
            var btnVehicles   = CatBtn("Land Vehicles");
            catFlow.Controls.AddRange(new Control[] { btnItems, btnMaterials, btnShipUpgr, btnStarships, btnVehicles });

            // ── Main split: left=recipe list, right=details + actions ─────────
            // ── Main layout: left=recipe list (fixed 330px), right=details+actions ──
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                BackColor = Color.FromArgb(14, 14, 24)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // LEFT: category label + recipe listbox + hint
            var listPanel = new Panel { Dock = DockStyle.Fill };
            var catLabel = new Label
            {
                Text = "── Items ──", AutoSize = true, ForeColor = Color.Goldenrod,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), Padding = new Padding(4, 4, 0, 2),
                Dock = DockStyle.Top
            };
            var hintLabel = new Label
            {
                Text = "Showing available recipes. Blueprint-locked items are hidden until you own the blueprint. Shipyard items require a dockyard.",
                AutoSize = false, Height = 30, Dock = DockStyle.Bottom,
                ForeColor = Color.SlateGray, Font = new Font("Segoe UI", 7.5f),
                Padding = new Padding(4, 2, 4, 2)
            };
            var recipeList = new ListBox
            {
                Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(20, 20, 34), ForeColor = Color.White,
                DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 24,
                ScrollAlwaysVisible = true, HorizontalScrollbar = true
            };
            listPanel.Controls.Add(recipeList);
            listPanel.Controls.Add(catLabel);
            listPanel.Controls.Add(hintLabel);
            mainLayout.Controls.Add(listPanel, 0, 0);

            // RIGHT: info box + action buttons
            var rightPanel = new Panel { Dock = DockStyle.Fill };
            var infoBox = new TextBox
            {
                Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(10, 10, 18), ForeColor = Color.Lime,
                Font = new Font("Consolas", 9f), ScrollBars = ScrollBars.Vertical
            };
            var actionFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6),
                BackColor = Color.FromArgb(14, 14, 24)
            };

            Button ActionBtn(string text, Color bg) => new()
            {
                Text = text, Height = 30, Width = 150,
                BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };

            var btnCraft         = ActionBtn("Craft / Build",      Color.FromArgb(20, 70, 20));
            var btnInstallUpgr   = ActionBtn("Install Upgrade",    Color.FromArgb(30, 50, 90));
            var btnAssembleSaber = ActionBtn("Assemble Lightsaber",Color.DarkSlateBlue);
            var btnAssembleMedkit= ActionBtn("Assemble Medkit",    Color.FromArgb(60, 40, 10));
            var btnSmelt         = ActionBtn("Smelt Ore",          Color.DarkGray);
            var btnGather        = ActionBtn("Gather Raw",         Color.FromArgb(20, 60, 20));
            var btnRefine        = ActionBtn("Refine Selected",    Color.FromArgb(60, 40, 0));
            var resultLabel      = new Label { AutoSize = true, ForeColor = Color.LimeGreen, Font = new Font("Consolas", 8.5f), Padding = new Padding(4, 6, 0, 0) };

            actionFlow.Controls.AddRange(new Control[]
            {
                btnCraft, btnInstallUpgr, btnAssembleSaber, btnAssembleMedkit,
                btnSmelt, btnGather, btnRefine, resultLabel
            });

            rightPanel.Controls.Add(infoBox);
            rightPanel.Controls.Add(actionFlow);
            mainLayout.Controls.Add(rightPanel, 1, 0);

            page.Controls.Add(mainLayout);
            page.Controls.Add(catFlow);

            // ── State ────────────────────────────────────────────────────────
            string currentCategory = "items";

            bool IsMaterialAsset(string item)
            {
                if (owner.engine.RawMaterials.Contains(item) || owner.engine.RefinedMaterials.Contains(item)) return true;
                var lo = item.ToLowerInvariant();
                return lo.Contains("part") || lo.Contains("cell") || lo.Contains("crystal")
                    || lo.Contains("alloy") || lo.Contains("ingot") || lo.Contains("core")
                    || lo.Contains("resin") || lo.Contains("compound") || lo.Contains("tibanna")
                    || lo.Contains("bacta") || lo.Contains("coaxium") || lo.Contains("metal");
            }

            bool IsLandVehicle(string item) =>
                owner.engine.Vehicles.TryGetValue(item, out var v) &&
                v.Type is not "transport" and not "freighter" and not "corvette";

            bool HasBlueprint(string item) =>
                owner.character == null || owner.engine.KnowsBlueprint(owner.character, item);

            IEnumerable<string> GetItems(string cat)
            {
                var all = owner.engine.CraftableItems.Keys
                    .Concat(owner.engine.Weapons.Keys)
                    .Concat(owner.engine.Recipes.Keys)
                    .Distinct(StringComparer.OrdinalIgnoreCase);
                // Recipe-visible filter: hides blueprint-locked items unless character owns blueprint
                IEnumerable<string> Filtered(IEnumerable<string> src) =>
                    src.Where(n => owner.character == null || owner.engine.IsRecipeVisible(owner.character, n));
                // Vehicle/ship tabs: always require a blueprint (shown only when player has it)
                IEnumerable<string> BlueprintOwned(IEnumerable<string> src) =>
                    src.Where(n => HasBlueprint(n));
                return cat switch
                {
                    "materials"    => Filtered(all.Where(IsMaterialAsset)),
                    // items tab: excludes vehicles, ships, armaments, and ship upgrades (those belong in their own tabs)
                    "items"        => Filtered(all.Where(x => !IsMaterialAsset(x) && !owner.engine.Vehicles.ContainsKey(x) && !owner.engine.ShipCatalog.ContainsKey(x) && !owner.engine.ShipArmaments.ContainsKey(x) && !owner.engine.ShipUpgradeCatalog.ContainsKey(x))),
                    "ship-upgrades"=> Filtered(owner.engine.ShipUpgradeCatalog.Keys.Distinct(StringComparer.OrdinalIgnoreCase)),
                    // ships & vehicles ALWAYS require blueprint — show nothing without one
                    "starships"    => BlueprintOwned(owner.engine.ShipCatalog.Keys),
                    "land-vehicles"=> BlueprintOwned(owner.engine.Vehicles.Keys.Where(IsLandVehicle)),
                    _              => Filtered(all)
                };
            }

            string? SelectedItem() =>
                recipeList.SelectedItem is string s ? (s.EndsWith(" [LOCKED]") ? s[..^9] : s) : null;

            void LoadCategory(string cat)
            {
                currentCategory = cat;
                catLabel.Text = $"── {cat switch { "materials" => "Materials", "items" => "Items", "ship-upgrades" => "Ship Upgrades", "starships" => "Starships", "land-vehicles" => "Land Vehicles", _ => "Items" }} ──";

                recipeList.Items.Clear();
                foreach (var item in GetItems(cat).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x))
                {
                    bool locked = owner.character != null && owner.engine.TryGetAssetLockReason(owner.character, item, out _);
                    recipeList.Items.Add(locked ? $"{item} [LOCKED]" : item);
                }
                if (recipeList.Items.Count > 0) recipeList.SelectedIndex = 0;

                btnInstallUpgr.Visible  = cat == "ship-upgrades";
                btnSmelt.Visible        = cat == "materials";
                btnGather.Visible       = cat == "materials";
                btnRefine.Visible       = cat == "materials";
                btnCraft.Text = cat switch
                {
                    "materials"    => "Process Material",
                    "ship-upgrades"=> "Craft Upgrade",
                    "starships"    => "Build Ship",
                    "land-vehicles"=> "Assemble Vehicle",
                    _ => "Craft / Build"
                };
                resultLabel.Text = "";
            }

            // Owner-draw coloring by lock status
            recipeList.DrawItem += (_, e) =>
            {
                if (e.Index < 0 || e.Index >= recipeList.Items.Count) return;
                e.DrawBackground();
                var text = recipeList.Items[e.Index]?.ToString() ?? "";
                var name = text.EndsWith(" [LOCKED]") ? text[..^9] : text;
                var cat2  = owner.character != null ? owner.engine.GetAssetAccessCategory(owner.character, name, out string _) : "";
                var color = cat2 switch
                {
                    "faction-locked" => Color.IndianRed,
                    "era-locked"     => Color.Goldenrod,
                    _ => text.EndsWith(" [LOCKED]") ? Color.DimGray : Color.DarkSeaGreen
                };
                var bounds = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height);
                TextRenderer.DrawText(e.Graphics, text, e.Font, bounds, color, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                e.DrawFocusRectangle();
            };

            recipeList.SelectedIndexChanged += (_, _) =>
            {
                var name = SelectedItem();
                if (name is null || owner.character is null) { infoBox.Text = ""; return; }
                infoBox.Text = owner.engine.GetAssetRequirementReport(owner.character, name);
                btnInstallUpgr.Enabled = currentCategory == "ship-upgrades" &&
                    owner.character.Inventory.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
            };

            // ── Actions ──────────────────────────────────────────────────────
            void ShowResult(string msg)
            {
                resultLabel.Text = msg.Length > 90 ? msg[..90] + "…" : msg;
                owner.AppendLog(msg);
                var name = SelectedItem();
                if (name != null && owner.character != null)
                    infoBox.Text = owner.engine.GetAssetRequirementReport(owner.character, name);
            }

            btnCraft.Click += (_, _) =>
            {
                var name = SelectedItem();
                if (name is null) return;
                if (currentCategory == "starships") owner.BuildShip(name);
                else owner.CraftItem(name);
                ShowResult($"Attempted: {name}");
            };

            btnInstallUpgr.Click += (_, _) =>
            {
                var name = SelectedItem();
                if (name is null) return;
                owner.InstallShipUpgrade(name);
                ShowResult($"Install attempted: {name}");
                btnInstallUpgr.Enabled = owner.character?.Inventory.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? false;
            };

            btnAssembleSaber.Click += (_, _) =>
            {
                if (owner.character is null) return;
                ShowResult(owner.engine.CraftLightsaber(owner.character));
            };

            btnAssembleMedkit.Click += (_, _) =>
            {
                if (owner.character is null) return;
                ShowResult(owner.engine.CraftMedkit(owner.character));
            };

            btnSmelt.Click += (_, _) =>
            {
                if (owner.character is null) return;
                ShowResult(owner.engine.SmeltMaterials(owner.character, 3));
                owner.RefreshStatus();
            };

            btnGather.Click += (_, _) =>
            {
                if (owner.character is null) return;
                var available = owner.engine.GetPlanetRawMaterials(owner.character.Location);
                if (available.Count == 0) { ShowResult("No mapped raw materials for this planet."); return; }
                ShowResult(owner.engine.HarvestRawMaterial(owner.character, available[0], 4));
                owner.RefreshStatus();
            };

            btnRefine.Click += (_, _) =>
            {
                var name = SelectedItem() ?? "raw ore";
                if (owner.character is null) return;
                ShowResult(owner.engine.RefineMaterial(owner.character, name, 3));
                owner.RefreshStatus();
            };

            // Category button clicks
            btnItems.Click     += (_, _) => LoadCategory("items");
            btnMaterials.Click += (_, _) => LoadCategory("materials");
            btnShipUpgr.Click  += (_, _) => LoadCategory("ship-upgrades");
            btnStarships.Click += (_, _) => LoadCategory("starships");
            btnVehicles.Click  += (_, _) => LoadCategory("land-vehicles");

            LoadCategory("items");
            return page;
        }

        // ── QUEST TAB ─────────────────────────────────────────────────────────

        private TabPage BuildQuestTab()
        {
            var page = new TabPage("Quests") { BackColor = Color.FromArgb(14, 14, 24) };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                BackColor = Color.FromArgb(14, 14, 24)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // buttons
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // log
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // actions

            // ── top buttons ────────────────────────────────────────────────
            var topFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6, 6, 6, 2),
                BackColor = Color.FromArgb(18, 18, 34)
            };

            Button TBtn(string text, Color bg) => new()
            {
                Text = text, Height = 28, Width = 130,
                BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f)
            };

            var btnRefresh   = TBtn("Refresh Log",     Color.FromArgb(30, 30, 60));
            var btnStartNext = TBtn("Start Next Step",  Color.FromArgb(20, 60, 20));
            var statusLabel  = new Label
            {
                AutoSize = true, ForeColor = Color.Goldenrod,
                Font = new Font("Consolas", 8.5f), Padding = new Padding(8, 6, 0, 0)
            };
            topFlow.Controls.AddRange(new Control[] { btnRefresh, btnStartNext, statusLabel });

            // ── chain list (left) + quest log (right) ──────────────────────
            var questLog = new TextBox
            {
                Multiline = true, ReadOnly = true, Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(10, 10, 18), ForeColor = Color.Lime,
                Font = new Font("Consolas", 9f), ScrollBars = ScrollBars.Vertical
            };

            // ── chain/faction generator panel ──────────────────────────────
            var chainFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6),
                BackColor = Color.FromArgb(18, 18, 34)
            };
            // Faction selector for procedural chain generation
            var factionCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList, Width = 130,
                BackColor = Color.FromArgb(20, 20, 34), ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f)
            };
            foreach (var f in new[] { "Rebels", "Empire", "Smugglers", "Mandalorians", "Jedi", "Guilds", "Sith" })
                factionCombo.Items.Add(f);
            factionCombo.SelectedIndex = 0;

            var btnGenChain  = TBtn("Generate Chain",  Color.FromArgb(40, 20, 80));
            var factionStatus = new Label { AutoSize = true, ForeColor = Color.SlateGray, Font = new Font("Segoe UI", 8f), Padding = new Padding(4, 6, 0, 0) };
            chainFlow.Controls.AddRange(new Control[]
            {
                new Label { Text = "Faction:", AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9f), Padding = new Padding(0, 6, 4, 0) },
                factionCombo, btnGenChain, factionStatus
            });

            layout.Controls.Add(topFlow, 0, 0);
            layout.Controls.Add(questLog, 0, 1);
            layout.Controls.Add(chainFlow, 0, 2);
            page.Controls.Add(layout);

            // ── actions ────────────────────────────────────────────────────
            void Refresh()
            {
                if (owner.character is null) { questLog.Text = "Create a character first."; return; }
                questLog.Text = owner.engine.GetQuestLogDisplay(owner.character);
                statusLabel.Text = $"Active: {owner.engine.ActiveQuests.Count(q => q.Status is "active" or "ready")}  |  Completed: {owner.engine.ActiveQuests.Count(q => q.Status == "completed")}";
                // Show access status for selected faction
                if (factionCombo.SelectedItem is string sel)
                    factionStatus.Text = owner.engine.GetFactionQuestAccessStatus(sel);
            }

            factionCombo.SelectedIndexChanged += (_, _) =>
            {
                if (owner.character is null || factionCombo.SelectedItem is not string sel) return;
                factionStatus.Text = owner.engine.GetFactionQuestAccessStatus(sel);
            };

            btnRefresh.Click += (_, _) => Refresh();

            btnStartNext.Click += (_, _) =>
            {
                if (owner.character is null) return;
                var inactive = owner.engine.ActiveQuests.FirstOrDefault(q => q.Status == "inactive");
                if (inactive is null) { statusLabel.Text = "No pending chain steps."; return; }
                var msg = owner.engine.ActivateChainStep(owner.character, inactive.Id);
                owner.AppendLog(msg);
                Refresh();
            };

            btnGenChain.Click += (_, _) =>
            {
                if (owner.character is null) return;
                if (factionCombo.SelectedItem is not string faction) return;
                var standing = owner.engine.FactionStandings.GetValueOrDefault(faction);
                if (standing <= GameEngine.FactionQuestLockThreshold)
                {
                    factionStatus.Text = $"LOCKED — reputation with {faction} too low ({standing}).";
                    return;
                }
                if (owner.engine.ActiveQuests.Count(q => q.Status is "active" or "ready") >= 5)
                {
                    factionStatus.Text = "Datapad full — complete active missions first.";
                    return;
                }
                var chain = owner.engine.GenerateProceduralChain(faction, owner.character, owner.character.Location);
                owner.AppendLog("════════════════════════════════════════════════");
                owner.AppendLog($"★  Chain started: {chain.Title}  [{faction}]");
                owner.AppendLog($"   {chain.Steps.Count} steps — first step is now ACTIVE");
                foreach (var step in chain.Steps)
                {
                    var status = step.Status == "active" ? "▶ ACTIVE" : "  locked";
                    var bp = string.IsNullOrEmpty(step.RewardBlueprint) ? "" : $"  BP: {step.RewardBlueprint}";
                    owner.AppendLog($"  [{status}] Step {step.ChainStep}: {step.Title} ({step.ObjectiveType} on {step.TargetPlanet}){bp}");
                }
                owner.AppendLog("  Open the Quest tab to track progress and start next steps.");
                owner.AppendLog("════════════════════════════════════════════════");
                factionStatus.Text = $"✓ {chain.Title} — {chain.Steps.Count} steps. Check Quest tab.";
                Refresh();
            };

            // auto-refresh when tab is selected
            page.Enter += (_, _) => Refresh();

            Refresh();
            return page;
        }

        private TabPage BuildStorageTab()
        {
            var page = new TabPage("Storage") { BackColor = Color.FromArgb(14, 14, 24) };
            // Embed the InventoryManagerWindow logic inline
            var srcCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
            var dstCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
            var srcList  = new ListBox  { BackColor = Color.FromArgb(20, 20, 34), ForeColor = Color.White, Font = new Font("Segoe UI", 9f) };
            var dstList  = new ListBox  { BackColor = Color.FromArgb(20, 20, 34), ForeColor = Color.White, Font = new Font("Segoe UI", 9f) };
            var srcCapLbl= new Label    { AutoSize = true, ForeColor = Color.LightGray };
            var dstCapLbl= new Label    { AutoSize = true, ForeColor = Color.LightGray };
            var logLbl   = new Label    { AutoSize = true, ForeColor = Color.LightYellow };
            var storageTypes = new[] { "Character Bag", "Ship Locker", "Ship Cargo", "Home Storage" };

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(8) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
            page.Controls.Add(root);

            Panel MakeStoragePanel(string header, ComboBox combo, ListBox lb, Label capLbl)
            {
                var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(14, 14, 24) };
                var hdr = new Label { Text = header, AutoSize = true, ForeColor = Color.Gold, Font = new Font("Segoe UI", 10f, FontStyle.Bold), Left = 0, Top = 0 };
                combo.Location = new Point(0, 24); capLbl.Location = new Point(0, 52);
                lb.Bounds = new Rectangle(0, 70, 300, 380);
                lb.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
                p.Controls.AddRange(new Control[] { hdr, combo, capLbl, lb });
                return p;
            }

            List<string> GetItems(string st)
            {
                if (owner.character == null) return new();
                return st switch
                {
                    "Character Bag" => owner.character.Inventory,
                    "Ship Locker"   => owner.character.Ship?.PersonalStorageItems ?? new(),
                    "Ship Cargo"    => owner.character.Ship?.CargoItems ?? new(),
                    "Home Storage"  => owner.character.Home?.StorageItems ?? new(),
                    _ => new()
                };
            }

            void RefreshList(ComboBox combo, ListBox lb, Label capLbl)
            {
                if (combo.SelectedItem is not string st) return;
                lb.Items.Clear();
                foreach (var i in GetItems(st)) lb.Items.Add(i);
                capLbl.Text = st switch
                {
                    "Character Bag" => owner.character is null ? "" : $"{owner.engine.GetInventoryUsedMicroScu(owner.character)} / {owner.engine.GetInventoryCapacityMicroScu(owner.character)} µSCU",
                    _ => ""
                };
            }

            srcCombo.SelectedIndexChanged += (_, _) => RefreshList(srcCombo, srcList, srcCapLbl);
            dstCombo.SelectedIndexChanged += (_, _) => RefreshList(dstCombo, dstList, dstCapLbl);
            srcCombo.Items.AddRange(storageTypes); dstCombo.Items.AddRange(storageTypes);
            srcCombo.SelectedIndex = 0; dstCombo.SelectedIndex = 1;

            root.Controls.Add(MakeStoragePanel("Source",      srcCombo, srcList, srcCapLbl), 0, 0);

            var midFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(4) };
            var transferBtn = new Button { Text = "→ Transfer", Width = 82, Height = 36, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var discardBtn  = new Button { Text = "✕ Discard",  Width = 82, Height = 36, BackColor = Color.FromArgb(80, 20, 20), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 6, 0, 0) };
            transferBtn.Click += (_, _) =>
            {
                if (owner.character == null || srcList.SelectedItem is not string item) return;
                if (srcCombo.SelectedItem is not string src || dstCombo.SelectedItem is not string dst) return;
                var srcItems = GetItems(src);
                var dstItems = GetItems(dst);
                if (srcItems.Contains(item, StringComparer.OrdinalIgnoreCase))
                {
                    srcItems.Remove(srcItems.First(x => x.Equals(item, StringComparison.OrdinalIgnoreCase)));
                    dstItems.Add(item);
                    logLbl.Text = $"Moved {item} → {dst}";
                    RefreshList(srcCombo, srcList, srcCapLbl);
                    RefreshList(dstCombo, dstList, dstCapLbl);
                }
            };
            discardBtn.Click += (_, _) =>
            {
                if (owner.character == null || srcList.SelectedItem is not string item) return;
                if (srcCombo.SelectedItem is not string src) return;
                var srcItems = GetItems(src);
                srcItems.Remove(srcItems.FirstOrDefault(x => x.Equals(item, StringComparison.OrdinalIgnoreCase)) ?? "");
                logLbl.Text = $"Discarded {item}.";
                RefreshList(srcCombo, srcList, srcCapLbl);
            };
            midFlow.Controls.AddRange(new Control[] { transferBtn, discardBtn, logLbl });
            root.Controls.Add(midFlow, 1, 0);

            root.Controls.Add(MakeStoragePanel("Destination", dstCombo, dstList, dstCapLbl), 2, 0);

            return page;
        }

        private TabPage BuildStatsTab()
        {
            var page = new TabPage("Stats & Skills") { BackColor = Color.FromArgb(14, 14, 24) };
            page.Controls.Add(statsBox);
            return page;
        }

        // ── REFRESH ──────────────────────────────────────────────────────────

        public void Refresh()
        {
            if (owner.character is null) return;
            var ch = owner.character;

            // Equipment slots
            foreach (var slot in ArmorSlots)
            {
                if (!slotLabels.TryGetValue(slot, out var lbl)) continue;
                ch.EquippedArmorPieces.TryGetValue(slot, out var name);
                if (string.IsNullOrEmpty(name) && ch.EquippedArmorPieces.TryGetValue("Full Suit", out var fs))
                    name = $"{fs} (Full Suit)";
                var extra = "";
                if (!string.IsNullOrEmpty(name) && owner.engine.Armors.TryGetValue(name.Replace(" (Full Suit)", ""), out var a))
                    extra = $"  [AR+{a.ArmorRating} HP+{a.HpBonus} Stam+{a.StaminaBonus}]";
                lbl.Text = string.IsNullOrEmpty(name) ? "— empty —" : $"{name}{extra}";
                lbl.ForeColor = string.IsNullOrEmpty(name) ? Color.DimGray : Color.White;
            }

            // Backpack
            if (slotLabels.TryGetValue("Backpack", out var bpLbl))
            {
                bpLbl.Text = string.IsNullOrEmpty(ch.EquippedBackpack) ? "— empty —" : ch.EquippedBackpack;
                bpLbl.ForeColor = string.IsNullOrEmpty(ch.EquippedBackpack) ? Color.DimGray : Color.Plum;
            }

            // Weapon slots
            weaponMainLabel.Text = string.IsNullOrEmpty(ch.EquippedWeapon) ? "— empty —" : ch.EquippedWeapon;
            weaponOffLabel.Text  = string.IsNullOrEmpty(ch.OffHandWeapon)  ? "— empty —  (pistols, shoto, or Jar'kai lightsaber)" : ch.OffHandWeapon;
            toolLabel.Text       = string.IsNullOrEmpty(ch.EquippedTool)   ? "— empty —  (mining pick, vibro-pick, axe, etc.)"  : ch.EquippedTool;

            // Summary line
            var foragingBonus = owner.engine.GetForagingBonus(ch);
            var weapAbils = owner.engine.GetWeaponAbilities(ch);
            equipSummaryLabel.Text =
                $"AR: {ch.Armor}  |  HP: {ch.Hp}/{ch.MaxHp}  |  Stamina: {ch.Stamina}/{ch.MaxStamina}" +
                $"  |  Foraging: +{foragingBonus}" +
                (weapAbils.Count > 0 ? $"\nWeapon abilities: {string.Join(", ", weapAbils)}" : "");

            // Items tab
            var selItem = itemList.SelectedItem as string;
            itemList.Items.Clear();
            foreach (var item in ch.Inventory.OrderBy(x => x)) itemList.Items.Add(item);
            if (selItem != null && itemList.Items.Contains(selItem)) itemList.SelectedItem = selItem;
            var cap = owner.engine.GetInventoryCapacity(ch);
            itemsCapLabel.Text = $"Inventory: {ch.Inventory.Count}/{cap} items  |  Credits: {ch.Credits} {ch.CurrencyType}";
            itemsCapLabel.ForeColor = ch.Inventory.Count >= cap ? Color.Red : Color.Orange;
            OnItemSelected();

            // Stats tab
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"══ {ch.Name} ══");
            sb.AppendLine($"Species : {ch.Species}   Role: {ch.Role}   Age: {ch.Age}   Homeworld: {ch.Homeworld}");
            sb.AppendLine($"Background : {ch.Background}   Faction : {ch.Faction}");
            sb.AppendLine($"HP      : {ch.Hp}/{ch.MaxHp}   Armor: {ch.Armor}   Stamina: {ch.Stamina}/{ch.MaxStamina}");
            sb.AppendLine($"Hunger  : {ch.Hunger}/100   Energy: {ch.Energy}/100");
            sb.AppendLine($"Credits : {ch.Credits} {ch.CurrencyType}   XP: {ch.Experience}   Rep: {ch.Reputation}");
            sb.AppendLine($"Location: {ch.Location}");
            if (ch.IsForceUser) sb.AppendLine($"Jedi Rank: {ch.JediRank}   FP: {ch.ForcePoints}/{ch.MaxForcePoints}");
            sb.AppendLine();
            sb.AppendLine("── Stats ──");
            foreach (var kv in ch.Stats) sb.AppendLine($"  {kv.Key,-14}: {kv.Value}");
            sb.AppendLine();
            sb.AppendLine("── Skills ──");
            foreach (var s in ch.Skills) sb.AppendLine($"  {s}");
            if (ch.IsForceUser && ch.ForceAbilities.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("── Force Abilities ──");
                foreach (var fa in ch.ForceAbilities) sb.AppendLine($"  {fa}");
            }
            if (weapAbils.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"── Weapon Abilities ({ch.EquippedWeapon}) ──");
                foreach (var wa in weapAbils)
                {
                    if (owner.engine.WeaponAbilities.TryGetValue(wa, out var wad))
                        sb.AppendLine($"  {wa} — {wad.Description}  [Stam: {wad.StaminaCost}]");
                }
            }
            if (ch.StatusEffects.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("── Status Effects ──");
                foreach (var se in ch.StatusEffects) sb.AppendLine($"  {se}");
            }
            statsBox.Text = sb.ToString();
        }

        // ── ARMOR SLOT EVENTS ─────────────────────────────────────────────────

        private void OnArmorChange(object? sender, EventArgs e)
        {
            if (owner.character == null || sender is not Button btn || btn.Tag is not string slot) return;
            var ch = owner.character;
            var available = ch.Inventory
                .Where(item => owner.engine.Armors.TryGetValue(item, out var a) && a.Slot == slot)
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            if (available.Count == 0) { MessageBox.Show($"No {slot} pieces in your inventory.", "Nothing to Equip"); return; }
            var picked = PickFromList($"Choose {slot} Armor", available, item =>
                owner.engine.Armors.TryGetValue(item, out var a)
                    ? $"AR: {a.ArmorRating}  HP: {a.HpBonus}  Stam: {a.StaminaBonus}\n{a.Description}"
                    : "");
            if (picked is null) return;
            owner.AppendLog(owner.engine.EquipArmorPiece(ch, picked));
            owner.RefreshStatus(); Refresh();
        }

        private void OnArmorRemove(object? sender, EventArgs e)
        {
            if (owner.character == null || sender is not Button btn || btn.Tag is not string slot) return;
            owner.AppendLog(owner.engine.UnequipArmorSlot(owner.character, slot));
            owner.RefreshStatus(); Refresh();
        }

        private void OnBackpackChange(object? sender, EventArgs e)
        {
            if (owner.character == null) return;
            var available = owner.character.Inventory
                .Where(i => owner.engine.Backpacks.ContainsKey(i))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            if (available.Count == 0) { MessageBox.Show("No backpacks in your inventory.", "Nothing to Equip"); return; }
            var picked = PickFromList("Choose Backpack", available, item =>
                owner.engine.Backpacks.TryGetValue(item, out var bp) ? bp.Description : "");
            if (picked is null) return;
            owner.AppendLog(owner.engine.EquipBackpack(owner.character, picked));
            owner.RefreshStatus(); Refresh();
        }

        private void OnBackpackRemove(object? sender, EventArgs e)
        {
            if (owner.character == null) return;
            owner.AppendLog(owner.engine.UnequipBackpack(owner.character));
            owner.RefreshStatus(); Refresh();
        }

        // ── WEAPON SLOT EVENTS ────────────────────────────────────────────────

        private void EquipWeaponMain(object? sender, EventArgs e)
        {
            if (owner.character == null) return;
            var weapons = owner.character.Inventory
                .Where(i => owner.engine.Weapons.ContainsKey(i) || i.Contains("lightsaber", StringComparison.OrdinalIgnoreCase) || i.Contains("pistol", StringComparison.OrdinalIgnoreCase) || i.Contains("rifle", StringComparison.OrdinalIgnoreCase) || i.Contains("blade", StringComparison.OrdinalIgnoreCase) || i.Contains("axe", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            if (weapons.Count == 0) { MessageBox.Show("No weapons in your inventory.", "Nothing to Equip"); return; }
            var picked = PickFromList("Choose Main Hand Weapon", weapons, item =>
                owner.engine.Weapons.TryGetValue(item, out var w) ? $"Dmg: {w.Damage}  Type: {w.WeaponSubtype}\n{w.Description}" : "");
            if (picked is null) return;
            var msg = owner.engine.EquipItem(owner.character, picked);
            owner.AppendLog(msg); owner.RefreshStatus(); Refresh();
        }

        private void UnequipWeaponMain(object? sender, EventArgs e)
        {
            if (owner.character == null) return;
            owner.character.EquippedWeapon = "";
            owner.AppendLog("Main hand weapon unequipped."); owner.RefreshStatus(); Refresh();
        }

        private void EquipWeaponOff(object? sender, EventArgs e)
        {
            if (owner.character == null) return;
            var mainSubtype = owner.engine.GetWeaponSubtype(owner.character.EquippedWeapon);
            if (string.IsNullOrEmpty(mainSubtype)) { MessageBox.Show("Equip a main-hand weapon first.", "No Main Hand"); return; }
            var available = owner.character.Inventory
                .Where(i => { owner.engine.CanEquipOffHand(owner.character, i, out _); return owner.engine.Weapons.TryGetValue(i, out var w) && w.IsOneHanded; })
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            if (available.Count == 0) { MessageBox.Show("No valid off-hand weapons in your inventory.\n(Pistols, shoto lightsabers, or a lightsaber with Jar'kai training.)", "Nothing to Equip"); return; }
            var picked = PickFromList("Choose Off-Hand Weapon", available, item =>
            {
                owner.engine.CanEquipOffHand(owner.character, item, out var reason);
                return owner.engine.Weapons.TryGetValue(item, out var w)
                    ? $"Dmg: {w.Damage}  Type: {w.WeaponSubtype}\n{w.Description}" + (string.IsNullOrEmpty(reason) ? "" : $"\n⚠ {reason}")
                    : "";
            });
            if (picked is null) return;
            owner.AppendLog(owner.engine.EquipOffHandWeapon(owner.character, picked));
            owner.RefreshStatus(); Refresh();
        }

        private void UnequipWeaponOff(object? sender, EventArgs e)
        {
            if (owner.character == null) return;
            owner.AppendLog(owner.engine.UnequipOffHand(owner.character));
            owner.RefreshStatus(); Refresh();
        }

        private void EquipTool(object? sender, EventArgs e)
        {
            if (owner.character == null) return;
            var tools = owner.character.Inventory
                .Where(i => owner.engine.HarvestingTools.ContainsKey(i))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            if (tools.Count == 0) { MessageBox.Show("No harvesting tools in your inventory.\n(Buy mining picks or woodcutting axes from merchants.)", "Nothing to Equip"); return; }
            var picked = PickFromList("Choose Tool", tools, item =>
                owner.engine.HarvestingTools.TryGetValue(item, out var t)
                    ? $"Type: {t.ToolType}  Tier: {t.Tier}  Yield: +{t.YieldBonus}\n{t.Description}" : "");
            if (picked is null) return;
            owner.AppendLog(owner.engine.EquipTool(owner.character, picked));
            owner.RefreshStatus(); Refresh();
        }

        private void UnequipTool(object? sender, EventArgs e)
        {
            if (owner.character == null) return;
            owner.AppendLog(owner.engine.UnequipTool(owner.character));
            owner.RefreshStatus(); Refresh();
        }

        // ── ITEM TAB EVENTS ───────────────────────────────────────────────────

        private void OnItemSelected()
        {
            if (itemList.SelectedItem is not string item) { itemInfoBox.Text = ""; return; }
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Item: {item}");

            if (owner.engine.Weapons.TryGetValue(item, out var w))
                sb.AppendLine($"Weapon — Damage: {w.Damage}  Subtype: {w.WeaponSubtype}  {(w.IsOneHanded ? "One-handed" : "Two-handed")}\n{w.Description}");
            else if (owner.engine.Armors.TryGetValue(item, out var a))
                sb.AppendLine($"Armor [{a.Slot}] — AR: {a.ArmorRating}  HP: +{a.HpBonus}  Stam: +{a.StaminaBonus}\n{a.Description}");
            else if (owner.engine.HarvestingTools.TryGetValue(item, out var ht))
                sb.AppendLine($"Tool [{ht.ToolType}] — Tier {ht.Tier}  Yield: +{ht.YieldBonus}  Chance: +{ht.ChanceBonus}%\n{ht.Description}");
            else if (owner.engine.FoodItems.TryGetValue(item, out var f))
                sb.AppendLine($"Food — Hunger: +{f.HungerValue}  Energy: +{f.EnergyValue}  HP: +{f.HpBonus}" + (f.RequiresCooking ? "  ⚠ Requires cooking" : "") + $"\n{f.Description}");
            else if (owner.engine.CraftableItems.TryGetValue(item, out var ci))
                sb.AppendLine($"Craftable — {ci.Category}\n{ci.Description}");
            else
                sb.AppendLine("(No additional info available)");

            itemInfoBox.Text = sb.ToString();
        }

        private void OnSellItem()
        {
            if (owner.character == null || itemList.SelectedItem is not string item) return;
            owner.SellSelectedItem(item);
            Refresh();
        }

        private void OnEquipItem()
        {
            if (owner.character == null || itemList.SelectedItem is not string item) return;
            // Route to the right equip method
            if (owner.engine.Armors.TryGetValue(item, out _))
                owner.AppendLog(owner.engine.EquipArmorPiece(owner.character, item));
            else if (owner.engine.HarvestingTools.TryGetValue(item, out _))
                owner.AppendLog(owner.engine.EquipTool(owner.character, item));
            else
                owner.AppendLog(owner.engine.EquipItem(owner.character, item));
            owner.RefreshStatus(); Refresh();
        }

        // ── HELPER ───────────────────────────────────────────────────────────

        private string? PickFromList(string title, List<string> options, Func<string, string> getInfo)
        {
            string? result = null;
            using var frm = new Form
            {
                Text = title, Width = 400, Height = 420, BackColor = Color.FromArgb(14, 14, 24),
                FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.Manual,
                Location = new Point(Location.X + 80, Location.Y + 80)
            };
            var lb = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 20, 34), ForeColor = Color.White, Font = new Font("Segoe UI", 9f) };
            lb.Items.AddRange(options.ToArray<object>());
            var info = new Label { Dock = DockStyle.Bottom, AutoSize = false, Height = 70, ForeColor = Color.LightGray, BackColor = Color.FromArgb(12, 12, 20) };
            lb.SelectedIndexChanged += (_, _) => { if (lb.SelectedItem is string s) info.Text = getInfo(s); };
            var ok = new Button { Text = "Select", Dock = DockStyle.Bottom, Height = 34, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            ok.Click += (_, _) => { result = lb.SelectedItem as string; frm.Close(); };
            frm.Controls.AddRange(new Control[] { lb, info, ok });
            frm.ShowDialog(this);
            return result;
        }
    }

    // ── BLASTER MOD WINDOW ────────────────────────────────────────────────────

    private sealed class BlasterModWindow : Form
    {
        private static readonly string[] AllSlots = { "Scope", "Barrel", "Grip", "Stock", "Underbarrel", "Power Cell" };

        private readonly Form1 owner;
        private readonly ComboBox coreCombo   = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260, Font = new Font("Segoe UI", 10f) };
        private readonly TextBox  infoBox     = new() { Multiline = true, ReadOnly = true, Height = 90, Dock = DockStyle.Bottom,
            BackColor = Color.FromArgb(10, 10, 18), ForeColor = Color.LightYellow, Font = new Font("Consolas", 9f), ScrollBars = ScrollBars.Vertical };
        private readonly Label    resultLabel = new() { AutoSize = true, ForeColor = Color.LimeGreen, Font = new Font("Consolas", 9f), Dock = DockStyle.Bottom };

        // Per-slot controls
        private readonly Dictionary<string, GroupBox> slotGroups  = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Label>    installedLbls = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ComboBox> modCombos   = new(StringComparer.OrdinalIgnoreCase);

        public BlasterModWindow(Form1 owner)
        {
            this.owner = owner;
            Text = "Blaster Mods";
            Size = new Size(720, 680);
            MinimumSize = new Size(640, 560);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 80, owner.Location.Y + 60);
            BackColor = Color.FromArgb(12, 18, 12);

            // ── Top: Core weapon selector ────────────────────────────────────
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(16, 26, 16) };
            var coreLabel = new Label { Text = "Core Weapon:", Left = 8, Top = 14, Width = 110, ForeColor = Color.LightGreen, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            coreCombo.Location = new Point(122, 12);
            topPanel.Controls.AddRange(new Control[] { coreLabel, coreCombo });

            // ── Scroll panel with slot groups ────────────────────────────────
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(12, 18, 12) };

            var slotFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown, WrapContents = false,
                AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(8, 4, 8, 4)
            };
            scroll.Controls.Add(slotFlow);

            foreach (var slot in AllSlots)
            {
                var grp = BuildSlotGroup(slot);
                slotGroups[slot] = grp;
                slotFlow.Controls.Add(grp);
            }

            // ── Bottom: info + result ────────────────────────────────────────
            var btmPanel = new Panel { Dock = DockStyle.Bottom, Height = 130, BackColor = Color.FromArgb(10, 14, 10) };
            var infoHeader = new Label { Text = "Mod Info:", Left = 6, Top = 4, AutoSize = true, ForeColor = Color.Gold, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            infoBox.Location = new Point(6, 22);
            infoBox.Size     = new Size(this.Width - 24, 72);
            infoBox.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            resultLabel.Location = new Point(6, 100);
            btmPanel.Controls.AddRange(new Control[] { infoHeader, infoBox, resultLabel });

            Controls.Add(scroll);
            Controls.Add(topPanel);
            Controls.Add(btmPanel);

            Resize += (_, _) => { infoBox.Width = ClientSize.Width - 24; };
            coreCombo.SelectedIndexChanged += (_, _) => RefreshSlots();
            PopulateCoreCombo();
        }

        // ── Builds one slot GroupBox ─────────────────────────────────────────

        private GroupBox BuildSlotGroup(string slot)
        {
            var slotColor = slot switch
            {
                "Scope"      => Color.CornflowerBlue,
                "Barrel"     => Color.LightSteelBlue,
                "Grip"       => Color.Goldenrod,
                "Stock"      => Color.Peru,
                "Underbarrel"=> Color.MediumOrchid,
                "Power Cell" => Color.OrangeRed,
                _ => Color.LightGray
            };

            var grp = new GroupBox
            {
                Text = slot, Width = 660, Height = 80,
                ForeColor = slotColor, BackColor = Color.FromArgb(16, 22, 16),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            var installedLbl = new Label { Left = 8, Top = 20, Width = 280, Height = 18, ForeColor = Color.White, Font = new Font("Segoe UI", 9f), AutoSize = false };
            installedLbls[slot] = installedLbl;

            var combo = new ComboBox { Left = 8, Top = 44, Width = 280, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f) };
            combo.BackColor = Color.FromArgb(20, 28, 20);
            combo.ForeColor = Color.White;
            modCombos[slot] = combo;
            combo.SelectedIndexChanged += (_, _) => OnModSelected(slot);

            var installBtn = new Button
            {
                Text = "Install", Left = 296, Top = 44, Width = 76, Height = 26,
                BackColor = Color.FromArgb(20, 80, 20), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            installBtn.Click += (_, _) => OnInstall(slot);

            var removeBtn = new Button
            {
                Text = "Remove", Left = 378, Top = 44, Width = 76, Height = 26,
                BackColor = Color.FromArgb(80, 20, 20), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            removeBtn.Click += (_, _) => OnRemove(slot);

            grp.Controls.AddRange(new Control[] { installedLbl, combo, installBtn, removeBtn });
            return grp;
        }

        // ── Populate core combo with moddable weapons ───────────────────────

        private void PopulateCoreCombo()
        {
            coreCombo.Items.Clear();
            if (owner.character == null) return;
            var ch = owner.character;

            var weapons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // Equipped weapons count
            if (!string.IsNullOrEmpty(ch.EquippedWeapon)) weapons.Add(ch.EquippedWeapon);
            if (!string.IsNullOrEmpty(ch.OffHandWeapon))  weapons.Add(ch.OffHandWeapon);
            // Inventory weapons
            foreach (var item in ch.Inventory)
                if (owner.engine.Weapons.ContainsKey(item)) weapons.Add(item);

            // Only keep moddable weapons
            foreach (var w in weapons.OrderBy(x => x))
                if (owner.engine.GetModSlotsForWeapon(w).Count > 0)
                    coreCombo.Items.Add(w);

            if (coreCombo.Items.Count > 0) coreCombo.SelectedIndex = 0;
            else resultLabel.Text = "No moddable weapons found. Buy or equip a blaster first.";
        }

        // ── Show/hide slots based on selected weapon ─────────────────────────

        private void RefreshSlots()
        {
            if (coreCombo.SelectedItem is not string weaponName)
            {
                foreach (var grp in slotGroups.Values) grp.Visible = false;
                return;
            }

            var supportedSlots = owner.engine.GetModSlotsForWeapon(weaponName);
            var ch = owner.character!;

            ch.InstalledMods.TryGetValue(weaponName, out var installedMap);

            foreach (var slot in AllSlots)
            {
                var supported = supportedSlots.Contains(slot);
                slotGroups[slot].Visible = supported;
                if (!supported) continue;

                // Update installed label
                if (installedMap != null && installedMap.TryGetValue(slot, out var installed))
                {
                    var bonus = owner.engine.BlasterMods.TryGetValue(installed, out var m) && m.DamageBonus > 0 ? $" (+{m.DamageBonus}dmg)" : "";
                    var cat   = owner.engine.BlasterMods.TryGetValue(installed, out var m2) ? $" [{m2.Category}]" : "";
                    installedLbls[slot].Text = $"Installed: {installed}{bonus}{cat}";
                    installedLbls[slot].ForeColor = installed.Length > 0 && owner.engine.BlasterMods.TryGetValue(installed, out var m3) && m3.Category == "Illegal" ? Color.OrangeRed : Color.LimeGreen;
                }
                else
                {
                    installedLbls[slot].Text = "Installed: — none —";
                    installedLbls[slot].ForeColor = Color.DimGray;
                }

                // Populate mod combo with inventory items for this slot
                var combo = modCombos[slot];
                combo.Items.Clear();
                combo.Items.Add("— select mod —");
                foreach (var item in ch.Inventory.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x))
                    if (owner.engine.BlasterMods.TryGetValue(item, out var bm) && bm.Slot.Equals(slot, StringComparison.OrdinalIgnoreCase))
                        combo.Items.Add(item);
                combo.SelectedIndex = 0;
            }

            resultLabel.Text = $"Total mod damage bonus: +{owner.engine.GetWeaponModDamageBonus(ch, weaponName)} dmg";
        }

        // ── Mod selection → update info box ─────────────────────────────────

        private void OnModSelected(string slot)
        {
            if (modCombos[slot].SelectedItem is not string modName || modName.StartsWith("—")) { infoBox.Text = ""; return; }
            if (!owner.engine.BlasterMods.TryGetValue(modName, out var mod)) { infoBox.Text = ""; return; }

            var catColor = mod.Category switch { "Illegal" => "⚠ ILLEGAL", "Crafted" => "✦ CRAFTED", _ => "✓ Legal" };
            var lines = new List<string>
            {
                $"{mod.Name}  [{catColor}]  |  Slot: {mod.Slot}  |  Price: {mod.Price} cr",
                mod.DamageBonus > 0 ? $"Damage Bonus: +{mod.DamageBonus}" : "Damage Bonus: none",
            };
            if (!string.IsNullOrEmpty(mod.SpecialEffect)) lines.Add($"Special: {mod.SpecialEffect}");
            if (mod.HasExplosionRisk) lines.Add($"⚠ EXPLOSION RISK: {(int)(mod.ExplosionChance * 100)}% chance on install!");
            lines.Add(mod.Description);
            infoBox.Text = string.Join("\n", lines);
        }

        // ── Install / Remove ─────────────────────────────────────────────────

        private void OnInstall(string slot)
        {
            if (owner.character == null) return;
            if (coreCombo.SelectedItem is not string weaponName) return;
            if (modCombos[slot].SelectedItem is not string modName || modName.StartsWith("—"))
            { resultLabel.Text = "Select a mod from the dropdown first."; return; }

            var result = owner.engine.InstallMod(owner.character, weaponName, slot, modName);
            resultLabel.Text = result;
            resultLabel.ForeColor = result.Contains("⚠") ? Color.OrangeRed : Color.LimeGreen;
            owner.RefreshStatus();
            RefreshSlots();
        }

        private void OnRemove(string slot)
        {
            if (owner.character == null) return;
            if (coreCombo.SelectedItem is not string weaponName) return;
            var result = owner.engine.RemoveMod(owner.character, weaponName, slot);
            resultLabel.Text = result;
            resultLabel.ForeColor = Color.Orange;
            owner.RefreshStatus();
            RefreshSlots();
        }
    }

    private sealed class MerchantWindow : Form
    {
        private readonly Form1 owner;
        private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
        private readonly Dictionary<string, ListView> listViews = new(StringComparer.OrdinalIgnoreCase);
        private readonly Label infoLabel = new() { AutoSize = true, Dock = DockStyle.Bottom, ForeColor = Color.LightYellow, Font = new Font("Consolas", 8.5f) };

        private static readonly (string key, string display, Color headerColor)[] MerchantTypes =
        {
            ("general",      "General Goods",    Color.LightGray),
            ("armor",        "Armor Merchant",   Color.CornflowerBlue),
            ("weapons",      "Weapons Merchant", Color.Goldenrod),
            ("black_market", "Black Market",     Color.OrangeRed),
        };

        public MerchantWindow(Form1 owner)
        {
            this.owner = owner;
            Text = "Merchant District";
            Size = new Size(750, 560);
            MinimumSize = new Size(620, 440);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 260, owner.Location.Y + 160);
            BackColor = Color.FromArgb(12, 12, 22);

            Controls.Add(tabs);
            Controls.Add(infoLabel);

            foreach (var (key, display, headerColor) in MerchantTypes)
            {
                if (key == "black_market" && !owner.engine.HasBlackMarket(owner.character?.Location ?? "")) continue;

                var page = new TabPage(display) { BackColor = Color.FromArgb(14, 14, 24) };
                var lv = BuildListView(headerColor);
                listViews[key] = lv;

                var buyBtn = new Button { Text = "Buy Selected", Height = 30, Width = 120, Dock = DockStyle.None,
                    BackColor = key == "black_market" ? Color.DarkRed : Color.DarkSlateBlue,
                    ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                buyBtn.Click += (_, _) => BuySelected(key);

                var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                root.Controls.Add(lv, 0, 0);
                var btnRow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(4) };
                btnRow.Controls.Add(buyBtn);
                if (key == "black_market")
                {
                    var warnLabel = new Label { AutoSize = true, ForeColor = Color.OrangeRed,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        Text = "⚠ ILLEGAL — possessing these items may attract Imperial attention." };
                    btnRow.Controls.Add(warnLabel);
                }
                root.Controls.Add(btnRow, 0, 1);
                page.Controls.Add(root);
                tabs.TabPages.Add(page);
            }

            tabs.SelectedIndexChanged += (_, _) => RefreshCurrentTab();
            RefreshCurrentTab();
        }

        private ListView BuildListView(Color headerColor)
        {
            var lv = new ListView
            {
                Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
                BackColor = Color.FromArgb(18, 18, 32), ForeColor = Color.White
            };
            lv.Columns.Add("Item",     220);
            lv.Columns.Add("Price",     80);
            lv.Columns.Add("Stock",     60);
            lv.Columns.Add("Category", 110);
            lv.SelectedIndexChanged += (_, _) => UpdateInfoLabel(lv);
            return lv;
        }

        private string? CurrentMerchantKey()
        {
            var page = tabs.SelectedTab;
            if (page is null) return null;
            foreach (var (key, display, _) in MerchantTypes)
                if (page.Text == display) return key;
            return null;
        }

        private void RefreshCurrentTab()
        {
            var key = CurrentMerchantKey();
            if (key is null || !listViews.TryGetValue(key, out var lv)) return;
            if (owner.character is null) return;
            PopulateListView(lv, key);
        }

        private void PopulateListView(ListView lv, string merchantKey)
        {
            lv.Items.Clear();
            var listings = owner.engine.GetMerchantInventory(owner.character!.Location, merchantKey);
            foreach (var entry in listings)
            {
                var access = owner.engine.TryGetAssetLockReason(owner.character, entry.ItemName, out _) ? "🔒" : "";
                var row = new ListViewItem(new[]
                {
                    entry.ItemName,
                    $"{entry.Price} cr",
                    entry.Stock.ToString(),
                    entry.Category
                });
                row.ForeColor = merchantKey == "black_market" ? Color.OrangeRed
                    : access.Length > 0 ? Color.IndianRed
                    : Color.DarkSeaGreen;
                lv.Items.Add(row);
            }
        }

        private void UpdateInfoLabel(ListView lv)
        {
            if (owner.character is null) return;
            var selectedItem = lv.SelectedItems.Count > 0 ? lv.SelectedItems[0].SubItems[0].Text : "";
            var economy = owner.engine.GetPlanetEconomyStatus(owner.character.Location);
            var req = string.IsNullOrEmpty(selectedItem) ? "Select an item for details."
                : owner.engine.GetAssetRequirementReport(owner.character, selectedItem).Replace(Environment.NewLine, "  |  ");
            infoLabel.Text = $"📍 {owner.character.Location}  |  Credits: {owner.character.Credits} {owner.character.CurrencyType}  |  Economy: {economy.StatusText}  |  {req}";
        }

        private void BuySelected(string merchantKey)
        {
            if (owner.character is null) return;
            if (!listViews.TryGetValue(merchantKey, out var lv) || lv.SelectedItems.Count == 0) return;
            var itemName = lv.SelectedItems[0].SubItems[0].Text;
            var result = owner.engine.BuyFromMerchant(owner.character, owner.character.Location, itemName, merchantKey);
            owner.AppendLog(result);
            owner.RefreshStatus();
            PopulateListView(lv, merchantKey);
            UpdateInfoLabel(lv);
        }
    }

    private sealed class CombatWindow : Form
    {
        private readonly Form1 owner;
        private CombatEncounter? encounter;
        private GameCharacter? character;
        private readonly GameEngine engine;

        // Status
        private readonly Label stateLabel  = new() { AutoSize = true, ForeColor = Color.Lime };
        private readonly Label enemyLabel  = new() { AutoSize = true, ForeColor = Color.OrangeRed };
        private readonly Label fpLabel     = new() { AutoSize = true, ForeColor = Color.CornflowerBlue };
        private readonly TextBox logBox    = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                                                     Dock = DockStyle.Fill, Font = new Font("Consolas", 9f),
                                                     BackColor = Color.FromArgb(15, 15, 15), ForeColor = Color.Lime };
        // Action panels
        private readonly FlowLayoutPanel generalPanel = new() { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        private readonly FlowLayoutPanel weaponPanel  = new() { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        private readonly FlowLayoutPanel forcePanel   = new() { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
        private readonly Label forceHeader = new() { Text = "── Force Abilities ──", AutoSize = true, ForeColor = Color.CornflowerBlue, Font = new Font("Segoe UI", 9f, FontStyle.Italic) };
        private readonly Label weaponHeader= new() { AutoSize = true, ForeColor = Color.Goldenrod, Font = new Font("Segoe UI", 9f, FontStyle.Italic) };
        private readonly ComboBox itemBox  = new() { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };

        public CombatWindow(Form1 owner) : base()
        {
            this.owner  = owner;
            this.engine = owner.engine;
            Text = "Combat";
            Size = new Size(680, 600);
            MinimumSize = new Size(560, 480);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 200, owner.Location.Y + 160);
            BackColor = Color.FromArgb(18, 18, 18);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // status
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // log
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // general actions
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // weapon abilities
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // force actions
            Controls.Add(root);

            // Status strip
            var statusFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6) };
            statusFlow.Controls.Add(stateLabel);
            statusFlow.Controls.Add(enemyLabel);
            statusFlow.Controls.Add(fpLabel);
            root.Controls.Add(statusFlow, 0, 0);

            // Log
            root.Controls.Add(logBox, 0, 1);

            // General action buttons
            var genGroup = new GroupBox { Text = "Actions", Dock = DockStyle.Fill, ForeColor = Color.White, AutoSize = true };
            genGroup.Controls.Add(generalPanel);
            root.Controls.Add(genGroup, 0, 2);

            // Weapon ability buttons
            var weapGroup = new GroupBox { Text = "Weapon Abilities", Dock = DockStyle.Fill, ForeColor = Color.Goldenrod, AutoSize = true };
            weapGroup.Controls.Add(weaponHeader);
            weaponHeader.Dock = DockStyle.Top;
            weapGroup.Controls.Add(weaponPanel);
            root.Controls.Add(weapGroup, 0, 3);

            // Force action buttons
            var forceGroup = new GroupBox { Text = "Force", Dock = DockStyle.Fill, ForeColor = Color.CornflowerBlue, AutoSize = true };
            forceGroup.Controls.Add(forcePanel);
            root.Controls.Add(forceGroup, 0, 4);
        }

        public void SetEncounter(CombatEncounter enc, GameCharacter ch, GameEngine _)
        {
            encounter = enc;
            character = ch;
            RebuildActionButtons();
            UpdateView();
        }

        private void RebuildActionButtons()
        {
            generalPanel.Controls.Clear();
            weaponPanel.Controls.Clear();
            forcePanel.Controls.Clear();

            void AddBtn(string label, string action, string? item = null, FlowLayoutPanel? target = null)
            {
                var isForce  = action.StartsWith("Force") || label.StartsWith("Force") || (character?.ForceAbilities.Contains(action) ?? false);
                var isWeapon = !isForce && engine.WeaponAbilities.ContainsKey(action);
                var color    = isForce ? Color.CornflowerBlue : isWeapon ? Color.DarkGoldenrod : Color.DimGray;
                var blocked  = character is not null && engine.IsBlockedByFatigue(character, action);
                var btn = new Button
                {
                    Text = blocked ? $"{label} [Tired]" : label,
                    Width = 128, Height = 30,
                    BackColor = blocked ? Color.FromArgb(50, 50, 50) : color,
                    ForeColor = blocked ? Color.Gray : Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Enabled = !blocked
                };
                if (isWeapon && engine.WeaponAbilities.TryGetValue(action, out var wd))
                    btn.Font = new Font("Segoe UI", 8.5f);
                btn.Click += (_, _) => Resolve(action, item);
                (target ?? generalPanel).Controls.Add(btn);
            }

            // General
            AddBtn("Attack",       "attack");
            AddBtn("Heavy Strike", "heavy strike");
            AddBtn("Aimed Shot",   "aimed shot");
            AddBtn("Disarm",       "disarm");
            AddBtn("Guard",        "guard");
            AddBtn("Taunt",        "taunt");
            AddBtn("Flee",         "flee");

            // Use Item combo
            if (character is not null)
            {
                itemBox.Items.Clear();
                foreach (var it in character.Inventory.Where(x =>
                    x.Contains("medpack", StringComparison.OrdinalIgnoreCase) ||
                    x.Contains("stim", StringComparison.OrdinalIgnoreCase) ||
                    x.Contains("grenade", StringComparison.OrdinalIgnoreCase)))
                {
                    if (!itemBox.Items.Contains(it)) itemBox.Items.Add(it);
                }
                if (itemBox.Items.Count > 0) { itemBox.SelectedIndex = 0; generalPanel.Controls.Add(itemBox); }
                var useBtn = new Button { Text = "Use Item", Width = 80, Height = 30, BackColor = Color.DarkOliveGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                useBtn.Click += (_, _) => { if (itemBox.SelectedItem is string s) Resolve("use item", s); };
                generalPanel.Controls.Add(useBtn);
            }

            // Weapon abilities
            if (character is not null)
            {
                var weapAbils = engine.GetWeaponAbilities(character);
                if (weapAbils.Count > 0)
                {
                    weaponPanel.Visible = true;
                    var subtype = engine.GetWeaponSubtype(character.EquippedWeapon);
                    var offNote = !string.IsNullOrEmpty(character.OffHandWeapon) ? $" + {character.OffHandWeapon}" : "";
                    weaponHeader.Text = $"── {character.EquippedWeapon}{offNote} [{subtype}] ──";
                    foreach (var ability in weapAbils)
                    {
                        var stamCost = engine.WeaponAbilities.TryGetValue(ability, out var wa) ? wa.StaminaCost : 0;
                        AddBtn($"{ability}\n[{stamCost} Stam]", ability, target: weaponPanel);
                    }
                }
                else weaponPanel.Visible = false;
            }

            // Force abilities
            if (character is { IsForceUser: true } && character.ForceAbilities.Count > 0)
            {
                forcePanel.Visible = true;
                foreach (var ability in character.ForceAbilities)
                    AddBtn(ability, ability.ToLowerInvariant(), target: forcePanel);
            }
            else forcePanel.Visible = false;
        }

        private void Resolve(string action, string? itemName = null)
        {
            if (encounter is null || character is null) return;
            var text = engine.ResolveCombatAction(character, encounter, action, itemName);
            logBox.AppendText(text + Environment.NewLine + Environment.NewLine);
            logBox.ScrollToCaret();
            UpdateView();

            if (encounter.IsOver)
            {
                if (encounter.PlayerWon)
                    owner.AppendLog($"Combat complete. Rewards: {encounter.RewardCredits} credits, {encounter.RewardXp} XP, {encounter.RewardItem}.");
                else
                    owner.AppendLog("Combat ended — you retreated or fell.");

                if (!character.IsAlive || character.Hp <= 0)
                {
                    owner.HandleCharacterDeathAndReset("combat");
                    return;
                }
                owner.RefreshStatus();
                Close();
            }
        }

        private void UpdateView()
        {
            if (encounter is null || character is null) return;

            // Rebuild item combo in case inventory changed
            itemBox.Items.Clear();
            foreach (var it in character.Inventory.Where(x =>
                x.Contains("medpack", StringComparison.OrdinalIgnoreCase) ||
                x.Contains("stim", StringComparison.OrdinalIgnoreCase) ||
                x.Contains("grenade", StringComparison.OrdinalIgnoreCase)))
                if (!itemBox.Items.Contains(it)) itemBox.Items.Add(it);
            if (itemBox.Items.Count > 0 && itemBox.SelectedIndex < 0) itemBox.SelectedIndex = 0;

            stateLabel.Text = $"You: {character.Hp}/{character.MaxHp} HP  | Armor {character.Armor} | {character.Condition} | Round {encounter.Round} | Hunger {character.Hunger}/100 | Energy {character.Energy}/100 ({engine.GetEnergyState(character)})";
            enemyLabel.Text = $"Enemy: {encounter.EnemyName}  HP {encounter.EnemyHp}/{encounter.EnemyMaxHp}  Armor {encounter.EnemyArmor}" +
                              (encounter.EnemyStunned > 0 ? $"  [STUNNED {encounter.EnemyStunned}]" : "") +
                              (encounter.EnemyDisarmed ? "  [DISARMED]" : "");
            fpLabel.Text = character.IsForceUser
                ? $"Force: {character.ForcePoints}/{character.MaxForcePoints} FP  |  Rank: {character.JediRank}"
                : "";
        }
    }

    private sealed class RomanceWindow : Form
    {
        private readonly Form1 owner;

        // Top target panel
        private readonly TextBox targetNameBox    = new() { Width = 160, PlaceholderText = "NPC name" };
        private readonly ComboBox targetSpeciesBox = new() { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox targetPlanetBox  = new() { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };

        // Status
        private readonly Label statusLabel  = new() { AutoSize = true, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
        private readonly Label heartLabel   = new() { AutoSize = true, Font = new Font("Segoe UI", 20f) };

        // Love bar
        private readonly ProgressBar loveBar = new() { Minimum = 0, Maximum = 100, Height = 18, Width = 300 };
        private readonly Label loveLabel     = new() { AutoSize = true };

        // Gift
        private readonly ComboBox giftBox    = new() { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Button giftButton   = new() { Text = "Give Gift ♥", Width = 110, Height = 30 };

        // Action buttons
        private readonly Button flirtButton    = new() { Text = "Flirt ♥", Width = 100, Height = 34 };
        private readonly Button proposeButton  = new() { Text = "Propose 💍", Width = 110, Height = 34 };
        private readonly Button childButton    = new() { Text = "Have Child", Width = 110, Height = 34 };
        private readonly Button divorceButton  = new() { Text = "Separate", Width = 100, Height = 34 };
        private readonly Button legacyButton   = new() { Text = "Pass Legacy", Width = 110, Height = 34 };

        // Log
        private readonly TextBox logBox = new()
        {
            Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10f), Dock = DockStyle.Fill
        };

        // History
        private readonly ListBox historyBox = new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 10f) };
        private readonly ListBox giftLogBox = new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 10f) };
        private readonly ListBox childrenBox = new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 10f) };

        public RomanceWindow(Form1 owner)
        {
            this.owner = owner;
            Text = "Romance & Family";
            Size = new Size(860, 640);
            MinimumSize = new Size(700, 520);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 120, owner.Location.Y + 100);

            // Populate species / planet dropdowns from engine
            targetSpeciesBox.Items.AddRange(owner.engine.Races.Keys.ToArray<object>());
            if (targetSpeciesBox.Items.Count > 0) targetSpeciesBox.SelectedIndex = 0;
            targetPlanetBox.Items.AddRange(owner.engine.Planets.Keys.ToArray<object>());
            if (targetPlanetBox.Items.Count > 0) targetPlanetBox.SelectedItem =
                owner.character?.Location ?? targetPlanetBox.Items[0];

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // status strip
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // target input
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // action buttons
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 55)); // log
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 45)); // history tabs
            Controls.Add(root);

            // ── Row 0: Status strip ───────────────────────────────────────────
            var statusFlow = new FlowLayoutPanel
                { Dock = DockStyle.Fill, Padding = new Padding(8, 6, 8, 4), AutoSize = true };
            heartLabel.Text = "♡";
            heartLabel.ForeColor = Color.HotPink;
            statusFlow.Controls.Add(heartLabel);
            statusFlow.Controls.Add(statusLabel);
            loveBar.ForeColor = Color.HotPink;
            statusFlow.Controls.Add(new Label { Text = " ♥", AutoSize = true, ForeColor = Color.HotPink });
            statusFlow.Controls.Add(loveBar);
            statusFlow.Controls.Add(loveLabel);
            root.Controls.Add(statusFlow, 0, 0);

            // ── Row 1: Target NPC input ───────────────────────────────────────
            var inputFlow = new FlowLayoutPanel
                { Dock = DockStyle.Fill, Padding = new Padding(8, 2, 8, 2), AutoSize = true };
            inputFlow.Controls.Add(new Label { Text = "Name:", AutoSize = true });
            inputFlow.Controls.Add(targetNameBox);
            var randomNameBtn = new Button { Text = "Random", Width = 72, Height = 26 };
            randomNameBtn.Click += (_, _) =>
                targetNameBox.Text = owner.engine.GenerateNameForRace(targetSpeciesBox.Text);
            inputFlow.Controls.Add(randomNameBtn);
            inputFlow.Controls.Add(new Label { Text = "  Species:", AutoSize = true });
            inputFlow.Controls.Add(targetSpeciesBox);
            inputFlow.Controls.Add(new Label { Text = "  Planet:", AutoSize = true });
            inputFlow.Controls.Add(targetPlanetBox);
            root.Controls.Add(inputFlow, 0, 1);

            // ── Row 2: Action buttons + gift ─────────────────────────────────
            var actFlow = new FlowLayoutPanel
                { Dock = DockStyle.Fill, Padding = new Padding(8, 4, 8, 4), AutoSize = true };
            actFlow.Controls.Add(flirtButton);
            actFlow.Controls.Add(proposeButton);
            actFlow.Controls.Add(childButton);
            actFlow.Controls.Add(divorceButton);
            actFlow.Controls.Add(legacyButton);
            actFlow.Controls.Add(new Label { Text = "  Gift:", AutoSize = true });
            actFlow.Controls.Add(giftBox);
            actFlow.Controls.Add(giftButton);
            var closeBtn = new Button { Text = "Close", Width = 80, Height = 30 };
            closeBtn.Click += (_, _) => Close();
            actFlow.Controls.Add(closeBtn);
            root.Controls.Add(actFlow, 0, 2);

            // ── Row 3: Romance log ────────────────────────────────────────────
            var logGroup = new GroupBox { Text = "Romance Log", Dock = DockStyle.Fill, Padding = new Padding(4) };
            logGroup.Controls.Add(logBox);
            root.Controls.Add(logGroup, 0, 3);

            // ── Row 4: History tabs ───────────────────────────────────────────
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var histTab   = new TabPage("History");   histTab.Controls.Add(historyBox);
            var giftTab   = new TabPage("Gifts");     giftTab.Controls.Add(giftLogBox);
            var childTab  = new TabPage("Children");  childTab.Controls.Add(childrenBox);
            tabs.TabPages.AddRange(new[] { histTab, giftTab, childTab });
            root.Controls.Add(tabs, 0, 4);

            // ── Wire button events ────────────────────────────────────────────
            flirtButton.Click += (_, _) =>
            {
                if (!ValidateChar()) return;
                var npc     = GetOrGenerateTarget();
                var result  = owner.engine.FlirtWithNpc(owner.character!, npc.name, npc.species, npc.planet);
                AppendLog(result.message, result.success ? Color.HotPink : Color.Orange);
                if (result.success) ApplyHeart();
                owner.RefreshStatus();
                RefreshState();
            };

            proposeButton.Click += (_, _) =>
            {
                if (!ValidateChar()) return;
                var result = owner.engine.ProposeToNpc(owner.character!);
                AppendLog(result.message, result.success ? Color.Gold : Color.OrangeRed);
                owner.AppendLog(result.message);
                owner.RefreshStatus();
                RefreshState();
            };

            childButton.Click += (_, _) =>
            {
                if (!ValidateChar()) return;
                var msg = owner.engine.HaveChild(owner.character!);
                AppendLog(msg, Color.Cyan);
                owner.AppendLog(msg);
                owner.RefreshStatus();
                RefreshState();
            };

            divorceButton.Click += (_, _) =>
            {
                if (!ValidateChar()) return;
                var confirm = MessageBox.Show("Are you sure you want to separate?",
                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;
                var result = owner.engine.DivorceSpouse(owner.character!);
                AppendLog(result.message, Color.Gray);
                owner.AppendLog(result.message);
                owner.RefreshStatus();
                RefreshState();
            };

            legacyButton.Click += (_, _) =>
            {
                if (!ValidateChar()) return;
                var msg = owner.engine.AdvanceLegacy(owner.character!);
                AppendLog(msg, Color.Silver);
                owner.AppendLog(msg);
                owner.RefreshStatus();
                RefreshState();
            };

            giftButton.Click += (_, _) =>
            {
                if (!ValidateChar()) return;
                if (string.IsNullOrWhiteSpace(giftBox.Text)) return;
                var result = owner.engine.GiftNpc(owner.character!, giftBox.Text);
                AppendLog(result.message, result.success ? Color.HotPink : Color.Orange);
                if (result.success) ApplyHeart();
                owner.RefreshStatus();
                RefreshState();
            };

            // Buttons continued — Jedi hidden spouse
            var hideSpouseCheck = new CheckBox
            {
                Text = "Conceal relationship (Jedi code)", AutoSize = true,
                ForeColor = Color.CornflowerBlue
            };
            hideSpouseCheck.CheckedChanged += (_, _) =>
            {
                if (owner.character is not null)
                    owner.character.SpouseHidden = hideSpouseCheck.Checked;
            };
            actFlow.Controls.Add(hideSpouseCheck);

            // Pre-fill NPC name if romance already active
            RefreshState();

            // After RefreshState so IsForceUser is always updated
            void UpdateHideCheck()
            {
                hideSpouseCheck.Visible = owner.character?.IsForceUser == true;
                hideSpouseCheck.Checked = owner.character?.SpouseHidden == true;
            }
            UpdateHideCheck();
        }

        private (string name, string species, string planet) GetOrGenerateTarget()
        {
            var fam = owner.character!.Family;
            // If already courting, use existing target
            if (!string.IsNullOrWhiteSpace(fam.RomanceTargetName))
                return (fam.RomanceTargetName, fam.RomanceTargetSpecies, fam.RomanceTargetPlanet);

            var name    = string.IsNullOrWhiteSpace(targetNameBox.Text)
                ? owner.engine.GenerateNameForRace(targetSpeciesBox.Text)
                : targetNameBox.Text.Trim();
            var species = targetSpeciesBox.Text;
            var planet  = targetPlanetBox.Text;
            return (name, species, planet);
        }

        private void RefreshState()
        {
            if (owner.character is null) return;
            var fam  = owner.character.Family;
            var ch   = owner.character;

            // Love bar
            loveBar.Value = Math.Clamp(fam.LovePoints, 0, 100);
            loveLabel.Text = $"{fam.LovePoints}/100";

            // Heart icon & status
            heartLabel.Text = fam.FlirtStage switch
            {
                "married"      => "💑",
                "devoted"      => "💗",
                "crush"        => "💓",
                "acquaintance" => "💛",
                _              => "♡"
            };

            var targetInfo = fam.Married
                ? $"Married to {fam.SpouseName} ({fam.SpouseSpecies})"
                : !string.IsNullOrWhiteSpace(fam.RomanceTargetName)
                    ? $"Courting {fam.RomanceTargetName} ({fam.RomanceTargetSpecies}) — {fam.FlirtStage}"
                    : "No active romance";
            statusLabel.Text = $"{targetInfo}  |  Morale {ch.Morale}  |  Rep {ch.Reputation}";

            // Pre-fill name box if courting
            if (!string.IsNullOrWhiteSpace(fam.RomanceTargetName) && string.IsNullOrWhiteSpace(targetNameBox.Text))
                targetNameBox.Text = fam.RomanceTargetName;

            // Button availability
            flirtButton.Enabled   = !fam.Married;
            proposeButton.Enabled = !fam.Married && fam.LovePoints >= 70;
            childButton.Enabled   = fam.Married;
            divorceButton.Enabled = fam.Married;
            legacyButton.Enabled  = ch.Family.Children.Count > 0;

            // Gift box — populate from inventory
            var current = giftBox.Text;
            giftBox.Items.Clear();
            foreach (var item in ch.Inventory.OrderBy(x => x))
                if (!giftBox.Items.Contains(item))
                    giftBox.Items.Add(item);
            if (giftBox.Items.Contains(current)) giftBox.SelectedItem = current;
            else if (giftBox.Items.Count > 0) giftBox.SelectedIndex = 0;
            giftButton.Enabled = !fam.Married && !string.IsNullOrWhiteSpace(fam.RomanceTargetName) && giftBox.Items.Count > 0;

            // History / gift log / children lists
            historyBox.Items.Clear();
            foreach (var h in fam.RomanceHistory) historyBox.Items.Add(h);
            giftLogBox.Items.Clear();
            foreach (var g in fam.GiftLog) giftLogBox.Items.Add(g);
            childrenBox.Items.Clear();
            foreach (var kid in ch.Family.Children) childrenBox.Items.Add(kid);
        }

        private void AppendLog(string msg, Color? color = null)
        {
            if (string.IsNullOrWhiteSpace(msg)) return;
            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionLength = 0;
            // RichTextBox not available, use plain TextBox append
            logBox.AppendText($"{msg}{Environment.NewLine}");
            logBox.ScrollToCaret();
        }

        private void ApplyHeart()
        {
            heartLabel.ForeColor = Color.HotPink;
            var t = new System.Windows.Forms.Timer { Interval = 600 };
            t.Tick += (_, _) => { heartLabel.ForeColor = Color.HotPink; t.Stop(); };
            t.Start();
        }

        private bool ValidateChar()
        {
            if (owner.character is null)
            { AppendLog("Create a character first."); return false; }
            return true;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Jedi Awakening Window
    // ══════════════════════════════════════════════════════════════════════════
    private sealed class JediAwakeningWindow : Form
    {
        private readonly Form1 owner;
        private readonly JediAwakeningEvent evt;

        public JediAwakeningWindow(Form1 owner, JediAwakeningEvent evt)
        {
            this.owner = owner;
            this.evt   = evt;

            Text = "A Stranger Approaches...";
            Size = new Size(640, 420);
            MinimumSize = new Size(520, 340);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 160, owner.Location.Y + 140);
            BackColor = Color.FromArgb(18, 18, 18);
            ForeColor = Color.White;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.BackColor = Color.FromArgb(18, 18, 18);
            Controls.Add(root);

            var titleLabel = new Label
            {
                Text = $"Jedi Master {evt.MasterName}",
                AutoSize = true, ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                Padding = new Padding(12, 8, 0, 4)
            };
            root.Controls.Add(titleLabel, 0, 0);

            var textBox = new TextBox
            {
                Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(28, 28, 28), ForeColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.None,
                Text = evt.Dialogue + Environment.NewLine + Environment.NewLine + evt.OfferText
            };
            root.Controls.Add(textBox, 0, 1);

            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(10) };

            var acceptBtn = new Button
            {
                Text = "Accept — Join the Order", Width = 190, Height = 36,
                BackColor = Color.MidnightBlue, ForeColor = Color.Gold,
                FlatStyle = FlatStyle.Flat
            };
            acceptBtn.Click += (_, _) =>
            {
                if (owner.character is null) { Close(); return; }
                var result = owner.engine.AcceptJediTraining(owner.character);
                owner.AppendLog(result);
                owner.RefreshStatus();
                Close();
            };

            var declineBtn = new Button
            {
                Text = "Decline", Width = 100, Height = 36,
                BackColor = Color.FromArgb(50, 0, 0), ForeColor = Color.OrangeRed,
                FlatStyle = FlatStyle.Flat
            };
            declineBtn.Click += (_, _) =>
            {
                if (owner.character is null) { Close(); return; }
                var result = owner.engine.DeclineJediTraining(owner.character);
                owner.AppendLog(result);
                Close();
            };

            btnFlow.Controls.Add(acceptBtn);
            btnFlow.Controls.Add(declineBtn);
            root.Controls.Add(btnFlow, 0, 2);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Jedi Order Window
    // ══════════════════════════════════════════════════════════════════════════
    private sealed class JediOrderWindow : Form
    {
        private readonly Form1 owner;

        // Temple tab
        private readonly ComboBox zoneBox   = new() { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox actionBox = new() { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox  templeLog = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill,
                                                       Font = new Font("Consolas", 9f), BackColor = Color.FromArgb(18,18,18), ForeColor = Color.Lime };

        // Rank tab
        private readonly Label rankLabel    = new() { AutoSize = true, Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.Gold };
        private readonly ProgressBar xpBar  = new() { Minimum = 0, Maximum = 500, Width = 360, Height = 20 };
        private readonly Label xpLabel      = new() { AutoSize = true };
        private readonly ListBox abilityBox = new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 10f),
                                                       BackColor = Color.FromArgb(14,14,26), ForeColor = Color.CornflowerBlue };
        private readonly Label fpLabel      = new() { AutoSize = true };

        // Lightsaber tab
        private readonly TextBox reqBox     = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill,
                                                       Font = new Font("Consolas", 9f), BackColor = Color.FromArgb(18,18,18), ForeColor = Color.WhiteSmoke };
        private readonly ComboBox colorBox  = new() { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Button craftBtn    = new() { Text = "Craft Lightsaber", Width = 150, Height = 34,
                                                       BackColor = Color.MidnightBlue, ForeColor = Color.Cyan, FlatStyle = FlatStyle.Flat };
        private readonly Label craftResult  = new() { AutoSize = true, ForeColor = Color.Cyan };

        public JediOrderWindow(Form1 owner)
        {
            this.owner = owner;

            Text = "Jedi Order";
            Size = new Size(700, 540);
            MinimumSize = new Size(580, 440);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 80, owner.Location.Y + 60);
            BackColor = Color.FromArgb(18, 18, 18);
            ForeColor = Color.White;

            var tabs = new TabControl { Dock = DockStyle.Fill };
            Controls.Add(tabs);

            // ── Tab 1: Temple ─────────────────────────────────────────────────
            var templePage = new TabPage("Temple");
            var templeRoot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            templeRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            templeRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            templeRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            templeRoot.BackColor = Color.FromArgb(18, 18, 18);

            var zones = owner.engine.GetJediTempleZones();
            zoneBox.Items.AddRange(zones.ToArray<object>());
            if (zones.Count > 0) zoneBox.SelectedIndex = 0;

            zoneBox.SelectedIndexChanged += (_, _) =>
            {
                actionBox.Items.Clear();
                if (zoneBox.SelectedItem is string z)
                    actionBox.Items.AddRange(owner.engine.GetJediTempleActions(z).ToArray<object>());
                if (actionBox.Items.Count > 0) actionBox.SelectedIndex = 0;
            };

            // Trigger initial population of actionBox
            if (zoneBox.Items.Count > 0)
            {
                actionBox.Items.AddRange(owner.engine.GetJediTempleActions(zones[0]).ToArray<object>());
                if (actionBox.Items.Count > 0) actionBox.SelectedIndex = 0;
            }

            var pickFlow = new FlowLayoutPanel { AutoSize = true, Padding = new Padding(8) };
            pickFlow.Controls.Add(new Label { Text = "Zone:", AutoSize = true });
            pickFlow.Controls.Add(zoneBox);
            pickFlow.Controls.Add(new Label { Text = "  Action:", AutoSize = true });
            pickFlow.Controls.Add(actionBox);
            templeRoot.Controls.Add(pickFlow, 0, 0);

            var execBtn = new Button { Text = "Execute", Width = 120, Height = 32,
                BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            execBtn.Click += (_, _) =>
            {
                if (owner.character is null) return;
                if (zoneBox.SelectedItem is not string z || actionBox.SelectedItem is not string a) return;
                var result = owner.engine.ExecuteJediTempleAction(owner.character, z, a);
                templeLog.AppendText(result + Environment.NewLine + Environment.NewLine);
                templeLog.ScrollToCaret();
                owner.AppendLog(result);
                RefreshRankTab();
                owner.RefreshStatus();
            };
            var execFlow = new FlowLayoutPanel { AutoSize = true, Padding = new Padding(8, 0, 8, 4) };
            execFlow.Controls.Add(execBtn);
            templeRoot.Controls.Add(execFlow, 0, 1);
            templeRoot.Controls.Add(templeLog, 0, 2);
            templePage.Controls.Add(templeRoot);
            tabs.TabPages.Add(templePage);

            // ── Tab 2: Rank & Abilities ───────────────────────────────────────
            var rankPage = new TabPage("Rank");
            var rankRoot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
            rankRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rankRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rankRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rankRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rankRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            rankRoot.BackColor = Color.FromArgb(18, 18, 18);
            rankRoot.Padding = new Padding(10);
            rankRoot.Controls.Add(rankLabel, 0, 0);
            rankRoot.Controls.Add(xpBar, 0, 1);
            rankRoot.Controls.Add(xpLabel, 0, 2);
            rankRoot.Controls.Add(fpLabel, 0, 3);
            rankRoot.Controls.Add(abilityBox, 0, 4);
            rankPage.Controls.Add(rankRoot);
            tabs.TabPages.Add(rankPage);

            // ── Tab 3: Lightsaber ─────────────────────────────────────────────
            var saberPage = new TabPage("Lightsaber");
            var saberRoot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            saberRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            saberRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            saberRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            saberRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            saberRoot.BackColor = Color.FromArgb(18, 18, 18);

            saberRoot.Controls.Add(reqBox, 0, 0);

            colorBox.Items.AddRange(new object[] { "blue", "green", "yellow", "white", "purple" });
            colorBox.SelectedIndex = 0;
            var colorFlow = new FlowLayoutPanel { AutoSize = true, Padding = new Padding(8, 4, 8, 4) };
            colorFlow.Controls.Add(new Label { Text = "Crystal colour:", AutoSize = true });
            colorFlow.Controls.Add(colorBox);
            saberRoot.Controls.Add(colorFlow, 0, 1);

            craftBtn.Click += (_, _) =>
            {
                if (owner.character is null) return;
                var result = owner.engine.CraftLightsaber(owner.character, colorBox.SelectedItem as string);
                craftResult.Text = result;
                owner.AppendLog(result);
                RefreshSaberTab();
                owner.RefreshStatus();
            };
            var craftFlow = new FlowLayoutPanel { AutoSize = true, Padding = new Padding(8, 0, 8, 4) };
            craftFlow.Controls.Add(craftBtn);
            saberRoot.Controls.Add(craftFlow, 0, 2);
            saberRoot.Controls.Add(craftResult, 0, 3);
            saberPage.Controls.Add(saberRoot);
            tabs.TabPages.Add(saberPage);

            RefreshRankTab();
            RefreshSaberTab();
        }

        private void RefreshRankTab()
        {
            if (owner.character is null) return;
            var ch = owner.character;
            rankLabel.Text = $"Rank: {(string.IsNullOrWhiteSpace(ch.JediRank) ? "Initiate" : ch.JediRank)}";
            xpBar.Value = Math.Clamp(ch.JediXp, 0, 500);
            xpLabel.Text = $"{ch.JediXp} XP  (next rank at {GetNextRankThreshold(ch.JediRank)} XP)";
            fpLabel.Text  = $"Force Points: {ch.ForcePoints} / {ch.MaxForcePoints}";
            abilityBox.Items.Clear();
            foreach (var a in ch.ForceAbilities) abilityBox.Items.Add(a);
        }

        private static int GetNextRankThreshold(string rank) => rank switch
        {
            "Initiate" or "" => 80,
            "Padawan"        => 220,
            "Knight"         => 450,
            _                => 450
        };

        private void RefreshSaberTab()
        {
            if (owner.character is null) return;
            var (_, msg) = owner.engine.CheckLightsaberCraftRequirements(owner.character);
            reqBox.Text = msg;
            craftBtn.Enabled = !owner.character.LightsaberCrafted;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ═══════════════════════════════════════════════════════════════════════════
    // FOOD WINDOW — Eat from inventory, cook raw ingredients, buy food summary
    // ═══════════════════════════════════════════════════════════════════════════
    private sealed class FoodWindow : Form
    {
        private readonly Form1 owner;
        private readonly ListBox foodListBox   = new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 10f) };
        private readonly ListBox recipeListBox = new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 10f) };
        private readonly TextBox logBox        = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 8, 8), ForeColor = Color.Lime, Font = new Font("Consolas", 9f) };
        private readonly Label hungerLabel     = new() { AutoSize = true, ForeColor = Color.Orange, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
        private readonly Label energyLabel     = new() { AutoSize = true, ForeColor = Color.CornflowerBlue, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
        private readonly ProgressBar hungerBar = new() { Minimum = 0, Maximum = 100, Height = 16, Width = 260 };
        private readonly ProgressBar energyBar = new() { Minimum = 0, Maximum = 100, Height = 16, Width = 260 };

        public FoodWindow(Form1 owner)
        {
            this.owner = owner;
            Text = "Eat & Cook Food";
            Size = new Size(680, 600);
            MinimumSize = new Size(580, 480);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 100, owner.Location.Y + 100);
            BackColor = Color.FromArgb(15, 15, 20);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // status bars
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 40)); // food in inventory
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // cooking recipes
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // log
            Controls.Add(root);

            // ── Row 0: Hunger & Energy bars ───────────────────────────────────
            var statusFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(8, 6, 8, 4) };
            hungerBar.ForeColor = Color.OrangeRed;
            energyBar.ForeColor = Color.CornflowerBlue;
            statusFlow.Controls.Add(new Label { Text = "Hunger:", AutoSize = true, ForeColor = Color.White });
            statusFlow.Controls.Add(hungerBar);
            statusFlow.Controls.Add(hungerLabel);
            statusFlow.Controls.Add(new Label { Text = "  Energy:", AutoSize = true, ForeColor = Color.White });
            statusFlow.Controls.Add(energyBar);
            statusFlow.Controls.Add(energyLabel);
            root.Controls.Add(statusFlow, 0, 0);

            // ── Row 1: Food in inventory ──────────────────────────────────────
            var foodGroup = new GroupBox { Text = "Food in Inventory  (select + Eat)", Dock = DockStyle.Fill, ForeColor = Color.Lime };
            foodGroup.Controls.Add(foodListBox);
            var eatBtn = new Button { Text = "Eat Selected", Height = 28, Dock = DockStyle.Bottom, BackColor = Color.DarkGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            eatBtn.Click += (_, _) => DoEat();
            foodGroup.Controls.Add(eatBtn);
            root.Controls.Add(foodGroup, 0, 1);

            // ── Row 2: Cooking recipes ────────────────────────────────────────
            var cookGroup = new GroupBox { Text = "Cooking Recipes  (select + Cook)", Dock = DockStyle.Fill, ForeColor = Color.Goldenrod };
            cookGroup.Controls.Add(recipeListBox);
            var cookBtn = new Button { Text = "Cook Selected Recipe", Height = 28, Dock = DockStyle.Bottom, BackColor = Color.SaddleBrown, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            cookBtn.Click += (_, _) => DoCook();
            cookGroup.Controls.Add(cookBtn);
            root.Controls.Add(cookGroup, 0, 2);

            // ── Row 3: Log ────────────────────────────────────────────────────
            var logGroup = new GroupBox { Text = "Activity Log", Dock = DockStyle.Fill, ForeColor = Color.White };
            logGroup.Controls.Add(logBox);
            var closeBtn = new Button { Text = "Close", Height = 28, Dock = DockStyle.Bottom, BackColor = Color.DimGray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            closeBtn.Click += (_, _) => Close();
            logGroup.Controls.Add(closeBtn);
            root.Controls.Add(logGroup, 0, 3);

            RefreshLists();
        }

        private void RefreshLists()
        {
            var ch = owner.character;
            if (ch is null) return;

            // Status bars
            hungerBar.Value = Math.Clamp(ch.Hunger, 0, 100);
            hungerLabel.Text = $"{ch.Hunger}/100  ({owner.engine.GetNutritionState(ch)})";
            energyBar.Value = Math.Clamp(ch.Energy, 0, 100);
            energyLabel.Text = $"{ch.Energy}/100  ({owner.engine.GetEnergyState(ch)})";

            // Food in inventory
            foodListBox.Items.Clear();
            foreach (var item in ch.Inventory
                .Where(i => owner.engine.FoodItems.ContainsKey(i))
                .OrderBy(i => i))
            {
                var fi = owner.engine.FoodItems[item];
                var tag = fi.RequiresCooking ? " [must cook]" : "";
                foodListBox.Items.Add($"{item}  — +{fi.HungerValue} hunger, +{fi.EnergyValue} energy{(fi.HpBonus > 0 ? $", +{fi.HpBonus} HP" : "")}{tag}");
            }
            if (foodListBox.Items.Count == 0) foodListBox.Items.Add("(no food in inventory — buy from merchants or harvest)");

            // Cooking recipes
            recipeListBox.Items.Clear();
            foreach (var kvp in owner.engine.CookingRecipes.OrderBy(x => x.Key))
            {
                var recipe = kvp.Value;
                var ingredients = string.Join(", ", recipe.Inputs.Select(i => $"{i.Quantity}× {i.Item}"));
                // Check if player has all ingredients
                var canCook = recipe.Inputs.All(i =>
                    ch.Inventory.Count(x => x.Equals(i.Item, StringComparison.OrdinalIgnoreCase)) >= i.Quantity);
                var tag = canCook ? "  ✓" : "  ✗";
                recipeListBox.Items.Add($"{recipe.Output}{tag}  [{recipe.TimeHours}h]  Needs: {ingredients}");
            }
        }

        private void DoEat()
        {
            var ch = owner.character;
            if (ch is null || foodListBox.SelectedItem is not string selected) return;
            // Extract item name (before the em-dash)
            var itemName = selected.Split(new[] { "  —" }, StringSplitOptions.None)[0].Trim();
            if (itemName.StartsWith("(")) return;
            var (ok, msg) = owner.engine.EatFood(ch, itemName);
            logBox.AppendText(msg + Environment.NewLine);
            logBox.ScrollToCaret();
            owner.AppendLog(msg);
            owner.RefreshStatus();
            RefreshLists();
        }

        private void DoCook()
        {
            var ch = owner.character;
            if (ch is null || recipeListBox.SelectedItem is not string selected) return;
            // Extract recipe name (before first space+✓/✗ or [)
            var recipeName = selected.Split(new[] { "  ✓", "  ✗", "  [" }, StringSplitOptions.None)[0].Trim();
            var (ok, msg) = owner.engine.CookFood(ch, recipeName);
            logBox.AppendText(msg + Environment.NewLine);
            logBox.ScrollToCaret();
            owner.AppendLog(msg);
            owner.RefreshStatus();
            RefreshLists();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MINING / HARVEST / WOODCUT WINDOW  (scan → minigame → mine / chop / harvest)
    // ═══════════════════════════════════════════════════════════════════════════
    private sealed class WoodCuttingWindow : Form
    {
        private readonly Form1 owner;
        private readonly string zone;
        private List<WoodTypeData> woodTypes = new();
        private int selectedIndex = -1;

        private readonly Panel   forestCanvas   = new() { Height = 200, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(8, 18, 8) };
        private readonly Label   surveyHint     = new() { AutoSize = true, ForeColor = Color.LightGray, Text = "Click [Survey Forest] to locate trees, then select a species to cut." };
        private readonly Label   woodInfoLabel  = new() { AutoSize = false, Height = 54, Dock = DockStyle.Fill, ForeColor = Color.LightGreen, Font = new Font("Consolas", 8.5f), Padding = new Padding(6, 3, 0, 0), BackColor = Color.FromArgb(8, 18, 8) };
        private readonly Label   nodeLabel      = new() { AutoSize = false, Height = 44, Dock = DockStyle.Fill, ForeColor = Color.Goldenrod, Font = new Font("Consolas", 8.5f), Padding = new Padding(6, 3, 0, 0), BackColor = Color.FromArgb(15, 12, 5) };
        private readonly Label   capLabel       = new() { AutoSize = true, ForeColor = Color.Orange };
        private readonly TextBox logBox         = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 8, 8), ForeColor = Color.LightGreen, Font = new Font("Consolas", 9f) };

        private readonly Button surveyBtn = new() { Text = "🌲 Survey Forest", Width = 140, Height = 30, BackColor = Color.DarkGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        private readonly Button chopBtn   = new() { Text = "🪓 Chop Selected", Width = 140, Height = 30, BackColor = Color.SaddleBrown, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };
        private readonly Button chopAllBtn= new() { Text = "🪓 Chop Any Wood", Width = 140, Height = 30, BackColor = Color.FromArgb(120, 60, 10), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

        public WoodCuttingWindow(Form1 owner, string zone)
        {
            this.owner = owner;
            this.zone  = zone;
            var planet = owner.character?.Location ?? "";

            Text            = $"Woodcutting — {zone}  ·  {planet}";
            Size            = new Size(680, 640);
            MinimumSize     = new Size(560, 520);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition   = FormStartPosition.Manual;
            Location        = new Point(owner.Location.X + 80, owner.Location.Y + 80);
            BackColor       = Color.FromArgb(12, 15, 10);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 7, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // 0: button strip
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); // 1: tool/node status
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); // 2: wood info
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 205));// 3: forest canvas
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // 4: hint
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // 5: log
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // 6: close
            Controls.Add(root);

            // ── Row 0: Buttons ─────────────────────────────────────────────
            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6) };
            btnFlow.Controls.Add(surveyBtn);
            btnFlow.Controls.Add(chopBtn);
            btnFlow.Controls.Add(chopAllBtn);
            btnFlow.Controls.Add(capLabel);
            root.Controls.Add(btnFlow, 0, 0);

            // ── Row 1: Node status ─────────────────────────────────────────
            var nodeGroup = new GroupBox { Text = "Grove Status", Dock = DockStyle.Fill, ForeColor = Color.Goldenrod };
            nodeGroup.Controls.Add(nodeLabel);
            root.Controls.Add(nodeGroup, 0, 1);

            // ── Row 2: Selected wood info ──────────────────────────────────
            var infoGroup = new GroupBox { Text = "Selected Wood Species", Dock = DockStyle.Fill, ForeColor = Color.LightGreen };
            infoGroup.Controls.Add(woodInfoLabel);
            root.Controls.Add(infoGroup, 0, 2);

            // ── Row 3: Forest canvas ───────────────────────────────────────
            var canvasGroup = new GroupBox { Text = "Forest Survey", Dock = DockStyle.Fill, ForeColor = Color.ForestGreen };
            forestCanvas.Dock = DockStyle.Fill;
            canvasGroup.Controls.Add(forestCanvas);
            canvasGroup.Controls.Add(surveyHint);
            surveyHint.Dock = DockStyle.Top;
            root.Controls.Add(canvasGroup, 0, 3);

            // ── Row 4: Hint ────────────────────────────────────────────────
            root.Controls.Add(new Label
            {
                AutoSize = true, ForeColor = Color.DimGray,
                Padding = new Padding(6, 2, 0, 0),
                Text = "Click a tree card to select a species. Wood types vary by planet."
            }, 0, 4);

            // ── Row 5: Log ─────────────────────────────────────────────────
            var logGroup = new GroupBox { Text = "Activity Log", Dock = DockStyle.Fill, ForeColor = Color.White };
            logGroup.Controls.Add(logBox);
            root.Controls.Add(logGroup, 0, 5);

            // ── Row 6: Close ───────────────────────────────────────────────
            var closeFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6) };
            var closeBtn = new Button { Text = "Close", Width = 90, Height = 28 };
            closeBtn.Click += (_, _) => Close();
            closeFlow.Controls.Add(closeBtn);
            root.Controls.Add(closeFlow, 0, 6);

            forestCanvas.Paint      += OnCanvasPaint;
            forestCanvas.MouseClick += OnCanvasClick;
            surveyBtn.Click  += (_, _) => DoSurvey();
            chopBtn.Click    += (_, _) => DoChop(selectedIndex >= 0 ? woodTypes[selectedIndex].Name : "");
            chopAllBtn.Click += (_, _) => DoChop("");

            UpdateNodeLabel();
            UpdateCapLabel();
        }

        private void AppendLog(string text)
        {
            logBox.AppendText(text + "\r\n");
            logBox.ScrollToCaret();
        }

        // ── Survey: load wood types and redraw canvas ──────────────────────
        private void DoSurvey()
        {
            if (owner.character is null) return;
            woodTypes = owner.engine.GetPlanetWoodTypes(owner.character.Location);
            selectedIndex = -1;
            chopBtn.Enabled = false;
            woodInfoLabel.Text = "";

            if (woodTypes.Count == 0)
            {
                AppendLog($"No harvestable trees found in {zone} on {owner.character.Location}.");
                surveyHint.Text = "No wood species available here.";
            }
            else
            {
                AppendLog($"Survey complete. {woodTypes.Count} wood species found:");
                foreach (var w in woodTypes)
                    AppendLog($"  🌲 {w.Name}  [Hardness {w.Hardness}/10]  — {w.Description}");
                surveyHint.Text = "Click a tree card to select a species, then Chop.";
            }

            forestCanvas.Invalidate();
            UpdateNodeLabel();
            UpdateCapLabel();
        }

        private void DoChop(string woodName)
        {
            if (owner.character is null) return;
            var (ok, msg, obtained) = owner.engine.CutWood(owner.character, zone, woodName);
            AppendLog(ok ? $"🪓 {msg}" : $"✗ {msg}");
            if (ok)
            {
                owner.AppendLog($"🪓 Cut: {string.Join(", ", obtained)}");
                owner.RefreshStatus();
                forestCanvas.Invalidate();
            }
            UpdateNodeLabel();
            UpdateCapLabel();
        }

        private void UpdateNodeLabel()
        {
            if (owner.character is null) return;
            var (dep, left, respawn) = owner.engine.GetNodeStatus(owner.character.Location, zone, "wood");
            if (dep)
            {
                nodeLabel.Text     = $"⚠ Grove depleted — trees will regrow in {respawn} rotation(s).";
                nodeLabel.ForeColor = Color.OrangeRed;
                chopBtn.Enabled   = false;
                chopAllBtn.Enabled = false;
            }
            else
            {
                nodeLabel.Text     = $"🌲 Grove healthy — {left}/4 cut(s) remaining this rotation.";
                nodeLabel.ForeColor = Color.Goldenrod;
                chopAllBtn.Enabled = true;
                if (selectedIndex >= 0) chopBtn.Enabled = true;
            }
        }

        private void UpdateCapLabel()
        {
            if (owner.character is null) return;
            var cap = owner.engine.GetInventoryCapacity(owner.character);
            capLabel.Text      = $"  Inv: {owner.character.Inventory.Count}/{cap}";
            capLabel.ForeColor = owner.character.Inventory.Count >= cap ? Color.Red : Color.Orange;
        }

        private void SelectTree(int idx)
        {
            selectedIndex = idx;
            if (idx >= 0 && idx < woodTypes.Count)
            {
                var w = woodTypes[idx];
                woodInfoLabel.Text =
                    $"{w.Name}  [Hardness {w.Hardness}/10]  Value: {w.BaseValue} cr/log\r\n{w.Description}";
                chopBtn.Enabled = true;
            }
            forestCanvas.Invalidate();
        }

        // ── Canvas paint ───────────────────────────────────────────────────
        private void OnCanvasPaint(object? s, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.FromArgb(8, 18, 8));

            if (woodTypes.Count == 0)
            {
                g.DrawString("Survey the forest to locate wood species...",
                    new Font("Consolas", 10f), Brushes.DimGray, 20, 80);
                return;
            }

            var (nodeDep, _, _) = owner.character is not null
                ? owner.engine.GetNodeStatus(owner.character.Location, zone, "wood")
                : (false, 0, 0);

            int cardW = Math.Max(110, (forestCanvas.Width - 20) / Math.Max(1, woodTypes.Count));
            for (int i = 0; i < woodTypes.Count; i++)
            {
                var w     = woodTypes[i];
                bool sel  = i == selectedIndex;
                int x     = 10 + i * cardW;
                int y     = 12;
                int cw    = cardW - 8;
                int ch    = forestCanvas.Height - 28;

                // Background
                var bg = nodeDep   ? Color.FromArgb(30, 40, 30)
                       : sel       ? Color.FromArgb(20, 60, 20)
                                   : Color.FromArgb(12, 28, 12);
                using var bgBrush = new SolidBrush(bg);
                g.FillRectangle(bgBrush, x, y, cw, ch);

                var border = nodeDep ? Color.DimGray : sel ? Color.LimeGreen : Color.ForestGreen;
                using var pen = new Pen(border, sel ? 2 : 1);
                g.DrawRectangle(pen, x, y, cw, ch);

                // Tree icon (ASCII art, stacked lines)
                var iconBrush  = nodeDep ? Brushes.DimGray : sel ? Brushes.LightGreen : Brushes.ForestGreen;
                var textBrush  = nodeDep ? Brushes.DimGray : Brushes.White;
                var treeFont   = new Font("Courier New", 11f);
                var labelFont  = new Font("Consolas", 7.5f);
                int cx = x + cw / 2;
                g.DrawString("  🌲", new Font("Segoe UI Symbol", 20f), nodeDep ? Brushes.Gray : Brushes.LimeGreen, x + 2, y + 4);
                g.DrawString(w.Name.Length > 14 ? w.Name[..14] : w.Name, labelFont, textBrush, x + 2, y + 46);
                g.DrawString($"Hard: {w.Hardness}/10", labelFont, nodeDep ? Brushes.DimGray : Brushes.LightGray, x + 2, y + 60);
                g.DrawString($"{w.BaseValue} cr/log", labelFont, nodeDep ? Brushes.DimGray : Brushes.Gold, x + 2, y + 74);
                if (nodeDep)
                    g.DrawString("FELLED", new Font("Consolas", 7f, FontStyle.Bold), Brushes.OrangeRed, x + 2, y + 90);
                else if (sel)
                    g.DrawString("SELECTED", new Font("Consolas", 7f, FontStyle.Bold), Brushes.LimeGreen, x + 2, y + 90);

                treeFont.Dispose();
                labelFont.Dispose();
            }
        }

        private void OnCanvasClick(object? s, MouseEventArgs e)
        {
            if (woodTypes.Count == 0) return;
            int cardW = Math.Max(110, (forestCanvas.Width - 20) / Math.Max(1, woodTypes.Count));
            int idx = (e.X - 10) / cardW;
            if (idx >= 0 && idx < woodTypes.Count) SelectTree(idx);
        }
    }

    private sealed class MiningWindow : Form
    {
        private readonly Form1 owner;
        private readonly string zone;
        private List<OreVeinNode> scannedNodes = new();
        private int selectedNodeIndex = -1;

        // UI
        private readonly Panel scanCanvas = new() { Width = 640, Height = 240, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(10, 10, 20) };
        private readonly Label scanHintLabel = new() { AutoSize = true, ForeColor = Color.LightGray, Text = "Click [Scan Zone] to detect ore veins." };
        private readonly TextBox logBox = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 8, 8), ForeColor = Color.Lime, Font = new Font("Consolas", 9f) };
        private readonly Button scanBtn  = new() { Text = "⬡ Scan Zone",  Width = 130, Height = 30, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        private readonly Button mineBtn  = new() { Text = "⛏ Mine Vein",  Width = 130, Height = 30, BackColor = Color.DimGray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };
        private readonly Button harvestBtn = new() { Text = "✿ Harvest",   Width = 110, Height = 30, BackColor = Color.DarkGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        private readonly Label capLabel  = new() { AutoSize = true, ForeColor = Color.Orange };
        private readonly Label toolInfoLabel = new() { AutoSize = false, Height = 44, Dock = DockStyle.Fill, ForeColor = Color.LightCyan, Font = new Font("Consolas", 8.5f), Padding = new Padding(6, 3, 0, 0), BackColor = Color.FromArgb(10, 20, 30) };
        private readonly Label nodeStatusLabel = new() { AutoSize = false, Height = 44, Dock = DockStyle.Fill, ForeColor = Color.Gold, Font = new Font("Consolas", 8.5f), Padding = new Padding(6, 3, 0, 0), BackColor = Color.FromArgb(20, 15, 5) };

        // Sub-zone travel
        private readonly ComboBox subZoneBox = new() { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Button travelBtn   = new() { Text = "Travel to Sub-Zone", Width = 140, Height = 28, BackColor = Color.Navy, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

        public MiningWindow(Form1 owner, string zone)
        {
            this.owner = owner;
            this.zone  = zone;

            Text = $"Resource Operations — {zone}";
            Size = new Size(720, 700);
            MinimumSize = new Size(600, 560);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 80, owner.Location.Y + 80);
            BackColor = Color.FromArgb(15, 15, 15);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 6, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));    // row 0: buttons
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); // row 1: tool info
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); // row 2: node status
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 255));// row 3: scan canvas
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));    // row 4: hint
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // row 5: log
            Controls.Add(root);

            // ── Row 0: Button strip ────────────────────────────────────────────
            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6) };
            btnFlow.Controls.Add(scanBtn);
            btnFlow.Controls.Add(mineBtn);
            btnFlow.Controls.Add(harvestBtn);
            btnFlow.Controls.Add(capLabel);
            btnFlow.Controls.Add(new Label { Text = "  SubZone:", AutoSize = true, ForeColor = Color.CornflowerBlue });
            btnFlow.Controls.Add(subZoneBox);
            btnFlow.Controls.Add(travelBtn);
            root.Controls.Add(btnFlow, 0, 0);

            // ── Row 1: Tool info ───────────────────────────────────────────────
            var toolGroup = new GroupBox { Text = "Equipped Gathering Tools", Dock = DockStyle.Fill, ForeColor = Color.LightCyan, Height = 52 };
            toolGroup.Controls.Add(toolInfoLabel);
            root.Controls.Add(toolGroup, 0, 1);

            // ── Row 2: Node status ─────────────────────────────────────────────
            var nodeGroup = new GroupBox { Text = "Node Depletion Status", Dock = DockStyle.Fill, ForeColor = Color.Gold, Height = 52 };
            nodeGroup.Controls.Add(nodeStatusLabel);
            root.Controls.Add(nodeGroup, 0, 2);

            // ── Row 3: Scan canvas ─────────────────────────────────────────────
            var canvasGroup = new GroupBox { Text = "Ore Scanner", Dock = DockStyle.Fill, ForeColor = Color.LimeGreen, Height = 260 };
            canvasGroup.Controls.Add(scanCanvas);
            scanCanvas.Dock = DockStyle.Fill;
            canvasGroup.Controls.Add(scanHintLabel);
            scanHintLabel.Dock = DockStyle.Top;
            root.Controls.Add(canvasGroup, 0, 3);

            // ── Row 4: hint ────────────────────────────────────────────────────
            var hintLabel = new Label { AutoSize = true, ForeColor = Color.Gray, Padding = new Padding(6, 2, 0, 0),
                Text = "Selected vein shown in yellow. Click a vein marker on the scanner to select it." };
            root.Controls.Add(hintLabel, 0, 4);

            // ── Row 5: Log ─────────────────────────────────────────────────────
            var logGroup = new GroupBox { Text = "Activity Log", Dock = DockStyle.Fill, ForeColor = Color.White };
            logGroup.Controls.Add(logBox);
            root.Controls.Add(logGroup, 0, 5);

            // ── Canvas drawing ─────────────────────────────────────────────────
            scanCanvas.Paint += OnScanCanvasPaint;
            scanCanvas.MouseClick += OnScanCanvasClick;

            // ── Button events ──────────────────────────────────────────────────
            scanBtn.Click += (_, _) => DoScan();
            mineBtn.Click += (_, _) => DoMine();
            harvestBtn.Click += (_, _) => DoHarvest();
            travelBtn.Click += (_, _) => DoSubZoneTravel();

            PopulateSubZoneBox();
            UpdateCapLabel();
            UpdateToolInfo();
            UpdateNodeStatus();
        }

        private void PopulateSubZoneBox()
        {
            subZoneBox.Items.Clear();
            if (owner.character is null) return;
            var destinations = owner.engine.GetSubZoneDestinations(owner.character.Location, zone);
            if (destinations.Count == 0) { subZoneBox.Items.Add("(none)"); travelBtn.Enabled = false; }
            else { foreach (var d in destinations) subZoneBox.Items.Add(d.Name); subZoneBox.SelectedIndex = 0; }
        }

        private void UpdateCapLabel()
        {
            if (owner.character is null) return;
            var cap = owner.engine.GetInventoryCapacity(owner.character);
            capLabel.Text = $"Inv: {owner.character.Inventory.Count}/{cap}";
            capLabel.ForeColor = owner.character.Inventory.Count >= cap ? Color.Red : Color.Orange;
        }

        private void UpdateToolInfo()
        {
            if (owner.character is null) return;
            toolInfoLabel.Text = owner.engine.GetGatheringToolStatus(owner.character);
        }

        private void UpdateNodeStatus()
        {
            if (owner.character is null) return;
            var (mDepleted, mLeft, mRespawn) = owner.engine.GetNodeStatus(owner.character.Location, zone, "mine");
            var (hDepleted, hLeft, hRespawn) = owner.engine.GetNodeStatus(owner.character.Location, zone, "harvest");

            string MineStr()   => mDepleted ? $"DEPLETED (respawn in {mRespawn} rot.)" : $"{mLeft}/3 uses left";
            string HarvestStr()=> hDepleted ? $"DEPLETED (respawn in {hRespawn} rot.)" : $"{hLeft}/5 uses left";

            nodeStatusLabel.Text = $"⛏ Mine: {MineStr()}   ✿ Harvest: {HarvestStr()}";
            nodeStatusLabel.ForeColor = (mDepleted || hDepleted) ? Color.OrangeRed : Color.Gold;
        }

        private void DoScan()
        {
            if (owner.character is null) return;
            scannedNodes = owner.engine.ScanMiningZone(owner.character, zone);
            selectedNodeIndex = -1;
            mineBtn.Enabled = false;
            scanCanvas.Invalidate();
            AppendLog($"Scan complete. {scannedNodes.Count} ore vein(s) detected in {zone}:");
            foreach (var n in scannedNodes)
            {
                var (dep, left, respawn) = owner.engine.GetVeinStatus(owner.character.Location, n.NodeId);
                var depStr = dep ? $" [DEPLETED — +{respawn} rot]" : $" [{left} use(s) left]";
                AppendLog($"  {n.Icon} {n.OreType}  — {n.ChancePercent}% yield  {(n.IsRare ? "[RARE]" : "")}  Vein size: {n.VeinSize}{depStr}");
            }
            UpdateNodeStatus();
        }

        private void DoMine()
        {
            if (owner.character is null) return;
            var toMine = selectedNodeIndex >= 0 && selectedNodeIndex < scannedNodes.Count
                ? new List<OreVeinNode> { scannedNodes[selectedNodeIndex] }
                : scannedNodes;
            var (ok, msg, obtained) = owner.engine.ExecuteMine(owner.character, zone, toMine);
            AppendLog(msg);
            if (ok)
            {
                owner.AppendLog($"⛏ Mined: {string.Join(", ", obtained)}");
                owner.RefreshStatus();
            }
            scanCanvas.Invalidate();  // refresh to show updated depletion colours
            UpdateCapLabel();
            UpdateNodeStatus();
            UpdateToolInfo();
        }

        private void DoHarvest()
        {
            if (owner.character is null) return;
            var (ok, msg, obtained) = owner.engine.HarvestZone(owner.character, zone);
            AppendLog(msg);
            if (ok)
            {
                owner.AppendLog($"✿ Harvested: {string.Join(", ", obtained)}");
                owner.RefreshStatus();
            }
            UpdateCapLabel();
            UpdateNodeStatus();
            UpdateToolInfo();
        }

        private void DoSubZoneTravel()
        {
            if (owner.character is null) return;
            var dest = subZoneBox.SelectedItem as string ?? "";
            if (dest == "(none)") return;
            var (ok, msg) = owner.engine.TravelToSubZone(owner.character, dest);
            AppendLog(msg);
            if (ok)
            {
                owner.AppendLog($"→ Arrived at {dest}");
                PopulateSubZoneBox();
                owner.RefreshStatus();
            }
        }

        private void OnScanCanvasPaint(object? s, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.FromArgb(10, 10, 20));
            if (scannedNodes.Count == 0)
            {
                g.DrawString("Scan the zone to reveal ore veins...", new Font("Consolas", 10f), Brushes.DimGray, 20, 100);
                return;
            }

            int w = scanCanvas.Width - 20;
            int h = scanCanvas.Height - 20;
            int cols = Math.Min(scannedNodes.Count, 6);
            int colW = w / Math.Max(1, cols);

            for (int i = 0; i < scannedNodes.Count; i++)
            {
                var node = scannedNodes[i];
                int x = 10 + (i % cols) * colW;
                int y = 10 + (i / cols) * 80;

                bool selected = i == selectedNodeIndex;
                var (vDep, vLeft, vRespawn) = owner.character is not null
                    ? owner.engine.GetVeinStatus(owner.character.Location, node.NodeId)
                    : (false, 3, 0);
                var boxColor = vDep ? Color.Gray :
                               selected ? Color.Yellow :
                               node.IsRare ? Color.MediumPurple : Color.SteelBlue;
                var boxBrush = new SolidBrush(Color.FromArgb(vDep ? 20 : 40, boxColor.R, boxColor.G, boxColor.B));
                g.FillRectangle(boxBrush, x, y, colW - 8, 72);
                g.DrawRectangle(new Pen(boxColor, selected ? 2 : 1), x, y, colW - 8, 72);

                var iconFont  = new Font("Segoe UI Symbol", 14f);
                var textFont  = new Font("Consolas", 7.5f);
                var iconColor = vDep ? Brushes.DimGray : node.IsRare ? Brushes.Gold : Brushes.LightGreen;
                g.DrawString(node.Icon, iconFont, iconColor, x + 4, y + 2);
                var nameColor = vDep ? Brushes.DimGray : Brushes.White;
                g.DrawString(node.OreType.Length > 18 ? node.OreType[..18] : node.OreType, textFont, nameColor, x + 2, y + 26);
                if (vDep)
                    g.DrawString($"DEPLETED +{vRespawn}rot", new Font("Consolas", 7f, FontStyle.Bold), Brushes.OrangeRed, x + 2, y + 44);
                else
                {
                    g.DrawString($"{node.ChancePercent}%  sz:{node.VeinSize}  [{vLeft}]", textFont, node.IsRare ? Brushes.Gold : Brushes.LightBlue, x + 2, y + 44);
                    if (node.IsRare) g.DrawString("RARE", new Font("Consolas", 7f, FontStyle.Bold), Brushes.Gold, x + 2, y + 58);
                }
                boxBrush.Dispose();
            }
        }

        private void OnScanCanvasClick(object? s, MouseEventArgs e)
        {
            if (scannedNodes.Count == 0) return;
            int w = scanCanvas.Width - 20;
            int cols = Math.Min(scannedNodes.Count, 6);
            int colW = w / Math.Max(1, cols);

            for (int i = 0; i < scannedNodes.Count; i++)
            {
                int x = 10 + (i % cols) * colW;
                int y = 10 + (i / cols) * 80;
                if (e.X >= x && e.X <= x + colW - 8 && e.Y >= y && e.Y <= y + 72)
                {
                    selectedNodeIndex = i;
                    mineBtn.Enabled = true;
                    mineBtn.Text = $"⛏ Mine: {scannedNodes[i].OreType[..Math.Min(12, scannedNodes[i].OreType.Length)]}";
                    scanCanvas.Invalidate();
                    AppendLog($"Selected vein: {scannedNodes[i].OreType} ({scannedNodes[i].ChancePercent}% yield)");
                    return;
                }
            }
            selectedNodeIndex = -1;
            mineBtn.Enabled = false;
            mineBtn.Text = "⛏ Mine Vein";
            scanCanvas.Invalidate();
        }

        private void AppendLog(string msg)
        {
            logBox.AppendText($"{msg}{Environment.NewLine}");
            logBox.ScrollToCaret();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ARMOR WINDOW  (browse / equip / craft)
    // ═══════════════════════════════════════════════════════════════════════════
    private sealed class ArmorWindow : Form
    {
        private readonly Form1 owner;
        private ArmorBlueprint? selected;

        private readonly ListBox armorList = new() { Width = 240, Dock = DockStyle.Fill, Font = new Font("Consolas", 9f) };
        private readonly TextBox statBox   = new() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Font = new Font("Consolas", 9f), ScrollBars = ScrollBars.Vertical, BackColor = Color.FromArgb(12, 12, 12), ForeColor = Color.Lime };
        private readonly TextBox recipeBox = new() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Font = new Font("Consolas", 9f), ScrollBars = ScrollBars.Vertical, BackColor = Color.FromArgb(12, 12, 12), ForeColor = Color.Orange };
        private readonly Button equipBtn  = new() { Text = "Equip Armor", Width = 120, Height = 30, BackColor = Color.DarkGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        private readonly Button craftBtn  = new() { Text = "Craft Armor",  Width = 120, Height = 30, BackColor = Color.DarkOliveGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        private readonly Button unequipBtn= new() { Text = "Unequip",      Width = 100, Height = 30, BackColor = Color.DimGray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        private readonly Label equippedLabel = new() { AutoSize = true, ForeColor = Color.Gold };
        private readonly ComboBox filterBox = new() { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };

        public ArmorWindow(Form1 owner)
        {
            this.owner = owner;
            Text = "Armor Catalog";
            Size = new Size(860, 620);
            MinimumSize = new Size(700, 500);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 100, owner.Location.Y + 60);
            BackColor = Color.FromArgb(14, 14, 14);

            // Filter
            filterBox.Items.AddRange(new object[] { "All", "light", "medium", "heavy", "exotic" });
            filterBox.SelectedItem = "All";

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            // Top strip
            var topFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6) };
            topFlow.Controls.Add(new Label { Text = "Filter:", AutoSize = true, ForeColor = Color.White });
            topFlow.Controls.Add(filterBox);
            topFlow.Controls.Add(equipBtn);
            topFlow.Controls.Add(craftBtn);
            topFlow.Controls.Add(unequipBtn);
            topFlow.Controls.Add(equippedLabel);
            root.SetColumnSpan(topFlow, 2);
            root.Controls.Add(topFlow, 0, 0);

            // Left: list
            var leftGroup = new GroupBox { Text = "Armors", Dock = DockStyle.Fill, ForeColor = Color.White };
            leftGroup.Controls.Add(armorList);
            root.Controls.Add(leftGroup, 0, 1);

            // Right: tabs
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var statsTab = new TabPage("Stats");   statsTab.Controls.Add(statBox);
            var recipeTab= new TabPage("Recipe");  recipeTab.Controls.Add(recipeBox);
            tabs.TabPages.AddRange(new[] { statsTab, recipeTab });
            root.Controls.Add(tabs, 1, 1);

            // Events
            filterBox.SelectedIndexChanged += (_, _) => RebuildList();
            armorList.SelectedIndexChanged += (_, _) => OnSelectArmor();
            equipBtn.Click  += (_, _) => OnEquip();
            craftBtn.Click  += (_, _) => OnCraft();
            unequipBtn.Click+= (_, _) => OnUnequip();

            RebuildList();
            UpdateEquippedLabel();
        }

        private void RebuildList()
        {
            armorList.Items.Clear();
            var filter = filterBox.SelectedItem as string ?? "All";
            foreach (var a in owner.engine.Armors.Values.OrderBy(a => a.Category).ThenBy(a => a.Name))
            {
                if (filter != "All" && !a.Category.Equals(filter, StringComparison.OrdinalIgnoreCase)) continue;
                armorList.Items.Add(a.Name);
            }
        }

        private void OnSelectArmor()
        {
            if (armorList.SelectedItem is not string name) return;
            if (!owner.engine.Armors.TryGetValue(name, out var armor)) return;
            selected = armor;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"═══ {armor.Name} ═══");
            sb.AppendLine($"Category : {armor.Category.ToUpper()}");
            sb.AppendLine(armor.Description);
            sb.AppendLine();
            sb.AppendLine($"Armor Rating  : {armor.ArmorRating}");
            sb.AppendLine($"HP Bonus      : +{armor.HpBonus}");
            sb.AppendLine($"Stamina Bonus : +{armor.StaminaBonus}");
            if (armor.MobilityPenalty > 0) sb.AppendLine($"Mobility Pen  : -{armor.MobilityPenalty}");
            sb.AppendLine();
            sb.AppendLine("─── Environmental ───");
            if (armor.HeatResistance)      sb.AppendLine("  ✓ Heat Resistance");
            if (armor.ColdResistance)      sb.AppendLine("  ✓ Cold Resistance");
            if (armor.AcidResistance)      sb.AppendLine("  ✓ Acid Resistance");
            if (armor.ToxinResistance)     sb.AppendLine("  ✓ Toxin Resistance");
            if (armor.VacuumSealed)        sb.AppendLine("  ✓ Vacuum Sealed");
            if (armor.RadiationShielded)   sb.AppendLine("  ✓ Radiation Shield");
            if (armor.WaterResistant)      sb.AppendLine("  ✓ Water Resistant");
            if (armor.StealthBonus)        sb.AppendLine("  ✓ Stealth Bonus");
            if (armor.LightsaberDampening) sb.AppendLine("  ✓ Lightsaber Dampening");
            if (armor.LifeSupport)         sb.AppendLine("  ✓ Full Life Support");
            sb.AppendLine();
            sb.AppendLine($"Value     : {armor.BaseValue} credits");
            sb.AppendLine($"Craftable : {(armor.Craftable ? "Yes" : "No — must be found")}");
            if (!string.IsNullOrWhiteSpace(armor.FactionRequired)) sb.AppendLine($"Requires  : {armor.FactionRequired}");
            if (!string.IsNullOrWhiteSpace(armor.EraAvailable) && armor.EraAvailable != "All") sb.AppendLine($"Era       : {armor.EraAvailable}");
            statBox.Text = sb.ToString();

            // Recipe
            if (owner.engine.ArmorRecipes.TryGetValue(name, out var recipe))
            {
                var rb = new System.Text.StringBuilder();
                rb.AppendLine($"─── Recipe for {name} ───");
                rb.AppendLine($"Credits required: {recipe.CreditCost}");
                if (recipe.RequiresForge) rb.AppendLine("Requires: Forge or Industrial Furnace");
                if (recipe.RequiresBlueprint) rb.AppendLine("Requires: Blueprint in inventory");
                rb.AppendLine();
                rb.AppendLine("Materials:");
                foreach (var (item, qty) in recipe.Materials) rb.AppendLine($"  {qty}x  {item}");

                // Check if player has materials
                if (owner.character != null)
                {
                    rb.AppendLine();
                    rb.AppendLine("Your stock:");
                    foreach (var (item, qty) in recipe.Materials)
                    {
                        var have = owner.character.Inventory.Count(x => x.Equals(item, StringComparison.OrdinalIgnoreCase));
                        var mark = have >= qty ? "✓" : "✗";
                        rb.AppendLine($"  {mark} {item}: {have}/{qty}");
                    }
                }
                recipeBox.Text = rb.ToString();
            }
            else recipeBox.Text = armor.Craftable ? "No recipe data available." : "This armor cannot be crafted.";

            equipBtn.Enabled  = owner.character?.Inventory.Contains(name) ?? false;
            craftBtn.Enabled  = armor.Craftable && owner.engine.ArmorRecipes.ContainsKey(name);
        }

        private void OnEquip()
        {
            if (selected == null || owner.character == null) return;
            var msg = owner.engine.EquipArmor(owner.character, selected.Name);
            owner.AppendLog(msg);
            UpdateEquippedLabel();
            owner.RefreshStatus();
        }

        private void OnCraft()
        {
            if (selected == null || owner.character == null) return;
            var (ok, msg) = owner.engine.CraftArmor(owner.character, selected.Name);
            owner.AppendLog(msg);
            if (ok) { equipBtn.Enabled = true; OnSelectArmor(); }
            owner.RefreshStatus();
        }

        private void OnUnequip()
        {
            if (owner.character == null) return;
            var msg = owner.engine.UnequipArmor(owner.character);
            owner.AppendLog(msg);
            UpdateEquippedLabel();
            owner.RefreshStatus();
        }

        private void UpdateEquippedLabel()
        {
            var eq = owner.character?.EquippedArmor;
            equippedLabel.Text = string.IsNullOrWhiteSpace(eq) ? "None equipped" : $"Equipped: {eq}";
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REFINERY WINDOW  (refine raw materials)
    // ═══════════════════════════════════════════════════════════════════════════
    private sealed class RefineryWindow : Form
    {
        private readonly Form1 owner;

        private readonly ListBox rawList   = new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 10f) };
        private readonly Label infoLabel   = new() { AutoSize = true, ForeColor = Color.Orange };
        private readonly NumericUpDown qtyBox = new() { Minimum = 1, Maximum = 999, Value = 1, Width = 60 };
        private readonly Button refineBtn  = new() { Text = "Refine", Width = 100, Height = 30, BackColor = Color.DarkOrange, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat };
        private readonly TextBox logBox    = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, BackColor = Color.Black, ForeColor = Color.Lime, Font = new Font("Consolas", 9f) };
        private readonly Label locLabel    = new() { AutoSize = true, ForeColor = Color.LightGray };

        public RefineryWindow(Form1 owner)
        {
            this.owner = owner;
            Text = "Refinery";
            Size = new Size(660, 520);
            MinimumSize = new Size(520, 400);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 160, owner.Location.Y + 120);
            BackColor = Color.FromArgb(14, 14, 14);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            Controls.Add(root);

            // Row 0: location info + controls
            var ctrlFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6) };
            ctrlFlow.Controls.Add(locLabel);
            ctrlFlow.Controls.Add(infoLabel);
            ctrlFlow.Controls.Add(new Label { Text = "Qty:", AutoSize = true, ForeColor = Color.White });
            ctrlFlow.Controls.Add(qtyBox);
            ctrlFlow.Controls.Add(refineBtn);
            root.Controls.Add(ctrlFlow, 0, 0);

            // Row 1: raw material list
            var rawGroup = new GroupBox { Text = "Refinable at this Location", Dock = DockStyle.Fill, ForeColor = Color.Orange };
            rawGroup.Controls.Add(rawList);
            root.Controls.Add(rawGroup, 0, 1);

            // Row 2: info for selected
            root.Controls.Add(infoLabel, 0, 2);

            // Row 3: log
            var logGroup = new GroupBox { Text = "Refinery Log", Dock = DockStyle.Fill, ForeColor = Color.White };
            logGroup.Controls.Add(logBox);
            root.Controls.Add(logGroup, 0, 3);

            // Events
            rawList.SelectedIndexChanged += (_, _) => OnSelectRaw();
            refineBtn.Click += (_, _) => OnRefine();

            RebuildList();
        }

        private void RebuildList()
        {
            rawList.Items.Clear();
            if (owner.character == null) return;
            locLabel.Text = $"Location: {owner.character.Location}  |  ";
            var refinable = owner.engine.GetRefinableAtLocation(owner.character);
            foreach (var raw in refinable) rawList.Items.Add(raw);
            if (refinable.Count == 0)
                rawList.Items.Add("(nothing to refine here)");
            refineBtn.Enabled = refinable.Count > 0;
        }

        private void OnSelectRaw()
        {
            if (rawList.SelectedItem is not string raw) return;
            if (!owner.engine.RefiningRecipes.TryGetValue(raw, out var rec)) return;
            var have = owner.character?.Inventory.Count(x => x.Equals(raw, StringComparison.OrdinalIgnoreCase)) ?? 0;
            infoLabel.Text = $"{raw}  →  {rec.Quantity}x {rec.RefinedOutput}  |  Have: {have}  |  Cost: {rec.CreditCost}c/batch  |  Time: {rec.TimeCost} rotation(s)";
            qtyBox.Maximum = Math.Max(1, have);
        }

        private void OnRefine()
        {
            if (rawList.SelectedItem is not string raw || owner.character == null) return;
            var (ok, msg) = owner.engine.RefineRawMaterial(owner.character, raw, (int)qtyBox.Value);
            logBox.AppendText($"{msg}{Environment.NewLine}");
            logBox.ScrollToCaret();
            if (ok) { owner.AppendLog(msg); RebuildList(); owner.RefreshStatus(); }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // QUEST ENCOUNTER POPUP
    // ═══════════════════════════════════════════════════════════════════════════
    private sealed class QuestEncounterPopup : Form
    {
        public QuestEncounterPopup(Form1 owner, QuestEncounterEvent evt)
        {
            Text = $"New Contract: {evt.Quest.Title}";
            Size = new Size(540, 360);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 140, owner.Location.Y + 140);
            BackColor = Color.FromArgb(12, 12, 20);

            var root = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(14), WrapContents = false };
            Controls.Add(root);

            var titleL = new Label { Text = $"★  {evt.Quest.Title}", AutoSize = true, ForeColor = Color.Gold, Font = new Font("Segoe UI", 13f, FontStyle.Bold) };
            root.Controls.Add(titleL);

            var descL = new Label { Text = evt.Quest.Description, AutoSize = true, MaximumSize = new Size(480, 120), ForeColor = Color.LightGray };
            root.Controls.Add(descL);

            var sep = new Label { Text = new string('─', 60), AutoSize = true, ForeColor = Color.DimGray };
            root.Controls.Add(sep);

            var hintL = new Label { Text = $"Intel: {evt.HintDialogue}", AutoSize = true, MaximumSize = new Size(480, 80), ForeColor = Color.LightSteelBlue };
            root.Controls.Add(hintL);

            var rewardL = new Label
            {
                Text = $"Reward: {evt.Quest.RewardCredits} credits  |  {evt.Quest.RewardXp} XP  " +
                       (string.IsNullOrWhiteSpace(evt.Quest.RewardItem) ? "" : $"|  Item: {evt.Quest.RewardItem}"),
                AutoSize = true, ForeColor = Color.LightGreen
            };
            root.Controls.Add(rewardL);

            if (evt.SpawnsCombat)
            {
                var warnL = new Label { Text = "⚠ This contract may involve combat.", AutoSize = true, ForeColor = Color.OrangeRed };
                root.Controls.Add(warnL);
            }

            var closeBtn = new Button { Text = "Accept & Close", Width = 130, Height = 32, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            closeBtn.Click += (_, _) => Close();
            root.Controls.Add(closeBtn);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ARMOR EQUIP WINDOW  —  slot-based mix-and-match armor UI
    // ═══════════════════════════════════════════════════════════════════════════
    private sealed class ArmorEquipWindow : Form
    {
        private readonly Form1 owner;
        private static readonly string[] Slots = { "Helmet", "Chest", "Arms", "Legs", "Boots", "Belt" };
        private readonly Dictionary<string, Label>  slotEquippedLabels = new();
        private readonly Dictionary<string, Label>  slotArLabel        = new();
        private readonly Label totalArLabel    = new() { AutoSize = true, ForeColor = Color.Gold, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
        private readonly Label totalHpLabel    = new() { AutoSize = true, ForeColor = Color.LightGreen };
        private readonly Label totalStamLabel  = new() { AutoSize = true, ForeColor = Color.LightSkyBlue };
        private readonly Label backpackLabel   = new() { AutoSize = true, ForeColor = Color.Plum };
        private readonly Label envLabel        = new() { AutoSize = true, ForeColor = Color.LightCoral, MaximumSize = new Size(260, 100) };

        public ArmorEquipWindow(Form1 owner)
        {
            this.owner = owner;
            Text = "Equip Armor";
            Size = new Size(820, 620);
            MinimumSize = new Size(820, 560);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 60, owner.Location.Y + 40);
            BackColor = Color.FromArgb(14, 14, 24);

            var mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                Padding = new Padding(10)
            };
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            Controls.Add(mainTable);

            // ── Left: slot grid ───────────────────────────────────────────────
            var leftPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, Padding = new Padding(4)
            };
            mainTable.Controls.Add(leftPanel, 0, 0);

            foreach (var slot in Slots)
            {
                var gb = new GroupBox
                {
                    Text = slot, ForeColor = Color.LightGray, BackColor = Color.FromArgb(20, 20, 34),
                    Width = 440, Height = 80, Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                };
                var eqLbl = new Label { Left = 8, Top = 22, AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 9f) };
                var arLbl = new Label { Left = 8, Top = 44, AutoSize = true, ForeColor = Color.Gold, Font = new Font("Segoe UI", 8f) };
                var changeBtn = new Button
                {
                    Text = "Change", Width = 72, Height = 26,
                    Left = gb.Width - 160, Top = 22,
                    BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                    Tag = slot
                };
                var unequipBtn = new Button
                {
                    Text = "Remove", Width = 72, Height = 26,
                    Left = gb.Width - 82, Top = 22,
                    BackColor = Color.FromArgb(80, 20, 20), ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                    Tag = slot
                };
                changeBtn.Click  += OnChangeSlot;
                unequipBtn.Click += OnUnequipSlot;
                gb.Controls.AddRange(new Control[] { eqLbl, arLbl, changeBtn, unequipBtn });
                slotEquippedLabels[slot] = eqLbl;
                slotArLabel[slot]        = arLbl;
                leftPanel.Controls.Add(gb);
            }

            // Backpack section
            var bpGb = new GroupBox
            {
                Text = "Backpack", ForeColor = Color.Plum, BackColor = Color.FromArgb(20, 20, 34),
                Width = 440, Height = 80, Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            backpackLabel.Location = new Point(8, 24);
            var bpChange = new Button { Text = "Change", Width = 72, Height = 26, Left = bpGb.Width - 160, Top = 24, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var bpRemove = new Button { Text = "Remove", Width = 72, Height = 26, Left = bpGb.Width - 82, Top = 24, BackColor = Color.FromArgb(80, 20, 20), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            bpChange.Click += OnChangeBackpack;
            bpRemove.Click += OnRemoveBackpack;
            bpGb.Controls.AddRange(new Control[] { backpackLabel, bpChange, bpRemove });
            leftPanel.Controls.Add(bpGb);

            var removeAllBtn = new Button { Text = "Remove All Armor", Width = 150, Height = 32, BackColor = Color.FromArgb(80, 20, 20), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            removeAllBtn.Click += (_, _) =>
            {
                if (owner.character == null) return;
                owner.engine.UnequipAllArmor(owner.character);
                owner.RefreshStatus();
                RefreshDisplay();
            };
            leftPanel.Controls.Add(removeAllBtn);

            // ── Right: summary panel ──────────────────────────────────────────
            var rightPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, Padding = new Padding(8)
            };
            mainTable.Controls.Add(rightPanel, 1, 0);

            rightPanel.Controls.Add(new Label { Text = "── Totals ──", AutoSize = true, ForeColor = Color.DimGray });
            rightPanel.Controls.Add(totalArLabel);
            rightPanel.Controls.Add(totalHpLabel);
            rightPanel.Controls.Add(totalStamLabel);
            rightPanel.Controls.Add(new Label { Text = "── Protections ──", AutoSize = true, ForeColor = Color.DimGray });
            rightPanel.Controls.Add(envLabel);
            rightPanel.Controls.Add(new Label { Text = "── Backpack Capacity ──", AutoSize = true, ForeColor = Color.DimGray });
            rightPanel.Controls.Add(backpackLabel);

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (owner.character == null) return;
            var ch = owner.character;

            foreach (var slot in Slots)
            {
                ch.EquippedArmorPieces.TryGetValue(slot, out var equipped);
                // Also check Full Suit for display in all slot rows
                if (string.IsNullOrEmpty(equipped) && ch.EquippedArmorPieces.TryGetValue("Full Suit", out var fs))
                    equipped = $"{fs} (Full Suit)";
                slotEquippedLabels[slot].Text = string.IsNullOrEmpty(equipped) ? "— none —" : equipped;
                if (!string.IsNullOrEmpty(equipped) && owner.engine.Armors.TryGetValue(equipped.Replace(" (Full Suit)", ""), out var a))
                    slotArLabel[slot].Text = $"AR +{a.ArmorRating}  HP +{a.HpBonus}  Stam +{a.StaminaBonus}";
                else
                    slotArLabel[slot].Text = "";
            }

            totalArLabel.Text   = $"Total AR: {ch.Armor}";
            totalHpLabel.Text   = $"Max HP: {ch.MaxHp}";
            totalStamLabel.Text = $"Max Stamina: {ch.MaxStamina}";

            // Env protections from equipped pieces
            var envSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in ch.EquippedArmorPieces.Values.Where(v => !string.IsNullOrEmpty(v)))
                if (owner.engine.Armors.TryGetValue(name, out var a))
                {
                    if (a.HeatResistance)       envSet.Add("Heat");
                    if (a.ColdResistance)       envSet.Add("Cold");
                    if (a.AcidResistance)       envSet.Add("Acid");
                    if (a.ToxinResistance)      envSet.Add("Toxin");
                    if (a.VacuumSealed)         envSet.Add("Vacuum");
                    if (a.RadiationShielded)    envSet.Add("Radiation");
                    if (a.WaterResistant)       envSet.Add("Water");
                    if (a.LightsaberDampening)  envSet.Add("Saber-damp");
                    if (a.StealthBonus)         envSet.Add("Stealth");
                    if (a.LifeSupport)          envSet.Add("Life Support");
                }
            envLabel.Text = envSet.Count > 0 ? string.Join(", ", envSet) : "None";

            if (!string.IsNullOrEmpty(ch.EquippedBackpack) && owner.engine.Backpacks.TryGetValue(ch.EquippedBackpack, out var bp))
                backpackLabel.Text = $"{ch.EquippedBackpack}  (+{ScuConversion.FormatMicroScu(bp.CapacityMicroScu)})";
            else
                backpackLabel.Text = "— none —";
        }

        private void OnChangeSlot(object? sender, EventArgs e)
        {
            if (owner.character == null || sender is not Button btn || btn.Tag is not string slot) return;
            var ch = owner.character;

            // Gather inventory items matching this slot
            var available = ch.Inventory
                .Where(item => owner.engine.Armors.TryGetValue(item, out var a) && a.Slot == slot)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            if (available.Count == 0) { MessageBox.Show($"No {slot} pieces in your inventory.", "Nothing to Equip", MessageBoxButtons.OK); return; }

            using var picker = new Form
            {
                Text = $"Choose {slot}", Width = 360, Height = 400,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(Location.X + 100, Location.Y + 100),
                BackColor = Color.FromArgb(14, 14, 24), FormBorderStyle = FormBorderStyle.FixedDialog
            };
            var lb = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 20, 34), ForeColor = Color.White, Font = new Font("Segoe UI", 9f) };
            lb.Items.AddRange(available.ToArray<object>());
            var info = new Label { Dock = DockStyle.Bottom, AutoSize = false, Height = 60, ForeColor = Color.LightGray, BackColor = Color.FromArgb(12, 12, 20), Text = "Select a piece to see details." };
            lb.SelectedIndexChanged += (_, _) =>
            {
                if (lb.SelectedItem is string n && owner.engine.Armors.TryGetValue(n, out var a))
                    info.Text = $"AR: {a.ArmorRating}  HP: {a.HpBonus}  Stam: {a.StaminaBonus}\n{a.Description}";
            };
            var okBtn = new Button { Text = "Equip", Dock = DockStyle.Bottom, Height = 34, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            okBtn.Click += (_, _) =>
            {
                if (lb.SelectedItem is string name)
                {
                    var msg = owner.engine.EquipArmorPiece(ch, name);
                    owner.AppendLog(msg);
                    owner.RefreshStatus();
                    RefreshDisplay();
                }
                picker.Close();
            };
            picker.Controls.AddRange(new Control[] { lb, info, okBtn });
            picker.ShowDialog(this);
        }

        private void OnUnequipSlot(object? sender, EventArgs e)
        {
            if (owner.character == null || sender is not Button btn || btn.Tag is not string slot) return;
            var msg = owner.engine.UnequipArmorSlot(owner.character, slot);
            owner.AppendLog(msg);
            owner.RefreshStatus();
            RefreshDisplay();
        }

        private void OnChangeBackpack(object? sender, EventArgs e)
        {
            if (owner.character == null) return;
            var ch = owner.character;
            var available = ch.Inventory
                .Where(item => owner.engine.Backpacks.ContainsKey(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
            if (available.Count == 0) { MessageBox.Show("No backpacks in your inventory.", "Nothing to Equip", MessageBoxButtons.OK); return; }

            using var picker = new Form
            {
                Text = "Choose Backpack", Width = 360, Height = 360,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(Location.X + 100, Location.Y + 150),
                BackColor = Color.FromArgb(14, 14, 24), FormBorderStyle = FormBorderStyle.FixedDialog
            };
            var lb = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 20, 34), ForeColor = Color.White, Font = new Font("Segoe UI", 9f) };
            lb.Items.AddRange(available.ToArray<object>());
            var okBtn = new Button { Text = "Attach", Dock = DockStyle.Bottom, Height = 34, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            okBtn.Click += (_, _) =>
            {
                if (lb.SelectedItem is string name)
                {
                    var msg = owner.engine.EquipBackpack(ch, name);
                    owner.AppendLog(msg);
                    owner.RefreshStatus();
                    RefreshDisplay();
                }
                picker.Close();
            };
            picker.Controls.AddRange(new Control[] { lb, okBtn });
            picker.ShowDialog(this);
        }

        private void OnRemoveBackpack(object? sender, EventArgs e)
        {
            if (owner.character == null) return;
            var msg = owner.engine.UnequipBackpack(owner.character);
            owner.AppendLog(msg);
            owner.RefreshStatus();
            RefreshDisplay();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INVENTORY MANAGER WINDOW  —  transfer items between all storages
    // ═══════════════════════════════════════════════════════════════════════════
    private sealed class InventoryManagerWindow : Form
    {
        private readonly Form1 owner;
        private readonly ComboBox srcCombo  = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        private readonly ComboBox dstCombo  = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        private readonly ListBox  srcList   = new() { SelectionMode = SelectionMode.One };
        private readonly ListBox  dstList   = new() { SelectionMode = SelectionMode.One };
        private readonly Label    srcCapLbl = new() { AutoSize = true, ForeColor = Color.LightGray };
        private readonly Label    dstCapLbl = new() { AutoSize = true, ForeColor = Color.LightGray };
        private readonly Label    logLbl    = new() { AutoSize = true, ForeColor = Color.LightYellow };

        private static readonly string[] StorageTypes = { "Character Bag", "Ship Locker", "Ship Cargo", "Home Storage" };

        public InventoryManagerWindow(Form1 owner)
        {
            this.owner = owner;
            Text = "Inventory Manager";
            Size = new Size(780, 560);
            MinimumSize = new Size(700, 480);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(owner.Location.X + 80, owner.Location.Y + 60);
            BackColor = Color.FromArgb(14, 14, 24);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(8) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(root);

            // Source panel
            var srcPanel = BuildStoragePanel("Source", srcCombo, srcList, srcCapLbl);
            root.Controls.Add(srcPanel, 0, 0);

            // Middle buttons
            var midPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
                Padding = new Padding(4), BackColor = Color.FromArgb(14, 14, 24)
            };
            var transferBtn = new Button { Text = "→ Transfer", Width = 82, Height = 36, BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var discardBtn  = new Button { Text = "✕ Discard",  Width = 82, Height = 36, BackColor = Color.FromArgb(80, 20, 20), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 6, 0, 0) };
            transferBtn.Click += OnTransfer;
            discardBtn.Click  += OnDiscard;
            midPanel.Controls.AddRange(new Control[] { transferBtn, discardBtn, logLbl });
            root.Controls.Add(midPanel, 1, 0);

            // Dest panel
            var dstPanel = BuildStoragePanel("Destination", dstCombo, dstList, dstCapLbl);
            root.Controls.Add(dstPanel, 2, 0);

            // Wire combos
            srcCombo.SelectedIndexChanged += (_, _) => RefreshStorageList(srcCombo, srcList, srcCapLbl);
            dstCombo.SelectedIndexChanged += (_, _) => RefreshStorageList(dstCombo, dstList, dstCapLbl);
            srcCombo.Items.AddRange(StorageTypes);
            dstCombo.Items.AddRange(StorageTypes);
            srcCombo.SelectedIndex = 0;
            dstCombo.SelectedIndex = 1;
        }

        private Panel BuildStoragePanel(string header, ComboBox combo, ListBox lb, Label capLbl)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(14, 14, 24) };
            var headerLbl = new Label { Text = header, AutoSize = true, ForeColor = Color.Gold, Font = new Font("Segoe UI", 10f, FontStyle.Bold), Left = 0, Top = 0 };
            combo.Location = new Point(0, 24);
            capLbl.Location = new Point(0, 52);
            lb.BackColor = Color.FromArgb(20, 20, 34);
            lb.ForeColor = Color.White;
            lb.Font = new Font("Segoe UI", 9f);
            lb.Bounds = new Rectangle(0, 70, 300, 380);
            lb.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            p.Controls.AddRange(new Control[] { headerLbl, combo, capLbl, lb });
            return p;
        }

        private List<string> GetStorageItems(string storageType)
        {
            if (owner.character == null) return new();
            return storageType switch
            {
                "Character Bag" => owner.character.Inventory,
                "Ship Locker"   => owner.character.Ship?.PersonalStorageItems ?? new(),
                "Ship Cargo"    => owner.character.Ship?.CargoItems ?? new(),
                "Home Storage"  => owner.character.Home?.StorageItems ?? new(),
                _ => new()
            };
        }

        private string GetCapacityLabel(string storageType)
        {
            if (owner.character == null) return "";
            var ch = owner.character;
            return storageType switch
            {
                "Character Bag" => $"{ScuConversion.FormatMicroScu(owner.engine.GetInventoryUsedMicroScu(ch))} / {ScuConversion.FormatMicroScu(owner.engine.GetInventoryCapacityMicroScu(ch))}",
                "Ship Locker"   => ch.Ship == null ? "No ship" :
                    $"{ScuConversion.FormatMicroScu(owner.engine.GetShipPersonalStorageUsed(ch.Ship))} / {ScuConversion.FormatMicroScu(owner.engine.GetShipPersonalStorageCapacity(ch.Ship))}",
                "Ship Cargo"    => ch.Ship == null ? "No ship" :
                    $"{owner.engine.GetCargoUsedScu(ch.Ship)} / {ch.Ship.CargoCapacity} SCU",
                "Home Storage"  => ch.Home == null ? "No home" :
                    $"{ch.Home.StorageItems.Count} / {ch.Home.StorageCapacityScu} SCU",
                _ => ""
            };
        }

        private void RefreshStorageList(ComboBox combo, ListBox lb, Label capLbl)
        {
            if (combo.SelectedItem is not string st) return;
            lb.Items.Clear();
            var items = GetStorageItems(st);
            foreach (var i in items)
            {
                var size = owner.engine.GetItemMicroScuSize(i);
                lb.Items.Add($"{i}  [{ScuConversion.FormatMicroScu(size)}]");
            }
            capLbl.Text = GetCapacityLabel(st);
        }

        private string? SelectedItemName(ListBox lb, string storageType)
        {
            if (lb.SelectedIndex < 0) return null;
            var items = GetStorageItems(storageType);
            if (lb.SelectedIndex >= items.Count) return null;
            return items[lb.SelectedIndex];
        }

        private void OnTransfer(object? sender, EventArgs e)
        {
            if (owner.character == null) return;
            var srcType = srcCombo.SelectedItem as string;
            var dstType = dstCombo.SelectedItem as string;
            if (srcType == null || dstType == null) return;
            var itemName = SelectedItemName(srcList, srcType);
            if (itemName == null) { logLbl.Text = "Select an item."; return; }

            var ch = owner.character;
            (bool ok, string msg) result = (srcType, dstType) switch
            {
                ("Character Bag", "Ship Locker") => owner.engine.TransferToShipPersonal(ch, itemName),
                ("Character Bag", "Ship Cargo")  => owner.engine.TransferToShipCargo(ch, itemName),
                ("Character Bag", "Home Storage")=> owner.engine.TransferToHome(ch, itemName),
                ("Ship Locker",   "Character Bag")=> owner.engine.TransferFromShipPersonal(ch, itemName),
                ("Ship Cargo",    "Character Bag")=> owner.engine.TransferFromShipCargo(ch, itemName),
                ("Home Storage",  "Character Bag")=> owner.engine.TransferFromHome(ch, itemName),
                _ => (false, "Direct transfer between non-bag storages: move to bag first.")
            };
            logLbl.Text = result.msg;
            if (result.ok) owner.RefreshStatus();
            RefreshStorageList(srcCombo, srcList, srcCapLbl);
            RefreshStorageList(dstCombo, dstList, dstCapLbl);
        }

        private void OnDiscard(object? sender, EventArgs e)
        {
            if (owner.character == null) return;
            var srcType = srcCombo.SelectedItem as string;
            if (srcType == null) return;
            var itemName = SelectedItemName(srcList, srcType);
            if (itemName == null) { logLbl.Text = "Select an item to discard."; return; }
            if (MessageBox.Show($"Discard '{itemName}'?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            var items = GetStorageItems(srcType);
            var idx = items.FindIndex(x => x.Equals(itemName, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) items.RemoveAt(idx);
            logLbl.Text = $"Discarded '{itemName}'.";
            owner.RefreshStatus();
            RefreshStorageList(srcCombo, srcList, srcCapLbl);
        }
    }

}
