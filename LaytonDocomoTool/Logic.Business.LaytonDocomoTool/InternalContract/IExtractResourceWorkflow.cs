namespace Logic.Business.LaytonDocomoTool.InternalContract
{
    public interface IExtractResourceWorkflow
    {
        void Work();
        void Work(Stream resourceStream, string extractDir);
    }
}
