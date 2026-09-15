using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class ConfiguredAssetStorage(IConfiguration config):IAssetStorage { public Task DeleteAsync(string key)=>Task.CompletedTask; public string ReadUrl(string key)=>$"{config["MediaStorage:PublicBaseUrl"]?.TrimEnd('/')}/{Uri.EscapeDataString(key)}"; }
