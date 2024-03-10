using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Melody
{
    public interface IMelodyParser
    {
        MelodyData Parse(Stream inputStream);
        MelodyData Parse(MelodyChunkData[] chunks);
    }
}
