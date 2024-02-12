using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Script;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo;
using Logic.Domain.Level5Management.Docomo.Contract;

namespace Logic.Business.LaytonDocomoTool
{
    internal class ExtractTableWorkflow : IExtractTableWorkflow
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly ITableReader _tableReader;
        private readonly IScriptParser _scriptParser;
        private readonly IResourceReader _resourceReader;
        private readonly ILevel5DocomoEventDataConverter _scriptConverter;
        private readonly ILevel5DocomoWhitespaceNormalizer _whitespaceNormalizer;
        private readonly ILevel5DocomoComposer _scriptComposer;

        public ExtractTableWorkflow(LaytonDocomoExtractorConfiguration config, ITableReader tableReader, IScriptParser scriptParser, IResourceReader resourceReader,
            ILevel5DocomoEventDataConverter scriptConverter, ILevel5DocomoWhitespaceNormalizer whitespaceNormalizer, ILevel5DocomoComposer scriptComposer)
        {
            _config = config;
            _tableReader = tableReader;
            _resourceReader = resourceReader;
            _scriptParser = scriptParser;
            _scriptConverter = scriptConverter;
            _whitespaceNormalizer = whitespaceNormalizer;
            _scriptComposer = scriptComposer;
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

            TableData table = _tableReader.Read(indexStream, dataStream);

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
                using StreamWriter scriptWriter = File.CreateText(extractPath);

                EventData[] events = _scriptParser.Parse(entry.Data);

                CodeUnitSyntax codeUnit = _scriptConverter.CreateCodeUnit(events);
                _whitespaceNormalizer.NormalizeCodeUnit(codeUnit);

                string readableScript = _scriptComposer.ComposeCodeUnit(codeUnit);

                scriptWriter.Write(readableScript);
            }
        }

        private void ExtractResourceEntries(EntryData[] entries, string baseDir, string subDir)
        {
            foreach (EntryData entry in entries)
            {
                string extractDir = Path.Combine(baseDir, subDir, Path.GetFileNameWithoutExtension(entry.Name));

                EntryData[] resourceEntries = _resourceReader.ReadEntries(entry.Data);

                foreach (EntryData resourceEntry in resourceEntries)
                {
                    string resourceDir = extractDir;
                    if (Path.GetExtension(resourceEntry.Name) is ".jpg" or ".gif" or ".mld")
                        resourceDir = Path.Combine(resourceDir, "res");

                    Directory.CreateDirectory(resourceDir);

                    string resourcePath = Path.Combine(resourceDir, resourceEntry.Name);

                    using Stream imageStream = File.Create(resourcePath);
                    resourceEntry.Data.CopyTo(imageStream);
                }
            }
        }
    }
}
