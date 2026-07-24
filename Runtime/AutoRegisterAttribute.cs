using System;
using VContainer;

namespace PolliPond.DI
{
    /// <summary>
    /// 標記需要由容器自動註冊的類別。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AutoRegisterAttribute : PreserveAttribute
    {
        /// <summary>
        /// 取得類別註冊至容器時使用的生命週期。
        /// </summary>
        public Lifetime Lifetime { get; }

        /// <summary>
        /// 取得用來區分註冊群組的標籤。
        /// </summary>
        public string Tag { get; }

        /// <summary>
        /// 初始化 <see cref="AutoRegisterAttribute"/> 的新執行個體。
        /// </summary>
        /// <param name="lifetime">類別註冊至容器時使用的生命週期。</param>
        /// <param name="tag">用來區分註冊群組的標籤。</param>
        public AutoRegisterAttribute(Lifetime lifetime = Lifetime.Singleton, string tag = null)
        {
            Lifetime = lifetime;
            Tag = tag;
        }
    }
}
