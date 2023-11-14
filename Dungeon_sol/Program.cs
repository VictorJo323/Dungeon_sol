using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.Arm;
using System.Numerics;

namespace Dungeon_sol
{
    public class Character
    {
        public string Name { get; }
        public string Job { get; }
        public int Level { get; }
        public int Atk { get; }
        public int Def { get; }
        public int Hp { get; }
        public int Gold { get; set; }

        public Character(string name, string job, int level, int atk, int def, int hp, int gold)
        {
            Name = name;
            Job = job;
            Level = level;
            Atk = atk;
            Def = def;
            Hp = hp;
            Gold = gold;
        }
    }

    public class Item
    {
        public string Name { get; }
        public string Description { get; }
        public int Type { get; }
        public int Atk { get; }
        public int Def { get; }
        public int Hp { get; }
        public int Price { get; }
        public bool IsEquipped { get; set; }
        public bool IsBought { get; set; }

        public static int ItemCnt = 0;

        public Item(string name, string description, int type, int atk, int def, int hp, int price, bool isEquipped = false, bool isBought = false)
        {
            Name = name;
            Description = description;
            Type = type;
            Atk = atk; Def = def;
            Hp = hp;
            Price = price;
            IsEquipped = isEquipped;
            IsBought = isBought;
        }

        public void PrintItemStatDescription(bool withNumber = false, int idx = 0)
        {
            Console.Write("- ");
            if(withNumber)
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("{0} ", idx);
                Console.ResetColor();
            }
            if (IsEquipped)
            {
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("E");
                Console.ResetColor();
                Console.Write("]");
                Console.Write(PadRightForMixedText(Name, 9));
            }
            else 
            { 
                Console.Write(PadRightForMixedText(Name, 12)); 
            }

            Console.Write("|");
            if (Atk != 0)
            {
                Console.Write($"ATK {(Atk >= 0 ? "+" : "")}{Atk} ");
            }
            if (Def != 0)
            {
                Console.Write($"DEF {(Def >= 0 ? "+" : "")}{Def} ");
            }
            if (Hp != 0)
            {
                Console.Write($"HP {(Hp >= 0 ? "+" : "")}{Hp} ");
            }
            Console.Write("|");
            Console.WriteLine(Description);
        }
        public static int GetPrintableLength(string str)

        {
            int length = 0;
            foreach (char c in str)
            {
                if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
                {
                    length += 2;
                }
                else
                {
                    length += 1;
                }
            }
            return length;
        }

