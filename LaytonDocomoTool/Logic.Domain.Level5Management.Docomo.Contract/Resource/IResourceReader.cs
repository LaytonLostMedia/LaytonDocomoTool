using Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource
{
    public interface IResourceReader
    {
        ResourceEntryData[] Read(Stream input);
    }
}
