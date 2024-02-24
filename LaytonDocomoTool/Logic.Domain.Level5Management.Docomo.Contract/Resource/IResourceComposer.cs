using Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource
{
    public interface IResourceComposer
    {
        ResourceEntryData[] Compose(ResourceData resourceData);
    }
}
