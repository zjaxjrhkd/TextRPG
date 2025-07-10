using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Threading;
using System.Xml.Linq;
using System.Text.Json;

class GameManager
{
    public static GameManager Instance { get; private set; }

    private CreateManager createManager; 
    public CreateManager CreateManager
    {
        get { return createManager; }
    }
    private TextManager textManager;
    public TextManager TextManager
    {
        get { return textManager; }
    }
    private InputManager inputManager;
    public InputManager InputManager
    {
        get { return inputManager; }
    }
    private ConditionManager conditionManager;
    public ConditionManager ConditionManager
    {
        get { return conditionManager; }
    }
    private BattleManager battleManager;
    public BattleManager BattleManager
    {
        get { return battleManager; }
    }
    private DungeonManager dungeonManager;
    public DungeonManager DungeonManager
    {
        get { return dungeonManager; }
    }

    public GameManager()
    {
        Instance = this;
        createManager = new CreateManager();
        textManager = new TextManager();
        inputManager = new InputManager();
        conditionManager = new ConditionManager();
        battleManager = new BattleManager();
        dungeonManager = new DungeonManager();
    }
    public void CreatePlayer()
    {
        createManager.CreatePlayer();
    }
    public void CreateMonster()
    {
        createManager.CreateMonster();
        Console.WriteLine("몬스터 생성 완료.");
    }
    public void CreateTown()
    {
        createManager.CreateTown();
        Console.WriteLine("마을 생성 완료.");
    }
    public void CreateItem()
    {
        createManager.CreteItem();
        Console.WriteLine($"아이템이 생성되었습니다.");
    }
    public void InitializeGame()
    {
        Console.WriteLine("게임 초기 데이터를 세팅합니다.");
        CreatePlayer();
        CreateMonster();
        CreateItem();
        CreateTown();
        Console.WriteLine("게임 세팅 완료.");
    }
    public void StartGame()
    {
        Console.WriteLine("게임을 시작합니다.");
        InitializeGame();
        GameManager.Instance.TextManager.ViewText($"=================================================================================");

        PresentItem();
        ConditionManager.Action();
    }
    public void PresentItem()
    {
        if (CreateManager.Player.Item == null)
        {
            TextManager.ViewText($"스파르타 마을에 오신걸 환영합니다! {GameManager.Instance.CreateManager.Player.Name}님!.");
            GameManager.Instance.TextManager.ViewText("기본적인 아이템을 가방에 넣어두었으니 확인해보세요.");
            CreateManager.Player.AddItem(CreateManager.ItemDatabase.ItemDict["설명서"]);
            CreateManager.Player.AddItem(CreateManager.ItemDatabase.ItemDict["나무검"]);
            CreateManager.Player.AddItem(CreateManager.ItemDatabase.ItemDict["가죽갑옷"]);
            CreateManager.Player.AddItem(CreateManager.ItemDatabase.ItemDict["반지"]);
            return;
        }
        TextManager.ViewText($"환영합니다, {GameManager.Instance.CreateManager.Player.Name}님!.");
    }

    public void SaveGame()
    {
        Console.WriteLine("게임을 저장합니다.");

        PlayerData playerData = new PlayerData
        {
            Id = CreateManager.Player.Id,
            Name = CreateManager.Player.Name,
            MaxHp = CreateManager.Player.MaxHp,
            Hp = CreateManager.Player.Hp,
            MaxMp = CreateManager.Player.MaxMp,
            Mp = CreateManager.Player.Mp,
            Str = CreateManager.Player.Str,
            Int = CreateManager.Player.Int,
            ArmorPoint = CreateManager.Player.ArmorPoint,
            Weapon = CreateManager.Player.Weapon,
            Armor = CreateManager.Player.Armor,
            Accessory = CreateManager.Player.Accessory,
            DropItem = CreateManager.Player.DropItem,
            Lv = CreateManager.Player.Lv,
            Exp = CreateManager.Player.Exp,
            NextExp = CreateManager.Player.NextExp,
            Gold = CreateManager.Player.Gold,
            Item = CreateManager.Player.Item,
            plusStr = CreateManager.Player.plusStr,
            plusInt = CreateManager.Player.plusInt,
            plusArmorPoint = CreateManager.Player.plusArmorPoint,
            plusHP = CreateManager.Player.plusHP,
            plusMP = CreateManager.Player.plusMP,
            CurrentLocation = CreateManager.Player.CurrentLocation,
            QuestProgress = CreateManager.Player.QuestProgress
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(playerData, options);
        File.WriteAllText("player.json", json);
        Console.WriteLine("게임이 저장되었습니다.");
    }

    public PlayerData LoadGame()
    {
        if (!File.Exists("player.json"))
        {
            Console.WriteLine("저장된 파일이 없습니다.");
            return null;
        }

        try
        {
            string json = File.ReadAllText("player.json");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            PlayerData loadedPlayer = JsonSerializer.Deserialize<PlayerData>(json, options);
            Console.WriteLine("게임 데이터가 성공적으로 불러와졌습니다.");
            return loadedPlayer;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"불러오기 중 오류가 발생했습니다: {ex.Message}");
            return null;
        }
    }


    public void EndGame()
    {
        Console.WriteLine("게임을 종료합니다. 감사합니다!");
        Environment.Exit(0);
    }
}
class CreateManager
{

    private MonsterDatabase monsterDatabase;
    public MonsterDatabase MonsterDatabase
    {
        get { return monsterDatabase; }
    }

    private BuildingDatabase buildingDatabase;
    public BuildingDatabase BuildingDatabase
    {
        get { return buildingDatabase; }
    }
    private ItemDatabase itemDatabase;
    public ItemDatabase ItemDatabase
    {
        get { return itemDatabase; }
    }

    private StatData _player;
    public StatData Player
    {
        get { return _player; }
    }

    public CreateManager()
    {
        monsterDatabase = new MonsterDatabase();
        buildingDatabase = new BuildingDatabase();
        itemDatabase = new ItemDatabase();
    }

    public void CreatePlayer()
    {
        Console.WriteLine("플레이어 생성");

        if (File.Exists("player.json"))
        {
            Console.WriteLine("저장된 플레이어가 있습니다. 불러오시겠습니까? (1:예/2:아니오)");
            string input = Console.ReadLine();
            if (input == "1")
            {
                PlayerData loadedPlayer = GameManager.Instance.LoadGame();

                if (loadedPlayer != null)
                {
                    _player = new StatData(
                        loadedPlayer.Id,
                        loadedPlayer.Name,
                        loadedPlayer.MaxHp,
                        loadedPlayer.Hp,
                        loadedPlayer.MaxMp,
                        loadedPlayer.Mp,
                        loadedPlayer.Str,
                        loadedPlayer.Int,
                        loadedPlayer.ArmorPoint,
                        loadedPlayer.Weapon,
                        loadedPlayer.Armor,
                        loadedPlayer.Accessory,
                        loadedPlayer.DropItem,
                        loadedPlayer.Lv,
                        loadedPlayer.Exp,
                        loadedPlayer.NextExp,
                        loadedPlayer.Gold,
                        loadedPlayer.Item,
                        loadedPlayer.plusStr,
                        loadedPlayer.plusInt,
                        loadedPlayer.plusArmorPoint,
                        loadedPlayer.plusHP,
                        loadedPlayer.plusMP,
                        loadedPlayer.QuestProgress
                    );

                    Console.WriteLine($"{_player.Name} 플레이어가 불러와졌습니다.");
                    _player.ShowPlayerStat();
                    return;
                }
            }
        }
        Console.WriteLine("새로 생성할 플레이어 이름을 입력하세요:");
        string playerName = Console.ReadLine();
        // 새 플레이어 생성
        _player = new StatData(
            0, playerName, 100, 100, 50, 50, 20, 10, 1,
            "없음", "없음", "없음", "없음",
            1, 0, 100, 50,
            "마을", "초보"
        );
        Console.WriteLine($"{playerName} 플레이어가 생성되었습니다.");
        _player.ShowPlayerStat();
    }

    public void CreateMonster()
    {
        monsterDatabase.Create(GameManager.Instance.CreateManager.Player.QuestProgress);
        Console.WriteLine("몬스터 생성");
    }
    public void CreateTown()
    {
        buildingDatabase.Create();
        Console.WriteLine("마을 생성");
    }
    public void CreteItem()
    {
        itemDatabase.Create()            ;
        Console.WriteLine("아이템 생성");
    }
}
class MonsterDatabase
{
    private Dictionary<string, StatData> monsterDict;
    public Dictionary<string, StatData> MonsterDict
    {
        get { return monsterDict; }
    }

