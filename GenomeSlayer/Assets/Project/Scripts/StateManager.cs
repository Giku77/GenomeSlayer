using JetBrains.Annotations;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StateManager : MonoBehaviour
{
    public float DamageP { get; private set; } 
    public int Health { get; private set; }

    public StateDef StateDefData;

    public int GesPoint
    {
        get => StateDefData.GenomePoint;
        set
        {
            StateDefData.GenomePoint = value;
        }
    }

    private void Awake()
    {
        if (StateDefData.id.Length == 0)
        {
            var s = DataTableManger.GeTable.GetAllItems();
            StateDefData.id = new int[s.Count];
            StateDefData.lv = new int[s.Count];
        }
    }


    private int GetNeedPoint(GesPointData gs, int lv)
    {
        switch (lv)
        {
            case 1:
                return gs.genomePoint1;
            case 2:
                return gs.genomePoint2;
            case 3:
                return gs.genomePoint3;
            case 4:
                return gs.genomePoint4;
            case 5:
                return gs.genomePoint5;
        }
        return -1;
    }


    public void UpdateDamage(int id)
    {
        DamageP = DataTableManger.GeTable.GetItem(id).upgradeStatAmount;
    }

    public void UpdateLv(int index, TextMeshProUGUI point, GameObject StateUI, GameObject AcceptUI,TextMeshProUGUI w)
    {
        var currentLv = StateDefData.lv[index];
        var currentPoint = StateDefData.GenomePoint;
        var needPoint = GetNeedPoint(DataTableManger.GeTable.GetItem(StateDefData.id[index]), currentLv + 1);
        if (currentPoint < needPoint)
        {
            w.text = "포인트가 부족합니다.";
            return;
        }
        if (needPoint < 0)
        {
            w.text = "최대 레벨입니다.";
            return;
        }


        StateDefData.lv[index]++;
        StateDefData.GenomePoint -= DataTableManger.GeTable.GetItem(StateDefData.id[index]).genomePoint1;
        point.text = StateDefData.GenomePoint.ToString();
        var t = StateUI.GetComponentInChildren<GridLayoutGroup>().GetComponentsInChildren<TextMeshProUGUI>()[index];
        t.text = StateDefData.lv[index].ToString();
        AcceptUI.SetActive(false);
    }

}
