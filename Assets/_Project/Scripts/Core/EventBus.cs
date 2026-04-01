using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaneLedger.Core
{
    /// <summary>
    /// 全局事件总线，用于模块间解耦通信。
    /// 使用泛型事件类型作为频道，避免字符串匹配。
    ///
    /// 用法:
    ///   EventBus.Subscribe<DailySettlementCompleted>(OnSettlement);
    ///   EventBus.Publish(new DailySettlementCompleted { Date = today });
    ///   EventBus.Unsubscribe<DailySettlementCompleted>(OnSettlement);
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        /// <summary>
        /// 订阅一个事件类型。
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                _handlers[type] = Delegate.Combine(existing, handler);
            }
            else
            {
                _handlers[type] = handler;
            }
        }

        /// <summary>
        /// 取消订阅一个事件类型。
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                var result = Delegate.Remove(existing, handler);
                if (result == null)
                    _handlers.Remove(type);
                else
                    _handlers[type] = result;
            }
        }

        /// <summary>
        /// 发布一个事件，通知所有订阅者。
        /// </summary>
        public static void Publish<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                try
                {
                    ((Action<T>)existing)?.Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventBus] 处理事件 {type.Name} 时发生异常: {e}");
                }
            }
        }

        /// <summary>
        /// 清空所有订阅（仅用于测试或场景切换）。
        /// </summary>
        public static void Clear()
        {
            _handlers.Clear();
        }
    }
}
