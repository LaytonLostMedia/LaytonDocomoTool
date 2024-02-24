using CrossCutting.Core.Contract.Serialization;
using Logic.Business.LaytonDocomoTool.DataClasses;
using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.Level5Management.Docomo.Contract.Resource;
using Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace Logic.Business.LaytonDocomoTool
{
    internal class CreateResourceWorkflow : ICreateResourceWorkflow
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly IResourceWriter _resourceWriter;
        private readonly ISerializer _serializer;

        public CreateResourceWorkflow(LaytonDocomoExtractorConfiguration config, IResourceWriter resourceWriter, ISerializer serializer)
        {
            _config = config;
            _resourceWriter = resourceWriter;
            _serializer = serializer;
        }

        public void Work()
        {
            string targetFilePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(_config.FilePath))!, Path.GetFileName(Path.GetFullPath(_config.FilePath)) + ".dat");
            using Stream targetFileStream = File.Create(targetFilePath);

            Work(_config.FilePath, targetFileStream);
        }

        public void Work(string resourceDirectory, Stream output)
        {
            ResourceResEntryData[] resourceEntries = LoadResourceFiles(resourceDirectory);

            ResourceObjectData[] resourceObjects = LoadResourceObjects(resourceDirectory);
            ResourceArrayData resourceArrays = LoadResourceArrays(resourceDirectory);

            var resourceData = new ResourceData
            {
                ResourceFiles = resourceEntries,
                Objects = resourceObjects,
                ValueArrays = resourceArrays
            };
            _resourceWriter.Write(resourceData, output);
        }

        private ResourceResEntryData[] LoadResourceFiles(string baseDir)
        {
            string resourceDir = Path.Combine(baseDir, "res");

            string orderFilePath = Path.Combine(resourceDir, "resources.json");
            var orderMap = new Dictionary<string, int>();
            if (File.Exists(orderFilePath))
                orderMap = _serializer.Deserialize<string[]>(File.ReadAllText(orderFilePath)).Select((x, i) => (x, i)).ToDictionary(x => x.x, x => x.i);

            var result = new ResourceResEntryData[orderMap.Count];

            foreach (string file in Directory.GetFiles(resourceDir, "*", SearchOption.TopDirectoryOnly))
            {
                if (Path.GetFileName(file) == "resources.json")
                    continue;

                if (!orderMap.TryGetValue(Path.GetFileName(file), out int orderIndex))
                    continue;

                result[orderIndex] = new ResourceResEntryData
                {
                    Name = Path.GetFileName(file),
                    Data = File.OpenRead(file)
                };
            }

            return result.ToArray();
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectExtractData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectPartsData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectAnimationData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectLayoutData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectPimgData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectButtonData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectAnimeData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectKeyboardData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectSelectData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectSwitchData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectLineData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ButtonData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(LayoutResourcePosition))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(LayoutResourceArea))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(PartData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AnimationData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AnimationStepData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AnimationStepPartData))]
        private ResourceObjectData[] LoadResourceObjects(string baseDir)
        {
            var result = new List<ResourceObjectData>();

            string objText = File.ReadAllText(Path.Combine(baseDir, "obj.json"));
            ResourceObjectExtractData[] resourcesObjects = _serializer.Deserialize<ResourceObjectExtractData[]>(objText);

            foreach (ResourceObjectExtractData resourceObject in resourcesObjects)
            {
                ResourceObjectData deserializedObject;

                switch (resourceObject.Type)
                {
                    case "Parts":
                        deserializedObject = ((JObject)resourceObject.Data).ToObject<ResourceObjectPartsData>();
                        break;

                    case "Animation":
                        deserializedObject = ((JObject)resourceObject.Data).ToObject<ResourceObjectAnimationData>();
                        break;

                    case "Layout":
                        deserializedObject = ((JObject)resourceObject.Data).ToObject<ResourceObjectLayoutData>();
                        break;

                    case "PalettedImage":
                        deserializedObject = ((JObject)resourceObject.Data).ToObject<ResourceObjectPimgData>();
                        break;

                    case "Button":
                        deserializedObject = ((JObject)resourceObject.Data).ToObject<ResourceObjectButtonData>();
                        break;

                    case "Anime":
                        deserializedObject = ((JObject)resourceObject.Data).ToObject<ResourceObjectAnimeData>();
                        break;

                    case "Keyboard":
                        deserializedObject = ((JObject)resourceObject.Data).ToObject<ResourceObjectKeyboardData>();
                        break;

                    case "Selection":
                        deserializedObject = ((JObject)resourceObject.Data).ToObject<ResourceObjectSelectData>();
                        break;

                    case "Switch":
                        deserializedObject = ((JObject)resourceObject.Data).ToObject<ResourceObjectSwitchData>();
                        break;

                    case "Line":
                        deserializedObject = ((JObject)resourceObject.Data).ToObject<ResourceObjectLineData>();
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported resource object {resourceObject.Type}.");
                }

                result.Add(deserializedObject);
            }

            return result.ToArray();
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceArrayData))]
        private ResourceArrayData LoadResourceArrays(string baseDir)
        {
            string aryText = File.ReadAllText(Path.Combine(baseDir, "ary.json"));
            return _serializer.Deserialize<ResourceArrayData>(aryText);
        }
    }
}
