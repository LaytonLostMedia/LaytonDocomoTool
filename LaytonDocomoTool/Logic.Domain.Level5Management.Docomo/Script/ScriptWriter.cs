using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.Script;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Script
{
    internal class ScriptWriter : IScriptWriter
    {
        private readonly IBinaryFactory _binaryFactory;

        public ScriptWriter(IBinaryFactory binaryFactory)
        {
            _binaryFactory = binaryFactory;
        }

        public void Write(EventEntryData[] events, Stream output)
        {
            using IBinaryWriterX bw = _binaryFactory.CreateWriter(output, true);

            foreach (EventEntryData entry in events)
            {
                bw.WriteString(entry.identifier, Encoding.ASCII, false, false);
                bw.Write(entry.dataSize);
                bw.Write(entry.data);
            }
        }
    }
}
