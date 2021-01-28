using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [Serializable]
    public class ObjectPoolEntry
    {
        [SerializeField]
        public GameObject m_prefab;
        [SerializeField]
        public int m_count = 3;
    }

    public ObjectPoolEntry[] m_entries;
    private List<GameObject>[] m_pools;
    private GameObject m_container;

    public static PoolManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            InitializePool();
        }
    }

    public void PoolObject(GameObject gameObject, bool keepActive)
    {
        bool found = false;
        for (int i = 0; i < m_entries.Length && !found; i++)
        {
            // verify this object has a pool
            if (gameObject.name == m_entries[i].m_prefab.name)
            {
                gameObject.SetActive(keepActive);
                // move to container in hierarchy
                gameObject.transform.parent = m_container.transform;
                m_pools[i].Add(gameObject);
                found = true;
            }
        }
    }

    private void InitializePool()
    {
        // create the hierarchy container
        m_container = new GameObject("ObjectPool");
        m_container.transform.parent = transform;

        // create pools for each entry defined
        m_pools = new List<GameObject>[m_entries.Length];
        for (int i = 0; i < m_entries.Length; i++)
        {
            // grab an entry
            ObjectPoolEntry poolEntry = m_entries[i];
            // initialize the pool it will use
            m_pools[i] = new List<GameObject>();
            for (int n = 0; n < poolEntry.m_count; n++)
            {
                // instantiate the prefab
                GameObject go = Instantiate(poolEntry.m_prefab);
                // name it accordingly
                go.name = poolEntry.m_prefab.name;
                // place it in its pool, deactivated
                PoolObject(go, false);
            }
        }
    }

    public GameObject GetObjectOfType(string objectType, bool onlyPooled = true)
    {
        GameObject gameObject = null;
        bool found = false;
        for (int i = 0; i < m_entries.Length && !found; i++)
        {
            // look for the matching entry
            if (m_entries[i].m_prefab.name == objectType)
            {
                found = true;
                // verify an object is available in the pool
                if (m_pools[i].Count > 0)
                {
                    // retrieve the object
                    gameObject = m_pools[i][0];
                    // remove it from the pool
                    m_pools[i].RemoveAt(0);
                    // detach it from the hierarchy container
                    gameObject.transform.parent = null;
                    // activate
                    gameObject.SetActive(true);
                }
                // pool is empty, verify if permitted to create a new object
                else if (!onlyPooled)
                {
                    GameObject newObject = Instantiate(m_entries[i].m_prefab);
                    newObject.name = m_entries[i].m_prefab.name;
                    gameObject = newObject;
                }
            }
        }

        return gameObject;
    }
}
