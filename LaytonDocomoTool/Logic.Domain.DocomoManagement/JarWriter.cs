using System.IO.Compression;
using Logic.Business.LaytonDocomoTool.Contract.DataClasses;
using Logic.Domain.DocomoManagement.Contract;

namespace Logic.Domain.DocomoManagement
{
    internal class JarWriter : IJarWriter
    {
        public void Write(JarArchiveEntry[] entries, Stream output)
        {
            using var zipArchive = new ZipArchive(output, ZipArchiveMode.Create, true);

            foreach (JarArchiveEntry entry in entries)
            {
                ZipArchiveEntry zipEntry = zipArchive.CreateEntry(entry.Path, CompressionLevel.Optimal);

                using Stream entryStream = zipEntry.Open();
                entry.Data.CopyTo(entryStream);
            }
        }
    }
}
