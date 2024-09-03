using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.CodeAnalysis.Contract.DataClasses;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;

namespace Logic.Business.LaytonDocomoTool
{
    internal class Level5DocomoEventDataConverter : ILevel5DocomoEventDataConverter
    {
        private readonly ILevel5DocomoSyntaxFactory _syntaxFactory;
        private readonly IScriptParameterMapper _parameterMapper;

        public Level5DocomoEventDataConverter(ILevel5DocomoSyntaxFactory syntaxFactory, IScriptParameterMapper parameterMapper)
        {
            _syntaxFactory = syntaxFactory;
            _parameterMapper = parameterMapper;
        }

        public CodeUnitSyntax CreateCodeUnit(EventData[] events)
        {
            IReadOnlyList<StatementSyntax> statements = CreateStatements(events);
            var codeUnit = new CodeUnitSyntax(statements);

            codeUnit.Update();

            return codeUnit;
        }

        private IReadOnlyList<StatementSyntax> CreateStatements(EventData[] events)
        {
            var result = new List<StatementSyntax>();

            for (var i = 0; i < events.Length;)
            {
                EventData currentEvent = events[i++];

                StatementSyntax statement = currentEvent is IfEventData { Events: { } } ifData ?
                    CreateIfElseStatement(ifData, events, ref i) :
                    CreateFunctionInvocationStatement(currentEvent);

                result.Add(statement);
            }

            return result;
        }

        private IfElseStatementSyntax CreateIfElseStatement(IfEventData ifData, EventData[] events, ref int index)
        {
            IfExpressionSyntax ifExpression = CreateIfExpression(ifData);
            BlockExpressionSyntax blockExpression = CreateBlockExpression(ifData.Events);

            var elseStatements = new List<ElseStatementSyntax>();
            while (events[index] is ElseIfEventData or ElseEventData)
            {
                EventData elseEvent = events[index++];

                if (elseEvent is ElseIfEventData elseIfData)
                    elseStatements.Add(CreateElseIfStatement(elseIfData));
                else if (elseEvent is ElseEventData elseData)
                    elseStatements.Add(CreateElseStatement(elseData));
                else
                    break;
            }

            if (events[index++] is not EndIfEventData)
                throw new InvalidOperationException("If-else-statement was not properly closed.");

            return new IfElseStatementSyntax(ifExpression, blockExpression, elseStatements);
        }

        private ElseStatementSyntax CreateElseStatement(ElseEventData elseData)
        {
            SyntaxToken elseToken = _syntaxFactory.Token(Level5DocomoTokenKind.ElseKeyword);
            BlockExpressionSyntax blockExpression = CreateBlockExpression(elseData.Events);

            return new ElseStatementSyntax(elseToken, blockExpression);
        }

        private ElseIfStatementSyntax CreateElseIfStatement(ConditionalBranchBlockEventData conditionalBranchData)
        {
            SyntaxToken elseToken = _syntaxFactory.Token(Level5DocomoTokenKind.ElseKeyword);
            IfExpressionSyntax ifExpression = CreateIfExpression(conditionalBranchData);
            BlockExpressionSyntax blockExpression = CreateBlockExpression(conditionalBranchData.Events);

            return new ElseIfStatementSyntax(elseToken, ifExpression, blockExpression);
        }

        private IfExpressionSyntax CreateIfExpression(ConditionalBranchBlockEventData conditionalBranchData)
        {
            SyntaxToken ifToken = _syntaxFactory.Token(Level5DocomoTokenKind.IfKeyword);
            ExpressionSyntax comparisonExpression = CreateConditionExpression(conditionalBranchData);

            return new IfExpressionSyntax(ifToken, comparisonExpression);
        }

        private ExpressionSyntax CreateConditionExpression(ConditionalBranchBlockEventData conditionalBranchData)
        {
            ExpressionSyntax leftExpression = CreateIfConditionExpression(conditionalBranchData.Conditions[0]);

            for (var i = 1; i < conditionalBranchData.Conditions.Length; i++)
            {
                ExpressionSyntax rightExpression = CreateIfConditionExpression(conditionalBranchData.Conditions[i]);

                leftExpression = CreateAndLogicalExpression(leftExpression, rightExpression);
            }

            return leftExpression;
        }

