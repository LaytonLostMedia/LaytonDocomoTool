using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Table;
using Logic.Domain.Level5Management.Docomo.Contract.Table.DataClasses;

namespace Logic.Business.LaytonDocomoTool
{
    internal class ExtractTableWorkflow : IExtractTableWorkflow
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly ITableParser _tableReader;
        private readonly IExtractScriptWorkflow _extractScriptWorkflow;
        private readonly IExtractResourceWorkflow _extractResourceWorkflow;

        public ExtractTableWorkflow(LaytonDocomoExtractorConfiguration config, ITableParser tableReader,
            IExtractScriptWorkflow extractScriptWorkflow, IExtractResourceWorkflow extractResourceWorkflow)
        {
            _config = config;
            _tableReader = tableReader;
            _extractScriptWorkflow = extractScriptWorkflow;
            _extractResourceWorkflow = extractResourceWorkflow;
        }

        public void Work()
        {
            string extractDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(_config.FilePath))!, "tbl");

            string indexFilePath;
            string dataFilePath;

            if (Path.GetFileName(_config.FilePath) == "0.dat")
            {
                indexFilePath = _config.FilePath;
                dataFilePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(_config.FilePath))!, "1.dat");
            }
            else
            {
                indexFilePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(_config.FilePath))!, "0.dat");
                dataFilePath = _config.FilePath;
            }

            using Stream indexStream = File.OpenRead(indexFilePath);
            using Stream dataStream = File.OpenRead(dataFilePath);

            TableData table = _tableReader.Parse(indexStream, dataStream);

            Work(table, extractDir);
        }

        public void Work(TableData tableData, string extractDir)
        {
            ExtractTableEntries(tableData.Images, extractDir, "img");
            ExtractTableEntries(tableData.Animations, extractDir, "gif");
            ExtractTableEntries(tableData.Melodies, extractDir, "mld");
            ExtractTableEntries(tableData.Texts, extractDir, "txt");

            if (_config.IsShallow)
            {
                ExtractTableEntries(tableData.Scripts, extractDir, "scripts");
                ExtractTableEntries(tableData.Resources, extractDir, "res");

                return;
            }

            ExtractScriptEntries(tableData.Scripts, extractDir, "scripts");
            ExtractResourceEntries(tableData.Resources, extractDir, "res");
        }

        private void ExtractTableEntries(EntryData[] entries, string baseDir, string subDir)
        {
            foreach (EntryData entry in entries)
            {
                string extractDir = Path.Combine(baseDir, subDir);
                Directory.CreateDirectory(extractDir);

                string extractPath = Path.Combine(extractDir, entry.Name);

                using Stream imageStream = File.Create(extractPath);
                entry.Data.CopyTo(imageStream);
            }
        }

        private void ExtractScriptEntries(EntryData[] entries, string baseDir, string subDir)
        {
            string extractDir = Path.Combine(baseDir, subDir);
            Directory.CreateDirectory(extractDir);

            foreach (EntryData entry in entries)
            {
                string extractPath = Path.Combine(extractDir, Path.GetFileNameWithoutExtension(entry.Name) + ".txt");

                try
                {
                    _extractScriptWorkflow.Work(entry.Data, extractPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error further extracting script file {entry.Name}: {e.Message}");

                    using Stream extractStream = File.Create(Path.Combine(extractDir, entry.Name));

                    entry.Data.Position = 0;
                    entry.Data.CopyTo(extractStream);
                }
            }
        }

        private void ExtractResourceEntries(EntryData[] entries, string baseDir, string subDir)
        {
            foreach (EntryData entry in entries)
            {
                string extractDir = Path.Combine(baseDir, subDir, Path.GetFileNameWithoutExtension(entry.Name));

                try
                {
                    _extractResourceWorkflow.Work(entry.Data, extractDir);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error further extracting resource file {entry.Name}: {e.Message}");

                    using Stream extractStream = File.Create(Path.Combine(baseDir, subDir, entry.Name));

                    entry.Data.Position = 0;
                    entry.Data.CopyTo(extractStream);
                }
            }
        }
    }
}
