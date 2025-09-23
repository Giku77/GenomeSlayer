using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmorBinder : MonoBehaviour
{
    [Header("Player side")]
    public SkinnedMeshRenderer playerBodySMR;   // 플레이어 바디 SMR(기준용)

    [Header("Armor side")]
    public SkinnedMeshRenderer armorSMR;        // 갑옷 SMR(바꿔 끼울 대상)

    private void Start()
    {
        Bind();
    }


    static readonly Dictionary<string, string> Manual = new()
{
    {"PT_LeftUpArm",  "LeftUpperArm"},
    {"PT_LeftLowArm", "LeftLowerArm"},
    {"PT_RightUpArm", "RightUpperArm"},
    {"PT_RightLowArm","RightLowerArm"},
    {"PT_Left_BackCloth", "Spine"},
    {"PT_Left_BackCloth2","Spine"},
};
    //public void Bind()
    //{
    //    if (!playerBodySMR || !armorSMR) { Debug.LogWarning("Assign SMRs first."); return; }

    //    armorSMR.rootBone = playerBodySMR.rootBone;

    //    var map = playerBodySMR.bones.ToDictionary(b => b.name, b => b);
    //    var srcBones = armorSMR.bones; 
    //    var newBones = new Transform[srcBones.Length];

    //    for (int i = 0; i < srcBones.Length; i++)
    //    {
    //        var src = srcBones[i];
    //        var name = src ? src.name : "";
    //        if (!string.IsNullOrEmpty(name) && map.TryGetValue(name, out var dst))
    //            newBones[i] = dst;
    //        else
    //            newBones[i] = playerBodySMR.rootBone; // 매칭 실패 시 루트로 대체
    //    }

    //    for (int i = 0; i < srcBones.Length; i++)
    //    {
    //        var name = srcBones[i] ? srcBones[i].name : "<null>";
    //        if (!map.ContainsKey(name))
    //            Debug.LogWarning($"[ArmorBinder] Missing bone: {name} → mapped to root");
    //    }

    //    armorSMR.bones = newBones;

    //    Debug.Log("Armor bound to player bones.");
    //}
    public void Bind()
    {
        if (!playerBodySMR || !armorSMR) { Debug.LogWarning("Assign SMRs first."); return; }

        armorSMR.rootBone = playerBodySMR.rootBone;

        var map = playerBodySMR.bones.ToDictionary(b => b.name, b => b);
        var srcBones = armorSMR.bones;
        var newBones = new Transform[srcBones.Length];
        for (int i = 0; i < srcBones.Length; i++)
        {
            var name = srcBones[i] ? srcBones[i].name : "";
            if (!string.IsNullOrEmpty(name) && map.TryGetValue(name, out var dst))
                newBones[i] = dst;
            else if (Manual.TryGetValue(name, out var target) && map.TryGetValue(target, out var mapped))
                newBones[i] = mapped;
            else
                newBones[i] = playerBodySMR.rootBone;
        }
        armorSMR.bones = newBones;

        // 2) ★ bind pose 재계산(현재 포즈를 기준으로) ★
        //    mesh를 복제해서 수정(원본 공유메시를 직접 바꾸지 않도록)
        var mesh = Instantiate(armorSMR.sharedMesh);
        var newBindposes = new Matrix4x4[armorSMR.bones.Length];
        // SkinnedMeshRenderer는 bones[i].worldToLocalMatrix * renderer.localToWorldMatrix 를 bindpose로 사용
        for (int i = 0; i < newBindposes.Length; i++)
            newBindposes[i] = armorSMR.bones[i].worldToLocalMatrix * armorSMR.transform.localToWorldMatrix;

        mesh.bindposes = newBindposes;
        armorSMR.sharedMesh = mesh;

        armorSMR.quality = playerBodySMR.quality;
        armorSMR.updateWhenOffscreen = true; 
        armorSMR.transform.localScale = Vector3.one; 

        Debug.Log("Armor bound & bindposes recalculated.");
    }
}
