using Logic.Business.LaytonDocomoTool.InternalContract;

namespace Logic.Business.LaytonDocomoTool
{
    internal class ConfigurationValidator : IConfigurationValidator
    {
        public void Validate(LaytonDocomoExtractorConfiguration config)
        {
            if (config.ShowHelp)
                return;

            ValidateOperation(config);
            ValidateType(config);
            ValidateFilePath(config);
        }

        private void ValidateOperation(LaytonDocomoExtractorConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(config.Operation))
                throw new InvalidOperationException("No operation mode was given. Specify an operation mode by using the -o argument.");

            if (config.Operation != "extract" && config.Operation != "create")
                throw new InvalidOperationException($"The operation mode '{config.Operation}' is not valid. Use -h to see a list of valid operation modes.");
        }

        private void ValidateType(LaytonDocomoExtractorConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(config.Type))
                throw new InvalidOperationException("No data type was given. Specify a data type by using the -t argument.");

            if (config.Type != "jar" && config.Type != "table" && config.Type != "script" && config.Type != "resource" && config.Type != "melody")
                throw new InvalidOperationException($"The data type '{config.Type}' is not valid. Use -h to see a list of valid data types.");
        }

        private void ValidateFilePath(LaytonDocomoExtractorConfiguration config)
        {
            if (string.IsNullOrEmpty(config.FilePath))
                throw new InvalidOperationException("No file path or directory given. Specify one by using the -f argument.");

            if (!File.Exists(config.FilePath) && !Directory.Exists(config.FilePath))
                throw new InvalidOperationException($"Path '{config.FilePath}' does not exist.");

            if (config is { Operation: "extract", Type: "table" })
            {
                string fileName = Path.GetFileName(config.FilePath);
                if (fileName != "0.dat" && fileName != "1.dat")
                    throw new InvalidOperationException("Path has to reference 0.dat or 1.dat for extraction. Both files have to be in the same folder.");
            }

            if (config is { Operation: "create", Type: "jar" })
            {
                string jarFile = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(config.FilePath))!, "jar");
                if (!File.Exists(jarFile))
                    throw new InvalidOperationException($"No jar file found next to {config.FilePath}");
            }
        }
    }
}
