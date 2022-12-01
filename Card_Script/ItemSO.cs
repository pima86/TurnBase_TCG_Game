using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public Sprite illust; //일러스트
    public Sprite side; //일러스트
    public string name; //이름
    public int cost; //공격
    public int attack; //공격
    public int defense; //수비
    public string type; //효과
    public string effect; //효과

    public int percent; //카드를 뽑을 확률
    public int range; //사거리
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Object/ItemSO")]
public class ItemSO : ScriptableObject
{
    public List<Item> items; //덱
}
