using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 
using TMPro;
using DG.Tweening;

public class Card : MonoBehaviour
{
    [SerializeField] ItemSO itemSO;

    [SerializeField] SpriteRenderer card;
    [SerializeField] SpriteRenderer illust;
    [SerializeField] SpriteRenderer[] RangeSprite;

    [SerializeField] TMP_Text name;
    public TMP_Text type;
    public TMP_Text cost;
    public TMP_Text attack;
    public TMP_Text defense;
    [SerializeField] TMP_Text Key;

    [SerializeField] public TMP_Text effect;

    public int range; //근거리인지 원거리인지

    /*
    [SerializeField] Sprite cardFront; //앞면
    [SerializeField] Sprite cardBACK; //뒷면

    bool isFront; //앞면인지 뒷면인지
    */
    public bool used = false;
    public Item item;
    public PRS originPRS;

    #region 카드 사용 이벤트
    public void Destro()
    {
        CardManager.Inst.myCards.Remove(this);
        Destroy(gameObject);
    }
    #endregion

    //public void SetUp(Item item, bool isFront)
    public void SetUp(Item item)
    {
        this.item = item;

        name.text = this.item.name;
        type.text = this.item.type;
        cost.text = this.item.cost.ToString();
        attack.text = this.item.attack.ToString();
        defense.text = this.item.defense.ToString();
        effect.text = this.item.effect;

        range = this.item.range;
        for (int i = 0; i < range; i++)
        {
            if(this.item.type == "참격")
                RangeSprite[i].color = new Color(1f, 200 / 255f, 0/255f, 1f);
            else if (this.item.type == "타격")
                RangeSprite[i].color = new Color(1f, 100/255f, 0f, 1f);
        }

        for (int i = 0; i < itemSO.items.Count; i++)
        {
            if (name.text == itemSO.items[i].name)
            {
                illust.sprite = itemSO.items[i].illust;
            }
        }
    }

    private KeyCode[] 
       keyCodes = {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
       };

    void Update()
    {
        if (item != null)
        {
            int attack_dam = 0;
            int defense_dam = this.item.defense + Player_UseItem.Inst.Out_Set("내구");
            
            if (type.text == "참격")
                attack_dam = this.item.attack + Player_UseItem.Inst.Out_Set("참격");
            if (type.text == "타격")
                attack_dam = this.item.attack + Player_UseItem.Inst.Out_Set("타격");

            if (attack_dam < 0)
                attack_dam = 0;
            if (defense_dam < 0)
                defense_dam = 0;

            if (PlayerCharacter.Inst.weak != 0)
                attack_dam = attack_dam / 2;

            attack.text = attack_dam.ToString();
            defense.text = defense_dam.ToString();

            if (type.text == "소비")
            {
                attack.color = new Color(1f, 1f, 1f, 0f);
                defense.color = new Color(1f, 1f, 1f, 0f);
            }
            else
            {
                if (attack_dam > this.item.attack)
                    attack.color = new Color(0 / 255f, 255 / 255f, 100 / 255f, 1f);
                else if (attack_dam == this.item.attack)
                    attack.color = new Color(255 / 255f, 255 / 255f, 255 / 255f, 1f);
                else if (attack_dam < this.item.attack)
                    attack.color = new Color(255 / 255f, 35 / 255f, 35 / 255f, 1f);

                if (defense_dam > this.item.defense)
                    defense.color = new Color(0 / 255f, 255 / 255f, 100 / 255f, 1f);
                else if (defense_dam == this.item.defense)
                    defense.color = new Color(255 / 255f, 255 / 255f, 255 / 255f, 1f);
                else if (defense_dam < this.item.defense)
                    defense.color = new Color(255 / 255f, 35 / 255f, 35 / 255f, 1f);
            }
            
        }

        #region 키 관련 스크립트
        if (!Player.Inst.playerdata.NumberKey)
            Key.color = new Color(0f, 0f, 0f, 0f);
        else
            Key.color = new Color(1f, 1f, 1f, 1f);

        if (Key.text != "" && Player.Inst.playerdata.NumberKey)
        {
            if (Input.GetKeyDown(keyCodes[int.Parse(Key.text) - 1]))
            {
                CardManager.Inst.selectCard = this;
                CardManager.Inst.CardMouseDown(this);
            }
            if (Input.GetKeyUp(keyCodes[int.Parse(Key.text) - 1]))
            {
                CardManager.Inst.CardMouseUp(this);
                CardManager.Inst.UseMouseUp();
            }
        }
        #endregion
    }

    public void KeyText(int num)
    {
        if (num == 0)
        {
            Key.text = null;
        }
        else
        {
            Key.text = num.ToString();
        }
    }

    void OnMouseOver()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (!CardManager.Inst.isMyCardDrag)
            CardManager.Inst.CardMouseOver(this);
    }

    void OnMouseExit()
    {
        CardManager.Inst.CardMouseExit(this);
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        CardManager.Inst.CardMouseDown(this);
    }

    void OnMouseUp()
    {
        if (!TurnManager.Inst.isLoading)
        {
            CardManager.Inst.CardMouseUp(this);
            CardManager.Inst.UseMouseUp();
        }
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
}
