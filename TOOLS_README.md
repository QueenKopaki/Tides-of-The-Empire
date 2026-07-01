Scraper and Importer

This project includes a basic recipe scraper utility implemented in `Tools/RecipeScraper.cs`.

Usage (from code):

- Call `RecipeScraper.ScrapeWookieepediaAsync(urls)` to retrieve a list of `RecipeEntry` objects for the supplied URLs.
- Call `RecipeScraper.SaveRecipesToFileAsync(recipes, "data/recipes_output.json")` to save results.

Notes and cautions:
- Respect site `robots.txt` and the source site's Terms of Use. This scraper performs only a minimal robots check.
- Wookieepedia and other fandom sites use varied page structures; results require human review and normalization.

Suggested workflow:
1. Prepare a list of target URLs (one per item page).
2. Run a small C# runner or call from the game to scrape and save JSON.
3. Manually review `data/recipes_output.json` and map names/materials to in-game IDs.
4. Use an importer (to be implemented) to merge recipes into the game's catalogs.

If you want, I can:
- Add a UI button to run the scraper from the game and show progress.
- Implement per-site adapters to improve accuracy for specific sites (Wookieepedia, fandom wikis).
- Implement an importer that maps recipes into `GameEngine` catalogs and persists them.
