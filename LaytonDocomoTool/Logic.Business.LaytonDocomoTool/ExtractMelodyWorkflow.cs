using System.Security.Cryptography;
using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.Level5Management.Docomo.Contract.Melody;
using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses;
using Logic.Domain.MidiManagement.Contract;
using Logic.Domain.MidiManagement.Contract.DataClasses;

namespace Logic.Business.LaytonDocomoTool
{
    internal class ExtractMelodyWorkflow : IExtractMelodyWorkflow
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly IMelodyParser _melodyParser;
        private readonly IMidiParser _midiParser;
        private readonly IMidiWriter _midiWriter;
        private readonly IMelodyMidiConverter _melodyMidiConverter;

        public ExtractMelodyWorkflow(LaytonDocomoExtractorConfiguration config, IMelodyParser melodyParser, IMidiParser midiParser, IMidiWriter midiWriter,
            IMelodyMidiConverter melodyMidiConverter)
        {
            _config = config;
            _melodyParser = melodyParser;
            _midiParser = midiParser;
            _midiWriter = midiWriter;
            _melodyMidiConverter = melodyMidiConverter;
        }

        public void Work()
        {
            //using Stream fileStream = File.OpenRead(_config.FilePath);
            //using Stream outputStream = File.Create(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(_config.FilePath))!, Path.GetFileNameWithoutExtension(_config.FilePath) + ".mid"));

            var path = @"D:\Users\Kirito\Desktop\Layton_Motdm\tools\psm380";

            using Stream s1 = File.OpenRead(Path.Combine(path, "se_011.mid"));
            using Stream s2 = File.OpenRead(Path.Combine(path, "se_011.mld"));

            MidiData midData = _midiParser.Parse(s1);
            MelodyData mldData = _melodyParser.Parse(s2);

            //MelodyData melodyData = _melodyParser.Parse(fileStream);
            MidiData midiData = _melodyMidiConverter.ConvertMelodyData(mldData);

            //_midiWriter.Write(midiData, outputStream);
        }
    }
}
