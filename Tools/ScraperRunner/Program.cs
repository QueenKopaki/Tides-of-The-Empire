using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

internal class Program
{
    private static readonly HttpClient http = new HttpClient();
    private const string ApiBase = "https://starwars.fandom.com/api.php";

    private sealed class RecipeEntry
    {
        public string Name { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Materials { get; set; } = new();
    }

    private sealed class RaceEntry
    {
        public string Name { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private sealed class CreatureEntry
    {
        public string Name { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SizeClass { get; set; } = "medium";
        public string Habitat { get; set; } = "general";
        public int DangerRating { get; set; } = 4;
    }

    private sealed class PlanetEntry
    {
        public string Name { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
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

    private sealed class ShipArmamentEntry
    {
        public string Name { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string Category { get; set; } = "laser";
        public string HardpointSize { get; set; } = "S";
        public string Era { get; set; } = "Old Republic";
        public int DamageRating { get; set; } = 8;
        public int FuelDraw { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    static Program()
    {
        http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        http.DefaultRequestHeaders.Accept.ParseAdd("application/json,text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
    }

    static async Task<int> Main(string[] args)
    {
        if (args.Length > 0 && string.Equals(args[0], "creatures", StringComparison.OrdinalIgnoreCase))
        {
            var creatureMax = 3000;
            var creatureDelay = 30;
            if (args.Length > 1 && int.TryParse(args[1], out var parsedCreatureMax) && parsedCreatureMax > 0) creatureMax = parsedCreatureMax;
            if (args.Length > 2 && int.TryParse(args[2], out var parsedCreatureDelay) && parsedCreatureDelay >= 0) creatureDelay = parsedCreatureDelay;
            return await CrawlCreaturesAsync(creatureMax, creatureDelay);
        }

        if (args.Length > 0 && string.Equals(args[0], "races", StringComparison.OrdinalIgnoreCase))
        {
            var raceMax = 3000;
            var raceDelay = 40;
            if (args.Length > 1 && int.TryParse(args[1], out var parsedRaceMax) && parsedRaceMax > 0) raceMax = parsedRaceMax;
            if (args.Length > 2 && int.TryParse(args[2], out var parsedRaceDelay) && parsedRaceDelay >= 0) raceDelay = parsedRaceDelay;
            return await CrawlRacesAsync(raceMax, raceDelay);
        }

        if (args.Length > 0 && string.Equals(args[0], "planets", StringComparison.OrdinalIgnoreCase))
        {
            var planetMax = 5000;
            var planetDelay = 40;
            if (args.Length > 1 && int.TryParse(args[1], out var parsedPlanetMax) && parsedPlanetMax > 0) planetMax = parsedPlanetMax;
            if (args.Length > 2 && int.TryParse(args[2], out var parsedPlanetDelay) && parsedPlanetDelay >= 0) planetDelay = parsedPlanetDelay;
            return await CrawlPlanetsAsync(planetMax, planetDelay);
        }

        if (args.Length > 0 && string.Equals(args[0], "armaments", StringComparison.OrdinalIgnoreCase))
        {
            var armamentMax = 1200;
            var armamentDelay = 40;
            if (args.Length > 1 && int.TryParse(args[1], out var parsedArmamentMax) && parsedArmamentMax > 0) armamentMax = parsedArmamentMax;
            if (args.Length > 2 && int.TryParse(args[2], out var parsedArmamentDelay) && parsedArmamentDelay >= 0) armamentDelay = parsedArmamentDelay;
            return await CrawlShipArmamentsAsync(armamentMax, armamentDelay);
        }

        var maxPages = 2000;
        var delayMs = 300;
        if (args.Length > 0 && int.TryParse(args[0], out var parsed) && parsed > 0) maxPages = parsed;
        if (args.Length > 1 && int.TryParse(args[1], out var parsedDelay) && parsedDelay >= 0) delayMs = parsedDelay;

        var categories = new[]
        {
            "Category:Items",
            "Category:Weapons",
            "Category:Vehicles",
            "Category:Starships",
            "Category:Materials",
            "Category:Starfighters"
        };

        var entries = new List<RecipeEntry>();
        var seenTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in categories)
        {
            Console.WriteLine($"Enumerating {category}...");
            var titles = await GetCategoryMembersAsync(category);
            Console.WriteLine($"Found {titles.Count} members in {category}.");

            foreach (var title in titles)
            {
                if (entries.Count >= maxPages) break;
                if (!seenTitles.Add(title)) continue;

                var recipe = await ParseRecipeFromPageAsync(title, category, delayMs);
                if (recipe is not null)
                {
                    entries.Add(recipe);
                    if (entries.Count % 100 == 0)
                    {
                        Console.WriteLine($"Progress: {entries.Count}/{maxPages} entries...");
                    }
                }
            }
            if (entries.Count >= maxPages) break;
        }

        // Fallback discovery: crawl allpages and classify by title keywords.
        if (entries.Count < maxPages)
        {
            var needed = maxPages - entries.Count;
            Console.WriteLine($"Category crawl sparse, discovering up to {needed} additional pages via allpages...");
            var discovered = await DiscoverCandidatePagesAsync(Math.Max(needed * 3, 1500));
            foreach (var title in discovered)
            {
                if (entries.Count >= maxPages) break;
                if (!seenTitles.Add(title)) continue;
                var recipe = await ParseRecipeFromPageAsync(title, "Discovered", delayMs);
                if (recipe is not null)
                {
                    entries.Add(recipe);
                    if (entries.Count % 100 == 0)
                    {
                        Console.WriteLine($"Discovery progress: {entries.Count}/{maxPages} entries...");
                    }
                }
            }
        }

        var outDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "StarWarsRpgCs", "data");
        System.IO.Directory.CreateDirectory(outDir);
        var outPath = System.IO.Path.Combine(outDir, "recipes_output.json");
        var json = System.Text.Json.JsonSerializer.Serialize(entries, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        await System.IO.File.WriteAllTextAsync(outPath, json);

        Console.WriteLine($"Saved {entries.Count} scraped recipe stubs to {outPath}");
        Console.WriteLine("Args: [maxPages] [delayMs]   e.g. dotnet run --project ... 5000 300");
        return 0;
    }

    private static async Task<int> CrawlCreaturesAsync(int maxPages, int delayMs)
    {
        var creatureCategories = new[]
        {
            "Category:Creatures",
            "Category:Fauna",
            "Category:Beasts",
            "Category:Monsters",
            "Category:Megafauna",
            "Category:Space_fauna"
        };

        var entries = new List<CreatureEntry>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in creatureCategories)
        {
            Console.WriteLine($"Enumerating {category}...");
            var titles = await GetCategoryMembersAsync(category);
            Console.WriteLine($"Found {titles.Count} members in {category}.");

            foreach (var title in titles)
            {
                if (entries.Count >= maxPages) break;
                var clean = NormalizeCreatureName(title);
                if (string.IsNullOrWhiteSpace(clean)) continue;
                if (!seen.Add(clean)) continue;

                var parsed = await ParseCreatureFromPageAsync(title, clean, category, delayMs);
                if (parsed is null) continue;
                entries.Add(parsed);
                if (entries.Count % 100 == 0) Console.WriteLine($"Creature progress: {entries.Count}/{maxPages}...");
            }

            if (entries.Count >= maxPages) break;
        }

        if (entries.Count < maxPages)
        {
            var needed = maxPages - entries.Count;
            Console.WriteLine($"Creature categories sparse, discovering up to {needed} additional pages...");
            var discovered = await DiscoverCreaturePagesAsync(Math.Max(needed * 2, 400));
            foreach (var title in discovered)
            {
                if (entries.Count >= maxPages) break;
                var clean = NormalizeCreatureName(title);
                if (string.IsNullOrWhiteSpace(clean)) continue;
                if (!seen.Add(clean)) continue;

                var parsed = await ParseCreatureFromPageAsync(title, clean, "Discovered", delayMs);
                if (parsed is null) continue;
                entries.Add(parsed);
                if (entries.Count % 100 == 0) Console.WriteLine($"Creature discovery progress: {entries.Count}/{maxPages}...");
            }
        }

        var outDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "StarWarsRpgCs", "data");
        System.IO.Directory.CreateDirectory(outDir);
        var outPath = System.IO.Path.Combine(outDir, "creatures_output.json");
        var json = System.Text.Json.JsonSerializer.Serialize(entries, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        await System.IO.File.WriteAllTextAsync(outPath, json);

        Console.WriteLine($"Saved {entries.Count} creature entries to {outPath}");
        Console.WriteLine("Args: creatures [maxPages] [delayMs]   e.g. dotnet run --project ... creatures 4000 40");
        return 0;
    }

    private static async Task<CreatureEntry?> ParseCreatureFromPageAsync(string title, string cleanName, string category, int delayMs)
    {
        try
        {
            var api = $"{ApiBase}?action=parse&page={Uri.EscapeDataString(title)}&prop=text&format=json";
            var apiJson = await http.GetStringAsync(api);
            using var docj = System.Text.Json.JsonDocument.Parse(apiJson);
            if (!docj.RootElement.TryGetProperty("parse", out var parseNode)) return null;
            var text = parseNode.GetProperty("text").GetProperty("*").GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text)) return null;

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(text);
            var desc = ExtractCreatureDescription(doc);
            if (string.IsNullOrWhiteSpace(desc)) return null;

            var size = InferCreatureSize(cleanName, desc, doc);
            var habitat = InferCreatureHabitat(cleanName, desc);
            var danger = InferCreatureDanger(size, desc);
            if (delayMs > 0) await Task.Delay(delayMs);

            return new CreatureEntry
            {
                Name = cleanName,
                SourceUrl = "https://starwars.fandom.com/wiki/" + Uri.EscapeDataString(title.Replace(' ', '_')),
                Category = category,
                Description = desc,
                SizeClass = size,
                Habitat = habitat,
                DangerRating = danger
            };
        }
        catch
        {
            return null;
        }
    }

    private static string ExtractCreatureDescription(HtmlAgilityPack.HtmlDocument doc)
    {
        var p = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'mw-parser-output')]/p[normalize-space()]")
            ?? doc.DocumentNode.SelectSingleNode("//p[normalize-space()]");
        if (p is null) return string.Empty;
        var text = HtmlEntity.DeEntitize(p.InnerText.Trim());
        if (text.Length > 420) text = text.Substring(0, 420);
        return text;
    }

    private static string NormalizeCreatureName(string title)
    {
        var name = title.Trim();
        var slash = name.IndexOf('/');
        if (slash > 0) name = name.Substring(0, slash).Trim();
        name = name.Replace("(creature)", "", StringComparison.OrdinalIgnoreCase).Trim();
        return name;
    }

    private static async Task<List<string>> DiscoverCreaturePagesAsync(int maxCandidates)
    {
        var results = new List<string>();
        string? apcontinue = null;

        while (results.Count < maxCandidates)
        {
            var url = $"{ApiBase}?action=query&list=allpages&apnamespace=0&aplimit=500&format=json";
            if (!string.IsNullOrWhiteSpace(apcontinue)) url += $"&apcontinue={Uri.EscapeDataString(apcontinue)}";

            var json = await http.GetStringAsync(url);
            using var doc = System.Text.Json.JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("query", out var query) && query.TryGetProperty("allpages", out var pages))
            {
                foreach (var p in pages.EnumerateArray())
                {
                    if (!p.TryGetProperty("title", out var t)) continue;
                    var title = t.GetString();
                    if (string.IsNullOrWhiteSpace(title)) continue;
                    if (LooksLikeCreatureTarget(title)) results.Add(title);
                    if (results.Count >= maxCandidates) break;
                }
            }

            apcontinue = null;
            if (doc.RootElement.TryGetProperty("continue", out var cont) && cont.TryGetProperty("apcontinue", out var ap))
            {
                apcontinue = ap.GetString();
            }
            if (string.IsNullOrWhiteSpace(apcontinue)) break;
        }

        return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool LooksLikeCreatureTarget(string title)
    {
        var lower = title.ToLowerInvariant();
        var tokens = new[]
        {
            "creature", "beast", "monster", "fauna", "predator", "wyrm", "dragon", "rancor", "sarlacc",
            "wampa", "krayt", "mynock", "purrgil", "exogorth", "dianoga", "leviathan", "maw"
        };
        return tokens.Any(lower.Contains);
    }

    private static string InferCreatureSize(string name, string description, HtmlAgilityPack.HtmlDocument doc)
    {
        var tableText = (doc.DocumentNode.SelectSingleNode("//table")?.InnerText ?? string.Empty).ToLowerInvariant();
        var t = (name + " " + description + " " + tableText).ToLowerInvariant();
        if (t.Contains("colossal") || t.Contains("gigantic") || t.Contains("leviathan") || t.Contains("sarlacc") || t.Contains("maw")) return "colossal";
        if (t.Contains("huge") || t.Contains("giant")) return "huge";
        if (t.Contains("large")) return "large";
        if (t.Contains("small") || t.Contains("tiny")) return "small";
        return "medium";
    }

    private static string InferCreatureHabitat(string name, string description)
    {
        var t = (name + " " + description).ToLowerInvariant();
        if (t.Contains("desert") || t.Contains("dune") || t.Contains("sand")) return "desert";
        if (t.Contains("ice") || t.Contains("frozen") || t.Contains("snow")) return "ice";
        if (t.Contains("swamp") || t.Contains("jungle") || t.Contains("forest")) return "jungle";
        if (t.Contains("space") || t.Contains("asteroid") || t.Contains("nebula") || t.Contains("vacuum")) return "space";
        if (t.Contains("ocean") || t.Contains("water") || t.Contains("sea")) return "ocean";
        if (t.Contains("cave") || t.Contains("pit") || t.Contains("underground")) return "cave";
        return "general";
    }

    private static int InferCreatureDanger(string sizeClass, string description)
    {
        var danger = sizeClass switch
        {
            "small" => 2,
            "medium" => 4,
            "large" => 6,
            "huge" => 8,
            "colossal" => 10,
            _ => 4
        };
        var d = description.ToLowerInvariant();
        if (d.Contains("apex") || d.Contains("deadly") || d.Contains("predator") || d.Contains("ferocious")) danger++;
        if (d.Contains("docile") || d.Contains("passive")) danger--;
        return Math.Clamp(danger, 1, 10);
    }

    private static async Task<int> CrawlRacesAsync(int maxPages, int delayMs)
    {
        var raceCategories = new[]
        {
            "Category:Sentient_species",
            "Category:Humanoid_species",
            "Category:Near-Humans",
            "Category:Species"
        };

        var entries = new List<RaceEntry>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in raceCategories)
        {
            Console.WriteLine($"Enumerating {category}...");
            var titles = await GetCategoryMembersAsync(category);
            Console.WriteLine($"Found {titles.Count} members in {category}.");

            foreach (var title in titles)
            {
                if (entries.Count >= maxPages) break;
                var cleanName = NormalizeRaceName(title);
                if (string.IsNullOrWhiteSpace(cleanName)) continue;
                if (!seen.Add(cleanName)) continue;

                var parsed = await ParseRaceFromPageAsync(title, cleanName, category, delayMs);
                if (parsed is null) continue;
                entries.Add(parsed);
                if (entries.Count % 100 == 0) Console.WriteLine($"Race progress: {entries.Count}/{maxPages}...");
            }
            if (entries.Count >= maxPages) break;
        }

        if (entries.Count < maxPages)
        {
            var needed = maxPages - entries.Count;
            Console.WriteLine($"Race categories sparse, discovering up to {needed} additional race pages...");
            var discovered = await DiscoverRacePagesAsync(Math.Max(needed * 3, 1500));
            foreach (var title in discovered)
            {
                if (entries.Count >= maxPages) break;
                var cleanName = NormalizeRaceName(title);
                if (string.IsNullOrWhiteSpace(cleanName)) continue;
                if (!seen.Add(cleanName)) continue;

                var parsed = await ParseRaceFromPageAsync(title, cleanName, "Discovered", delayMs);
                if (parsed is null) continue;
                entries.Add(parsed);
                if (entries.Count % 100 == 0) Console.WriteLine($"Race discovery progress: {entries.Count}/{maxPages}...");
            }
        }

        var outDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "StarWarsRpgCs", "data");
        System.IO.Directory.CreateDirectory(outDir);
        var outPath = System.IO.Path.Combine(outDir, "races_output.json");
        var json = System.Text.Json.JsonSerializer.Serialize(entries, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        await System.IO.File.WriteAllTextAsync(outPath, json);

        Console.WriteLine($"Saved {entries.Count} race entries to {outPath}");
        Console.WriteLine("Args: races [maxPages] [delayMs]   e.g. dotnet run --project ... races 4000 40");
        return 0;
    }

    private static async Task<int> CrawlPlanetsAsync(int maxPages, int delayMs)
    {
        var planetCategories = new[]
        {
            "Category:Planets",
            "Category:Moons",
            "Category:Star systems",
            "Category:Colonies",
            "Category:Locations"
        };

        var entries = new List<PlanetEntry>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in planetCategories)
        {
            Console.WriteLine($"Enumerating {category}...");
            var titles = await GetCategoryMembersAsync(category);
            Console.WriteLine($"Found {titles.Count} members in {category}.");

            foreach (var title in titles)
            {
                if (entries.Count >= maxPages) break;
                var cleanName = NormalizePlanetName(title);
                if (string.IsNullOrWhiteSpace(cleanName)) continue;
                if (!seen.Add(cleanName)) continue;

                var parsed = await ParsePlanetFromPageAsync(title, cleanName, delayMs);
                if (parsed is null) continue;
                entries.Add(parsed);
                if (entries.Count % 100 == 0) Console.WriteLine($"Planet progress: {entries.Count}/{maxPages}...");
            }

            if (entries.Count >= maxPages) break;
        }

        if (entries.Count < maxPages)
        {
            var needed = maxPages - entries.Count;
            Console.WriteLine($"Planet categories sparse, discovering up to {needed} additional planet pages...");
            var discovered = await DiscoverPlanetPagesAsync(Math.Max(needed * 2, 1500));
            foreach (var title in discovered)
            {
                if (entries.Count >= maxPages) break;
                var cleanName = NormalizePlanetName(title);
                if (string.IsNullOrWhiteSpace(cleanName)) continue;
                if (!seen.Add(cleanName)) continue;

                var parsed = await ParsePlanetFromPageAsync(title, cleanName, delayMs);
                if (parsed is null) continue;
                entries.Add(parsed);
                if (entries.Count % 100 == 0) Console.WriteLine($"Planet discovery progress: {entries.Count}/{maxPages}...");
            }
        }

        var outDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "StarWarsRpgCs", "data");
        System.IO.Directory.CreateDirectory(outDir);
        var outPath = System.IO.Path.Combine(outDir, "planets_output.json");
        var json = System.Text.Json.JsonSerializer.Serialize(entries, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        await System.IO.File.WriteAllTextAsync(outPath, json);

        Console.WriteLine($"Saved {entries.Count} planet entries to {outPath}");
        Console.WriteLine("Args: planets [maxPages] [delayMs]   e.g. dotnet run --project ... planets 5000 40");
        return 0;
    }

