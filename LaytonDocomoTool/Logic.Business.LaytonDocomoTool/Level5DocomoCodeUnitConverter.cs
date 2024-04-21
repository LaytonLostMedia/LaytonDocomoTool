using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Logic.Business.LaytonDocomoTool.InternalContract;
using Logic.Domain.CodeAnalysis.Contract.Level5.Docomo.DataClasses;
using Logic.Domain.Level5Management.Docomo.Contract.Script.DataClasses;

namespace Logic.Business.LaytonDocomoTool
{
    internal class Level5DocomoCodeUnitConverter : ILevel5DocomoCodeUnitConverter
    {
        private readonly IDictionary<string, Type> _eventDataTypes;

        public Level5DocomoCodeUnitConverter()
        {
            _eventDataTypes = CacheEventDataTypes();
        }

        public EventData[] CreateEvents(CodeUnitSyntax codeUnit)
        {
            var result = new List<EventData>();

            AddStatements(codeUnit, result);

            return result.ToArray();
        }

        private void AddStatements(CodeUnitSyntax codeUnit, IList<EventData> events)
        {
            var blockId = 0;
            foreach (StatementSyntax statement in codeUnit.Statements)
                AddStatement(statement, events, ref blockId);
        }

        private void AddStatement(StatementSyntax statement, IList<EventData> events, ref int blockId)
        {
            switch (statement)
            {
                case FunctionInvocationStatementSyntax functionInvocation:
                    AddFunctionInvocationExpression(functionInvocation.FunctionInvocation, events);
                    break;

                case IfElseStatementSyntax ifElseStatement:
                    AddIfElseStatement(ifElseStatement, events, ref blockId);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid statement {statement.GetType().Name}.");
            }
        }

        private void AddIfElseStatement(IfElseStatementSyntax ifElseStatement, IList<EventData> events, ref int blockId)
        {
            int localBlockId = blockId;

            var ifEventData = new IfEventData
            {
                Id = (byte)localBlockId,
                Conditions = Array.Empty<IfConditionData>()
            };
            AddIfCondition(ifElseStatement.If.ConditionExpression, ifEventData);
            blockId += ifEventData.Conditions.Length;

            var ifEvents = new List<EventData>();
            foreach (StatementSyntax statement in ifElseStatement.Block.Statements)
                AddStatement(statement, ifEvents, ref blockId);

            ifEventData.Events = ifEvents.ToArray();

            events.Add(ifEventData);

            foreach (ElseStatementSyntax elseStatement in ifElseStatement.Else)
            {
                if (elseStatement is ElseIfStatementSyntax elseIfStatement)
                {
                    var elseIfEventData = new ElseIfEventData
                    {
                        Id = (byte)localBlockId,
                        Conditions = Array.Empty<IfConditionData>()
                    };
                    AddIfCondition(elseIfStatement.If.ConditionExpression, elseIfEventData);
                    blockId += elseIfEventData.Conditions.Length - 1;

                    var elseIfEvents = new List<EventData>();
                    foreach (StatementSyntax statement in elseIfStatement.Block.Statements)
                        AddStatement(statement, elseIfEvents, ref blockId);

                    elseIfEventData.Events = elseIfEvents.ToArray();

                    events.Add(elseIfEventData);

                    continue;
                }

                var elseEventData = new ElseEventData { Id = (byte)localBlockId };

                var elseEvents = new List<EventData>();
                foreach (StatementSyntax statement in elseStatement.Block.Statements)
                    AddStatement(statement, elseEvents, ref blockId);

                elseEventData.Events = elseEvents.ToArray();

                events.Add(elseEventData);
            }

            events.Add(new EndIfEventData { Id = (byte)localBlockId });
        }

        private void AddIfCondition(ExpressionSyntax ifExpression, ConditionalBranchBlockEventData conditionalBranchData)
        {
            if (ifExpression is LogicalExpressionSyntax compoundExpression)
            {
                AddIfCondition(compoundExpression.Left, conditionalBranchData);
                AddIfCondition(compoundExpression.Right, conditionalBranchData);

                return;
            }

            IfConditionData[] conditions = conditionalBranchData.Conditions;
            Array.Resize(ref conditions, conditions.Length + 1);

            conditions[^1] = CreateIfCondition(ifExpression);
            conditionalBranchData.Conditions = conditions;
        }

        private IfConditionData CreateIfCondition(ExpressionSyntax comparisonExpression)
        {
            var result = new IfConditionData
            {
                IsNegate = IsConditionNegated(comparisonExpression)
            };

            if (comparisonExpression is UnaryExpressionSyntax unaryExpression)
                comparisonExpression = unaryExpression.Expression;

            result.ComparisonType = GetComparisonType(comparisonExpression);
            result.ComparisonValue = GetComparisonValue(comparisonExpression);

            return result;
        }

