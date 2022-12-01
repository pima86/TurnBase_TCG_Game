using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class Stat_Drop : MonoBehaviour
{
    [SerializeField] SpriteRenderer icon;
    [SerializeField] TMP_Text percent;
    [SerializeField] GameObject Prefab;

    bool show = false;
    Acc originAcc;
    public void SetUp(Acc acc, int num = 0, bool show = true)
    {
        this.show = show;
        originAcc = acc;

        icon.sprite = acc.icon;
        if (num != 0)
        {
            string temp = num.ToString();
            percent.text = temp + "%";
        }
        if(show)
            gameObject.GetComponent<Order>().SetOriginOrder(10);
    }

    #region 아이템 획득 모션
    Vector3[] m_points = new Vector3[4];
    float m_speed;
    float m_timerMax = 0;
    float m_timerCurrent = 0;
    bool thisDropItem = false;
    public void Init(Transform _startTr, Transform _endTr, float _speed, float _newPointDistanceFromStartTr, float _newPointDistanceFromEndTr)
    {
        thisDropItem = true;

        m_speed = _speed;

        // 끝에 도착할 시간을 랜덤으로 줌.
        m_timerMax = Random.Range(0.8f, 1.0f);

        // 시작 지점.
        m_points[0] = _startTr.position;

        // 시작 지점을 기준으로 랜덤 포인트 지정.
        m_points[1] = _startTr.position +
            (_newPointDistanceFromStartTr * Random.Range(-1.0f, 1.0f) * _startTr.right) + // X (좌, 우 전체)
            (_newPointDistanceFromStartTr * Random.Range(-0.15f, 1.0f) * _startTr.up) + // Y (아래쪽 조금, 위쪽 전체)
            (_newPointDistanceFromStartTr * Random.Range(-1.0f, -0.8f) * _startTr.forward); // Z (뒤 쪽만)

        // 도착 지점을 기준으로 랜덤 포인트 지정.
        m_points[2] = _endTr.position +
            (_newPointDistanceFromEndTr * Random.Range(-1.0f, 1.0f) * _endTr.right) + // X (좌, 우 전체)
            (_newPointDistanceFromEndTr * Random.Range(-1.0f, 1.0f) * _endTr.up) + // Y (위, 아래 전체)
            (_newPointDistanceFromEndTr * Random.Range(0.8f, 1.0f) * _endTr.forward); // Z (앞 쪽만)

        // 도착 지점.
        m_points[3] = _endTr.position;

        transform.position = _startTr.position;
    }

    void Update()
    {
        if (m_timerCurrent > m_timerMax || !thisDropItem)
            return;

        // 경과 시간 계산.
        m_timerCurrent += Time.deltaTime * m_speed;

        // 베지어 곡선으로 X,Y,Z 좌표 얻기.
        transform.position = new Vector3(
            CubicBezierCurve(m_points[0].x, m_points[1].x, m_points[2].x, m_points[3].x),
            CubicBezierCurve(m_points[0].y, m_points[1].y, m_points[2].y, m_points[3].y),
            CubicBezierCurve(m_points[0].z, m_points[1].z, m_points[2].z, m_points[3].z)
        );
    }

    private float CubicBezierCurve(float a, float b, float c, float d)
    {
        // (0~1)의 값에 따라 베지어 곡선 값을 구하기 때문에, 비율에 따른 시간을 구했다.
        float t = m_timerCurrent / m_timerMax; // (현재 경과 시간 / 최대 시간)

        float ab = Mathf.Lerp(a, b, t);
        float bc = Mathf.Lerp(b, c, t);
        float cd = Mathf.Lerp(c, d, t);

        float abbc = Mathf.Lerp(ab, bc, t);
        float bccd = Mathf.Lerp(bc, cd, t);

        return Mathf.Lerp(abbc, bccd, t);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("box"))
        {
            List<AccItem> Obtain_Item = DropItem_Mob.Inst.Obtain_Item;
            int addrass = Obtain_Item.FindIndex(x => x.originAcc.name == originAcc.name);
            if (addrass == -1)
            {
                var accObject = Instantiate(Prefab, new Vector3(0, 0, -14), Utils.QI, GameObject.Find("가방_그리드").transform);
                var drop_item = accObject.GetComponent<AccItem>();

                drop_item.SetUp(originAcc, 1);//SO에서 해당 정보를 가져옵니다.
                DropItem_Mob.Inst.Obtain_Item.Add(drop_item);
            }
            else
            {
                Obtain_Item[addrass].Plus_amount();
            }

            MobManager.Inst.DropItem.Remove(originAcc.name);
            Destroy(this.gameObject);
        }
    }
    #endregion

    #region 사이즈 조정허고 파괴하고 게임오브 내보내고 마우스관리하고
    public void ScaleThis(Vector3 scale)
    {
        transform.localScale = scale;
    }
    public void objDestroy() => Destroy(gameObject);
    public void OnMouseOver()
    {
        if(show)
            Tooltip.Inst.SetUp(originAcc);
    }
    public void OnMouseExit()
    {
        if (show)
            Tooltip.Inst.CloseSet();
    }
    public GameObject ReturnGameObject()
    {
        return gameObject;
    }
    #endregion
}
