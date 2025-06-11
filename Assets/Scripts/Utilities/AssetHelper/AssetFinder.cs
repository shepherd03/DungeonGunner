#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

public static class AssetFinder
{
    public static List<T> FindAssets<T>() where T : Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        
        return assets;
    }
    
    public static List<T> FindAssets<T>(Func<T,bool> filter) where T : Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null && filter(asset))
            {
                assets.Add(asset);
            }
        }
        
        return assets;
    }
}

#endif
