using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BikeStore.Models
{

    public class Giohang
    {
        QLBanXeGanMayEntities db = new QLBanXeGanMayEntities();
        public int iMaXe { get; set; }
        public string sTenXe { get; set; }
        public string sAnhbia { get; set; }
        public double dDongia { get; set; }
        public int iSoluong { get; set; }
        public Double dThanhtien { get { return iSoluong * dDongia; } }
        public Giohang(int MaXe) { 
            iMaXe = MaXe;
            XEGANMAY xe=db.XEGANMAYs.Single(n => n.MaXe == iMaXe);
            sTenXe = xe.TenXe;
            sAnhbia = xe.Anhbia;
            dDongia = double.Parse(xe.Giaban.ToString());
            iSoluong = 1;
        }
    }
}