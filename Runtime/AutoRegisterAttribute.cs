using System;
using VContainer;

namespace PolliPond.DI
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AutoRegisterAttribute : PreserveAttribute
    {
        public Lifetime Lifetime { get; }

        /// <summary>
        /// 由使用端自行定義意義，例如 "Root"、"Battle"。
        /// 不指定的話視為通用（RegisterAttributedTypes 不篩 tag 時都會被抓到）。
        /// </summary>
        public string Tag { get; }

        public AutoRegisterAttribute(Lifetime lifetime = Lifetime.Singleton, string tag = null)
        {
            Lifetime = lifetime;
            Tag = tag;
        }
    }
}
