using System.Collections.Generic;
using UnityEngine;

public static class DataTableManger
{
    //private static readonly string ItemCsv = "Items";
    private static readonly Dictionary<string, DataTable> tables =
        new Dictionary<string, DataTable>();

    static DataTableManger()
    {
        Init();
    }

    private static void Init()
    {
    //#if UNITY_EDITOR
    //        foreach (var id in DataTableIds.StringTableIds)
    //        {
    //            var table = new StringTable();
    //            table.Load(id);
    //            tables.Add(id, table);
    //        }
    //#else
    //        var stringTable = new StringTable();
    //        stringTable.Load(DataTableIds.String);
    //        tables.Add(DataTableIds.String, stringTable);
    //#endif

    var itemTable = new ItemTable();
    //itemTable.Load(DataTableIds.Item);
    itemTable.Load(ItemTable.ItemTableId);
    tables.Add(ItemTable.ItemTableId, itemTable);
    var buffTable = new BuffTable();
    buffTable.Load(BuffTable.BuffTableId);
    tables.Add(BuffTable.BuffTableId, buffTable);
    var plantTable = new PlantTable();
    plantTable.Load(PlantTable.PlantTableId);
    tables.Add(PlantTable.PlantTableId, plantTable);
    var equipmentTable = new EquipmentTable();
    equipmentTable.Load(EquipmentTable.EquipTableId);
    tables.Add(EquipmentTable.EquipTableId, equipmentTable);
    var gesTable = new GeTable();
    gesTable.Load(GeTable.GesTableId);
    tables.Add(GeTable.GesTableId, gesTable);
    var stringTable = new StringTable();
    stringTable.Load(StringTable.StrTableId);
    tables.Add(StringTable.StrTableId, stringTable);
    var breedTable = new BreedTable();
    breedTable.Load(BreedTable.BreedTableId);
    tables.Add(BreedTable.BreedTableId, breedTable);
    }

    //public static StringTable StringTable => Get<StringTable>(DataTableIds.String);
    public static ItemTable ItemTable => Get<ItemTable>(ItemTable.ItemTableId);

    public static EquipmentTable EquipmentTable => Get<EquipmentTable>(EquipmentTable.EquipTableId);
    public static BuffTable BuffTable => Get<BuffTable>(BuffTable.BuffTableId);

    public static PlantTable PlantTable => Get<PlantTable>(PlantTable.PlantTableId);
    public static GeTable GeTable => Get<GeTable>(GeTable.GesTableId);

    public static StringTable StringTable => Get<StringTable>(StringTable.StrTableId);

    public static BreedTable BreedTable => Get<BreedTable>(BreedTable.BreedTableId);

    public static T Get<T>(string id) where T : DataTable
    {
        if (!tables.ContainsKey(id))
        {
            Debug.LogError("테이블 없음");
            return null;
        }
        return tables[id] as T;
    }
}