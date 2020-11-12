using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Defense;

public class PlayerWeaponManager : MonoBehaviour {
    public GameObject Grenade;
    public Transform Knife;
    public Transform[] Handgun;
    public Transform[] Infantry;
    public Transform[] Machinegun;
    public Transform[] RocketLauncher;
    
    //  무기의 clone을 담을 리스트
    [HideInInspector]
    public List<Transform> HandgunL;
    [HideInInspector]
    public List<Transform> InfantryL;
    [HideInInspector]
    public List<Transform> MachinegunL;
    [HideInInspector]
    public List<Transform> RocketLauncherL;

    //  무기의 위치
    public Transform Grenadeoffset;
    public Transform KnifeOffset;
    public Transform HandgunOffset;
    public Transform InfantryOffset;
    public Transform MachinegunOffset;
    public Transform RocketLauncherOffset;

    //  무기 휴대 중 위치
    public Transform frontGunCarry; //  앞 장비 있을 때
    public Transform backGunCarry;  //  뒤 장비 있을 때

    public Transform frontGunCarryEquipNone;    //  앞 장비 없을 때
    public Transform backGunCarryEquipNone; //  뒤 장비 없을 때

    public Transform backpackGunCarry;  //  뒤 가방 있을 때

    private int weaponSlingNum;   //  무기 휴대 위치를 설정할 변수 

    private Animator anim;

    public Weapon curWeaponType;    //  현재 무기 타입
    public Transform curWeaponTransform;    //  현재 무기 Transform

    [HideInInspector]
    public bool Changing;   //  무기 변경 중

    private int num;    //  무기 코드를 설정할 때 증가 될 인덱스 값을 저장 (데이터 베이스에 순서대로 저장하기 위해 전역 변수 사용)

    public Transform remainWeaponActive;    //  활성화 무기
    public Transform remainWeaponDisable;   //  비활성화 무기

    private InputManager im;
    private PlayerMovement pm;
    private PlayerIk pik;
    private EquipmentManager em;

    private void Awake() {
        HandgunL = new List<Transform>();
        InfantryL = new List<Transform>();
        MachinegunL = new List<Transform>();
        RocketLauncherL = new List<Transform>();

        im = FindObjectOfType<InputManager>();
        pm = FindObjectOfType<PlayerMovement>();
        pik = FindObjectOfType<PlayerIk>();
        em = FindObjectOfType<EquipmentManager>();

        anim = GetComponent<Animator>();

        InitWeapon(Handgun, HandgunL, HandgunOffset, ItemType.Handgun);
        InitWeapon(Infantry, InfantryL, InfantryOffset, ItemType.Weapon);
        InitWeapon(Machinegun, MachinegunL, MachinegunOffset, ItemType.Weapon);
        InitWeapon(RocketLauncher, RocketLauncherL, RocketLauncherOffset, ItemType.Weapon);
    }

    private void Start() {
        Knife.GetComponent<PlayerWeapon>().weaponCode = -1;

        remainWeaponDisable = Knife;

        //  1번 2번 슬롯에 무기가 없다면 현재 무기를 Knife로 바꿈
        if (Inventory.instance.equipmentSlots[0].GetChild(0).GetComponent<Slot>().item.itemValue == 0 && Inventory.instance.equipmentSlots[1].GetChild(0).GetComponent<Slot>().item.itemValue == 0) {
            curWeaponTransform = Knife;
            drawKnife();
        }
    }

