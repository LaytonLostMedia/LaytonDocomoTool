using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Script;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;

namespace Logic.Business.LaytonDocomoTool
{
    internal class CreateScriptWorkflow : ICreateScriptWorkflow
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly ILevel5DocomoParser _scriptParser;
        private readonly ILevel5DocomoCodeUnitConverter _codeUnitConverter;
        private readonly IScriptComposer _scriptComposer;
        private readonly IScriptWriter _scriptWriter;
        private readonly IEncodingProvider _encodingProvider;

        public CreateScriptWorkflow(LaytonDocomoExtractorConfiguration config, ILevel5DocomoParser scriptParser,
            ILevel5DocomoCodeUnitConverter codeUnitConverter, IScriptComposer scriptComposer, IScriptWriter scriptWriter,IEncodingProvider encodingProvider)
        {
            _config = config;
            _scriptParser = scriptParser;
            _codeUnitConverter = codeUnitConverter;
            _scriptComposer = scriptComposer;
            _scriptWriter = scriptWriter;
            _encodingProvider = encodingProvider;
        }

        public void Work()
        {
            string outputPath = Path.Combine(Path.GetDirectoryName(_config.FilePath)!, Path.GetFileNameWithoutExtension(_config.FilePath) + ".dat");
            using Stream outputStream = File.Create(outputPath);

            Work(_config.FilePath, outputStream);
        }

        public void Work(string scriptFilePath, Stream output)
        {
            string scriptText = File.ReadAllText(scriptFilePath);

            CodeUnitSyntax codeUnit = _scriptParser.ParseCodeUnit(scriptText);
            EventData[] events = _codeUnitConverter.CreateEvents(codeUnit);
            EventEntryData[] eventEntries = _scriptComposer.Compose(events, _encodingProvider.GetEncoding());

            _scriptWriter.Write(eventEntries, output);
        }
    }
}