        public static string PadRightForMixedText(string str, int totalLength)
        {
            int currentLength = GetPrintableLength(str);
            int padding = totalLength - currentLength;
            return str.PadRight(str.Length + padding);
        }
    }

    public class Shop
    {
        public List<Item> shopItemList;
        

        public Shop()
        {
            shopItemList = new List<Item>();
            shopItemList.Add(new Item("수련자 갑옷", "수련에 도움을 주는 갑옷입니다.", 0, 0, 3, 0, 300));
            shopItemList.Add(new Item("무쇠갑옷", "무쇠로 만들어져 튼튼한 갑옷입니다.", 0, 0, 5, 0, 500));
            shopItemList.Add(new Item("스파르타의 갑옷", "스파르타의 전사들이 사용했다는 전설의 갑옷입니다.", 0, 0, 10, 0, 1000));
            shopItemList.Add(new Item("낡은 검", "쉽게 볼 수 있는 낡은 검입니다.", 1, 2, 0, 0, 300));
            shopItemList.Add(new Item("청동 도끼", "어디선가 사용됐던 것 같은 도끼입니다.", 1, 5, 0, 0, 700));
            shopItemList.Add(new Item("스파르타의 창", "스파르타의 전사들이 사용했다는 전설의 창입니다.", 1, 7, 0, 0, 1000));
        }
    }

    
    internal class Program
    {
        static Character _player;
        static Item[] _items;
        
        static void Main(string[] args)
        {
            GameDataSetting();
            PrintStartLogo();
            StartMenu();
        }

        static void StartMenu()
        {
            Console.Clear();
            Console.WriteLine("■□■□■□■□■□■□■□■□■□■□■□■□■□");
            Console.WriteLine("스파르타 마을에 오신 여러분 환영합니다.");
            Console.WriteLine("이곳에서 던전으로 들어가기 전 활동을 할 수 있습니다.");
            Console.WriteLine("□■□■□■□■□■□■□■□■□■□■□■□■□■");

            Console.WriteLine();
            Console.WriteLine("1. 상태 보기");
            Console.WriteLine("2. 인벤토리");
            Console.WriteLine("3. 상점");
            Console.WriteLine();

            switch (CheckValidInput(1, 3))
            {
                case 1:
                    StatusMenu();
                    break;
                case 2:
                    InventoryMenu();
                    break;
                case 3:
                    ShopMenu();
                    break;
            }
        }


        private static void ShopMenu()     // 상점 메뉴 생성
        {
            Console.Clear();
            Shop shop = new Shop();
            ShowHighlightedText("◆상점◆");
            Console.WriteLine("필요한 아이템을 살 수 있는 상점입니다.");
            Console.WriteLine();
            Console.WriteLine("[보유 골드]");
            Console.WriteLine("{0} G", _player.Gold);
            Console.WriteLine();
            Console.WriteLine("[상점 목록]");
            for (int i = 0; i < shop.shopItemList.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {shop.shopItemList[i].Name} | {shop.shopItemList[i].Description} | {shop.shopItemList[i].Price} G");
            }

            Console.WriteLine("\n\n");
            Console.WriteLine("구매하려는 아이템의 번호를 입력하세요.");
            Console.WriteLine("0. 뒤로가기");

            int keyInput = CheckValidInput(0, shop.shopItemList.Count);
            switch(keyInput)
            {
                case 0:
                    StartMenu();
                    break;

                default:
                BuyItem(_player, shop.shopItemList[keyInput - 1]);
                ShopMenu();
                    break;
            }
        }

        private static void BuyItem(Character _player, Item item)
        {
            while(!item.IsBought)
            {
                if (_player.Gold >= item.Price && !item.IsBought)
                {
                    _player.Gold -= item.Price;
                    item.IsBought = true;
                    AddItem(item);
                    Console.WriteLine($"구매 완료: {item.Name}");
                }
                else
                {
                    Console.WriteLine("소지금액이 부족합니다.");
                    Console.WriteLine("아무 키나 눌러 화면을 나가십시오.");
                    ShopMenu();
                    break;
                }            
            }            
            Console.WriteLine("뒤로가기");
            Console.ReadKey();
        }

        private static void Shop()
        {
            AddItem(new Item("수련자 갑옷", "수련에 도움을 주는 갑옷입니다.", 0, 0, 3, 0, 300));
            AddItem(new Item("무쇠갑옷", "무쇠로 만들어져 튼튼한 갑옷입니다.", 0, 0, 5, 0, 500));
            AddItem(new Item("스파르타의 갑옷", "스파르타의 전사들이 사용했다는 전설의 갑옷입니다.", 0, 0, 10, 0, 1000));
            AddItem(new Item("낡은 검", "쉽게 볼 수 있는 낡은 검입니다.", 1, 2, 0, 0, 300));
            AddItem(new Item("청동 도끼", "어디선가 사용됐던 것 같은 도끼입니다.", 1, 5, 0, 0, 700));
            AddItem(new Item("스파르타의 창", "스파르타의 전사들이 사용했다는 전설의 창입니다.", 1, 7, 0, 0, 1000));
        }
 

        private static void InventoryMenu()
        {
            Console.Clear();

            ShowHighlightedText("◆인벤토리◆");
            Console.WriteLine("보유 중인 아이템을 관리할 수 있습니다.");
            Console.WriteLine();
            Console.WriteLine("[아이템 목록]");

            for (int i = 0; i < Item.ItemCnt; i++)
            {
                _items[i].PrintItemStatDescription();
            }
            Console.WriteLine();
            Console.WriteLine("0. 나가기");
            Console.WriteLine("1. 장착 관리");
            switch (CheckValidInput(0, 1))
            {
                case 0:
                    StartMenu();
                    break;
                case 1:
                    EquipMenu();
                    break;
            }
        }

        private static void EquipMenu()
        {
            Console.Clear();

            ShowHighlightedText("◆인벤토리 - 장착 관리◆");
            Console.WriteLine("보유중인 아이템을 관리할 수 있습니다.");
            Console.WriteLine();
            Console.WriteLine("[아이템 목록]");
            for (int i = 0; i < Item.ItemCnt; i++)
            {
                _items[i].PrintItemStatDescription(true, i+1);
            }
            Console.WriteLine();
            Console.WriteLine("0 : 나가기");

            int keyInput = CheckValidInput(0, Item.ItemCnt);

            switch(keyInput)
            {
                case 0:
                    InventoryMenu();
                    break;
                default:
                    ToggleEquipStatus(keyInput - 1);
                    EquipMenu();
                    break;
            }    
        }

        private static void ToggleEquipStatus(int idx)
        {
            _items[idx].IsEquipped = !_items[idx].IsEquipped;
        }

        private static void ToggleBoughtStatus(int idx)
        {
            _items[idx].IsBought = !_items[idx].IsBought;
        }

        private static void StatusMenu()
        {
            Console.Clear();
            ShowHighlightedText("◆상태 보기◆");
            Console.WriteLine("캐릭터의 정보가 표기됩니다.");

            PrintTextWithHighlights("Lv : ", _player.Level.ToString("00"));
            Console.WriteLine();
            Console.WriteLine("{0}, {1})", _player.Name, _player.Job);


            int bonusAtk = getSumBonusAtk();
            int bonusDef = getSumBonusDef();
            int bonusHp = getSumBonusHp();
            PrintTextWithHighlights("공격력 : ", (_player.Atk + bonusAtk).ToString(), bonusAtk>0? string.Format(" (+{0})", bonusAtk) : "");
            PrintTextWithHighlights("방어력 : ", (_player.Def + bonusDef).ToString(), bonusDef > 0 ? string.Format(" (+{0})", bonusDef) : "");
            PrintTextWithHighlights("체력 : ", (_player.Hp + bonusHp).ToString(), bonusHp>0? string.Format(" (+{0})", bonusHp): "");
            PrintTextWithHighlights("골드 : ", _player.Gold.ToString());
            Console.WriteLine();
            Console.WriteLine("0 : 뒤로가기");
            Console.WriteLine();


            switch (CheckValidInput(0, 0))
            {
                case 0:
                    StartMenu();
                    break;
            }
        }

        private static int getSumBonusAtk()
        {
            int sum = 0;
            for(int i = 0; i < Item.ItemCnt; i++)
            {
                if (_items[i].IsEquipped) sum += _items[i].Atk;
            }
            return sum;
        }
        private static int getSumBonusDef()
        {
            int sum = 0;
            for (int i = 0; i < Item.ItemCnt; i++)
            {
                if (_items[i].IsEquipped) sum += _items[i].Def;
            }
            return sum;
        }
        private static int getSumBonusHp()
        {
            int sum = 0;
            for (int i = 0; i < Item.ItemCnt; i++)
            {
                if (_items[i].IsEquipped) sum += _items[i].Hp;
            }
            return sum;
        }
        private static void ShowHighlightedText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void PrintTextWithHighlights(string s1, string s2, string s3 = "")
        {
            Console.Write(s1);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(s2);
            Console.ResetColor();
            Console.WriteLine(s3);
        }

        private static int CheckValidInput(int min, int max)
        {
            int keyInput;  // tryparse
            bool result;  // while

            do
            {
                Console.WriteLine("원하시는 행동을 입력해주세요.");
                result = int.TryParse(Console.ReadLine(), out keyInput);
            }
            while (result == false || CheckIfValid(keyInput, min, max) == false);
            {
                return keyInput;
            }
        }

        private static bool CheckIfValid(int keyInput, int min, int max)
        {
            if (min <= keyInput && max >= keyInput)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void PrintStartLogo()
        {
            Console.WriteLine("=====================================================");
            Console.WriteLine(" _____ _      _____  _____ ____    _____  _     _____");
            Console.WriteLine("/  __// \\  /|/__ __\\/  __//  __\\  /__ __\\/ \\ /|/  __/");
            Console.WriteLine("|  \\  | |\\ ||  / \\  |  \\  |  \\/|    / \\  | |_|||  \\  ");
            Console.WriteLine("|  /_ | | \\||  | |  |  /_ |    /    | |  | | |||  /_ ");
            Console.WriteLine("\\____\\\\_/  \\|  \\_/  \\____\\\\_/\\_\\    \\_/  \\_/ \\|\\____\\");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("    ____  _     _      _____ _____ ____  _      ");
            Console.WriteLine("   /  _ \\/ \\ /\\/ \\  /|/  __//  __//  _ \\/ \\  /|         ");
            Console.WriteLine("   | | \\|| | ||| |\\ ||| |  _|  \\  | / \\|| |\\ ||         ");
            Console.WriteLine("   | |_/|| \\_/|| | \\||| |_//|  /_ | \\_/|| | \\||         ");
            Console.WriteLine("   \\____/\\____/\\_/  \\|\\____\\\\____\\\\____/\\_/  \\|         ");
            Console.WriteLine("=====================================================");
            Console.WriteLine("               Press Any Key to Start!               ");
            Console.WriteLine("=====================================================");

            Console.ReadKey();
        }
        private static void GameDataSetting()
        {
            _player = new Character("Chad", "전사", 1, 01, 5, 100, 1500);
            _items = new Item[10];
            AddItem(new Item("무쇠갑옷", "무쇠로 만들어져 튼튼한 갑옷입니다.", 0, 0, 5, 0, 500));    //Enum?
            AddItem(new Item("낡은 검", "쉽게 볼 수 있는 낡은 검입니다.", 1, 2, 0, 0, 300));
        }

        static void AddItem(Item item)
        {
            if (Item.ItemCnt == 10) return;
            _items[Item.ItemCnt] = item;
            Item.ItemCnt++;
        }
    }
}