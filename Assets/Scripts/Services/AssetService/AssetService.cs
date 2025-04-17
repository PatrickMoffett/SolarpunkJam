using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Services;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;


/// <summary>
/// Service to wrap the loading, unloading, and releasing of Addressable assets
/// </summary>
public class AssetService : IService
{
    /// <summary>
    /// Load an asset
    /// </summary>
    /// <param name="address">The address to load the asset from</param>
    /// <typeparam name="T">The type of asset to load</typeparam>
    /// <returns>The requested asset</returns>
    /// <exception cref="InvalidKeyException">Thrown when loading the asset fails</exception>
    public T LoadAsset<T>(string address)
    {
        AsyncOperationHandle<T> asset = Addressables.LoadAssetAsync<T>(address);
        
        asset.WaitForCompletion();

        if (asset.Status == AsyncOperationStatus.Succeeded)
        {
            return asset.Result;
        }
        
        throw new InvalidKeyException($"[{GetType().Name}] Failed to load asset with address {address}");
    }

    public IList<T> LoadAllAssets<T>(string label)
    { 
        AsyncOperationHandle<IList<T>> process = Addressables.LoadAssetsAsync<T>(label, o => { });
        return process.WaitForCompletion();
    }
    
    /// <summary>
    /// Load an asset and return its handle
    /// </summary>
    /// <param name="address">The address of the asset to load</param>
    /// <typeparam name="T">The type of asset to load</typeparam>
    /// <returns>The requested asset's handle</returns>
    /// <exception cref="InvalidKeyException">Thrown when loading the asset fails</exception>
    public AsyncOperationHandle<T> LoadAssetHandle<T>(string address)
    {
        AsyncOperationHandle<T> asset = Addressables.LoadAssetAsync<T>(address);

        asset.WaitForCompletion();

        if (asset.Status == AsyncOperationStatus.Succeeded)
        {
            return asset;
        }

        throw new InvalidKeyException($"[{GetType().Name}] Failed to load asset with address {address}");
    }

    /// <summary>
    /// Load the asset asynchronously
    /// </summary>
    /// <param name="address">The address of the asset to load</param>
    /// <typeparam name="T">The type of asset to load</typeparam>
    /// <returns>The async task loading the asset</returns>
    public async UniTask<T> LoadAssetAsync<T>(string address, CancellationToken token = default)
    {
        AsyncOperationHandle<T> asset = Addressables.LoadAssetAsync<T>(address);
        return await asset.WithCancellation(token, false, true);
    }

    /// <summary>
    /// Unload a GameObject instantiated via Addressables
    /// </summary>
    /// <param name="go">The GameObject to unload</param>
    /// <returns>True if the instance was unloaded. False otherwise.</returns>
    public bool ReleaseInstance(GameObject go)
    {
        Debug.Log($"[{GetType().Name}] Releasing object {go}");
        return Addressables.ReleaseInstance(go);
    }

    public async UniTask<bool> ReleaseInstance(GameObject go, float time)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(time));
        return ReleaseInstance(go);
    }
    
    /// <summary>
    /// Unload an asset of a given type
    /// </summary>
    /// <param name="obj">The asset to unload</param>
    /// <typeparam name="T">The type of the asset to unload</typeparam>
    public void Release<T>(T obj)
    {
        Addressables.Release<T>(obj);
    }

    /// <summary>
    /// Unload an asset with a typeless handle
    /// </summary>
    /// <param name="handle">The handle for the asset to unload</param>
    public void Release(AsyncOperationHandle handle)
    {
        Addressables.Release(handle);
    }
    
    /// <summary>
    /// Unload an asset by passing its handle
    /// </summary>
    /// <param name="handle">The handle to unload</param>
    /// <typeparam name="T">The type of the asset the handle contains</typeparam>
    public void Release<T>(AsyncOperationHandle<T> handle)
    {
        Addressables.Release<T>(handle);
    }

    /// <summary>
    /// Instantiate a GameObject by address
    /// </summary>
    /// <param name="address">The address of the GameObject to instantiate</param>
    /// <param name="parent">The parent transform, if any</param>
    /// <param name="instantiateInWorldSpace">If the instantiation should happen in worldspace or not</param>
    /// <returns>The instantiated GameObject</returns>
    /// <exception cref="InvalidKeyException">Thrown when the instantiation fails</exception>
    public GameObject Instantiate(string address, Transform parent = null,
        bool instantiateInWorldSpace = false)
    {
        AsyncOperationHandle<GameObject> handle =
            Addressables.InstantiateAsync(address, parent, instantiateInWorldSpace);
        handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }

        throw new InvalidKeyException(
            $"[{GetType().Name}] Failed to instantiate GameObject with address {address}");
    }

    /// <summary>
    /// Instantiate a GameObject by AssetReference
    /// </summary>
    /// <param name="aref">The AssetReference to instantiate</param>
    /// <param name="parent">The parent transform, if any</param>
    /// <param name="instantiateInWorldSpace">If the instantiation should happen in worldspace or not</param>
    /// <returns>The instantiated GameObject</returns>
    /// <exception cref="InvalidKeyException">Thrown when the instantiation fails</exception>
    public GameObject Instantiate(AssetReference aref, Transform parent = null,
        bool instantiateInWorldSpace = false)
    {
        AsyncOperationHandle<GameObject> handle =
            Addressables.InstantiateAsync(aref, parent, instantiateInWorldSpace);
        handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }

        throw new InvalidKeyException(
            $"[{GetType().Name}] Failed to instantiate GameObject with reference {aref}");
        
    }

    /// <summary>
    /// Instantiate a GameObject by AssetReference
    /// </summary>
    /// <param name="aref">The AssetReference to instantiate</param>
    /// <param name="position">The location to instantiate the object at</param>
    /// <param name="rotation">The rotation to instantiate the object at</param>
    /// <param name="instantiateInWorldSpace">If the instantiation should happen in worldspace or not</param>
    /// <returns>The instantiated GameObject</returns>
    /// <exception cref="InvalidKeyException">Thrown when the instantiation fails</exception>
    public GameObject Instantiate(AssetReference aref, Vector3 position, Quaternion rotation,
        bool instantiateInWorldSpace = false)
    {
        IResourceLocation loc = Addressables.LoadResourceLocationsAsync(aref).Result[0];
        AsyncOperationHandle<GameObject> handle =
            Addressables.InstantiateAsync(loc, position, rotation);
        handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }

        throw new InvalidKeyException(
            $"[{GetType().Name}] Failed to instantiate GameObject with reference {aref}");
    }
    public async UniTask<GameObject> InstantiateAsync(string address, Transform parent = null,
        bool instantiateInWorldSpace = false)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, parent, instantiateInWorldSpace);

        return await handle;
    }
    
    public async UniTask<GameObject> InstantiateAsync(AssetReference aref, Transform parent = null,
        bool instantiateInWorldSpace = false)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(aref, parent, instantiateInWorldSpace);

        return await handle;
    }
    
}
