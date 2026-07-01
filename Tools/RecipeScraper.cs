using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace StarWarsRpgCs.Tools
{
    public class RecipeEntry
    {
        public string Name { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public List<string> Materials { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
    }

    public static class RecipeScraper
    {
        private static readonly HttpClient http = new HttpClient();

        static RecipeScraper()
        {
            try
            {
                http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                http.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            }
            catch { }
        }

        public static async Task<List<RecipeEntry>> ScrapeWookieepediaAsync(IEnumerable<string> urls)
        {
            var results = new List<RecipeEntry>();
            foreach (var url in urls)
            {
                try
                {
                    var uri = new Uri(url);
                    var baseUri = uri.GetLeftPart(UriPartial.Authority);

                    // basic robots.txt check - if disallowed on all, skip
                    try
                    {
                        var robots = await http.GetStringAsync(new Uri(new Uri(baseUri), "/robots.txt"));
                        if (robots.Contains("Disallow: /"))
                        {
                            Console.WriteLine($"Robots.txt disallows scraping {baseUri} — skipping {url}");
                            continue;
                        }
                    }
                    catch
                    {
                        // ignore robots fetch errors and attempt page fetch
                    }

                    var html = await http.GetStringAsync(uri);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    // try to get page title
                    var titleNode = doc.GetElementbyId("firstHeading") ?? doc.DocumentNode.SelectSingleNode("//h1");
                    var title = titleNode?.InnerText?.Trim() ?? uri.Segments.Last().Replace('_', ' ');

                    var entry = new RecipeEntry { Name = title, SourceUrl = url };

                    // attempt to find infobox rows (th/td pairs) mentioning materials/components
                    var ths = doc.DocumentNode.SelectNodes("//table//th|//table//td");
                    if (ths != null)
                    {
                        for (int i = 0; i < ths.Count; i++)
                        {
                            var nodeText = ths[i].InnerText?.Trim() ?? string.Empty;
                            var lower = nodeText.ToLowerInvariant();
                            if (lower.Contains("material") || lower.Contains("component") || lower.Contains("craft") || lower.Contains("parts"))
                            {
                                // get sibling td if possible
                                HtmlNode? td = null;
                                if (ths[i].Name.Equals("th", StringComparison.OrdinalIgnoreCase))
                                {
                                    td = ths[i].ParentNode.SelectSingleNode("td");
                                }
                                else td = ths[i];

                                if (td != null)
                                {
                                    var items = ExtractMaterialList(td);
                                    foreach (var it in items) if (!entry.Materials.Contains(it)) entry.Materials.Add(it);
                                }
                            }
                        }
                    }

                    // fallback: look for lists under headers mentioning "Materials" or "Components"
                    var headers = doc.DocumentNode.SelectNodes("//h2|//h3|//h4");
                    if (headers != null)
                    {
                        foreach (var h in headers)
                        {
                            var hdr = h.InnerText?.ToLowerInvariant() ?? string.Empty;
                            if (hdr.Contains("material") || hdr.Contains("component") || hdr.Contains("craft") || hdr.Contains("parts"))
                            {
                                var next = h.NextSibling;
                                while (next != null && next.Name != "ul" && next.Name != "ol") next = next.NextSibling;
                                if (next != null && (next.Name == "ul" || next.Name == "ol"))
                                {
                                    var items = next.SelectNodes(".//li")?.Select(n => HtmlEntity.DeEntitize(n.InnerText.Trim())).Where(s => !string.IsNullOrWhiteSpace(s)).ToList() ?? new List<string>();
                                    foreach (var it in items) if (!entry.Materials.Contains(it)) entry.Materials.Add(it);
                                }
                            }
                        }
                    }

                    // final fallback: search for any bolded items or lists
                    if (!entry.Materials.Any())
                    {
                        var lis = doc.DocumentNode.SelectNodes("//li");
                        if (lis != null)
                        {
                            foreach (var li in lis.Take(20))
                            {
                                var text = HtmlEntity.DeEntitize(li.InnerText.Trim());
                                if (text.Length > 3 && text.Length < 120 && text.Any(char.IsLetter))
                                {
                                    entry.Materials.Add(text);
                                }
                            }
                            entry.Materials = entry.Materials.Distinct().ToList();
                        }
                    }

                    results.Add(entry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error scraping {url}: {ex.Message}");
                }
            }

            return results;
        }

        private static List<string> ExtractMaterialList(HtmlNode td)
        {
            var found = new List<string>();
            // look for list items inside td
            var lis = td.SelectNodes(".//li");
            if (lis != null)
            {
                foreach (var li in lis)
                {
                    var s = HtmlEntity.DeEntitize(li.InnerText.Trim());
                    if (!string.IsNullOrWhiteSpace(s)) found.Add(s);
                }
                return found.Distinct().ToList();
            }

            // otherwise split by <br> or commas
            var html = td.InnerHtml;
            var parts = html.Split(new[] { "<br>", "<br/>", "<br />", "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var t = HtmlEntity.DeEntitize(StripTags(p).Trim());
                if (!string.IsNullOrWhiteSpace(t)) found.Add(t);
            }
            return found.Distinct().ToList();
        }

        private static string StripTags(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode.InnerText;
        }

        public static async Task SaveRecipesToFileAsync(IEnumerable<RecipeEntry> recipes, string outPath)
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(recipes, opts);
            await System.IO.File.WriteAllTextAsync(outPath, json);
        }
    }
}
