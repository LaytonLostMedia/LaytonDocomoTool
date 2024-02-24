using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5Management.Docomo.Contract.Resource;
using Logic.Domain.Level5Management.Docomo.Contract.Resource.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Resource
{
    internal class ResourceParser : IResourceParser
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly IStreamFactory _streamFactory;
        private readonly IResourceReader _resourceReader;
        private readonly Encoding _sjisEncoding;

        public ResourceParser(IBinaryFactory binaryFactory, IStreamFactory streamFactory, IResourceReader resourceReader)
        {
            _binaryFactory = binaryFactory;
            _streamFactory = streamFactory;
            _resourceReader = resourceReader;
            _sjisEncoding = Encoding.GetEncoding("Shift-JIS");
        }

        public ResourceData Parse(Stream resourceStream)
        {
            ResourceEntryData[] entries = _resourceReader.Read(resourceStream);

            return Parse(entries);
        }

        public ResourceData Parse(ResourceEntryData[] entries)
        {
            var resourceFiles = new List<ResourceResEntryData>();

            foreach (ResourceEntryData entry in entries)
            {
                switch (entry.Identifier)
                {
                    case "res":
                        AddResourceFiles(entry.Data, resourceFiles);
                        break;
                }
            }

            var objects = new List<ResourceObjectData>();
            var resourceArrays = new ResourceArrayData();

            foreach (ResourceEntryData entry in entries)
            {
                switch (entry.Identifier)
                {
                    case "obj":
                        AddObjects(entry.Data, resourceFiles, objects);
                        break;

                    case "ary":
                        AddArrayValues(entry.Data, resourceArrays);
                        break;
                }
            }

            return new ResourceData
            {
                ResourceFiles = resourceFiles.ToArray(),
                Objects = objects.ToArray(),
                ValueArrays = resourceArrays
            };
        }

        private void AddResourceFiles(Stream resStream, IList<ResourceResEntryData> resourceFiles)
        {
            using IBinaryReaderX reader = _binaryFactory.CreateReader(resStream, true);

            int resCount = reader.ReadInt32();

            for (var i = 0; i < resCount; i++)
            {
                string name = ReadString(reader);

                int dataLength = reader.ReadInt32();
                Stream dataStream = _streamFactory.CreateSubStream(resStream, resStream.Position, dataLength);

                resStream.Position += dataLength;

                resourceFiles.Add(new ResourceResEntryData
                {
                    Name = name,
                    Data = dataStream
                });
            }
        }

        private void AddArrayValues(Stream aryStream, ResourceArrayData arrayData)
        {
            using IBinaryReaderX reader = _binaryFactory.CreateReader(aryStream, true);

            string identifier = ReadString(reader);
            switch (identifier)
            {
                case "int":
                    int intCount = (int)(aryStream.Length - aryStream.Position) / 4;

                    arrayData.IntValues = new int[intCount];

                    for (var i = 0; i < intCount; i++)
                        arrayData.IntValues[i] = reader.ReadInt32();
                    break;

                case "short":
                    int shortCount = (int)(aryStream.Length - aryStream.Position) / 2;

                    arrayData.ShortValues = new short[shortCount];

                    for (var i = 0; i < shortCount; i++)
                        arrayData.ShortValues[i] = reader.ReadInt16();
                    break;

                case "byte":
                    var byteCount = (int)(aryStream.Length - aryStream.Position);

                    arrayData.ByteValues = reader.ReadBytes(byteCount);
                    break;

                case "str":
                    var stringValues = new List<string>(100);

                    for (var i = 0; i < 100; i++)
                    {
                        if (aryStream.Position >= aryStream.Length)
                            break;

                        stringValues.Add(ReadString(reader));
                    }

                    arrayData.StringValues = stringValues.ToArray();
                    break;
            }
        }

        private void AddObjects(Stream objStream, IList<ResourceResEntryData> resourceEntries, IList<ResourceObjectData> objects)
        {
            using IBinaryReaderX reader = _binaryFactory.CreateReader(objStream, true);

            while (objStream.Position < objStream.Length)
            {
                string identifier = ReadString(reader);
                switch (identifier)
                {
                    case "parts":
                        int partCount = reader.ReadInt32();

                        var parts = new PartData[partCount];
                        for (var i = 0; i < partCount; i++)
                        {
                            parts[i] = new PartData
                            {
                                ResourceName = resourceEntries[reader.ReadInt32()].Name,
                                X = reader.ReadInt32(),
                                Y = reader.ReadInt32(),
                                Width = reader.ReadInt32(),
                                Height = reader.ReadInt32()
                            };
                        }

                        objects.Add(new ResourceObjectPartsData
                        {
                            Parts = parts
                        });
                        break;

                    case "animation":
                        int animationCount = reader.ReadInt32();

                        var animations = new AnimationData[animationCount];
                        for (var i = 0; i < animationCount; i++)
                        {
                            int stepCount = reader.ReadInt32();

                            animations[i] = new AnimationData
                            {
                                Steps = new AnimationStepData[stepCount]
                            };

                            for (var j = 0; j < stepCount; j++)
                            {
                                int stepPartCount = reader.ReadInt32();

                                animations[i].Steps[j] = new AnimationStepData
                                {
                                    Parts = new AnimationStepPartData[stepPartCount]
                                };

                                for (var h = 0; h < stepPartCount; h++)
                                {
                                    animations[i].Steps[j].Parts[h] = new AnimationStepPartData
                                    {
                                        PartIndex = reader.ReadInt32(),
                                        X = reader.ReadInt32(),
                                        Y = reader.ReadInt32()
                                    };
                                }

                                animations[i].Steps[j].EndFrame = reader.ReadInt32();
                            }
                        }

                        objects.Add(new ResourceObjectAnimationData
                        {
                            Animations = animations
                        });
                        break;

                    case "layout":
                        reader.ReadInt32();

                        var layoutObject = new ResourceObjectLayoutData();

                        int layoutType = reader.ReadInt32();
                        switch (layoutType)
                        {
                            case 1:
                                layoutObject.Width = reader.ReadInt32();
                                layoutObject.Height = reader.ReadInt32();

                                layoutObject.ResourcePositions = new LayoutResourcePosition[17];
                                for (var i = 0; i < 17; i++)
                                {
                                    layoutObject.ResourcePositions[i] = new LayoutResourcePosition
                                    {
                                        X = reader.ReadInt32(),
                                        Y = reader.ReadInt32()
                                    };
                                }

                                layoutObject.ResourceAreas = new LayoutResourceArea[6];
                                for (var i = 0; i < 6; i++)
                                {
                                    layoutObject.ResourceAreas[i] = new LayoutResourceArea
                                    {
                                        X = reader.ReadInt32(),
                                        Y = reader.ReadInt32(),
                                        Width = reader.ReadInt32(),
                                        Height = reader.ReadInt32()
                                    };
                                }

                                layoutObject.Unknown1 = new int[31];
                                for (var i = 0; i < 31; i++)
                                    layoutObject.Unknown1[i] = reader.ReadInt32();

                                layoutObject.Unknown2 = new int[2];
                                for (var i = 0; i < 2; i++)
                                    layoutObject.Unknown2[i] = reader.ReadInt32();
                                break;
                        }

                        objects.Add(layoutObject);
                        break;

                    case "pimg":
                        reader.ReadInt32();

                        var pimgObject = new ResourceObjectPimgData();

                        int nameCount = reader.ReadInt32();
                        pimgObject.ImageNames = new string[nameCount];

                        for (var i = 0; i < pimgObject.ImageNames.Length; i++)
                            pimgObject.ImageNames[i] = ReadString(reader);

                        objects.Add(pimgObject);
                        break;

                    case "button":
                        int buttonCount = reader.ReadInt32() - 1;
                        int cmdImageIndex = reader.ReadInt32();

                        var buttonObject = new ResourceObjectButtonData
                        {
                            Buttons = new ButtonData[buttonCount],
                            BackgroundImageName = cmdImageIndex == -1 ? null : resourceEntries[cmdImageIndex].Name,
                            Unknown = reader.ReadInt32()
                        };

                        for (var i = 0; i < buttonObject.Buttons.Length; i++)
                        {
                            var imageNames = new string[3];
                            for (var j = 0; j < 3; j++)
                            {
                                string imageName;

                                int imageIndex = reader.ReadInt32();
                                switch (imageIndex)
                                {
                                    case >= 0:
                                        imageName = resourceEntries[imageIndex].Name;
                                        break;

                                    case -1:
                                        imageName = null;
                                        break;

                                    default:
                                        throw new InvalidOperationException($"Unsupported image index {imageIndex}.");
                                }

                                imageNames[j] = imageName;
                            }

                            buttonObject.Buttons[i] = new ButtonData
                            {
                                ImageNames = imageNames,
                                ImageX = reader.ReadInt32(),
                                ImageY = reader.ReadInt32(),
                                FlipMode = reader.ReadInt32(),
                                X = reader.ReadInt32(),
                                Y = reader.ReadInt32(),
                                OrderY = reader.ReadInt32(),
                                OrderX = reader.ReadInt32(),
                                Result = ReadString(reader)
                            };
                        }

                        objects.Add(buttonObject);
                        break;

                    case "anime":
                        reader.ReadInt32();

                        var v1 = reader.ReadInt32();
                        var v2 = reader.ReadInt32();
                        var v3 = reader.ReadInt32();

                        var animeObject = new ResourceObjectAnimeData
                        {
                            X = v1,
                            Y = v2,
                            ImageNames = new string[v3],
                            Values2 = new int[v3],
                            Values3 = new int[v3]
                        };

                        for (var i = 0; i < v3; i++)
                        {
                            int imgIndex = reader.ReadInt32();
                            animeObject.ImageNames[i] = imgIndex == -1 ? null : resourceEntries[imgIndex].Name;
                        }
                        for (var i = 0; i < v3; i++)
                            animeObject.Values2[i] = reader.ReadInt32();
                        for (var i = 0; i < v3; i++)
                            animeObject.Values3[i] = reader.ReadInt32();

                        objects.Add(animeObject);
                        break;

                    case "keyboard":
                        reader.ReadInt32();

                        int resourceIndex = reader.ReadInt32();

                        var texts = new string[reader.ReadInt32()];
                        for (var i = 0; i < texts.Length; i++)
                            texts[i] = ReadString(reader);

                        int digits = reader.ReadInt32();

                        objects.Add(new ResourceObjectKeyboardData
                        {
                            ResourceName = resourceEntries[resourceIndex].Name,
                            Solutions = texts,
                            Digits = digits
                        });
                        break;

                    case "select":
                        reader.ReadInt32();

                        string imgName10 = resourceEntries[reader.ReadInt32()].Name;
                        int v11 = reader.ReadInt32();
                        int v12 = reader.ReadInt32();
                        int v13 = reader.ReadInt32();

                        var resourceNames14 = new string[v11];
                        var v15 = new int[v11];
                        var v16 = new int[v11];
                        for (var i = 0; i < v11; i++)
                        {
                            resourceNames14[i] = resourceEntries[reader.ReadInt32()].Name;
                            v15[i] = reader.ReadInt32();
                            v16[i] = reader.ReadInt32();
                        }

                        var v17 = new int[v12];
                        var v18 = new int[v12];
                        var v19 = new int[v12];
                        var v20 = new int[v12];
                        var v21 = new int[v12];
                        var v22 = new int[v12];
                        var v23 = new int[v12];
                        var v24 = new int[v12];
                        for (var i = 0; i < v12; i++)
                        {
                            v17[i] = reader.ReadInt32();
                            v18[i] = reader.ReadInt32();
                            v19[i] = reader.ReadInt32();
                            v20[i] = reader.ReadInt32();
                            v21[i] = reader.ReadInt32();
                            v22[i] = reader.ReadInt32();
                            v23[i] = reader.ReadInt32();
                            v24[i] = reader.ReadInt32();
                        }

                        var v25 = new string[v13];
                        for (var i = 0; i < v13; i++)
                            v25[i] = ReadString(reader);

                        objects.Add(new ResourceObjectSelectData
                        {
                            BackgroundImageName = imgName10,

                            SelectableImageNames = resourceNames14,
                            Unknown1 = v15,
                            Unknown2 = v16,

                            UnselectedImagePosX = v17,
                            UnselectedImagePosY = v18,
                            SelectorFlipMode = v19,
                            SelectorPosX = v20,
                            SelectorPosY = v21,
                            OrderY = v22,
                            OrderX = v23,
                            Unknown3 = v24,

                            Solutions = v25
                        });
                        break;

                    case "switch":
                        int v30 = reader.ReadInt32();
                        string imgName = resourceEntries[reader.ReadInt32()].Name;

                        var v31 = new string[v30 - 1][];
                        var v32 = new int[v30 - 1];
                        var v33 = new int[v30 - 1];
                        var v34 = new int[v30 - 1];
                        var v35 = new int[v30 - 1];
                        var v36 = new int[v30 - 1];
                        var v37 = new int[v30 - 1];
                        var v38 = new int[v30 - 1];
                        var v39 = new int[v30 - 1];

                        for (var i = 1; i < v30; i++)
                        {
                            int v40 = reader.ReadInt32();
                            v31[i - 1] = new string[v40];

                            for (var j = 0; j < v40; j++)
                                v31[i - 1][j] = resourceEntries[reader.ReadInt32()].Name;

                            v32[i - 1] = reader.ReadInt32();
                            v33[i - 1] = reader.ReadInt32();
                            v34[i - 1] = reader.ReadInt32();
                            v35[i - 1] = reader.ReadInt32();
                            v36[i - 1] = reader.ReadInt32();
                            v37[i - 1] = reader.ReadInt32();
                            v38[i - 1] = reader.ReadInt32();
                            v39[i - 1] = reader.ReadInt32();
                        }

                        objects.Add(new ResourceObjectSwitchData
                        {
                            BackgroundImageName = imgName,
                            ImageNames = v31,
                            ImagePosX = v32,
                            ImagePosY = v33,
                            SelectorFlipMode = v34,
                            SelectorPosX = v35,
                            SelectorPosY = v36,
                            OrderY = v37,
                            OrderX = v38,
                            Solution = v39
                        });
                        break;

                    case "line":
                        reader.ReadInt32();

                        string imgName1 = resourceEntries[reader.ReadInt32()].Name;

                        int v50 = reader.ReadInt32();
                        int v51 = reader.ReadInt32();
                        int v52 = reader.ReadInt32();
                        int v53 = reader.ReadInt32();

                        var v54 = new int[v50];
                        var v55 = new int[v50];
                        var v56 = new int[v50];
                        var v57 = new int[v50];

                        for (var i = 0; i < v50; i++)
                        {
                            v54[i] = reader.ReadInt32();
                            v55[i] = reader.ReadInt32();
                            v56[i] = reader.ReadInt32();
                            v57[i] = reader.ReadInt32();
                        }

                        var v58 = new string[v51];

                        for (var i = 0; i < v51; i++)
                            v58[i] = ReadString(reader);

                        var v59 = new string[v52];

                        for (var i = 0; i < v52; i++)
                            v59[i] = ReadString(reader);

                        var v510 = new string[v53];

                        for (var i = 0; i < v53; i++)
                            v510[i] = ReadString(reader);

                        objects.Add(new ResourceObjectLineData
                        {
                            BackgroundImageName = imgName1,
                            LinePointPosX = v54,
                            LinePointPosY = v55,
                            OrderY = v56,
                            OrderX = v57,
                            Solution = v58,
                            BlockPointCombination = v59,
                            Unknown1 = v510,
                        });
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown resource object {identifier}.");
                }
            }
        }

        private string ReadString(IBinaryReaderX reader)
        {
            int identifierSize = reader.ReadInt16();
            byte[] identifierBytes = reader.ReadBytes(identifierSize);

            string identifier = _sjisEncoding.GetString(identifierBytes);

            return identifier;
        }
    }
}
