using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.MidiManagement.Contract;
using Logic.Domain.MidiManagement.Contract.DataClasses;

namespace Logic.Business.LaytonDocomoTool
{
    internal class CreateMelodyWorkflow : ICreateMelodyWorkflow
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly IMidiParser _midiParser;

        public CreateMelodyWorkflow(LaytonDocomoExtractorConfiguration config, IMidiParser midiParser)
        {
            _config = config;
            _midiParser = midiParser;
        }

        public void Work()
        {
            string outputPath = Path.Combine(Path.GetDirectoryName(_config.FilePath)!, Path.GetFileNameWithoutExtension(_config.FilePath) + ".mid");
            using Stream outputStream = File.Create(outputPath);

            Work(_config.FilePath, outputStream);
        }

        public void Work(string midiFilePath, Stream output)
        {
            using Stream midiStream = File.OpenRead(midiFilePath);

            MidiData midiData = _midiParser.Parse(midiStream);
        }
    }
}