    StatData 토끼 = new StatData(
    0, "토끼", 10, 10, 0, 0, 2, 1, 1, "없음", "없음", "없음", "토끼고기", 1, 0, 5, 2, "초보"
    );
    StatData 슬라임 = new StatData(
    1, "슬라임", 50, 50, 0, 0, 10, 2, 1, "없음", "없음", "없음", "슬라임젤리", 2, 0, 10, 5, "던전입구"
    );
    StatData 고블린 = new StatData(
        2, "고블린", 100, 100, 0, 0, 15, 3, 2, "나무몽둥이", "천옷", "없음", "고블린귀", 3, 0, 20, 10, "던전"
    );
    StatData 고블린주술사 = new StatData(
        3, "고블린주술사", 150, 150, 20, 20, 25, 40, 2, "나무지팡이", "천옷", "없음", "나무지팡이", 5, 0, 20, 10, "던전 깊은곳"
    );

    public MonsterDatabase()
    {
        monsterDict = new Dictionary<string, StatData>();
    }

    public void Create(string map)
    {
        //int_id, string_name, int _hp, int _mp int _str, int _int, int _ArmorPoint,
        //string _Weapon, string _Armor, string _accessory, string _dropItem,
        //int _lv, int _exp, int _nextExp, int _gold, string _currentLocation

        monsterDict.Add("토끼", 토끼);
        monsterDict.Add("슬라임", 슬라임);
        monsterDict.Add("고블린", 고블린);
        monsterDict.Add("고블린주술사", 고블린주술사);
        Console.WriteLine("몬스터가 생성되었습니다.");
    }
    public void RemoveMonster()
    {
        monsterDict.Clear();
    }
}
public class StatData
{
    private int _id;
    private string _name;
    private int _maxHp;
    private int _hp;
    private int _maxMp;
    private int _mp;
    private int _str;
    private int _int;
    private int _armorPoint;
    private string _weapon;
    private string _armor;
    private string _accessory;
    private string _dropItem;
    private int _lv;
    private int _exp;
    private int _nextExp;
    private int _gold;
    private string _currentLocation;
    private List<ItemData> _item;

    public int Id => _id;
    public string Name => _name;
    public int MaxHp => _maxHp;
    public int Hp => _hp;
    public int MaxMp => _maxMp;
    public int Mp => _mp;
    public int Str => _str;
    public int Int => _int;
    public int ArmorPoint => _armorPoint;
    public string Weapon => _weapon;
    public string Armor => _armor;
    public string Accessory => _accessory;
    public string DropItem => _dropItem;
    public int Lv => _lv;
    public int Exp => _exp;
    public int NextExp => _nextExp;
    public int Gold => _gold;
    public int plusStr;
    public int plusInt;
    public int plusArmorPoint;
    public int plusHP;
    public int plusMP;
    public string CurrentLocation => _currentLocation;
    public List<ItemData> Item
    {
        get { return _item; }
        set { _item = value; }
    }
    private string _questProgress;
    public string QuestProgress
    {
        get { return _questProgress; }
        set { _questProgress = value; }
    }

    //로드용 생성자 오버로드
    public StatData(int id, string name, int maxHp, int hp, int maxMp, int mp, int str,
    int int_, int armorPoint, string weapon, string armor,
    string accessory, string dropItem, int lv, int exp, int nextExp,
    int gold, List<ItemData> item, int PlusStr, int PlusInt, int PlusArmorPoint, int PlusHP, int PlusMP, string QuestProgress)
    {
        _id = id;
        _name = name;
        _maxHp = maxHp;
        _hp = hp;
        _maxMp = maxMp;
        _mp = mp;
        _str = str;
        _int = int_;
        _armorPoint = armorPoint;
        _weapon = weapon;
        _armor = armor;
        _accessory = accessory;
        _dropItem = dropItem;
        _lv = lv;
        _exp = exp;
        _nextExp = nextExp;
        _gold = gold;
        _currentLocation = "마을";
        _item = item;
        plusStr = PlusStr;
        plusInt = PlusInt;
        plusArmorPoint = PlusArmorPoint;
        plusHP = PlusHP;
        plusMP = PlusMP;
        _questProgress = QuestProgress;
    }

    public StatData(int id, string name, int maxHp, int hp, int maxMp, int mp, int str, int int_, int armorPoint, string weapon, string armor, string accessory, string dropItem, int lv, int exp, int nextExp, int gold, string currentLocation, string questProgress)
    {
        _id = id;
        _name = name;
        _maxHp = maxHp;
        _hp = hp;
        _maxMp = maxMp;
        _mp = mp;
        _str = str;
        _int = int_;
        _armorPoint = armorPoint;
        _weapon = weapon;
        _armor = armor;
        _accessory = accessory;
        _dropItem = dropItem;
        _lv = lv;
        _exp = exp;
        _nextExp = nextExp;
        _gold = gold;
        _currentLocation = currentLocation;
        _questProgress = questProgress;
    }
    public StatData(int id, string name, int maxHp, int hp,int maxMp, int mp, int str, int int_, int armorPoint, string weapon, string armor, string accessory, string dropItem, int lv, int exp, int nextExp, int gold, string currentLocation)
    {
        _id = id;
        _name = name;
        _maxHp = maxHp;
        _hp = hp;
        _maxMp = maxMp;
        _mp = mp;
        _str = str;
        _int = int_;
        _armorPoint = armorPoint;
        _weapon = weapon;
        _armor = armor;
        _accessory = accessory;
        _dropItem = dropItem;
        _lv = lv;
        _exp = exp;
        _nextExp = nextExp;
        _gold = gold;
        _currentLocation = currentLocation;
    }
    public StatData(StatData other)
    {
        _id = other._id;
        _name = other._name;
        _maxHp = other._maxHp;
        _hp = other._hp;
        _maxMp = other._maxMp;
        _mp = other._mp;
        _str = other._str;
        _int = other._int;
        _armorPoint = other._armorPoint;
        _weapon = other._weapon;
        _armor = other._armor;
        _accessory = other._accessory;
        _dropItem = other._dropItem;
        _lv = other._lv;
        _exp = other._exp;
        _nextExp = other._nextExp;
        _gold = other._gold;
        _currentLocation = other._currentLocation;
        _questProgress = other._questProgress;
             if (other._item != null)
            _item = new List<ItemData>(other._item);
        else
            _item = null;
        plusStr = other.plusStr;
        plusInt = other.plusInt;
        plusArmorPoint = other.plusArmorPoint;
        plusHP = other.plusHP;
        plusMP = other.plusMP;
    }

    public void ShowPlayerStat()
    {
        Console.WriteLine($"====================================캐릭터 스텟=================================== ");
        Console.WriteLine($"이름: {_name}");
        Console.WriteLine($"레벨: {_lv}");
        Console.WriteLine($"경험치: {_exp} / {_nextExp}");
        Console.WriteLine($"HP: {_hp} / {_maxHp}  + {plusHP}");
        Console.WriteLine($"MP: {_mp} / {_maxMp} + {plusMP}");
        Console.WriteLine($"STR: {_str} / + {plusStr}");
        Console.WriteLine($"INT: {_int} / + {plusInt}");
        Console.WriteLine($"방어력: {_armorPoint} / + {plusArmorPoint}");
        Console.WriteLine($"무기: {_weapon}");
        Console.WriteLine($"방어구: {_armor}");
        Console.WriteLine($"장신구: {_accessory}");
        Console.WriteLine($"소지금: {_gold}");
        Console.WriteLine($"현재위치: {_currentLocation}");
        Console.WriteLine($"=================================================================================");
    }

