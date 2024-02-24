using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource
{
    public interface IResourceWriter
    {
        void Write(ResourceData resourceData, Stream output);
        void Write(ResourceEntryData[] entries, Stream output);
    }
}