    private static async Task<int> CrawlShipArmamentsAsync(int maxPages, int delayMs)
    {
        var categories = new[]
        {
            "Category:Starship_weapons",
            "Category:Turbolasers",
            "Category:Laser_cannons",
            "Category:Ion_cannons",
            "Category:Missile_launchers",
            "Category:Proton_torpedoes",
            "Category:Turrets",
            "Category:Bombs"
        };

        var entries = new List<ShipArmamentEntry>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in categories)
        {
            Console.WriteLine($"Enumerating {category}...");
            var titles = await GetCategoryMembersAsync(category);
            Console.WriteLine($"Found {titles.Count} members in {category}.");

            foreach (var title in titles)
            {
                if (entries.Count >= maxPages) break;
                var cleanName = NormalizeArmamentName(title);
                if (string.IsNullOrWhiteSpace(cleanName)) continue;
                if (!seen.Add(cleanName)) continue;

                var parsed = await ParseArmamentFromPageAsync(title, cleanName, category, delayMs);
                if (parsed is null) continue;
                entries.Add(parsed);
                if (entries.Count % 100 == 0) Console.WriteLine($"Armament progress: {entries.Count}/{maxPages}...");
            }

            if (entries.Count >= maxPages) break;
        }

        if (entries.Count < maxPages)
        {
            var needed = maxPages - entries.Count;
            Console.WriteLine($"Armament categories sparse, discovering up to {needed} additional pages...");
            var discovered = await DiscoverArmamentPagesAsync(Math.Max(needed * 3, 800));
            foreach (var title in discovered)
            {
                if (entries.Count >= maxPages) break;
                var cleanName = NormalizeArmamentName(title);
                if (string.IsNullOrWhiteSpace(cleanName)) continue;
                if (!seen.Add(cleanName)) continue;

                var parsed = await ParseArmamentFromPageAsync(title, cleanName, "Discovered", delayMs);
                if (parsed is null) continue;
                entries.Add(parsed);
                if (entries.Count % 100 == 0) Console.WriteLine($"Armament discovery progress: {entries.Count}/{maxPages}...");
            }
        }

