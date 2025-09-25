using UnityEngine;

public enum ItemIds
{
    UNKNOWN_ITEM = -1,
    Mace_Durian = 1010001,
    Katana_Pepper = 1010002,
    Bowling_Coconut = 1020003,
    Armor_Watermelon = 1030004,
    Water = 1110001,
    Earthy_Fertilizer = 1110002,
    Mystery_Seed = 1120000,
    Durian_Seed = 1120001,
    Pepper_Seed = 1120002,
    Coconut_Seed = 1120003,
    Watermelon_Seed = 1120004,
}

public enum WeaponIds
{
    UNKNOWN_WEAPON = -1,
    Mace_Durian = 1010001,
    Katana_Pepper = 1010002,
    Bowling_Coconut = 1020003,
    Watermelon_Armor = 1030004,
}

public enum TreeIds
{
    UNKNOWN_TREE = -1,
    Durian_Tree = 1220001,
    Pepper_Tree = 1221002,
    Coconut_Tree = 1222003,
    Watermelon_Tree = 1224004,
}

public enum GenomIds
{
    // Player
    PlayerAttackUp = 1401000, // 플레이어 공격력 증가
    PlayerAttackSpeedUp = 1402000, // 플레이어 공격속도 증가
    PlayerMoveSpeedUp = 1403000, // 플레이어 이동 속도 증가

    // Weapons / Fruits
    MaceDurianAttackUp = 1401001, // 철퇴 두리안 공격력 증가
    KatanaPepperAttackUp = 1401002, // 카타나 페퍼 공격력 증가
    KatanaPepperAtkSpeedUp = 1402002, // 카타나 페퍼 공격 속도 증가
    BowlingCoconutAttackUp = 1401003, // 볼링 코코넛 공격력 증가
    BowlingCoconutAtkSpeedUp = 1402003, // 볼링 코코넛 공격 속도 증가
    ArmorWatermelonDefenseUp = 1404004, // 갑옷 수박 방어력 증가
    ArmorWatermelonMaxHpUp = 1405004, // 갑옷 수박 추가 체력 증가
}

public enum EnemyIds
{
    UNKNOWN_ENEMY = -1,
    MushroomMonster = 1301001, // 괴물버섯
    PoisonSpider = 1302002, // 독거미
    Bat = 1303003, // 박쥐
    Orc = 1304004, // 오크
}

public enum BuffIds
{
    PlayerAttackDamageUp20 = 1511001,
    PlayerAttackSpeedUp30 = 1512002,
    PlayerRegen1PerSec = 1514003,
    EnemyMoveSpeedDown25 = 1523004,
    EnemyHpDot5PerSec = 1534005,
}
