using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract.DataClasses;
using Logic.Domain.MidiManagement.Contract;
using Logic.Domain.MidiManagement.Contract.DataClasses;

namespace Logic.Domain.MidiManagement
{
    internal class MidiWriter : IMidiWriter
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly IMidiComposer _midiComposer;

        public MidiWriter(IBinaryFactory binaryFactory, IMidiComposer midiComposer)
        {
            _binaryFactory = binaryFactory;
            _midiComposer = midiComposer;
        }

        public void Write(MidiData midiData, Stream output)
        {
            MidiChunkData[] chunks = _midiComposer.Compose(midiData);

            Write(chunks, output);
        }

        public void Write(MidiChunkData[] chunks, Stream output)
        {
            using IBinaryWriterX writer = _binaryFactory.CreateWriter(output, true, ByteOrder.BigEndian);

            foreach (MidiChunkData chunk in chunks)
            {
                writer.WriteString(chunk.Identifier, Encoding.ASCII, false, false);
                writer.Write((int)chunk.Data.Length);

                chunk.Data.CopyTo(output);
            }
        }
    }
}
