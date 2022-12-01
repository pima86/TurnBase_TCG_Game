using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Random = UnityEngine.Random;

public class CardManager : MonoBehaviour
{
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] List<Item> Deck;
    [SerializeField] List<Item> UsedCards;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] public List<Card> myCards;
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;
    [SerializeField] ECardState eCardState;

    [SerializeField] TMP_Text DeckNum;
    [SerializeField] TMP_Text UseDeckNum;

    [SerializeField] GameObject CardPointer;

    [SerializeField] List<Item> itemBuffer;
    public Card selectCard;
    public bool numKeyUse; //나중에 옵션값 만들어서 유지할 것
    public bool isMyCardDrag;
    bool onMyCardArea;
    enum ECardState { Nothing, CanMouseOver, CanMouseDrag }


    #region 프리팹 추가
    public Item PopItem()
    {
        if (itemBuffer.Count == 0)
            SetupItemRe();

        SetupItemDraw(); // 덱 숫자 표시

        Item item = itemBuffer[0];
        itemBuffer.RemoveAt(0);

        return item;
    }

    void SetupItemBuffer()
    {
        itemBuffer = new List<Item>(100);


        for (int i = 0; i < Deck.Count; i++)
        {
            Item item = Deck[i];

            for (int j = 0; j < item.percent; j++)
                itemBuffer.Add(item);
        }
    }

    void SetupItemRe()
    {
        itemBuffer = UsedCards;
        UseDeckNum.text = "0";
    }

    void SetupItemDraw()
    {
        int num = -1;
        for (int i = 0; i < itemBuffer.Count; i++)
        {
            int rand = Random.Range(i, itemBuffer.Count);

            num += 1;

            Item temp = itemBuffer[i];
            itemBuffer[i] = itemBuffer[rand];
            itemBuffer[rand] = temp;
        }
        DeckNum.text = num.ToString();
    }

    void Start()
    {
        
        TurnManager.OnAddCard += AddCard;

        Invoke("MyDeckLoad", 0.01f);
    }

    void MyDeckLoad()
    {
        SoundPlayer.Inst.BGM_START("N1");

        switch (Player.Inst.playerdata.ThisDeck)
        {
            case 0: Deck = Player.Inst.playerdata.Decks_box_1; break;
            case 1: Deck = Player.Inst.playerdata.Decks_box_2; break;
            case 2: Deck = Player.Inst.playerdata.Decks_box_3; break;
            case 3: Deck = Player.Inst.playerdata.Decks_box_4; break;
            case 4: Deck = Player.Inst.playerdata.Decks_box_5; break;
            case 5: Deck = Player.Inst.playerdata.Decks_box_6; break;
            case 6: Deck = Player.Inst.playerdata.Decks_box_7; break;
            case 7: Deck = Player.Inst.playerdata.Decks_box_8; break;
        }
        SetupItemBuffer();
    }

    void OnDestroy()
    {
        TurnManager.OnAddCard -= AddCard;
    }

    void Update()
    {
        if (isMyCardDrag)
            CardDrag();
        else if (CardPointer.activeSelf)
        {
            LineEdit.Inst.DownAct();
        }

        DetectCardArea();
        SetECardState();
        NumberKey();
    }

    void AddCard()
    {
        if (myCards.Count < 9)
        {
            var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
            var card = cardObject.GetComponent<Card>();
            card.SetUp(PopItem());
            myCards.Add(card);

            SetOriginOrder();
            CardAlignment(true);

            ClipManager.Inst.CardDrow();
        }
    }

    void SetOriginOrder()
    {
        for (int i = 0; i < myCards.Count; i++)
        {
            myCards[i].GetComponent<Order>().SetOriginOrder(i);
        }
    }

    void CardAlignment(bool tp)
    {
        List<PRS> originCardPRSs = new List<PRS>();
        originCardPRSs = RoundAlignment(myCardLeft, myCardRight, myCards.Count, 0.5f, Vector3.one * 0.6f);
        for (int i = 0; i < myCards.Count; i++)
        {
            myCards[i].originPRS = originCardPRSs[i];
            myCards[i].MoveTransform(myCards[i].originPRS, tp, 0.3f);
        }
    }

    void NumberKey()
    {

        for (int i = 0; i < myCards.Count; i++)
        {
            if (numKeyUse)
                myCards[i].GetComponent<Card>().KeyText(i + 1);
            else
                myCards[i].GetComponent<Card>().KeyText(0);
        }
    }

    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PRS> results = new List<PRS>(objCount);

        switch (objCount)
        {
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.4f, 0.6f }; break;
            case 3: objLerps = new float[] { 0.35f, 0.5f, 0.65f }; break;
            case 4: objLerps = new float[] { 0.3f, 0.4f, 0.5f, 0.6f }; break;
            case 5: objLerps = new float[] { 0.25f, 0.35f, 0.45f, 0.55f, 0.65f }; break;
            case 6: objLerps = new float[] { 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f }; break;
            case 7: objLerps = new float[] { 0.15f, 0.25f, 0.35f, 0.45f, 0.55f, 0.65f, 0.75f }; break;
            case 8: objLerps = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f }; break;
            case 9: objLerps = new float[] { 0.05f, 0.15f, 0.25f, 0.35f, 0.45f, 0.55f, 0.65f, 0.75f, 0.85f }; break;
            default:
                break;
        }

        for (int i = 0; i < objCount; i++)
        {
            var myCardPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
            var myCardRot = Quaternion.identity;

            float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
            curve = height >= 0 ? curve : -curve;
            myCardPos.y += curve;
            myCardRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
            results.Add(new PRS(myCardPos, myCardRot, scale));
        }
        return results;
    }
    #endregion
    public bool isUse = false;
    #region 카드 사용 이벤트
    public void UseMouseUp()
    {
        if (!isUse)
        {
            int layerMask = (-1) - (1 << LayerMask.NameToLayer("UI"));
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider != null && hit.collider.tag != "Untagged")
                {
                    if (eCardState != ECardState.CanMouseDrag || selectCard == null)
                        return;
                    if (hit.transform.gameObject.tag == "Player")
                    {
                        StartCoroutine(ShieldPlay(selectCard));

                        UsedCards.Add(selectCard.item);
                        StartCoroutine(DestroCard(selectCard));
                        selectCard.GetComponent<CardUse>().StartDissolve(20);
                    }
                    if (hit.transform.gameObject.tag == "Monster")
                    {
                        if (!StageBlock.Inst.Card_Range(selectCard))
                            return;

                        isUse = true;

                        StartCoroutine(DamagePlay(selectCard));

                        UsedCards.Add(selectCard.item);
                        StartCoroutine(DestroCard(selectCard));
                        selectCard.GetComponent<CardUse>().StartDissolve(20);
                    }

                    UseDeckNum.text = UsedCards.Count.ToString();
                }
            }
        }
    }

    IEnumerator ShieldPlay(Card card)
    {
        PlayerCharacter.Inst.ManaMove(int.Parse(card.cost.text));

        if (card.effect.text.Contains("다중"))
        {
            int limit = card.effect.text.IndexOf("다중");
            string effect_num = card.effect.text.Substring(limit + 2, 1);

            limit = int.Parse(effect_num);
            for (int i = 0; i < limit; i++)
            {
                PlayerCharacter.Inst.SetShield(int.Parse(card.defense.text));
                yield return new WaitForSeconds(0.2f);
            }
        }
        else if (!card.effect.text.Contains("다중"))
        {
            PlayerCharacter.Inst.SetShield(int.Parse(card.defense.text));
        }

        StartCoroutine(CardEffect(card));
    }

    IEnumerator DamagePlay(Card card)
    {
        int p_point = StageBlock.Inst.Player_back();
        int m_point = StageBlock.Inst.Monster_back(MobManager.Inst.select);

        PlayerCharacter.Inst.ManaMove(card.item.cost);

        #region 관통
        if (card.effect.text.Contains("관통"))
        {
            if (p_point > m_point)
            {
                for (int i = p_point; i >= m_point; i--)
                {
                    if (StageBlock.Inst.situation[i] != null && StageBlock.Inst.situation[i] != StageBlock.Inst.player)
                    {
                        StageBlock.Inst.situation[i].GetComponent<Mob>().SetUpdate(card);
                        yield return new WaitForSeconds(0.2f);
                    }
                }
            }
            else
            {
                for (int i = p_point; i <= m_point; i++)
                {
                    if (StageBlock.Inst.situation[i] != null && StageBlock.Inst.situation[i] != StageBlock.Inst.player)
                    {
                        StageBlock.Inst.situation[i].GetComponent<Mob>().SetUpdate(card);
                        yield return new WaitForSeconds(0.2f);
                    }
                }
            }
        }
        #endregion
        #region 다중
        if (card.effect.text.Contains("다중"))
        {
            int limit = card.effect.text.IndexOf("다중");
            string effect_num = card.effect.text.Substring(limit + 2, 1);
            Mob mob = null;
            if (MobManager.Inst.select != null)
                mob = MobManager.Inst.select;

            limit = int.Parse(effect_num);
            for (int i = 0; i < limit; i++)
            {
                mob.SetUpdate(card);
                yield return new WaitForSeconds(0.2f);
            }
        }
        #endregion

        if (!card.effect.text.Contains("관통") && !card.effect.text.Contains("다중"))
        {
            MobManager.Inst.select.SetUpdate(card);
        }


        StartCoroutine(CardEffect(card));
    }

    IEnumerator CardEffect(Card card)
    {
        if (card.effect.text.Contains("카드"))
        {
            int limit = card.effect.text.IndexOf("카드");
            string effect_num = card.effect.text.Substring(limit + 2, 1);

            limit = int.Parse(effect_num);
            for (int i = 0; i < limit; i++)
            {
                AddCard();
                yield return new WaitForSeconds(0.2f);
            }
        }
        if (card.effect.text.Contains("회복"))
        {
            int limit = card.effect.text.IndexOf("회복");
            string effect_num = card.effect.text.Substring(limit + 2, 2);

            limit = int.Parse(effect_num);
            PlayerCharacter.Inst.Heal(limit);
        }
        if (card.effect.text.Contains("마나"))
        {
            int limit = card.effect.text.IndexOf("마나");
            string effect_num = card.effect.text.Substring(limit + 2, 1);

            limit = int.Parse(effect_num);
            PlayerCharacter.Inst.ManaMove(-limit);
        }
        if (card.effect.text.Contains("이동"))
        {
            int limit = card.effect.text.IndexOf("이동");
            string effect_num = card.effect.text.Substring(limit + 2, 1);

            limit = int.Parse(effect_num);
            int act = 0;

            for (int i = 0; i < PlayerCharacter.Inst.move.Length; i++)
            {
                if (PlayerCharacter.Inst.move[i].activeSelf)
                    act++;
            }

            for (int i = 0; i < act + limit; i++)
            {
                PlayerCharacter.Inst.move[i].SetActive(true);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    IEnumerator DestroCard(Card card)
    {
        EnlargeCard(true, card);
        card.used = true;

        Color color = new Color(0f, 0f, 0f, 0f);
        card.attack.color = color;
        card.defense.color = color;
        card.effect.color = color;

        card.transform.position = new Vector3(0, 0, -8);
        card.transform.rotation = Quaternion.Euler(0, 0, 0);
        myCards.Remove(card);
        CardAlignment(true);
        yield return new WaitForSeconds(0.3f);
        card.Destro();
    }

    #endregion

    #region MyCard

    public void CardMouseOver(Card card)
    {
        if (eCardState == ECardState.Nothing)
            return;
        EnlargeCard(true, card);
    }

    public void CardMouseExit(Card card)
    {
        if(!isMyCardDrag)
            EnlargeCard(false, card);
    }

    Vector3 Pointer;
    public void CardMouseDown(Card card)
    {

        if (eCardState != ECardState.CanMouseDrag)
            return;

        selectCard = card;
        if (selectCard.item.cost > PlayerCharacter.Inst.mananum)
        {
            selectCard = null;
            PlayerCharacter.Inst.TextBox.text = "기력이 부족합니다.";
            return;
        }

        isMyCardDrag = true;
        //ClipManager.Inst.CardDrow();

        StageBlock.Inst.TargetPoint(card);
    }

    public void CharMove(GameObject obj, int num)
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;

        StageBlock.Inst.Move(num);
        obj.GetComponent<FieldOn>().StartPart(false);
    }

    public void StartPart(ParticleSystem part, bool bo)
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;

        if (bo)
            part.Play();
        else
            part.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); ;
    }

    public void CardMouseUp(Card card)
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;
        isMyCardDrag = false;
        EnlargeCard(false, card);
        CardAlignment(false);

        if(MobManager.Inst.select != null)
            MobManager.Inst.select.MouseUp();
        StageBlock.Inst.TargetPoint_Clear();
    }

    void CardDrag()
    {
        if (!onMyCardArea)
        {
            if (!CardPointer.activeSelf)
            {
                Pointer = selectCard.transform.position;
                Pointer.z = -9;
                CardPointer.transform.position = Pointer;

                CardPointer.SetActive(true);
            }

            //selectCard.MoveTransform(new PRS(selectCard.originPRS.pos, Utils.QI, Vector3.one * 0f), false);
        }
    }


    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        int layer = LayerMask.NameToLayer("MyCardArea");
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }

    void EnlargeCard(bool isEnlarge, Card card)
    {
        if (!card.used)
        {
            if (isEnlarge)
            {
                Vector3 enlargePos = new Vector3(card.originPRS.pos.x, -0.5f, card.originPRS.pos.z);
                card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 0.7f), false);
            }
            else
            {
                card.MoveTransform(card.originPRS, false);
            }
        
        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
        }
    }

    void SetECardState()
    {
        if (TurnManager.Inst.isLoading)
            eCardState = ECardState.Nothing;
        else if (!TurnManager.Inst.myTurn)
            eCardState = ECardState.CanMouseOver;
        else if (TurnManager.Inst.myTurn)
            eCardState = ECardState.CanMouseDrag;
    }
    #endregion
}
