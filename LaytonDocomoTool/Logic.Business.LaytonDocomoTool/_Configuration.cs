using CrossCutting.Core.Contract.Configuration.DataClasses;

namespace Logic.Business.LaytonDocomoTool
{
    public class LaytonDocomoExtractorConfiguration
    {
        [ConfigMap("CommandLine", new[] { "h", "help" })]
        public virtual bool ShowHelp { get; set; } = false;

        [ConfigMap("CommandLine", new[] { "s", "shallow" })]
        public virtual bool IsShallow { get; set; } = false;

        [ConfigMap("CommandLine", new[] { "o", "operation" })]
        public virtual string Operation { get; set; } = "extract";

        [ConfigMap("CommandLine", new[] { "e", "encoding" })]
        public virtual string Encoding { get; set; } = "sjis";

        [ConfigMap("CommandLine", new[] { "t", "type" })]
        public virtual string Type { get; set; } = "jar";

        [ConfigMap("CommandLine", new[] { "f", "file" })]
        public virtual string FilePath { get; set; } = string.Empty;
    }
}