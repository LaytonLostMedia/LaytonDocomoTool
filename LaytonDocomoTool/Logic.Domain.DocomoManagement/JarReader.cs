using System.IO.Compression;
using Logic.Business.LaytonDocomoTool.Contract.DataClasses;
using Logic.Domain.DocomoManagement.Contract;

namespace Logic.Domain.DocomoManagement
{
    internal class JarReader : IJarReader
    {
        public JarArchiveEntry[] Read(Stream input)
        {
            var zipArchive = new ZipArchive(input, ZipArchiveMode.Read);

            var result = new JarArchiveEntry[zipArchive.Entries.Count];

            var i = 0;
            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                using Stream zipFileStream = entry.Open();

                var ms = new MemoryStream();
                zipFileStream.CopyTo(ms);

                ms.Position = 0;
                result[i++] = new JarArchiveEntry { Data = ms, Path = entry.FullName };
            }

            return result;
        }
    }
}
