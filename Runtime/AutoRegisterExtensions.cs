using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PolliPond.DI
{
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

        // key: 組件前綴組合，value: 該組合下掃描到的所有標記類別
        static readonly Dictionary<string, List<(Type type, AutoRegisterAttribute attr)>> _cache =
            new();

        /// <summary>
        /// 掃描指定組件前綴下所有標有 [AutoRegister] 的類別並註冊進容器。
        /// </summary>
        /// <param name="assemblyPrefixes">要掃描的組件名稱前綴，例如 "PolliPond.Game"</param>
        /// <param name="tag">
        /// 只註冊 Tag 等於此值的類別；傳 null 時只註冊 Tag 也是 null 的類別（即沒指定 tag 的類別）。
        /// </param>
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
                    // 部分型別載入失敗時，跳過失敗的，仍處理其餘成功載入的型別
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
