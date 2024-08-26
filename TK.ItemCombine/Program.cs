using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using TK.ItemCombine.Models;
using Z.BulkOperations;
using Z.Dapper.Plus;

#region CreateMainObject
List<FA3901_00101_MODEL>? FA3901 = new List<FA3901_00101_MODEL>();
#endregion

#region Dapper
var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory());
builder.AddJsonFile("appsettings.json", optional: false);
var configuration = builder.Build();
var connectionString = configuration.GetConnectionString("csbContext").ToString();
using (IDbConnection db = new OracleConnection(connectionString))
{
    // Получение списка заказов для объединения
    Console.WriteLine($"Создается слепок анализа 3901: {34001}");
    FA3901 = db.Query<FA3901_00101_MODEL>("SELECT FA3901_BS_NR, FA3901_POOL_NR, FA3901_FREIGABE, FA3176_TEXT_INHALT,FA078_VERS_ST " +
        "FROM tkmiratorg.FA3901_00101,TKMIRATORG.fa3176_00104,TKMIRATORG.FA0078_00112 " +
        "WHERE FA3901_POOL_NR = " + 34001 + " " +
        "AND FA3901_FREIGABE = 1 " +
        "AND FA3901_BS_NR between 127313 and 127343 " +
        "AND FA3901_BS_NR = FA3176_BS_NR AND FA3901_BS_SUB_NR = FA3176_BS_SUB_NR " +
        "AND FA3901_BS_NR = FA078_BS_NR AND FA3901_BS_SUB_NR = FA078_BS_SUB_NR").ToList();
    // -Получение списка заказов для объединения
    // Группировка по термическому состоянию
    var FA3901_GROP_BY_MAIN_KST = FA3901.GroupBy(x => x.FA078_VERS_ST);
    foreach (var item in FA3901_GROP_BY_MAIN_KST)
    {
        Console.WriteLine(item.Key);
        List<FA3901_00101_MODEL> new_3901 = new List<FA3901_00101_MODEL>();
        new_3901 = (from i in FA3901 where i.FA078_VERS_ST == item.Key select i).ToList();
        Console.WriteLine(new_3901);
        // -Группировка по термическому состоянию
        // Группировка артикулов в заказах по признаку артикул + дата
        var ids = new_3901.Select(x => x.FA3901_BS_NR);
        var query = "select FA077_BS_NR,FA077_BS_SUB_NR,FA077_NR,FA077_ART_NR, FA077_HBK_DATUM, FA077_KAT_GRP_NR from tkmiratorg.FA0077_00114 where FA077_BS_NR in ( ";
        foreach (var id in ids)
        {
            query = query + id + ", ";
        }
        query = query + " )";
        query = query.Replace(",  )", " )");
        var items = db.Query<FA0077_00114_MIN>(query);
        var items_group = items.GroupBy(x => new { x.FA077_ART_NR, x.FA077_HBK_DATUM, x.FA077_KAT_GRP_NR });
        List<FA3886_00101> FA3886_00101_ = new List<FA3886_00101>();
        int MAX_ID = db.Query<int>("SELECT max(FA3886_ART_KOMM_ID) FA3886_ART_KOMM_ID FROM tkmiratorg.FA3886_00101").First();
        int MAX_OBER_ID = db.Query<int>("SELECT max(FA3887_OBER_ID) FA3887_OBER_ID FROM tkmiratorg.FA3887_00102").First();
        int MAX_AUTO = db.Query<int>("SELECT max(FA3886_AUTO_INC) FA3886_ART_KOMM_ID FROM tkmiratorg.FA3886_00101").First();
        MAX_ID++;
        MAX_OBER_ID++;
        foreach (var id in items_group)
        {
            foreach (var ed in id)
            {
                var OBER_ID = db.Query<FA3887_00102>("select * from TKMIRATORG.FA3887_00102 WHERE FA3887_AUSW_NR = 34001 AND FA3887_ART_KOMM_ID = " + MAX_ID).FirstOrDefault();
                if (OBER_ID == null)
                {
                    FA3887_00102 FA3887 = new FA3887_00102();
                    FA3887.FA3887_PROG_NR = 3901;
                    FA3887.FA3887_AUSW_NR = 34001;
                    FA3887.FA3887_OBER_ID = MAX_OBER_ID;
                    FA3887.FA3887_ART_KOMM_ID = MAX_ID;
                    FA3887.FA3887_PRIO = 100;
                    FA3887.FA3887_ZUOR_TYP = 0;
                    FA3887.FA3887_ZUORDNUNG = 8900;
                    FA3887.FA3887_ANL_DATUM = 20240826;
                    FA3887.FA3887_ANL_ZEIT = 114400;
                    FA3887.FA3887_ANL_USER = 8900;
                    FA3887.FA3887_ANL_STATION = 18;
                    FA3887.FA3887_ANL_PROG = 3901;
                    FA3887.FA3887_ANL_FKT = 0;
                    FA3887.FA3887_UPD_DATUM = 20240826;
                    FA3887.FA3887_UPD_ZEIT = 114400;
                    FA3887.FA3887_UPD_USER = 8900;
                    FA3887.FA3887_UPD_STATION = 18;
                    FA3887.FA3887_UPD_PROG = 3886;
                    FA3887.FA3887_UPD_FKT = 0;
                    FA3887.FA3887_FREIGABE = 1;
                    FA3887.FA3887_STAT_IN_BEARB = 0;
                    FA3887.FA3887_X_FELD_1 = " ";
                    FA3887.FA3887_SORTER_LINIE = 0;

                    FA3887.FA3887_BEARBEITET = 0;
                    FA3887.FA3887_PAZ = 0;
                    FA3887.FA3887_ANZ_ART = 0;
                    FA3887.FA3887_VOLL_ZUORD_TYP = 0;
                    FA3887.FA3887_VOLL_ZUORDNUNG = 0;
                    FA3887.FA3887_TEIL_ZUORD_TYP = 0;
                    FA3887.FA3887_TEIL_ZUORDNUNG = 0;
                    FA3887.FA3887_X_FELD_2 = " ";
                    db.BulkInsert(FA3887);
                }
                FA3886_00101 new_id = new FA3886_00101();
                new_id.FA3886_PROG_NR = 3901;
                new_id.FA3886_AUSW_NR = 34001;
                new_id.FA3886_PRIO = 100;
                new_id.FA3886_ART_KOMM_ID = MAX_ID;
                new_id.FA3886_SORT_NR = 1000;
                new_id.FA3886_ART_NR = ed.FA077_ART_NR;
                new_id.FA3886_SORT_NR_2 = 1000;
                new_id.FA3886_BS_NR = ed.FA077_BS_NR;
                new_id.FA3886_BS_SUB_NR = ed.FA077_BS_SUB_NR;
                new_id.FA3886_TYP = 30;
                new_id.FA3886_POSTEN_ID = ed.FA077_NR;
                new_id.FA3886_ZUORD_TYP = 0;
                new_id.FA3886_ZUORDNUNG = 0;
                new_id.FA3886_RELEASE = 0;
                new_id.FA3886_ANL_DATUM = 20240826;
                new_id.FA3886_ANL_ZEIT = 113400;
                new_id.FA3886_ANL_USER = 8900;
                new_id.FA3886_ANL_PROG = 3901;
                new_id.FA3886_ANL_FKT = 0;
                new_id.FA3886_UPD_DATUM = 20240826;
                new_id.FA3886_UPD_ZEIT = 113400;
                new_id.FA3886_UPD_USER = 8900;
                new_id.FA3886_UPD_FKT = 0;
                new_id.FA3886_UPD_PROG = 3886;
                new_id.FA3886_FREIGABE = 1;
                new_id.FA3886_STAT_IN_BEARB = 0;
                new_id.FA3886_BEARB_LFD_NR = 0;
                new_id.FA3886_FREIE_NR_1 = ed.FA077_BS_NR;
                MAX_AUTO++;
                new_id.FA3886_AUTO_INC = MAX_AUTO;
                FA3886_00101_.Add(new_id);
            }
            MAX_ID++;
        }        
        db.BulkInsert(FA3886_00101_);
        Console.WriteLine(MAX_ID);
    }
}
#endregion