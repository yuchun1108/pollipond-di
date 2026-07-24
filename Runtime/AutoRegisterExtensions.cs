using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PolliPond.DI
{
    /// <summary>
    /// 提供依據 <see cref="AutoRegisterAttribute"/> 自動註冊類別的擴充方法。
    /// </summary>
    public static class AutoRegisterExtensions
    {
        static readonly Type[] EntryPointInterfaces =
        {
            typeof(IInitializable),
            typeof(IPostInitializable),
            typeof(IStartable),
            typeof(IPostStartable),
            typeof(IFixedTickable),
            typeof(IPostFixedTickable),
            typeof(ITickable),
            typeof(IPostTickable),
            typeof(ILateTickable),
            typeof(IPostLateTickable),
#if VCONTAINER_UNITASK_INTEGRATION
            typeof(IAsyncStartable),
#endif
        };

        // key: 組件名稱前綴；value: 掃描取得的型別及其自動註冊設定。
        static readonly Dictionary<string, List<(Type type, AutoRegisterAttribute attr)>> _cache =
            new();

        /// <summary>
        /// 掃描指定組件並將具有 <see cref="AutoRegisterAttribute"/> 的類別註冊至容器。
        /// </summary>
        /// <param name="builder">要加入類別註冊的容器建構器。</param>
        /// <param name="assemblyPrefixes">要掃描的組件名稱前綴。</param>
        /// <param name="tag">只註冊具有相同標籤的類別；預設只註冊未設定標籤的類別。</param>
        /// <exception cref="ArgumentException">未提供任何組件名稱前綴時擲回。</exception>
        public static void RegisterAttributedTypes(
            this IContainerBuilder builder,
            string[] assemblyPrefixes,
            string tag = null
        )
        {
            if (assemblyPrefixes == null || assemblyPrefixes.Length == 0)
                throw new ArgumentException("至少要指定一個組件前綴", nameof(assemblyPrefixes));

            var cacheKey = string.Join(
                "|",
                assemblyPrefixes.OrderBy(x => x, StringComparer.Ordinal)
            );

            if (!_cache.TryGetValue(cacheKey, out var all))
            {
                all = Scan(assemblyPrefixes);
                _cache[cacheKey] = all;
            }

            var filtered = all.Where(x => x.attr.Tag == tag);

            foreach (var (type, attr) in filtered)
            {
                var isEntryPoint = EntryPointInterfaces.Any(i => i.IsAssignableFrom(type));

                if (isEntryPoint)
                {
                    EntryPointsBuilder.EnsureDispatcherRegistered(builder);
                    builder.Register(type, attr.Lifetime).AsImplementedInterfaces().AsSelf();
                }
                else
                {
                    builder.Register(type, attr.Lifetime).AsImplementedInterfaces().AsSelf();
                }
            }
        }

        /// <summary>
        /// 掃描名稱符合指定前綴的組件，並取得可自動註冊的類別。
        /// </summary>
        /// <param name="prefixes">要掃描的組件名稱前綴。</param>
        /// <returns>可自動註冊的類別及其註冊設定。</returns>
        static List<(Type, AutoRegisterAttribute)> Scan(string[] prefixes)
        {
            var result = new List<(Type, AutoRegisterAttribute)>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assembly.GetName().Name;
                if (!prefixes.Any(p => name.StartsWith(p, StringComparison.Ordinal)))
                    continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // 若部分型別無法載入，仍保留成功載入的型別繼續掃描。
                    types = ex.Types.Where(t => t != null).ToArray();
                }

                foreach (var type in types)
                {
                    if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                        continue;

                    var attr = type.GetCustomAttribute<AutoRegisterAttribute>();
                    if (attr != null)
                        result.Add((type, attr));
                }
            }

            return result;
        }
    }
}
