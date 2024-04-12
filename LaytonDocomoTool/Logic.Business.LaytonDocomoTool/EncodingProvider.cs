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
            switch (_config.Encoding)
            {
                case "sjis":
                    return Encoding.GetEncoding("Shift-JIS");

                case "windows-1252":
                    return Encoding.Latin1;

                default:
                    throw new InvalidOperationException($"Unknown encoding '{_config.Encoding}'.");
            }
        }
    }
}
