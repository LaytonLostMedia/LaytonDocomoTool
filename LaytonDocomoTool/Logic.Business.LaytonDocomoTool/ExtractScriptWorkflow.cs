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
        private readonly IScriptReader _scriptReader;
        private readonly IScriptParser _scriptParser;
        private readonly IEncodingProvider _encodingProvider;
        private readonly ILevel5DocomoEventDataConverter _scriptConverter;
        private readonly ILevel5DocomoWhitespaceNormalizer _whitespaceNormalizer;
        private readonly ILevel5DocomoComposer _scriptComposer;

        public ExtractScriptWorkflow(LaytonDocomoExtractorConfiguration config, IScriptReader scriptReader, IScriptParser scriptParser, IEncodingProvider encodingProvider,
            ILevel5DocomoEventDataConverter scriptConverter, ILevel5DocomoWhitespaceNormalizer whitespaceNormalizer, ILevel5DocomoComposer scriptComposer)
        {
            _config = config;
            _scriptReader = scriptReader;
            _scriptParser = scriptParser;
            _encodingProvider = encodingProvider;
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

            EventEntryData[] entries = _scriptReader.Read(scriptStream);

            EventData[] events;
            try
            {
                events = _scriptParser.Parse(entries, _encodingProvider.GetEncoding());
            }
            catch
            {
                Console.WriteLine($"Error fully extracting script file {Path.GetFileNameWithoutExtension(extractFilePath)}. Try shallow decompilation.");

                events = _scriptParser.Parse(entries, _encodingProvider.GetEncoding(), false);
            }

            CodeUnitSyntax codeUnit = _scriptConverter.CreateCodeUnit(events);
            _whitespaceNormalizer.NormalizeCodeUnit(codeUnit);

            string readableScript = _scriptComposer.ComposeCodeUnit(codeUnit);

            scriptWriter.Write(readableScript);
        }
    }
}