    public void ShowMosterStat()
    {
        Console.WriteLine($"====================================몬스터 스텟=================================== ");
        Console.WriteLine($"이름: {_name}");
        Console.WriteLine($"HP: {_hp}");
        Console.WriteLine($"MP: {_mp}");
        Console.WriteLine($"STR: {_str}");
        Console.WriteLine($"INT: {_int}");
        Console.WriteLine($"방어력: {_armorPoint}");
        Console.WriteLine($"무기: {_weapon}");
        Console.WriteLine($"방어구: {_armor}");
        Console.WriteLine($"장신구: {_accessory}");
        Console.WriteLine($"드랍 아이템: {_dropItem}");
        Console.WriteLine($"레벨: {_lv}");
        Console.WriteLine($"골드: {_gold}");
        Console.WriteLine($"=================================================================================");
    }
    public void STRUP(int value)
    {
        _str += value;
        plusStr += value;
    }
    public void INTUP(int value)
    {
        _int += value;
        plusInt += value;
    }
    public void ArmorUP(int value)
    {
        _armorPoint += value;
        plusArmorPoint += value;
    }
    public void MaxHPUP(int value)
    {
        _maxHp += value;
        _hp += value; 
        plusHP += value; 
    }
    public void MaxMPUP(int value)
    {
        _maxMp += value;
        _mp += value; 
        plusMP += value;
    }
    public void Use(ItemData item)
    {
        if (item.Type == ItemData.ItemType.Weapon || item.Type == ItemData.ItemType.Armor || item.Type == ItemData.ItemType.Accessory || item.Type == ItemData.ItemType.Staff)
        {
            GameManager.Instance.TextManager.ViewText("아이템을 장비합니다.");
            if (item.Type == ItemData.ItemType.Weapon)
            {
                if (_weapon != "없음")
                {
                    GameManager.Instance.TextManager.ViewText("이미 무기를 장착하고 있습니다. 먼저 해제해주세요.");
                    ItemData currentWeapon = GameManager.Instance.CreateManager.ItemDatabase.ItemDict[_weapon];
                    Unequip(currentWeapon);
                }
                GameManager.Instance.TextManager.ViewText($"{item.Name}을 장비했습니다.");
                _weapon = item.Name;
                STRUP(item.Value);
            }
            else if (item.Type == ItemData.ItemType.Armor)
            {
                if (_armor != "없음")
                {
                    GameManager.Instance.TextManager.ViewText("이미 갑옷 장착하고 있습니다. 먼저 해제해주세요.");
                    ItemData currentArmor = GameManager.Instance.CreateManager.ItemDatabase.ItemDict[_armor];
                    Unequip(currentArmor);
                }
                GameManager.Instance.TextManager.ViewText($"{item.Name}을 장비했습니다.");
                _armor = item.Name;
                ArmorUP(item.Value);
            }
            else if (item.Type == ItemData.ItemType.Accessory)
            {
                if (_accessory != "없음")
                {
                    GameManager.Instance.TextManager.ViewText("이미 악세사리를 장착하고 있습니다. 먼저 해제해주세요.");
                    ItemData currentAccessory = GameManager.Instance.CreateManager.ItemDatabase.ItemDict[_accessory];
                    Unequip(currentAccessory);
                }
                GameManager.Instance.TextManager.ViewText($"{item.Name}을 장비했습니다.");

                _accessory = item.Name;
                MaxHPUP(item.Value);
            }
            else if (item.Type == ItemData.ItemType.Staff)
            {
                GameManager.Instance.TextManager.ViewText($"{item.Name}을 장비했습니다.");
                _weapon = item.Name;
                INTUP(item.Value);
                MaxMPUP(item.Value);
            }
        }
        else if (item.Type == ItemData.ItemType.QuestItem)
        {
            GameManager.Instance.TextManager.ViewText("퀘스트 아이템은 사용 할 수 없습니다. 남는 퀘스트 아이템은 상점에 판매하세요");
            return;
        }
        else if (item.Type == ItemData.ItemType.Consumable)
        {
            GameManager.Instance.TextManager.ViewText("아이템을 사용합니다.");
            if (item.Name == "슬라임젤리")
            {
                TakeDamage(-item.Value);
                RemoveItem(item.Name);
            }
            else if (item.Name == "설명서")
            {
                if (GameManager.Instance.CreateManager.Player.QuestProgress == "초보")
                {
                    GameManager.Instance.TextManager.ViewText("우리는 던전 깊속한 곳에 있는 고블린주술사를 없애기 위해 이 곳에 왔습니다.");
                    GameManager.Instance.TextManager.ViewText("하지만 지금의 나는 주술사를 없애기에는 너무 약합니다.");
                    GameManager.Instance.TextManager.ViewText("가방에 있는 아이템을 착용한뒤 길드라는 곳에서 조언을 들어봅시다.");
                }
                else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전입구")
                {
                    GameManager.Instance.TextManager.ViewText("슬라임을 죽이면 슬라임 젤리가 나옵니다.");
                    GameManager.Instance.TextManager.ViewText("젤리를 사용하면 상처를 회복할 수 있습니다.");
                }
                else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전")
                {
                    GameManager.Instance.TextManager.ViewText("고블린을 처치하면 고블린 귀가 나옵니다.");
                    GameManager.Instance.TextManager.ViewText("고블린 귀를 가져가면 길드에서 퀘스트를 완료할 수 있습니다.");
                }
                else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전깊은곳")
                {
                    GameManager.Instance.TextManager.ViewText("고블린주술사는 굉장히 강합니다.");
                    GameManager.Instance.TextManager.ViewText("MP가 남아있다면 마법공격을 사용합니다.");
                    GameManager.Instance.TextManager.ViewText("마법공격은 방어력을 무시하니 주의해주세요.");
                    GameManager.Instance.TextManager.ViewText("주술사를 처치하기 전에 철로 만든 장비를 구매합시다.");
                }
                else if(GameManager.Instance.CreateManager.Player.QuestProgress == "모든퀘스트완료")
                {
                    GameManager.Instance.TextManager.ViewText("모든 퀘스트를 완료하였습니다. 수고하셨습니다!");
                    GameManager.Instance.TextManager.ViewText("게임을 끄셔도 좋지만 다시 던전을 가보셔도 좋습니다.");
                    GameManager.Instance.TextManager.ViewText("이제 던전에는 모든 종류의 몬스터들이 나옵니다.");
                    GameManager.Instance.TextManager.ViewText("다양한 몬스터를 잡고 무한 레벨업을 해보세요.");
                }
            }
            else
            {
                Console.WriteLine("알 수 없는 소비 아이템입니다.");
                return;
            }
        }
        else
        {
            Console.WriteLine("장착할 수 없는 아이템입니다.");
        }
        if (item.Name != "설명서")
        {
            RemoveItem(item.Name);
        }
        return;
    }
    public void Unequip(ItemData item)
    {
        GameManager.Instance.TextManager.ViewText("아이템을 해제합니다.");

        if (_item == null)
            _item = new List<ItemData>();

        if (_item.Count >= 10)
        {
            GameManager.Instance.TextManager.ViewText("인벤토리를 1칸 여유를 두고 해제해주세요.");
            return;
        }

        // 무기 해제
        if (item.Type == ItemData.ItemType.Weapon && _weapon == item.Name)
        {
            AddItem(item);
            STRUP(-item.Value);
            _weapon = "없음";
            Console.WriteLine("무기를 해제했습니다.");
        }
        // 지팡이 해제
        else if (item.Type == ItemData.ItemType.Staff && _weapon == item.Name)
        {
            AddItem(item);
            INTUP(-item.Value);
            MaxMPUP(-item.Value);
            _weapon = "없음";
            Console.WriteLine("지팡이를 해제했습니다.");
        }
        // 방어구 해제
        else if (item.Type == ItemData.ItemType.Armor && _armor == item.Name)
        {
            AddItem(item);
            ArmorUP(-item.Value);
            _armor = "없음";
            Console.WriteLine("방어구를 해제했습니다.");
        }
        // 장신구 해제
        else if (item.Type == ItemData.ItemType.Accessory && _accessory == item.Name)
        {
            AddItem(item);
            MaxHPUP(-item.Value);
            _accessory = "없음";
            Console.WriteLine("장신구를 해제했습니다.");
        }
        else
        {
            GameManager.Instance.TextManager.ViewText("해제할 아이템이 없습니다.");
        }
        return;
    }

    public void AddItem(ItemData item)
    {
        if (_item == null)
            _item = new List<ItemData>();

        if (_item.Count >= 10)
        {
            Console.WriteLine("아이템 인벤토리가 가득 찼습니다.");
            return;
        }

        _item.Add(item);
        Console.WriteLine($"{item.Name} 아이템을 획득했습니다.");
    }

    public void RemoveItem(string itemName)
    {
        if (_item == null || _item.Count == 0)
        {
            Console.WriteLine("인벤토리에 아이템이 없습니다.");
            return;
        }

        for (int i = 0; i < _item.Count; i++)
        {
            if (_item[i] != null && _item[i].Name == itemName)
            {
                _item.RemoveAt(i);
                return;
            }
        }
        Console.WriteLine($"{itemName} 아이템이 인벤토리에 없습니다.");
    }

