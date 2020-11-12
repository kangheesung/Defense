using UnityEngine;
using UnityEditor;
using Defense;

[CustomEditor(typeof(WeaponStats))]
public class InspectorEditor_WeaponStats : Editor {
    public override void OnInspectorGUI() {
        WeaponStats ws = (WeaponStats)target;
        if (ws.weaponType == weaponType.Collider) {
            ws.weaponName = EditorGUILayout.TextField("weaponName", ws.weaponName);
            ws.weaponFireType = (weaponFireType)EditorGUILayout.EnumPopup("weaponFireType", ws.weaponFireType);
            ws.weaponType = (weaponType)EditorGUILayout.EnumPopup("weaponType", ws.weaponType);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stats");
            ws.timeBetweenBullets = EditorGUILayout.FloatField("timeBetweenBullets", ws.timeBetweenBullets);
            ws.damagePerShot = EditorGUILayout.IntField("damagePerShot", ws.damagePerShot);
        } else {
            DrawDefaultInspector();
        }
    }
}