using System;
using System.Collections;
using UnityEngine;

namespace AddressablesManagement
{
    public class Pool<T> where T : class, new()
    {
        public bool initialized = false;

        protected T[] pool;
        protected Transform customPoolParent;
        protected Action<T> customInitializer; //optional

        public void Initialize(int initialPoolSize = 1)
        {
            pool = new T[initialPoolSize];

            for (int i = 0; i < initialPoolSize; i++)
            {
                InitializeObjectIntoPool(i);
            }

            initialized = true;
        }

        //This is OnlyUseBeforeInit because it won't initialize existing objects, only configures the pool to initialize objects with this moving forward
        public void UseCustomInitializer_OnlyUseBeforeInit(Action<T> initializer)
        {
            customInitializer = initializer;
        }

        //This is OnlyUseBeforeInit because it won't reparent existing objects, only reparents objects moving forward
        public void UseCustomParent_OnlyUseBeforeInit(Transform parent)
        {
            customPoolParent = parent;
        }

        protected void InitializeObjectIntoPool(int index)
        {       
            pool[index] = new T();

            customInitializer?.Invoke(pool[index]);
        }

        protected virtual T PrepObjectForUse(T obj)
        {                        
            return obj;
        }

        //Returns a fresh object from the expanded area
        public T ExpandPool(int expandAmount)
        {
            int prevLength = pool.Length;

            Array.Resize(ref pool, pool.Length + expandAmount);

            for (int i = prevLength; i < pool.Length; i++)
            {
                InitializeObjectIntoPool(i);
            }

            return pool[pool.Length - 1]; //return the object at the end
        }

        public virtual T Get(int amountToExpandIfNoneLeft = 1)
        {
            foreach (var obj in pool)
            {
                return PrepObjectForUse(obj);
            }

            return PrepObjectForUse(ExpandPool(amountToExpandIfNoneLeft));
        }

        public T GetCustom(Predicate<T> customAvailableForPoolCheck, int amountToExpandIfNoneLeft)
        {
            foreach (var obj in pool)
            {
                if (customAvailableForPoolCheck(obj))
                {
                    return PrepObjectForUse(obj);
                }
            }

            return PrepObjectForUse(ExpandPool(amountToExpandIfNoneLeft));
        }

        public virtual void ReturnToPool(T obj)
        {
            //obj.SetActive(false);
        }

        public void ReturnAllToPool()
        {
            foreach (var obj in pool)
            {
                ReturnToPool(obj);
            }
        }

        public void ForEachObjectInPool(Action<T> objProcessor)
        {
            foreach (var obj in pool)
            {
                objProcessor(obj);
            }
        }
    }

    public class ObjectLoaderPool : Pool<ObjectLoader>
    {
        protected override ObjectLoader PrepObjectForUse(ObjectLoader obj)
        {
            return obj;
        }

        public override void ReturnToPool(ObjectLoader obj)
        {
            obj.loadedGameobject = null;
            //obj.SetActive(false);
        }

        public override ObjectLoader Get(int amountToExpandIfNoneLeft = 1)
        {
            foreach (var obj in pool)
            {
                if (obj.loadedGameobject != null)
                {
                    ReturnToPool(obj);
                    return obj;
                }

                return PrepObjectForUse(obj);
            }

            return PrepObjectForUse(ExpandPool(amountToExpandIfNoneLeft));
        }
    }

    public class GameobjectPool
    {
        protected GameObject[] pool;
        protected GameObject source; //the reference the pool gets filled with
        protected Transform customPoolParent;
        protected Action<GameObject> customInitializer; //optional

        public async void InitializeFromGameObjectAddressable(string addressableKey, int initialPoolSize = 1)
        {
            source = await AddressablesManager.Instance.Load<GameObject>(addressableKey);

            pool = new GameObject[initialPoolSize];

            for (int i = 0; i < initialPoolSize; i++)
            {
                InitializeObjectIntoPool(i);
            }
        }

        //This is OnlyUseBeforeInit because it won't initialize existing objects, only configures the pool to initialize objects with this moving forward
        public void UseCustomInitializer_OnlyUseBeforeInit(Action<GameObject> initializer)
        {
            customInitializer = initializer;
        }

