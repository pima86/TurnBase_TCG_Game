using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;

public class Player : MonoBehaviour
{
    public static Player Inst { get; private set; }
    void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(Inst);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start() => Load();

    private static readonly string privateKey = "54dasd51sds8a3q5w4fhrt41m3c5xz8a4a2wd1c";

    public PlayerData playerdata;

    public void SaveListToData(string name, List<Item> item)
    {
        int i = DeckSelect.Inst.DeckDropdown.value;

        playerdata.Decks_illust[i].name = name;
        switch (i)
        {
            case 0: playerdata.Decks_box_1 = item; break;
            case 1: playerdata.Decks_box_2 = item; break;
            case 2: playerdata.Decks_box_3 = item; break;
            case 3: playerdata.Decks_box_4 = item; break;
            case 4: playerdata.Decks_box_5 = item; break;
            case 5: playerdata.Decks_box_6 = item; break;
        }

        Save();
    }

    #region Save
    [ContextMenu("To Json Data")]
    public void Save()
    {
        string jsonString = DataToJson(playerdata);
        string encryptString = Encrypt(jsonString);
        SaveFile(encryptString);
    }

    [ContextMenu("From Json Data")]
    public void Load()
    {
        if (LoadFile() == null)
        {
            FirstData();
        }
        else
        {
            playerdata = JsonUtility.FromJson<PlayerData>(LoadFile());
        }
    }

    public string LoadFile()
    {
        //������ �����ϴ������� üũ.
        if (!File.Exists(GetPath()))
        {
            Debug.Log("���̺� ������ �������� ����.");
            return null;
        }

        string encryptData = LoadFile(GetPath());
        string decryptData = Decrypt(encryptData);

        Debug.Log(decryptData);

        return decryptData;
    }

    public void FirstData()
    {
        playerdata.story = "�帴�� ���";

        playerdata.ThisDeck = 0;
        playerdata.ThisItem = 0;

        playerdata.hp = 10;
        playerdata.maxmana = 5;
        playerdata.remana = 1;
        playerdata.move = 1;

        playerdata.Decks_illust[0].name = "�⺻��";
        playerdata.BGMSound = 0.5f;
        playerdata.SFXSound = 0.5f;
        playerdata.NumberKey = false;

        playerdata.storyList[0].content = "0";
        playerdata.storyList[0].clear = false;
        playerdata.storyList.RemoveRange(1, playerdata.storyList.Count-1);
    }

    static string DataToJson(PlayerData sd)
    {
        string jsonData = JsonUtility.ToJson(sd);
        return jsonData;
    }

    //json string�� PlayerData�� ��ȯ
    static PlayerData JsonToData(string jsonData)
    {
        PlayerData sd = JsonUtility.FromJson<PlayerData>(jsonData);
        return sd;
    }

    //json string�� ���Ϸ� ����
    static void SaveFile(string jsonData)
    {
        using (FileStream fs = new FileStream(GetPath(), FileMode.Create, FileAccess.Write))
        {
            //���Ϸ� ������ �� �ְ� ����Ʈȭ
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

            //bytes�� ���빰�� 0 ~ max ���̱��� fs�� ����
            fs.Write(bytes, 0, bytes.Length);
        }
    }

    //���� �ҷ�����
    static string LoadFile(string path)
    {
        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            //������ ����Ʈȭ ���� �� ���� ������ ����
            byte[] bytes = new byte[(int)fs.Length];

            //���Ͻ�Ʈ������ ���� ����Ʈ ����
            fs.Read(bytes, 0, (int)fs.Length);

            //������ ����Ʈ�� json string���� ���ڵ�
            string jsonString = System.Text.Encoding.UTF8.GetString(bytes);
            return jsonString;
        }
    }

    private static string Encrypt(string data)
    {

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
        RijndaelManaged rm = CreateRijndaelManaged();
        ICryptoTransform ct = rm.CreateEncryptor();
        byte[] results = ct.TransformFinalBlock(bytes, 0, bytes.Length);
        return System.Convert.ToBase64String(results, 0, results.Length);

    }

    private static string Decrypt(string data)
    {

        byte[] bytes = System.Convert.FromBase64String(data);
        RijndaelManaged rm = CreateRijndaelManaged();
        ICryptoTransform ct = rm.CreateDecryptor();
        byte[] resultArray = ct.TransformFinalBlock(bytes, 0, bytes.Length);
        return System.Text.Encoding.UTF8.GetString(resultArray);
    }


    private static RijndaelManaged CreateRijndaelManaged()
    {
        byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(privateKey);
        RijndaelManaged result = new RijndaelManaged();

        byte[] newKeysArray = new byte[16];
        System.Array.Copy(keyArray, 0, newKeysArray, 0, 16);

        result.Key = newKeysArray;
        result.Mode = CipherMode.ECB;
        result.Padding = PaddingMode.PKCS7;
        return result;
    }

    static string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, "playerData.abcd");
    }

    public void DeletSaveFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "playerData.abcd");

        File.Delete(path);
    }
    #endregion
}

[System.Serializable]
public class PlayerData
{
    public List<Item> CardCollect; //���� ���� �ִ� ī��
    public List<Acc> ItemCollect; //���� ���� �ִ� ������

    #region ĳ����
    public int hp;
    public int power; //��
    public int ptype1; //����
    public int ptype2; //Ÿ��
    public int htype; //����
    public int maxmana;
    public int remana;
    public int move;
    #endregion

    #region ��� ������
    public int ThisDeck;
    public int ThisItem;
    #endregion

    #region �ɼ� ������
    public bool NumberKey;
    public bool StoryUpDown;
    public float BGMSound;
    public float SFXSound;
    #endregion

    #region ����
    public string story;
    public bool ScreenFull;
    public bool[] tutorial;
    #endregion

    public List<Deck> Decks_illust;
    public List<Story> storyList;

    #region ���� ������
    [Header("������ ���� ����Ʈ")]
    public List<string> box_1;
    public List<string> box_2;
    public List<string> box_3;
    public List<string> box_4;
    public List<string> box_5;
    public List<string> box_6;
    public List<string> box_7;
    public List<string> box_8;
    #endregion
    #region �� ������
    [Header("�� ���� ����Ʈ")]
    public List<Item> Decks_box_1;
    public List<Item> Decks_box_2;
    public List<Item> Decks_box_3;
    public List<Item> Decks_box_4;
    public List<Item> Decks_box_5;
    public List<Item> Decks_box_6;
    public List<Item> Decks_box_7;
    public List<Item> Decks_box_8;
    #endregion
    
}