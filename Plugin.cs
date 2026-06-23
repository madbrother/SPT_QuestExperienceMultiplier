using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers.ItemEvents;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Globalization;
using System.Reflection;

namespace MadBro.QuestExpMultiplier
{
    public record ModMetadata : AbstractModMetadata
    {
        public override string ModGuid { get; init; } = "madbro.QuestExpMultiplier";
        public override string Name { get; init; } = "QuestExpMultiplier";
        public override string Author { get; init; } = "MadBrother";
        public override List<string>? Contributors { get; init; }
        public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
        public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
        public override List<string>? Incompatibilities { get; init; }
        public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
        public override string? Url { get; init; }
        public override bool? IsBundleMod { get; init; }
        public override string License { get; init; } = "MIT";
    }

    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 10)]
    public class Plugin(DatabaseServer databaseServer, ModUtils modUtils, ISptLogger<HealthHelper> logger) : IOnLoad
    {
        public Task OnLoad()
        {
            float multiplier = 1;
            try
            {

                //multiplier = float.Parse(modUtils.ModConfig.Multiplier, CultureInfo.InvariantCulture);
                multiplier = modUtils.ModConfig.Multiplier;

                var quests = databaseServer.GetTables().Templates.Quests;
                foreach ((_, var quest) in quests)
                {
                    if (quest.Rewards != null)
                    {
                        var expReward = quest.Rewards.Values.SelectMany(rewardList => rewardList.Where(r => r.Type == RewardType.Experience));
                        foreach (var reward in expReward)
                        {
                            reward.Value *= multiplier;
                        }
                    }
                }
            } catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Success($"QuestExpMultiplier harmony patch has successfully loaded! Multiplier applied: {multiplier}");
            return Task.CompletedTask;
        }
    }
}