        var outDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "StarWarsRpgCs", "data");
        System.IO.Directory.CreateDirectory(outDir);
        var outPath = System.IO.Path.Combine(outDir, "ship_armaments_output.json");
        var json = System.Text.Json.JsonSerializer.Serialize(entries, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        await System.IO.File.WriteAllTextAsync(outPath, json);

        Console.WriteLine($"Saved {entries.Count} ship armament entries to {outPath}");
        Console.WriteLine("Args: armaments [maxPages] [delayMs]   e.g. dotnet run --project ... armaments 1500 40");
        return 0;
    }

    private static async Task<RaceEntry?> ParseRaceFromPageAsync(string title, string cleanName, string category, int delayMs)
    {
        try
        {
            var api = $"{ApiBase}?action=parse&page={Uri.EscapeDataString(title)}&prop=text&format=json";
            var apiJson = await http.GetStringAsync(api);
            using var docj = System.Text.Json.JsonDocument.Parse(apiJson);
            if (!docj.RootElement.TryGetProperty("parse", out var parseNode)) return null;
            var text = parseNode.GetProperty("text").GetProperty("*").GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text)) return null;

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(text);
            var desc = ExtractRaceDescription(doc);
            if (string.IsNullOrWhiteSpace(desc)) return null;

            if (delayMs > 0) await Task.Delay(delayMs);
            return new RaceEntry
            {
                Name = cleanName,
                SourceUrl = "https://starwars.fandom.com/wiki/" + Uri.EscapeDataString(title.Replace(' ', '_')),
                Category = category,
                Description = desc
            };
        }
        catch
        {
            return null;
        }
    }

    private static string ExtractRaceDescription(HtmlAgilityPack.HtmlDocument doc)
    {
        var p = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'mw-parser-output')]/p[normalize-space()]")
            ?? doc.DocumentNode.SelectSingleNode("//p[normalize-space()]");
        if (p is null) return string.Empty;
        var text = HtmlEntity.DeEntitize(p.InnerText.Trim());
        if (text.Length > 420) text = text.Substring(0, 420);
        return text;
    }

    private static string? NormalizeRaceName(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return null;
        var name = title.Trim();
        var slash = name.IndexOf('/');
        if (slash > 0) name = name.Substring(0, slash).Trim();
        name = name.Replace("(species)", "", StringComparison.OrdinalIgnoreCase).Trim();
        if (name.StartsWith("Category:", StringComparison.OrdinalIgnoreCase)) return null;
        if (name.Length < 2 || name.Length > 70) return null;
        if (name.Any(char.IsDigit) && !name.Contains("-")) return null;
        return name;
    }

    private static async Task<List<string>> DiscoverRacePagesAsync(int maxCandidates)
    {
        var results = new List<string>();
        string? apcontinue = null;

        while (results.Count < maxCandidates)
        {
            var url = $"{ApiBase}?action=query&list=allpages&apnamespace=0&aplimit=500&format=json";
            if (!string.IsNullOrWhiteSpace(apcontinue)) url += $"&apcontinue={Uri.EscapeDataString(apcontinue)}";

            var json = await http.GetStringAsync(url);
            using var doc = System.Text.Json.JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("query", out var query) && query.TryGetProperty("allpages", out var pages))
            {
                foreach (var p in pages.EnumerateArray())
                {
                    if (!p.TryGetProperty("title", out var t)) continue;
                    var title = t.GetString();
                    if (string.IsNullOrWhiteSpace(title)) continue;
                    if (LooksLikeRaceTarget(title)) results.Add(title);
                    if (results.Count >= maxCandidates) break;
                }
            }

            apcontinue = null;
            if (doc.RootElement.TryGetProperty("continue", out var cont) && cont.TryGetProperty("apcontinue", out var ap))
            {
                apcontinue = ap.GetString();
            }
            if (string.IsNullOrWhiteSpace(apcontinue)) break;
        }

        return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool LooksLikeRaceTarget(string title)
    {
        var lower = title.ToLowerInvariant();
        return lower.Contains("species") ||
               lower.Contains("people") ||
               lower.Contains("near-human") ||
               lower.EndsWith("ian") ||
               lower.EndsWith("an") ||
               lower.EndsWith("ese") ||
               lower.EndsWith("ari") ||
               lower.EndsWith("ite");
    }

    private static async Task<PlanetEntry?> ParsePlanetFromPageAsync(string title, string cleanName, int delayMs)
    {
        try
        {
            var api = $"{ApiBase}?action=parse&page={Uri.EscapeDataString(title)}&prop=text&format=json";
            var apiJson = await http.GetStringAsync(api);
            using var docj = System.Text.Json.JsonDocument.Parse(apiJson);
            if (!docj.RootElement.TryGetProperty("parse", out var parseNode)) return null;
            var text = parseNode.GetProperty("text").GetProperty("*").GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text)) return null;

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(text);
            var desc = ExtractRaceDescription(doc);
            if (string.IsNullOrWhiteSpace(desc)) return null;

            var infoboxText = HtmlEntity.DeEntitize(doc.DocumentNode.SelectSingleNode("//aside|//table")?.InnerText ?? string.Empty);
            var region = InferPlanetRegion(infoboxText, desc);
            var sector = InferPlanetSector(infoboxText, desc);
            var economy = InferPlanetEconomy(cleanName, infoboxText, desc);
            var era = InferEraFromPlanetText(cleanName, desc, infoboxText);
            var threat = InferPlanetThreat(desc, economy);
            var travelCost = InferPlanetTravelCost(region, threat);
            var hasDockyard = economy.Contains("ship", StringComparison.OrdinalIgnoreCase)
                || economy.Contains("dock", StringComparison.OrdinalIgnoreCase)
                || infoboxText.Contains("shipyard", StringComparison.OrdinalIgnoreCase);
            var hasIndustrialFurnace = hasDockyard
                || economy.Contains("mining", StringComparison.OrdinalIgnoreCase)
                || economy.Contains("refinery", StringComparison.OrdinalIgnoreCase)
                || economy.Contains("industry", StringComparison.OrdinalIgnoreCase)
                || economy.Contains("factory", StringComparison.OrdinalIgnoreCase);

            if (delayMs > 0) await Task.Delay(delayMs);

            return new PlanetEntry
            {
                Name = cleanName,
                SourceUrl = "https://starwars.fandom.com/wiki/" + Uri.EscapeDataString(title.Replace(' ', '_')),
                Description = desc,
                Region = region,
                Sector = sector,
                Economy = economy,
                Era = era,
                ThreatLevel = threat,
                TravelCost = travelCost,
                HasDockyard = hasDockyard,
                HasIndustrialFurnace = hasIndustrialFurnace,
                DayEvents = BuildPlanetEvents(cleanName, desc, false),
                NightEvents = BuildPlanetEvents(cleanName, desc, true)
            };
        }
        catch
        {
            return null;
        }
    }

    private static string? NormalizePlanetName(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return null;
        var name = title.Trim();
        var slash = name.IndexOf('/');
        if (slash > 0) name = name[..slash].Trim();
        name = name.Replace("(planet)", "", StringComparison.OrdinalIgnoreCase)
            .Replace("(moon)", "", StringComparison.OrdinalIgnoreCase)
            .Replace("(location)", "", StringComparison.OrdinalIgnoreCase)
            .Trim();
        if (name.StartsWith("Category:", StringComparison.OrdinalIgnoreCase)) return null;
        if (name.Length < 2 || name.Length > 90) return null;
        if (name.Any(char.IsDigit) && !name.Contains('-')) return null;
        return name;
    }

    private static async Task<List<string>> DiscoverPlanetPagesAsync(int maxCandidates)
    {
        var results = new List<string>();
        string? apcontinue = null;

        while (results.Count < maxCandidates)
        {
            var url = $"{ApiBase}?action=query&list=allpages&apnamespace=0&aplimit=500&format=json";
            if (!string.IsNullOrWhiteSpace(apcontinue)) url += $"&apcontinue={Uri.EscapeDataString(apcontinue)}";

            var json = await http.GetStringAsync(url);
            using var doc = System.Text.Json.JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("query", out var query) && query.TryGetProperty("allpages", out var pages))
            {
                foreach (var p in pages.EnumerateArray())
                {
                    if (!p.TryGetProperty("title", out var t)) continue;
                    var title = t.GetString();
                    if (string.IsNullOrWhiteSpace(title)) continue;
                    if (LooksLikePlanetTarget(title)) results.Add(title);
                    if (results.Count >= maxCandidates) break;
                }
            }

            apcontinue = null;
            if (doc.RootElement.TryGetProperty("continue", out var cont) && cont.TryGetProperty("apcontinue", out var ap))
            {
                apcontinue = ap.GetString();
            }
            if (string.IsNullOrWhiteSpace(apcontinue)) break;
        }

        return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool LooksLikePlanetTarget(string title)
    {
        var lower = title.ToLowerInvariant();
        var tokens = new[]
        {
            "planet", "moon", "system", "world", "sector", "colony", "rim", "core", "hoth", "naboo", "coruscant", "tatooine"
        };
        return tokens.Any(lower.Contains);
    }

    private static async Task<ShipArmamentEntry?> ParseArmamentFromPageAsync(string title, string cleanName, string category, int delayMs)
    {
        try
        {
            var api = $"{ApiBase}?action=parse&page={Uri.EscapeDataString(title)}&prop=text&format=json";
            var apiJson = await http.GetStringAsync(api);
            using var docj = System.Text.Json.JsonDocument.Parse(apiJson);
            if (!docj.RootElement.TryGetProperty("parse", out var parseNode)) return null;
            var text = parseNode.GetProperty("text").GetProperty("*").GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text)) return null;

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(text);
            var desc = ExtractRaceDescription(doc);
            if (string.IsNullOrWhiteSpace(desc)) return null;

            var categoryGuess = InferArmamentCategory(cleanName, desc, category);
            var size = InferArmamentHardpointSize(cleanName, desc, categoryGuess);
            var era = InferEraFromPlanetText(cleanName, desc, category);
            var damage = InferArmamentDamage(size, cleanName, desc, categoryGuess);
            var fuelDraw = InferArmamentFuelDraw(size, categoryGuess);

            if (delayMs > 0) await Task.Delay(delayMs);

            return new ShipArmamentEntry
            {
                Name = cleanName,
                SourceUrl = "https://starwars.fandom.com/wiki/" + Uri.EscapeDataString(title.Replace(' ', '_')),
                Category = categoryGuess,
                HardpointSize = size,
                Era = era,
                DamageRating = damage,
                FuelDraw = fuelDraw,
                Description = desc
            };
        }
        catch
        {
            return null;
        }
    }

    private static string? NormalizeArmamentName(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return null;
        var name = title.Trim();
        var slash = name.IndexOf('/');
        if (slash > 0) name = name[..slash].Trim();
        name = name.Replace("(weapon)", "", StringComparison.OrdinalIgnoreCase)
            .Replace("(starship weapon)", "", StringComparison.OrdinalIgnoreCase)
            .Replace("(turbolaser)", "", StringComparison.OrdinalIgnoreCase)
            .Trim();
        if (name.StartsWith("Category:", StringComparison.OrdinalIgnoreCase)) return null;
        if (name.Length < 3 || name.Length > 120) return null;
        return name;
    }

    private static async Task<List<string>> DiscoverArmamentPagesAsync(int maxCandidates)
    {
        var results = new List<string>();
        string? apcontinue = null;

        while (results.Count < maxCandidates)
        {
            var url = $"{ApiBase}?action=query&list=allpages&apnamespace=0&aplimit=500&format=json";
            if (!string.IsNullOrWhiteSpace(apcontinue)) url += $"&apcontinue={Uri.EscapeDataString(apcontinue)}";

            var json = await http.GetStringAsync(url);
            using var doc = System.Text.Json.JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("query", out var query) && query.TryGetProperty("allpages", out var pages))
            {
                foreach (var p in pages.EnumerateArray())
                {
                    if (!p.TryGetProperty("title", out var t)) continue;
                    var title = t.GetString();
                    if (string.IsNullOrWhiteSpace(title)) continue;
                    if (LooksLikeArmamentTarget(title)) results.Add(title);
                    if (results.Count >= maxCandidates) break;
                }
            }

            apcontinue = null;
            if (doc.RootElement.TryGetProperty("continue", out var cont) && cont.TryGetProperty("apcontinue", out var ap))
            {
                apcontinue = ap.GetString();
            }
            if (string.IsNullOrWhiteSpace(apcontinue)) break;
        }

        return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool LooksLikeArmamentTarget(string title)
    {
        var lower = title.ToLowerInvariant();
        var tokens = new[]
        {
            "turbolaser", "laser cannon", "ion cannon", "turret", "missile", "torpedo", "bomb launcher", "autoblaster", "tractor beam"
        };
        return tokens.Any(lower.Contains);
    }

    private static string InferArmamentCategory(string name, string description, string category)
    {
        var lower = (name + " " + description + " " + category).ToLowerInvariant();
        if (lower.Contains("turbolaser")) return "turbolaser";
        if (lower.Contains("ion")) return "ion";
        if (lower.Contains("torpedo")) return "torpedo";
        if (lower.Contains("missile") || lower.Contains("rocket")) return "missile";
        if (lower.Contains("autoblaster")) return "autoblaster";
        if (lower.Contains("tractor")) return "tractor";
        return "laser";
    }

    private static string InferArmamentHardpointSize(string name, string description, string category)
    {
        var lower = (name + " " + description + " " + category).ToLowerInvariant();
        if (lower.Contains("heavy") || lower.Contains("battery") || lower.Contains("capital") || lower.Contains("quad turbolaser") || lower.Contains("tractor beam")) return "L";
        if (lower.Contains("turret") || lower.Contains("torpedo") || lower.Contains("missile") || lower.Contains("bomb") || lower.Contains("ion cannon")) return "M";
        return "S";
    }

    private static int InferArmamentDamage(string size, string name, string description, string category)
    {
        var damage = size switch
        {
            "M" => 14,
            "L" => 22,
            _ => 8
        };

        var lower = (name + " " + description + " " + category).ToLowerInvariant();
        if (lower.Contains("heavy") || lower.Contains("quad")) damage += 3;
        if (lower.Contains("light")) damage -= 1;
        if (lower.Contains("ion")) damage -= 1;
        return Math.Clamp(damage, 6, 30);
    }

    private static int InferArmamentFuelDraw(string size, string category)
    {
        var draw = size switch
        {
            "M" => 1,
            "L" => 3,
            _ => 0
        };

        if (category.Equals("ion", StringComparison.OrdinalIgnoreCase)) draw += 1;
        return Math.Clamp(draw, 0, 5);
    }

    private static string InferPlanetRegion(string infoboxText, string description)
    {
        var lower = (infoboxText + " " + description).ToLowerInvariant();
        if (lower.Contains("core worlds")) return "Core Worlds";
        if (lower.Contains("inner rim")) return "Inner Rim";
        if (lower.Contains("mid rim")) return "Mid Rim";
        if (lower.Contains("outer rim")) return "Outer Rim";
        if (lower.Contains("unknown regions")) return "Unknown Regions";
        if (lower.Contains("corporate sector")) return "Corporate Sector";
        return "Outer Rim";
    }

    private static string InferPlanetSector(string infoboxText, string description)
    {
        var lower = (infoboxText + " " + description).ToLowerInvariant();
        if (lower.Contains("core worlds")) return "Core Worlds";
        if (lower.Contains("corporate sector")) return "Corporate Sector";
        if (lower.Contains("unknown regions")) return "Unknown Regions";
        if (lower.Contains("mid rim")) return "Mid Rim";
        if (lower.Contains("outer rim")) return "Outer Rim";
        return "Unknown";
    }

    private static string InferPlanetEconomy(string name, string infoboxText, string description)
    {
        var lower = (name + " " + infoboxText + " " + description).ToLowerInvariant();
        var parts = new List<string>();
        if (lower.Contains("ship") || lower.Contains("dock")) parts.Add("Shipyards");
        if (lower.Contains("mine") || lower.Contains("ore")) parts.Add("Mining");
        if (lower.Contains("refin") || lower.Contains("factory") || lower.Contains("industry")) parts.Add("Refinery and industry");
        if (lower.Contains("farm") || lower.Contains("agri")) parts.Add("Agriculture");
        if (lower.Contains("trade") || lower.Contains("market") || lower.Contains("smuggl")) parts.Add("Trade");
        if (lower.Contains("temple") || lower.Contains("relic") || lower.Contains("pilgrim")) parts.Add("Relic study");
        if (parts.Count == 0) parts.Add("Trade and survival");
        return string.Join(", ", parts.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static string InferEraFromPlanetText(string name, string description, string infoboxText)
    {
        var text = (name + " " + description + " " + infoboxText).ToLowerInvariant();
        if (text.Contains("first order") || text.Contains("resistance") || text.Contains("sequel")) return "Sequel Trilogy";
        if (text.Contains("new republic")) return "New Republic";
        if (text.Contains("empire") || text.Contains("imperial") || text.Contains("rebel")) return "Original Trilogy";
        if (text.Contains("clone") || text.Contains("separat")) return "Clone Wars";
        if (text.Contains("high republic") || text.Contains("old republic") || text.Contains("ancient")) return "Old Republic";
        return "Old Republic";
    }

    private static string InferPlanetThreat(string description, string economy)
    {
        var text = (description + " " + economy).ToLowerInvariant();
        if (text.Contains("war") || text.Contains("danger") || text.Contains("sith") || text.Contains("deadly") || text.Contains("volcan")) return "High";
        if (text.Contains("peaceful") || text.Contains("calm") || text.Contains("diplom")) return "Low";
        return "Moderate";
    }

    private static int InferPlanetTravelCost(string region, string threat)
    {
        var baseCost = region switch
        {
            "Core Worlds" => 14,
            "Inner Rim" => 16,
            "Mid Rim" => 18,
            "Outer Rim" => 22,
            "Unknown Regions" => 28,
            _ => 20
        };

        baseCost += threat switch
        {
            "Low" => -2,
            "High" => 4,
            "Very High" => 6,
            _ => 0
        };

        return Math.Clamp(baseCost, 10, 40);
    }

    private static List<string> BuildPlanetEvents(string name, string description, bool night)
    {
        var lower = (name + " " + description).ToLowerInvariant();
        var events = new List<string>();
        if (lower.Contains("desert")) events.Add(night ? "dune ambush" : "sand market");
        if (lower.Contains("forest") || lower.Contains("jungle")) events.Add(night ? "canopy watch" : "wild hunt");
        if (lower.Contains("city") || lower.Contains("ecumenopolis")) events.Add(night ? "underworld exchange" : "trade district rush");
        if (lower.Contains("volcan")) events.Add(night ? "ash storm" : "forge caravan");
        if (lower.Contains("ocean") || lower.Contains("water")) events.Add(night ? "storm crossing" : "dock trade");
        if (events.Count == 0) events.Add(night ? "night patrol" : "market trade");
        events.Add(night ? "shadow exchange" : "survey mission");
        return events.Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToList();
    }

    private static async Task<List<string>> DiscoverCandidatePagesAsync(int maxCandidates)
    {
        var results = new List<string>();
        string? apcontinue = null;
        while (results.Count < maxCandidates)
        {
            var url = $"{ApiBase}?action=query&list=allpages&apnamespace=0&aplimit=500&format=json";
            if (!string.IsNullOrWhiteSpace(apcontinue)) url += $"&apcontinue={Uri.EscapeDataString(apcontinue)}";

            var json = await http.GetStringAsync(url);
            using var doc = System.Text.Json.JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("query", out var query) && query.TryGetProperty("allpages", out var pages))
            {
                foreach (var p in pages.EnumerateArray())
                {
                    if (!p.TryGetProperty("title", out var t)) continue;
                    var title = t.GetString();
                    if (string.IsNullOrWhiteSpace(title)) continue;
                    if (LooksLikeCraftingTarget(title)) results.Add(title);
                    if (results.Count >= maxCandidates) break;
                }
            }

            apcontinue = null;
            if (doc.RootElement.TryGetProperty("continue", out var cont) && cont.TryGetProperty("apcontinue", out var ap))
            {
                apcontinue = ap.GetString();
            }

            if (string.IsNullOrWhiteSpace(apcontinue)) break;
        }

        return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool LooksLikeCraftingTarget(string title)
    {
        var lower = title.ToLowerInvariant();
        var tokens = new[]
        {
            "blaster", "rifle", "pistol", "cannon", "saber", "lightsaber", "staff", "detonator",
            "ship", "starship", "fighter", "bomber", "freighter", "shuttle", "cruiser", "destroyer", "corvette",
            "speeder", "walker", "tank", "vehicle", "bike", "droid", "probe",
            "ore", "alloy", "crystal", "material", "armor", "reactor", "hyperdrive", "sensor", "cell", "capacitor"
        };
        return tokens.Any(lower.Contains);
    }

    private static async Task<List<string>> GetCategoryMembersAsync(string categoryTitle)
    {
        var titles = new List<string>();
        string? cmcontinue = null;

        do
        {
            var url = $"{ApiBase}?action=query&list=categorymembers&cmtitle={Uri.EscapeDataString(categoryTitle)}&cmlimit=500&format=json";
            if (!string.IsNullOrWhiteSpace(cmcontinue)) url += $"&cmcontinue={Uri.EscapeDataString(cmcontinue)}";

            var json = await http.GetStringAsync(url);
            using var doc = System.Text.Json.JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("query", out var query) && query.TryGetProperty("categorymembers", out var members))
            {
                foreach (var m in members.EnumerateArray())
                {
                    if (m.TryGetProperty("title", out var t))
                    {
                        var title = t.GetString();
                        if (!string.IsNullOrWhiteSpace(title) && !title.StartsWith("Category:", StringComparison.OrdinalIgnoreCase))
                        {
                            titles.Add(title);
                        }
                    }
                }
            }

            cmcontinue = null;
            if (doc.RootElement.TryGetProperty("continue", out var cont) && cont.TryGetProperty("cmcontinue", out var cm))
            {
                cmcontinue = cm.GetString();
            }
        }
        while (!string.IsNullOrWhiteSpace(cmcontinue));

        return titles;
    }

    private static async Task<RecipeEntry?> ParseRecipeFromPageAsync(string title, string category, int delayMs)
    {
        try
        {
            var api = $"{ApiBase}?action=parse&page={Uri.EscapeDataString(title)}&prop=text&format=json";
            var apiJson = await http.GetStringAsync(api);
            using var docj = System.Text.Json.JsonDocument.Parse(apiJson);

            if (!docj.RootElement.TryGetProperty("parse", out var parseNode)) return null;
            var text = parseNode.GetProperty("text").GetProperty("*").GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text)) return null;

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(text);

            var materials = ExtractMaterials(doc);
            var pageUrl = "https://starwars.fandom.com/wiki/" + Uri.EscapeDataString(title.Replace(' ', '_'));

            if (delayMs > 0) await Task.Delay(delayMs);

            return new RecipeEntry
            {
                Name = title,
                SourceUrl = pageUrl,
                Category = category,
                Materials = materials
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Parse failed for {title}: {ex.Message}");
            return null;
        }
    }

    private static List<string> ExtractMaterials(HtmlAgilityPack.HtmlDocument doc)
    {
        var materials = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var nodes = doc.DocumentNode.SelectNodes("//table//th|//table//td");
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                var label = (node.InnerText ?? string.Empty).Trim().ToLowerInvariant();
                if (!(label.Contains("material") || label.Contains("component") || label.Contains("part") || label.Contains("craft"))) continue;

                var td = node.Name.Equals("th", StringComparison.OrdinalIgnoreCase) ? node.ParentNode.SelectSingleNode("td") : node;
                if (td is null) continue;

                foreach (var token in ExtractTokens(td)) materials.Add(token);
            }
        }

        if (materials.Count == 0)
        {
            var lists = doc.DocumentNode.SelectNodes("//ul//li|//ol//li");
            if (lists != null)
            {
                foreach (var li in lists.Take(25))
                {
                    var token = HtmlEntity.DeEntitize(li.InnerText.Trim());
                    if (IsLikelyMaterialToken(token)) materials.Add(token);
                }
            }
        }

        return materials.Where(IsLikelyMaterialToken).Take(30).ToList();
    }

    private static IEnumerable<string> ExtractTokens(HtmlNode node)
    {
        var lis = node.SelectNodes(".//li");
        if (lis != null)
        {
            foreach (var li in lis)
            {
                var t = HtmlEntity.DeEntitize(li.InnerText.Trim());
                if (!string.IsNullOrWhiteSpace(t)) yield return t;
            }
            yield break;
        }

        var parts = node.InnerHtml.Split(new[] { "<br>", "<br/>", "<br />", ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            var text = HtmlEntity.DeEntitize(StripTags(p).Trim());
            if (!string.IsNullOrWhiteSpace(text)) yield return text;
        }
    }

    private static bool IsLikelyMaterialToken(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        if (s.Length < 2 || s.Length > 140) return false;
        var lower = s.ToLowerInvariant();
        if (lower.Contains("article") || lower.Contains("episode") || lower.Contains("comic") || lower.Contains("insider") || lower.Contains("expand") || lower.Contains("update")) return false;
        return true;
    }

    private static string StripTags(string html)
    {
        var d = new HtmlAgilityPack.HtmlDocument();
        d.LoadHtml(html);
        return d.DocumentNode.InnerText;
    }
}
