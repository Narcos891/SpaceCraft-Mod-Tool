#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SpaceCraft_Mod_Tool
{
    public class PakFileEntry
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsDir { get; set; }
        public long Pos { get; set; }
        public int Size { get; set; }
        public uint Checksum { get; set; }
    }

    public class HeapsPakReader
    {
        private string _pakPath;
        public int Version { get; private set; }
        public int HeaderSize { get; private set; }
        public int DataSize { get; private set; }
        public List<PakFileEntry> Files { get; private set; } = new List<PakFileEntry>();

        public HeapsPakReader(string pakPath)
        {
            _pakPath = pakPath;
            ParseHeader();
        }

        private void ParseHeader()
        {
            if (!File.Exists(_pakPath))
                throw new FileNotFoundException($"File PAK non trovato: {_pakPath}");

            using (FileStream f = new FileStream(_pakPath, FileMode.Open, FileAccess.Read))
            {
                byte[] magic = new byte[4];
                f.Read(magic, 0, 4);

                // Verifica magic "PAK\0" o "PAK"
                if (magic[0] != 'P' || magic[1] != 'A' || magic[2] != 'K')
                {
                    throw new InvalidDataError("Formato non valido: Magic atteso 'PAK'");
                }

                Version = magic[3];

                byte[] headerSizeBuf = new byte[4];
                byte[] dataSizeBuf = new byte[4];
                f.Read(headerSizeBuf, 0, 4);
                f.Read(dataSizeBuf, 0, 4);

                HeaderSize = BitConverter.ToInt32(headerSizeBuf, 0);
                DataSize = BitConverter.ToInt32(dataSizeBuf, 0);

                int treeBytesLen = HeaderSize - 16;
                byte[] headerBytes = new byte[treeBytesLen];
                f.Read(headerBytes, 0, treeBytesLen);

                byte[] dataTag = new byte[4];
                f.Read(dataTag, 0, 4); // Salta il tag "DATA"

                using (MemoryStream bio = new MemoryStream(headerBytes))
                {
                    PakFileEntry ReadNode(Stream s, string currentPath)
                    {
                        int nameLen = s.ReadByte();
                        if (nameLen == -1) return null;

                        byte[] nameBuf = new byte[nameLen];
                        s.Read(nameBuf, 0, nameLen);
                        string name = Encoding.UTF8.GetString(nameBuf);

                        int flags = s.ReadByte();
                        bool isDir = (flags & 1) != 0;

                        string fullPath = string.IsNullOrEmpty(currentPath) ? name : $"{currentPath}/{name}";

                        if (isDir)
                        {
                            byte[] countBuf = new byte[4];
                            s.Read(countBuf, 0, 4);
                            int childCount = BitConverter.ToInt32(countBuf, 0);

                            for (int i = 0; i < childCount; i++)
                            {
                                ReadNode(s, fullPath);
                            }
                            return new PakFileEntry { Name = name, Path = fullPath, IsDir = true };
                        }
                        else
                        {
                            long dataPos = 0;
                            if ((flags & 2) != 0)
                            {
                                byte[] posBuf = new byte[8];
                                s.Read(posBuf, 0, 8);
                                dataPos = (long)BitConverter.ToDouble(posBuf, 0);
                            }
                            else
                            {
                                byte[] posBuf = new byte[4];
                                s.Read(posBuf, 0, 4);
                                dataPos = BitConverter.ToInt32(posBuf, 0);
                            }

                            byte[] sizeBuf = new byte[4];
                            byte[] checksumBuf = new byte[4];
                            s.Read(sizeBuf, 0, 4);
                            s.Read(checksumBuf, 0, 4);

                            int size = BitConverter.ToInt32(sizeBuf, 0);
                            uint checksum = BitConverter.ToUInt32(checksumBuf, 0);

                            var entry = new PakFileEntry
                            {
                                Name = name,
                                Path = fullPath,
                                IsDir = false,
                                Pos = dataPos,
                                Size = size,
                                Checksum = checksum
                            };
                            Files.Add(entry);
                            return entry;
                        }
                    }

                    ReadNode(bio, "");
                }
            }
        }

        public byte[] ExtractFile(PakFileEntry entry)
        {
            long absPos = HeaderSize + entry.Pos;
            using (FileStream f = new FileStream(_pakPath, FileMode.Open, FileAccess.Read))
            {
                f.Seek(absPos, SeekOrigin.Begin);
                byte[] buffer = new byte[entry.Size];
                f.Read(buffer, 0, entry.Size);
                return buffer;
            }
        }

        public PakFileEntry FindFile(string filename)
        {
            foreach (var file in Files)
            {
                if (file.Name.Equals(filename, StringComparison.OrdinalIgnoreCase) ||
                    file.Path.EndsWith(filename, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }
            return null;
        }
    }

    public class InvalidDataError : Exception
    {
        public InvalidDataError(string message) : base(message) { }
    }
}