        //This is OnlyUseBeforeInit because it won't reparent existing objects, only reparents objects moving forward
        public void UseCustomParent_OnlyUseBeforeInit(Transform parent)
        {
            customPoolParent = parent;
        }

        void InitializeObjectIntoPool(int index)
        {
            var obj = GameObject.Instantiate(source);
            obj.SetActive(false);
            pool[index] = obj;

            if (customPoolParent)
            {
                obj.transform.SetParent(customPoolParent, true);
            }

            customInitializer?.Invoke(obj);
        }

        protected GameObject PrepObjectForUse(GameObject obj)
        {
            obj.SetActive(true);
            return obj;
        }

        //Returns a fresh object from the expanded area
        public GameObject ExpandPool(int expandAmount)
        {
            int prevLength = pool.Length;

            Array.Resize(ref pool, pool.Length + expandAmount);

            for (int i = prevLength; i < pool.Length; i++)
            {
                InitializeObjectIntoPool(i);
            }

            return pool[pool.Length - 1]; //return the object at the end
        }

        public GameObject Get(int amountToExpandIfNoneLeft)
        {
            foreach (var obj in pool)
            {
                if (!obj.activeSelf)
                {
                    return PrepObjectForUse(obj);
                }
            }

            return PrepObjectForUse(ExpandPool(amountToExpandIfNoneLeft));
        }

        public GameObject GetCustom(Predicate<GameObject> customAvailableForPoolCheck, int amountToExpandIfNoneLeft)
        {
            foreach (var obj in pool)
            {
                if (customAvailableForPoolCheck(obj))
                {
                    return PrepObjectForUse(obj);
                }
            }

            return PrepObjectForUse(ExpandPool(amountToExpandIfNoneLeft));
        }

        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
        }

        public void ReturnAllToPool()
        {
            foreach (var obj in pool)
            {
                ReturnToPool(obj);
            }
        }

        public void ForEachObjectInPool(Action<GameObject> objProcessor)
        {
            foreach (var obj in pool)
            {
                objProcessor(obj);
            }
        }

    }

    public class TimedPool : GameobjectPool
    {
        private MonoBehaviour asyncSource;
        private float delayBeforeReturnToPool;

        public Action<GameObject> OnObjectReturnedToPool; //Assign to this for custom de-initialization behavior for the pooled object.

        public new async void InitializeFromGameObjectAddressable(string addressableKey, int initialPoolSize, MonoBehaviour asyncSource, float delayBeforeReturnToPool)
        {
            this.asyncSource = asyncSource;
            this.delayBeforeReturnToPool = delayBeforeReturnToPool;

            base.InitializeFromGameObjectAddressable(addressableKey, initialPoolSize);
        }

        void StartPoolReturnRoutineForObj(GameObject obj)
        {
            asyncSource.StartCoroutine(DelayedReturn(obj));
        }

        IEnumerator DelayedReturn(GameObject obj)
        {
            yield return new WaitForSeconds(delayBeforeReturnToPool);

            ReturnToPool(obj);

            OnObjectReturnedToPool?.Invoke(obj);
        }

        public new GameObject Get(int amountToExpandIfNoneLeft)
        {
            foreach (var obj in pool)
            {
                if (!obj.activeSelf)
                {
                    StartPoolReturnRoutineForObj(PrepObjectForUse(obj));
                    return obj;
                }
            }

            var resultObj = ExpandPool(amountToExpandIfNoneLeft);
            StartPoolReturnRoutineForObj(PrepObjectForUse(resultObj));
            return resultObj;
        }

        public new GameObject GetCustom(Predicate<GameObject> customAvailableForPoolCheck, int amountToExpandIfNoneLeft)
        {
            foreach (var obj in pool)
            {
                if (customAvailableForPoolCheck(obj))
                {
                    return PrepObjectForUse(obj);
                }
            }

            var resultObj = ExpandPool(amountToExpandIfNoneLeft);
            StartPoolReturnRoutineForObj(PrepObjectForUse(resultObj));
            return resultObj;
        }
    }
}