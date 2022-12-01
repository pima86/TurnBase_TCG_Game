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
        //파일이 존재하는지부터 체크.
        if (!File.Exists(GetPath()))
        {
            Debug.Log("세이브 파일이 존재하지 않음.");
            return null;
        }

        string encryptData = LoadFile(GetPath());
        string decryptData = Decrypt(encryptData);

        Debug.Log(decryptData);

        return decryptData;
    }

    public void FirstData()
    {
        playerdata.story = "흐릿한 기억";

        playerdata.ThisDeck = 0;
        playerdata.ThisItem = 0;

        playerdata.hp = 10;
        playerdata.maxmana = 5;
        playerdata.remana = 1;
        playerdata.move = 1;

        playerdata.Decks_illust[0].name = "기본덱";
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

    //json string을 PlayerData로 변환
    static PlayerData JsonToData(string jsonData)
    {
        PlayerData sd = JsonUtility.FromJson<PlayerData>(jsonData);
        return sd;
    }

    //json string을 파일로 저장
    static void SaveFile(string jsonData)
    {
        using (FileStream fs = new FileStream(GetPath(), FileMode.Create, FileAccess.Write))
        {
            //파일로 저장할 수 있게 바이트화
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

            //bytes의 내용물을 0 ~ max 길이까지 fs에 복사
            fs.Write(bytes, 0, bytes.Length);
        }
    }

    //파일 불러오기
    static string LoadFile(string path)
    {
        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            //파일을 바이트화 했을 때 담을 변수를 제작
            byte[] bytes = new byte[(int)fs.Length];

            //파일스트림으로 부터 바이트 추출
            fs.Read(bytes, 0, (int)fs.Length);

            //추출한 바이트를 json string으로 인코딩
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
    public List<Item> CardCollect; //내가 갖고 있는 카드
    public List<Acc> ItemCollect; //내가 갖고 있는 아이템

    #region 캐릭터
    public int hp;
    public int power; //힘
    public int ptype1; //참격
    public int ptype2; //타격
    public int htype; //내구
    public int maxmana;
    public int remana;
    public int move;
    #endregion

    #region 잠깐씩 보관용
    public int ThisDeck;
    public int ThisItem;
    #endregion

    #region 옵션 데이터
    public bool NumberKey;
    public bool StoryUpDown;
    public float BGMSound;
    public float SFXSound;
    #endregion

    #region 진행
    public string story;
    public bool ScreenFull;
    public bool[] tutorial;
    #endregion

    public List<Deck> Decks_illust;
    public List<Story> storyList;

    #region 착용 아이템
    [Header("아이템 세팅 리스트")]
    public List<string> box_1;
    public List<string> box_2;
    public List<string> box_3;
    public List<string> box_4;
    public List<string> box_5;
    public List<string> box_6;
    public List<string> box_7;
    public List<string> box_8;
    #endregion
    #region 덱 레시피
    [Header("덱 세팅 리스트")]
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