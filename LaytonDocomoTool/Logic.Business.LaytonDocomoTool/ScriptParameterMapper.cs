using CrossCutting.Core.Contract.Serialization;
using Logic.Business.LaytonDocomoTool.DataClasses;
using Logic.Business.LaytonDocomoTool.InternalContract;
using System.Diagnostics.CodeAnalysis;

namespace Logic.Business.LaytonDocomoTool
{
    internal class ScriptParameterMapper : IScriptParameterMapper
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly ISerializer _serializer;

        private MappingData? _mappings;
        private IDictionary<string, int>? _bitLookup;
        private IDictionary<string, int>? _storyLookup;
        private IDictionary<string, int>? _speakerLookup;

        public ScriptParameterMapper(LaytonDocomoExtractorConfiguration config, ISerializer serializer)
        {
            _config = config;
            _serializer = serializer;

            LoadMappings();
        }

        public bool TryGetBitName(int value, out string? name)
        {
            name = null;

            if (_mappings == null)
                return false;

            return _mappings.Bits.TryGetValue(value, out name);
        }

        public bool TryGetBitValue(string name, out int value)
        {
            value = 0;

            if (_bitLookup == null)
                return false;

            return _bitLookup.TryGetValue(name, out value);
        }

        public bool TryGetStoryName(int value, out string? name)
        {
            name = null;

            if (_mappings == null)
                return false;

            return _mappings.Stories.TryGetValue(value, out name);
        }

        public bool TryGetStoryValue(string name, out int value)
        {
            value = 0;

            if (_storyLookup == null)
                return false;

            return _storyLookup.TryGetValue(name, out value);
        }

        public bool TryGetSpeakerName(int value, out string? name)
        {
            name = null;

            if (_mappings == null)
                return false;

            return _mappings.Speakers.TryGetValue(value, out name);
        }

        public bool TryGetSpeakerValue(string name, out int value)
        {
            value = 0;

            if (_speakerLookup == null)
                return false;

            return _speakerLookup.TryGetValue(name, out value);
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(MappingData))]
        private void LoadMappings()
        {
            if (string.IsNullOrEmpty(_config.MappingPath))
                return;

            string path = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, _config.MappingPath);
            _mappings = _serializer.Deserialize<MappingData>(File.ReadAllText(path));

            _bitLookup = new Dictionary<string, int>();
            foreach (int bit in _mappings.Bits.Keys)
            {
                if (!_bitLookup.ContainsKey(_mappings.Bits[bit]))
                    _bitLookup[_mappings.Bits[bit]] = bit;
            }

            _storyLookup = new Dictionary<string, int>();
            foreach (int story in _mappings.Stories.Keys)
            {
                if (!_storyLookup.ContainsKey(_mappings.Stories[story]))
                    _storyLookup[_mappings.Stories[story]] = story;
            }

            _speakerLookup = new Dictionary<string, int>();
            foreach (int speaker in _mappings.Speakers.Keys)
            {
                if (!_speakerLookup.ContainsKey(_mappings.Speakers[speaker]))
                    _speakerLookup[_mappings.Speakers[speaker]] = speaker;
            }
        }
    }
}
