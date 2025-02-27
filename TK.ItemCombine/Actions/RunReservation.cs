using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TK.ItemCombine.Data;
using TK.ItemCombine.Models;
using Z.Dapper.Plus;

namespace TK.ItemCombine.Actions
{
    public class RunReservation : IDisposable
    {
        public RunReservation() { }
        public async Task WriteToFile(string path,string text)
        {
            await File.AppendAllTextAsync(path, text);
        }

        public async Task<double?> CombineManual(Params? prm)
        {
            Console.WriteLine("s");
            #region CreateMainObject
            List<FA3901_00101_MODEL_MANUAL>? FA3901 = new List<FA3901_00101_MODEL_MANUAL>();
            List<ITEMCOMBINE>? ITEMCOMBINE_ = new List<ITEMCOMBINE>();
            #endregion
            var ausw_nr = prm.pool_nr;
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();
            var connectionString = configuration.GetConnectionString("csbContext").ToString();
            using (IDbConnection db = new OracleConnection(connectionString))
            {
                var sql_get_polled_orders = "select OrderNr,DateCreated,DateUpdated from tkmiratorg.ITEMCOMBINE, TKMIRATORG.FA0704_00105, TKMIRATORG.fa0706_00110 WHERE DateUpdated = '0' " +
                    " AND FA704_VERSANDSTELLE = FA706_VERSANDSTELLE" +
                    " AND FA704_TOURNUMMER = FA706_TOURNUMMER" +
                    " AND FA704_VERLADEDATUM = FA706_VERLADEDATUM" +
                    " AND OrderNr = FA706_BS_NR" +
                    " AND FA704_LADE_KST_3 between 0 and 999351" +
                    " AND DateUpdated = 0" +
                    " AND PoolNr = " + ausw_nr;
                ITEMCOMBINE_ = db.Query<ITEMCOMBINE>(sql_get_polled_orders).ToList();
                if (ITEMCOMBINE_.Count > 0)
                {
                    // Получение списка заказов для объединения
                    Console.WriteLine($"Создается слепок анализа 3901: {ausw_nr}");
                    var sql1 = "" +
                    "SELECT DISTINCT " +
                    "FA3901_BS_NR " +
                    ", FA3901_POOL_NR " +
                    ", FA3901_FREIGABE " +
                    ", FA078_VERS_ST " +
                        "FROM tkmiratorg.FA3901_00101, TKMIRATORG.FA0078_00112 " +
                        "WHERE FA3901_POOL_NR = " + ausw_nr + " " +
                        "AND FA3901_FREIGABE = 1 ";
                    var sql2 = sql1 + " AND FA3901_BS_NR IN ( ";

                    foreach (var item in ITEMCOMBINE_.Distinct())
                    {
                        sql2 = sql2 + item.OrderNr + ", ";
                    }
                    sql2 = sql2 + " )";
                    var sql3 = " " +
                        "AND FA3901_BS_NR = FA078_BS_NR " +
                        "AND FA3901_BS_SUB_NR = FA078_BS_SUB_NR";
                    var sql = sql2 + sql3;
                    sql = sql.Replace(",  )", " )");
                    FA3901 = db.Query<FA3901_00101_MODEL_MANUAL>(sql).ToList();
                    foreach (var item in FA3901)
                    {
                        DateTime now = DateTime.Now;
                        string split_new = $"{now:u}";
                        string DateUpdated = split_new.Replace("-", "").Replace(":", "").Replace("Z", "").Replace(" ", "");
                        var sql_update = @"UPDATE tkmiratorg.ITEMCOMBINE SET DateUpdated = '" + DateUpdated + "' WHERE OrderNr = " + item.FA3901_BS_NR;
                        db.Execute(sql_update);
                    }
                    var sql_sy8529 = "" +
                        "select SY8529_ADR_NR, SY8529_SORTGR_TITEL, SY8529_SORTGR_NR " +
                        "FROM tkmiratorg.SY8529_00101 " +
                        "where sy8529_sortgr_titel = 3" +
                        "";
                    var BS_NR = db.Query<FA3886_00101>("SELECT * FROM tkmiratorg.FA3886_00101_ITEM").AsQueryable().AsNoTracking().ToList();
                    FA3901 = FA3901.Where(l => !BS_NR.Any(e => e.FA3886_BS_NR == l.FA3901_BS_NR)).AsQueryable().AsNoTracking().ToList();
                    if (FA3901.Count != 0)
                    {
                        var FA3901_GROP_BY_MAIN_KST = FA3901.GroupBy(x => new { x.FA078_VERS_ST });
                        foreach (var item in FA3901_GROP_BY_MAIN_KST)
                        {
                            Console.WriteLine(item.Key);
                            List<FA3901_00101_MODEL_MANUAL> new_3901 = new List<FA3901_00101_MODEL_MANUAL>();
                            new_3901 = (from i in FA3901 where i.FA078_VERS_ST == item.Key.FA078_VERS_ST select i).ToList();
                            Console.WriteLine(new_3901);
                            // -Группировка по термическому состоянию
                            // Группировка артикулов в заказах по признаку артикул + дата
                            var ids = new_3901.Select(x => x.FA3901_BS_NR);
                            var query = "select FA077_BS_NR,FA077_BS_SUB_NR,FA077_NR,FA077_ART_NR, FA077_HBK_DATUM, FA077_KAT_GRP_NR, FA077_LS_RET_GRUND, FA3902_ZUORD_NR, FA078_TOUR_NR, FA078_VERLADEDATUM " +
                            "from tkmiratorg.FA0077_00114, TKMIRATORG.FA3902_00101, TKMIRATORG.FA0078_00112 " +
                            "where FA077_LS_RET_GRUND = 1 AND FA077_BS_NR = FA3902_BS_NR AND FA077_BS_SUB_NR = FA3902_BS_SUB_NR AND FA077_BS_NR = FA078_BS_NR " +
                            "AND FA077_BS_SUB_NR = FA078_BS_SUB_NR " +
                            "AND FA078_VERS_ST = " + item.Key.FA078_VERS_ST + " " +
                            "AND FA077_BS_NR in ( ";
                            foreach (var id in ids)
                            {
                                query = query + id + ", ";
                            }
                            query = query + " )";
                            query = query.Replace(",  )", " ) ORDER BY FA3902_ZUORD_NR, FA078_VERLADEDATUM, FA078_TOUR_NR");
                            var items = db.Query<FA0077_00114_MIN>(query);
                            var items_group = items.GroupBy(x => new { x.FA077_ART_NR, x.FA077_HBK_DATUM, x.FA077_KAT_GRP_NR, x.FA3902_ZUORD_NR, x.FA078_VERLADEDATUM, x.FA078_TOUR_NR });
                            // добавить разбивку по терм состоянию. Сперва охл
                            // На форму арт компл. добавить кнопку для показа детальной информации по суммарной фикс. позиции
                            List<FA3886_00101> FA3886_00101_ = new List<FA3886_00101>();
                            int MAX_ID = db.Query<int>("SELECT NVL(max(FA3886_ART_KOMM_ID), 0) FA3886_ART_KOMM_ID FROM tkmiratorg.FA3886_00101").First();
                            int MAX_OBER_ID = db.Query<int>("SELECT NVL(max(FA3887_OBER_ID), 0) FA3887_OBER_ID FROM tkmiratorg.FA3887_00102").First();
                            int MAX_AUTO = db.Query<int>("SELECT NVL(max(FA3886_AUTO_INC), 0) FA3886_ART_KOMM_ID FROM tkmiratorg.FA3886_00101").First();
                            MAX_ID++;
                            MAX_OBER_ID++;
                            var zuord_nrs = new Dictionary<double, int>() { };
                            foreach (var id in items_group)
                            {
                                bool zuord_founded = false;
                                foreach (var zuord_nr in zuord_nrs)
                                {
                                    if (id.Key.FA3902_ZUORD_NR == zuord_nr.Key)
                                    {
                                        zuord_founded = true;
                                        MAX_OBER_ID = zuord_nr.Value;
                                    }
                                }
                                if (!zuord_founded)
                                {
                                    int zuord_value = Convert.ToInt32(DateTime.Now.ToString("hhmmss")) + Convert.ToInt32(id.Key.FA3902_ZUORD_NR) + new Random().Next(100, 1000);
                                    zuord_nrs.Add(id.Key.FA3902_ZUORD_NR, zuord_value);
                                    MAX_OBER_ID = zuord_value;
                                }

                                Console.WriteLine(item.Count());
                                Console.WriteLine(item.Key);

                                Console.WriteLine(id.Key.FA3902_ZUORD_NR + " " + id.Key.FA078_VERLADEDATUM + " " + id.Key.FA078_TOUR_NR + " " + MAX_OBER_ID);
                                var OBER_ID = db.Query<FA3887_00102>("select * from TKMIRATORG.FA3887_00102 WHERE FA3887_AUSW_NR = " + ausw_nr + " AND FA3887_ART_KOMM_ID = " + MAX_ID).FirstOrDefault();
                                if (OBER_ID == null)
                                {
                                    FA3887_00102 FA3887 = new FA3887_00102();
                                    FA3887.FA3887_PROG_NR = 3901;
                                    FA3887.FA3887_AUSW_NR = ausw_nr;
                                    FA3887.FA3887_OBER_ID = MAX_OBER_ID;
                                    FA3887.FA3887_ART_KOMM_ID = MAX_ID;
                                    FA3887.FA3887_PRIO = 100;
                                    FA3887.FA3887_ZUOR_TYP = 0;
                                    FA3887.FA3887_ZUORDNUNG = id.Key.FA3902_ZUORD_NR;
                                    FA3887.FA3887_ANL_DATUM = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd")); ;
                                    FA3887.FA3887_ANL_ZEIT = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                                    FA3887.FA3887_ANL_USER = 8900;
                                    FA3887.FA3887_ANL_STATION = 18;
                                    FA3887.FA3887_ANL_PROG = 3901;
                                    FA3887.FA3887_ANL_FKT = 0;
                                    FA3887.FA3887_UPD_DATUM = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
                                    FA3887.FA3887_UPD_ZEIT = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                                    FA3887.FA3887_UPD_USER = 8900;
                                    FA3887.FA3887_UPD_STATION = 18;
                                    FA3887.FA3887_UPD_PROG = 3886;
                                    FA3887.FA3887_UPD_FKT = 0;
                                    FA3887.FA3887_FREIGABE = 1;
                                    FA3887.FA3887_STAT_IN_BEARB = 1;
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
                                foreach (var ed in items)
                                {
                                    if (ed.FA077_ART_NR == id.Key.FA077_ART_NR && ed.FA077_HBK_DATUM == id.Key.FA077_HBK_DATUM
                                    && ed.FA077_KAT_GRP_NR == id.Key.FA077_KAT_GRP_NR && ed.FA3902_ZUORD_NR == id.Key.FA3902_ZUORD_NR
                                    && ed.FA078_VERLADEDATUM == id.Key.FA078_VERLADEDATUM && ed.FA078_TOUR_NR == id.Key.FA078_TOUR_NR)
                                    {
                                        Console.WriteLine("OrderNr: " + ed.FA077_BS_NR + " posNr: " + ed.FA077_NR);
                                        FA3886_00101 new_id = new FA3886_00101();
                                        new_id.FA3886_PROG_NR = 3901;
                                        new_id.FA3886_AUSW_NR = ausw_nr;
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
                                        new_id.FA3886_ANL_DATUM = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
                                        new_id.FA3886_ANL_ZEIT = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                                        new_id.FA3886_ANL_USER = 8900;
                                        new_id.FA3886_ANL_PROG = 3901;
                                        new_id.FA3886_ANL_FKT = 0;
                                        new_id.FA3886_UPD_DATUM = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
                                        new_id.FA3886_UPD_ZEIT = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                                        new_id.FA3886_UPD_USER = 8900;
                                        new_id.FA3886_UPD_FKT = 0;
                                        new_id.FA3886_UPD_PROG = 3886;
                                        new_id.FA3886_FREIGABE = 1;
                                        new_id.FA3886_STAT_IN_BEARB = 1;
                                        new_id.FA3886_BEARB_LFD_NR = 0;
                                        new_id.FA3886_FREIE_NR_1 = ed.FA3902_ZUORD_NR;
                                        MAX_AUTO++;
                                        new_id.FA3886_AUTO_INC = MAX_AUTO;
                                        FA3886_00101_.Add(new_id);
                                    }
                                }
                                MAX_ID++;
                            }
                            db.BulkInsert(FA3886_00101_);
                            Console.WriteLine(MAX_ID);
                        }
                    }
                }
            }
            return 1;
        }

        #region Dispose
        public void Dispose()
        {
            
        }
        ~RunReservation()
        {

        }
        #endregion
    }
}
