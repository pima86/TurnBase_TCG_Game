using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Monster
{
    public string name; //�̸�
    public Sprite illust; //�������Ϸ���Ʈ
    public GameObject idle; //�������Ϸ���Ʈ

    public string live;
    public string rating;

    //ü��
    public int health; 

    //�нú�
    public string passive_1;
    public string passive_2;
    public string passive_3;

    //���丮
    public string story;

    public Sprite[] stand; //�������Ϸ���Ʈ_���ĵ�
}

[CreateAssetMenu(fileName = "MonsterSO", menuName = "Scriptable Object/MonsterSO")]
public class MonsterSO : ScriptableObject
{
    public Monster[] monsters; //����Ʈ ���
}

