﻿using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;

namespace AddressablesManagement
{
    public class AddressablesManager : MonoBehaviour
    {
        private static AddressablesManager _instance;

        private bool _currentlyLoading;
        private object _loadedObject;
        private GameObject _loadedGameObject;
        private Scene _currentlyLoadingScene;

        private ObjectLoaderPool _objectLoadersPool = new ObjectLoaderPool();

        #region PROPERTIES
        /// <summary>
        /// Reports whether there's an object currently loading or not.
        /// </summary>
        public bool CurrentlyLoading
        {
            get { return _currentlyLoading; }
            private set { _currentlyLoading = value; }
        }

        public object LoadedObject
        {
            get
            {
                object temp = _loadedObject;
                _loadedObject = null;

                return temp;
            }
        }

        public GameObject LoadedGameObject
        {
            get
            {
                GameObject temp = _loadedGameObject;
                _loadedGameObject = null;

                return temp;
            }
        }

        public ObjectLoaderPool LoaderObjectsPool
        {
            get
            {
                if (!_objectLoadersPool.initialized)
                    _objectLoadersPool.Initialize(10);

                return _objectLoadersPool;
            }
        }

        /// <summary>
        /// Instance object of this class
        /// </summary>
        public static AddressablesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Create();
                }

                return _instance;
            }
        }
        #endregion

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;                
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }

            if (!_objectLoadersPool.initialized)
                _objectLoadersPool.Initialize(10);
        }     

        #region PUBLIC METHODS
        /// <summary>
        /// Loads a given scene either in additive or single mode to the current scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load.</param>
        /// <param name="loadMode">Scene load mode.</param>
        public Task<Scene> LoadScene(string sceneName, LoadSceneMode loadMode)
        {
            _currentlyLoadingScene = default;
            _currentlyLoadingScene.name = "";            

            Addressables.LoadSceneAsync(sceneName, loadMode).Completed += AddressablesManager_OnSceneLoadCompleted;

            if (string.IsNullOrEmpty(_currentlyLoadingScene.name))
            {
                Task.Delay(1);
            }

            return Task.Run(() => _currentlyLoadingScene);
        }

        private void AddressablesManager_OnSceneLoadCompleted(AsyncOperationHandle<SceneInstance> obj)
        {
            if (obj.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                _currentlyLoadingScene = obj.Result.Scene;

            }
        }

        /// <summary>
        /// Loads a given scene either in additive or single mode to the current scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load.</param>
        /// <param name="loadMode">Scene load mode.</param>
        public async Task OnlyLoadScene(string sceneName, LoadSceneMode loadMode)
        {
            await Task.Run(() => Addressables.LoadSceneAsync(sceneName, loadMode));
        }

        /// <summary>
        /// Unloads a given scene from memory asynchronously.
        /// </summary>
        /// <param name="scene">Scene object to unload.</param>
        public async Task UnloadScene(SceneInstance scene)
        {
            await Task.Run(() => Addressables.UnloadSceneAsync(scene));
        }

        /// <summary>
        /// Instantiates a gameobject in a given position and rotation in the world.
        /// </summary>
        /// <param name="path">Addressables</param>
        /// <param name="position">Position in the world to instantiate the gameobject.</param>
        /// <param name="rotation">Rotation to instantiate the gameobject.</param>
        /// <returns>Returns the gameobject set to instantiate.</returns>
        public async Task<GameObject> InstantiateGameObject(string path, Vector3 position, Quaternion rotation)
        {
            return await Addressables.InstantiateAsync(path, position, rotation) as GameObject;
        }

        /// <summary>
        /// Instantiates a gameobject in a default position and rotation in the world.
        /// </summary>
        /// <param name="path">Project path where the gameobject resides.</param>
        /// <returns>Returns the gameobject set to instantiate.</returns>
        public async Task<GameObject> InstantiateGameObject(string path)
        {
            return await Addressables.InstantiateAsync(path, Vector3.zero, Quaternion.identity) as GameObject; ;
        }

        /// <summary>
        /// Loads an object of type T into memory.
        /// </summary>
        /// <typeparam name="T">Type of object to load.</typeparam>
        /// <param name="path">Path of the object to load in the Addressables system.</param>
        /// <returns>Returns an object of type T.</returns>
        public async Task<T> Load<T>(string path) where T : class
        {
            return await Addressables.LoadAssetAsync<T>(path);
        }

        /// <summary>
        /// Preloads all dependencies of an object, given its path.
        /// </summary>
        /// <param name="path">Path of the object to load dependencies from.</param>
        public async Task DownloadDependencies(string path)
        {
            await Addressables.DownloadDependenciesAsync(path);
        }

        /// <summary>
        /// Loads all assets of a given label into memory.
        /// </summary>
        /// <typeparam name="T">Type of the objects to load.</typeparam>
        /// <param name="label">Label in the Addressables of the objects to load.</param>
        /// <param name="callback">Callback to execute after loading</param>
        /// <returns>Returns a list with elements of type T.</returns>
        public async Task<List<T>> LoadAssetsByLabel<T>(string label, Action<T> callback = null) where T : class
        {
            return await Addressables.LoadAssetsAsync<T>(label, callback) as List<T>;
        }

        /// <summary>
        /// Loads all assets from a given list into memory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="labels">List of labels to load</param>
        /// <param name="callback">Callback to execute after loading</param>
        /// <param name="mergeMode">Assets merge mode. For more information, visit https://docs.unity3d.com/Packages/com.unity.addressables@0.4/api/UnityEngine.AddressableAssets.Addressables.MergeMode.html?q=mergemode</param>
        /// <returns>Returns a list with elements of type T.</returns>
        public async Task<List<T>> LoadFromList<T>(List<string> labels, Action<T> callback = null, Addressables.MergeMode mergeMode = Addressables.MergeMode.None) where T : class
        {
            List<object> labelsAsObjects = labels.Cast<object>().ToList();
            return await Addressables.LoadAssetsAsync<T>(labelsAsObjects, callback, mergeMode) as List<T>;
        }

        /// <summary>
        /// Releases prefab from memory and destroys its instance in the currently active scene.
        /// </summary>
        /// <param name="obj">Gameobject reference of the prefab to release.</param>
        public void ReleaseInstance(ref GameObject obj)
        {
            if (obj != null)
                Addressables.ReleaseInstance(obj);
        }
        /// <summary>
        /// Releases a given object from memory.
        /// </summary>
        /// <typeparam name="T">Type of the object to release.</typeparam>
        /// <param name="obj">Object reference to release.</param>
        public void ReleaseAsset<T>(ref T obj) where T : class
        {
            if (obj != null)
                Addressables.Release(obj);
        }
        #endregion

        /// <summary>
        /// Instantiates a gameobject through the Addressables namespace.
        /// </summary>
        /// <param name="path">Project path where the gameobject resides.</param>
        /// <param name="position">Position in the world to instantiate the gameobject.</param>
        /// <param name="rotation">Rotation to instantiate the gameobject in.</param>
        public IEnumerator TryLoadObject<T>(string path) where T : class
        {
            var operation = Addressables.LoadAssetAsync<T>(path);
            operation.Completed += (op) =>
            {
                _loadedObject = op.Result;
            };
            yield return operation;
        }

        /// <summary>
        /// Instantiates a gameobject through the Addressables namespace.
        /// </summary>
        /// <param name="path">Project path where the gameobject resides.</param>
        /// <param name="position">Position in the world to instantiate the gameobject.</param>
        /// <param name="rotation">Rotation to instantiate the gameobject in.</param>
        public IEnumerator TryInstantiateGameobject(string path, Vector3 position, Quaternion rotation, ObjectLoader objLoader)
        {            
            var operation = Addressables.InstantiateAsync(path, position, rotation);
            operation.Completed += (op) =>
            {
                objLoader.loadedGameObject = op.Result;
            };
            yield return operation;
        }

        /// <summary>
        /// Creates a gameobject with the Addressables Manager as a component.
        /// </summary>
        private static void Create()
        {
            new GameObject("AddressablesManager").AddComponent<AddressablesManager>();
        }

        private void OnApplicationQuit()
        {
            StopAllCoroutines();
        }      
    }
}