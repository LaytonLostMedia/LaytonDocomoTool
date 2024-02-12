using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract;

namespace Logic.Business.LaytonDocomoTool
{
    internal class ExtractJarWorkflow : IExtractJarWorkflow
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly IGameReader _gameReader;
        private readonly IExtractTableWorkflow _extractTableWorkflow;

        public ExtractJarWorkflow(LaytonDocomoExtractorConfiguration config, IGameReader gameReader, IExtractTableWorkflow extractTableWorkflow)
        {
            _config = config;
            _gameReader = gameReader;
            _extractTableWorkflow = extractTableWorkflow;
        }

        public void Work()
        {
            using Stream fileStream = File.OpenRead(_config.FilePath);
            GameData gameData = _gameReader.Read(fileStream);

            string extractDir = Path.Combine(Path.GetDirectoryName(_config.FilePath)!, "tbl");

            _extractTableWorkflow.Work(gameData.Table, extractDir);
        }
    }
}
