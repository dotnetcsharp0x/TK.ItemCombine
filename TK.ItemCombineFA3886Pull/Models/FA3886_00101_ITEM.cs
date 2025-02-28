using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.ItemCombineFA3886Pull.Models
{
    public class FA3886_00101_ITEM
    {
        public double FA3886_PROG_NR {  get; set; }
        public double FA3886_AUSW_NR {  get; set; }
        public double FA3886_PRIO {  get; set; }
        public double FA3886_ART_KOMM_ID {  get; set; }
        public double FA3886_SORT_NR {  get; set; }
        public double FA3886_ART_NR {  get; set; }
        public double FA3886_SORT_NR_2 {  get; set; }
        public double FA3886_BS_NR {  get; set; }
        public double FA3886_BS_SUB_NR {  get; set; }
        public double FA3886_TYP {  get; set; }
        public double FA3886_POSTEN_ID {  get; set; }
        public double FA3886_ZUORD_TYP {  get; set; }
        public double FA3886_ZUORDNUNG {  get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public double FA3886_AUTO_INC {  get; set; }
        public double FA3886_RELEASE {  get; set; }
        public double FA3886_ANL_DATUM {  get; set; }
        public double FA3886_ANL_ZEIT {  get; set; }
        public double FA3886_ANL_USER {  get; set; }
        public double FA3886_ANL_PROG {  get; set; }
        public double FA3886_ANL_FKT {  get; set; }
        public double FA3886_UPD_DATUM {  get; set; }
        public double FA3886_UPD_ZEIT {  get; set; }
        public double FA3886_UPD_USER {  get; set; }
        public double FA3886_UPD_PROG {  get; set; }
        public double FA3886_UPD_FKT {  get; set; }
        public double FA3886_FREIGABE {  get; set; }
        public double FA3886_STAT_IN_BEARB {  get; set; }
        public double FA3886_BEARB_LFD_NR {  get; set; }
        public double FA3886_FREIE_NR_1 {  get; set; }
        public double IMPORT {  get; set; }
    }
}
