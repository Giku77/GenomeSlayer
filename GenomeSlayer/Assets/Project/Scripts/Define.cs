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
    Durian_Seed = 1120001,
    Watermelon_Seed = 1120002,
    Pepper_Seed = 1120003,
    Coconut_Seed = 1120004,
}

public enum WeaponIds
{
    UNKNOWN_WEAPON = -1,
    Mace_Durian = 1010001,
    Katana_Pepper = 1010002,
    Bowling_Coconut = 1020003,
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
