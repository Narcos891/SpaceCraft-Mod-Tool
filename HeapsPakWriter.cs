#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpaceCraft_Mod_Tool
{
    public class HeapsPakWriter
    {
        private class PakNode
        {
            public string Name { get; set; }
            public bool IsDir { get; set; }
            public Dictionary<string, PakNode> Children { get; set; } = new Dictionary<string, PakNode>();
            public byte[] Data { get; set; }
            public int Pos { get; set; }
            public int Size { get; set; }
            public uint Checksum { get; set; }
        }

        public static void CreatePak(Dictionary<string, byte[]> filesToPack, string outputPakPath)
        {
            PakNode root = new PakNode { Name = "", IsDir = true };

            // 1. Costruzione dell'albero dei file e delle cartelle
            foreach (var kvp in filesToPack)
            {
                string relPath = kvp.Key.Replace("\\", "/").Trim('/');
                string[] parts = relPath.Split('/');

                PakNode current = root;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string part = parts[i];
                    if (!current.Children.ContainsKey(part))
                    {
                        current.Children[part] = new PakNode { Name = part, IsDir = true };
                    }
                    current = current.Children[part];
                }

                string fileName = parts[parts.Length - 1];
                byte[] fileBytes = kvp.Value;
                current.Children[fileName] = new PakNode
                {
                    Name = fileName,
                    IsDir = false,
                    Data = fileBytes,
                    Size = fileBytes.Length,
                    Checksum = CalculateCrc32(fileBytes)
                };
            }

            // 2. Calcolo dei percorsi dei dati grezzi
            int currentDataPos = 0;
            List<PakNode> fileNodes = new List<PakNode>();

            void AssignPositions(PakNode node)
            {
                if (!node.IsDir)
                {
                    node.Pos = currentDataPos;
                    currentDataPos += node.Size;
                    fileNodes.Add(node);
                }
                else
                {
                    foreach (var child in node.Children.Values)
                    {
                        AssignPositions(child);
                    }
                }
            }

            AssignPositions(root);

            // 3. Serializzazione dell'header dei metadati
            byte[] SerializeNode(PakNode node)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] nameBytes = Encoding.UTF8.GetBytes(node.Name);
                    ms.WriteByte((byte)nameBytes.Length);
                    ms.Write(nameBytes, 0, nameBytes.Length);

                    if (node.IsDir)
                    {
                        ms.WriteByte(1); // Flag directory = true
                        byte[] childCount = BitConverter.GetBytes(node.Children.Count);
                        ms.Write(childCount, 0, 4);

                        foreach (var child in node.Children.Values)
                        {
                            byte[] childBytes = SerializeNode(child);
                            ms.Write(childBytes, 0, childBytes.Length);
                        }
                    }
                    else
                    {
                        ms.WriteByte(0); // Flag directory = false
                        ms.Write(BitConverter.GetBytes(node.Pos), 0, 4);
                        ms.Write(BitConverter.GetBytes(node.Size), 0, 4);
                        ms.Write(BitConverter.GetBytes(node.Checksum), 0, 4);
                    }

                    return ms.ToArray();
                }
            }

            byte[] headerContent = SerializeNode(root);
            int headerSize = headerContent.Length + 16;

            // 4. Scrittura del file PAK finale
            using (FileStream fs = new FileStream(outputPakPath, FileMode.Create, FileAccess.Write))
            {
                // Magic e dimensioni dell'header
                fs.Write(Encoding.ASCII.GetBytes("PAK\0"), 0, 4);
                fs.Write(BitConverter.GetBytes(headerSize), 0, 4);
                fs.Write(BitConverter.GetBytes(currentDataPos), 0, 4);

                // Struttura ad albero dei nodi
                fs.Write(headerContent, 0, headerContent.Length);

                // Marcatura blocchi dati
                fs.Write(Encoding.ASCII.GetBytes("DATA"), 0, 4);

                // Scrittura dei dati dei file
                foreach (var node in fileNodes)
                {
                    fs.Write(node.Data, 0, node.Data.Length);
                }
            }
        }

        private static uint CalculateCrc32(byte[] data)
        {
            uint crc = 0xffffffff;
            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                crc ^= b;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) != 0)
                        crc = (crc >> 1) ^ 0xedb88320;
                    else
                        crc >>= 1;
                }
            }
            return ~crc;
        }
    }
}