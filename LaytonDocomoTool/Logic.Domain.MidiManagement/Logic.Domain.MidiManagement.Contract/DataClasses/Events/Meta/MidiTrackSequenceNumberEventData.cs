using Logic.Domain.MidiManagement.Contract.DataClasses.Events;

namespace Logic.Domain.MidiManagement.Contract.DataClasses.Events.Meta
{
    public sealed class MidiTrackSequenceNumberEventData : MidiTrackMetaEventData
    {
        public int SequenceNumber { get; set; }
    }
}
