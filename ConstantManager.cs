using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SpaceCraft_Mod_Tool
{
    public class ConstantItem
    {
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public string? Id { get; set; }
        public string? DataType { get; set; }
        public string? Value { get; set; }
        public string? DescriptionIt { get; set; }
    }

    public static class ConstantDescriptions
    {
        public static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>()
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
        };
    }

    public class ConstantManager
    {
        private List<ConstantItem> _allItems = new List<ConstantItem>();

        public void LoadFromCdb(List<ConstantItem> rawCdbItems)
        {
            _allItems = rawCdbItems;

            foreach (var item in _allItems)
            {
                item.Category = ResolveHideCategory(item.Id);
                item.SubCategory = ResolveHideSubCategory(item.Id);

                item.DataType = ExtractDataType(item.Value);
                item.Value = ExtractPureNumericValue(item.Value);

                if (item.Id != null && ConstantDescriptions.Descriptions.TryGetValue(item.Id, out string? desc))
                {
                    item.DescriptionIt = desc;
                }
                else
                {
                    item.DescriptionIt = "Nessuna descrizione disponibile per questo parametro.";
                }
            }
        }

        private string ExtractDataType(string? rawVal)
        {
            if (string.IsNullOrEmpty(rawVal)) return "unknown";
            if (rawVal.Contains("float")) return "float";
            if (rawVal.Contains("int")) return "integer";
            if (rawVal.Contains("curve")) return "curve";
            if (rawVal.Contains("gradient")) return "gradient";
            return "text";
        }

        private string ExtractPureNumericValue(string? rawVal)
        {
            if (string.IsNullOrEmpty(rawVal)) return "";
            try
            {
                var jObj = JObject.Parse(rawVal);
                foreach (var prop in jObj.Properties())
                {
                    return prop.Value?.ToString() ?? "";
                }
            }
            catch
            {
                // Se non è un JSON con le graffe, restituisce il valore così com'è
            }
            return rawVal;
        }

        private string ResolveHideCategory(string? id)
        {
            if (string.IsNullOrEmpty(id)) return "General";

            if (id.StartsWith("Craft") || id.Contains("Price") || id.Contains("Cost") || id.StartsWith("Corpo") || id.StartsWith("Sold") || id.StartsWith("Market"))
                return "Economy";
            if (id.StartsWith("Mining") || id.StartsWith("Ore") || id.StartsWith("Resource") || id.StartsWith("Gather"))
                return "Mining & Resources";
            if (id.StartsWith("Ship") || id.StartsWith("Thrust") || id.StartsWith("Radar") || id.StartsWith("Orbit") || id.StartsWith("System"))
                return "Ship & Navigation";
            if (id.StartsWith("FTL") || id.StartsWith("Jump"))
                return "FTL Travel";
            if (id.StartsWith("Persona") || id.StartsWith("Player") || id.StartsWith("XPLife"))
                return "Player & Persona";

            return "General";
        }

        private string ResolveHideSubCategory(string? id)
        {
            if (string.IsNullOrEmpty(id)) return "Parameters";

            if (id.StartsWith("Craft")) return "Crafting";
            if (id.StartsWith("Merchant") || id.StartsWith("Market")) return "Market & Trade";
            if (id.StartsWith("Mining") || id.StartsWith("Ore")) return "Mining Tools";
            if (id.StartsWith("Resource")) return "Resources";
            if (id.StartsWith("Ship")) return "Ship Controls";
            if (id.StartsWith("Radar")) return "Radar Systems";
            if (id.StartsWith("FTL")) return "FTL Drive";
            if (id.StartsWith("Corpo")) return "Corporation";

            return "Configuration";
        }

        public List<string> GetCategories()
        {
            return _allItems.Select(x => x.Category).Where(c => c != null).Cast<string>().Distinct().OrderBy(c => c).ToList();
        }

        public List<string> GetSubCategories(string category)
        {
            return _allItems.Where(x => x.Category == category).Select(x => x.SubCategory).Where(s => s != null).Cast<string>().Distinct().OrderBy(s => s).ToList();
        }

        public List<ConstantItem> GetFilteredItems(string category, string subCategory)
        {
            return _allItems.Where(x => x.Category == category && (string.IsNullOrEmpty(subCategory) || x.SubCategory == subCategory)).ToList();
        }

        public bool UpdateValue(string id, string newNumericVal)
        {
            var item = _allItems.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                item.Value = newNumericVal;
                return true;
            }
            return false;
        }
    }
}