    public void TakeDamage(int damage)
    {
        if (damage < 0)
        {
            if (_hp + damage > _maxHp)
            {
                damage = _maxHp - _hp; // 최대 HP를 초과하지 않도록 조정
            }
            _hp -= damage;
            Console.WriteLine($"{_name}은(는) 회복했습니다. 회복량: {-damage}");
            return;
        }
        else
        {
            _hp -= damage;
            Console.WriteLine($"{_name}은(는) 피해를 입었습니다. 피해량: {damage}");
            return;
        }
    }
    public void Death()
    {
        if (_hp <= 0)
        {
            Console.WriteLine($"{_name} has died.");
        }
    }
    public void TakeExp(int exp)
    {
        _exp += exp;
        Console.WriteLine($"{_name} gained {exp} experience. Total EXP: {_exp}");
        if (_exp >= _nextExp)
        {
            LevelUp();
        }
    }
    public void LevelUp()
    {
        GameManager.Instance.TextManager.ViewText($"**********************************************************************");

        GameManager.Instance.TextManager.ViewText($"{_name}이(가) 레벨업했습니다!");
        _lv++;
        _nextExp += 100;
        PlusStat();
        ShowPlayerStat();
        Console.WriteLine($"{_name} leveled up to level {_lv}! Next EXP: {_nextExp}");
        GameManager.Instance.TextManager.ViewText($"**********************************************************************");
    }
    public void PlusStat()
    {
        GameManager.Instance.TextManager.ViewText("레벨업을 하여 5개의 스텟을 올립니다.");
        for(int i = 0; i<5; i++)
        {
            GameManager.Instance.TextManager.ViewText("올릴 스텟을 선택하세요: 1. STR+5, 2. INT+5, 3. 방어력+1, 4. HP+10, 5. MP+10");
            GameManager.Instance.InputManager.inputData = GameManager.Instance.InputManager.InputString();

            if( GameManager.Instance.InputManager.inputData == "1")
            {
                _str += 5;
                GameManager.Instance.TextManager.ViewText($"STR이 5 증가했습니다. 현재 STR: {_str}");
            }
            else if (GameManager.Instance.InputManager.inputData == "2")
            {
                _int += 5;
                GameManager.Instance.TextManager.ViewText($"INT가 5 증가했습니다. 현재 INT: {_int}");
            }
            else if (GameManager.Instance.InputManager.inputData == "3")
            {
                _armorPoint += 1;
                GameManager.Instance.TextManager.ViewText($"방어력이 1 증가했습니다. 현재 방어력: {_armorPoint}");
            }
            else if (GameManager.Instance.InputManager.inputData == "4")
            {
                _hp += 10;
                GameManager.Instance.TextManager.ViewText($"HP가 10 증가했습니다. 현재 HP: {_hp}/{_maxHp}");
            }
            else if (GameManager.Instance.InputManager.inputData == "5")
            {
                _mp += 10;
                GameManager.Instance.TextManager.ViewText($"MP가 10 증가했습니다. 현재 MP: {_mp}/{_maxMp}");
            }
            else
            {
                GameManager.Instance.TextManager.ViewText("잘못된 입력입니다. 다시 시도하세요.");
            }
        }
        return;
    }
    public void MoveMap(string newLocation)
    {
        _currentLocation = newLocation;
    }
    public void SpendGold(int amount)
    {
        if (amount < 0)
        {
            _gold += -amount;
            Console.WriteLine($"{_name}은 {-amount} 골드를 얻었다! Total gold: {_gold}");
            return;
        }
        if (_gold >= amount)
        {
            _gold -= amount;
            Console.WriteLine($"{_name} spent {amount} gold. Remaining gold: {_gold}");
        }
        else
        {
            Console.WriteLine($"{_name} does not have enough gold to spend {amount}.");
        }
    }
    public void ShowInventory()
    {
        if (_item == null || _item.Count == 0)
        {
            Console.WriteLine("인벤토리에 아이템이 없습니다.");
            return;
        }
        Console.WriteLine("인벤토리:");
        Console.WriteLine("===================================인벤토리=======================================");
        for (int i = 0; i < _item.Count; i++)
        {
            if (_item[i] != null)
            {
                Console.WriteLine($"{i + 1}: {_item[i].Name} - {_item[i].Description}");
            }
        }
        Console.WriteLine("=================================================================================");
    }
}
public enum BuildingType { Town, Shop, Inn, Guild, Dungeon }

public class BuildingData
{
    private string _name;
    private BuildingType _type;
    public BuildingType type
    {
        get { return _type; }
        set { _type = value; }
    }
    
    private List<ItemData> _shopItemList;
    public List<ItemData> ShopItemList
    {
        get { return _shopItemList; }
    }

    public BuildingData(string name, BuildingType type)
    {
        _name = name;
        _type = type;
        if(_type == BuildingType.Shop)
        {
            _shopItemList = new List<ItemData>();
            InitializeShopItems();
        }
        else
        {
            _shopItemList = null; // 상점이 아닌 경우 아이템 리스트는 null
        }
    }
    
    public void InitializeShopItems()
    {
        _shopItemList.Add(GameManager.Instance.CreateManager.ItemDatabase.ItemDict["철검"]);
        _shopItemList.Add(GameManager.Instance.CreateManager.ItemDatabase.ItemDict["철갑옷"]);
        _shopItemList.Add(GameManager.Instance.CreateManager.ItemDatabase.ItemDict["보석반지"]);
    }
    public void BuyItem(ItemData item)
    {
        if (_shopItemList.Contains(item))
        {
            if (GameManager.Instance.CreateManager.Player.Gold >= item.Price)
            {
                GameManager.Instance.CreateManager.Player.SpendGold(item.Price);
                GameManager.Instance.CreateManager.Player.AddItem(item);
                _shopItemList.Remove(item);
                GameManager.Instance.TextManager.ViewText($"{item.Name} 아이템을 구매했습니다.");
            }
            else
            {
                GameManager.Instance.TextManager.ViewText("골드가 부족합니다.");
            }
        }
    }
    public void SellItem(ItemData item)
    {
        if (GameManager.Instance.CreateManager.Player.Item.Count!=0)
        {
            if(item.Name == "설명서")
            {
                GameManager.Instance.TextManager.ViewText("설명서는 판매할 수 없습니다.");
                return;
            }
            GameManager.Instance.CreateManager.Player.RemoveItem(item.Name);
            GameManager.Instance.CreateManager.Player.SpendGold(-item.Price / 2);
            if(item.Type == ItemData.ItemType.Consumable || item.Type == ItemData.ItemType.QuestItem)
            {
                /// Consumable 아이템이나 QuestItem은 상점에서 되팔지 않는다.
            }
            else
            {
                _shopItemList.Add(item);
            }
                GameManager.Instance.TextManager.ViewText($"{item.Name} 아이템을 판매했습니다.");
        }
        else
        {
            GameManager.Instance.TextManager.ViewText("인벤토리에 아이템이 없습니다.");
        }
    }
    public void ShowShopItems()
    {
        if (_shopItemList.Count == 0)
        {
            GameManager.Instance.TextManager.ViewText("상점에 아이템이 없습니다.");
            return;
        }
        GameManager.Instance.TextManager.ViewText("상점 아이템 목록:");
        for (int i = 0; i < _shopItemList.Count; i++)
        {
            GameManager.Instance.TextManager.ViewText($"{i + 1}: {_shopItemList[i].Name} - 가격: {_shopItemList[i].Price}골드");
        }
        GameManager.Instance.TextManager.ViewText("==========================================================================");
    }
    public void ShowBuilding()
    {
        GameManager.Instance.TextManager.ViewText($"건물 이름: {_name}, 종류: {_type}");
    }
    public void EnterBuilding()
    {
        GameManager.Instance.TextManager.ViewText($"{_name}에 들어갑니다.");
        if(_type == BuildingType.Inn)
        {
            if(GameManager.Instance.CreateManager.Player.Hp < 100 && GameManager.Instance.CreateManager.Player.Gold > 10)
            {
                GameManager.Instance.CreateManager.Player.SpendGold(10);
                GameManager.Instance.CreateManager.Player.TakeDamage(-50);
                GameManager.Instance.TextManager.ViewText("HP가 50 회복되었습니다.");
            }
            else
            {
                GameManager.Instance.TextManager.ViewText("이미 HP가 가득 찼습니다.");
            }
            GameManager.Instance.TextManager.ViewText("여관에서 휴식을 취합니다.");

        }
        else if (_type == BuildingType.Shop)
        {
            GameManager.Instance.TextManager.ViewText("상점에서 물건을 구매하거나 판매 합니다.");
            GameManager.Instance.ConditionManager.Shop();
        }
        else if (_type == BuildingType.Guild)
        {
            if(GameManager.Instance.CreateManager.Player.QuestProgress == "초보")
            {
                GameManager.Instance.TextManager.ViewText($"=================================================================================");
                GameManager.Instance.TextManager.ViewText("길드에 가입하려면 던전에서 토끼고기 1개를 얻어오세요.");
                if( GameManager.Instance.CreateManager.Player.Item != null)
                {
                    int count = GameManager.Instance.CreateManager.Player.Item.Count(item => item.Name == "토끼고기");
                    if (count >= 1)
                    {
                        GameManager.Instance.TextManager.ViewText("길드에 가입합니다.");
                        GameManager.Instance.CreateManager.Player.QuestProgress = "던전입구";
                        GameManager.Instance.CreateManager.Player.RemoveItem("토끼고기");
                        GameManager.Instance.TextManager.ViewText("길드에 가입했습니다.");
                        GameManager.Instance.TextManager.ViewText("아직은 약해서 던전 깊숙히 갈 수 없습니다.");
                        GameManager.Instance.TextManager.ViewText("슬라임이 나오는 던전에서 Lv을 3까지 올리세요");
                    }
                    else
                    {
                        GameManager.Instance.TextManager.ViewText($"토끼고기가 부족합니다. 현재 토끼고기: {count}개 필요: 1개");
                    }
                }
                else
                {
                    GameManager.Instance.TextManager.ViewText("토끼고기가 없습니다. 던전에서 토끼를 처치하여 획득하세요.");
                    GameManager.Instance.TextManager.ViewText($"=================================================================================");
                }
                return;
            }
            else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전입구")
            {
                GameManager.Instance.TextManager.ViewText($"=================================================================================");
                GameManager.Instance.TextManager.ViewText("길드에서 퀘스트를 받습니다.");
                GameManager.Instance.TextManager.ViewText("던전에서 슬라임을 처치하여 Lv을 3까지 올리세요.");
                if(GameManager.Instance.CreateManager.Player.Lv>2)
                {
                    GameManager.Instance.TextManager.ViewText("충분히 강해졌습니다 이제 고블린 던전으로 갈 수 있습니다.");
                    GameManager.Instance.CreateManager.Player.QuestProgress = "던전";
                }
                else
                {
                    GameManager.Instance.TextManager.ViewText("아직은 약해서 고블린 던전으로 갈 수 없습니다.");
                }
            }
            else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전")
            {
                GameManager.Instance.TextManager.ViewText("길드에서 퀘스트를 받습니다.");
                GameManager.Instance.TextManager.ViewText("던전에서 고블린을 처치하여 고블린귀를 가져다주세요.");
                
                if(GameManager.Instance.CreateManager.Player.Item != null)
                {
                    int count = GameManager.Instance.CreateManager.Player.Item.Count(item => item.Name == "고블린귀");
                    if (count >= 1)
                    {
                        GameManager.Instance.TextManager.ViewText("고블린귀를 가져왔군요. 퀘스트를 완료합니다.");
                        GameManager.Instance.CreateManager.Player.RemoveItem("고블린귀");
                        GameManager.Instance.CreateManager.Player.QuestProgress = "던전깊은곳";
                        GameManager.Instance.TextManager.ViewText("던전 깊은 곳으로 갈 수 있습니다.");
                    }
                    else
                    {
                        GameManager.Instance.TextManager.ViewText($"고블린귀가 부족합니다. 현재 고블린귀: {count}개 필요: 1개");
                    }
                }
                else
                {
                    GameManager.Instance.TextManager.ViewText("고블린귀가 없습니다. 던전에서 고블린을 처치하여 획득하세요.");
                }
            }
            else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전깊은곳")
            {
                GameManager.Instance.TextManager.ViewText("길드에서 퀘스트를 받습니다.");
                GameManager.Instance.TextManager.ViewText("던전 깊은곳에서 고블린주술사를 처치하고 지팡이를 가져와주세요.");
                if(GameManager.Instance.CreateManager.Player.Item.Count > 0)
                {
                    int count = GameManager.Instance.CreateManager.Player.Item.Count(item => item.Name == "나무지팡이");
                    if (count >= 1)
                    {
                        GameManager.Instance.TextManager.ViewText("나무지팡이를 가져왔군요. 퀘스트를 완료합니다.");
                        GameManager.Instance.CreateManager.Player.RemoveItem("나무지팡이");
                        GameManager.Instance.TextManager.ViewText("길드 퀘스트를 완료했습니다.");
                        GameManager.Instance.CreateManager.Player.QuestProgress = "모든퀘스트완료";
                    }
                    else
                    {
                        GameManager.Instance.TextManager.ViewText($"나무지팡이가 부족합니다. 현재 나무지팡이: {count}개 필요: 1개");
                    }
                }
                else
                {
                    GameManager.Instance.TextManager.ViewText("나무지팡이가 없습니다. 던전 깊은 곳에서 고블린주술사를 처치하여 획득하세요.");
                }
                GameManager.Instance.TextManager.ViewText($"=================================================================================");
            }
            else if (GameManager.Instance.CreateManager.Player.QuestProgress == "모든퀘스트완료")
            {
                GameManager.Instance.TextManager.ViewText("길드 퀘스트를 모두 완료했습니다.");
                GameManager.Instance.TextManager.ViewText("이제 던전에는 모든 종류의 몬스터들이 나옵니다.");
                GameManager.Instance.TextManager.ViewText("다양한 몬스터를 잡고 무한 레벨업을 해보세요.");
            }

        }
        else if (_type == BuildingType.Dungeon)
        {
            GameManager.Instance.TextManager.ViewText("던전에 입장합니다.");
            GameManager.Instance.DungeonManager.OnEncounter();
        }
    }
}

