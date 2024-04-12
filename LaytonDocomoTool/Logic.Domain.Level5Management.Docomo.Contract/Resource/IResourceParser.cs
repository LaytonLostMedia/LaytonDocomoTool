using System.Text;
using Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource
{
    public interface IResourceParser
    {
        ResourceData Parse(Stream resourceStream, Encoding textEncoding);
        ResourceData Parse(ResourceEntryData[] entries, Encoding textEncoding);
    }
}
