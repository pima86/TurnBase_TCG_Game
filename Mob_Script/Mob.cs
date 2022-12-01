using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using TMPro;
using DG.Tweening;

public class Mob : MonoBehaviour
{
    #region 변수들
    [SerializeField] MonsterSO monsterSO;

    public ParticleSystem plusbuff;
    public ParticleSystem minusbuff;
    public ParticleSystem shiledbuff;

    public ParticleSystem part;
    public GameObject HPobj;
    public GameObject rotate;//회전
    public SpriteRenderer illust;
    [SerializeField] SpriteRenderer effect;
    
    public TMP_Text name;
    

    [SerializeField] GameObject[] buff;//버프이미지들
    [SerializeField] GameObject GoNum;//다음 행동에 취해질 숫자
    [SerializeField] TMP_Text Go_Time;

    [SerializeField] GameObject Idle;//다음 행동에 취해질 숫자
    

    [SerializeField] Sprite[] Behavior;
    [SerializeField] Sprite[] Motion;

    public Monster monster;
    public PRS originPRS;
    public int originAdd;
    public int HP;


    float hpx; //해당 몬스터의 스프라이트 사이즈
    float hpy;
    #region 상태관련
    public int damage = 0;
    public int shield = 0;
    public int mob_power = 0;
    public int mob_time = 1;

    public int vulnerable = 0; //취약
    public int powerUp = 0;
    public int weak = 0; //약화
    public int bleed = 0; //출혈

    //특수효과
    public int Slime = 0; //슬라임
    #endregion

    Rigidbody2D rb;

    TMP_Text shield_TMP;
    TMP_Text GoNum_TMP;
    Transform ThisObj;
    #endregion

    #region 상태이상
    void UseBuff()
    {
        if (vulnerable != 0)
        {
            if(buff[0].activeSelf == false)
                buff[0].SetActive(true);
            buff[0].transform.GetChild(0).GetComponent<TMP_Text>().text = vulnerable.ToString();
        }
        else
            buff[0].SetActive(false);

        if (weak != 0)
        {
            if (buff[1].activeSelf == false)
                buff[1].SetActive(true);
            buff[1].transform.GetChild(0).GetComponent<TMP_Text>().text = weak.ToString();
        }
        else
            buff[1].SetActive(false);

        if (bleed != 0)
        {
            if (buff[3].activeSelf == false)
                buff[3].SetActive(true);
            buff[3].transform.GetChild(0).GetComponent<TMP_Text>().text = bleed.ToString();
        }
        else
            buff[3].SetActive(false);

        if (powerUp != 0)
        {
            if (buff[4].activeSelf == false)
                buff[4].SetActive(true);
            buff[4].transform.GetChild(0).GetComponent<TMP_Text>().text = powerUp.ToString();
        }
        else
            buff[4].SetActive(false);

        if (Slime != 0)
        {
            if (buff[6].activeSelf == false)
                buff[6].SetActive(true);
            buff[6].transform.GetChild(0).GetComponent<TMP_Text>().text = Slime.ToString();
        }
        else
            buff[6].SetActive(false);

    }

    public void Vulner_buf(int num){ vulnerable += num;}
    public void Weak_buf(int num){ weak += num;}
    public void Bleed_buf(int num){ bleed += num;}

    public void Mob_Turn_buf()
    {
        TurnMobPassive();

        if (vulnerable > 0) vulnerable -= 1;
        if (weak > 0) weak -= 1;
        if (bleed > 0) bleed -= 1;
    }
    #endregion
    #region 죽음
    public void DeadHp()
    {
        HPobj.SetActive(false);
    }
    #endregion
    #region start랑 update
    void Start()
    {
        ThisObj = GetComponent<Transform>();
        GoNum_TMP = GoNum.GetComponent<TMP_Text>();
        rb = GetComponent<Rigidbody2D>();
    }


    Sprite value = null;
    bool resetSprite = false;
    public float time = 0;
    void Update()
    {
        if (mob_power != 0)
        {
            if(weak > 0)
                GoNum_TMP.text = ((mob_power + powerUp) / 2).ToString();
            else 
                GoNum_TMP.text = (mob_power + powerUp).ToString();
        }
        else
            GoNum_TMP.text = "";

        if (mob_time != 1)
            Go_Time.text = "x" + mob_time.ToString();
        else
            Go_Time.text = "";

        UseBuff();

        if (illust.sprite != value)
        {
            value = illust.sprite;
            resetSprite = true;
            time = 0;
        }
        if (resetSprite)
        {
            time += Time.deltaTime;
            if (time > 1f)
            {
                resetSprite = false;
                illustOn(true, 0);
                value = illust.sprite;
            }
        }
    }

    public void MotionChange(int num)
    {
        illust.sprite = Motion[num];
        time = 0;
    }
    #endregion
    #region 데미지 계산, 몬스터 정보 저장, 파괴
    public void SetShield(int num) //실드를 받을 때
    {
        shield += num;
        HPobj.GetComponent<HPUpdate>().ShIeldSetAct_1();
        ShieldSetting(0);
    }

    public void ShieldSetting(int num)
    {
        if (shield > 0)
        {
            HPobj.GetComponent<HPUpdate>().ShIeldSetAct_1();
            shield -= num;
            if (shield <= 0)
            {
                damage += -shield;
                shield = 0;
                HPobj.GetComponent<HPUpdate>().ShIeldSetAct_2();
            }
            else
                HPobj.GetComponent<HPUpdate>().ShIeldSetAct_3(shield);
        }
        else
        {
            HPobj.GetComponent<HPUpdate>().ShIeldSetAct_2();
            damage += num;
        }
    }