class BuildingDatabase
{
    private Dictionary<string, BuildingData> _buildingDict;
    public Dictionary<string, BuildingData> BuildingDict
    {
        get { return _buildingDict; }
    }
    public BuildingDatabase()
    {
        _buildingDict = new Dictionary<string, BuildingData>();
    }
    public void Create()
    {
        BuildingData inn = new BuildingData("여관", BuildingType.Inn);
        BuildingData shop = new BuildingData("상점", BuildingType.Shop);
        BuildingData town = new BuildingData("마을", BuildingType.Town);
        BuildingData guild = new BuildingData("길드", BuildingType.Guild);
        BuildingData dungeon = new BuildingData("던전", BuildingType.Dungeon);

        _buildingDict.Add("여관", inn);
        _buildingDict.Add("상점", shop);
        _buildingDict.Add("마을", town);
        _buildingDict.Add("길드", guild);
        _buildingDict.Add("던전", dungeon);

        Console.WriteLine("건물 데이터가 추가되었습니다.");
    }
}


class TextManager
{
    public void ViewText(string text)
    {
        Console.WriteLine(text);
    }


}
class InputManager
{
    public string inputData;
    public string InputString()
    {
        Console.Write("입력: ");
        return Console.ReadLine();
    }
}

class ConditionManager
{
    public void Action()
    {
        while (true)
        {
            Console.WriteLine("=================================================================================");
            GameManager.Instance.TextManager.ViewText("행동을 선택해주세요.");
            GameManager.Instance.TextManager.ViewText("1: 이동 2: 상태창 3: 가방 4: 종료");
            GameManager.Instance.InputManager.inputData = GameManager.Instance.InputManager.InputString();
            if (GameManager.Instance.InputManager.inputData == "1")
            {
                Console.WriteLine("=================================================================================");
                CheckMove();
            }
            else if (GameManager.Instance.InputManager.inputData == "2")
            {
                GameManager.Instance.CreateManager.Player.ShowPlayerStat();
            }
            else if (GameManager.Instance.InputManager.inputData == "3")
            {
                Console.WriteLine("=================================================================================");
                GameManager.Instance.TextManager.ViewText("가방!");
                if (GameManager.Instance.CreateManager.Player.Item!=null)
                {
                    GameManager.Instance.CreateManager.Player.ShowInventory();
                    GameManager.Instance.TextManager.ViewText("1: 아이템 사용 / 장비 장착 2: 장비 해제 3: 가방 닫기");
                    GameManager.Instance.InputManager.inputData = GameManager.Instance.InputManager.InputString();
                    if (GameManager.Instance.InputManager.inputData == "1")
                    {
                        GameManager.Instance.TextManager.ViewText("사용할 아이템의 번호를 입력해주세요.");
                        GameManager.Instance.InputManager.inputData = GameManager.Instance.InputManager.InputString();
                        if (GameManager.Instance.CreateManager.Player.Item.Count < int.Parse(GameManager.Instance.InputManager.inputData) || int.Parse(GameManager.Instance.InputManager.inputData) <= 0)
                        {
                            GameManager.Instance.TextManager.ViewText("잘못된 아이템 번호입니다.");
                            continue;
                        }
                        else
                        {
                            GameManager.Instance.CreateManager.Player.Use(GameManager.Instance.CreateManager.Player.Item[int.Parse(GameManager.Instance.InputManager.inputData) - 1]);
                        }
                    }
                    else if (GameManager.Instance.InputManager.inputData == "3")
                    {
                        GameManager.Instance.TextManager.ViewText("가방을 닫습니다.");
                        continue;
                    }
                    else if (GameManager.Instance.InputManager.inputData == "2")
                    {
                        if(GameManager.Instance.CreateManager.Player.Weapon != "없음" || GameManager.Instance.CreateManager.Player.Armor != "없음" || GameManager.Instance.CreateManager.Player.Accessory != "없음")
                        {
                            GameManager.Instance.TextManager.ViewText("장비 해제 메뉴로 이동합니다.");
                            while (true)
                            {
                                GameManager.Instance.TextManager.ViewText("장비 해제할 장비칸의 번호를 입력해주세요.");
                                GameManager.Instance.TextManager.ViewText("1: 무기 2: 방어구 3: 장신구");
                                GameManager.Instance.InputManager.inputData = GameManager.Instance.InputManager.InputString();
                                if (GameManager.Instance.InputManager.inputData == "1")
                                {
                                    if (GameManager.Instance.CreateManager.Player.Weapon == "없음")
                                    {
                                        GameManager.Instance.TextManager.ViewText("무기를 착용하고 있지 않습니다.");
                                        continue;
                                    }
                                    else
                                    {
                                        GameManager.Instance.CreateManager.Player.Unequip(GameManager.Instance.CreateManager.ItemDatabase.ItemDict[GameManager.Instance.CreateManager.Player.Weapon]);
                                        break;
                                    }
                                }
                                else if (GameManager.Instance.InputManager.inputData == "2")
                                {
                                    if (GameManager.Instance.CreateManager.Player.Armor == "없음")
                                    {
                                        GameManager.Instance.TextManager.ViewText("방어구를 착용하고 있지 않습니다.");
                                        continue;
                                    }
                                    else if (GameManager.Instance.CreateManager.Player.Armor != "없음")
                                    {
                                        GameManager.Instance.CreateManager.Player.Unequip(GameManager.Instance.CreateManager.ItemDatabase.ItemDict[GameManager.Instance.CreateManager.Player.Armor]);
                                        break;
                                    }
                                }
                                else if (GameManager.Instance.InputManager.inputData == "3")
                                {
                                    if (GameManager.Instance.CreateManager.Player.Accessory == "없음")
                                    {
                                        GameManager.Instance.TextManager.ViewText("장신구를 착용하고 있지 않습니다.");
                                        continue;
                                    }
                                    else if (GameManager.Instance.CreateManager.Player.Accessory != "없음")
                                    {
                                        GameManager.Instance.CreateManager.Player.Unequip(GameManager.Instance.CreateManager.ItemDatabase.ItemDict[GameManager.Instance.CreateManager.Player.Accessory]);
                                        break;
                                    }
                                }
                                else
                                {
                                    GameManager.Instance.TextManager.ViewText("잘못된 입력입니다.");
                                }
                            }
                        }
                        else
                        {
                            GameManager.Instance.TextManager.ViewText("장비가 없습니다. 장비를 먼저 착용해주세요.");
                            continue;
                        }
                        
                    }
                    else
                    {
                        GameManager.Instance.TextManager.ViewText("잘못된 입력입니다. 다시 시도해주세요.");
                    }
                }
                else
                {
                    GameManager.Instance.TextManager.ViewText("가방이 비어있습니다.");
                }
                Console.WriteLine("=================================================================================");

            }
            else if (GameManager.Instance.InputManager.inputData == "4")
            {
                GameManager.Instance.TextManager.ViewText("게임을 종료합니다.");
                GameManager.Instance.SaveGame();
                GameManager.Instance.EndGame();

            }
            else if (GameManager.Instance.InputManager.inputData == "5")
            {
                GameManager.Instance.TextManager.ViewText("게임을 종료합니다.");
                Environment.Exit(0);
            }
            else
            {
                GameManager.Instance.TextManager.ViewText("잘못된 입력입니다. 다시 시도해주세요.");
                Action();
            }
        }
    }
    public void Shop()
    {
        Console.WriteLine("=================================================================================");
        while (true)
        {
            GameManager.Instance.TextManager.ViewText("상점에 들어왔습니다.");
            GameManager.Instance.CreateManager.BuildingDatabase.BuildingDict["상점"].ShowShopItems();
            GameManager.Instance.TextManager.ViewText($"1: 아이템 구매 2: 아이템 판매 3: 상점 나가기 / 소지금 : {GameManager.Instance.CreateManager.Player.Gold}원");
            GameManager.Instance.InputManager.inputData = GameManager.Instance.InputManager.InputString();
            if (GameManager.Instance.InputManager.inputData == "1")
            {
                Console.WriteLine("=================================================================================");
                GameManager.Instance.TextManager.ViewText("구매할 아이템의 번호를 입력해주세요.");
                GameManager.Instance.InputManager.inputData = GameManager.Instance.InputManager.InputString();
                GameManager.Instance.CreateManager.BuildingDatabase.BuildingDict["상점"].ShowShopItems();
                int itemIndex = int.Parse(GameManager.Instance.InputManager.inputData) - 1;
                if (itemIndex >= 0 && itemIndex < GameManager.Instance.CreateManager.BuildingDatabase.BuildingDict["상점"].ShopItemList.Count)
                {
                    ItemData selectedItem = GameManager.Instance.CreateManager.BuildingDatabase.BuildingDict["상점"].ShopItemList[itemIndex];
                    GameManager.Instance.CreateManager.BuildingDatabase.BuildingDict["상점"].BuyItem(selectedItem);
                    Console.WriteLine("=================================================================================");
                }
                else
                {
                    GameManager.Instance.TextManager.ViewText("잘못된 아이템 번호입니다.");
                    Console.WriteLine("=================================================================================");
                }
            }
            else if (GameManager.Instance.InputManager.inputData == "2")
            {
                Console.WriteLine("=================================================================================");
                GameManager.Instance.TextManager.ViewText("판매할 아이템의 번호를 입력해주세요.");
                GameManager.Instance.CreateManager.Player.ShowInventory();
                int itemIndex = int.Parse(GameManager.Instance.InputManager.InputString());
                if (itemIndex <= 0 || itemIndex > GameManager.Instance.CreateManager.Player.Item.Count)
                {
                    GameManager.Instance.TextManager.ViewText("잘못된 아이템 번호입니다.");
                    Console.WriteLine("=================================================================================");
                    continue;
                }
                else
                {
                    ItemData itemToSell = GameManager.Instance.CreateManager.Player.Item[itemIndex - 1];
                    if (itemToSell != null)
                    {
                        GameManager.Instance.CreateManager.BuildingDatabase.BuildingDict["상점"].SellItem(itemToSell);
                    }
                    Console.WriteLine("=================================================================================");
                }
            }
            else if (GameManager.Instance.InputManager.inputData == "3")
            {
                GameManager.Instance.TextManager.ViewText("상점을 나갑니다.");
                Console.WriteLine("=================================================================================");
                break;
            }
            else
            {
                GameManager.Instance.TextManager.ViewText("잘못된 입력입니다. 다시 시도해주세요.");
            }
        }
    }
    public void CheckMove()
    {
        GameManager.Instance.TextManager.ViewText($"===================================마을설명======================================");
        GameManager.Instance.TextManager.ViewText("마을에는 상점, 여관, 길드, 던전이 있습니다.");
        GameManager.Instance.TextManager.ViewText("상점에서는 아이템을 구매하거나 판매할 수 있습니다.");
        GameManager.Instance.TextManager.ViewText("여관에서는 HP를 회복할 수 있습니다.");
        GameManager.Instance.TextManager.ViewText("길드에서는 퀘스트를 받을 수 있습니다.");
        GameManager.Instance.TextManager.ViewText("던전에서는 몬스터를 처치하고 경험치를 얻을 수 있습니다.");
        GameManager.Instance.TextManager.ViewText("광장에서는 아이템을 사용하거나 장비할 수 있습니다.");
        GameManager.Instance.TextManager.ViewText("=================================================================================");
        while (true)
        {
            GameManager.Instance.TextManager.ViewText("=================================================================================");
            GameManager.Instance.TextManager.ViewText($"이동 가능한 곳 1:상점 2:여관 3:길드 4:던전 5:광장");
            GameManager.Instance.InputManager.inputData = GameManager.Instance.InputManager.InputString();
            if (GameManager.Instance.InputManager.inputData == "1")
            {
                GameManager.Instance.TextManager.ViewText("=================================================================================");
                GameManager.Instance.TextManager.ViewText("상점으로 이동합니다.");
                GameManager.Instance.CreateManager.Player.MoveMap("상점");
                GameManager.Instance.CreateManager.BuildingDatabase.BuildingDict["상점"].EnterBuilding();
            }
            else if (GameManager.Instance.InputManager.inputData == "2")
            {
                GameManager.Instance.TextManager.ViewText("=================================================================================");
                GameManager.Instance.TextManager.ViewText("여관으로 이동합니다.");
                GameManager.Instance.CreateManager.Player.MoveMap("여관");
                GameManager.Instance.CreateManager.BuildingDatabase.BuildingDict["여관"].EnterBuilding();
            }
            else if (GameManager.Instance.InputManager.inputData == "3")
            {
                GameManager.Instance.TextManager.ViewText("=================================================================================");
                GameManager.Instance.TextManager.ViewText("길드로 이동합니다.");
                GameManager.Instance.CreateManager.Player.MoveMap("길드");
                GameManager.Instance.CreateManager.BuildingDatabase.BuildingDict["길드"].EnterBuilding();
            }
            else if (GameManager.Instance.InputManager.inputData == "4")
            {
                GameManager.Instance.TextManager.ViewText("=================================================================================");
                GameManager.Instance.TextManager.ViewText("던전으로 이동합니다.");
                GameManager.Instance.CreateManager.Player.MoveMap("던전");
                GameManager.Instance.DungeonManager.ExploreDungeon();
            }
            else if (GameManager.Instance.InputManager.inputData == "5")
            {
                GameManager.Instance.TextManager.ViewText("=================================================================================");
                GameManager.Instance.TextManager.ViewText("광장에서 정비합니다.");
                GameManager.Instance.CreateManager.Player.MoveMap("마을");
                Action();
            }
            else
            {
                GameManager.Instance.TextManager.ViewText("잘못된 입력입니다. 다시 시도해주세요.");
            }
        }
    }

}

