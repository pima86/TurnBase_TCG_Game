using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Monster
{
    public string name; //이름
    public Sprite illust; //도감용일러스트
    public GameObject idle; //도감용일러스트

    public string live;
    public string rating;

    //체력
    public int health; 

    //패시브
    public string passive_1;
    public string passive_2;
    public string passive_3;

    //스토리
    public string story;

    public Sprite[] stand; //교전용일러스트_스탠드
}

[CreateAssetMenu(fileName = "MonsterSO", menuName = "Scriptable Object/MonsterSO")]
public class MonsterSO : ScriptableObject
{
    public Monster[] monsters; //퀘스트 목록
}

