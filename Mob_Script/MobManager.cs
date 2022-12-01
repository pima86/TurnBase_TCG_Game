using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MobManager : MonoBehaviour
{
    public static MobManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] MonsterSO monsterSO;
    [SerializeField] StoryMonsterSO smSO;
    [SerializeField] AccSO accSO;
    [SerializeField] GameObject Prefab;
    [SerializeField] GameObject Item_Prefab;

    [SerializeField] LeftCanvasPanel LeftPanel;

    [SerializeField] List<Stat_Drop> Player_Item;

    public List<Mob> InField;
    public List<string> DropItem;

    public Mob select;
    public int addrass = 0;
    public string[] Mobname;

    public bool Playing = false;

    void Start()
    {
        Invoke("StartGame",0.05f);
    }

    void Update()
    {
        if (Playing)
            ClearStage();
    }

    public void DropList(List<string> droplist)//죽일시 드랍아이템 관리용
    {
        DropItem = new List<string>(droplist);

        for (int i = 0; i < DropItem.Count; i++)
        {
            //Debug.Log(DropItem[i]);
        }
    }

    void ClearStage() //교전 승리
    {
        if (InField.Count == 0 && DropItem.Count == 0)
        {
            DropItem_Mob.Inst.GetItem();
            StoryOpen.Inst.Openning(Player.Inst.playerdata.story);

            for (int i = 0; i < Player.Inst.playerdata.storyList.Count; i++) //주소값 찾기
            {
                if (Player.Inst.playerdata.storyList[i].name == Player.Inst.playerdata.story)
                    Player.Inst.playerdata.storyList[i].clear = true;
            }

            Playing = false;
        }
    }

    #region 프리팹 추가
    public Monster PopItem()
    {
        for (int i = 0; i < monsterSO.monsters.Length; i++)
        {
            if (Mobname[addrass] == null || Mobname[addrass] == "")
            {
                return null;
            }
            else if (Mobname[addrass] == monsterSO.monsters[i].name)
            {
                Monster monster = monsterSO.monsters[i];
                addrass++;
                return monster;
            }
        }
        return null;
    }

    public void AddCard()
    {
        Monster m = PopItem();
        if (m != null)
        {
            var mobObject = Instantiate(Prefab, new Vector3(0, 0, 0), Utils.QI);
            var mob = mobObject.GetComponent<Mob>();
            mob.SetUp(m);
            HPbar.Inst.MobCalculator(mobObject);
            mob.HPUpdate();
            InField.Add(mob);

            StageBlock.Inst.Mob.Add(mobObject); //칸 스크립트에서 어느 위치에 있는지 파악하기 위함.
            CardAlignment();
        }
        else
        {
            StageBlock.Inst.Mob.Add(null);
            //StageBlock.Inst.Size_num += 1;
            addrass++;
        }
    }


    public void AddMob(Monster m, int num, int add = 0, int special = 0)
    {
        var mobObject = Instantiate(Prefab, new Vector3(0, 0, 0), Utils.QI);
        var mob = mobObject.GetComponent<Mob>();
        mob.SetUp(m);
        HPbar.Inst.MobCalculator(mobObject);
        mob.HPUpdate();
        InField.Add(mob);
        StageBlock.Inst.Mob[num] = mobObject; //칸 스크립트에서 어느 위치에 있는지 파악하기 위함.
        mob.TakeOver(add, special);
        CardAlignment();
        StageBlock.Inst.Test_St_Clear();
        mob.ThisMonsterStay();
    }

    void AddItem(string name)
    {
        int index = accSO.acc.FindIndex(x => x.name == name);

        var accObject = Instantiate(Item_Prefab, new Vector3(0, 0, 0), Utils.QI, GameObject.Find("장착아이템").transform);
        var drop_item = accObject.GetComponent<Stat_Drop>();

        drop_item.SetUp(accSO.acc[index]);//SO에서 해당 정보를 가져옵니다.
        Player_Item.Add(drop_item);//현재 인스턴스화된 프리팹 오브젝트들의 모임

        ItemAlignment();
    }

    void ItemAlignment()
    {
        List<Vector3> originPos = new List<Vector3>();
        originPos = ItemAlignment(Player_Item.Count);
        for (int i = 0; i < Player_Item.Count; i++)
        {
            Player_Item[i].ScaleThis(Vector3.one * 0.8f);
            Player_Item[i].ReturnGameObject().transform.DOMove(originPos[i], 0.01f);
        }
    }

    List<Vector3> ItemAlignment(int objCount)
    {
        float objLerps = 5f;
        List<Vector3> results = new List<Vector3>();

        for (int i = 0; i < objCount; i++)
        {
            results.Add(new Vector3(-6.5f, objLerps, -9.5f));

            objLerps -= 0.5f;
        }

        return results;
    }

    List<PRS> originCardPRSs = new List<PRS>();
    void CardAlignment()
    {
        originCardPRSs = RoundAlignment(StageBlock.Inst.MobSpawnRoom(StageBlock.Inst.Mob.Count), InField.Count, Vector3.one * 0.3f);

        for (int i = 0; i < InField.Count; i++)
        {
            if (InField[i] != null)
            {
                InField[i].originPRS = originCardPRSs[i];
                InField[i].MoveTransform(InField[i].originPRS, false, 0);
            }
        }
    }

    List<PRS> RoundAlignment(List<Transform> CardTr, int objCount, Vector3 scale)
    {
        List<PRS> results = new List<PRS>(objCount);

        for (int i = 0; i < objCount; i++)
        {
            var myCardPos = new Vector3(CardTr[i].position.x, -1.91f, -2.07f);
            var myCardRot = Quaternion.Euler(0, 0, 0);
            results.Add(new PRS(myCardPos, myCardRot, scale));
        }
        return results;
    }
    #endregion

    public void On_panel()
    {
        LeftPanel.On_Move();
    }

    public void StartGame()
    {
        for (int i = 0; i < smSO.sm.Length; i++)
        {
            if (smSO.sm[i].name != null || smSO.sm[i].name != "")
            {
                if (Player.Inst.playerdata.story == smSO.sm[i].name)
                {
                    Mobname = smSO.sm[i].monster;
                    StageBlock.Inst.Start_Player(smSO.sm[i].player, smSO.sm[i].where);
                }
            }
        }
        for (int i = 0; i < Mobname.Length; i++)
            AddCard();

        List<string> use_item = Player.Inst.playerdata.box_1;
        for (int i = 0; i < use_item.Count; i++)
        {
            if(use_item[i] != null && use_item[i] != "")
                AddItem(use_item[i]);
        }

        Playing = true;
    }

    public void ZeroShield()
    {
        for (int i = 0; i < InField.Count; i++)
        {
            InField[i].ZeroShield();
        }
    }

    [SerializeField] List<GameObject> Go_stay; //이미 움직인 오브젝트의 개수
    public void monsterStay()
    {
        StageBlock.Inst.Test_St_Clear();

        if (Go_stay.Count > 0 && TurnManager.Inst.myTurn)
            Go_stay.Clear();

        GameObject[] dummy = (GameObject[])StageBlock.Inst.situation.Clone();

        for (int i =  0; i < dummy.Length; i++)
        {
            int isHave = Go_stay.FindIndex((GameObject p) => p == dummy[i]);
            if (dummy[i] != null && dummy[i] != StageBlock.Inst.player && isHave == -1)
            {
                dummy[i].GetComponent<Mob>().ThisMonsterStay();
            }
        }
    }

    public IEnumerator monsterGo()
    {
        int mobcount = 0;
        for (int j = 0; j < StageBlock.Inst.situation.Length; j++)
        {
            if (StageBlock.Inst.situation[j] != null && StageBlock.Inst.situation[j] != StageBlock.Inst.player)
                mobcount++;
        }

        GameObject[] dummy = (GameObject[])StageBlock.Inst.situation.Clone();
        int i = 0;
        while(true)
        {
            int isHave = Go_stay.FindIndex((GameObject p) => p == dummy[i]);
            if (dummy[i] != null && dummy[i] != StageBlock.Inst.player && isHave == -1)
            {
                Go_stay.Add(dummy[i]);
                dummy[i].GetComponent<Mob>().ThisMonsterGo();
                yield return new WaitForSeconds(0.5f);
            }

            i++;
            if (Go_stay.Count == mobcount)
                break;
        }
        TurnManager.Inst.EndTurn();
    }
}
