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

    private void InitializePool()
    {
        m_container = new GameObject("ObjectPool");
        m_container.transform.parent = transform;

        m_pools = new List<GameObject>[m_entries.Length];
        for (int i = 0; i < m_entries.Length; i++)
        {
            ObjectPoolEntry poolEntry = m_entries[i];
            m_pools[i] = new List<GameObject>();
            for (int n = 0; n < poolEntry.m_count; n++)
            {
                GameObject go = Instantiate(poolEntry.m_prefab);
                go.name = poolEntry.m_prefab.name;
                PoolObject(go, false);
            }
        }
    }

    public void PoolObject(GameObject gameObject, bool keepActive)
    {
        bool found = false;
        for (int i = 0; i < m_entries.Length && !found; i++)
        {
            if (gameObject.name == m_entries[i].m_prefab.name)
            {
                gameObject.SetActive(keepActive);
                gameObject.transform.parent = m_container.transform;
                m_pools[i].Add(gameObject);
                found = true;
            }
        }
    }

    public GameObject GetObjectOfType(string objectType, bool onlyPooled = true)
    {
        GameObject gameObject = null;
        bool found = false;
        for (int i = 0; i < m_entries.Length && !found; i++)
        {
            if (m_entries[i].m_prefab.name == objectType)
            {
                found = true;
                if (m_pools[i].Count > 0)
                {
                    gameObject = m_pools[i][0];
                    m_pools[i].RemoveAt(0);
                    gameObject.transform.parent = null;
                    gameObject.SetActive(true);
                }
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
