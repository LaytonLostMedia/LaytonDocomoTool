using System.Text;
using Logic.Domain.Level5Management.Docomo.Contract.Script;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Script
{
    internal class ScriptParser : IScriptParser
    {
        private readonly IScriptReader _reader;

        public ScriptParser(IScriptReader reader)
        {
            _reader = reader;
        }

        public EventData[] Parse(Stream input, Encoding textEncoding, bool createBranches = true)
        {
            EventEntryData[] entries = _reader.Read(input);

            return Parse(entries, textEncoding, createBranches);
        }

        public EventData[] Parse(EventEntryData[] entries, Encoding textEncoding, bool createBranches = true)
        {
            var parsedEntries = new EventData[entries.Length];
            for (var i = 0; i < entries.Length; i++)
                parsedEntries[i] = ParseEntry(entries[i], textEncoding);

            if (!createBranches)
                return parsedEntries;

            var index = 0;
            return Parse(parsedEntries, ref index);
        }

        private EventData[] Parse(EventData[] entries, ref int index, int branchId = -1)
        {
            var result = new List<EventData>();

            int startActiveBranchId = -1;
            int endActiveBranchId = -1;
            for (; index < entries.Length;)
            {
                EventData parsedEvent = entries[index];

                if (parsedEvent is BranchEventData branchData)
                {
                    if (branchData.Id == branchId)
                        break;

                    if (branchData is not IfEventData && branchData.Id != branchId && branchData.Id != startActiveBranchId && branchData.Id != endActiveBranchId)
                        throw new InvalidOperationException($"Inconsistent branch ID {branchData.Id}.");
                }

                index++;

                if (parsedEvent is IfEventData ifData)
                    startActiveBranchId = endActiveBranchId = ifData.Id;

                if (parsedEvent is BranchBlockEventData blockData)
                {
                    // If next branch part cannot be found, ignore this branch instruction
                    int nextBranchIndex = GetNextBranchIndex(entries, index, blockData.Id);
                    if (nextBranchIndex == -1)
                        continue;

                    if (entries[nextBranchIndex] is ElseEventData)
                    {
                        // If branch instruction is part of a compound expression
                        int compoundDepth = GetElseCompoundDepth(entries, nextBranchIndex, out endActiveBranchId);

                        // Collect if conditions for compound branch instruction
                        if (parsedEvent is ConditionalBranchBlockEventData parsedConditionalBranchData)
                        {
                            for (var i = 0; i < compoundDepth - 1; i++)
                            {
                                if (entries[index + i] is not ConditionalBranchBlockEventData conditionalBranchData)
                                    break;

                                IfConditionData[] conditions = parsedConditionalBranchData.Conditions;
                                Array.Resize(ref conditions, parsedConditionalBranchData.Conditions.Length + 1);

                                conditions[^1] = conditionalBranchData.Conditions[0];
                                parsedConditionalBranchData.Conditions = conditions;
                            }
                        }

                        index += compoundDepth - 1;

                        blockData.Events = Parse(entries, ref index, blockData.Id);
                    }
                    else
                    {
                        int compoundDepth = GetEndCompoundDepth(entries, nextBranchIndex, endActiveBranchId);
                        nextBranchIndex -= compoundDepth - 1;
                        index += compoundDepth - 1;

                        blockData.Events = Parse(entries, ref index, ((BranchEventData)entries[nextBranchIndex]).Id);

                        index += compoundDepth - 1;
                    }
                }

                result.Add(parsedEvent);
            }

            return result.ToArray();
        }

        private int GetElseCompoundDepth(EventData[] entries, int nextBranchIndex, out int lastBranchId)
        {
            var branchData = (BranchEventData)entries[nextBranchIndex];
            lastBranchId = branchData.Id;

            if (branchData is ElseEventData)
            {
                var depth = 1;
                for (int i = nextBranchIndex + 1; i < entries.Length; i++)
                {
                    if (entries[i] is not ElseEventData elseData)
                        break;

                    lastBranchId = elseData.Id;

                    depth++;
                }

                return depth;
            }

            return 1;
        }

        private int GetEndCompoundDepth(EventData[] entries, int nextBranchIndex, int endBranchId)
        {
            var branchData = (BranchEventData)entries[nextBranchIndex];

            if (branchData is EndIfEventData endIfData)
            {
                if (endIfData.Id == endBranchId)
                    return 1;

                var depth = 1;
                for (int i = nextBranchIndex - 1; i >= 0; i--)
                {
                    if (entries[i] is not EndIfEventData endIfData2)
                        break;

                    depth++;

                    if (endIfData2.Id == endBranchId)
                        break;
                }

                return depth;
            }

            return 1;
        }

        private int GetNextBranchIndex(EventData[] entries, int index, int branchId)
        {
            for (int i = index; i < entries.Length; i++)
            {
                switch (entries[i])
                {
                    case ElseIfEventData elseIfData when elseIfData.Id == branchId:
                    case ElseEventData elseData when elseData.Id == branchId:
                    case EndIfEventData endIfData when endIfData.Id == branchId:
                        return i;
                }
            }

            return -1;
        }

        private EventData ParseEntry(EventEntryData entry, Encoding textEncoding)
        {
            switch (entry.identifier.Trim())
            {
                case "addExit":
                    return new AddExitEventData
                    {
                        Id = entry.data[0],
                        Value1 = entry.data[1],
                        Value2 = entry.data[2],
                        X = (short)(entry.data[3] | (entry.data[4] << 8)),
                        Y = (short)(entry.data[5] | (entry.data[6] << 8)),
                        RoomId = entry.data[7]
                    };

                case "addPot":
                    return new AddPotEventData
                    {
                        Id = entry.data[0],
                        Value1 = entry.data[1],
                        Value2 = entry.data[2],
                        X = (short)(entry.data[3] | (entry.data[4] << 8)),
                        Y = (short)(entry.data[5] | (entry.data[6] << 8)),
                        Value3 = entry.data[7]
                    };

                case "addEvent":
                    var addEvent = new AddEventEventData
                    {
                        Value1 = entry.data[0],
                        Value2 = (short)(entry.data[1] | (entry.data[2] << 8)),
                        Value3 = entry.data[3],
                        Value4 = entry.data[4],
                        X = (short)(entry.data[5] | (entry.data[6] << 8)),
                        Y = (short)(entry.data[7] | (entry.data[8] << 8))
                    };

                    var nameSize = (short)(entry.data[9] | (entry.data[10] << 8));

                    var nameData = new byte[nameSize];
                    Array.Copy(entry.data, 11, nameData, 0, nameSize);

                    addEvent.Text = textEncoding.GetString(nameData);

                    if (addEvent.Value1 != 4)
                        return addEvent;

                    addEvent.Value5 = entry.data[nameSize + 11];
                    addEvent.Value6 = entry.data[nameSize + 12];

                    return addEvent;

                case "Return":
                    return new ReturnEventData
                    {
                        Value1 = entry.data[0],
                        Value2 = entry.data[1]
                    };

                case "addBG":
                    return new AddBgEventData
                    {
                        DataIndex = entry.data[0]
                    };

                case "setBGFlip":
                    return new SetBgFlipEventData();

                case "addBGObj":
                    return new AddBgObjEventData
                    {
                        Id = entry.data[0],
                        Value1 = (short)(entry.data[1] | (entry.data[2] << 8)),
                        Value2 = (short)(entry.data[3] | (entry.data[4] << 8))
                    };

                case "setObjFlip":
                    return new SetObjFlipEventData
                    {
                        Id = entry.data[0]
                    };

                case "if":
                    return new IfEventData
                    {
                        Id = entry.data[0],
                        Conditions = new[]
                        {
                            new IfConditionData
                            {
                                IsNegate = entry.data[1] == 1,
                                ComparisonType = entry.data[2],
                                ComparisonValue = (short)(entry.data[3] | (entry.data[4] << 8))
                            }
                        }
                    };

                case "elseif":
                    return new ElseIfEventData
                    {
                        Id = entry.data[0],
                        Conditions = new[]
                        {
                            new IfConditionData
                            {
                                IsNegate = entry.data[1] == 1,
                                ComparisonType = entry.data[2],
                                ComparisonValue = (short)(entry.data[3] | (entry.data[4] << 8))
                            }
                        }
                    };

                case "else":
                    return new ElseEventData
                    {
                        Id = entry.data[0]
                    };

                case "endif":
                    return new EndIfEventData
                    {
                        Id = entry.data[0]
                    };

                case "Choice":
                    var choiceEvent = new ChoiceEventData();

                    var identifierSize = (short)(entry.data[1] | (entry.data[2] << 8));

                    var identifierData = new byte[identifierSize];
                    Array.Copy(entry.data, 3, identifierData, 0, identifierSize);

                    choiceEvent.Name = Encoding.ASCII.GetString(identifierData);

                    choiceEvent.Choices = new string[entry.data[0]];
                    int offset = identifierSize + 3;

                    for (byte i = 0; i < choiceEvent.Choices.Length; i++)
                    {
                        identifierSize = (short)(entry.data[offset++] | (entry.data[offset++] << 8));
                        identifierData = new byte[identifierSize];

                        Array.Copy(entry.data, offset, identifierData, 0, identifierSize);
                        offset += identifierSize;

                        choiceEvent.Choices[i] = Encoding.ASCII.GetString(identifierData);
                    }

                    choiceEvent.Value1 = entry.data[offset];
                    return choiceEvent;

                case "Case":
                    return new CaseEventData
                    {
                        Value1 = entry.data[0]
                    };

                case "endChoice":
                    return new EndChoiceEventData();

                case "sendNazoba":
                    return new SendNazobaEventData();

                case "FadeIn":
                    return new FadeInEventData
                    {
                        FrameCount = entry.data[0]
                    };

                case "FadeOut":
                    return new FadeOutEventData
                    {
                        FrameCount = entry.data[0]
                    };

                case "SpFadeOut":
                    return new SpFadeOutEventData
                    {
                        Id = entry.data[0],
                        FrameCount = entry.data[1]
                    };

                case "PluralIn":
                    var ids = new byte[entry.data[0]];
                    for (var i = 0; i < ids.Length; i++)
                        ids[i] = entry.data[i + 1];

                    return new PluralInEventData
                    {
                        Ids = ids,
                        FrameCount = entry.data[ids.Length + 1]
                    };

                case "PluralOut":
                    var ids1 = new byte[entry.data[0]];
                    for (var i = 0; i < ids1.Length; i++)
                        ids1[i] = entry.data[i + 1];

                    return new PluralOutEventData
                    {
                        Ids = ids1,
                        FrameCount = entry.data[ids1.Length + 1]
                    };

                case "SpFadeIn":
                    return new SpFadeInEventData
                    {
                        Id = entry.data[0],
                        FrameCount = entry.data[1]
                    };

                case "ObjFadeOut":
                    return new ObjFadeOutEventData
                    {
                        Id = entry.data[0],
                        Value1 = entry.data[1],
                        FrameCount = entry.data[2],
                    };

                case "ObjFadeIn":
                    return new ObjFadeInEventData
                    {
                        Id = entry.data[0],
                        Value1 = entry.data[1],
                        FrameCount = entry.data[2]
                    };

                case "Vibe":
                    return new VibeEventData
                    {
                        FrameCount = entry.data[0]
                    };

                case "Shake":
                    return new ShakeEventData
                    {
                        FrameCount = entry.data[0],
                        Value1 = entry.data[1]
                    };

                case "ShakeBG":
                    return new ShakeBgEventData
                    {
                        FrameCount = entry.data[0]
                    };

                case "ShakeSP":
                    return new ShakeSpEventData
                    {
                        FrameCount = entry.data[0],
                        Value1 = entry.data[1]
                    };

                case "ShakeALL":
                    return new ShakeAllEventData
                    {
                        FrameCount = entry.data[0]
                    };

                case "Wait":
                    return new WaitEventData
                    {
                        FrameCount = entry.data[0]
                    };

                case "KeyWait":
                    return new KeyWaitEventData();

                case "CQuestion":
                    return new QuestionEventData
                    {
                        Id = entry.data[0]
                    };

                case "CEvent":
                    return new EventEventData
                    {
                        Id = (short)(entry.data[0] | (entry.data[1] << 8))
                    };

                case "setEVState":
                    return new SetEvStateEventData
                    {
                        Index = entry.data[0]
                    };

                case "GameMode":
                    return new GameModeEventData
                    {
                        Value = entry.data[0]
                    };

                case "AddObject":
                    return new AddObjectEventData
                    {
                        Value1 = entry.data[0],
                        Value2 = entry.data[1],
                        Value3 = entry.data[2],
                        Value4 = (short)(entry.data[3] | (entry.data[4] << 8)),
                        Value5 = (short)(entry.data[5] | (entry.data[6] << 8)),
                        Value6 = entry.data[7]
                    };

                case "AddSprite":
                    return new AddSpriteEventData
                    {
                        Value1 = entry.data[0],
                        Value2 = (short)(entry.data[1] | (entry.data[2] << 8)),
                        Value3 = (short)(entry.data[3] | (entry.data[4] << 8)),
                        Value4 = entry.data[5]
                    };

                case "ChangeAni":
                    return new ChangeAniEventData
                    {
                        Value1 = entry.data[0],
                        Value2 = entry.data[1]
                    };

                case "ChangeLAni":
                    return new ChangeLAniEventData
                    {
                        Value1 = entry.data[0],
                        Value2 = entry.data[1]
                    };

                case "reverseSP":
                    return new ReverseSpEventData
                    {
                        Value1 = entry.data[0]
                    };

                case "InfoWindow":
                    return new InfoWindowEventData
                    {
                        Value1 = entry.data[0],
                        Value2 = entry.data[1]
                    };

                case "TextWindow":
                    var textEvent = new TextWindowEventData
                    {
                        Value1 = entry.data[0],
                        PersonId = entry.data[1]
                    };

                    var textLength = (short)(entry.data[2] | (entry.data[3] << 8));
                    var textBytes = new byte[textLength];

                    Array.Copy(entry.data, 4, textBytes, 0, textLength);

                    textEvent.Text = textEncoding.GetString(textBytes);

                    return textEvent;

                case "addMemo":
                    return new AddMemoEventData
                    {
                        Id = entry.data[0]
                    };

                case "BGMask":
                    return new BgMaskEventData
                    {
                        R = entry.data[0],
                        G = entry.data[1],
                        B = entry.data[2],
                        A = entry.data[3]
                    };

                case "PlaySound":
                    var soundEvent = new PlaySoundEventData
                    {
                        Id = entry.data[0]
                    };

                    byte soundFileLength = entry.data[1];
                    var soundFileBytes = new byte[soundFileLength];

                    Array.Copy(entry.data, 2, soundFileBytes, 0, soundFileLength);

                    soundEvent.FileName = Encoding.ASCII.GetString(soundFileBytes);
                    soundEvent.Value2 = entry.data[2 + soundFileLength] == 1;

                    return soundEvent;

                case "StopSound":
                    return new StopSoundEventData
                    {
                        Id = entry.data[0]
                    };

                case "PlayRSound":
                    var soundREvent = new PlayRSoundEventData
                    {
                        Id = entry.data[0]
                    };

                    byte soundRFileLength = entry.data[1];
                    var soundRFileBytes = new byte[soundRFileLength];

                    Array.Copy(entry.data, 2, soundRFileBytes, 0, soundRFileLength);

                    soundREvent.FileName = Encoding.ASCII.GetString(soundRFileBytes);
                    soundREvent.Value2 = entry.data[2 + soundRFileLength] == 1;

                    return soundREvent;

                case "PlayMovie":
                    var movieEvent = new PlayMovieEventData
                    {
                        Id = entry.data[0]
                    };

                    byte moveFileLength = entry.data[1];
                    var moveFileBytes = new byte[moveFileLength];

                    Array.Copy(entry.data, 2, moveFileBytes, 0, moveFileLength);

                    movieEvent.FileName = Encoding.ASCII.GetString(moveFileBytes);

                    return movieEvent;

                case "SetBG":
                    return new SetBgEventData
                    {
                        Value1 = entry.data[0]
                    };

                case "reverseBG":
                    return new ReverseBgEventData();

                case "inTitle":
                    return new InTitleEventData();

                case "LukeLetter":
                    return new LukeLetterEventData();

                case "AdhereMsg":
                    return new AdhereMsgEventData();

                case "SolMystery":
                    var solValues = new byte[entry.data[0]];
                    for (var i = 0; i < solValues.Length; i++)
                        solValues[i] = (byte)(entry.data[i + 1] - 1);

                    return new SolMysteryEventData
                    {
                        Values = solValues
                    };

                case "Occur":
                    return new OccurEventData
                    {
                        Value1 = entry.data[0]
                    };

                case "Achieve":
                    return new AchieveEventData
                    {
                        Value1 = entry.data[0]
                    };

                case "comMove":
                    return new ComMoveEventData
                    {
                        Value1 = entry.data[0]
                    };

                case "updateMemo":
                    return new UpdateMemoEventData();

                case "setStory":
                    return new SetStoryEventData
                    {
                        Id = entry.data[0]
                    };

                case "setBitFlg":
                    return new SetBitFlgEventData
                    {
                        Index = entry.data[0]
                    };

                case "offBitFlg":
                    return new OffBitFlgEventData
                    {
                        Index = entry.data[0]
                    };

                case "setItemFlg":
                    return new SetItemFlgEventData
                    {
                        Id = entry.data[0],
                        Index = entry.data[1]
                    };

                case "setRiddle":
                    return new SetRiddleEventData
                    {
                        Value1 = entry.data[0],
                        Value2 = entry.data[1]
                    };

                case "LoadData":
                    var loadDataEvent = new LoadDataEventData
                    {
                        Files = new string[entry.data[0]]
                    };

                    var loadOffset = 1;
                    for (var i = 0; i < loadDataEvent.Files.Length; i++)
                    {
                        int loadLength = entry.data[loadOffset++];
                        var loadData = new byte[loadLength];

                        Array.Copy(entry.data, loadOffset, loadData, 0, loadLength);
                        loadOffset += loadLength;

                        loadDataEvent.Files[i] = Encoding.ASCII.GetString(loadData);
                    }

                    return loadDataEvent;

                case "LoadBG":
                    var loadBgData = new LoadBgEventData
                    {
                        Files = new string[entry.data[0]]
                    };

                    var bgOffset = 1;
                    for (var i = 0; i < loadBgData.Files.Length; i++)
                    {
                        int loadLength = entry.data[bgOffset++];
                        var loadData = new byte[loadLength];

                        Array.Copy(entry.data, bgOffset, loadData, 0, loadLength);
                        bgOffset += loadLength;

                        loadBgData.Files[i] = Encoding.ASCII.GetString(loadData);
                    }

                    return loadBgData;

                case "LoadSP":
                    var loadSpData = new LoadSpEventData
                    {
                        Files = new string[entry.data[0]]
                    };

                    for (var i = 0; i < loadSpData.Files.Length; i++)
                    {
                        if (entry.data[i + 1] == 0)
                            loadSpData.Files[i] = "layton.dat";
                        else if (entry.data[i + 1] == 1)
                            loadSpData.Files[i] = "luke.dat";
                        else if (entry.data[i + 1] == 2)
                            loadSpData.Files[i] = "bridge.dat";
                    }

                    return loadSpData;

                case "EndScript":
                    return new EndScriptEventData();

                default:
                    throw new InvalidOperationException($"cmd={entry.identifier}");
            }
        }
    }
}
