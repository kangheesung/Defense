using UnityEngine;
using Defense;

namespace Defense {
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapon Stats", order = 0)]
    public class WeaponStats : ScriptableObject {
        public string weaponName = "Rifle";
        public string weaponDesc = "";
        public weaponFireType weaponFireType = weaponFireType.Auto;
        public weaponType weaponType = weaponType.Projectile;
        public bool weaponMuzzle = false;
        public bool weaponLowerMount = false;
        public bool weaponSight = false;
        public bool weaponMagazine = true;

        [Header("Stats")]
        public float timeBetweenBullets = 0.15f;    //  연사 속도
        public int projectileCount = 1;             //  투사체 개수
        public int damagePerShot = 20;              //  데미지
        public float bulletSpeed = 1.0f;            //  탄환 속도
        public float bulletAcceleration = 0.0f;     //  탄환 가속도
        public float bulletTimeToLive = 3.0f;       //  탄환 시간(거리 영향)
        public int bulletCount = 30;                //  탄환 개수

        [Header("Spread")]
        public float minSpreadX = 0.1f;
        public float minSpreadY = 0.1f;
        public float maxSpreadX = 9f;
        public float maxSpreadY = 3f;
        public float deltaSpread = 3f;
        public float minusSpread = 6f;
        public float spreadTime = 0.2f;

        [Header("VFX")]
        public ParticleSystem gunParticle;
        public Light[] gunLight;
    }
}