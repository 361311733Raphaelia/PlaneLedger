using PlaneLedger.Core;
using PlaneLedger.Data.SO;
using UnityEngine;

namespace PlaneLedger.Steam
{
    /// <summary>
    /// Steam 成就桥接器。监听游戏内成就解锁事件，同步到 Steam 成就系统。
    /// 需要导入 Steamworks.NET 包后取消注释 Steam API 调用。
    /// </summary>
    public class SteamAchievementBridge
    {
        private readonly AchievementSO[] _allAchievements;

        public SteamAchievementBridge(AchievementSO[] allAchievements)
        {
            _allAchievements = allAchievements;
            EventBus.Subscribe<AchievementUnlocked>(OnAchievementUnlocked);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<AchievementUnlocked>(OnAchievementUnlocked);
        }

        private void OnAchievementUnlocked(AchievementUnlocked e)
        {
            // 查找对应的 Steam API Name
            foreach (var ach in _allAchievements)
            {
                if (ach.Id == e.AchievementId && !string.IsNullOrEmpty(ach.SteamApiName))
                {
                    SetSteamAchievement(ach.SteamApiName);
                    break;
                }
            }
        }

        private void SetSteamAchievement(string apiName)
        {
            // TODO: 导入 Steamworks.NET 后取消注释
            // SteamUserStats.SetAchievement(apiName);
            // SteamUserStats.StoreStats();

            Debug.Log($"[Steam] 成就同步: {apiName}（Steamworks.NET 待导入）");
        }

        /// <summary>
        /// 批量同步所有已解锁的游戏成就到 Steam（用于首次安装 Steamworks 后的补发）。
        /// </summary>
        public void SyncAllUnlockedToSteam(System.Collections.Generic.HashSet<string> unlockedIds)
        {
            foreach (var ach in _allAchievements)
            {
                if (unlockedIds.Contains(ach.Id) && !string.IsNullOrEmpty(ach.SteamApiName))
                {
                    SetSteamAchievement(ach.SteamApiName);
                }
            }
        }
    }
}