class DungeonManager
{
    private Random random = new Random();
    private StatData encounterMonster;
    private List<StatData> monsters = new List<StatData>();
    private int monsterCount = 5;
    public void SommonMonster()
    {
        if (GameManager.Instance.CreateManager.Player.QuestProgress == "초보")
        {
            for (int i = 0; i < monsterCount; i++)
            {
                monsters.Add(GameManager.Instance.CreateManager.MonsterDatabase.MonsterDict["토끼"]);
            }
        }
        else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전입구")
        {
            for (int i = 0; i < monsterCount; i++)
            {
                monsters.Add(GameManager.Instance.CreateManager.MonsterDatabase.MonsterDict["슬라임"]);
            }
        }
        else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전")
        {
            for(int i = 0; i < monsterCount; i++)
            {
                monsters.Add(GameManager.Instance.CreateManager.MonsterDatabase.MonsterDict["고블린"]);
            }
        }
        else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전깊은곳")
        {
            for (int i = 0; i < monsterCount; i++)
            {
                monsters.Add(GameManager.Instance.CreateManager.MonsterDatabase.MonsterDict["고블린"]);
            }
            monsters.Add(GameManager.Instance.CreateManager.MonsterDatabase.MonsterDict["고블린주술사"]);
        }
        else if (GameManager.Instance.CreateManager.Player.QuestProgress == "모든퀘스트완료")
        {
            for (int i = 0; i < monsterCount; i++)
            {
                monsters.Add(GameManager.Instance.CreateManager.MonsterDatabase.MonsterDict["슬라임"]);
                monsters.Add(GameManager.Instance.CreateManager.MonsterDatabase.MonsterDict["토끼"]);
                monsters.Add(GameManager.Instance.CreateManager.MonsterDatabase.MonsterDict["고블린"]);
                monsters.Add(GameManager.Instance.CreateManager.MonsterDatabase.MonsterDict["고블린주술사"]);
            }
        }
        Console.WriteLine("몬스터가 생성되었습니다.");
    }
    public void UnSommonMonster(StatData monster)
    {
        monsters.Remove(monster);
        Console.WriteLine("몬스터가 제거되었습니다.");
    }

