using Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses;
using System.Text;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource
{
    public interface IResourceComposer
    {
        ResourceEntryData[] Compose(ResourceData resourceData, Encoding textEncoding);
    }
}
