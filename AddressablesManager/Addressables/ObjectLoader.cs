using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AddressablesManagement;
using UnityEngine.AddressableAssets;

public class ObjectLoader
{
    public GameObject loadedGameObject;

    public ObjectLoader()
    {
        loadedGameObject = null;
    }

    #region Instantiate Operations
    public IEnumerator InstantiateGameObject(string path, Vector3 pos, Quaternion rot, MonoBehaviour mono)
    {
        yield return mono.StartCoroutine(InstantiateGameObject(path, pos, rot));
    }

    public IEnumerator InstantiateGameObject(string path, Vector3 pos, Quaternion rot)
    {
        var operation = Addressables.InstantiateAsync(path, pos, rot);
        operation.Completed += (op) =>
        {
            loadedGameObject = op.Result;
        };
        yield return operation;
    }

    public static void InstantiateGameObjectWithCallback(string path, Action<GameObject> callback)
    {
        var operation = Addressables.InstantiateAsync(path);
        operation.Completed += (op) =>
        {
            callback.Invoke(op.Result);
        };
    }
    #endregion

    #region Load Operations
    public IEnumerator LoadGameObject(string path, Vector3 pos, Quaternion rot, MonoBehaviour mono)
    {
        yield return mono.StartCoroutine(LoadGameObject(path, pos, rot));
    }

    public IEnumerator LoadGameObject(string path)
    {
        var operation = Addressables.LoadAssetAsync<GameObject>(path);
        operation.Completed += (op) =>
        {
            loadedGameObject = op.Result;
        };
        yield return operation;
    }

    public IEnumerator LoadGameObject(string path, Vector3 pos, Quaternion rot)
    {
        var operation = Addressables.LoadAssetAsync<GameObject>(path);
        operation.Completed += (op) =>
        {
            loadedGameObject = op.Result;
            loadedGameObject.transform.position = pos;
            loadedGameObject.transform.rotation = rot;
        };
        yield return operation;
    }

    public static IEnumerator LoadAssetRoutineWithCallback<T>(string path, Action<T> onLoaded) where T : class
    {
        var operation = Addressables.LoadAssetAsync<T>(path);
        operation.Completed += (op) =>
        {
            onLoaded.Invoke(op.Result);
        };
        yield return operation;
    }

    public static void LoadAssetWithCallback<T>(string path, Action<T> onLoaded) where T : class
    {
        var operation = Addressables.LoadAssetAsync<T>(path);
        operation.Completed += (op) =>
        {
            onLoaded.Invoke(op.Result);
        };
    }


    public static IEnumerator InstantiateGameObjectRoutineWithCallback(string path, Action<GameObject> onInstantiated)
    {
        var operation = Addressables.InstantiateAsync(path);
        operation.Completed += (op) =>
        {
            onInstantiated.Invoke(op.Result);
        };
        yield return operation;
    }

    #endregion
}
