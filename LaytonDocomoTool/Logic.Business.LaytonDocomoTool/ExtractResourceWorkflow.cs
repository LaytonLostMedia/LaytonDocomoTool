using System.Diagnostics.CodeAnalysis;
using CrossCutting.Core.Contract.Serialization;
using Logic.Business.LaytonDocomoTool.DataClasses;
using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.Level5Management.Docomo.Contract.Resource;
using Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses;

namespace Logic.Business.LaytonDocomoTool
{
    internal class ExtractResourceWorkflow : IExtractResourceWorkflow
    {
        private readonly LaytonDocomoExtractorConfiguration _config;
        private readonly IResourceParser _resourceParser;
        private readonly ISerializer _serializer;

        public ExtractResourceWorkflow(LaytonDocomoExtractorConfiguration config, IResourceParser resourceParser, ISerializer serializer)
        {
            _config = config;
            _resourceParser = resourceParser;
            _serializer = serializer;
        }

        public void Work()
        {
            using Stream fileStream = File.OpenRead(_config.FilePath);

            string extractDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(_config.FilePath))!, Path.GetFileNameWithoutExtension(_config.FilePath));

            Work(fileStream, extractDir);
        }

        public void Work(Stream resourceStream, string extractDir)
        {
            ResourceData resourceData = _resourceParser.Parse(resourceStream);

            ExtractResourceFiles(extractDir, resourceData.ResourceFiles);

            ExtractResourceObjects(extractDir, resourceData.Objects);
            ExtractResourceArrays(extractDir, resourceData.ValueArrays);
        }

        private void ExtractResourceFiles(string baseDir, ResourceResEntryData[] resourceFiles)
        {
            string extractDir = Path.Combine(baseDir, "res");
            Directory.CreateDirectory(extractDir);

            var nameOrder = new List<string>(resourceFiles.Length);

            foreach (ResourceResEntryData resourceFile in resourceFiles)
            {
                using Stream outputStream = File.OpenWrite(Path.Combine(extractDir, resourceFile.Name));

                resourceFile.Data.CopyTo(outputStream);

                nameOrder.Add(resourceFile.Name);
            }

            using Stream orderStream = File.OpenWrite(Path.Combine(extractDir, "resources.json"));
            using StreamWriter writer = new StreamWriter(orderStream);

            writer.Write(_serializer.Serialize(nameOrder));
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectExtractData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectPartsData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectAnimationData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectLayoutData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectPimgData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectButtonData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectAnimeData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectKeyboardData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectSelectData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectSwitchData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceObjectLineData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ButtonData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(LayoutResourcePosition))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(LayoutResourceArea))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(PartData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(AnimationData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(AnimationStepData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(AnimationStepPartData))]
        private void ExtractResourceObjects(string extractDir, ResourceObjectData[] resourceObjects)
        {
            var objectDescriptions = new List<ResourceObjectExtractData>(resourceObjects.Length);

            foreach (ResourceObjectData resourceObject in resourceObjects)
            {
                string name;

                switch (resourceObject)
                {
                    case ResourceObjectPartsData:
                        name = "Parts";
                        break;

                    case ResourceObjectAnimationData:
                        name = "Animation";
                        break;

                    case ResourceObjectLayoutData:
                        name = "Layout";
                        break;

                    case ResourceObjectPimgData:
                        name = "PalettedImage";
                        break;

                    case ResourceObjectButtonData:
                        name = "Button";
                        break;

                    case ResourceObjectAnimeData:
                        name = "Anime";
                        break;

                    case ResourceObjectKeyboardData:
                        name = "Keyboard";
                        break;

                    case ResourceObjectSelectData:
                        name = "Selection";
                        break;

                    case ResourceObjectSwitchData:
                        name = "Switch";
                        break;

                    case ResourceObjectLineData:
                        name = "Line";
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported resource object {resourceObjects.GetType().Name}.");
                }

                objectDescriptions.Add(new ResourceObjectExtractData
                {
                    Type = name,
                    Data = resourceObject
                });
            }

            using Stream outputStream = File.OpenWrite(Path.Combine(extractDir, "obj.json"));
            using StreamWriter writer = new StreamWriter(outputStream);

            writer.Write(_serializer.Serialize(objectDescriptions));
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ResourceArrayData))]
        private void ExtractResourceArrays(string extractDir, ResourceArrayData arrayData)
        {
            using Stream outputStream = File.OpenWrite(Path.Combine(extractDir, "ary.json"));
            using StreamWriter writer = new StreamWriter(outputStream);

            writer.Write(_serializer.Serialize(arrayData));
        }
    }
}