    private void InitWeapon(Transform[] weapon, List<Transform> weaponL, Transform weaponOffset, ItemType itemtype) {
        //  무기 리스트의 정보를 데이터 베이스에 추가
        string name;
        string desc;
        GameObject prefab;
        GameObject uiPrefab;
        for (int i = 0; i < weapon.Length; i++) {   //  무기 리스트에 무기 추가(Transform 참조)
            weaponL.Add(weapon[i]);
        }
        for (int i = 0; i < weapon.Length; i++) {
            weapon[i].GetComponent<PlayerWeapon>().weaponCode = num + ItemDatabase.instance.itemlength;
            name = weapon[i].GetComponent<PlayerWeapon>().weaponStats.weaponName;
            desc = weapon[i].GetComponent<PlayerWeapon>().weaponStats.weaponDesc;
            prefab = weapon[i].GetComponent<PlayerWeapon>().weaponPrefab;
            uiPrefab = weapon[i].GetComponent<PlayerWeapon>().weaponUIPrefab;
            if (prefab != null && uiPrefab != null) {
                ItemDatabase.instance.Add(name, 1, num + ItemDatabase.instance.itemlength, desc, itemtype, Attachments.LowerMount, 1, prefab, uiPrefab);
            } else {
                ItemDatabase.instance.Add(name, 1, num + ItemDatabase.instance.itemlength, desc, itemtype, Attachments.LowerMount, 1, null, Inventory.instance.dummyPrefab);
            }
            Transform tempWeapon = Instantiate(weapon[i], weaponOffset);
            tempWeapon.GetComponent<PlayerWeapon>().weaponClone = true;
            weaponL.Add(tempWeapon);

            num++;
        }
    }

    private void changingStart() {  //  무기 교체 시작시 상태 설정
        curWeaponTransform.GetComponent<PlayerWeapon>().DisableShootingInput = true;    //  사격 비활성화
        pm.sprintDisable = true;    //  달리기 비활성화
        pik.ikActive = false;   //  애니메이션 저장을 위해 ik비활성화
        Changing = true;    //  무기 변경 중
    }

    public Transform returnWeaponTransform(int code, bool clone) {  //  입력받은 코드를 해당 무기의 Transform으로 반환 (clone값에 맞는 Transform을 반환)
        //  입력 받은 코드의 아이템 타입이 무기가 아니라면 null 리턴
        if (ItemDatabase.instance.items[code].itemType != ItemType.Weapon &&
            ItemDatabase.instance.items[code].itemType != ItemType.Handgun
            ) {
            return null;
        }
        for (int i = 0; i < InfantryL.Count; i++) {
            if (InfantryL[i].GetComponent<PlayerWeapon>().weaponCode == code) {
                if (InfantryL[i].GetComponent<PlayerWeapon>().weaponClone == clone) {
                    return InfantryL[i];
                }
            }
        }
        for (int i = 0; i < MachinegunL.Count; i++) {
            if (MachinegunL[i].GetComponent<PlayerWeapon>().weaponCode == code) {
                if (MachinegunL[i].GetComponent<PlayerWeapon>().weaponClone == clone) {
                    return MachinegunL[i];
                }
            }
        }
        for (int i = 0; i < RocketLauncherL.Count; i++) {
            if (RocketLauncherL[i].GetComponent<PlayerWeapon>().weaponCode == code) {
                if (RocketLauncherL[i].GetComponent<PlayerWeapon>().weaponClone == clone) {
                    return RocketLauncherL[i];
                }
            }
        }
        for (int i = 0; i < HandgunL.Count; i++) {
            if (HandgunL[i].GetComponent<PlayerWeapon>().weaponCode == code) {
                return HandgunL[i];
            }
        }
        //  code에 해당하는 Transform이 없다면 null 리턴
        return null;
    }