    public void ExploreDungeon()
    {
        GameManager.Instance.TextManager.ViewText($"===================================던전설명======================================");
        GameManager.Instance.TextManager.ViewText("던전에서 탐험을 하면 몬스터 혹은 보물 상자를 얻을 수 있습니다.");
        GameManager.Instance.TextManager.ViewText("길드의 허가를 얻으면 던전의 깊숙한 곳으로 들어갈 수 있습니다.");
        if(GameManager.Instance.CreateManager.Player.QuestProgress == "초보")
        {
            GameManager.Instance.TextManager.ViewText("초보자용 던전입니다. 토끼가 출몰합니다.");
        }
        else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전입구")
        {
            GameManager.Instance.TextManager.ViewText("던전 입구입니다. 슬라임이 출몰합니다.");
        }
        else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전")
        {
            GameManager.Instance.TextManager.ViewText("고블린이 출몰하는 던전입니다.");
        }
        else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전깊은곳")
        {
            GameManager.Instance.TextManager.ViewText("고블린주술사가 출몰하는 던전 깊은 곳입니다.");
        }
        else if (GameManager.Instance.CreateManager.Player.QuestProgress == "모든퀘스트완료")
        {
            GameManager.Instance.TextManager.ViewText("당신은 이곳에 있던 모든 적을 쓰러트렸습니다! 몬스터들이 복수를 위해 찾아옵니다.");
        }
        GameManager.Instance.TextManager.ViewText("=================================================================================");
        Console.WriteLine("던전을 탐험합니다.");
        SommonMonster(); // 몬스터 소환
        while (true)
        {
            Console.WriteLine("던전에서 이동할 곳을 선택해주세요.\n1: 계속 탐험 2: 던전 나가기");
            GameManager.Instance.InputManager.inputData = GameManager.Instance.InputManager.InputString();
            if (GameManager.Instance.InputManager.inputData == "1")
            {
                if (monsters.Count == 0)
                {
                    Console.WriteLine("던전의 모든 몬스터를 처치했습니다. 던전을 클리어합니다.");
                    ClearDungeon();
                    break;
                }
                encounterMonster = OnEncounter();
                if (encounterMonster == null)
                {
                    continue; // 아무 일도 일어나지 않았으면 다시 탐험
                }
                else
                {
                    GameManager.Instance.BattleManager.Battle(GameManager.Instance.CreateManager.Player, encounterMonster);
                }
            }
            else if (GameManager.Instance.InputManager.inputData == "2")
            {
                Console.WriteLine("던전을 나갑니다.");
                GameManager.Instance.CreateManager.Player.MoveMap("마을");
                ClearDungeon();
                break;
            }
            else
            {
                Console.WriteLine("잘못된 입력입니다. 다시 시도해주세요.");
            }
        }
    }
    public void ClearDungeon()
    {
        if (GameManager.Instance.CreateManager.Player.QuestProgress == "초보")
        {
            Console.WriteLine("던전의 모든 몬스터를 제거했습니다. 던전이 클리어되었습니다.");
            GameManager.Instance.CreateManager.Player.SpendGold(-10);
            Console.WriteLine("초보자용 던전이 클리어되었습니다.");

            return;
        }
        else if( GameManager.Instance.CreateManager.Player.QuestProgress == "던전입구")
        {
            Console.WriteLine("던전의 모든 몬스터를 제거했습니다. 던전이 클리어되었습니다.");
            GameManager.Instance.CreateManager.Player.SpendGold(-20);

            return;
        }
        else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전")
        {
            Console.WriteLine("던전의 모든 몬스터를 제거했습니다. 던전이 클리어되었습니다.");
            GameManager.Instance.CreateManager.Player.SpendGold(-30);
            return;
        }
        else if (GameManager.Instance.CreateManager.Player.QuestProgress == "던전깊은곳")
        {
            Console.WriteLine("던전 깊은 곳의 모든 몬스터를 제거했습니다. 던전이 클리어되었습니다.");
            GameManager.Instance.CreateManager.Player.SpendGold(-40);
            return;
        }
        else if (GameManager.Instance.CreateManager.Player.QuestProgress == "모든퀘스트완료")
        {
            Console.WriteLine("던전의 모든 몬스터를 처치했습니다. 던전이 클리어되었습니다.");
            GameManager.Instance.CreateManager.Player.SpendGold(-40);
            return;
        }
    }

    public StatData OnEncounter()
    {
        if (random.Next(0, 100) < 50) // 50% 확률로 몬스터 등장
        {
            int idx = random.Next(monsters.Count);
            var encounterMonster = new StatData(monsters[idx]);  //처음에는 참조였다가 생성으로 변경
            Console.WriteLine($"{encounterMonster.Name}가 나타났습니다!");
            encounterMonster.ShowMosterStat();
            monsters.RemoveAt(idx); // 등장한 몬스터는 리스트에서 제거
            return encounterMonster;
        }
        if(random.Next(0, 100) < 2) // 2% 확률로 보물 상자 등장
        {
            Console.WriteLine("보물 상자를 발견했습니다!");
            GameManager.Instance.CreateManager.Player.SpendGold(-50);
            Console.WriteLine($"골드를 획득했습니다.");
            return null;
        }
        else
        {
            Console.WriteLine("아무 일도 일어나지 않았습니다.");
            return null;
        }
    }
}

class BattleManager
{
    bool isRun = false;

    public void Battle(StatData player, StatData monster)
    {
        Console.WriteLine($"{player.Name}과(와) {monster.Name}의 전투가 시작되었습니다!");
        while (player.Hp > 0 && monster.Hp > 0)
        {
            PlayerTurn(player, monster);
            if (isRun)
            {
                isRun = false;
                Console.WriteLine($"{player.Name}이(가) 도망쳤습니다.");
                break;
            }
            EnemyTurn(player, monster);

        }
        if (player.Hp <= 0)
        {
            Console.WriteLine($"{player.Name}이(가) 쓰러졌습니다. 게임 오버!");
        }
        else if (monster.Hp <= 0)
        {
            Console.WriteLine($"{monster.Name}을(를) 처치했습니다!");
            player.TakeExp(monster.Lv * 10);
            player.SpendGold(-monster.Gold);
            player.AddItem(GameManager.Instance.CreateManager.ItemDatabase.ItemDict[monster.DropItem]);
            Console.WriteLine($"{monster.Name}이(가){monster.DropItem}, {monster.Gold}골드를 드랍했습니다.");
            PlayerWin(monster);
        }
    }

