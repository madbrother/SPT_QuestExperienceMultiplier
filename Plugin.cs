using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace MadBro.QuestExpMultiplier
{
    public record ModMetadata : AbstractModMetadata
    {
        public override string ModGuid { get; init; } = "madbro.QuestExpMultiplier";
        public override string Name { get; init; } = "QuestExpMultiplier";
        public override string Author { get; init; } = "MadBrother";
        public override List<string>? Contributors { get; init; }
        public override SemanticVersioning.Version Version { get; init; } = new("1.0.1");
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
            string? message = null;
            try
            {

                (multiplier, message) = NormalizeMultiplier(modUtils.ModConfig);

                var quests = databaseServer.GetTables().Templates.Quests;
                foreach ((_, var quest) in quests)
                {
                    if (quest.Rewards != null)
                    {
                        var expReward = quest.Rewards.Values.SelectMany(rewardList => rewardList.Where(r => r.Type == RewardType.Experience));
                        foreach (var reward in expReward)
                        {
                            if (reward.Value == null) continue;

                            double newValue = reward.Value.Value * multiplier;
                            reward.Value = (int)Math.Round((decimal)newValue);
                        }
                    }
                }
            } catch (Exception ex)
            {
                logger.Error(ex.Message);
                return Task.CompletedTask;
            }

            if (message != null)
            {
                logger.Warning("QuestExpMultiplier: " + message);
                return Task.CompletedTask;
            }

            logger.Success($"QuestExpMultiplier has successfully loaded! Multiplier applied: {multiplier}");
            return Task.CompletedTask;
        }

        private static (float, string?) NormalizeMultiplier(ModConfig config)
        {
            float multiplier = config.Multiplier;
            if (multiplier <= 0)
                return (0.1f, "Multiplier cannot be less than or equal to 0. Setting to 0.1.");
            if (multiplier > 10 && !config.IgnoreUpperLimit)
                return (10f, "Multiplier cannot be greater than 10. Setting to 10.");
            return (multiplier, null);
        }
    }
}