    private void throwGrenade() {
        GameObject tempGrenade = Instantiate(Grenade, Grenadeoffset);
        Grenadeoffset.LookAt(pm.Aim.transform.position);
        tempGrenade.GetComponent<Collider>().isTrigger = false;
        Grenadeoffset.transform.DetachChildren();
        Vector3 grenadeForce = Grenadeoffset.forward * 2000 + Vector3.up * 3000;
        float targetDistance = Vector3.Distance(pm.Aim.transform.position, Grenadeoffset.position);//transform.position);
        Vector3 finalGrenadeForce = grenadeForce * (targetDistance / 20.0f);
        float MaxForce = 1000;
        finalGrenadeForce.x = Mathf.Clamp(finalGrenadeForce.x, -MaxForce, MaxForce);
        finalGrenadeForce.y = Mathf.Clamp(finalGrenadeForce.y, -MaxForce, MaxForce);
        finalGrenadeForce.z = Mathf.Clamp(finalGrenadeForce.z, -MaxForce, MaxForce);
        Debug.Log(finalGrenadeForce);
        tempGrenade.GetComponent<Rigidbody>().AddForce(finalGrenadeForce);
        tempGrenade.GetComponent<Rigidbody>().AddRelativeTorque(Grenadeoffset.forward * 50f, ForceMode.Impulse);
        pm.aimRotate = false;
    }

