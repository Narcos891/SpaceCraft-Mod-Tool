#nullable disable
using System;
using System.Collections.Generic;

namespace SpaceCraft_Mod_Tool
{
    public static class LanguageManager
    {
        public static string CurrentLanguage { get; set; } = "EN"; // Predefinito Inglese per Nexus Mods

        public static event Action OnLanguageChanged;

        private static readonly Dictionary<string, Dictionary<string, string>> Translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["IT"] = new Dictionary<string, string>
            {
                // Top Panel & Main Form
                ["BtnOpen"] = "📂 Apri data.cdb / JSON",
                ["BtnSave"] = "💾 Salva Database",
                ["BtnThemeLight"] = "☀️ Modalità Chiara",
                ["BtnThemeDark"] = "🌙 Modalità Scura",
                ["BtnGuided"] = "🪟 Pannello Guidato",
                ["SearchLabel"] = "🔍 Cerca:",
                ["FormTitle"] = "SpaceCraft CastleDB Editor (Stile HIDE Avanzato + Smart Editor Tabellare + Ricerca)",

                // Smart Editor Side Panel
                ["SmartTitle"] = "✨ Smart Editor Guidato",
                ["CatLabel"] = "1. Categoria (Tabella):",
                ["ItemLabel"] = "2. Elemento / Riga:",
                ["PropLabel"] = "3. Parametro:",
                ["ValLabel"] = "4. Valore / Tabella Dati:",
                ["BtnAddRow"] = "➕ Aggiungi Riga",
                ["BtnRemoveRow"] = "➖ Rimuovi Selezionato",
                ["BtnApply"] = "⚡ Salva Modifiche nel DB",
                ["SideWarning"] = "💡 Modifica i dati direttamente nella tabella o nel campo.",

                // Constant Interface
                ["MacroCatLabel"] = "Macro-Categoria:",
                ["SubCatLabel"] = "Sotto-Categoria:",
                ["ColId"] = "Parametro (ID)",
                ["ColDataType"] = "Tipo di Dato",
                ["ColValue"] = "Valore",
                ["ColDesc"] = "Descrizione",

                // Launcher UI
                ["LauncherTitle"] = "SpaceCraft Mod Tool - Launcher & Setup",
                ["LblGameFolder"] = "Cartella di installazione SpaceCraft:",
                ["BtnBrowse"] = "Sfoglia...",
                ["ChkDefaultOutput"] = "Usa cartella di default nella directory di gioco",
                ["LblOutputFolder"] = "Cartella di output per il Modding / Estrazione:",
                ["BtnExtract"] = "1. Estrai / Rileva data.cdb",
                ["BtnOpenExisting"] = "Riapri Mod Esistente (res1.pak / res2.pak)",
                ["BtnLaunchEditor"] = "2. Apri Editor CDB (Stile HIDE)",
                ["BtnCreatePak"] = "3. Compila / Crea Mod (res2.pak)",
                ["StatusWait"] = "Stato: In attesa di configurazione..."
            },
            ["EN"] = new Dictionary<string, string>
            {
                // Top Panel & Main Form
                ["BtnOpen"] = "📂 Open data.cdb / JSON",
                ["BtnSave"] = "💾 Save Database",
                ["BtnThemeLight"] = "☀️ Light Mode",
                ["BtnThemeDark"] = "🌙 Dark Mode",
                ["BtnGuided"] = "🪟 Guided Panel",
                ["SearchLabel"] = "🔍 Search:",
                ["FormTitle"] = "SpaceCraft CastleDB Editor (Advanced HIDE Style + Tabular Smart Editor + Search)",

                // Smart Editor Side Panel
                ["SmartTitle"] = "✨ Smart Guided Editor",
                ["CatLabel"] = "1. Category (Table):",
                ["ItemLabel"] = "2. Item / Row:",
                ["PropLabel"] = "3. Parameter:",
                ["ValLabel"] = "4. Value / Data Table:",
                ["BtnAddRow"] = "➕ Add Row",
                ["BtnRemoveRow"] = "➖ Remove Selected",
                ["BtnApply"] = "⚡ Save Changes to DB",
                ["SideWarning"] = "💡 Edit data directly inside the table or text field.",

                // Constant Interface
                ["MacroCatLabel"] = "Macro-Category:",
                ["SubCatLabel"] = "Sub-Category:",
                ["ColId"] = "Parameter (ID)",
                ["ColDataType"] = "Data Type",
                ["ColValue"] = "Value",
                ["ColDesc"] = "Description",

                // Launcher UI
                ["LauncherTitle"] = "SpaceCraft Mod Tool - Launcher & Setup",
                ["LblGameFolder"] = "SpaceCraft Installation Folder:",
                ["BtnBrowse"] = "Browse...",
                ["ChkDefaultOutput"] = "Use default folder inside game directory",
                ["LblOutputFolder"] = "Output folder for Modding / Extraction:",
                ["BtnExtract"] = "1. Extract / Detect data.cdb",
                ["BtnOpenExisting"] = "Reopen Existing Mod (res1.pak / res2.pak)",
                ["BtnLaunchEditor"] = "2. Open CDB Editor (HIDE Style)",
                ["BtnCreatePak"] = "3. Build / Create Mod (res2.pak)",
                ["StatusWait"] = "Status: Waiting for configuration..."
            },
            ["DE"] = new Dictionary<string, string>
            {
                // Top Panel & Main Form
                ["BtnOpen"] = "📂 data.cdb / JSON öffnen",
                ["BtnSave"] = "💾 Datenbank speichern",
                ["BtnThemeLight"] = "☀️ Heller Modus",
                ["BtnThemeDark"] = "🌙 Dunkler Modus",
                ["BtnGuided"] = "🪟 Geführtes Panel",
                ["SearchLabel"] = "🔍 Suchen:",
                ["FormTitle"] = "SpaceCraft CastleDB Editor (Erweiterter HIDE-Stil + Geführter Editor + Suche)",

                // Smart Editor Side Panel
                ["SmartTitle"] = "✨ Geführter Smart Editor",
                ["CatLabel"] = "1. Kategorie (Tabelle):",
                ["ItemLabel"] = "2. Element / Zeile:",
                ["PropLabel"] = "3. Parameter:",
                ["ValLabel"] = "4. Wert / Datentabelle:",
                ["BtnAddRow"] = "➕ Zeile hinzufügen",
                ["BtnRemoveRow"] = "➖ Ausgewählte entfernen",
                ["BtnApply"] = "⚡ Änderungen in DB speichern",
                ["SideWarning"] = "💡 Bearbeiten Sie Daten direkt in der Tabelle oder im Textfeld.",

                // Constant Interface
                ["MacroCatLabel"] = "Makro-Kategorie:",
                ["SubCatLabel"] = "Unterkategorie:",
                ["ColId"] = "Parameter (ID)",
                ["ColDataType"] = "Datentyp",
                ["ColValue"] = "Wert",
                ["ColDesc"] = "Beschreibung",

                // Launcher UI
                ["LauncherTitle"] = "SpaceCraft Mod Tool - Launcher & Setup",
                ["LblGameFolder"] = "SpaceCraft-Installationsordner:",
                ["BtnBrowse"] = "Durchsuchen...",
                ["ChkDefaultOutput"] = "Standardordner im Spielverzeichnis verwenden",
                ["LblOutputFolder"] = "Ausgabeordner für Modding / Extraktion:",
                ["BtnExtract"] = "1. data.cdb extrahieren / erkennen",
                ["BtnOpenExisting"] = "Bestehende Mod öffnen (res1.pak / res2.pak)",
                ["BtnLaunchEditor"] = "2. CDB-Editor öffnen (HIDE-Stil)",
                ["BtnCreatePak"] = "3. Mod erstellen / kompilieren (res2.pak)",
                ["StatusWait"] = "Status: Warten auf Konfiguration..."
            }
        };