        private BlockExpressionSyntax CreateBlockExpression(EventData[] events)
        {
            SyntaxToken curlyOpen = _syntaxFactory.Token(Level5DocomoTokenKind.CurlyOpen);
            SyntaxToken curlyClose = _syntaxFactory.Token(Level5DocomoTokenKind.CurlyClose);

            var result = new List<StatementSyntax>();

            for (var i = 0; i < events.Length;)
            {
                EventData currentEvent = events[i++];

                StatementSyntax statement = currentEvent is IfEventData ifData ?
                    CreateIfElseStatement(ifData, events, ref i) :
                    CreateFunctionInvocationStatement(currentEvent);

                result.Add(statement);
            }

            return new BlockExpressionSyntax(curlyOpen, result, curlyClose);
        }

        private UnaryExpressionSyntax CreateNotUnaryExpression(ExpressionSyntax expression)
        {
            SyntaxToken notToken = _syntaxFactory.Token(Level5DocomoTokenKind.NotKeyword);

            return new UnaryExpressionSyntax(notToken, expression);
        }

        private LogicalExpressionSyntax CreateAndLogicalExpression(ExpressionSyntax leftExpression, ExpressionSyntax rightExpression)
        {
            SyntaxToken andToken = _syntaxFactory.Token(Level5DocomoTokenKind.AndKeyword);

            return new LogicalExpressionSyntax(leftExpression, andToken, rightExpression);
        }

        private FunctionInvocationStatementSyntax CreateFunctionInvocationStatement(EventData eventData)
        {
            FunctionInvocationExpressionSyntax functionInvocation = CreateFunctionInvocationExpression(eventData);
            SyntaxToken semicolon = _syntaxFactory.Token(Level5DocomoTokenKind.Semicolon);

            return new FunctionInvocationStatementSyntax(functionInvocation, semicolon);
        }

        private FunctionInvocationExpressionSyntax CreateFunctionInvocationExpression(EventData eventData)
        {
            NameSyntax simpleName = CreateFunctionName(eventData);
            FunctionParametersSyntax functionParameters = CreateFunctionParameters(eventData);

            return new FunctionInvocationExpressionSyntax(simpleName, functionParameters);
        }

        private ExpressionSyntax CreateIfConditionExpression(IfConditionData conditionData)
        {
            var indexName = new SimpleNameSyntax(_syntaxFactory.Identifier("MemoIndex"));
            ExpressionSyntax valueExpression = CreateValueExpression(conditionData.ComparisonValue);

            ExpressionSyntax conditionExpression;
            var applyNotCondition = true;

            switch (conditionData.ComparisonType)
            {
                case 0:
                    SyntaxToken equalsToken = conditionData.IsNegate ?
                        _syntaxFactory.Token(Level5DocomoTokenKind.NotEquals) :
                        _syntaxFactory.Token(Level5DocomoTokenKind.EqualsEquals);

                    conditionExpression = new BinaryExpressionSyntax(indexName, equalsToken, valueExpression);
                    applyNotCondition = false;
                    break;

                case 1:
                    SyntaxToken smallerToken = _syntaxFactory.Token(Level5DocomoTokenKind.SmallerThan);

                    conditionExpression = new BinaryExpressionSyntax(indexName, smallerToken, valueExpression);
                    break;

                case 2:
                    SyntaxToken greaterToken = _syntaxFactory.Token(Level5DocomoTokenKind.GreaterThan);

                    conditionExpression = new BinaryExpressionSyntax(indexName, greaterToken, valueExpression);
                    break;

                case 3:
                    SyntaxToken smallerEqualsToken = _syntaxFactory.Token(Level5DocomoTokenKind.SmallerEquals);

                    conditionExpression = new BinaryExpressionSyntax(indexName, smallerEqualsToken, valueExpression);
                    break;

                case 4:
                    SyntaxToken greaterEqualsToken = _syntaxFactory.Token(Level5DocomoTokenKind.GreaterEquals);

                    conditionExpression = new BinaryExpressionSyntax(indexName, greaterEqualsToken, valueExpression);
                    break;

                default:
                    NameSyntax simpleName = CreateFunctionName(conditionData);
                    FunctionParametersSyntax functionParameters = CreateFunctionParameters(conditionData);

                    conditionExpression = new FunctionInvocationExpressionSyntax(simpleName, functionParameters);
                    break;
            }

            if (applyNotCondition && conditionData.IsNegate)
                conditionExpression = CreateNotUnaryExpression(conditionExpression);

            return conditionExpression;
        }