        private bool IsConditionNegated(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case UnaryExpressionSyntax unaryExpression when (Level5DocomoTokenKind)unaryExpression.Operation.RawKind == Level5DocomoTokenKind.NotKeyword:
                case BinaryExpressionSyntax binaryExpression when (Level5DocomoTokenKind)binaryExpression.Operation.RawKind == Level5DocomoTokenKind.NotEquals:
                    return true;

                default:
                    return false;
            }
        }

        private byte GetComparisonType(ExpressionSyntax expression)
        {
            if (expression is FunctionInvocationExpressionSyntax functionInvocation)
                return GetComparisonType(GetName(functionInvocation.Name));

            if (expression is not BinaryExpressionSyntax binaryExpression)
                throw new InvalidOperationException($"Invalid expression {expression.GetType().Name} for if condition.");

            switch ((Level5DocomoTokenKind)binaryExpression.Operation.RawKind)
            {
                case Level5DocomoTokenKind.EqualsEquals:
                case Level5DocomoTokenKind.NotEquals:
                    return 0;

                case Level5DocomoTokenKind.SmallerThan:
                    return 1;

                case Level5DocomoTokenKind.GreaterThan:
                    return 2;

                case Level5DocomoTokenKind.SmallerEquals:
                    return 3;

                case Level5DocomoTokenKind.GreaterEquals:
                    return 4;

                default:
                    throw new InvalidOperationException($"Invalid binary expression {(Level5DocomoTokenKind)binaryExpression.Operation.RawKind} for if condition.");
            }
        }

        private short GetComparisonValue(ExpressionSyntax expression)
        {
            if (expression is FunctionInvocationExpressionSyntax functionInvocation)
            {
                if (functionInvocation.ParameterList.Parameters.Elements.Count <= 0)
                    return 0;

                return GetValue<short>(functionInvocation.ParameterList.Parameters.Elements[0]);
            }

            if (expression is not BinaryExpressionSyntax binaryExpression)
                throw new InvalidOperationException($"Invalid expression {expression.GetType().Name} for if condition.");

            return GetValue<short>(binaryExpression.Right);
        }

        private void AddFunctionInvocationExpression(FunctionInvocationExpressionSyntax functionInvocation, IList<EventData> events)
        {
            string functionName = GetName(functionInvocation.Name);

            EventData? eventData = CreateEventData(functionName);
            if (eventData == null)
                return;

            AddParameters(eventData, functionInvocation.ParameterList.Parameters.Elements);

            events.Add(eventData);
        }

        private void AddParameters(EventData eventData, IReadOnlyList<ExpressionSyntax> parameters)
        {
            if (eventData is ConditionalBranchBlockEventData conditionalBranch)
            {
                conditionalBranch.Id = GetValue<byte>(parameters[0]);

                conditionalBranch.Conditions = new IfConditionData[parameters.Count - 1];
                for (var i = 1; i < parameters.Count; i++)
                    conditionalBranch.Conditions[i - 1] = CreateIfCondition(parameters[i]);
            }
            else if (eventData is BranchEventData branch)
            {
                branch.Id = GetValue<byte>(parameters[0]);
            }
            else
            {
                var index = 0;
                foreach (PropertyInfo property in eventData.GetType().GetProperties())
                {
                    if (index >= parameters.Count)
                        break;

                    Type valueType = property.PropertyType;
                    if (valueType.IsArray)
                        valueType = valueType.GetElementType()!;
                    if (Nullable.GetUnderlyingType(valueType) != null)
                        valueType = Nullable.GetUnderlyingType(valueType)!;

                    ExpressionSyntax valueExpression = parameters[index++];
                    object value = GetValue(valueExpression, valueType);

                    property.SetValue(eventData, value);
                }
            }
        }

        private T GetValue<T>(ExpressionSyntax valueExpression)
        {
            return (T)GetValue(valueExpression, typeof(T));
        }

        private object GetValue(ExpressionSyntax valueExpression, Type valueType)
        {
            switch (valueExpression)
            {
                case LiteralExpressionSyntax literal:
                    return GetLiteralValue(literal, valueType);

                case ArrayInitializerExpressionSyntax arrayInitializer:
                    return GetArrayValue(arrayInitializer, valueType);

                default:
                    throw new InvalidOperationException($"Invalid value expression {valueExpression.GetType().Name}.");
            }
        }

