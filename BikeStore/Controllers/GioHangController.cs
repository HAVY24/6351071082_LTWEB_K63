using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BikeStore.Models;

namespace BikeStore.Controllers
{
    public class GioHangController : Controller
    {
        QLBanXeGanMayEntities db = new QLBanXeGanMayEntities();

        // Lấy giỏ hàng từ Session
        private List<Giohang> LayGiohang()
        {
            var lstGiohang = Session["Giohang"] as List<Giohang>;
            if (lstGiohang == null)
            {
                lstGiohang = new List<Giohang>();
                Session["Giohang"] = lstGiohang;
            }
            return lstGiohang;
        }

        // Thêm sản phẩm vào giỏ hàng
        public ActionResult ThemGiohang(int iMaXe, string strURL)
        {
            var lstGiohang = LayGiohang();
            var sanpham = lstGiohang.SingleOrDefault(n => n.iMaXe == iMaXe);

            if (sanpham == null)
            {
                sanpham = new Giohang(iMaXe);
                lstGiohang.Add(sanpham);
            }
            else
            {
                sanpham.iSoluong++; // Tăng số lượng nếu đã tồn tại
            }

            return Redirect(strURL); // Quay lại trang trước đó
        }

        // Tính tổng số lượng sản phẩm
        private int TongSoLuong()
        {
            var lstGiohang = Session["Giohang"] as List<Giohang>;
            return lstGiohang?.Sum(n => n.iSoluong) ?? 0;
        }

        // Tính tổng tiền
        private double TongTien()
        {
            var lstGiohang = Session["Giohang"] as List<Giohang>;
            return lstGiohang?.Sum(n => n.dThanhtien) ?? 0;
        }

        // Hiển thị giỏ hàng
        public ActionResult Giohang()
        {
            var lstGiohang = LayGiohang();
            if (!lstGiohang.Any())
            {
                return RedirectToAction("Index", "QuanLyXe");
            }
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return View(lstGiohang);
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public ActionResult XoaGiohang(int MaXe)
        {
            var lstGiohang = LayGiohang();
            var sanpham = lstGiohang.SingleOrDefault(s => s.iMaXe == MaXe);

            if (sanpham != null)
            {
                lstGiohang.Remove(sanpham);
            }

            if (!lstGiohang.Any()) // Nếu giỏ hàng rỗng
            {
                return RedirectToAction("Index","NguoiDung");
            }

            return RedirectToAction("Giohang");
        }

        // Cập nhật Giỏ Hàng
        public ActionResult CapnhatGioHang(int iMaXe, FormCollection f)
        {
            //Lay gio hang từ Session
            List<Giohang> lstGiohang =LayGiohang();
            Giohang sanpham = lstGiohang.SingleOrDefault(s => s.iMaXe == iMaXe);

            if (sanpham != null)
            {
                sanpham.iSoluong = int.Parse(f["txtSoluong"].ToString());
            }

            return RedirectToAction("Giohang");
        }

        // Xóa toàn bộ giỏ hàng
        public ActionResult XoaTatcaGiohang()
        {
            var giohang = LayGiohang();
            giohang.Clear(); // Xóa toàn bộ sản phẩm trong giỏ
            return RedirectToAction("Index", "NguoiDung");
        }

        // Partial View hiển thị thông tin giỏ hàng
        public ActionResult GiohangPartial()
        {
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return PartialView();
        }
        [HttpGet]
        public ActionResult DatHang()
        {
            //kiemtra dang nhap
            if (Session["Taikhoan"] == null || Session["Taikhoan"].ToString() == "")
            {
                return RedirectToAction("Dangnhap", "NguoiDung");
            }
            if (Session["Giohang"] == null)
            {
                return RedirectToAction("Index", "NguoiDung");
            }

            List<Giohang> lstGiohang =LayGiohang();
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            return View(lstGiohang);
        }
        public ActionResult DatHang(FormCollection collection)
        {
            //Them Don hang  
            DONDATHANG ddh = new DONDATHANG();
            KHACHHANG kh = (KHACHHANG)Session["Taikhoan"];
            List<Giohang> gh = LayGiohang();
            ddh.MaKH = kh.MaKH;
            ddh.Ngaydat = DateTime.Now;
            var ngaygiao = String.Format("{0:MM/dd/yyyy}", collection["Ngaygiao"]);
            ddh.Ngaygiao = DateTime.Parse(ngaygiao);
            ddh.Tinhtranggiaohang = false;
            ddh.Dathanhtoan = false;
            db.DONDATHANGs.Add(ddh);
            db.SaveChanges();
            //Them chi tiet don hang  
            foreach (var item in gh)
            {
                CHITIETDONTHANG ctdh = new CHITIETDONTHANG();
                ctdh.MaDonHang = ddh.MaDonHang;
                ctdh.MaXe = item.iMaXe;
                ctdh.Soluong = item.iSoluong;
                ctdh.Dongia = (decimal)item.dDongia;
                db.CHITIETDONTHANGs.Add(ctdh);
                db.SaveChanges();
            }
            Session["Giohang"] = null;
            return RedirectToAction("Xacnhandonhang", "Giohang");
        }
        public ActionResult Xacnhandonhang() {
            return View();
        }
    }
}