    private void Update() {
        //  DisableShootingInput이라면 사격 입력을 멈춤
        if (curWeaponTransform.GetComponent<PlayerWeapon>().DisableShootingInput) {
            return;
        }
        //  Grenade
        for (int i = 0; i < im.Grenade.Length; i++) {
            if (Input.GetKeyDown(im.Grenade[i]) && !Changing) {
                if (!anim.GetBool("sprint")) {
                    anim.SetTrigger("grenade");
                    pm.aimRotate = true;    //  aim으로 player회전
                    curWeaponTransform.GetComponent<PlayerWeapon>().shootingActive(false);
                    Invoke("throwGrenade", 0.65f);
                }
                break;
            }
        }
        //  1번
        if (Input.GetKeyDown(KeyCode.Alpha1) && !Changing) {    //  1번을 눌렀고 교환중이 아니라면
            //  1번이 선택되었는데 1번이 눌린거라면 리턴
            if (curWeaponTransform != null) {
                if (curWeaponTransform.GetComponent<PlayerWeapon>().weaponPrefab == Inventory.instance.equipmentSlots[0].GetChild(0).GetComponent<Slot>().item.itemPrefab) {
                    if (!curWeaponTransform.GetComponent<PlayerWeapon>().weaponClone) { //  clone유무를 확인해 같은 무기일 때 리턴되어 버그가 걸리는걸 막음
                        return;
                    }
                }
            }
            WeaponActChk(); //  활성화된 무기를 확인하여 비활성화할 무기로 설정
            int weaponCode = Inventory.instance.equipmentSlots[0].GetChild(0).GetComponent<Slot>().item.itemCode;   //  주무기 슬롯의 아이템 코드
            weaponSlingNum = 0;
            for (int i = 0; i < InfantryL.Count; i++) {
                if (InfantryL[i].GetComponent<PlayerWeapon>().weaponCode == weaponCode &&
                    InfantryL[i].GetComponent<PlayerWeapon>().weaponClone == false
                    ) {
                    remainWeaponActive = InfantryL[i];  //  활성화 무기 변경
                    curWeaponAnimChange(Weapon.Rifle);  //  무기 애니메이션 변경
                    changingStart();    //  변경 시작 설정
                    return;
                }
            }
            for (int i = 0; i < MachinegunL.Count; i++) {
                if (MachinegunL[i].GetComponent<PlayerWeapon>().weaponCode == weaponCode &&
                    MachinegunL[i].GetComponent<PlayerWeapon>().weaponClone == false
                    ) {
                    remainWeaponActive = MachinegunL[i];
                    curWeaponAnimChange(Weapon.Machinegun);
                    changingStart();
                    return;
                }
            }
            for (int i = 0; i < RocketLauncherL.Count; i++) {
                if (RocketLauncherL[i].GetComponent<PlayerWeapon>().weaponCode == weaponCode &&
                    RocketLauncherL[i].GetComponent<PlayerWeapon>().weaponClone == false
                    ) {
                    remainWeaponActive = RocketLauncherL[i];
                    curWeaponAnimChange(Weapon.Rocketlauncher);
                    changingStart();
                    return;
                }
            }
            if (remainWeaponDisable != Knife) {
                drawKnife();
            }
        }
        //  2번
        if (Input.GetKeyDown(KeyCode.Alpha2) && !Changing) {
            if (curWeaponTransform != null) {
                if (curWeaponTransform.GetComponent<PlayerWeapon>().weaponPrefab == Inventory.instance.equipmentSlots[1].GetChild(0).GetComponent<Slot>().item.itemPrefab) {
                    if (curWeaponTransform.GetComponent<PlayerWeapon>().weaponClone) {
                        return;
                    }
                }
            }
            WeaponActChk();
            int weaponCode = Inventory.instance.equipmentSlots[1].GetChild(0).GetComponent<Slot>().item.itemCode;
            weaponSlingNum = 1;
            for (int i = 0; i < InfantryL.Count; i++) {
                if (InfantryL[i].GetComponent<PlayerWeapon>().weaponCode == weaponCode &&
                    InfantryL[i].GetComponent<PlayerWeapon>().weaponClone == true
                    ) {
                    remainWeaponActive = InfantryL[i];
                    curWeaponAnimChange(Weapon.Rifle);
                    changingStart();
                    return;
                }
            }
            for (int i = 0; i < MachinegunL.Count; i++) {
                if (MachinegunL[i].GetComponent<PlayerWeapon>().weaponCode == weaponCode &&
                    MachinegunL[i].GetComponent<PlayerWeapon>().weaponClone == true
                    ) {
                    remainWeaponActive = MachinegunL[i];
                    curWeaponAnimChange(Weapon.Machinegun);
                    changingStart();
                    return;
                }
            }
            for (int i = 0; i < RocketLauncherL.Count; i++) {
                if (RocketLauncherL[i].GetComponent<PlayerWeapon>().weaponCode == weaponCode &&
                    RocketLauncherL[i].GetComponent<PlayerWeapon>().weaponClone == true
                    ) {
                    remainWeaponActive = RocketLauncherL[i];
                    curWeaponAnimChange(Weapon.Rocketlauncher);
                    changingStart();
                    return;
                }
            }
            if (remainWeaponDisable != Knife) {
                drawKnife();
            }
        }
        //  3번
        if (Input.GetKeyDown(KeyCode.Alpha3) && !Changing) {
            if (curWeaponTransform != null) {
                if (curWeaponTransform.GetComponent<PlayerWeapon>().weaponPrefab == Inventory.instance.equipmentSlots[2].GetChild(0).GetComponent<Slot>().item.itemPrefab) return;
            }
            WeaponActChk();
            int weaponCode = Inventory.instance.equipmentSlots[2].GetChild(0).GetComponent<Slot>().item.itemCode;
            weaponSlingNum = 2;
            for (int i = 0; i < HandgunL.Count; i++) {
                if (HandgunL[i].GetComponent<PlayerWeapon>().weaponCode == weaponCode) {
                    remainWeaponActive = HandgunL[i];
                    curWeaponAnimChange(Weapon.Pistol);
                    changingStart();
                    return;
                }
            }
            if (remainWeaponDisable != Knife) {
                drawKnife();
            }
        }
        //  4번
        if (Input.GetKeyDown(KeyCode.Alpha4) && !Changing) {
            if (curWeaponTransform != null) {
                if (curWeaponTransform == Knife) {
                    return;
                }
            }
            WeaponActChk();
            if (weaponSlingNum == 0) {
                weaponSlingNum = 1;
            } else if (weaponSlingNum == 1) {
                weaponSlingNum = 0;
            }
            drawKnife();
            return;
        }
    }

    private void drawKnife() {
        remainWeaponActive = Knife;
        curWeaponAnimChange(Weapon.Knife);
        changingStart();
    }

