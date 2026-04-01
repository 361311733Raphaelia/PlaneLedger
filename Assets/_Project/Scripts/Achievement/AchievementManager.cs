using System;
using System.Collections.Generic;
using System.Linq;
using PlaneLedger.Core;
using PlaneLedger.Currency;
using PlaneLedger.Data.Models;
using PlaneLedger.Data.SO;
using UnityEngine;

namespace PlaneLedger.Achievement
{
    /// <summary>
    /// 成就管理器。管理成就解锁状态、进度追踪、奖励发放。
    /// </summary>
    public class AchievementManager
    {
        private readonly AchievementData _achievementData;
        private readonly TokenManager _tokenManager;
        private readonly Dictionary<string, AchievementSO> _allAchievements;

        /// <summary>成就解锁时触发（供UI弹窗使用）</summary>
        public event Action<AchievementSO> OnUnlocked;

        public AchievementManager(SaveData saveData, TokenManager tokenManager, AchievementSO[] allAchievements)
        {
            _achievementData = saveData.Achievements;
            _tokenManager = tokenManager;
            _allAchievements = new Dictionary<string, AchievementSO>();
            foreach (var ach in allAchievements)
            {
                _allAchievements[ach.Id] = ach;
            }
        }

        /// <summary>
        /// 尝试解锁一个成就。已解锁则跳过。
        /// </summary>
        public bool TryUnlock(string achievementId)
        {
            if (_achievementData.UnlockedIds.Contains(achievementId)) return false;
            if (!_allAchievements.TryGetValue(achievementId, out var ach)) return false;

            _achievementData.UnlockedIds.Add(achievementId);

            // 发放代币奖励
            if (ach.TokenReward > 0)
            {
                _tokenManager.Earn(ach.TokenReward, $"achievement_{achievementId}");
            }

            // 发布事件
            EventBus.Publish(new AchievementUnlocked
            {
                AchievementId = achievementId,
                AchievementName = ach.DisplayName,
                TokenReward = ach.TokenReward,
                DecorationRewardId = ach.DecorationRewardId
            });

            OnUnlocked?.Invoke(ach);
            Debug.Log($"[Achievement] 解锁: {ach.DisplayName} (+{ach.TokenReward}代币)");
            return true;
        }

        /// <summary>
        /// 更新成就进度（累计类）。达到目标值时自动解锁。
        /// </summary>
        public void UpdateProgress(string achievementId, int newValue)
        {
            if (_achievementData.UnlockedIds.Contains(achievementId)) return;
            if (!_allAchievements.TryGetValue(achievementId, out var ach)) return;

            _achievementData.Progress[achievementId] = newValue;

            EventBus.Publish(new AchievementProgressUpdated
            {
                AchievementId = achievementId,
                CurrentValue = newValue,
                TargetValue = ach.TargetValue
            });

            if (newValue >= ach.TargetValue)
            {
                TryUnlock(achievementId);
            }
        }

        /// <summary>
        /// 递增成就进度（+1）。
        /// </summary>
        public void IncrementProgress(string achievementId)
        {
            int current = GetProgress(achievementId);
            UpdateProgress(achievementId, current + 1);
        }

        /// <summary>获取成就当前进度。</summary>
        public int GetProgress(string achievementId)
        {
            return _achievementData.Progress.TryGetValue(achievementId, out var val) ? val : 0;
        }

        /// <summary>检查成就是否已解锁。</summary>
        public bool IsUnlocked(string achievementId)
        {
            return _achievementData.UnlockedIds.Contains(achievementId);
        }

        /// <summary>获取成就配置。</summary>
        public AchievementSO GetAchievement(string id)
        {
            return _allAchievements.TryGetValue(id, out var ach) ? ach : null;
        }

        /// <summary>获取指定分类的所有成就。</summary>
        public List<AchievementSO> GetByCategory(AchievementCategory category)
        {
            return _allAchievements.Values.Where(a => a.Category == category).ToList();
        }

        /// <summary>已解锁成就总数。</summary>
        public int UnlockedCount => _achievementData.UnlockedIds.Count;

        /// <summary>成就总数。</summary>
        public int TotalCount => _allAchievements.Count;

        /// <summary>检查是否全部成就已解锁（不含"镜中镜"）。</summary>
        public bool AreAllUnlocked()
        {
            return _allAchievements.Values
                .Where(a => a.Id != "G-012" && a.Id != "G-018") // 排除自引用成就
                .All(a => _achievementData.UnlockedIds.Contains(a.Id));
        }

        #region 连续天数追踪

        /// <summary>
        /// 更新连续天数追踪。每日调用，自动判断是否连续。
        /// </summary>
        public void UpdateStreak(string trackerId, string todayDate, bool conditionMet)
        {
            if (conditionMet)
            {
                var lastDate = _achievementData.StreakLastDates.TryGetValue(trackerId, out var d) ? d : "";
                if (!string.IsNullOrEmpty(lastDate))
                {
                    var last = DateTime.Parse(lastDate);
                    var today = DateTime.Parse(todayDate);
                    if ((today - last).Days == 1)
                    {
                        // 连续
                        _achievementData.ConsecutiveStreaks[trackerId] =
                            (_achievementData.ConsecutiveStreaks.TryGetValue(trackerId, out var v) ? v : 0) + 1;
                    }
                    else if ((today - last).Days > 1)
                    {
                        // 断连
                        _achievementData.ConsecutiveStreaks[trackerId] = 1;
                    }
                    // Days == 0: 同一天重复调用，不处理
                }
                else
                {
                    _achievementData.ConsecutiveStreaks[trackerId] = 1;
                }
                _achievementData.StreakLastDates[trackerId] = todayDate;
            }
            else
            {
                // 条件不满足，重置
                _achievementData.ConsecutiveStreaks[trackerId] = 0;
            }
        }

        /// <summary>获取连续天数。</summary>
        public int GetStreak(string trackerId)
        {
            return _achievementData.ConsecutiveStreaks.TryGetValue(trackerId, out var v) ? v : 0;
        }

        #endregion
    }
}
