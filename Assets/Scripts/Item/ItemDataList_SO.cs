using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataList_SO", menuName = "Inventory/ItemDataList")]
public class ItemDataList_SO : ScriptableObject
{
    public List<ItemDetails> itemDetailsList;

    public ItemDetails GetItemDetailsFromID(int itemID)
    {
        return itemDetailsList.FirstOrDefault(itemDetail => itemDetail.itemID == itemID);
    }
}