    public void SetUpdate(Card card, int dam = 0, bool isBuff = true)
    {
        if (dam == 0)
        {
            ShieldSetting(BuffManager.Inst.damage_player(this, int.Parse(card.attack.text)));
            if (HP - damage <= 0)
            {
                ActionDestro();
                StartCoroutine(BattleCam.Inst.playerattmotion(this, card, true));
            }
            else //if (int.Parse(card.attack.text) != 0)
                StartCoroutine(BattleCam.Inst.playerattmotion(this, card));
        }
        else if(isBuff)
        {
            ShieldSetting(BuffManager.Inst.damage_player(this, dam));

            if (HP - damage <= 0)
            {
                ActionDestro();
                StartCoroutine(BattleCam.Inst.playerattmotion(this, card, true));
            }
        }
        else
        {
            ShieldSetting(dam);

            if (HP - damage <= 0)
            {
                ActionDestro();
                StartCoroutine(BattleCam.Inst.playerattDestroy(this, card));
            }
        }
    }

    void BuffSetUpdate(int num)
    {
        damage += num+1;

        if (HP - damage <= 0)
            StartCoroutine(BattleCam.Inst.playerattDestroy(this, null));
    }

    public void HPUpdate()
    {
        HPobj.GetComponent<HPUpdate>().HPDODO((HP - damage), HP);
    }

    GameObject Idleobj;
    public void SetUp(Monster monster)
    {
        this.monster = monster;

        name.text = this.monster.name;
        patten(name.text);

        if (name.text != "보스 슬라임2")
            HP = this.monster.health;

        for (int i = 0; i < monsterSO.monsters.Length; i++)
        {
            if (name.text == monsterSO.monsters[i].name)
            {
                originAdd = i;
                Idle = monsterSO.monsters[i].idle;
                illust.sprite = monsterSO.monsters[i].stand[0];
                Motion = monsterSO.monsters[i].stand;
            }
        }

        Idleobj = Instantiate(Idle, new Vector3(0, -1.3f, 0), Utils.QI, this.transform.Find("회전시킬오브젝트"));
        ColliderSize();
    }

    public void illustOn(bool act, int num)
    {
        Idleobj.SetActive(act);
        MotionChange(num);
        if(act)
            illust.color = new Color(0f, 0f, 0f, 0f);
        else
            illust.color = new Color(1f, 1f, 1f, 1f);
    }

    bool DieThisMob = false;
    public void ActionDestro()
    {
        for (int i = 0; i < buff.Length; i++)
            buff[i].SetActive(false);

        name.color = new Color(0,0,0,0);
        DieThisMob = true;
        GoNum.SetActive(false);
    }

    public void Destro()
    {
        int addrass_num = StageBlock.Inst.Monster_back(this);
        rotate.SetActive(false);

        List<string> droplist = DropItem_Mob.Inst.ItemList(name.text, gameObject.transform);


        MobManager.Inst.DropList(droplist);

        MobManager.Inst.InField.Remove(this);
        StageBlock.Inst.DestroyMob(gameObject);

        ClickMob.Inst.AllClear();

        if (name.text == "보스 슬라임")
        {
            MobManager.Inst.AddMob(monsterSO.monsters[6], addrass_num, 1, Slime);
        }
        Invoke("Destr_invoke", 0.5f);
    }

    public void TakeOver(int add, int num)
    {
        if (add != 0)
        {
            Slime = num;
            if (Slime == 0)
            {
                HP = 1;
            }
            else
                HP = Slime * 30;
            HPUpdate();
        }
    }

    void Destr_invoke()
    {
        Destroy(gameObject);
    }
    #endregion
    #region 콜라이더 사이즈 및 행동 위치
    void ColliderSize()
    {
        BoxCollider col = gameObject.GetComponent<BoxCollider>();
        hpx = illust.bounds.size.x;
        hpy = illust.bounds.size.y;
        GoNum.transform.position += new Vector3(0, hpy + 1f, 0);
        col.size = new Vector3(hpx, hpy, 0.2f);
        col.center = new Vector3(0, hpy / 2, 0);
    }

