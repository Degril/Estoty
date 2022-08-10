using Data.Storages;

namespace Data
{
    public class PlayerData : IService
    {
        public ResourcesStorage ResourcesStorage { get; } = new();
    }
}