#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SpaceCraft_Mod_Tool
{
    public class ConstantItem
    {
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Id { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }
        public string DescriptionIt => LanguageManager.GetConstantDescription(Id);
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
            }
        }

        private string ExtractDataType(string rawVal)
        {
            if (string.IsNullOrEmpty(rawVal)) return "unknown";
            if (rawVal.Contains("float")) return "float";
            if (rawVal.Contains("int")) return "integer";
            if (rawVal.Contains("curve")) return "curve";
            if (rawVal.Contains("gradient")) return "gradient";
            return "text";
        }

        private string ExtractPureNumericValue(string rawVal)
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
            catch { }
            return rawVal;
        }

        private string ResolveHideCategory(string id)
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

        private string ResolveHideSubCategory(string id)
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

        public List<string> GetCategories() => _allItems.Select(x => x.Category).Where(c => c != null).Distinct().OrderBy(c => c).ToList();

        public List<string> GetSubCategories(string category) => _allItems.Where(x => x.Category == category).Select(x => x.SubCategory).Where(s => s != null).Distinct().OrderBy(s => s).ToList();

        public List<ConstantItem> GetFilteredItems(string category, string subCategory) => _allItems.Where(x => x.Category == category && (string.IsNullOrEmpty(subCategory) || x.SubCategory == subCategory)).ToList();

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