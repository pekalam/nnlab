namespace Data.Application.Interfaces
{
    public interface IFileDialogService
    {
        (bool? result, string filePath) OpenCsv();
    }
}