    public void slingWeaponDisable() {  //  sling 중인 무기 모두 초기화(원 위치)
        Weapon tempWeaponType;
        for (int i = 0; i < frontGunCarry.childCount; i++) {
            tempWeaponType = frontGunCarry.GetChild(i).GetComponent<PlayerWeapon>().weapon; //  무기 타입 설정
            frontGunCarry.GetChild(i).gameObject.SetActive(false);  //  비활성화
            weaponSetOffset(tempWeaponType, frontGunCarry.GetChild(i)); //  무기 오프셋 변경
        }
        for (int i = 0; i < backGunCarry.childCount; i++) {
            tempWeaponType = backGunCarry.GetChild(i).GetComponent<PlayerWeapon>().weapon;
            backGunCarry.GetChild(i).gameObject.SetActive(false);
            weaponSetOffset(tempWeaponType, backGunCarry.GetChild(i));
        }
        for (int i = 0; i < frontGunCarryEquipNone.childCount; i++) {
            tempWeaponType = frontGunCarryEquipNone.GetChild(i).GetComponent<PlayerWeapon>().weapon;
            frontGunCarryEquipNone.GetChild(i).gameObject.SetActive(false);
            weaponSetOffset(tempWeaponType, frontGunCarryEquipNone.GetChild(i));
        }
        for (int i = 0; i < backGunCarryEquipNone.childCount; i++) {
            tempWeaponType = backGunCarryEquipNone.GetChild(i).GetComponent<PlayerWeapon>().weapon;
            backGunCarryEquipNone.GetChild(i).gameObject.SetActive(false);
            weaponSetOffset(tempWeaponType, backGunCarryEquipNone.GetChild(i));
        }
        for (int i = 0; i < backpackGunCarry.childCount; i++) {
            tempWeaponType = backpackGunCarry.GetChild(i).GetComponent<PlayerWeapon>().weapon;
            backpackGunCarry.GetChild(i).gameObject.SetActive(false);
            weaponSetOffset(tempWeaponType, backpackGunCarry.GetChild(i));
        }
    }

    private void weaponSetOffset(Weapon weapon, Transform transform) {  //  transform을 weapon 타입에 맞는 오프셋으로 이동
        switch (weapon) {
            case Weapon.Knife:
                transform.SetParent(KnifeOffset);
                break;
            case Weapon.Pistol:
                transform.SetParent(HandgunOffset);
                break;
            case Weapon.Rifle:
                transform.SetParent(InfantryOffset);
                break;
            case Weapon.Machinegun:
                transform.SetParent(MachinegunOffset);
                break;
            case Weapon.Rocketlauncher:
                transform.SetParent(RocketLauncherOffset);
                break;
        }
    }

    private void WeaponActChk() {   //  활성화된 무기를 확인하여 remainWeaponDisable로 설정
        if (Knife.gameObject.activeSelf == true) {
            if (!Knife.GetComponent<PlayerWeapon>().weaponSling) {
                remainWeaponDisable = Knife;
                return;
            }
        }
        for (int i = 0; i < HandgunL.Count; i++) {
            if (HandgunL[i].gameObject.activeSelf == true) {
                if (!HandgunL[i].GetComponent<PlayerWeapon>().weaponSling) {
                    remainWeaponDisable = HandgunL[i];
                    return;
                }
            }
        }
        for (int i = 0; i < InfantryL.Count; i++) {
            if (InfantryL[i].gameObject.activeSelf == true) {
                if (!InfantryL[i].GetComponent<PlayerWeapon>().weaponSling) {
                    remainWeaponDisable = InfantryL[i];
                    return;
                }
            }
        }
        for (int i = 0; i < MachinegunL.Count; i++) {
            if (MachinegunL[i].gameObject.activeSelf == true) {
                if (!MachinegunL[i].GetComponent<PlayerWeapon>().weaponSling) {
                    remainWeaponDisable = MachinegunL[i];
                    return;
                }
            }
        }
        for (int i = 0; i < RocketLauncherL.Count; i++) {
            if (RocketLauncherL[i].gameObject.activeSelf == true) {
                if (!RocketLauncherL[i].GetComponent<PlayerWeapon>().weaponSling) {
                    remainWeaponDisable = RocketLauncherL[i];
                    return;
                }
            }
        }
    }

