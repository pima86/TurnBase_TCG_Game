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

    #region ������ ȹ�� ���
    Vector3[] m_points = new Vector3[4];
    float m_speed;
    float m_timerMax = 0;
    float m_timerCurrent = 0;
    bool thisDropItem = false;
    public void Init(Transform _startTr, Transform _endTr, float _speed, float _newPointDistanceFromStartTr, float _newPointDistanceFromEndTr)
    {
        thisDropItem = true;

        m_speed = _speed;

        // ���� ������ �ð��� �������� ��.
        m_timerMax = Random.Range(0.8f, 1.0f);

        // ���� ����.
        m_points[0] = _startTr.position;

        // ���� ������ �������� ���� ����Ʈ ����.
        m_points[1] = _startTr.position +
            (_newPointDistanceFromStartTr * Random.Range(-1.0f, 1.0f) * _startTr.right) + // X (��, �� ��ü)
            (_newPointDistanceFromStartTr * Random.Range(-0.15f, 1.0f) * _startTr.up) + // Y (�Ʒ��� ����, ���� ��ü)
            (_newPointDistanceFromStartTr * Random.Range(-1.0f, -0.8f) * _startTr.forward); // Z (�� �ʸ�)

        // ���� ������ �������� ���� ����Ʈ ����.
        m_points[2] = _endTr.position +
            (_newPointDistanceFromEndTr * Random.Range(-1.0f, 1.0f) * _endTr.right) + // X (��, �� ��ü)
            (_newPointDistanceFromEndTr * Random.Range(-1.0f, 1.0f) * _endTr.up) + // Y (��, �Ʒ� ��ü)
            (_newPointDistanceFromEndTr * Random.Range(0.8f, 1.0f) * _endTr.forward); // Z (�� �ʸ�)

        // ���� ����.
        m_points[3] = _endTr.position;

        transform.position = _startTr.position;
    }

    void Update()
    {
        if (m_timerCurrent > m_timerMax || !thisDropItem)
            return;

        // ��� �ð� ���.
        m_timerCurrent += Time.deltaTime * m_speed;

        // ������ ����� X,Y,Z ��ǥ ���.
        transform.position = new Vector3(
            CubicBezierCurve(m_points[0].x, m_points[1].x, m_points[2].x, m_points[3].x),
            CubicBezierCurve(m_points[0].y, m_points[1].y, m_points[2].y, m_points[3].y),
            CubicBezierCurve(m_points[0].z, m_points[1].z, m_points[2].z, m_points[3].z)
        );
    }

    private float CubicBezierCurve(float a, float b, float c, float d)
    {
        // (0~1)�� ���� ���� ������ � ���� ���ϱ� ������, ������ ���� �ð��� ���ߴ�.
        float t = m_timerCurrent / m_timerMax; // (���� ��� �ð� / �ִ� �ð�)

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
                var accObject = Instantiate(Prefab, new Vector3(0, 0, -14), Utils.QI, GameObject.Find("����_�׸���").transform);
                var drop_item = accObject.GetComponent<AccItem>();

                drop_item.SetUp(originAcc, 1);//SO���� �ش� ������ �����ɴϴ�.
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

    #region ������ ������� �ı��ϰ� ���ӿ��� �������� ���콺�����ϰ�
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
