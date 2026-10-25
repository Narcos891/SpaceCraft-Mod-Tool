# SpaceCraft Mod Tool

A Windows desktop toolkit for **SpaceCraft** modding: extract the game database (`data.cdb`) from a `res.pak` archive, edit it with a CastleDB-style editor, and repack it as a `res2.pak` mod.

Unofficial community tool. Not affiliated with or endorsed by the game's developers.

## Features

- **PAK extraction / detection** — reads Heaps `PAK` archives and pulls out `data.cdb`, with an automatic `.bak` backup of any previous database.
- **CastleDB editor** — one tab per sheet, spreadsheet view of every row and column.
- **Search** — filters the current sheet live across all columns.
- **Guided side panel** — pick category → row → parameter and edit the value, including nested lists/objects in a small table grid with add/remove row support.
- **Constants browser** — the `constant` sheet grouped into macro-categories and sub-categories, with a typed data-type column and human-readable descriptions for the most common gameplay parameters.
- **Dark / light theme** toggled at runtime.
- **Three languages** — Italian, English, German (English by default).

## Requirements

- Windows 10 or 11
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet) (x64)

## Usage

Launch `SpaceCraft Mod Tool.exe`. The launcher walks through the modding workflow:

1. **Select the SpaceCraft installation folder.** Keep *Use default folder inside game directory* checked to write into `<game>\ModdingData`, or pick a custom output folder.
2. **Extract / Detect data.cdb** — reads `<game>\res.pak` and saves `data.cdb` into the output folder. If a `data.cdb` is already there it is backed up to `data.cdb.bak` first.
   *Alternative:* **Reopen Existing Mod** loads a `res1.pak` / `res2.pak` and extracts its `data.cdb` for editing.
3. **Open CDB Editor** — edit sheets, values, lists and constants, then **Save Database**.
4. **Build / Create Mod** — packs the edited `data.cdb` into `<game>\res2.pak`.

Start the game with `res2.pak` next to `res.pak` to load your changes. Back up the original files before modding.

## Building from source

```bash
git clone https://github.com/Narcos891/SpaceCraft-Mod-Tool.git
cd SpaceCraft-Mod-Tool
dotnet build -c Release
```

The executable is produced in `bin\Release\net10.0-windows\`. You can also open `SpaceCraft Mod Tool.slnx` in Visual Studio 2022 (17.13+) or newer.

Dependencies: [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json) 13.x (restored automatically by NuGet).

## Project layout

| File | Purpose |
| --- | --- |
| `Program.cs` | Entry point, starts the launcher. |
| `LauncherForm.cs` | Setup window: game folder, output folder, extract / open / build actions. |
| `Form1.cs` | Main CastleDB editor: tabs, grid, search, themes, guided panel, constants view. |
| `CellDetailEditorForm.cs` | Detail editor for a single cell (plain text or list grid). |
| `HeapsPakReader.cs` | Read-only Heaps `PAK` parser and file extractor. |
| `HeapsPakWriter.cs` | Builds a `PAK` archive from in-memory files. |
| `ConstantManager.cs` | Categorises `constant` rows and exposes filtering helpers. |
| `LanguageManager.cs` | UI translations (IT / EN / DE) and constant descriptions. |
| `BlueprintHandler.cs` | Blueprint encode/decode helpers. Currently **excluded from the build**. |

## Known limitations

- Only `data.cdb` is packed into `res2.pak`; other modified assets are not included.
- The PAK writer always emits header version 0 with 32-bit data offsets.
- Parameter descriptions in the constants tab are bundled translations and edits to that column are not persisted.
- Game updates may replace `res.pak`; re-extract the database after patching.

## License

Released under the [MIT License](LICENSE).
