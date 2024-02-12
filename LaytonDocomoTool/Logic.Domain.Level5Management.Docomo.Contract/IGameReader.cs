using Logic.Domain.Level5Management.Docomo.Contract.DataClasses;

namespace Logic.Domain.Level5Management.Docomo.Contract
{
    public interface IGameReader
    {
        GameData Read(Stream input);
    }
}