    #endregion
    #region 리턴 함수
    public string ReturnName()
    {
        return name.text;
    }
    public GameObject ReturnGameObject()
    {
        return gameObject;
    } //해당 게임오브젝트를 리턴해줌.
    public float ReturnSpriteXY(string str)
    {
        if (str == "X")
            return hpx;
        else if (str == "Y")
            return hpy;
        return 0;
    }
    #endregion
    #region 움직임
    public void MoveForce(bool Direction, int num = 500)
    {
        if(Direction)
            rb.AddForce(new Vector3(num, 0, 0));
        else
            rb.AddForce(new Vector3(-num, 0, 0));
    }

    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
    {
        if (useDotween)
        {
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime);
        }
        else
        {
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
        }
    }
    #endregion
    #region 마우스 이벤트
    void OnMouseOver()
    {
        MobManager.Inst.select = this;
        if (CardManager.Inst.isMyCardDrag)
        {
            int index = StageBlock.Inst.room.FindIndex((Transform p) => p.position.x == gameObject.transform.position.x);
            if (StageBlock.Inst.room[index].GetComponent<SpriteRenderer>().color == new Color(1, 1, 1, 1))
            {
                LineEdit.Inst.MobMouseOver(ThisObj.position + new Vector3(0, hpy * 0.125f, 0));
                StageBlock.Inst.room[index].GetComponent<FieldOn>().StartPart(true);
            }
        }
    }

    void OnMouseExit()
    {
        MobManager.Inst.select = null;
        if (CardManager.Inst.isMyCardDrag)
        {
            LineEdit.Inst.MobMouseExit();

            int index = StageBlock.Inst.room.FindIndex((Transform p) => p.position.x == gameObject.transform.position.x);
            StageBlock.Inst.room[index].GetComponent<FieldOn>().StartPart(false);
        }
    }

    public void MouseUp()
    {
        LineEdit.Inst.MobMouseExit();

        int index = StageBlock.Inst.room.FindIndex((Transform p) => p.position.x == gameObject.transform.position.x);
        StageBlock.Inst.room[index].GetComponent<FieldOn>().StartPart(false);
    }

    void OnMouseDown()
    {
        MobManager.Inst.On_panel();
        //ClickMob.Inst.DropItem_List(name.text); 드랍 아이템 띄우기
        ClickMob.Inst.MobStat(this);
    }
    #endregion

    #region 몬스터 패턴
    //-------------------------------------------------------------------------------
    #region 부가 변수들
    int MobStaynum;

    public string[] patten_damage;
    public int[] patten_range;
    public int patten_num;

    public string[] mob_effect;

    public int Mobdamage = 0; //가할 데미지량. 취약이라던가도 포함
    public int knockback; //넉백 시킬 거리
    #endregion
    //-------------------------------------------------------------------------------
    
    
    public void ZeroShield(int num = 0)
    {
        shield = num;
        ShieldSetting(num);

        StageBlock.Inst.Player_Mob_Flip(ReturnGameObject()); //플레이어가 있는 위치로 고개를 돌리도록
    }
    public void ColorandImage(int num)
    {
        if (num == 6)
        {
            effect.sprite = Behavior[6];
            effect.color = new Color(200 / 255f, 70 / 255f, 255 / 255f, 255 / 255f);
        }
        if (num == 5)
        {
            effect.sprite = Behavior[5];
            effect.color = new Color(255 / 255f, 255 / 255f, 255 / 255f, 255 / 255f);
        }
        if (num == 4)
        {
            effect.sprite = Behavior[4];
            effect.color = new Color(255 / 255f, 255 / 255f, 255 / 255f, 255 / 255f);
        }
        else if (num == 1 || num == 2 || num == 3)
        {
            if (num == 1)
                effect.sprite = Behavior[1];
            else if (num == 2)
                effect.sprite = Behavior[2];
            else if (num == 3)
                effect.sprite = Behavior[3];
            effect.color = new Color(255 / 255f, 76 / 255f, 118 / 255f, 255 / 255f);
        }
        else if (num == 0)
        {
            effect.sprite = Behavior[0];
            effect.color = new Color(53 / 255f, 138 / 255f, 225 / 255f, 255 / 255f);
        }
    }
    public void ThisMonsterStay() //예상되는 몬스터의 행동을 표시
    {
        switch (name.text)
        {
            case "쥐":
                쥐_Stay();
                break;
            case "소형 슬라임":
                소형슬라임_Stay();
                break;
            case "중형 슬라임":
                중형슬라임_Stay();
                break;
            case "외팔무사":
                외팔무사_Stay();
                break;
            case "날개 없는 자":
                날개없는자_Stay();
                break;
            case "보스 슬라임":
                보스슬라임_Stay();
                break;
            case "보스 슬라임2":
                보스슬라임2_Stay();
                break;
        }
    }

    public void ThisMonsterGo() //예상으로 표시된 행동을 수행
    {
        switch (name.text)
        {
            case "쥐":
                StartCoroutine(쥐_Go());
                break;
            case "소형 슬라임":
                StartCoroutine(소형슬라임_Go());
                break;
            case "중형 슬라임":
                StartCoroutine(중형슬라임_Go());
                break;
            case "외팔무사":
                StartCoroutine(외팔무사_Go());
                break;
            case "날개 없는 자":
                StartCoroutine(날개없는자_Go());
                break;
            case "보스 슬라임":
                StartCoroutine(보스슬라임_Go());
                break;
            case "보스 슬라임2":
                StartCoroutine(보스슬라임2_Go());
                break;
        }
    }

    public void TurnMobPassive()
    {
        switch (name.text)
        {
            case "쥐":
                break;
            case "소형 슬라임":
                Debug.Log("damage" + damage);
                if (damage > 0)
                {
                    if (damage > 2)
                        damage -= 2;
                    else
                        damage = 0;

                    BattleCam.Inst.DamageText(this, false, 2, "", Color.green);
                }
                break;
            case "중형 슬라임":
                if (damage > 0)
                {
                    if (damage > 5)
                    {
                        BattleCam.Inst.DamageText(this, false, 5, "", Color.green);
                        damage -= 5;
                    }
                    else
                    {
                        BattleCam.Inst.DamageText(this, false, damage, "", Color.green);
                        damage = 0;
                    }

                    
                }
                break;
            case "외팔무사":
                break;
            case "날개 없는 자":
                break;
            case "보스 슬라임":
                break;
            case "보스 슬라임2":
                if (damage > 0)
                {
                    if (damage > 10)
                        damage -= 10;
                    else
                        damage = 0;

                    BattleCam.Inst.DamageText(this, false, 10, "", Color.green);
                }
                break;
        }
    }

    void DamageIcon(int num) //데미지 수치에 따른 아이콘
    {
        Mobdamage = BuffManager.Inst.damage_mob(this, num);

        if(Mobdamage < 10)
            ColorandImage(1);
        else if (Mobdamage < 20 )
            ColorandImage(2);
        else
            ColorandImage(3);
    }

    void patten(string name)
    {
        patten_damage = new string[3];
        patten_range = new int[3];
        mob_effect = new string[3];
        if (name == "쥐")
        {
            patten_damage[0] = "데미지3";
            patten_damage[1] = "방어막5";
            patten_damage[2] = "데미지10 넉백1";

            patten_range[0] = 1;
            patten_range[1] = 1;
            patten_range[2] = 2;

            mob_effect[0] = "";
            mob_effect[1] = "";
            mob_effect[2] = "";
        }
        else if (name == "소형 슬라임")
        {
            patten_damage[0] = "데미지4";
            patten_damage[1] = "데미지2 약화2";
            patten_damage[2] = "";

            patten_range[0] = 1;
            patten_range[1] = 2;
            patten_range[2] = 0;

            mob_effect[0] = "1턴마다 회복2";
            mob_effect[1] = "";
            mob_effect[2] = "";
        }
        else if (name == "중형 슬라임")
        {
            patten_damage[0] = "방어막10";
            patten_damage[1] = "데미지5";
            patten_damage[2] = "";

            patten_range[0] = 1;
            patten_range[1] = 2;
            patten_range[2] = 0;

            mob_effect[0] = "1턴마다 회복5";
            mob_effect[1] = "장애물에 막히지 않음";
            mob_effect[2] = "";
        }
        else if (name == "외팔무사")
        {
            patten_damage[0] = "데미지2 x (1 + 힘)";
            patten_damage[1] = "데미지10";
            patten_damage[2] = "데미지1 방어막3 기력-1";

            patten_range[0] = 1;
            patten_range[1] = 2;
            patten_range[2] = 5;

            mob_effect[0] = "공격 1회마다 힘1";
            mob_effect[1] = "";
            mob_effect[2] = "";
        }
        else if (name == "날개 없는 자")
        {
            patten_damage[0] = "";
            patten_damage[1] = "";
            patten_damage[2] = "";

            patten_range[0] = 1;
            patten_range[1] = 0;
            patten_range[2] = 0;

            mob_effect[0] = "";
            mob_effect[1] = "";
            mob_effect[2] = "";
        }
        else if (name == "보스 슬라임")
        {
            patten_damage[0] = "방어막10";
            patten_damage[1] = "";
            patten_damage[2] = "";

            patten_range[0] = 6;
            patten_range[1] = 0;
            patten_range[2] = 0;

            mob_effect[0] = "뭔가를 소중히 지키고 있다.";
            mob_effect[1] = "";
            mob_effect[2] = "";
        }
        else if (name == "보스 슬라임2")
        {
            patten_damage[0] = "공격5X스택 넉백2";
            patten_damage[1] = "공격2X스택";
            patten_damage[2] = "";

            patten_range[0] = 1;
            patten_range[1] = 2;
            patten_range[2] = 0;

            mob_effect[0] = "1턴마다 회복10";
            mob_effect[1] = "체력=30X스택";
            mob_effect[2] = "";
        }
    }

    int Distance_P_M(bool no_test = false)
    {
        int play_point = 0;
        int mob_point = 0;

        if (no_test)
        {
            play_point = Array.IndexOf(StageBlock.Inst.situation, GameObject.Find("Character"));
            mob_point = Array.IndexOf(StageBlock.Inst.situation, gameObject);
        }
        else
        {
            play_point = Array.IndexOf(StageBlock.Inst.test_situation, GameObject.Find("Character"));
            mob_point = Array.IndexOf(StageBlock.Inst.test_situation, gameObject);
        }
        int Distance = Mathf.Abs(Mathf.Abs(play_point) - Mathf.Abs(mob_point));
        
        Debug.Log("play_point = " + play_point);
        Debug.Log("mob_point = " + mob_point);
        Debug.Log("Distance = " + Distance);

        return Distance;
    }

    #region 쥐
    void 쥐_skill()
    {
        int play_point = Array.IndexOf(StageBlock.Inst.test_situation, GameObject.Find("Character"));
        int mob_point = Array.IndexOf(StageBlock.Inst.test_situation, gameObject);
        int Distance = Mathf.Abs(Mathf.Abs(play_point) - Mathf.Abs(mob_point));

        if (Distance == 1)
        {
            if (HP - damage <= 5)
                MobStaynum = 1; //실드 5 부여
            else
                MobStaynum = 2; //근접공격 3의 피해
        }
        else if (Distance == 2)
            MobStaynum = 3; //대쉬 공격 10의 피해
        else
            MobStaynum = 0; //앞으로 전진

        if (MobStaynum == 0) //앞에 뭔가 있다면 행동이 막힘.
        {
            if (!StageBlock.Inst.Test_St(StageBlock.Inst.player, gameObject, 1))
                MobStaynum = -1;
            else
                StageBlock.Inst.Test_St_Real(StageBlock.Inst.player, gameObject, 1);
        }
        if (MobStaynum == 2 || MobStaynum == 3)
        {
            StageBlock.Inst.Test_St_Real(gameObject, StageBlock.Inst.player, 1);
        }
        if (MobStaynum == 3)
        {
            StageBlock.Inst.Test_St_Real(StageBlock.Inst.player, gameObject, 1);
        }
    }

    void 쥐_passive()
    {
        knockback = 1; //모든 공격에 넉백1부여
    }

    void 쥐_Stay()
    {
        if(!DieThisMob)
            GoNum.SetActive(true);

        쥐_skill();
        쥐_passive();

        switch (MobStaynum)
        {
            case -1:
                ColorandImage(5);

                mob_power = 0;
                patten_num = 0;
                break;
            case 0: //전진
                ColorandImage(4);

                mob_power = 0;
                patten_num = 0;
                break;
            case 1: //실드
                ColorandImage(0);

                mob_power = 0;
                patten_num = 2;
                break;
            case 2: //3공

                mob_power = 3;
                patten_num = 1;

                DamageIcon(mob_power + powerUp);
                break;
            case 3: //10공

                mob_power = 10;
                patten_num = 3;

                DamageIcon(mob_power + powerUp);
                break;
        }
    }

    IEnumerator 쥐_Go()
    {
        if (!DieThisMob)
            GoNum.SetActive(false);

        switch (MobStaynum)
        {
            case -1:
                //Debug.Log("쥐은 어리둥절해하고 있습니다.");
                break;
            case 0:
                //Debug.Log("쥐가 1칸 이동합니다.");
                ClipManager.Inst.Mouse_1();
                StageBlock.Inst.Move_mob(gameObject, 1);
                illustOn(false, 1);
                yield return new WaitForSeconds(0.3f);
                illustOn(true, 0);
                break;
            case 1:
                //Debug.Log("쥐가 자신에게 5의 실드를 부여했습니다.");
                SetShield(5);
                break;
            case 2:
                //Debug.Log("쥐가 플레이어에게 3의 피해를 입혔습니다.");
                ClipManager.Inst.Mouse_3();
                PlayerCharacter.Inst.SetUpdate(this);
                break;
            case 3:
                //Debug.Log("쥐가 플레이어에게 10의 피해를 입혔습니다.");
                illustOn(false, 1);
                ClipManager.Inst.Mouse_1();
                StageBlock.Inst.Knockback_mob(this, null, false, -1);  //앞을 뚫고 이동
                yield return new WaitForSeconds(0.2f);
                ClipManager.Inst.Mouse_3();
                PlayerCharacter.Inst.SetUpdate(this);
                break;
        }

        yield return new WaitForSeconds(0.7f);
        Finish();
    }
    #endregion
    #region 소형 슬라임
    void 소형슬라임_skill()
    {
        int play_point = Array.IndexOf(StageBlock.Inst.test_situation, GameObject.Find("Character"));
        int mob_point = Array.IndexOf(StageBlock.Inst.test_situation, gameObject);

        int Distance = Mathf.Abs(Mathf.Abs(play_point) - Mathf.Abs(mob_point));
        if (Distance == 2)
        {
            MobStaynum = 2; //공격2
        }
        else if (Distance == 1)
        {
            MobStaynum = 1; //공격4
        }
        else
            MobStaynum = 0; //앞으로 전진

        if (MobStaynum == 0)
        {
            if (!StageBlock.Inst.Test_St(StageBlock.Inst.player, gameObject, 1))
                MobStaynum = -1;
            else
                StageBlock.Inst.Test_St_Real(StageBlock.Inst.player, gameObject, 1);
        }
    }
    void 소형슬라임_passive()
    {
        knockback = 0;
    }
    void 소형슬라임_Stay()
    {
        if (!DieThisMob)
            GoNum.SetActive(true);

        소형슬라임_skill();
        소형슬라임_passive();

        switch (MobStaynum)
        {
            case -1:
                ColorandImage(5);
                minusbuff.Stop();

                mob_power = 0;
                patten_num = 0;
                break;
            case 0: //전진
                ColorandImage(4);
                minusbuff.Stop();

                mob_power = 0;
                patten_num = 0;
                break;
            case 1: //공4
                minusbuff.Stop();

                mob_power = 4;
                patten_num = 1;

                DamageIcon(mob_power + powerUp);
                break;
            case 2: //공2
                minusbuff.Play();

                mob_power = 2;
                patten_num = 2;

                DamageIcon(mob_power + powerUp);
                break;
        }
    }
    IEnumerator 소형슬라임_Go()
    {
        if (!DieThisMob)
            GoNum.SetActive(false);
        switch (MobStaynum)
        {
            case -1:
                break;
            case 0:
                ClipManager.Inst.Slime_1();
                StageBlock.Inst.Move_mob(gameObject, 1);
                illustOn(false, 1);
                yield return new WaitForSeconds(0.3f);
                illustOn(true, 0);
                break;
            case 1:
                minusbuff.Stop();
                ClipManager.Inst.Slime_3();
                PlayerCharacter.Inst.SetUpdate(this);
                break;
            case 2:
                minusbuff.Stop();
                ClipManager.Inst.Slime_3();
                PlayerCharacter.Inst.weak += 2;
                PlayerCharacter.Inst.SetUpdate(this);
                break;
        }

        yield return new WaitForSeconds(0.7f);
        Finish();
    }
    #endregion
    #region 중형 슬라임
    void 중형슬라임_skill()
    {
        int play_point = Array.IndexOf(StageBlock.Inst.test_situation, GameObject.Find("Character"));
        int mob_point = Array.IndexOf(StageBlock.Inst.test_situation, gameObject);

        int Distance = Mathf.Abs(Mathf.Abs(play_point) - Mathf.Abs(mob_point));

        if (Distance == 2)
        {
            MobStaynum = 2; //공격 5
        }
        else if (Distance == 1)
        {
            MobStaynum = 1; //실드 10
        }
        else
            MobStaynum = 0; //앞으로 전진

        
        if (MobStaynum == 0)
        {
            StageBlock.Inst.Test_St_Real(StageBlock.Inst.player, gameObject, 1);
        }
        else if (MobStaynum == 2)
        {
            if(play_point < mob_point)
            StageBlock.Inst.Test_St_Real(StageBlock.Inst.player, gameObject, 1);
        }

    }

    void 중형슬라임_passive()
    {
        knockback = 0; //모든 공격에 넉백1부여
    }

    void 중형슬라임_Stay()
    {
        if (!DieThisMob)
            GoNum.SetActive(true);

        중형슬라임_skill();
        중형슬라임_passive();

        switch (MobStaynum)
        {
            case -1:
                ColorandImage(5);
                minusbuff.Stop();

                mob_power = 0;
                patten_num = 0;
                break;
            case 0: //전진
                ColorandImage(4);
                minusbuff.Stop();

                mob_power = 0;
                patten_num = 0;
                break;
            case 1: //실드 10
                ColorandImage(0);
                minusbuff.Stop();

                mob_power = 0;
                patten_num = 1;
                break;
            case 2: //공격 5
                minusbuff.Play();

                mob_power = 5;
                patten_num = 2;

                DamageIcon(mob_power + powerUp);
                break;
        }
    }

    IEnumerator 중형슬라임_Go()
    {
        if (!DieThisMob)
            GoNum.SetActive(false);
        switch (MobStaynum)
        {
            case -1:
                break;
            case 0:
                ClipManager.Inst.Slime_1();
                //StageBlock.Inst.Move_mob(gameObject, 1);
                StageBlock.Inst.Knockback_mob(this, null, false, -1);  //앞을 뚫고 이동
                illustOn(false, 1);
                yield return new WaitForSeconds(0.2f);
                illustOn(true, 0);
                break;
            case 1:
                minusbuff.Stop();
                ClipManager.Inst.Slime_3();
                SetShield(10);
                break;
            case 2:
                minusbuff.Stop();
                illustOn(false, 1);
                StageBlock.Inst.Knockback_mob(this, null, false, -1);  //앞을 뚫고 이동
                yield return new WaitForSeconds(0.2f);
                ClipManager.Inst.Slime_3();
                PlayerCharacter.Inst.weak += 2;
                PlayerCharacter.Inst.SetUpdate(this);
                break;
        }

        yield return new WaitForSeconds(0.7f);
        Finish();
    }
    #endregion
    #region 외팔무사
    void 외팔무사_skill()
    {
        int play_point = Array.IndexOf(StageBlock.Inst.test_situation, GameObject.Find("Character"));
        int mob_point = Array.IndexOf(StageBlock.Inst.test_situation, gameObject);

        int Distance = Mathf.Abs(Mathf.Abs(play_point) - Mathf.Abs(mob_point));

        if (Distance == 1)
        {
            MobStaynum = 3; //데미지2 x 3
        }
        else if (Distance <= 2)
        {
            MobStaynum = 2; //데미지10
        }
        else if (Distance <= 5)
        {
            MobStaynum = 1; //데미지1 방어막3 기력-1
        }
        else
            MobStaynum = 0; //앞으로 전진

        if (MobStaynum == 0)
        {
            if (!StageBlock.Inst.Test_St(StageBlock.Inst.player, gameObject, 1))
                MobStaynum = -1;
        }
        else if (MobStaynum == 2)
        {
            if (play_point == 0 || play_point == 6)
                StageBlock.Inst.Test_St_Real(StageBlock.Inst.player, gameObject, play_point, true);
            else
            {
                if(StageBlock.Inst.test_situation[(play_point + 1)] != null)
                    StageBlock.Inst.Test_St_Real(StageBlock.Inst.player, gameObject, play_point, true);
                else
                    StageBlock.Inst.Test_St_Real(StageBlock.Inst.player, gameObject, play_point + 1, false);
            }
        }
    }

    void 외팔무사_passive()
    {
        knockback = 0; //모든 공격에 넉백1부여
    }

    void 외팔무사_Stay()
    {
        if (!DieThisMob)
            GoNum.SetActive(true);

        외팔무사_skill();
        외팔무사_passive();

        switch (MobStaynum)
        {
            case -1:
                ColorandImage(5);
                minusbuff.Stop();
                shiledbuff.Stop();

                mob_power = 0;
                mob_time = 1;
                patten_num = 0;
                break;
            case 0: //전진
                ColorandImage(4);
                minusbuff.Stop();
                shiledbuff.Stop();

                mob_power = 0;
                mob_time = 1;
                patten_num = 0;
                break;
            case 1: //데미지1 방어막3 기력-1
                minusbuff.Play();
                shiledbuff.Play();

                mob_power = 1;
                mob_time = 1;
                patten_num = 3;

                DamageIcon(mob_power + powerUp);
                break;
            case 2: //데미지10
                minusbuff.Stop();
                shiledbuff.Stop();

                mob_power = 10;
                mob_time = 1;
                patten_num = 2;

                DamageIcon(mob_power + powerUp);
                break;
            case 3: //데미지2 x 3
                minusbuff.Stop();
                shiledbuff.Stop();

                mob_power = 2;
                if (powerUp > 0)
                    mob_time = powerUp + 1;
                else
                    mob_time = 1;

                patten_num = 1;

                DamageIcon(mob_power + powerUp);
                break;
        }
    }

    IEnumerator 외팔무사_Go()
    {
        if (!DieThisMob)
            GoNum.SetActive(false);
        switch (MobStaynum)
        {
            case -1:
                //Debug.Log("외팔무사은 어리둥절해하고 있습니다.");
                break;
            case 0:
                //Debug.Log("외팔무사이 1칸 이동합니다.");
                //소리
                StageBlock.Inst.Move_mob(gameObject, 1);
                illustOn(false, 1);
                yield return new WaitForSeconds(0.3f);
                illustOn(true, 0);
                break;
            case 1:
                powerUp++;
                //Debug.Log("외팔무사 1데미지 방어막3 기력-1");
                minusbuff.Stop();
                shiledbuff.Stop();
                //소리
                PlayerCharacter.Inst.SetUpdate(this); //데미지1
                SetShield(3); //방어막3
                StageBlock.Inst.MoveNum(false); //기력-1
                
                break;
            case 2:
                powerUp++;
                //Debug.Log("외팔무사 10의 피해");
                //소리

                int play_point = Array.IndexOf(StageBlock.Inst.situation, GameObject.Find("Character"));

                if (play_point == 0 || play_point == 6)
                    StageBlock.Inst.Knockback_mob(this, null, false, -(Distance_P_M(true)));  //앞을 뚫고 이동
                else
                    StageBlock.Inst.Knockback_mob(this, null, false, -(Distance_P_M(true) + 1));  //앞을 뚫고 이동

                PlayerCharacter.Inst.SetUpdate(this, 0, true, false);
                break;
            case 3:
                //Debug.Log("외팔무사 2 x n);
                //소리
                for (int i = 0; i < mob_time; i++)
                {
                    powerUp++;
                    PlayerCharacter.Inst.SetUpdate(this);
                    yield return new WaitForSeconds(0.2f);
                }
                break;
        }

        yield return new WaitForSeconds(0.7f);
        Finish();
    }
    #endregion
    #region 날개 없는 자
    void 날개없는자_skill()
    {
        int play_point = Array.IndexOf(StageBlock.Inst.test_situation, GameObject.Find("Character"));
        int mob_point = Array.IndexOf(StageBlock.Inst.test_situation, gameObject);

        int Distance = Mathf.Abs(Mathf.Abs(play_point) - Mathf.Abs(mob_point));

        if (Distance == 1)
        {
            MobStaynum = 1; //공격4
        }
        else
            MobStaynum = 0; //앞으로 전진

        if (MobStaynum == 0)
        {
            if (!StageBlock.Inst.Test_St(StageBlock.Inst.player, gameObject, 1))
                MobStaynum = -1;
            else
                StageBlock.Inst.Test_St_Real(StageBlock.Inst.player, gameObject, 1);
        }
    }

    void 날개없는자_passive()
    {
        knockback = 0; //모든 공격에 넉백1부여
    }

    void 날개없는자_Stay()
    {
        if (!DieThisMob)
            GoNum.SetActive(true);

        날개없는자_skill();
        날개없는자_passive();

        switch (MobStaynum)
        {
            case -1:
                ColorandImage(5);
                minusbuff.Stop();
                GoNum_TMP.text = "";
                patten_num = 0;
                break;
            case 0: //전진
                ColorandImage(4);
                minusbuff.Stop();
                GoNum_TMP.text = "";
                patten_num = 0;
                break;
            case 1: //공4
                DamageIcon(4);
                minusbuff.Play();
                patten_num = 1;
                break;
        }
    }

    IEnumerator 날개없는자_Go()
    {
        if (!DieThisMob)
            GoNum.SetActive(false);
        switch (MobStaynum)
        {
            case -1:
                Debug.Log("날개없는자은 어리둥절해하고 있습니다.");
                break;
            case 0:
                Debug.Log("날개없는자이 1칸 이동합니다.");
                ClipManager.Inst.Slime_1();
                StageBlock.Inst.Move_mob(gameObject, 1);
                yield return new WaitForSeconds(0.2f);
                MotionChange(0);
                break;
            case 1:
                Debug.Log("날개없는자이 플레이어에게 4의 피해를 입혔습니다.");
                minusbuff.Stop();
                ClipManager.Inst.Slime_3();
                PlayerCharacter.Inst.weak += 2;
                PlayerCharacter.Inst.SetUpdate(this, 4);
                break;
        }

        yield return new WaitForSeconds(0.7f);
        Finish();
    }
    #endregion
    #region 보스 슬라임
    void 보스슬라임_skill()
    {
        int play_point = Array.IndexOf(StageBlock.Inst.test_situation, GameObject.Find("Character"));
        int mob_point = Array.IndexOf(StageBlock.Inst.test_situation, gameObject);

        int Distance = Mathf.Abs(Mathf.Abs(play_point) - Mathf.Abs(mob_point));

        MobStaynum = 0; //스텍
    }

    void 보스슬라임_passive()
    {
        knockback = 0; //모든 공격에 넉백1부여
    }

    void 보스슬라임_Stay()
    {
        if (!DieThisMob)
            GoNum.SetActive(true);

        보스슬라임_skill();
        보스슬라임_passive();

        switch (MobStaynum)
        {
            case -1:
                ColorandImage(5);
                plusbuff.Stop();

                mob_power = 0;
                patten_num = 0;
                break;
            case 0: //스텍
                ColorandImage(0);

                plusbuff.Play();

                mob_power = 0;
                patten_num = 1;
                break;
        }
    }

    IEnumerator 보스슬라임_Go()
    {
        if (!DieThisMob)
            GoNum.SetActive(false);
        switch (MobStaynum)
        {
            case -1:
                break;
            case 0:
                ClipManager.Inst.Slime_1();
                SetShield(10);
                Slime++;

                illustOn(false, 3);
                yield return new WaitForSeconds(0.8f);
                illustOn(true, 0);
                break;
        }

        yield return new WaitForSeconds(0.7f);
        Finish();
    }
    #endregion
    #region 보스 슬라임2
    void 보스슬라임2_skill()
    {
        Debug.Log("2");
        int play_point = Array.IndexOf(StageBlock.Inst.test_situation, GameObject.Find("Character"));
        int mob_point = Array.IndexOf(StageBlock.Inst.test_situation, gameObject);

        int Distance = Mathf.Abs(Mathf.Abs(play_point) - Mathf.Abs(mob_point));

        if (Distance == 1)
        {
            MobStaynum = 1; //공격 넉백
        }
        else if (Distance <= 2)
        {
            MobStaynum = 2; //공격 달라붙기
        }
        else
            MobStaynum = 0; //앞으로 전진


        if (MobStaynum == 0)
        {
            StageBlock.Inst.Test_St_Real(StageBlock.Inst.player, gameObject, 1);
        }
        else if (MobStaynum == 1)
        {
            Debug.Log("play_point = " + play_point);
            StageBlock.Inst.Test_St_Real(StageBlock.Inst.player, gameObject, play_point-1, false);
            /*
            if(play_point < mob_point)
                StageBlock.Inst.Test_St_Real(StageBlock.Inst.player, gameObject, play_point, true);
            if (play_point > mob_point)
                StageBlock.Inst.Test_St_Real(StageBlock.Inst.player, gameObject, play_point, true);
            */
        }
        else if (MobStaynum == 2)
        {
            StageBlock.Inst.Test_St_Real(gameObject, StageBlock.Inst.player, 2);
        }

    }

    void 보스슬라임2_passive()
    {
        knockback = 0; //모든 공격에 넉백1부여
    }

    void 보스슬라임2_Stay()
    {
        if (!DieThisMob)
            GoNum.SetActive(true);

        보스슬라임2_skill();
        보스슬라임2_passive();

        switch (MobStaynum)
        {
            case -1:
                ColorandImage(5);

                mob_power = 0;
                patten_num = 0;
                knockback = 0;
                break;
            case 0: //전진
                ColorandImage(4);

                mob_power = 0;
                patten_num = 0;
                knockback = 0;
                break;
            case 1: //공격 스택 x 5
                if (Slime == 0)
                    mob_power = 1;
                else
                    mob_power = 5 * Slime;

                patten_num = 1;
                knockback = 2;
                DamageIcon(mob_power + powerUp);
                break;
            case 2: //공격 스택 x 2
                if (Slime == 0)
                    mob_power = 1;
                else
                    mob_power = 2 * Slime;

                patten_num = 2;
                knockback = 0;
                DamageIcon(mob_power + powerUp);
                break;
        }
    }

    IEnumerator 보스슬라임2_Go()
    {
        if (!DieThisMob)
            GoNum.SetActive(false);
        switch (MobStaynum)
        {
            case -1:
                break;
            case 0:
                ClipManager.Inst.Slime_1();
                //StageBlock.Inst.Move_mob(gameObject, 1);
                StageBlock.Inst.Knockback_mob(this, null, false, -1);  //앞을 뚫고 이동
                illustOn(false, 1);
                yield return new WaitForSeconds(0.2f);
                illustOn(true, 0);
                break;
            case 1:
                ClipManager.Inst.Slime_3();
                PlayerCharacter.Inst.SetUpdate(this);
                break;
            case 2:
                illustOn(false, 1);
                StageBlock.Inst.Knockback_mob(this, null, false, -1);  //앞을 뚫고 이동
                yield return new WaitForSeconds(0.2f);
                ClipManager.Inst.Slime_3();
                PlayerCharacter.Inst.SetUpdate(this);
                break;
        }

        yield return new WaitForSeconds(0.7f);
        Finish();
    }
    #endregion
    #region 마무리
    void Finish()
    {
        if (bleed != 0)
        {
            BuffSetUpdate(bleed);
            BattleCam.Inst.DamageText(this, false, bleed, "bleed", Color.red);
        }
    }
    #endregion
    #endregion
}