        private object GetLiteralValue(LiteralExpressionSyntax literalExpression, Type valueType)
        {
            switch ((Level5DocomoTokenKind)literalExpression.Literal.RawKind)
            {
                case Level5DocomoTokenKind.StringLiteral:
                    if (valueType != typeof(string))
                        throw new InvalidOperationException($"Literal value expected to be of type {valueType.Name}, got string instead.");

                    return literalExpression.Literal.Text[1..^1];

                case Level5DocomoTokenKind.NumericLiteral:
                    if (valueType == typeof(byte))
                    {
                        if (byte.TryParse(literalExpression.Literal.Text, out byte intLiteral))
                            return intLiteral;

                        throw new InvalidOperationException($"Invalid numeric literal value {literalExpression.Literal.Text}.");
                    }

                    if (valueType == typeof(short))
                    {
                        if (short.TryParse(literalExpression.Literal.Text, out short intLiteral))
                            return intLiteral;

                        throw new InvalidOperationException($"Invalid numeric literal value {literalExpression.Literal.Text}.");
                    }

                    throw new InvalidOperationException($"Literal value expected to be of type {valueType.Name}, got integer instead.");

                case Level5DocomoTokenKind.TrueKeyword:
                    if (valueType != typeof(bool))
                        throw new InvalidOperationException($"Literal value expected to be of type {valueType.Name}, got boolean instead.");

                    return true;

                case Level5DocomoTokenKind.FalseKeyword:
                    if (valueType != typeof(bool))
                        throw new InvalidOperationException($"Literal value expected to be of type {valueType.Name}, got boolean instead.");

                    return false;

                default:
                    throw new InvalidOperationException($"Invalid literal expression {(Level5DocomoTokenKind)literalExpression.Literal.RawKind}.");
            }
        }

        private object GetArrayValue(ArrayInitializerExpressionSyntax arrayInitializer, Type valueType)
        {
            IReadOnlyList<ExpressionSyntax>? valueExpressions = arrayInitializer.Values?.Elements;

            if (valueExpressions is not { Count: > 0 })
                return Array.CreateInstance(valueType, 0);

            var result = Array.CreateInstance(valueType, valueExpressions.Count);

            var index = 0;
            foreach (ExpressionSyntax valueExpression in valueExpressions)
            {
                object value = GetValue(valueExpression, valueType);
                result.SetValue(value, index++);
            }

            return result;
        }

        private string GetName(NameSyntax nameSyntax)
        {
            if (nameSyntax is SimpleNameSyntax simpleName)
                return simpleName.Identifier.Text;

            throw new InvalidOperationException($"Invalid name syntax {nameSyntax.GetType().Name}.");
        }

        private byte GetComparisonType(string name)
        {
            if (name.Equals("IsCurrentPuzzleEncountered", StringComparison.InvariantCultureIgnoreCase) ||
                name.Equals("IsPuzzleEncountered", StringComparison.InvariantCultureIgnoreCase))
                return 5;

            if (name.Equals("IsCurrentPuzzleSolved", StringComparison.InvariantCultureIgnoreCase) ||
                name.Equals("IsPuzzleSolved", StringComparison.InvariantCultureIgnoreCase))
                return 6;

            if (name.Equals("IsPlayerInEvent", StringComparison.InvariantCultureIgnoreCase))
                return 7;

            if (name.Equals("IsPlayerInPuzzle", StringComparison.InvariantCultureIgnoreCase))
                return 8;

            if (name.Equals("HasPlayerPuzzlesSolved", StringComparison.InvariantCultureIgnoreCase))
                return 9;

            if (name.Equals("IsBitSet", StringComparison.InvariantCultureIgnoreCase))
                return 12;

            if (name.Equals("IsPlayerEnteringRoom", StringComparison.InvariantCultureIgnoreCase))
                return 13;

            if (name.Equals("IsStorySet", StringComparison.InvariantCultureIgnoreCase))
                return 14;

            if (name.Equals("IsPlayerInRoom", StringComparison.InvariantCultureIgnoreCase))
                return 16;

            // 0,1,2,3,4,10,11,15

            if (!name.StartsWith("Compare", StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidOperationException($"Invalid comparison {name}.");

            if (!byte.TryParse(name[7..], out byte comparisonType))
                throw new InvalidOperationException($"Invalid comparison type {name[7..]}.");

            return comparisonType;
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
        private EventData? CreateEventData(string name)
        {
            if (!_eventDataTypes.TryGetValue(name.ToLower(), out Type? eventDataType))
                return null;

            return (EventData?)Activator.CreateInstance(eventDataType, true);
        }

        private IDictionary<string, Type> CacheEventDataTypes()
        {
            var result = new Dictionary<string, Type>();

            var assembly = Assembly.GetAssembly(typeof(EventData));
            if (assembly == null)
                return result;

            IEnumerable<Type> eventDataTypes = assembly.GetExportedTypes().Where(x => x.Name.EndsWith("EventData"));
            foreach (Type eventDataType in eventDataTypes)
            {
                int postfixIndex = eventDataType.Name.IndexOf("EventData", StringComparison.InvariantCulture);
                if (postfixIndex < 0)
                    continue;

                string eventDataName = eventDataType.Name[..postfixIndex];
                result[eventDataName.ToLower()] = eventDataType;
            }

            return result;
        }
    }
}
