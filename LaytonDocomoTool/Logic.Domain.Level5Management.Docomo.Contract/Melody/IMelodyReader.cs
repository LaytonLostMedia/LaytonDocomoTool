using Logic.Domain.Level5Management.Docomo.Contract.Melody.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Melody
{
    public interface IMelodyReader
    {
        MelodyChunkData[] Read(Stream inputStream);
    }
}
