using Logic.Business.LaytonDocomoTool.Contract.DataClasses;
using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.DocomoManagement.Contract;

namespace Logic.Business.LaytonDocomoTool
{
    internal class InjectJarWorkflow : IInjectJarWorkflow
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly IJarReader _jarReader;
        private readonly IJarWriter _jarWriter;
        private readonly ICreateTableWorkflow _createTableWorkflow;

        public InjectJarWorkflow(LaytonDocomoExtractorConfiguration config, IJarReader jarReader, IJarWriter jarWriter, ICreateTableWorkflow createTableWorkflow)
        {
            _config = config;
            _jarReader = jarReader;
            _jarWriter = jarWriter;
            _createTableWorkflow = createTableWorkflow;
        }

        public void Work()
        {
            string jarFile = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(_config.FilePath))!, "jar");

            Stream jarStream = File.OpenRead(jarFile);
            JarArchiveEntry[] jarEntries = _jarReader.Read(jarStream);

            jarStream.Dispose();

            Stream indexStream = new MemoryStream();
            Stream dataStream = new MemoryStream();
            _createTableWorkflow.Work(indexStream, dataStream);

            JarArchiveEntry? jarIndexFile = jarEntries.FirstOrDefault(x => x.Path == "tbl/0.dat");
            JarArchiveEntry? jarDatafile = jarEntries.FirstOrDefault(x => x.Path == "tbl/1.dat");

            if (jarIndexFile != null)
                jarIndexFile.Data = indexStream;
            if (jarDatafile != null)
                jarDatafile.Data = dataStream;

            indexStream.Position = 0;
            dataStream.Position = 0;

            jarStream = File.Create(jarFile);
            _jarWriter.Write(jarEntries, jarStream);

            jarStream.Dispose();
        }
    }
}
