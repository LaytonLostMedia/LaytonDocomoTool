using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5Management.Docomo.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;

namespace Logic.Domain.Level5Management.Docomo
{
    internal class ResourceWriter : IResourceWriter
    {
        private readonly IBinaryFactory _binaryFactory;

        public ResourceWriter(IBinaryFactory binaryFactory)
        {
            _binaryFactory = binaryFactory;
        }

        public void Write(EntryData[] entries, Stream input)
        {
            using IBinaryWriterX writer = _binaryFactory.CreateWriter(input, true);

            EntryData[] resourceEntries = entries.Where(x => x.Name.EndsWith(".jpg") || x.Name.EndsWith(".gif") || x.Name.EndsWith(".mld")).ToArray();
            WriteResources(resourceEntries, writer);

            entries = entries.Except(resourceEntries).ToArray();
            WriteEntries(entries, writer);
        }

        private void WriteResources(EntryData[] entries, IBinaryWriterX writer)
        {
            writer.WriteString("res", Encoding.ASCII, false, false);

            long sizePosition = writer.BaseStream.Position;
            writer.BaseStream.Position += 4;

            writer.Write(entries.Length);

            foreach (EntryData entry in entries)
            {
                writer.Write((short)entry.Name.Length);
                writer.WriteString(entry.Name, Encoding.ASCII, false, false);

                writer.Write((int)entry.Data.Length);
                entry.Data.CopyTo(writer.BaseStream);
            }

            long endPosition = writer.BaseStream.Position;
            writer.BaseStream.Position = sizePosition;

            writer.Write((int)(endPosition - sizePosition - 4));

            writer.BaseStream.Position = endPosition;
        }

        private void WriteEntries(EntryData[] entries, IBinaryWriterX writer)
        {
            var orderedEntries = new EntryData[entries.Length];
            foreach (EntryData entry in entries)
            {
                if (!int.TryParse(Path.GetFileNameWithoutExtension(entry.Name), out int index))
                    continue;

                if (index >= orderedEntries.Length)
                    continue;

                orderedEntries[index] = entry;
            }

            foreach (EntryData entry in orderedEntries)
            {
                writer.WriteString(Path.GetExtension(entry.Name)[1..], Encoding.ASCII, false, false);
                writer.Write((int)entry.Data.Length);

                entry.Data.CopyTo(writer.BaseStream);
            }
        }
    }
}
