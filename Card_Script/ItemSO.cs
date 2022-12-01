using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public Sprite illust; //�Ϸ���Ʈ
    public Sprite side; //�Ϸ���Ʈ
    public string name; //�̸�
    public int cost; //����
    public int attack; //����
    public int defense; //����
    public string type; //ȿ��
    public string effect; //ȿ��

    public int percent; //ī�带 ���� Ȯ��
    public int range; //��Ÿ�
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Object/ItemSO")]
public class ItemSO : ScriptableObject
{
    public List<Item> items; //��
}
