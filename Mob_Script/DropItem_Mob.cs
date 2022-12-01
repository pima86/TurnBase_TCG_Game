using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropItem_Mob : MonoBehaviour
{
    public static DropItem_Mob Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] GameObject Prefab;

    [SerializeField] GameObject Bag_Light;
    [SerializeField] TMP_Text Bag_num;

    [SerializeField] AccSO accSO;

    [SerializeField] List<Stat_Drop> Item;
    public List<AccItem> Obtain_Item;
    public GameObject target; // ���� ����.

    Transform trans;
    #region ��� ������ ���� 
    /* ClickMob 66��
    public string[] dropitem_icon_str(string name)
    {
        if (name == "��")
        {
            string[] str = { "���� ������", "���� ������ ��", "������� ����" };
            return str;
        }
        else if (name == "���� ������")
        {
            string[] str = { "���� ������", "���� ������ ��", "���볪�� ��� ����" };
            return str;
        }
        return null;
    }
    

    public int[] dropitem_icon_int(string name)
    {
        if (name == "��")
        {
            int[] str = { 80, 50, 30 };
            return str;
        }
        else if (name == "������")
        {
            int[] str = { 80, 50, 30 };
            return str;
        }
        return null;
    }
    */
    #endregion

    void Update()
    {
        if (Obtain_Item.Count != 0)
        {
            Bag_Light.SetActive(true);
            int obtain_amount = 0;
            for (int i = 0; i < Obtain_Item.Count; i++)
            {
                obtain_amount += Obtain_Item[i].real_amount;
            }
            string dummy = "x" + obtain_amount.ToString();
            Bag_num.text = dummy;
        }
        else
        {
            Bag_num.text = "";
            Bag_Light.SetActive(false);
        }
    }

    public void GetItem()
    {
        List<Acc> ItemCollect = Player.Inst.playerdata.ItemCollect;

        for (int i = 0; i < Obtain_Item.Count; i++)
        {
            int addrass = ItemCollect.FindIndex(x => x.name == Obtain_Item[i].originAcc.name);
            if (addrass == -1)
            {
                Acc Acc_dummy = new Acc();
                Acc_dummy.name = Obtain_Item[i].originAcc.name;
                Acc_dummy.amount = 1;
                ItemCollect.Add(Acc_dummy);
            }
            else
            {
                ItemCollect[addrass].amount += 1;
            }
        }
    }

    public List<string> ItemList(string name, Transform trans)
    {
        this.trans = trans;
        List<string> result = new List<string>();

        if (name == "��")
        {
            if (Random_persent() <= 70)
                result.Add("���� ������");
            if (Random_persent() <= 70)
                result.Add("������� ����");
        }
        else if (name == "���� ������")
        {
            if (Random_persent() <= 70)
                result.Add("���� ������ ��");
            if (Random_persent() <= 70)
                result.Add("���볪�� ��� ����");
        }
        else if (name == "���� ������")
        {
            if (Random_persent() <= 70)
                result.Add("���볪�� ��� ����");
            if (Random_persent() <= 70)
                result.Add("��� ��");
            if (Random_persent() <= 5)
                result.Add("������ ���");
        }
        else if (name == "���ȹ���")
        {
            if (Random_persent() <= 70)
                result.Add("Į��");
            if (Random_persent() <= 70)
                result.Add("Į��");
            if (Random_persent() <= 70)
                result.Add("Į ������");
            if (Random_persent() <= 5)
                result.Add("��� ������ ��");
        }
        else if (name == "���� ���� ��")
        {
            if (Random_persent() == 100)
                result.Add("���� ������ ��");
        }
        else if (name == "���� ������2")
        {
            if (Random_persent() <= 80)
                result.Add("�������� ������");
        }
        Drop(result);
        return result;
    }

    int Random_persent()
    {
        int persent = Random.Range(0, 100);
        return persent;
    }

    void Drop(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int index = accSO.acc.FindIndex(x => x.name == list[i]);

            var accObject = Instantiate(Prefab, trans.position, Utils.QI, GameObject.Find("���� ����� ������").transform);
            var drop_item = accObject.GetComponent<Stat_Drop>();

            drop_item.SetUp(accSO.acc[index], 0, false);//SO���� �ش� ������ �����ɴϴ�.
            Item.Add(drop_item);//���� �ν��Ͻ�ȭ�� ������ ������Ʈ���� ����
        }
        StartCoroutine(Obtain());
    }

    IEnumerator Obtain()
    {
        float interval = 0.15f;
        int ItemCount = Item.Count;
        while (ItemCount > 0)
        {
            for (int i = 0; i < 2; i++)
            {
                if (ItemCount > 0)
                {
                    Item[ItemCount - 1].Init(trans, target.transform, 1, 6, 3);
                    //�����, ��������, �ӵ�, �ʹ� ����, �Ĺ� ����
                    ItemCount--;
                }
            }
            yield return new WaitForSeconds(interval);
        }
        Item.Clear();
        yield return null;
    }
}
