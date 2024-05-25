namespace Logic.Business.LaytonDocomoTool.InternalContract
{
    internal interface IScriptParameterMapper
    {
        bool TryGetBitName(int value, out string? name);
        bool TryGetBitValue(string name, out int value);

        bool TryGetStoryName(int value, out string? name);
        bool TryGetStoryValue(string name, out int value);
    }
}