        private FunctionParametersSyntax CreateFunctionParameters(EventData eventData)
        {
            SyntaxToken parenOpen = _syntaxFactory.Token(Level5DocomoTokenKind.ParenOpen);
            SyntaxToken parenClose = _syntaxFactory.Token(Level5DocomoTokenKind.ParenClose);

            CommaSeparatedSyntaxList<ExpressionSyntax> parameters = CreateCommaSeparatedList(eventData);

            return new FunctionParametersSyntax(parenOpen, parameters, parenClose);
        }

        private FunctionParametersSyntax CreateFunctionParameters(IfConditionData conditionData)
        {
            SyntaxToken parenOpen = _syntaxFactory.Token(Level5DocomoTokenKind.ParenOpen);
            SyntaxToken parenClose = _syntaxFactory.Token(Level5DocomoTokenKind.ParenClose);

            ExpressionSyntax compareValueExpression;
            CommaSeparatedSyntaxList<ExpressionSyntax> parameters;

            switch (conditionData.ComparisonType)
            {
                case 5 when conditionData.ComparisonValue is 0:
                case 6 when conditionData.ComparisonValue is 0:
                case 7:
                case 8:
                case 13:
                    parameters = new CommaSeparatedSyntaxList<ExpressionSyntax>(Array.Empty<ExpressionSyntax>());
                    break;

                case 12:
                    compareValueExpression = _parameterMapper.TryGetBitName(conditionData.ComparisonValue, out string name) ?
                        CreateValueExpression(name) :
                        CreateValueExpression(conditionData.ComparisonValue);

                    parameters = new CommaSeparatedSyntaxList<ExpressionSyntax>(new[] { compareValueExpression });
                    break;

                case 14:
                    compareValueExpression = _parameterMapper.TryGetStoryName(conditionData.ComparisonValue, out string name1) ?
                        CreateValueExpression(name1) :
                        CreateValueExpression(conditionData.ComparisonValue);

                    parameters = new CommaSeparatedSyntaxList<ExpressionSyntax>(new[] { compareValueExpression });
                    break;

                default:
                    compareValueExpression = CreateValueExpression(conditionData.ComparisonValue);
                    parameters = new CommaSeparatedSyntaxList<ExpressionSyntax>(new[] { compareValueExpression });
                    break;
            }

            return new FunctionParametersSyntax(parenOpen, parameters, parenClose);
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AchieveEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AddBgEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AddBgObjEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AddEventEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AddExitEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AddMemoEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AddObjectEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AddPotEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AddSpriteEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AdhereMsgEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(AnimationEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(BgMaskEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(CaseEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ChangeAniEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ChangeLAniEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ChoiceEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ComMoveEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ElseEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ElseIfEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(EndChoiceEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(EndIfEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(EndScriptEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(EventEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(FadeInEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(FadeOutEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(GameModeEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(IfEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(InfoWindowEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(InTitleEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(KeyWaitEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(LoadBgEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(LoadDataEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(LoadSpEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(LukeLetterEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ObjFadeInEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ObjFadeOutEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(OccurEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(OffBitFlgEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(PlayMovieEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(PlayRSoundEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(PlaySoundEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(PluralInEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(PluralOutEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(QuestionEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ReturnEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ReverseBgEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ReverseSpEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SendNazobaEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SetBgFlipEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SetBgEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SetBitFlgEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SetEvStateEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SetItemFlgEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SetObjFlipEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SetRiddleEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SetStoryEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ShakeAllEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ShakeBgEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ShakeEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(ShakeSpEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SolMysteryEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SpFadeInEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SpFadeOutEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(StopSoundEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(TextWindowEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(UpdateMemoEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(VibeEventData))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof(WaitEventData))]
        private CommaSeparatedSyntaxList<ExpressionSyntax> CreateCommaSeparatedList(EventData eventData)
        {
            var result = new List<ExpressionSyntax>();

            if (eventData is ConditionalBranchBlockEventData conditionalBranch)
            {
                result.Add(CreateValueExpression(conditionalBranch.Id));

                foreach (IfConditionData condition in conditionalBranch.Conditions)
                    result.Add(CreateIfConditionExpression(condition));
            }
            else if (eventData is BranchEventData branch)
            {
                result.Add(CreateValueExpression(branch.Id));
            }
            else if (eventData is SetBitFlgEventData setBitFlg)
            {
                ExpressionSyntax eventExpression = _parameterMapper.TryGetBitName(setBitFlg.Index, out string? bitName)
                    ? CreateValueExpression(bitName!)
                    : CreateValueExpression(setBitFlg.Index);

                result.Add(eventExpression);
            }
            else if (eventData is SetStoryEventData setStory)
            {
                ExpressionSyntax eventExpression = _parameterMapper.TryGetStoryName(setStory.Id, out string? storyName)
                    ? CreateValueExpression(storyName!)
                    : CreateValueExpression(setStory.Id);

                result.Add(eventExpression);
            }
            else if (eventData is TextWindowEventData textWindow)
            {
                ExpressionSyntax idExpression = _parameterMapper.TryGetSpeakerName(textWindow.SpeakerId, out string? speakerName)
                    ? CreateValueExpression(speakerName!)
                    : CreateValueExpression(textWindow.SpeakerId);

                result.Add(CreateValueExpression(textWindow.SpeakerSide));
                result.Add(idExpression);
                result.Add(CreateValueExpression(textWindow.Text));
            }
            else if (eventData is AddEventEventData addEvent)
            {
                ExpressionSyntax idExpression = addEvent.EventType == 1 && _parameterMapper.TryGetSpeakerName(addEvent.SpeakerId, out string? speakerName)
                    ? CreateValueExpression(speakerName!)
                    : CreateValueExpression(addEvent.SpeakerId);

                result.Add(CreateValueExpression(addEvent.EventType));
                result.Add(idExpression);
                result.Add(CreateValueExpression(addEvent.RankX));
                result.Add(CreateValueExpression(addEvent.RankY));
                result.Add(CreateValueExpression(addEvent.X));
                result.Add(CreateValueExpression(addEvent.Y));
                result.Add(CreateValueExpression(addEvent.Text));

                if (addEvent.Value5 != null)
                    result.Add(CreateValueExpression(addEvent.Value5));
                if (addEvent.Value6 != null)
                    result.Add(CreateValueExpression(addEvent.Value6));
            }
            else
            {
                foreach (PropertyInfo property in eventData.GetType().GetProperties())
                {
                    object? value = property.GetValue(eventData);
                    if (value == null)
                        continue;

                    result.Add(CreateValueExpression(value));
                }
            }

            return new CommaSeparatedSyntaxList<ExpressionSyntax>(result);
        }

        private ExpressionSyntax CreateValueExpression(object value)
        {
            Type valueType = value.GetType();
            if (Nullable.GetUnderlyingType(valueType) != null)
                valueType = Nullable.GetUnderlyingType(valueType)!;

            if (valueType.IsArray)
                return CreateArrayInitializer(value);

            if (valueType == typeof(string))
                return CreateStringLiteralExpression((string)value);

            if (valueType == typeof(byte))
                return CreateNumericLiteralExpression((byte)value);

            if (valueType == typeof(short))
                return CreateNumericLiteralExpression((short)value);

            if (valueType == typeof(bool))
            {
                if ((bool)value)
                    return CreateTrueLiteralExpression();

                return CreateFalseLiteralExpression();
            }

            throw new InvalidOperationException($"Invalid type {value.GetType().Name} for value.");
        }

        private ArrayInitializerExpressionSyntax CreateArrayInitializer(object value)
        {
            SyntaxToken bracketOpen = _syntaxFactory.Token(Level5DocomoTokenKind.BracketOpen);
            SyntaxToken bracketClose = _syntaxFactory.Token(Level5DocomoTokenKind.BracketClose);
            CommaSeparatedSyntaxList<ExpressionSyntax> values = CreateArrayInitializerParameterList(value);

            return new ArrayInitializerExpressionSyntax(bracketOpen, values, bracketClose);
        }

        private CommaSeparatedSyntaxList<ExpressionSyntax> CreateArrayInitializerParameterList(object value)
        {
            var listValue = (IList)value;
            if (listValue == null)
                throw new InvalidOperationException("Invalid value for array initializer parameters.");

            var result = new ExpressionSyntax[listValue.Count];

            for (var i = 0; i < result.Length; i++)
                result[i] = CreateValueExpression(listValue[i]!);

            return new CommaSeparatedSyntaxList<ExpressionSyntax>(result);
        }

        private ExpressionSyntax CreateStringLiteralExpression(string value)
        {
            SyntaxToken stringLiteral = _syntaxFactory.StringLiteral(value);

            return new LiteralExpressionSyntax(stringLiteral);
        }

        private ExpressionSyntax CreateNumericLiteralExpression(int value)
        {
            SyntaxToken numericLiteral = _syntaxFactory.NumericLiteral(value);

            return new LiteralExpressionSyntax(numericLiteral);
        }

        private ExpressionSyntax CreateTrueLiteralExpression()
        {
            SyntaxToken trueToken = _syntaxFactory.Token(Level5DocomoTokenKind.TrueKeyword);

            return new LiteralExpressionSyntax(trueToken);
        }

        private ExpressionSyntax CreateFalseLiteralExpression()
        {
            SyntaxToken falseToken = _syntaxFactory.Token(Level5DocomoTokenKind.FalseKeyword);

            return new LiteralExpressionSyntax(falseToken);
        }

        private NameSyntax CreateFunctionName(EventData eventData)
        {
            string name = eventData.GetType().Name;

            int postfixIndex = name.IndexOf("EventData", StringComparison.InvariantCulture);
            if (postfixIndex >= 0)
                name = name[..postfixIndex];

            SyntaxToken identifier = _syntaxFactory.Identifier(name);

            return new SimpleNameSyntax(identifier);
        }

        private NameSyntax CreateFunctionName(IfConditionData conditionData)
        {
            var name = $"Compare{conditionData.ComparisonType}";

            switch (conditionData.ComparisonType)
            {
                case 5:
                    name = conditionData.ComparisonValue == 0 ? "IsCurrentPuzzleEncountered" : "IsPuzzleEncountered";
                    break;

                case 6:
                    name = conditionData.ComparisonValue == 0 ? "IsCurrentPuzzleSolved" : "IsPuzzleSolved";
                    break;

                case 7:
                    name = "IsPlayerInEvent";
                    break;

                case 8:
                    name = "IsPlayerInPuzzle";
                    break;

                case 9:
                    name = "HasPlayerPuzzlesSolved";
                    break;

                case 12:
                    name = "IsBitSet";
                    break;

                case 13:
                    name = "IsPlayerEnteringRoom";
                    break;

                case 14:
                    name = "IsStorySet";
                    break;

                case 16:
                    name = "IsPlayerInRoom";
                    break;
            }

            SyntaxToken identifier = _syntaxFactory.Identifier(name);

            return new SimpleNameSyntax(identifier);
        }
    }
}
