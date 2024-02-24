using Logic.Domain.Level5Management.Docomo.Contract.Table.DataClasses;

namespace Logic.Business.LaytonDocomoTool.InternalContract
{
    internal interface IExtractTableWorkflow
    {
        void Work();
        void Work(TableData table, string extractDir);
    }
}
