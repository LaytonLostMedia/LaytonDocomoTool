using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Table;
using Logic.Domain.Level5Management.Docomo.Contract.Table.DataClasses;

namespace Logic.Business.LaytonDocomoTool
{
    internal class CreateTableWorkflow : ICreateTableWorkflow
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly ITableWriter _tableWriter;
        private readonly ICreateScriptWorkflow _createScriptWorkflow;
        private readonly ICreateResourceWorkflow _createResourceWorkflow;

        public CreateTableWorkflow(LaytonDocomoExtractorConfiguration config, ITableWriter tableWriter,
            ICreateScriptWorkflow createScriptWorkflow, ICreateResourceWorkflow createResourceWorkflow)
        {
            _config = config;
            _tableWriter = tableWriter;
            _createScriptWorkflow = createScriptWorkflow;
            _createResourceWorkflow = createResourceWorkflow;
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

            _tableWriter.Write(tableData, dataOutput, indexOutput);
        }

        private TableData CreateTableData()
        {
            var result = new TableData
            {
                Images = CreateTableEntries("img", "*.jpg"),
                Animations = CreateTableEntries("gif", "*.gif"),
                Melodies = CreateTableEntries("mld", "*.mld"),
                Texts = CreateTableEntries("txt", "*.dat"),
                Scripts = CreateScriptEntries("scripts"),
                Resources = CreateResourceEntries("res")
            };

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
            var names = new HashSet<string>();

            foreach (string filePath in Directory.GetFiles(Path.Combine(_config.FilePath, subDir), "*.txt"))
            {
                string name = Path.GetFileNameWithoutExtension(filePath) + ".dat";

                names.Add(name);
                result.Add(new EntryData
                {
                    Name = name,
                    Data = CreateScriptStream(filePath)
                });
            }

            EntryData[] scriptEntries = CreateTableEntries("scripts", "*.dat");
            foreach (EntryData resourceEntry in scriptEntries)
            {
                if (names.Contains(resourceEntry.Name))
                    continue;

                names.Add(resourceEntry.Name);
                result.Add(resourceEntry);
            }

            return result.ToArray();
        }

        private Stream CreateScriptStream(string scriptFilePath)
        {
            try
            {
                Stream outputStream = new MemoryStream();

                _createScriptWorkflow.Work(scriptFilePath, outputStream);

                outputStream.Position = 0;
                return outputStream;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error processing script file {scriptFilePath}.");
                throw e;
            }
        }

        private EntryData[] CreateResourceEntries(string subDir)
        {
            var result = new List<EntryData>();
            var names = new HashSet<string>();

            foreach (string directoryPath in Directory.GetDirectories(Path.Combine(_config.FilePath, subDir), "*", SearchOption.TopDirectoryOnly))
            {
                string name = Path.GetFileName(directoryPath) + ".dat";

                names.Add(name);
                result.Add(new EntryData
                {
                    Name = name,
                    Data = CreateResourceStream(directoryPath)
                });
            }

            EntryData[] resourceEntries = CreateTableEntries("res", "*.dat");
            foreach (EntryData resourceEntry in resourceEntries)
            {
                if (names.Contains(resourceEntry.Name))
                    continue;

                names.Add(resourceEntry.Name);
                result.Add(resourceEntry);
            }

            return result.ToArray();
        }

        private Stream CreateResourceStream(string resourceDir)
        {
            try
            {
                Stream outputStream = new MemoryStream();

                _createResourceWorkflow.Work(resourceDir, outputStream);

                outputStream.Position = 0;
                return outputStream;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error processing resource directory {resourceDir}.");
                throw e;
            }
        }
    }
}
