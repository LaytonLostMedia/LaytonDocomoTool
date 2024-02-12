using System.Text;
using Logic.Domain.Level5Management.Docomo.Contract.Script;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Script
{
    internal class ScriptComposer : IScriptComposer
    {
        public ScriptComposer()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public EventEntryData[] Compose(EventData[] events)
        {
            var result = new List<EventEntryData>();

            Compose(events, result);

            return result.ToArray();
        }

        private void Compose(EventData[] events, IList<EventEntryData> entries)
        {
            for (var i = 0; i < events.Length;)
            {
                EventEntryData composedEventEntry = ComposeEntry(events[i]);
                composedEventEntry.identifier = composedEventEntry.identifier.PadRight(10);

                entries.Add(composedEventEntry);

                if (events[i++] is BranchBlockEventData branchData)
                    Compose(branchData.Events, entries);
            }
        }

        private EventEntryData ComposeEntry(EventData eventData)
        {
            switch (eventData)
            {
                case AddExitEventData addExit:
                    return new EventEntryData
                    {
                        identifier = "addExit",
                        dataSize = 8,
                        data = new[]
                        {
                            addExit.Id,
                            addExit.Value1,
                            addExit.Value2,
                            (byte)addExit.X, (byte)(addExit.X >> 8),
                            (byte)addExit.Y, (byte)(addExit.Y >> 8),
                            addExit.RoomId
                        }
                    };

                case AddPotEventData addPot:
                    return new EventEntryData
                    {
                        identifier = "addPot",
                        dataSize = 8,
                        data = new[]
                        {
                            addPot.Id,
                            addPot.Value1,
                            addPot.Value2,
                            (byte)addPot.X, (byte)(addPot.X >> 8),
                            (byte)addPot.Y, (byte)(addPot.Y >> 8),
                            addPot.Value3
                        }
                    };

                case AddEventEventData addEvent:
                    var addEventEntry = new EventEntryData
                    {
                        identifier = "addEvent"
                    };

                    var addEventBytes = new List<byte>();
                    addEventBytes.AddRange(new[]
                    {
                        addEvent.Value1,
                        (byte)addEvent.Value2, (byte)(addEvent.Value2 >> 8),
                        addEvent.Value3,
                        addEvent.Value4,
                        (byte)addEvent.X, (byte)(addEvent.X >> 8),
                        (byte)addEvent.Y, (byte)(addEvent.Y >> 8),
                    });

                    byte[] textBytes = Encoding.GetEncoding("Shift-JIS").GetBytes(addEvent.Text);

                    addEventBytes.AddRange(new[] { (byte)textBytes.Length, (byte)(textBytes.Length >> 8) });
                    addEventBytes.AddRange(textBytes);

                    if (addEvent.Value1 != 4)
                    {
                        addEventEntry.dataSize = (short)addEventBytes.Count;
                        addEventEntry.data = addEventBytes.ToArray();

                        return addEventEntry;
                    }

                    if (addEvent.Value5 == null || addEvent.Value6 == null)
                        throw new InvalidOperationException("AddEvent requires 9 parameters if first parameter has value 4.");

                    addEventBytes.Add(addEvent.Value5.Value);
                    addEventBytes.Add(addEvent.Value6.Value);

                    addEventEntry.dataSize = (short)addEventBytes.Count;
                    addEventEntry.data = addEventBytes.ToArray();

                    return addEventEntry;

                case ReturnEventData returnEvent:
                    return new EventEntryData
                    {
                        identifier = "Return",
                        dataSize = 2,
                        data = new[]
                        {
                            returnEvent.Value1,
                            returnEvent.Value2
                        }
                    };

                case AddBgEventData addBg:
                    return new EventEntryData
                    {
                        identifier = "addBG",
                        dataSize = 1,
                        data = new[]
                        {
                            addBg.DataIndex
                        }
                    };

                case SetBgFlipEventData:
                    return new EventEntryData
                    {
                        identifier = "setBGFlip",
                        dataSize = 0,
                        data = Array.Empty<byte>()
                    };

                case AddBgObjEventData addBgObj:
                    return new EventEntryData
                    {
                        identifier = "addBGObj",
                        dataSize = 5,
                        data = new[]
                        {
                            addBgObj.Id,
                            (byte)addBgObj.Value1, (byte)(addBgObj.Value1 >> 8),
                            (byte)addBgObj.Value2, (byte)(addBgObj.Value2 >> 8)
                        }
                    };

                case SetObjFlipEventData setObjFlip:
                    return new EventEntryData
                    {
                        identifier = "setObjFlip",
                        dataSize = 1,
                        data = new[]
                        {
                            setObjFlip.Id
                        }
                    };

                case ElseIfEventData elseIfData:
                    return new EventEntryData
                    {
                        identifier = "elseif",
                        dataSize = 5,
                        data = new[]
                        {
                            elseIfData.Id,
                            (byte)(elseIfData.IsNegate ? 1 : 0),
                            elseIfData.ComparisonType,
                            (byte)elseIfData.ComparisonValue, (byte)(elseIfData.ComparisonValue >> 8)
                        }
                    };

                case IfEventData ifData:
                    return new EventEntryData
                    {
                        identifier = "if",
                        dataSize = 5,
                        data = new[]
                        {
                            ifData.Id,
                            (byte)(ifData.IsNegate ? 1 : 0),
                            ifData.ComparisonType,
                            (byte)ifData.ComparisonValue, (byte)(ifData.ComparisonValue >> 8)
                        }
                    };

                case ElseEventData elseData:
                    return new EventEntryData
                    {
                        identifier = "else",
                        dataSize = 1,
                        data = new[]
                        {
                            elseData.Id
                        }
                    };

                case EndIfEventData endIfData:
                    return new EventEntryData
                    {
                        identifier = "endif",
                        dataSize = 1,
                        data = new[]
                        {
                            endIfData.Id
                        }
                    };

                case ChoiceEventData choiceData:
                    var choiceEntry = new EventEntryData
                    {
                        identifier = "Choice"
                    };

                    var choiceBytes = new List<byte>
                    {
                        (byte)choiceData.Choices.Length,
                        (byte)choiceData.Name.Length, (byte)(choiceData.Name.Length >> 8)
                    };

                    choiceBytes.AddRange(Encoding.ASCII.GetBytes(choiceData.Name));

                    for (byte i = 0; i < choiceData.Choices.Length; i++)
                    {
                        choiceBytes.AddRange(new[] { (byte)choiceData.Choices[i].Length, (byte)(choiceData.Choices[i].Length >> 8) });
                        choiceBytes.AddRange(Encoding.ASCII.GetBytes(choiceData.Choices[i]));
                    }

                    choiceBytes.Add(choiceData.Value1);

                    addEventEntry.dataSize = (short)choiceBytes.Count;
                    addEventEntry.data = choiceBytes.ToArray();

                    return choiceEntry;

                case CaseEventData caseData:
                    return new EventEntryData
                    {
                        identifier = "Case",
                        dataSize = 1,
                        data = new[]
                        {
                            caseData.Value1
                        }
                    };

                case EndChoiceEventData:
                    return new EventEntryData
                    {
                        identifier = "endChoice",
                        dataSize = 0,
                        data = Array.Empty<byte>()
                    };

                case SendNazobaEventData:
                    return new EventEntryData
                    {
                        identifier = "sendNazoba",
                        dataSize = 0,
                        data = Array.Empty<byte>()
                    };

                case FadeInEventData fadeIn:
                    return new EventEntryData
                    {
                        identifier = "FadeIn",
                        dataSize = 1,
                        data = new[]
                        {
                            fadeIn.FrameCount
                        }
                    };

                case FadeOutEventData fadeOut:
                    return new EventEntryData
                    {
                        identifier = "FadeOut",
                        dataSize = 1,
                        data = new[]
                        {
                            fadeOut.FrameCount
                        }
                    };

                case SpFadeOutEventData spFadeOut:
                    return new EventEntryData
                    {
                        identifier = "SpFadeOut",
                        dataSize = 2,
                        data = new[]
                        {
                            spFadeOut.Id,
                            spFadeOut.FrameCount
                        }
                    };

                case PluralInEventData pluralIn:
                    var pluralInEntry = new EventEntryData
                    {
                        identifier = "PluralIn"
                    };

                    var pluralInBytes = new List<byte>
                    {
                        (byte)pluralIn.Ids.Length
                    };

                    pluralInBytes.AddRange(pluralIn.Ids);
                    pluralInBytes.Add(pluralIn.FrameCount);

                    pluralInEntry.dataSize = (short)pluralInBytes.Count;
                    pluralInEntry.data = pluralInBytes.ToArray();

                    return pluralInEntry;

                case PluralOutEventData pluralOut:
                    var pluralOutEntry = new EventEntryData
                    {
                        identifier = "PluralOut"
                    };

                    var pluralOutBytes = new List<byte>
                    {
                        (byte)pluralOut.Ids.Length
                    };

                    pluralOutBytes.AddRange(pluralOut.Ids);
                    pluralOutBytes.Add(pluralOut.FrameCount);

                    pluralOutEntry.dataSize = (short)pluralOutBytes.Count;
                    pluralOutEntry.data = pluralOutBytes.ToArray();

                    return pluralOutEntry;

                case SpFadeInEventData spFadeIn:
                    return new EventEntryData
                    {
                        identifier = "SpFadeIn",
                        dataSize = 2,
                        data = new[]
                        {
                            spFadeIn.Id,
                            spFadeIn.FrameCount
                        }
                    };

                case ObjFadeOutEventData objFadeOut:
                    return new EventEntryData
                    {
                        identifier = "ObjFadeOut",
                        dataSize = 3,
                        data = new[]
                        {
                            objFadeOut.Id,
                            objFadeOut.Value1,
                            objFadeOut.FrameCount
                        }
                    };

                case ObjFadeInEventData objFadeIn:
                    return new EventEntryData
                    {
                        identifier = "ObjFadeIn",
                        dataSize = 3,
                        data = new[]
                        {
                            objFadeIn.Id,
                            objFadeIn.Value1,
                            objFadeIn.FrameCount
                        }
                    };

                case VibeEventData vibe:
                    return new EventEntryData
                    {
                        identifier = "Vibe",
                        dataSize = 1,
                        data = new[]
                        {
                            vibe.FrameCount
                        }
                    };

                case ShakeEventData shake:
                    return new EventEntryData
                    {
                        identifier = "Shake",
                        dataSize = 3,
                        data = new[]
                        {
                            shake.FrameCount,
                            shake.Value1
                        }
                    };

                case ShakeBgEventData shakeBg:
                    return new EventEntryData
                    {
                        identifier = "ShakeBG",
                        dataSize = 1,
                        data = new[]
                        {
                            shakeBg.FrameCount
                        }
                    };

                case ShakeSpEventData shakeSp:
                    return new EventEntryData
                    {
                        identifier = "ShakeSP",
                        dataSize = 2,
                        data = new[]
                        {
                            shakeSp.FrameCount,
                            shakeSp.Value1
                        }
                    };

                case ShakeAllEventData shakeAll:
                    return new EventEntryData
                    {
                        identifier = "ShakeALL",
                        dataSize = 1,
                        data = new[]
                        {
                            shakeAll.FrameCount
                        }
                    };

                case WaitEventData wait:
                    return new EventEntryData
                    {
                        identifier = "Wait",
                        dataSize = 1,
                        data = new[]
                        {
                            wait.FrameCount
                        }
                    };

                case KeyWaitEventData:
                    return new EventEntryData
                    {
                        identifier = "KeyWait",
                        dataSize = 0,
                        data = Array.Empty<byte>()
                    };

                case QuestionEventData question:
                    return new EventEntryData
                    {
                        identifier = "CQuestion",
                        dataSize = 1,
                        data = new[]
                        {
                            question.Id
                        }
                    };

                case EventEventData cEventData:
                    return new EventEntryData
                    {
                        identifier = "CEvent",
                        dataSize = 2,
                        data = new[]
                        {
                            (byte)cEventData.Id, (byte)(cEventData.Id >> 8)
                        }
                    };

                case SetEvStateEventData setEvState:
                    return new EventEntryData
                    {
                        identifier = "setEVState",
                        dataSize = 1,
                        data = new[]
                        {
                            setEvState.Index
                        }
                    };

                case GameModeEventData gameMode:
                    return new EventEntryData
                    {
                        identifier = "GameMode",
                        dataSize = 1,
                        data = new[]
                        {
                            gameMode.Value
                        }
                    };

                case AddObjectEventData addObject:
                    return new EventEntryData
                    {
                        identifier = "AddObject",
                        dataSize = 8,
                        data = new[]
                        {
                            addObject.Value1,
                            addObject.Value2,
                            addObject.Value3,
                            (byte)addObject.Value4, (byte)(addObject.Value4 >> 8),
                            (byte)addObject.Value5, (byte)(addObject.Value5 >> 8),
                            addObject.Value6
                        }
                    };

                case AddSpriteEventData addSprite:
                    return new EventEntryData
                    {
                        identifier = "AddSprite",
                        dataSize = 6,
                        data = new[]
                        {
                            addSprite.Value1,
                            (byte)addSprite.Value2, (byte)(addSprite.Value2 >> 8),
                            (byte)addSprite.Value3, (byte)(addSprite.Value3 >> 8),
                            addSprite.Value4
                        }
                    };

                case ChangeAniEventData changeAni:
                    return new EventEntryData
                    {
                        identifier = "ChangeAni",
                        dataSize = 2,
                        data = new[]
                        {
                            changeAni.Value1,
                            changeAni.Value2
                        }
                    };

                case ChangeLAniEventData changeLAni:
                    return new EventEntryData
                    {
                        identifier = "ChangeLAni",
                        dataSize = 2,
                        data = new[]
                        {
                            changeLAni.Value1,
                            changeLAni.Value2
                        }
                    };

                case ReverseSpEventData reverseSp:
                    return new EventEntryData
                    {
                        identifier = "reverseSP",
                        dataSize = 1,
                        data = new[]
                        {
                            reverseSp.Value1
                        }
                    };

                case InfoWindowEventData infoWindow:
                    return new EventEntryData
                    {
                        identifier = "InfoWindow",
                        dataSize = 2,
                        data = new[]
                        {
                            infoWindow.Value1,
                            infoWindow.Value2
                        }
                    };

                case TextWindowEventData textWindow:
                    var textEvent = new EventEntryData
                    {
                        identifier = "TextWindow"
                    };

                    var textWindowBytes = new List<byte>
                    {
                        textWindow.Value1,
                        textWindow.PersonId
                    };

                    byte[] textBytes1 = Encoding.GetEncoding("Shift-JIS").GetBytes(textWindow.Text);

                    textWindowBytes.AddRange(new[] { (byte)textBytes1.Length, (byte)(textBytes1.Length >> 8) });
                    textWindowBytes.AddRange(textBytes1);

                    textEvent.dataSize = (short)textWindowBytes.Count;
                    textEvent.data = textWindowBytes.ToArray();

                    return textEvent;

                case AddMemoEventData addMemo:
                    return new EventEntryData
                    {
                        identifier = "addMemo",
                        dataSize = 1,
                        data = new[]
                        {
                            addMemo.Id
                        }
                    };

                case BgMaskEventData bgMask:
                    return new EventEntryData
                    {
                        identifier = "BGMask",
                        dataSize = 4,
                        data = new[]
                        {
                            bgMask.R,
                            bgMask.G,
                            bgMask.B,
                            bgMask.A,
                        }
                    };

                case PlaySoundEventData playSound:
                    var playSoundEvent = new EventEntryData
                    {
                        identifier = "PlaySound"
                    };

                    var playSoundBytes = new List<byte>
                    {
                        playSound.Id,
                        (byte)playSound.FileName.Length
                    };

                    playSoundBytes.AddRange(Encoding.ASCII.GetBytes(playSound.FileName));

                    playSoundBytes.Add((byte)(playSound.Value2 ? 1 : 0));

                    playSoundEvent.dataSize = (short)playSoundBytes.Count;
                    playSoundEvent.data = playSoundBytes.ToArray();

                    return playSoundEvent;

                case StopSoundEventData stopSound:
                    return new EventEntryData
                    {
                        identifier = "StopSound",
                        dataSize = 1,
                        data = new[]
                        {
                            stopSound.Id
                        }
                    };

                case PlayRSoundEventData playRSound:
                    var playRSoundEvent = new EventEntryData
                    {
                        identifier = "PlayRSound"
                    };

                    var playRSoundBytes = new List<byte>
                    {
                        playRSound.Id,
                        (byte)playRSound.FileName.Length
                    };

                    playRSoundBytes.AddRange(Encoding.ASCII.GetBytes(playRSound.FileName));

                    playRSoundBytes.Add((byte)(playRSound.Value2 ? 1 : 0));

                    playRSoundEvent.dataSize = (short)playRSoundBytes.Count;
                    playRSoundEvent.data = playRSoundBytes.ToArray();

                    return playRSoundEvent;

                case PlayMovieEventData playMovie:
                    var playMovieEvent = new EventEntryData
                    {
                        identifier = "PlayMovie"
                    };

                    var playMovieBytes = new List<byte>
                    {
                        playMovie.Id,
                        (byte)playMovie.FileName.Length
                    };

                    playMovieBytes.AddRange(Encoding.ASCII.GetBytes(playMovie.FileName));

                    playMovieEvent.dataSize = (short)playMovieBytes.Count;
                    playMovieEvent.data = playMovieBytes.ToArray();

                    return playMovieEvent;

                case SetBgEventData setBg:
                    return new EventEntryData
                    {
                        identifier = "SetBG",
                        dataSize = 1,
                        data = new[]
                        {
                            setBg.Value1
                        }
                    };

                case ReverseBgEventData:
                    return new EventEntryData
                    {
                        identifier = "reverseBG",
                        dataSize = 0,
                        data = Array.Empty<byte>()
                    };

                case InTitleEventData:
                    return new EventEntryData
                    {
                        identifier = "inTitle",
                        dataSize = 0,
                        data = Array.Empty<byte>()
                    };

                case LukeLetterEventData:
                    return new EventEntryData
                    {
                        identifier = "LukeLetter",
                        dataSize = 0,
                        data = Array.Empty<byte>()
                    };

                case AdhereMsgEventData:
                    return new EventEntryData
                    {
                        identifier = "AdhereMsg",
                        dataSize = 0,
                        data = Array.Empty<byte>()
                    };

                case SolMysteryEventData solMystery:
                    return new EventEntryData
                    {
                        identifier = "SolMystery",
                        dataSize = (short)solMystery.Values.Length,
                        data = solMystery.Values
                    };

                case OccurEventData occur:
                    return new EventEntryData
                    {
                        identifier = "Occur",
                        dataSize = 1,
                        data = new[]
                        {
                            occur.Value1
                        }
                    };

                case AchieveEventData achieve:
                    return new EventEntryData
                    {
                        identifier = "Achieve",
                        dataSize = 1,
                        data = new[]
                        {
                            achieve.Value1
                        }
                    };

                case ComMoveEventData comMove:
                    return new EventEntryData
                    {
                        identifier = "comMove",
                        dataSize = 1,
                        data = new[]
                        {
                            comMove.Value1
                        }
                    };

                case UpdateMemoEventData:
                    return new EventEntryData
                    {
                        identifier = "updateMemo",
                        dataSize = 0,
                        data = Array.Empty<byte>()
                    };

                case SetStoryEventData setStory:
                    return new EventEntryData
                    {
                        identifier = "setStory",
                        dataSize = 1,
                        data = new[]
                        {
                            setStory.Id
                        }
                    };

                case SetBitFlgEventData setBitFlg:
                    return new EventEntryData
                    {
                        identifier = "setBitFlg",
                        dataSize = 1,
                        data = new[]
                        {
                            setBitFlg.Index
                        }
                    };

                case OffBitFlgEventData offBitFlg:
                    return new EventEntryData
                    {
                        identifier = "offBitFlg",
                        dataSize = 1,
                        data = new[]
                        {
                            offBitFlg.Index
                        }
                    };

                case SetItemFlgEventData setItemFlg:
                    return new EventEntryData
                    {
                        identifier = "setItemFlg",
                        dataSize = 2,
                        data = new[]
                        {
                            setItemFlg.Id,
                            setItemFlg.Index
                        }
                    };

                case SetRiddleEventData setRiddle:
                    return new EventEntryData
                    {
                        identifier = "setRiddle",
                        dataSize = 2,
                        data = new[]
                        {
                            setRiddle.Value1,
                            setRiddle.Value2
                        }
                    };

                case LoadDataEventData loadData:
                    var loadDataEntry = new EventEntryData
                    {
                        identifier = "LoadData"
                    };

                    var loadDataBytes = new List<byte>
                    {
                        (byte)loadData.Files.Length
                    };

                    foreach (string file in loadData.Files)
                    {
                        loadDataBytes.Add((byte)file.Length);
                        loadDataBytes.AddRange(Encoding.ASCII.GetBytes(file));
                    }

                    loadDataEntry.dataSize = (short)loadDataBytes.Count;
                    loadDataEntry.data = loadDataBytes.ToArray();

                    return loadDataEntry;

                case LoadBgEventData loadBg:
                    var loadBgEntry = new EventEntryData
                    {
                        identifier = "LoadBG"
                    };

                    var loadBgBytes = new List<byte>
                    {
                        (byte)loadBg.Files.Length
                    };

                    foreach (string file in loadBg.Files)
                    {
                        loadBgBytes.Add((byte)file.Length);
                        loadBgBytes.AddRange(Encoding.ASCII.GetBytes(file));
                    }

                    loadBgEntry.dataSize = (short)loadBgBytes.Count;
                    loadBgEntry.data = loadBgBytes.ToArray();

                    return loadBgEntry;

                case LoadSpEventData loadSp:
                    var loadSpEntry = new EventEntryData
                    {
                        identifier = "LoadSP"
                    };

                    var loadSpBytes = new List<byte>
                    {
                        (byte)loadSp.Files.Length
                    };

                    foreach (string file in loadSp.Files)
                    {
                        switch (file)
                        {
                            case "layton.dat":
                                loadSpBytes.Add(0);
                                break;

                            case "luke.dat":
                                loadSpBytes.Add(1);
                                break;

                            case "bridge.dat":
                                loadSpBytes.Add(2);
                                break;

                            default:
                                throw new InvalidOperationException($"Invalid SP file '{file}' to load.");
                        }
                    }

                    loadSpEntry.dataSize = (short)loadSpBytes.Count;
                    loadSpEntry.data = loadSpBytes.ToArray();

                    return loadSpEntry;

                case EndScriptEventData:
                    return new EventEntryData
                    {
                        identifier = "EndScript",
                        dataSize = 0,
                        data = Array.Empty<byte>()
                    };

                default:
                    throw new InvalidOperationException($"Invalid event {eventData.GetType().Name}.");
            }
        }
    }
}
