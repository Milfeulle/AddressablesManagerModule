using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AddressablesManagement;
using UnityEngine.AddressableAssets;

public class ObjectLoader
{
    public GameObject loadedGameobject;

    //public GameObject LoadedGameObject
    //{
    //    get
    //    {
    //        GameObject temp = _loadedGameobject;
    //        _loadedGameobject = null;

    //        return temp;
    //    }
    //}

    public ObjectLoader()
    {
        loadedGameobject = null;
    }

    public IEnumerator LoadGameObject(string path, Vector3 pos, Quaternion rot, MonoBehaviour mono)
    {
        yield return mono.StartCoroutine(LoadGameObject(path, pos, rot));
    }

    public IEnumerator LoadGameObject(string path, Vector3 pos, Quaternion rot)
    {
        var operation = Addressables.InstantiateAsync(path, pos, rot);
        operation.Completed += (op) =>
        {
            loadedGameobject = op.Result;
        };
        yield return operation;
    }
}
