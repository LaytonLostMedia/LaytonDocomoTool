using System.Text;
using Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Resource
{
    public interface IResourceWriter
    {
        void Write(ResourceData resourceData, Encoding textEncoding, Stream output);
        void Write(ResourceEntryData[] entries, Stream output);
    }
}
