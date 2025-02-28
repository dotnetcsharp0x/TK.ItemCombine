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
using TK.ItemCombineFA3886Pull.Data;
using TK.ItemCombineFA3886Pull.Models;
using Z.Dapper.Plus;

namespace TK.ItemCombineFA3886Pull.Actions
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
            #region CreateMainObject
            List<FA3886_00101_ITEM>? FA3886_ITEM = new List<FA3886_00101_ITEM>();
            List<FA3887_00102_ITEM>? FA3887_ITEM = new List<FA3887_00102_ITEM>();
            List<FA3886_00101>? FA3886 = new List<FA3886_00101>();
            List<FA3887_00102>? FA3887 = new List<FA3887_00102>();
            #endregion
            var ausw_nr = prm.pool_nr;
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();
            var connectionString = configuration.GetConnectionString("csbContext").ToString();
            using (IDbConnection db = new OracleConnection(connectionString))
            {
                Console.WriteLine($"Создается слепок fa3886_item: {ausw_nr}");
                var sql_FA3886 = "SELECT * FROM TKMIRATORG.FA3886_00101_ITEM WHERE IMPORT = 0";
                FA3886_ITEM = db.Query<FA3886_00101_ITEM>(sql_FA3886).ToList();
                var sql_FA3887 = "SELECT * FROM TKMIRATORG.FA3887_00102_ITEM WHERE IMPORT = 0";
                FA3887_ITEM = db.Query<FA3887_00102_ITEM>(sql_FA3887).ToList();
                foreach(var item in FA3886_ITEM)
                {
                    item.IMPORT = 1;
                    FA3886 = (from i in FA3886_ITEM
                              select new FA3886_00101
                              {
                                  FA3886_PROG_NR = i.FA3886_PROG_NR
                ,
                                  FA3886_AUSW_NR = i.FA3886_AUSW_NR
                ,
                                  FA3886_PRIO = i.FA3886_PRIO
                ,
                                  FA3886_ART_KOMM_ID = i.FA3886_ART_KOMM_ID
                ,
                                  FA3886_SORT_NR = i.FA3886_SORT_NR
                ,
                                  FA3886_ART_NR = i.FA3886_ART_NR
                ,
                                  FA3886_SORT_NR_2 = i.FA3886_SORT_NR_2
                ,
                                  FA3886_BS_NR = i.FA3886_BS_NR
                ,
                                  FA3886_BS_SUB_NR = i.FA3886_BS_SUB_NR
                ,
                                  FA3886_TYP = i.FA3886_TYP
                ,
                                  FA3886_POSTEN_ID = i.FA3886_POSTEN_ID
                ,
                                  FA3886_ZUORD_TYP = i.FA3886_ZUORD_TYP
                ,
                                  FA3886_ZUORDNUNG = i.FA3886_ZUORDNUNG
                ,
                                  FA3886_AUTO_INC = i.FA3886_AUTO_INC
                ,
                                  FA3886_RELEASE = i.FA3886_RELEASE
                ,
                                  FA3886_ANL_DATUM = i.FA3886_ANL_DATUM
                ,
                                  FA3886_ANL_ZEIT = i.FA3886_ANL_ZEIT
                ,
                                  FA3886_ANL_USER = i.FA3886_ANL_USER
                ,
                                  FA3886_ANL_PROG = i.FA3886_ANL_PROG
                ,
                                  FA3886_ANL_FKT = i.FA3886_ANL_FKT
                ,
                                  FA3886_UPD_DATUM = i.FA3886_UPD_DATUM
                ,
                                  FA3886_UPD_ZEIT = i.FA3886_UPD_ZEIT
                ,
                                  FA3886_UPD_USER = i.FA3886_UPD_USER
                ,
                                  FA3886_UPD_PROG = i.FA3886_UPD_PROG
                ,
                                  FA3886_UPD_FKT = i.FA3886_UPD_FKT
                ,
                                  FA3886_FREIGABE = i.FA3886_FREIGABE
                ,
                                  FA3886_STAT_IN_BEARB = i.FA3886_STAT_IN_BEARB
                ,
                                  FA3886_BEARB_LFD_NR = i.FA3886_BEARB_LFD_NR
                ,
                                  FA3886_FREIE_NR_1 = i.FA3886_FREIE_NR_1
                              }).ToList();
                    db.BulkInsert(FA3886);
                }
                foreach(var item in FA3887_ITEM)
                {
                    item.IMPORT = 1;
                    FA3887 = (from i in FA3887_ITEM
                              select new FA3887_00102
                              {
                                  FA3887_PROG_NR = i.FA3887_PROG_NR
                   ,
                                  FA3887_AUSW_NR = i.FA3887_AUSW_NR
                   ,
                                  FA3887_PRIO = i.FA3887_PRIO
                   ,
                                  FA3887_ART_KOMM_ID = i.FA3887_ART_KOMM_ID
                   ,
                                  FA3887_ZUOR_TYP = i.FA3887_ZUOR_TYP
                   ,
                                  FA3887_ZUORDNUNG = i.FA3887_ZUORDNUNG
                   ,
                                  FA3887_ANL_DATUM = i.FA3887_ANL_DATUM
                   ,
                                  FA3887_ANL_ZEIT = i.FA3887_ANL_ZEIT
                   ,
                                  FA3887_ANL_USER = i.FA3887_ANL_USER
                   ,
                                  FA3887_ANL_PROG = i.FA3887_ANL_PROG
                   ,
                                  FA3887_ANL_FKT = i.FA3887_ANL_FKT
                   ,
                                  FA3887_UPD_DATUM = i.FA3887_UPD_DATUM
                   ,
                                  FA3887_UPD_ZEIT = i.FA3887_UPD_ZEIT
                   ,
                                  FA3887_UPD_USER = i.FA3887_UPD_USER
                   ,
                                  FA3887_UPD_PROG = i.FA3887_UPD_PROG
                   ,
                                  FA3887_UPD_FKT = i.FA3887_UPD_FKT
                   ,
                                  FA3887_FREIGABE = i.FA3887_FREIGABE
                   ,
                                  FA3887_STAT_IN_BEARB = i.FA3887_STAT_IN_BEARB
                   ,
                                  FA3887_X_FELD_1 = i.FA3887_X_FELD_1
                   ,
                                  FA3887_OBER_ID = i.FA3887_OBER_ID
                   ,
                                  FA3887_SORTER_LINIE = i.FA3887_SORTER_LINIE
                   ,
                                  FA3887_BEARBEITET = i.FA3887_BEARBEITET
                   ,
                                  FA3887_PAZ = i.FA3887_PAZ
                   ,
                                  FA3887_ANZ_ART = i.FA3887_ANZ_ART
                   ,
                                  FA3887_VOLL_ZUORD_TYP = i.FA3887_VOLL_ZUORD_TYP
                   ,
                                  FA3887_VOLL_ZUORDNUNG = i.FA3887_VOLL_ZUORDNUNG
                   ,
                                  FA3887_TEIL_ZUORD_TYP = i.FA3887_TEIL_ZUORD_TYP
                   ,
                                  FA3887_TEIL_ZUORDNUNG = i.FA3887_TEIL_ZUORDNUNG
                   ,
                                  FA3887_ANL_STATION = i.FA3887_ANL_STATION
                   ,
                                  FA3887_UPD_STATION = i.FA3887_UPD_STATION
                   ,
                                  FA3887_X_FELD_2 = i.FA3887_X_FELD_2
                              }).ToList();

                    db.BulkInsert(FA3887);
                }
                
                

               

                

                db.BulkUpdate(FA3887_ITEM);
                db.BulkUpdate(FA3886_ITEM);
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
