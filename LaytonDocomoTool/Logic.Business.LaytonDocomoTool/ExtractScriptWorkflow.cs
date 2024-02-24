using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Script;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;

namespace Logic.Business.LaytonDocomoTool
{
    internal class ExtractScriptWorkflow : IExtractScriptWorkflow
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly IScriptParser _scriptParser;
        private readonly ILevel5DocomoEventDataConverter _scriptConverter;
        private readonly ILevel5DocomoWhitespaceNormalizer _whitespaceNormalizer;
        private readonly ILevel5DocomoComposer _scriptComposer;

        public ExtractScriptWorkflow(LaytonDocomoExtractorConfiguration config, IScriptParser scriptParser,
            ILevel5DocomoEventDataConverter scriptConverter, ILevel5DocomoWhitespaceNormalizer whitespaceNormalizer, ILevel5DocomoComposer scriptComposer)
        {
            _config = config;
            _scriptParser = scriptParser;
            _scriptConverter = scriptConverter;
            _whitespaceNormalizer = whitespaceNormalizer;
            _scriptComposer = scriptComposer;
        }

        public void Work()
        {
            using Stream fileStream = File.OpenRead(_config.FilePath);

            string extractPath = Path.Combine(Path.GetDirectoryName(_config.FilePath)!, Path.GetFileNameWithoutExtension(_config.FilePath) + ".txt");

            Work(fileStream, extractPath);
        }

        public void Work(Stream scriptStream, string extractFilePath)
        {
            using StreamWriter scriptWriter = File.CreateText(extractFilePath);

            EventData[] events = _scriptParser.Parse(scriptStream);

            CodeUnitSyntax codeUnit = _scriptConverter.CreateCodeUnit(events);
            _whitespaceNormalizer.NormalizeCodeUnit(codeUnit);

            string readableScript = _scriptComposer.ComposeCodeUnit(codeUnit);

            scriptWriter.Write(readableScript);
        }
    }
}
