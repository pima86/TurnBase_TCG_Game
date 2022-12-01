using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class PlayerCharacter : MonoBehaviour
{
    public static PlayerCharacter Inst { get; private set; }
    void Awake() => Inst = this;

    public ParticleSystem part;

    [SerializeField] GameObject[] buff;//버프이미지들
    [SerializeField] TMP_Text[] buff_text;//버프이미지들

    [SerializeField] TMP_Text Mana;
    public TMP_Text TextBox;
    public GameObject rotate;
    public GameObject HPobj;

    [SerializeField] Transform[] manapoint;

    #region 상태관련
    public int vulnerable = 0;
    public int weak = 0;
    #endregion

    public int Item_HP = 0;
    public int damage = 0;
    public int shield = 0;
    public int manamax = 0;
    public int mananum = 0;

    public GameObject[] move;
    public int movenum;

    Rigidbody2D rb;
    Transform ThisObj;

    void Start()
    {
        Item_HP = Player_UseItem.Inst.Out_Set("체력");
        ThisObj = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();

        movenum = Player_UseItem.Inst.Out_Set("이동 포인트");
        manamax = Player_UseItem.Inst.Out_Set("마나 최대치");
        /*
        if (mananum + Player_UseItem.Inst.Out_Set("마나 재생") > manamax) //마나 재상이 최대치를 넘을 경우
            mananum = manamax;
        else
            mananum = mananum + Player_UseItem.Inst.Out_Set("마나 재생");
        
        Mana.text = manamax.ToString() + "/" + manamax.ToString();
        */
        Invoke("ZeroShield", 0.1f);
    }

    string value = "";
    bool resetText = false;
    float time = 0;
    void Update()
    {
        if (weak != 0)
        {
            buff[1].SetActive(true);
            buff_text[1].text = weak.ToString();
        }
        else
            buff[1].SetActive(false);

        if (TextBox.text != value)
        {
            value = TextBox.text;
            resetText = true;
            time = 0;
        }
        if (resetText)
        {
            time += Time.deltaTime;
            if (time > 3)
            {
                resetText = false;
                TextBox.text = "";
                value = "";
            }
        }
    }

    public void Player_Turn_buf()
    {
        if (vulnerable > 0) vulnerable -= 1;
        if (weak > 0) weak -= 1;
    }

    #region 움직임
    public void MoveForce(bool Direction)
    {
        if (Direction)
            rb.AddForce(new Vector3(500, 0, 0));
        else
            rb.AddForce(new Vector3(-500, 0, 0));
    }
    #region MoveTransform
    
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
    #endregion
    #region 마나
    public void ManaMove(int num)
    {
        mananum -= num;
        Mana.text = mananum.ToString() + "/" + manamax.ToString();
        //mana.MoveTransform(new PRS(manapoint[mananum].position, manapoint[mananum].rotation, Vector3.one), true, 0.05f);
    }
    #endregion
    #region 실드 데미지 관련

    public void Heal(int num)
    {
        if (damage > num)
        {
            BattleCam.Inst.DamageText(null, true, num, "", Color.green);
            damage -= num;
        }
        else
        {
            BattleCam.Inst.DamageText(null, true, damage, "", Color.green);
            damage = 0;
        }

        HPUpdate();
    }

    public void SetShield(int num) //실드를 받을 때
    {
        Debug.Log(num);
        shield += num;
        HPobj.GetComponent<HPUpdate>().ShIeldSetAct_1();
        SetUpdate(null, 0 , false);
    }

    public void ZeroShield()
    {
        if (mananum + Player_UseItem.Inst.Out_Set("마나 재생") > manamax) //마나 재상이 최대치를 넘을 경우
            mananum = manamax;
        else
            mananum = mananum + Player_UseItem.Inst.Out_Set("마나 재생");

        Mana.text = mananum.ToString() + "/" + manamax.ToString();

        for (int i = 0; i < movenum; i++)
        {
            move[i].SetActive(true);
        }

        //mana.MoveTransform(new PRS(manapoint[mananum].position, manapoint[mananum].rotation, Vector3.one), true, 0.5f);
        shield = 0;

        HPUpdate();
        SetUpdate(null, 0, false);
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

    public void SetUpdate(Mob mob, int num = 0, bool isMobdamage = true, bool mobturn = true)
    {
        if (isMobdamage)
            num = mob.Mobdamage;

        if (num == 0)
            ShieldSetting(0);
        else if (!isMobdamage)
        {
            ShieldSetting(num);

            if (Item_HP - damage <= 0)
                StartCoroutine(BattleCam.Inst.mobattmotion(mob, true));
        }
        else
        {
            ShieldSetting(num);

            if (Item_HP - damage <= 0)
                StartCoroutine(BattleCam.Inst.mobattmotion(mob, true));
            else
                StartCoroutine(BattleCam.Inst.mobattmotion(mob, false, mobturn));
        }
    }

    public void HPUpdate()
    {
        HPobj.GetComponent<HPUpdate>().HPDODO(Item_HP - damage, Item_HP);
    }
    #endregion
    #region 파괴
    public void Destro()
    {
        Destroy(gameObject);
        Onclick.Inst.OnClickStory();
    }

    public void DeadHp()
    {
        HPobj.SetActive(false);
    }
    #endregion
    #region 마우스 이벤트
    void OnMouseOver()
    {
        if (CardManager.Inst.isMyCardDrag)
        {
            LineEdit.Inst.MobMouseOver(ThisObj.position);
        }
    }

    void OnMouseExit()
    {
        if (CardManager.Inst.isMyCardDrag)
            LineEdit.Inst.MobMouseExit();
    }
    #endregion
}
