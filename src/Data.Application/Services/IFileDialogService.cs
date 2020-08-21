namespace Data.Application
{
    public interface IFileDialogService
    {
        (bool? result, string filePath) OpenCsv();
    }
}
