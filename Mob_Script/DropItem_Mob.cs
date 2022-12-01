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
    public GameObject target; // 도착 지점.

    Transform trans;
    #region 드랍 아이템 띄우기 
    /* ClickMob 66줄
    public string[] dropitem_icon_str(string name)
    {
        if (name == "쥐")
        {
            string[] str = { "검은 나무판", "룬이 새겨진 돌", "지그재그 꼬리" };
            return str;
        }
        else if (name == "소형 슬라임")
        {
            string[] str = { "검은 나무판", "룬이 새겨진 돌", "악취나는 녹색 점액" };
            return str;
        }
        return null;
    }
    

    public int[] dropitem_icon_int(string name)
    {
        if (name == "쥐")
        {
            int[] str = { 80, 50, 30 };
            return str;
        }
        else if (name == "슬라임")
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

        if (name == "쥐")
        {
            if (Random_persent() <= 70)
                result.Add("검은 나무판");
            if (Random_persent() <= 70)
                result.Add("지그재그 꼬리");
        }
        else if (name == "소형 슬라임")
        {
            if (Random_persent() <= 70)
                result.Add("룬이 새겨진 돌");
            if (Random_persent() <= 70)
                result.Add("악취나는 녹색 점액");
        }
        else if (name == "중형 슬라임")
        {
            if (Random_persent() <= 70)
                result.Add("악취나는 녹색 점액");
            if (Random_persent() <= 70)
                result.Add("녹색 블럭");
            if (Random_persent() <= 5)
                result.Add("슬라임 덩어리");
        }
        else if (name == "외팔무사")
        {
            if (Random_persent() <= 70)
                result.Add("칼등");
            if (Random_persent() <= 70)
                result.Add("칼날");
            if (Random_persent() <= 70)
                result.Add("칼 손잡이");
            if (Random_persent() <= 5)
                result.Add("어느 무사의 검");
        }
        else if (name == "날개 없는 자")
        {
            if (Random_persent() == 100)
                result.Add("룬이 새겨진 돌");
        }
        else if (name == "보스 슬라임2")
        {
            if (Random_persent() <= 80)
                result.Add("돌연변이 슬라임");
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

            var accObject = Instantiate(Prefab, trans.position, Utils.QI, GameObject.Find("현재 드랍된 아이템").transform);
            var drop_item = accObject.GetComponent<Stat_Drop>();

            drop_item.SetUp(accSO.acc[index], 0, false);//SO에서 해당 정보를 가져옵니다.
            Item.Add(drop_item);//현재 인스턴스화된 프리팹 오브젝트들의 모임
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
                    //출발점, 도착지점, 속도, 초반 각도, 후반 각도
                    ItemCount--;
                }
            }
            yield return new WaitForSeconds(interval);
        }
        Item.Clear();
        yield return null;
    }
}
