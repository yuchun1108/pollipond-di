using PolliPond.DI;
using UnityEngine;
using VContainer;

namespace PolliPond.DI.Example
{
    /// <summary>
    /// 示範如何建立容器、註冊服務並設定遊戲容器範圍。
    /// </summary>
    public static class InstallerExample
    {
        private static SomeService _someService;
        private static IObjectResolver _resolver;

        /// <summary>
        /// 尋找場景服務、建立容器並設定遊戲容器範圍。
        /// </summary>
        public static void Install()
        {
            SetupSomeService();
            BuildScope();
        }

        /// <summary>
        /// 從目前場景取得要註冊至容器的服務。
        /// </summary>
        private static void SetupSomeService()
        {
            _someService = Object.FindAnyObjectByType<SomeService>();
        }

        /// <summary>
        /// 建立容器、註冊服務與自動註冊類別，並設定遊戲容器範圍。
        /// </summary>
        private static void BuildScope()
        {
            var baseBuilder = new ContainerBuilder();
            baseBuilder.RegisterInstance(_someService);
            baseBuilder.RegisterAttributedTypes(new[] { "PolliPond.Game", "PolliPond.View" });
            _resolver = baseBuilder.Build();

            GameScope.SetScope(_resolver);
        }

        /// <summary>
        /// 釋放目前建立的容器。
        /// </summary>
        public static void Dispose()
        {
            _resolver?.Dispose();
        }
    }
}
