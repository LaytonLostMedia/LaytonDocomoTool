namespace Logic.Business.LaytonDocomoTool.InternalContract
{
    public interface ICreateMelodyWorkflow
    {
        void Work();
        void Work(string midiFilePath, Stream output);
    }
}
