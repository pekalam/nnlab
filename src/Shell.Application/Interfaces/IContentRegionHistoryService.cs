namespace Shell.Application.Interfaces
{
    public interface IContentRegionHistoryService
    {
        void SaveContentForModule(int moduleNavId);
        void TryRestoreContentForModule(int moduleNavId);
        void ClearHistoryForModulesExcept(int moduleNavId);
    }
}