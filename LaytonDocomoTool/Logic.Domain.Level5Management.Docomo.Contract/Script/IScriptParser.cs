using System.Text;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract.Script
{
    public interface IScriptParser
    {
        EventData[] Parse(Stream input, Encoding textEncoding, bool createBranches = true);
        EventData[] Parse(EventEntryData[] entries, Encoding textEncoding, bool createBranches = true);
    }
}
