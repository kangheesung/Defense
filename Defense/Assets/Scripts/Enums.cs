using UnityEngine;
using Defense;

namespace Defense {
    /// <summary>
    /// 발사 타입(단발, 자동)
    /// </summary>
    public enum weaponFireType {
        Single,
        Auto
    }

    /// <summary>
    /// 무기 타입(종류)
    /// </summary>
    public enum Weapon {
        Knife,
        Pistol,
        Rifle,
        Machinegun,
        Rocketlauncher
    }

    /// <summary>
    /// 무기 타입(투사체)
    /// </summary>
    public enum weaponType {
        Projectile,
        Raycast,
        Collider
    }

    /// <summary>
    /// 무기 슬롯 타입(주, 보조)
    /// </summary>
    public enum weaponSlotType {
        Primary,
        Secondary
    }

    /// <summary>
    /// 아이템 타입
    /// </summary>
    public enum ItemType {
        InventorySlot,  //  인벤토리 용
        RestrictSlot,
        Helmet,
        Chest,
        Equipment,
        Backpack,
        Pant,
        Weapon,
        Handgun,
        Grenade,
        Consumption,
        Attachment
    }

    /// <summary>
    /// 부착물 타입(종류)
    /// </summary>
    public enum Attachments {
        Muzzle,
        LowerMount,
        Sight,
        Magazine
    }
}