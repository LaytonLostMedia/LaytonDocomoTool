using System.Text;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script
{
    public interface IScriptComposer
    {
        EventEntryData[] Compose(EventData[] events, Encoding textEncoding);
    }
}
