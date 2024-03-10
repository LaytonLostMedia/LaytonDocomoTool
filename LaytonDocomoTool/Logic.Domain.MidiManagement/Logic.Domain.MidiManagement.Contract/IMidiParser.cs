using Logic.Domain.MidiManagement.Contract.DataClasses;

namespace Logic.Domain.MidiManagement.Contract
{
    public interface IMidiParser
    {
        MidiData Parse(Stream input);
        MidiData Parse(MidiChunkData[] chunks);
    }
}
