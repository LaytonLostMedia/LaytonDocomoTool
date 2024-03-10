namespace Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses
{
    public class MelodyMetaData
    {
        public string? Title { get; set; }
        public string? Copyright { get; set; }
        public string? Version { get; set; }
        public string? Date { get; set; }
        public string? Protection { get; set; }
        public string? Support { get; set; }
        public byte? SorcValue { get; set; }
        public short? NoteValue { get; set; }
        public byte[]? ExtraData { get; set; }
    }
}
