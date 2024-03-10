namespace Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses
{
    public class MelodyData
    {
        public int MajorType { get; set; }
        public int MinorType { get; set; }
        public MelodyMetaData MetaData { get; set; }
        public MelodyTrackData[] Tracks { get; set; }
    }
}