    private void curWeaponAnimChange(Weapon cw) {   //  현재 무기를 집어넣는 애니메이션을 실행하고 무기 애니메이션 변경

        //  현재 무기 타입에 맞는 애니메이션 트리거 실행(무기 집어넣는 애니메이션)
        switch (curWeaponType) {
            case Weapon.Knife:
                anim.SetTrigger("PutKnife");
                break;
            case Weapon.Pistol:
                anim.SetTrigger("PutPistol");
                break;
            case Weapon.Rifle:
                anim.SetTrigger("PutRifle");
                break;
            case Weapon.Machinegun:
                anim.SetTrigger("PutMachinegun");
                break;
            case Weapon.Rocketlauncher:
                anim.SetTrigger("PutRocketlauncher");
                break;
        }

        //  무기 애니메이션 초기화
        anim.SetBool("Knife", false);
        anim.SetBool("Pistol", false);
        anim.SetBool("Rifle", false);
        anim.SetBool("Machinegun", false);
        anim.SetBool("Rocketlauncher", false);

        //  바꿀 무기로 애니메이션 변경 (cw : changeweapon)
        switch (cw) {
            case Weapon.Knife:
                anim.SetBool("Knife", true);
                break;
            case Weapon.Pistol:
                anim.SetBool("Pistol", true);
                break;
            case Weapon.Rifle:
                anim.SetBool("Rifle", true);
                break;
            case Weapon.Machinegun:
                anim.SetBool("Machinegun", true);
                break;
            case Weapon.Rocketlauncher:
                anim.SetBool("Rocketlauncher", true);
                break;
        }
        curWeaponType = cw; //  현재 무기를 cw로 설정
    }

    public void curWeaponDropChk() {    //  무기가 드랍된걸 확인하여 drawKnife
        if (curWeaponTransform != null) {
            for (int i = 0; i < HandgunL.Count; i++) {  //  handgun일경우 따로 확인하여 비활성화
                if (curWeaponTransform == HandgunL[i]) {
                    HandgunL[i].gameObject.SetActive(false);
                    break;
                }
            }
            if (!curWeaponTransform.gameObject.activeSelf) {
                drawKnife();
            }
        }
    }

