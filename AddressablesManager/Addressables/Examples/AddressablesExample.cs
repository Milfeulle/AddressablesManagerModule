using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace AddressablesManagement
{
    public class AddressablesExample : MonoBehaviour
    {
        public GameObject loneGameObject;
        public GameObject[] testObjects = new GameObject[20];
        public List<Material> materials;
        public List<string> listTest;
        Sprite sprite;
        Scene sceneToLoad;
        private Vector3 startingPos;

        private GameObject loadGameobject = null;

        private ObjectLoader objLoader = new ObjectLoader();

        [SerializeField] private List<GameObject> testObjs = new List<GameObject>();

        IEnumerator Start()
        {
            //materials = new List<Material>(3);

            //startingPos = transform.position;

            ////LoadScene("SceneTest");
            ////InstantiateSingleObject("TestObject");
            ////TestInstantiateObjects();
            ////TestLoadByLabel();
            ////LoadFromList(listTest);
            ///
            //ObjectLoader objLoaderPooled = AddressablesManager.Instance.LoaderObjectsPool.Get();
            //yield return StartCoroutine(AddressablesManager.Instance.TryInstantiateGameobject("TestObject", Vector3.zero, Quaternion.identity, objLoaderPooled));
            //loadGameobject = objLoaderPooled.loadedGameobject;

            yield return objLoader.LoadGameObject("TestObject", Vector3.zero, Quaternion.identity);
            loadGameobject = objLoader.loadedGameObject;
            Debug.Log(loadGameobject.name);
        }

        async void TestLoadByLabel()
        {
            materials = await AddressablesManager.Instance.LoadAssetsByLabel<Material>("materials");
        }

        async void TestInstantiateObjects()
        {
            Vector3 pos = startingPos;

            for (int i = 0; i < testObjects.Length; i++)
            {
                testObjects[i] = await AddressablesManager.Instance.InstantiateGameObject("TestObject");
                testObjects[i].transform.position = pos;
                pos += new Vector3(2f, 0, 0);
            }
        }

        async void InstantiateSingleObject(string path)
        {
            loneGameObject = await AddressablesManager.Instance.InstantiateGameObject(path);
        }

        async void LoadSprite(string path)
        {
            sprite = await AddressablesManager.Instance.Load<Sprite>(path);
        }

        async void LoadScene(string sceneName)
        {            
            sceneToLoad = await AddressablesManager.Instance.LoadScene(sceneName, LoadSceneMode.Single);
        }

        async void OnlyLoadScene(string sceneName)
        {
            await AddressablesManager.Instance.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        async void UnloadScene(SceneInstance scene)
        {
            await AddressablesManager.Instance.UnloadScene(scene);
        }

        async void LoadFromList(List<string> testList)
        {
            List<GameObject> GOs = await AddressablesManager.Instance.LoadFromList<GameObject>(testList);

            foreach (GameObject go in GOs)
            {
                Instantiate(go);
            }
        }
    }
}