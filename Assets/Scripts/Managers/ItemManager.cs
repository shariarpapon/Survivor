using System.Collections.Generic;
using UnityEngine;

namespace Survivor.Core
{
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Instance { get; private set; }
        public List<ItemData> itemList = new List<ItemData>();

        private readonly Dictionary<ItemData, int> indexDictionary = new Dictionary<ItemData, int>();

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else 
            {
                Destroy(gameObject);
                return;
            }

            //Init dictionary
            for (int i = 0; i < itemList.Count; i++) indexDictionary.Add(itemList[i], i);
        }

        public int GetItemDataIndex(ItemData data)
        {
            return indexDictionary[data];
        }

        public ItemData GetItemData(int index) => itemList[index];

        public ItemData GetItemData(string itemName)
        {
            itemName = itemName.ToLower().Replace(" ", "");
            foreach (ItemData item in itemList)
                if (item.name.ToLower().Replace(" ", "") == itemName) return item;

            return null;
        }
    }
}
