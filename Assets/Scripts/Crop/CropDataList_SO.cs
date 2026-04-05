using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CropDataList", menuName = "Crop/CropDataList")]
public class CropDataList_SO : ScriptableObject 
{
    public List<CropDetails> cropDetailsList = new List<CropDetails>();
}
