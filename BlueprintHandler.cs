using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace SpaceCraft_Mod_Tool
{
    public static class BlueprintHandler
    {
        public static string DecodeBlueprint(string rawInput)
        {
            if (string.IsNullOrEmpty(rawInput)) return string.Empty;

            try
            {
                string base64Data = rawInput.Trim();

                // Estrazione dal wrapper JSON se presente
                if (base64Data.StartsWith("{") && base64Data.Contains("blueprint"))
                {
                    try
                    {
                        var jsonObj = JObject.Parse(base64Data);
                        if (jsonObj["blueprint"] != null)
                        {
                            base64Data = jsonObj["blueprint"]?.ToString()?.Trim() ?? base64Data;
                        }
                    }
                    catch { }
                }

                if (base64Data.StartsWith("{") || base64Data.StartsWith("["))
                {
                    return base64Data;
                }

                byte[] compressedBytes = Convert.FromBase64String(base64Data);
                byte[] decompressedBytes;

                using (var memoryStream = new MemoryStream(compressedBytes))
                {
                    bool skipHeader = compressedBytes.Length > 2 && compressedBytes[0] == 0x78;
                    if (skipHeader)
                    {
                        memoryStream.Position = 2;
                    }

                    using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
                    using (var outStream = new MemoryStream())
                    {
                        deflateStream.CopyTo(outStream);
                        decompressedBytes = outStream.ToArray();
                    }
                }

                // Convertiamo in stringa ripulendo i caratteri binari non stampabili se è un formato HXS/binario
                string textResult = Encoding.UTF8.GetString(decompressedBytes);

                // Se contiene l'intestazione HXS, cerchiamo di estrarre la parte di testo utile o pulita
                if (textResult.Contains("ShipBlueprint") || textResult.Contains("HXS"))
                {
                    // Rimuoviamo i byte di controllo binari iniziali mantenendo la parte leggibile
                    StringBuilder cleanText = new StringBuilder();
                    foreach (char c in textResult)
                    {
                        if (!char.IsControl(c) || c == '\r' || c == '\n' || c == '\t')
                        {
                            cleanText.Append(c);
                        }
                        else
                        {
                            cleanText.Append(' ');
                        }
                    }
                    return cleanText.ToString();
                }

                return textResult;
            }
            catch (Exception ex)
            {
                return $"[Errore di decompressione blueprint: {ex.Message}]";
            }
        }

        public static string EncodeBlueprint(string jsonText)
        {
            if (string.IsNullOrEmpty(jsonText)) return string.Empty;

            try
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(jsonText);

                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(new byte[] { 0x78, 0x9C }, 0, 2);

                    using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
                    {
                        deflateStream.Write(inputBytes, 0, inputBytes.Length);
                    }

                    byte[] compressedBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(compressedBytes);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la ricompressione del blueprint:\n{ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }
    }
}