    public void setSling(int code, int slot) {  //  슬링 설정
        //  code == -1 : pickup, code != -1 drop
        Transform tempWeapon = null;
        int weaponCode;

        if (code == -1)
            weaponCode = Inventory.instance.equipmentSlots[slot].GetChild(0).GetComponent<Slot>().item.itemCode;
        else
            weaponCode = code;

        //  같은 무기를 들고있을 때 sling 설정이 안되는걸 방지
        if (curWeaponTransform != null) {
            if (code == -1) {
                if (Inventory.instance.equipmentSlots[slot].GetChild(0).GetComponent<Slot>().item.itemPrefab == curWeaponTransform.GetComponent<PlayerWeapon>().weaponPrefab) {
                    if (slot == 0) {
                        if (!curWeaponTransform.GetComponent<PlayerWeapon>().weaponClone) {
                            return;
                        }
                    } else {
                        if (curWeaponTransform.GetComponent<PlayerWeapon>().weaponClone) {
                            return;
                        }
                    }
                }
            }
        }
        if (weaponCode == -1) {
            return;
        }
        if (Inventory.instance.equipmentSlots[slot].GetChild(0).GetComponent<Slot>().item.itemValue == 1) { //  설정할려는 슬롯에 아이템이 존재한다면
            if (slot == 0) {    //  주무기 슬롯이라면
                tempWeapon = returnWeaponTransform(weaponCode, false);  //  무기 코드를 Transform으로 변환
                if (em.curEquipment == null) {  //  현재 Equipment가 없다면
                    tempWeapon.SetParent(frontGunCarryEquipNone);
                } else {
                    tempWeapon.SetParent(frontGunCarry);
                }
            } else {    //  보조무기 슬롯이라면
                tempWeapon = returnWeaponTransform(weaponCode, true);
                if (em.curBackpack != null) {   //  Backpack이 없다면
                    tempWeapon.SetParent(backpackGunCarry);
                } else if (em.curEquipment == null) {   //  Equipment가 없다면
                    tempWeapon.SetParent(backGunCarryEquipNone);
                } else {    //  아무것도 해당이 없다면
                    tempWeapon.SetParent(backGunCarry);
                }
            }
            if (tempWeapon != null) {
                tempWeapon.gameObject.SetActive(true);  //  무기 활성화
            }
        } else {    //  설정하려는 슬롯에 아이템이 존재하지 않다면
            if (slot == 0) {
                tempWeapon = returnWeaponTransform(weaponCode, false);  //  무기 코드를 Transform으로 변환
            } else {
                tempWeapon = returnWeaponTransform(weaponCode, true);
            }

            //  해당 오프셋으로 tempWeapon부모 변경
            weaponSetOffset(tempWeapon.GetComponent<PlayerWeapon>().weapon, tempWeapon);
            if (tempWeapon != null) {
                tempWeapon.gameObject.SetActive(false); //  무기 비활성화
            }
        }
        if (tempWeapon != null) {   //  오프셋으로 부모가 바뀐 오브젝트 이동을 위해 위치 초기화
            tempWeapon.GetComponent<PlayerWeapon>().weaponSling = true; //  sling 상태로 설정
            tempWeapon.localPosition = new Vector3(0, 0, 0);
            tempWeapon.localRotation = Quaternion.identity;
        }
    }

