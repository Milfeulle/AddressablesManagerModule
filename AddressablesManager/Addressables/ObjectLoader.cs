using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AddressablesManagement;
using UnityEngine.AddressableAssets;

public class ObjectLoader
{
    public GameObject loadedGameobject;

    public ObjectLoader()
    {
        loadedGameobject = null;
    }

    #region Instantiate Operations
    public IEnumerator InstantiateGameobject(string path, Vector3 pos, Quaternion rot, MonoBehaviour mono)
    {
        yield return mono.StartCoroutine(InstantiateGameobject(path, pos, rot));
    }

    public IEnumerator InstantiateGameobject(string path, Vector3 pos, Quaternion rot)
    {
        var operation = Addressables.InstantiateAsync(path, pos, rot);
        operation.Completed += (op) =>
        {
            loadedGameobject = op.Result;
        };
        yield return operation;
    }
    #endregion

    #region Load Operations
    public IEnumerator LoadGameobject(string path, Vector3 pos, Quaternion rot, MonoBehaviour mono)
    {
        yield return mono.StartCoroutine(LoadGameobject(path, pos, rot));
    }

    public IEnumerator LoadGameobject(string path)
    {
        var operation = Addressables.LoadAssetAsync<GameObject>(path);
        operation.Completed += (op) =>
        {
            loadedGameobject = op.Result;
        };
        yield return operation;
    }

    public IEnumerator LoadGameobject(string path, Vector3 pos, Quaternion rot)
    {
        var operation = Addressables.LoadAssetAsync<GameObject>(path);
        operation.Completed += (op) =>
        {
            loadedGameobject = op.Result;
            loadedGameobject.transform.position = pos;
            loadedGameobject.transform.rotation = rot;
        };
        yield return operation;
    }
    #endregion
}
