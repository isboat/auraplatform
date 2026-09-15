namespace Aura.Dashboard.Repositories;
public interface IAssetStorage
{
    Task DeleteAsync(string objectKey);
    string ReadUrl(string objectKey);
}
