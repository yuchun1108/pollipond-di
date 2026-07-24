using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PolliPond.DI
{
    /// <summary>
    /// 提供目前遊戲相依性注入範圍的全域存取介面。
    /// </summary>
    public static class GameScope
    {
        private static IObjectResolver _scope;

        /// <summary>
        /// 設定用於建立物件、注入相依性及解析服務的容器範圍。
        /// </summary>
        /// <param name="scope">要使用的物件解析器。</param>
        public static void SetScope(IObjectResolver scope)
        {
            _scope = scope;
        }

        /// <summary>
        /// 使用目前容器範圍建立指定的遊戲物件預製體。
        /// </summary>
        /// <param name="prefab">要建立的遊戲物件預製體。</param>
        /// <returns>建立完成的遊戲物件；若尚未設定容器範圍則傳回 <see langword="null"/>。</returns>
        public static GameObject Instantiate(GameObject prefab)
        {
            if (_scope == null)
            {
                Debug.LogError("GameScope not setup");
                return null;
            }
            return _scope.Instantiate(prefab);
        }

        /// <summary>
        /// 使用目前容器範圍建立指定的元件預製體。
        /// </summary>
        /// <typeparam name="T">預製體的元件型別。</typeparam>
        /// <param name="prefab">要建立的元件預製體。</param>
        /// <returns>建立完成的元件；若尚未設定容器範圍則傳回 <see langword="null"/>。</returns>
        public static T Instantiate<T>(T prefab)
            where T : Component
        {
            if (_scope == null)
            {
                Debug.LogError("GameScope not setup");
                return null;
            }
            return _scope.Instantiate(prefab);
        }

        /// <summary>
        /// 使用目前容器範圍，在指定位置與旋轉角度建立遊戲物件預製體。
        /// </summary>
        /// <param name="prefab">要建立的遊戲物件預製體。</param>
        /// <param name="position">建立物件的世界座標。</param>
        /// <param name="rotation">建立物件的旋轉角度。</param>
        /// <returns>建立完成的遊戲物件；若尚未設定容器範圍則傳回 <see langword="null"/>。</returns>
        public static GameObject Instantiate(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation
        )
        {
            if (_scope == null)
            {
                Debug.LogError("GameScope not setup");
                return null;
            }
            return _scope.Instantiate(prefab, position, rotation);
        }

        /// <summary>
        /// 使用目前容器範圍將相依性注入指定物件。
        /// </summary>
        /// <param name="obj">要注入相依性的物件。</param>
        public static void Inject(object obj)
        {
            if (_scope == null)
            {
                Debug.LogError("GameScope not setup");
                return;
            }
            _scope.Inject(obj);
        }

        /// <summary>
        /// 從目前容器範圍解析指定型別的服務。
        /// </summary>
        /// <typeparam name="T">要解析的服務型別。</typeparam>
        /// <returns>解析完成的服務；若尚未設定容器範圍則傳回型別的預設值。</returns>
        public static T Resolve<T>()
        {
            if (_scope == null)
            {
                Debug.LogError("GameScope not setup");
                return default;
            }
            return _scope.Resolve<T>();
        }
    }
}