        // Descrizioni parametrate per la scheda 'constant' nelle 3 lingue
        private static readonly Dictionary<string, Dictionary<string, string>> ConstantDescriptions = new Dictionary<string, Dictionary<string, string>>
        {
            ["IT"] = new Dictionary<string, string>
            {
                { "CraftPriceFactor", "Prezzo dell'output = prezzo dell'input * (CraftPriceFactor ^ NbrOfDifferentInputs)" },
                { "FTLRefillPrice", "Prezzo per la ricarica del carburante FTL." },
                { "StartMoney", "Crediti iniziali del giocatore all'avvio della partita." },
                { "StartDebt", "Debito iniziale del giocatore." },
                { "SoldValueDailyRegen", "Rigenerazione giornaliera del valore dei beni venduti." },
                { "SoldValueMaxCap", "Limite massimo accumulabile per il valore dei beni venduti." },
                { "CorpoCreateCost", "Costo in crediti per fondare una nuova corporazione." },
                { "CorpoRenameCost", "Costo in crediti per rinominare la corporazione." },
                { "ShipGatherDistance", "Distanza massima a cui la nave raccoglie automaticamente le risorse." },
                { "ResourceInteractiveRadius", "Raggio di interazione del giocatore con le risorse." },
                { "MiningBaseTime", "Tempo base necessario per minare un nodo di risorse (valori più bassi = mining più veloce)." },
                { "MiningDefaultPower", "Potenza di mining predefinita quando gli strumenti sono distrutti." },
                { "ResourceRadarDistance", "Distanza a cui il radar inizia a rilevare le risorse." },
                { "ShipStartSpeed", "Velocità iniziale di partenza della nave." },
                { "ShipAcceleration", "Tasso di accelerazione normale della nave." },
                { "ShipBoostAcceleration", "Tasso di accelerazione quando si attiva il boost." },
                { "ShipManeuverabilityScale", "Scala di manovrabilità globale della nave." },
                { "ShipRadarDetectDistance", "Distanza a cui i relitti e le navi vengono rilevati automaticamente dal radar." },
                { "RadarRotSpeed", "Velocità di rotazione del radar di bordo." },
                { "FTLMinTravelDelay", "Ritardo minimo prima di avviare il salto FTL." },
                { "FTLStaticCost", "Costo fisso di carburante/energia per eseguire un salto FTL." },
                { "FTLMinDamageRatio", "Rapporto di danno minimo subito durante un salto FTL problematico." },
                { "FTLMaxDamageRatio", "Rapporto di danno massimo subito durante un salto FTL." },
                { "PlayerStorageCapacity", "Capacità massima dell'inventario/storage principale del giocatore." },
                { "PersonaBusInventoryCost", "Costo in crediti per ogni unità di spazio occupato quando si usa il trasporto." }
            },
            ["EN"] = new Dictionary<string, string>
            {
                { "CraftPriceFactor", "Output price = input price * (CraftPriceFactor ^ NbrOfDifferentInputs)" },
                { "FTLRefillPrice", "Price for FTL fuel refill." },
                { "StartMoney", "Initial player credits at game start." },
                { "StartDebt", "Initial player debt." },
                { "SoldValueDailyRegen", "Daily regeneration rate of sold goods value." },
                { "SoldValueMaxCap", "Maximum accumulable limit for sold goods value." },
                { "CorpoCreateCost", "Credit cost to found a new corporation." },
                { "CorpoRenameCost", "Credit cost to rename the corporation." },
                { "ShipGatherDistance", "Maximum distance at which the ship automatically collects resources." },
                { "ResourceInteractiveRadius", "Player interaction radius with resources." },
                { "MiningBaseTime", "Base time required to mine a resource node (lower values = faster mining)." },
                { "MiningDefaultPower", "Default mining power when tools are destroyed." },
                { "ResourceRadarDistance", "Distance at which the radar detects resources." },
                { "ShipStartSpeed", "Initial starting speed of the ship." },
                { "ShipAcceleration", "Normal ship acceleration rate." },
                { "ShipBoostAcceleration", "Acceleration rate when boost is active." },
                { "ShipManeuverabilityScale", "Global ship maneuverability scale." },
                { "ShipRadarDetectDistance", "Distance at which wrecks and ships are detected by radar." },
                { "RadarRotSpeed", "Ship radar rotation speed." },
                { "FTLMinTravelDelay", "Minimum delay before initiating an FTL jump." },
                { "FTLStaticCost", "Fixed fuel/energy cost to perform an FTL jump." },
                { "FTLMinDamageRatio", "Minimum damage ratio taken during an unstable FTL jump." },
                { "FTLMaxDamageRatio", "Maximum damage ratio taken during an FTL jump." },
                { "PlayerStorageCapacity", "Maximum main inventory/storage capacity of the player." },
                { "PersonaBusInventoryCost", "Credit cost per inventory slot when using transport." }
            },
            ["DE"] = new Dictionary<string, string>
            {
                { "CraftPriceFactor", "Ausgabepreis = Eingabepreis * (CraftPriceFactor ^ NbrOfDifferentInputs)" },
                { "FTLRefillPrice", "Preis für FTL-Treibstoff-Nachfüllung." },
                { "StartMoney", "Startguthaben des Spielers bei Spielbeginn." },
                { "StartDebt", "Anfangsschulden des Spielers." },
                { "SoldValueDailyRegen", "Tägliche Regenerationsrate des Wertes verkaufter Waren." },
                { "SoldValueMaxCap", "Maximaler Grenzwert für den Wert verkaufter Waren." },
                { "CorpoCreateCost", "Kreditkosten zur Gründung einer neuen Korporation." },
                { "CorpoRenameCost", "Kreditkosten zur Umbenennung der Korporation." },
                { "ShipGatherDistance", "Maximale Distanz, in der das Schiff Ressourcen automatisch sammelt." },
                { "ResourceInteractiveRadius", "Interaktionsradius des Spielers mit Ressourcen." },
                { "MiningBaseTime", "Basiszeit zum Abbauen eines Ressourcenknotens (niedrigere Werte = schnellerer Abbau)." },
                { "MiningDefaultPower", "Standard-Abbauleistung, wenn Werkzeuge zerstört sind." },
                { "ResourceRadarDistance", "Entfernung, ab der das Radar Ressourcen erkennt." },
                { "ShipStartSpeed", "Anfangsgeschwindigkeit des Schiffes." },
                { "ShipAcceleration", "Normale Beschleunigungsrate des Schiffes." },
                { "ShipBoostAcceleration", "Beschleunigungsrate bei aktivem Boost." },
                { "ShipManeuverabilityScale", "Globale Manövrierfähigkeitsskala des Schiffes." },
                { "ShipRadarDetectDistance", "Entfernung, in der Wracks und Schiffe vom Radar erkannt werden." },
                { "RadarRotSpeed", "Rotationsgeschwindigkeit des Schiffsradars." },
                { "FTLMinTravelDelay", "Mindestverzögerung vor dem Ausführen eines FTL-Sprungs." },
                { "FTLStaticCost", "Feste Treibstoff-/Energiekosten für einen FTL-Sprung." },
                { "FTLMinDamageRatio", "Mindestschadensverhältnis bei einem instabilen FTL-Sprung." },
                { "FTLMaxDamageRatio", "Maximales Schadensverhältnis bei einem FTL-Sprung." },
                { "PlayerStorageCapacity", "Maximale Inventar-/Lagerkapazität des Spielers." },
                { "PersonaBusInventoryCost", "Kreditkosten pro Inventarplatz bei Nutzung von Transporten." }
            }
        };

        public static string GetText(string key)
        {
            if (Translations.ContainsKey(CurrentLanguage) && Translations[CurrentLanguage].ContainsKey(key))
            {
                return Translations[CurrentLanguage][key];
            }
            return key;
        }

        public static string GetConstantDescription(string paramId)
        {
            if (ConstantDescriptions.ContainsKey(CurrentLanguage) && ConstantDescriptions[CurrentLanguage].TryGetValue(paramId, out string desc))
            {
                return desc;
            }
            return CurrentLanguage == "IT" ? "Nessuna descrizione disponibile per questo parametro." :
                   CurrentLanguage == "DE" ? "Keine Beschreibung für diesen Parameter verfügbar." :
                                             "No description available for this parameter.";
        }

        public static void SetLanguage(string langCode)
        {
            if (CurrentLanguage != langCode)
            {
                CurrentLanguage = langCode;
                OnLanguageChanged?.Invoke();
            }
        }
    }
}