    public void PlayerWin(StatData monster)
    {
        GameManager.Instance.DungeonManager.UnSommonMonster(monster);
    }
    public void Attack(StatData attacker, StatData defender)
    {
        int damage = attacker.Str - defender.ArmorPoint;
        if (damage < 0) damage = 0;
        defender.TakeDamage(damage);
        Console.WriteLine($"{attacker.Name}이(가) {defender.Name}에게 {damage}의 피해를 입혔습니다.");
        defender.Death();
    }
    public void Magic(StatData attacker, StatData defender)
    {
        int magicDamage = attacker.Int; 
        defender.TakeDamage(magicDamage);
        Console.WriteLine($"{attacker.Name}이(가) {defender.Name}에게 {magicDamage}의 마법 피해를 입혔습니다.");
        defender.Death();
    }
    public void Run()
    {
        Console.WriteLine($"도망쳤습니다.");
        isRun = true;
        return;
    }
    public void Use(StatData player, StatData monster, ItemData item)
    {
        if (item.Type == ItemData.ItemType.Consumable)
        {
            player.TakeDamage(-item.Value); // 아이템 사용으로 HP 회복
            Console.WriteLine($"{player.Name}이(가) {item.Name}을(를) 사용하여 {item.Value}의 HP를 회복했습니다.");
        }
        else if (item.Type == ItemData.ItemType.QuestItem)
        {
            Console.WriteLine($"이 아이템을 사용하기 적절하지 못한 상대입니다.");
            //Console.WriteLine($"{player.Name}이(가) {item.Name}을(를) 사용했습니다.");
        }
        else
        {
            Console.WriteLine($"지금은 장비를 바꿀 수 없습니다.");
        }
    }
    public void PlayerTurn(StatData player, StatData monster)
    {
        Console.WriteLine($"{player.Name}의 턴입니다.");
        while (player.Hp > 0 && monster.Hp > 0)
        {
            Console.WriteLine($"공격을 선택해주세요.\n1: 공격 2: 스킬 3: 아이템 사용 4: 도망치기");
            GameManager.Instance.InputManager.inputData = GameManager.Instance.InputManager.InputString();
            if (GameManager.Instance.InputManager.inputData == "1")
            {
                Attack(player, monster);
                return;
            }
            else if (GameManager.Instance.InputManager.inputData == "2")
            {
                Magic(player, monster);
                return;
            }
            else if (GameManager.Instance.InputManager.inputData == "3")
            {
                Console.WriteLine("사용할 아이템의 번호를 입력해주세요.");
                GameManager.Instance.CreateManager.Player.ShowInventory();
                GameManager.Instance.InputManager.inputData = GameManager.Instance.InputManager.InputString();
                Use(player, monster, GameManager.Instance.CreateManager.Player.Item[int.Parse(GameManager.Instance.InputManager.inputData) - 1]);
                continue;
            }
            else if (GameManager.Instance.InputManager.inputData == "4")
            {
                Run();
                return;
            }
            else
            {
                Console.WriteLine("잘못된 입력입니다. 다시 시도해주세요.");
                continue;
            }
        }
    }
    public void EnemyTurn(StatData player, StatData monster)
    {
        Console.WriteLine($"{monster.Name}의 턴입니다.");
        if (monster.Hp > 0)
        {
            if(monster.Mp > 0 && new Random().Next(0, 2) == 0) // 50% 확률로 마법 공격
            {
                Magic(monster, player);
                return;
            }
            else
            { 
                Attack(monster, player);
            }
        }
        else
        {
            Console.WriteLine($"{monster.Name}은(는) 이미 쓰러졌습니다.");
        }
    }

}

class ItemDatabase
{    
    private Dictionary<string, ItemData> _itemDict;
    public Dictionary<string, ItemData> ItemDict
    {
        get { return _itemDict; }
    }
    public ItemDatabase()
    {
        _itemDict = new Dictionary<string, ItemData>();
    }
    public void Create()
    {
        ItemData 토끼고기 = new ItemData("토끼고기", "토끼를 죽이고 얻은 고기. 길드로 가져갑시다.", ItemData.ItemType.QuestItem, 20, 10);
        ItemData 슬라임젤리 = new ItemData("슬라임젤리", "슬라임이 떨어뜨린 젤리. 체력을 10 회복한다.", ItemData.ItemType.Consumable, 20, 10);
        ItemData 고블린귀 = new ItemData("고블린귀", "고블린의 귀. 길드로 가져갑시다.", ItemData.ItemType.QuestItem, 0, 15);
        ItemData 나무지팡이 = new ItemData("나무지팡이", "고블린주술사가 사용하는 지팡이. 마법 공격력이 5 증가합니다.", ItemData.ItemType.Staff, 5, 20);
        ItemData 철검 = new ItemData("철검", "철로 만든 검. 공격력이 15 증가합니다.", ItemData.ItemType.Weapon, 15, 50);
        ItemData 가죽갑옷 = new ItemData("가죽갑옷", "초보자에게 지급되는 방어구. 방어력이 5 증가합니다.", ItemData.ItemType.Armor, 5, 10);
        ItemData 반지 = new ItemData("반지", "초보자에게 지급되는 반지. 체력이 5 증가합니다.", ItemData.ItemType.Accessory, 5, 10);
        ItemData 나무검 = new ItemData("나무검", "초보자에게 지급되는 무기. 공격력이 5 증가합니다.", ItemData.ItemType.Weapon, 5, 10);
        ItemData 철갑옷 = new ItemData("철갑옷", "철로 만든 검. 방어력이 15 증가합니다.", ItemData.ItemType.Armor, 15, 50);
        ItemData 보석반지 = new ItemData("보석반지", "보석이 박힌 반지. 체력이 15 증가합니다.", ItemData.ItemType.Accessory, 15, 50);
        ItemData 설명서 = new ItemData("설명서", "게임 설명서입니다. 게임을 시작하기 전에 읽어보세요.", ItemData.ItemType.Consumable, 0, 0);


        _itemDict.Add("토끼고기", 토끼고기);
        _itemDict.Add("슬라임젤리", 슬라임젤리);
        _itemDict.Add("고블린귀", 고블린귀);
        _itemDict.Add("나무지팡이", 나무지팡이);
        _itemDict.Add("철검", 철검);
        _itemDict.Add("가죽갑옷", 가죽갑옷);
        _itemDict.Add("반지", 반지);
        _itemDict.Add("나무검", 나무검);
        _itemDict.Add("철갑옷", 철갑옷);
        _itemDict.Add("보석반지", 보석반지);
        _itemDict.Add("설명서", 설명서);

        Console.WriteLine("아이템 데이터가 추가되었습니다.");
    }
}

public class ItemData
{
    private string _name;
    private string _description;
    public string Name => _name;
    public string Description => _description;

    private int _price;
    public int Price
    {
        get { return _price; }
    }

    private int _Value;
    public int Value
    {
        get { return _Value; }
    }
    public enum ItemType { Weapon, Staff, Armor, Accessory, Consumable, QuestItem }
    public ItemType Type { get; set; }
    public ItemData(string name, string description, ItemType type, int value, int price)
    {
        _name = name;
        _description = description;
        Type = type;
        _Value = value;
        _price = price;
    }
    public void ShowItem()
    {
        Console.WriteLine($"아이템 이름: {_name}, 설명: {_description}, 가격: {_price}");
    }
}

public class PlayerData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int MaxHp { get; set; }
    public int Hp { get; set; }
    public int MaxMp { get; set; }
    public int Mp { get; set; }
    public int Str { get; set; }
    public int Int { get; set; }
    public int ArmorPoint { get; set; }
    public string Weapon { get; set; }
    public string Armor { get; set; }
    public string Accessory { get; set; }
    public string DropItem { get; set; }
    public int Lv { get; set; }
    public int Exp { get; set; }
    public int NextExp { get; set; }
    public int Gold { get; set; }
    public int plusStr { get; set; }
    public int plusInt { get; set; }
    public int plusArmorPoint { get; set; }
    public int plusHP { get; set; }
    public int plusMP { get; set; }
    public string CurrentLocation { get; set; }
    public List<ItemData> Item { get; set; }
    public string QuestProgress { get; set; }

    public PlayerData()
    {
    }
}


// 메인 프로그램
class Program
{
    static void Main(string[] args)
    {
        GameManager gameManager = new GameManager();
        gameManager.StartGame();

    }

}
