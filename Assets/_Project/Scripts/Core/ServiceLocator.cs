using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaneLedger.Core
{
    /// <summary>
    /// 轻量级服务定位器，用于模块间解耦访问。
    /// 所有 Service 在 GameManager 启动时注册，运行时通过 Get<T>() 获取。
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// 注册一个服务实例。重复注册同类型会覆盖。
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] 覆盖已注册的服务: {type.Name}");
            }
            _services[type] = service;
        }

        /// <summary>
        /// 获取已注册的服务。未找到时抛出异常。
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException($"[ServiceLocator] 未注册的服务: {type.Name}");
        }

        /// <summary>
        /// 尝试获取服务，未找到返回 null。
        /// </summary>
        public static T TryGet<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            return null;
        }

        /// <summary>
        /// 检查是否已注册某个服务。
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 注销一个服务。
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        /// <summary>
        /// 清空所有注册（仅用于测试或应用退出）。
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }
}