    public void animationDrawStart() {
        Weapon weapon;

        weapon = remainWeaponActive.GetComponent<PlayerWeapon>().weapon;    //  활성화할 무기의 타입

        weaponSetOffset(weapon, remainWeaponActive);    //  무기 오프셋을 활성화할 무기로 설정

        //  오프셋으로 부모가 바뀐 오브젝트 이동을 위해 위치 초기화
        remainWeaponActive.localPosition = new Vector3(0, 0, 0);
        remainWeaponActive.localRotation = Quaternion.identity;

        remainWeaponActive.GetComponent<PlayerWeapon>().weaponSling = false;    //  활성화할 무기의 sling 상태를 해제
        remainWeaponActive.gameObject.SetActive(true);  //  활성화할 무기 활성화

        if (remainWeaponDisable != remainWeaponActive) {
            switch (weaponSlingNum) {
                case 0:
                    if (em.curBackpack != null) {   //  backpack이 있다면
                        remainWeaponDisable.SetParent(backpackGunCarry);
                    }
                    else if (em.curEquipment == null) { //  Equipment가 없다면
                        remainWeaponDisable.SetParent(backGunCarryEquipNone);
                    } else {    //  아무것도 해당이 없다면
                        remainWeaponDisable.SetParent(backGunCarry);
                    }
                    break;
                case 1:
                    if (em.curEquipment == null) {  //  Equipment가 없다면
                        remainWeaponDisable.SetParent(frontGunCarryEquipNone);
                    } else {    //  Equipment가 있다면
                        remainWeaponDisable.SetParent(frontGunCarry);
                    }
                    break;
                case 2:
                    //  1번 슬롯
                    if (Inventory.instance.equipmentSlots[0].GetChild(0).GetComponent<Slot>().item.itemCode == remainWeaponDisable.GetComponent<PlayerWeapon>().weaponCode && !remainWeaponDisable.GetComponent<PlayerWeapon>().weaponClone) {
                        if (em.curEquipment == null) {  //  Equipment가 없다면
                            remainWeaponDisable.SetParent(frontGunCarryEquipNone);
                        } else {    //  Equipment가 있다면
                            remainWeaponDisable.SetParent(frontGunCarry);
                        }
                    }
                    //  2번 슬롯
                    else if (Inventory.instance.equipmentSlots[1].GetChild(0).GetComponent<Slot>().item.itemCode == remainWeaponDisable.GetComponent<PlayerWeapon>().weaponCode && remainWeaponDisable.GetComponent<PlayerWeapon>().weaponClone) {
                        if (em.curBackpack != null) {   //  Backpack이 있다면
                            remainWeaponDisable.SetParent(backpackGunCarry);
                        } else if (em.curEquipment == null) {   //  Equipment가 없다면
                            remainWeaponDisable.SetParent(backGunCarryEquipNone);
                        } else {    //  아무것도 해당이 없다면
                            remainWeaponDisable.SetParent(backGunCarry);
                        }
                    }
                    break;
            }
        }

        weapon = remainWeaponDisable.GetComponent<PlayerWeapon>().weapon;   //  비 활성화할 무기의 타입
        switch(weapon) {
            case Weapon.Knife:
                if (remainWeaponActive.GetComponent<PlayerWeapon>().weapon == Weapon.Knife) {   //  활성화할 무기가 Knife라면
                    remainWeaponDisable.SetParent(KnifeOffset);
                    remainWeaponDisable.gameObject.SetActive(true);
                } else {
                    remainWeaponDisable.gameObject.SetActive(false);
                }
                break;
            case Weapon.Pistol:
                remainWeaponDisable.gameObject.SetActive(false);
                break;
            case Weapon.Rifle:
                remainWeaponDisable.gameObject.SetActive(true);
                break;
            case Weapon.Machinegun:
                remainWeaponDisable.gameObject.SetActive(true);
                break;
            case Weapon.Rocketlauncher:
                remainWeaponDisable.gameObject.SetActive(true);
                break;
        }

        //  오프셋으로 부모가 바뀐 오브젝트 이동을 위해 위치 초기화
        remainWeaponDisable.localPosition = new Vector3(0, 0, 0);
        remainWeaponDisable.localRotation = Quaternion.identity;

        remainWeaponDisable.GetComponent<PlayerWeapon>().weaponSling = true; //  비활성화할 무기의 sling 상태를 설정

        Knife.GetComponent<PlayerWeapon>().weaponSling = false; //  Knife의 sling상태를 비활성화

        curWeaponTransform = remainWeaponActive;    //  현재 무기를 활성화할 무기로 설정

        //  애니메이션 트리거 리셋
        anim.ResetTrigger("PutKnife");
        anim.ResetTrigger("PutPistol");
        anim.ResetTrigger("PutRifle");
        anim.ResetTrigger("PutMachinegun");
        anim.ResetTrigger("PutRocketlauncher");
    }

    public void animationDrawEnd() {
        PlayerWeapon temppw = curWeaponTransform.GetComponent<PlayerWeapon>();  //  현재 무기의 PlayerWeapon 참조
        if (temppw.weaponIkOffset != null) {    //  오프셋이 있다면
            pik.ikChange(temppw.weaponIkOffset);    //  ik를 오프셋으로 설정
        } else {
            pik.ikChange(temppw.weapon);    //  ik를 무기 타입의 오프셋으로 설정
        }
        //  제한들을 다시 비활성화, 애니메이션 리셋
        temppw.DisableShootingInput = false;
        pm.sprintDisable = false;
        Changing = false;
        anim.ResetTrigger("PutKnife");
        anim.ResetTrigger("PutPistol");
        anim.ResetTrigger("PutRifle");
        anim.ResetTrigger("PutMachinegun");
        anim.ResetTrigger("PutRocketlauncher");
    }
}