using Logic.Business.LaytonDocomoTool.Contract;
using Logic.Business.LaytonDocomoTool.InternalContract;

namespace Logic.Business.LaytonDocomoTool
{
    internal class LaytonDocomoToolManagement : ILaytonDocomoToolManagement
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly IConfigurationValidator _configValidator;
        private readonly IExtractJarWorkflow _extractJarWorkflow;
        private readonly IExtractTableWorkflow _extractTableWorkflow;
        private readonly IExtractScriptWorkflow _extractScriptWorkflow;
        private readonly IExtractResourceWorkflow _extractResourceWorkflow;
        private readonly IInjectJarWorkflow _injectJarWorkflow;
        private readonly ICreateTableWorkflow _createTableWorkflow;
        private readonly ICreateScriptWorkflow _createScriptWorkflow;
        private readonly ICreateResourceWorkflow _createResourceWorkflow;

        public LaytonDocomoToolManagement(LaytonDocomoExtractorConfiguration config, IConfigurationValidator configValidator,
            IExtractJarWorkflow extractJarWorkflow, IExtractTableWorkflow extractTableWorkflow, IExtractScriptWorkflow extractScriptWorkflow, IExtractResourceWorkflow extractResourceWorkflow,
            IInjectJarWorkflow injectJarWorkflow, ICreateTableWorkflow createTableWorkflow, ICreateScriptWorkflow createScriptWorkflow, ICreateResourceWorkflow createResourceWorkflow)
        {
            _config = config;
            _configValidator = configValidator;
            _extractJarWorkflow = extractJarWorkflow;
            _extractTableWorkflow = extractTableWorkflow;
            _extractScriptWorkflow = extractScriptWorkflow;
            _extractResourceWorkflow = extractResourceWorkflow;
            _injectJarWorkflow = injectJarWorkflow;
            _createTableWorkflow = createTableWorkflow;
            _createScriptWorkflow = createScriptWorkflow;
            _createResourceWorkflow = createResourceWorkflow;
        }

        public int Execute()
        {
            if (_config.ShowHelp || Environment.GetCommandLineArgs().Length <= 1)
            {
                PrintHelp();
                return 0;
            }

            if (!IsValidConfig())
            {
                PrintHelp();
                return 1;
            }

            switch (_config.Operation)
            {
                case "extract":
                    Extract();
                    break;

                case "create":
                    Create();
                    break;
            }

            return 0;
        }

        private bool IsValidConfig()
        {
            try
            {
                _configValidator.Validate(_config);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Input parameters are incorrect: {GetInnermostException(e).Message}");
                Console.WriteLine();

                return false;
            }
        }

        private void Extract()
        {
            switch (_config.Type)
            {
                case "jar":
                    ExtractJar();
                    break;

                case "table":
                    ExtractTable();
                    break;

                case "script":
                    ExtractScript();
                    break;

                case "resource":
                    ExtractResource();
                    break;
            }
        }

        private void Create()
        {
            switch (_config.Type)
            {
                case "jar":
                    InjectJar();
                    break;

                case "table":
                    CreateTable();
                    break;

                case "script":
                    CreateScript();
                    break;

                case "resource":
                    CreateResource();
                    break;
            }
        }

        private void ExtractJar()
        {
            try
            {
                _extractJarWorkflow.Work();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not extract jar: {GetInnermostException(e).Message}");
            }
        }

        private void ExtractTable()
        {
            try
            {
                _extractTableWorkflow.Work();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not extract tbl: {GetInnermostException(e).Message}");
            }
        }

        private void ExtractScript()
        {
            try
            {
                _extractScriptWorkflow.Work();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not extract script: {GetInnermostException(e).Message}");
            }
        }

        private void ExtractResource()
        {
            try
            {
                _extractResourceWorkflow.Work();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not extract resource: {GetInnermostException(e).Message}");
            }
        }

        private void InjectJar()
        {
            try
            {
                _injectJarWorkflow.Work();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not create tbl: {GetInnermostException(e).Message}");
            }
        }

        private void CreateTable()
        {
            try
            {
                _createTableWorkflow.Work();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not create tbl: {GetInnermostException(e).Message}");
            }
        }

        private void CreateScript()
        {
            try
            {
                _createScriptWorkflow.Work();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not create script: {GetInnermostException(e).Message}");
            }
        }

        private void CreateResource()
        {
            try
            {
                _createResourceWorkflow.Work();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not create resource: {GetInnermostException(e).Message}");
            }
        }

        private void PrintHelp()
        {
            Console.WriteLine("Following commands exist:");
            Console.WriteLine("  -h, --help\t\tShows this help message.");
            Console.WriteLine("  -o, --operation\tThe operation to execute.");
            Console.WriteLine("    Valid operations are: 'extract', 'create'");
            Console.WriteLine("    Default: 'extract'");
            Console.WriteLine("  -t, --type\t\tThe type of file to process.");
            Console.WriteLine("    Valid types are: 'jar', 'table', 'script', 'resource'");
            Console.WriteLine("    Default: 'jar'");
            Console.WriteLine("  -f, --file\t\tThe file path or directory to use.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine($"  Extract data from jar: {Environment.ProcessPath} -f Path/To/File.jar");
            Console.WriteLine($"  Extract data from tbl dat's: {Environment.ProcessPath} -t table -f Path/To/0.dat");
            Console.WriteLine($"  Extract data from script: {Environment.ProcessPath} -t script -f Path/To/event_xxx_or_room_xx.dat");
            Console.WriteLine($"  Extract data from resource: {Environment.ProcessPath} -t resource -f Path/To/xxx_or_cxx_orbgoj_xxx.dat");
            Console.WriteLine($"  Create script from text: {Environment.ProcessPath} -o create -t script -f Path/To/event_xxx_or_room_xx.txt");
            Console.WriteLine($"  Create tbl dat's from directory: {Environment.ProcessPath} -o create -t table -f Path/To/Files");
        }

        private Exception GetInnermostException(Exception e)
        {
            while (e.InnerException != null)
                e = e.InnerException;

            return e;
        }
    }
}
