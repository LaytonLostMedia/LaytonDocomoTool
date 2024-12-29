using System.Text;
using Logic.Business.LaytonDocomoTool.InternalContract;

namespace Logic.Business.LaytonDocomoTool
{
    internal class EncodingProvider : IEncodingProvider
    {
        private readonly LaytonDocomoExtractorConfiguration _config;

        public EncodingProvider(LaytonDocomoExtractorConfiguration config)
        {
            _config = config;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public Encoding GetEncoding()
        {
            if (_config.Encoding.Equals("sjis", StringComparison.OrdinalIgnoreCase))
                return Encoding.GetEncoding("Shift-JIS");

            if (_config.Encoding.Equals("windows-1252", StringComparison.OrdinalIgnoreCase))
                return Encoding.GetEncoding("Windows-1252");

            throw new InvalidOperationException($"Unknown encoding '{_config.Encoding}'.");
        }
    }
}
