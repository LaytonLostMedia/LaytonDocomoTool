using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.Resource;
using Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Resource
{
    internal class ResourceWriter : IResourceWriter
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly IResourceComposer _resourceComposer;

        public ResourceWriter(IBinaryFactory binaryFactory, IResourceComposer resourceComposer)
        {
            _binaryFactory = binaryFactory;
            _resourceComposer = resourceComposer;
        }

        public void Write(ResourceData resourceData, Encoding textEncoding, Stream output)
        {
            ResourceEntryData[] entries = _resourceComposer.Compose(resourceData, textEncoding);

            Write(entries, output);
        }

        public void Write(ResourceEntryData[] entries, Stream output)
        {
            using IBinaryWriterX writer = _binaryFactory.CreateWriter(output, true);

            foreach (ResourceEntryData entry in entries)
            {
                writer.WriteString(entry.Identifier, Encoding.ASCII, false, false);
                writer.Write((int)entry.Data.Length);

                entry.Data.CopyTo(output);
            }
        }
    }
}
