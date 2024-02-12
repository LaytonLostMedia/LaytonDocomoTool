using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Script;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo;

namespace Logic.Business.LaytonDocomoTool
{
    internal class CreateTableWorkflow : ICreateTableWorkflow
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly ITableWriter _tableWriter;
        private readonly ILevel5DocomoParser _scriptParser;
        private readonly ILevel5DocomoCodeUnitConverter _codeUnitConverter;
        private readonly IScriptComposer _scriptComposer;
        private readonly IScriptWriter _scriptWriter;
        private readonly IResourceWriter _resourceWriter;

        public CreateTableWorkflow(LaytonDocomoExtractorConfiguration config, ITableWriter tableWriter, ILevel5DocomoParser scriptParser,
            ILevel5DocomoCodeUnitConverter codeUnitConverter, IScriptComposer scriptComposer, IScriptWriter scriptWriter,
            IResourceWriter resourceWriter)
        {
            _config = config;
            _tableWriter = tableWriter;
            _scriptParser = scriptParser;
            _codeUnitConverter = codeUnitConverter;
            _scriptComposer = scriptComposer;
            _scriptWriter = scriptWriter;
            _resourceWriter = resourceWriter;
        }

        public void Work()
        {
            string createDir = Path.GetDirectoryName(Path.GetFullPath(_config.FilePath))!;

            string indexPath = Path.Combine(createDir, "0.dat");
            string dataPath = Path.Combine(createDir, "1.dat");

            using Stream indexStream = File.Create(indexPath);
            using Stream dataStream = File.Create(dataPath);

            Work(indexStream, dataStream);
        }

        public void Work(Stream indexOutput, Stream dataOutput)
        {
            TableData tableData = CreateTableData();

            _tableWriter.Write(tableData, indexOutput, dataOutput);
        }

        private TableData CreateTableData()
        {
            var result = new TableData
            {
                Images = CreateTableEntries("img", "*.jpg"),
                Animations = CreateTableEntries("gif", "*.gif"),
                Melodies = CreateTableEntries("mld", "*.mld"),
                Texts = CreateTableEntries("txt", "*.dat")
            };

            if (_config.IsShallow)
            {
                result.Scripts = CreateTableEntries("scripts", "*.dat");
                result.Resources = CreateTableEntries("res", "*.dat");
            }
            else
            {
                result.Scripts = CreateScriptEntries("scripts");
                result.Resources = CreateResourceEntries("res");
            }

            return result;
        }

        private EntryData[] CreateTableEntries(string subDir, string searchFilter)
        {
            var result = new List<EntryData>();

            foreach (string filePath in Directory.GetFiles(Path.Combine(_config.FilePath, subDir), searchFilter))
            {
                result.Add(new EntryData
                {
                    Name = Path.GetFileName(filePath),
                    Data = File.OpenRead(filePath)
                });
            }

            return result.ToArray();
        }

        private EntryData[] CreateScriptEntries(string subDir)
        {
            var result = new List<EntryData>();

            foreach (string filePath in Directory.GetFiles(Path.Combine(_config.FilePath, subDir), "*.txt"))
            {
                string scriptText = File.ReadAllText(filePath);

                result.Add(new EntryData
                {
                    Name = Path.GetFileNameWithoutExtension(filePath) + ".dat",
                    Data = CreateScriptStream(scriptText)
                });
            }

            return result.ToArray();
        }

        private Stream CreateScriptStream(string scriptText)
        {
            CodeUnitSyntax codeUnit = _scriptParser.ParseCodeUnit(scriptText);
            EventData[] events = _codeUnitConverter.CreateEvents(codeUnit);
            EventEntryData[] eventEntries = _scriptComposer.Compose(events);

            Stream outputStream = new MemoryStream();

            _scriptWriter.Write(eventEntries, outputStream);

            outputStream.Position = 0;
            return outputStream;
        }

        private EntryData[] CreateResourceEntries(string subDir)
        {
            var result = new List<EntryData>();

            foreach (string directoryPath in Directory.GetDirectories(Path.Combine(_config.FilePath, subDir), "*", SearchOption.TopDirectoryOnly))
            {
                result.Add(new EntryData
                {
                    Name = Path.GetFileName(directoryPath) + ".dat",
                    Data = CreateResourceStream(directoryPath)
                });
            }

            return result.ToArray();
        }

        private Stream CreateResourceStream(string resourceDir)
        {
            var result = new List<EntryData>();

            foreach (string filePath in Directory.GetFiles(resourceDir, "*", SearchOption.AllDirectories))
            {
                result.Add(new EntryData
                {
                    Name = Path.GetFileName(filePath),
                    Data = File.OpenRead(filePath)
                });
            }

            Stream outputStream = new MemoryStream();

            _resourceWriter.Write(result.ToArray(), outputStream);

            outputStream.Position = 0;
            return outputStream;
        }
    }
}
