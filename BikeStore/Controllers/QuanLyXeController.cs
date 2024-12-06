using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PagedList;
using PagedList.Mvc;

namespace BikeStore.Controllers
{
    public class QuanLyXeController : Controller
    {
        QLBanXeGanMayEntities db=new QLBanXeGanMayEntities();
        // GET: QuanLyXe
        private List<XEGANMAY> Layxemoi(int count)
        {
            return db.XEGANMAYs.OrderByDescending(a => a.Ngaycapnhat).Take(count).ToList();
        }
        public ActionResult Index(int ? page)
        {
            int pageSize = 5;
            int pageNum = (page ?? 1);

            var data = Layxemoi(20);


            return View(data.ToPagedList(pageNum,pageSize));
        }
        public ActionResult LoaiXe()
        {
            var data = db.LOAIXEs.ToList();
            
            return View(data);
        }
        public ActionResult NhaPhanPhoi()
        {
            var data = db.NHAPHANPHOIs.ToList();

            return View(data);
        }
        public ActionResult SPTheoLoaiXe(int id)
        {
            var data = from s in db.XEGANMAYs where s.MaLX == id select s;

            return View(data);
        }
        public ActionResult SPTheoNhaphanphoi(int id)
        {
            var data = from cd in db.XEGANMAYs where cd.MaNPP == id select cd;

            return View(data);
        }
        public ActionResult Details(int id)
        {
            var data = from s in db.XEGANMAYs where s.MaXe == id select s;

            return View(data.Single());
        }
    }
}