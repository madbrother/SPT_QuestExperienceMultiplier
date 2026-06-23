using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using System.Reflection;

namespace MadBro.QuestExpMultiplier;

[Injectable]
public class ModUtils(
    ModHelper modHelper)
{
    private string? _pathToMod = null;
    public string PathToMod
    {
        get
        {
            _pathToMod ??= modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
            return _pathToMod;
        }
    }

    public string PathToConfig { get; set; } = "config.json";
    private ModConfig? _modConfig = null;
    public ModConfig ModConfig
    {
        get
        {
            _modConfig ??= modHelper.GetJsonDataFromFile<ModConfig>(PathToMod, PathToConfig);
            return _modConfig;
        }
    }

    public T GetJsonDataFromFile<T>(string relativePathToData)
    {
        return modHelper.GetJsonDataFromFile<T>(PathToMod, relativePathToData);
    }
}