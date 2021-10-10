using CoreFrame.Exts;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CachedResource
{
    private static Dictionary<string, Object> _cacher = new Dictionary<string, Object>();

    public static int Count { get { return CachedResource._cacher.Count; } }

    public static bool HasInCache(string pathName)
    {
        return CachedResource._cacher.ContainsKey(pathName);
    }

    public static Object GetFromCache(string pathName)
    {
        if (CachedResource.HasInCache(pathName))
        {
            if (CachedResource._cacher.TryGetValue(pathName, out Object resObj)) return resObj;
        }

        return null;
    }

    /// <summary>
    /// 預加載資源至快取中
    /// </summary>
    /// <param name="pathName"></param>
    public static async UniTask PreloadInCache(string pathName)
    {
        if (pathName.IsNullOrZeroEmpty()) return;

        var asset = await Resources.LoadAsync<Object>(pathName);
        Object resObj = asset;
        if (resObj != null)
        {
            // skipping duplicate keys
            if (!CachedResource.HasInCache(pathName)) CachedResource._cacher.Add(pathName, resObj);
        }

        Debug.Log("【預加載資源】 => 當前快取數量 : " + CachedResource.Count);
    }

    /// <summary>
    /// 載入資源 => 會優先從快取中取得資源, 如果快取中沒有才進行資源加載
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pathName"></param>
    /// <returns></returns>
    public static async UniTask<T> Load<T>(string pathName, bool cached = true) where T : Object
    {
        T resObj = null;

        if (cached) resObj = CachedResource.GetFromCache(pathName) as T;

        if (resObj == null)
        {
            var asset = await Resources.LoadAsync<T>(pathName);
            resObj = asset as T;

            if (resObj != null)
            {
                if (cached)
                {
                    // skipping duplicate keys
                    if (!CachedResource.HasInCache(pathName)) CachedResource._cacher.Add(pathName, resObj);
                }
            }

        }

        if (cached) Debug.Log("【載入資源】 => 當前快取數量 : " + CachedResource.Count);

        return resObj;
    }

    /// <summary>
    /// 從快取【移除】單個資源
    /// </summary>
    /// <param name="pathName"></param>
    public static void ClearFromCache(string pathName)
    {
        if (CachedResource.HasInCache(pathName))
        {
            CachedResource._cacher[pathName] = null;
            CachedResource._cacher.Remove(pathName);
        }

        Debug.Log("【單個移除】 => 當前快取數量 : " + CachedResource.Count);
    }

    /// <summary>
    /// 從快取【釋放】單個資源
    /// </summary>
    /// <param name="pathName"></param>
    public static void ReleaseFromCache(string pathName)
    {
        if (CachedResource.HasInCache(pathName))
        {
            CachedResource._cacher[pathName] = null;
            CachedResource._cacher.Remove(pathName);
        }

        // 刪除快取資源後, 並且釋放無使用的資源
        Resources.UnloadUnusedAssets();

        Debug.Log("【單個釋放】 => 當前快取數量 : " + CachedResource.Count);
    }

    /// <summary>
    /// 從快取中【移除】全部資源
    /// </summary>
    public static void ClearCache()
    {
        if (CachedResource.Count == 0) return;

        foreach (var key in CachedResource._cacher.Keys.ToList())
        {
            CachedResource._cacher[key] = null;
        }
        CachedResource._cacher.Clear();

        Debug.Log("【全部移除】 => 當前快取數量 : " + CachedResource.Count);
    }

    /// <summary>
    /// 從快取中【釋放】全部資源
    /// </summary>
    public static void ReleaseCache()
    {
        if (CachedResource.Count == 0) return;

        foreach (var key in CachedResource._cacher.Keys.ToList())
        {
            CachedResource._cacher[key] = null;
        }
        CachedResource._cacher.Clear();

        // 刪除快取資源後, 並且釋放無使用的資源
        Resources.UnloadUnusedAssets();

        Debug.Log("【全部釋放】 => 當前快取數量 : " + CachedResource.Count);
    }
}