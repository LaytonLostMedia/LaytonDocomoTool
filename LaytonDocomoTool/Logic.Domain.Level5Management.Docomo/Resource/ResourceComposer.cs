using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.Resource;
using Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Resource
{
    internal class ResourceComposer : IResourceComposer
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly Encoding _sjisEncoding;

        public ResourceComposer(IBinaryFactory binaryFactory)
        {
            _binaryFactory = binaryFactory;
            _sjisEncoding = Encoding.GetEncoding("Shift-JIS");
        }

        public ResourceEntryData[] Compose(ResourceData resourceData)
        {
            var result = new List<ResourceEntryData>();

            ResourceEntryData resourcesEntry = ComposeResourceObjects(resourceData.Objects.Where(x => x is ResourceObjectPimgData).ToArray(), null);
            if (resourcesEntry.Data.Length > 0)
                result.Add(resourcesEntry);

            resourcesEntry = ComposeResourceFiles(resourceData.ResourceFiles);
            if (resourcesEntry.Data.Length > 0)
                result.Add(resourcesEntry);

            resourcesEntry = ComposeResourceObjects(resourceData.Objects.Where(x => x is not ResourceObjectPimgData).ToArray(), resourceData.ResourceFiles);
            if (resourcesEntry.Data.Length > 0)
                result.Add(resourcesEntry);

            IReadOnlyList<ResourceEntryData> resourceEntries = ComposeResourceArrays(resourceData.ValueArrays);
            foreach (ResourceEntryData resourceEntry in resourceEntries)
                if (resourcesEntry.Data.Length > 0)
                    result.Add(resourceEntry);

            return result.ToArray();
        }

        private ResourceEntryData ComposeResourceFiles(ResourceResEntryData[] resources)
        {
            var resourceStream = new MemoryStream();
            using IBinaryWriterX writer = _binaryFactory.CreateWriter(resourceStream, true);

            writer.Write(resources.Length);

            foreach (ResourceResEntryData resource in resources)
            {
                WriteString(writer, resource.Name);

                writer.Write((int)resource.Data.Length);
                resource.Data.CopyTo(resourceStream);
            }

            resourceStream.Position = 0;

            return new ResourceEntryData
            {
                Identifier = "res",
                Data = resourceStream
            };
        }

        private ResourceEntryData ComposeResourceObjects(ResourceObjectData[] objects, ResourceResEntryData[] resources)
        {
            var objectStream = new MemoryStream();
            using IBinaryWriterX writer = _binaryFactory.CreateWriter(objectStream, true);

            foreach (ResourceObjectData resourceObj in objects)
            {
                switch (resourceObj)
                {
                    case ResourceObjectPartsData partsData:
                        WriteString(writer, "parts");

                        writer.Write(partsData.Parts.Length);

                        foreach (PartData part in partsData.Parts)
                        {
                            int resourceIndex = Array.IndexOf(resources, resources.FirstOrDefault(x => x.Name == part.ResourceName));

                            writer.Write(resourceIndex);
                            writer.Write(part.X);
                            writer.Write(part.Y);
                            writer.Write(part.Width);
                            writer.Write(part.Height);
                        }
                        break;

                    case ResourceObjectAnimationData animationData:
                        WriteString(writer, "animation");

                        writer.Write(animationData.Animations.Length);

                        foreach (AnimationData animation in animationData.Animations)
                        {
                            writer.Write(animation.Steps.Length);

                            foreach (AnimationStepData animationStep in animation.Steps)
                            {
                                writer.Write(animationStep.Parts.Length);

                                foreach (AnimationStepPartData animationStepPart in animationStep.Parts)
                                {
                                    writer.Write(animationStepPart.PartIndex);
                                    writer.Write(animationStepPart.X);
                                    writer.Write(animationStepPart.Y);
                                }

                                writer.Write(animationStep.EndFrame);
                            }
                        }
                        break;

                    case ResourceObjectLayoutData layoutData:
                        WriteString(writer, "layout");

                        writer.Write(0);

                        writer.Write(1);

                        writer.Write(layoutData.Width);
                        writer.Write(layoutData.Height);

                        for (var i = 0; i < 17; i++)
                        {
                            LayoutResourcePosition layoutPos = i >= layoutData.ResourcePositions.Length
                                ? new LayoutResourcePosition()
                                : layoutData.ResourcePositions[i];

                            writer.Write(layoutPos.X);
                            writer.Write(layoutPos.Y);
                        }

                        for (var i = 0; i < 6; i++)
                        {
                            LayoutResourceArea layoutArea = i >= layoutData.ResourceAreas.Length
                                ? new LayoutResourceArea()
                                : layoutData.ResourceAreas[i];

                            writer.Write(layoutArea.X);
                            writer.Write(layoutArea.Y);
                            writer.Write(layoutArea.Width);
                            writer.Write(layoutArea.Height);
                        }

                        for (var i = 0; i < 31; i++)
                            writer.Write(i >= layoutData.Unknown1.Length ? 0 : layoutData.Unknown1[i]);

                        for (var i = 0; i < 2; i++)
                            writer.Write(i >= layoutData.Unknown2.Length ? 0 : layoutData.Unknown2[i]);

                        break;

                    case ResourceObjectPimgData pimgData:
                        WriteString(writer, "pimg");

                        writer.Write(0);

                        writer.Write(pimgData.ImageNames.Length);
                        foreach (string imageName in pimgData.ImageNames)
                            WriteString(writer, imageName);

                        break;

                    case ResourceObjectButtonData buttonData:
                        WriteString(writer, "button");

                        writer.Write(buttonData.Buttons.Length + 1);
                        writer.Write(buttonData.BackgroundImageName == null ? -1 : Array.IndexOf(resources, resources.FirstOrDefault(x => x.Name == buttonData.BackgroundImageName)));
                        writer.Write(buttonData.Unknown);

                        foreach (ButtonData button in buttonData.Buttons)
                        {
                            for (var i = 0; i < 3; i++)
                            {
                                string? imageName = i >= button.ImageNames.Length ? null : button.ImageNames[i];
                                writer.Write(imageName == null ? -1 : Array.IndexOf(resources, resources.FirstOrDefault(x => x.Name == imageName)));
                            }

                            writer.Write(button.ImageX);
                            writer.Write(button.ImageY);
                            writer.Write(button.FlipMode);
                            writer.Write(button.X);
                            writer.Write(button.Y);
                            writer.Write(button.OrderY);
                            writer.Write(button.OrderX);
                            WriteString(writer, button.Result);
                        }
                        break;

                    case ResourceObjectAnimeData animeData:
                        WriteString(writer, "anime");

                        writer.Write(0);

                        writer.Write(animeData.X);
                        writer.Write(animeData.Y);
                        int elementCount = Math.Min(Math.Min(animeData.ImageNames.Length, animeData.Values2.Length), animeData.Values3.Length);
                        writer.Write(elementCount);

                        foreach (string? imageName in animeData.ImageNames)
                            writer.Write(imageName == null ? -1 : Array.IndexOf(resources, resources.FirstOrDefault(x => x.Name == imageName)));
                        foreach (int value in animeData.Values2)
                            writer.Write(value);
                        foreach (int value in animeData.Values3)
                            writer.Write(value);
                        break;

                    case ResourceObjectKeyboardData keyboardData:
                        WriteString(writer, "keyboard");

                        writer.Write(0);

                        writer.Write(Array.IndexOf(resources, resources.FirstOrDefault(x => x.Name == keyboardData.ResourceName)));

                        writer.Write(keyboardData.Solutions.Length);
                        foreach (string solution in keyboardData.Solutions)
                            WriteString(writer, solution);

                        writer.Write(keyboardData.Digits);
                        break;

                    case ResourceObjectSelectData selectData:
                        WriteString(writer, "select");

                        writer.Write(0);

                        writer.Write(Array.IndexOf(resources, resources.FirstOrDefault(x => x.Name == selectData.BackgroundImageName)));

                        int elementCount1 = Math.Min(Math.Min(selectData.SelectableImageNames.Length, selectData.Unknown1.Length), selectData.Unknown2.Length);
                        writer.Write(elementCount1);

                        int elementCount2 = Math.Min(Math.Min(selectData.UnselectedImagePosX.Length, selectData.UnselectedImagePosY.Length), selectData.SelectorFlipMode.Length);
                        elementCount2 = Math.Min(Math.Min(selectData.SelectorPosX.Length, selectData.SelectorPosY.Length), elementCount2);
                        elementCount2 = Math.Min(Math.Min(selectData.OrderY.Length, selectData.OrderX.Length), elementCount2);
                        elementCount2 = Math.Min(selectData.Unknown3.Length, elementCount2);
                        writer.Write(elementCount2);

                        writer.Write(selectData.Solutions.Length);

                        for (var i = 0; i < elementCount1; i++)
                        {
                            writer.Write(Array.IndexOf(resources, resources.FirstOrDefault(x => x.Name == selectData.SelectableImageNames[i])));
                            writer.Write(selectData.Unknown1[i]);
                            writer.Write(selectData.Unknown2[i]);
                        }

                        for (var i = 0; i < elementCount2; i++)
                        {
                            writer.Write(selectData.UnselectedImagePosX[i]);
                            writer.Write(selectData.UnselectedImagePosY[i]);
                            writer.Write(selectData.SelectorFlipMode[i]);
                            writer.Write(selectData.SelectorPosX[i]);
                            writer.Write(selectData.SelectorPosY[i]);
                            writer.Write(selectData.OrderY[i]);
                            writer.Write(selectData.OrderX[i]);
                            writer.Write(selectData.Unknown3[i]);
                        }

                        foreach (string solution in selectData.Solutions)
                            WriteString(writer, solution);

                        break;

                    case ResourceObjectSwitchData switchData:
                        WriteString(writer, "switch");

                        int elementCount3 = Math.Min(Math.Min(switchData.ImageNames.Length, switchData.ImagePosX.Length), switchData.ImagePosY.Length);
                        elementCount3 = Math.Min(Math.Min(elementCount3, switchData.ImagePosY.Length), switchData.SelectorFlipMode.Length);
                        elementCount3 = Math.Min(Math.Min(elementCount3, switchData.SelectorPosX.Length), switchData.SelectorPosY.Length);
                        elementCount3 = Math.Min(Math.Min(elementCount3, switchData.OrderY.Length), switchData.OrderX.Length);
                        elementCount3 = Math.Min(elementCount3, switchData.Solution.Length);

                        writer.Write(elementCount3 + 1);
                        writer.Write(Array.IndexOf(resources, resources.FirstOrDefault(x => x.Name == switchData.BackgroundImageName)));

                        for (var i = 0; i < elementCount3; i++)
                        {
                            writer.Write(switchData.ImageNames[i].Length);
                            for (var j = 0; j < switchData.ImageNames[i].Length; j++)
                                writer.Write(Array.IndexOf(resources, resources.FirstOrDefault(x => x.Name == switchData.ImageNames[i][j])));

                            writer.Write(switchData.ImagePosX[i]);
                            writer.Write(switchData.ImagePosY[i]);
                            writer.Write(switchData.SelectorFlipMode[i]);
                            writer.Write(switchData.SelectorPosX[i]);
                            writer.Write(switchData.SelectorPosY[i]);
                            writer.Write(switchData.OrderY[i]);
                            writer.Write(switchData.OrderX[i]);
                            writer.Write(switchData.Solution[i]);
                        }
                        break;

                    case ResourceObjectLineData lineData:
                        WriteString(writer, "line");

                        writer.Write(0);

                        writer.Write(Array.IndexOf(resources, resources.FirstOrDefault(x => x.Name == lineData.BackgroundImageName)));

                        int elementCount4 = Math.Min(Math.Min(lineData.LinePointPosX.Length, lineData.LinePointPosY.Length), lineData.OrderY.Length);
                        elementCount4 = Math.Min(elementCount4, lineData.OrderX.Length);

                        writer.Write(elementCount4);
                        writer.Write(lineData.Solution.Length);
                        writer.Write(lineData.BlockPointCombination.Length);
                        writer.Write(lineData.Unknown1.Length);

                        for (var i = 0; i < elementCount4; i++)
                        {
                            writer.Write(lineData.LinePointPosX[i]);
                            writer.Write(lineData.LinePointPosY[i]);
                            writer.Write(lineData.OrderY[i]);
                            writer.Write(lineData.OrderX[i]);
                        }

                        foreach (string name in lineData.Solution)
                            WriteString(writer, name);

                        foreach (string name in lineData.BlockPointCombination)
                            WriteString(writer, name);

                        foreach (string name in lineData.Unknown1)
                            WriteString(writer, name);
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown resource object {resourceObj.GetType().Name}.");
                }
            }

            objectStream.Position = 0;

            return new ResourceEntryData
            {
                Identifier = "obj",
                Data = objectStream
            };
        }

        private IReadOnlyList<ResourceEntryData> ComposeResourceArrays(ResourceArrayData resourceArrays)
        {
            var result = new List<ResourceEntryData>();

            if ((resourceArrays.IntValues?.Length ?? 0) > 0)
            {
                var resourceStream = new MemoryStream();
                using IBinaryWriterX writer = _binaryFactory.CreateWriter(resourceStream, true);

                WriteString(writer, "int");

                foreach (int intValue in resourceArrays.IntValues!)
                    writer.Write(intValue);

                resourceStream.Position = 0;

                result.Add(new ResourceEntryData
                {
                    Identifier = "ary",
                    Data = resourceStream
                });
            }

            if ((resourceArrays.ShortValues?.Length ?? 0) > 0)
            {
                var resourceStream = new MemoryStream();
                using IBinaryWriterX writer = _binaryFactory.CreateWriter(resourceStream, true);

                WriteString(writer, "short");

                foreach (short shortValue in resourceArrays.ShortValues!)
                    writer.Write(shortValue);

                resourceStream.Position = 0;

                result.Add(new ResourceEntryData
                {
                    Identifier = "ary",
                    Data = resourceStream
                });
            }

            if ((resourceArrays.ByteValues?.Length ?? 0) > 0)
            {
                var resourceStream = new MemoryStream();
                using IBinaryWriterX writer = _binaryFactory.CreateWriter(resourceStream, true);

                WriteString(writer, "byte");

                writer.Write(resourceArrays.ByteValues!);

                resourceStream.Position = 0;

                result.Add(new ResourceEntryData
                {
                    Identifier = "ary",
                    Data = resourceStream
                });
            }

            if ((resourceArrays.StringValues?.Length ?? 0) > 0)
            {
                var resourceStream = new MemoryStream();
                using IBinaryWriterX writer = _binaryFactory.CreateWriter(resourceStream, true);

                WriteString(writer, "str");

                foreach (string stringValue in resourceArrays.StringValues!)
                    WriteString(writer, stringValue);

                resourceStream.Position = 0;

                result.Add(new ResourceEntryData
                {
                    Identifier = "ary",
                    Data = resourceStream
                });
            }

            return result.ToArray();
        }

        private void WriteString(IBinaryWriterX writer, string value)
        {
            byte[] stringBytes = _sjisEncoding.GetBytes(value);
            int stringLength = stringBytes.Length;

            writer.Write((short)stringLength);
            writer.Write(stringBytes);
        }
    }
}
