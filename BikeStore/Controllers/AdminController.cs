using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using System.Web.Helpers; 


namespace BikeStore.Controllers
{
    public class AdminController : Controller
    {
        QLBanXeGanMayEntities db = new QLBanXeGanMayEntities();
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Xeganmay(int? page)
        {
            int pageNumber = page ?? 1; 
            int pageSize = 7; 

            var xeGanMayList = db.XEGANMAYs
                                 .OrderBy(n => n.MaXe) 
                                 .ToPagedList(pageNumber, pageSize); 

            return View(xeGanMayList); 
        }

        [HttpGet]
        public ActionResult Login() {
            return View();
        }

        [HttpGet]
        public ActionResult Dangky()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Dangky(FormCollection collection, KHACHHANG kh)
        {
            // Lấy dữ liệu từ form
            var hoten = collection["HotenKH"];
            var tendn = collection["TenDN"];
            var matkhau = collection["Matkhau"];
            var matkhaunhaplai = collection["Matkhaunhaplai"];
            var diachi = collection["Diachi"];
            var email = collection["Email"];
            var dienthoai = collection["Dienthoai"];
            var ngaysinh = collection["Ngaysinh"];

            // Kiểm tra validation
            if (String.IsNullOrEmpty(hoten))
                ModelState.AddModelError("HoTen", "Họ tên khách hàng không được để trống.");
            if (String.IsNullOrEmpty(tendn))
                ModelState.AddModelError("TenDN", "Tên đăng nhập không được để trống.");
            if (String.IsNullOrEmpty(matkhau))
                ModelState.AddModelError("Matkhau", "Mật khẩu không được để trống.");
            if (matkhau != matkhaunhaplai)
                ModelState.AddModelError("Matkhaunhaplai", "Mật khẩu nhập lại không khớp.");
            if (String.IsNullOrEmpty(email))
                ModelState.AddModelError("Email", "Email không được để trống.");
            if (String.IsNullOrEmpty(dienthoai))
                ModelState.AddModelError("Dienthoai", "Điện thoại không được để trống.");
            if (!DateTime.TryParse(ngaysinh, out var parsedNgaySinh))
                ModelState.AddModelError("Ngaysinh", "Ngày sinh không hợp lệ.");

            // Kiểm tra nếu tài khoản hoặc email đã tồn tại
            if (db.KHACHHANGs.Any(k => k.Taikhoan == tendn))
                ModelState.AddModelError("TenDN", "Tên đăng nhập đã tồn tại.");
            if (db.KHACHHANGs.Any(k => k.Email == email))
                ModelState.AddModelError("Email", "Email đã được sử dụng.");

            // Nếu có lỗi, quay lại form đăng ký
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Nếu không có lỗi, lưu thông tin khách hàng vào database
            kh.HoTen = hoten;
            kh.Taikhoan = tendn;
            kh.Matkhau = matkhau; // Lưu ý: Nên mã hóa mật khẩu trước khi lưu
            kh.Email = email;
            kh.DiachiKH = diachi;
            kh.DienthoaiKH = dienthoai;
            kh.Ngaysinh = parsedNgaySinh;

            db.KHACHHANGs.Add(kh);
            db.SaveChanges();

            ViewBag.Thongbao = "Đăng ký thành công!";
            return RedirectToAction("Login","Admin");
        }


        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var tendn = collection["username"];
            var matkhau = collection["password"];

            if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Phải nhập tên đăng nhập";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Phải nhập mật khẩu";
            }
            else
            {
                // Kiểm tra tài khoản admin
                Admin ad = db.Admins.SingleOrDefault(n => n.UserAdmin == tendn && n.PassAdmin == matkhau);
                if (ad != null)
                {
                    Session["Taikhoanadmin"] = ad;
                    return RedirectToAction("Index", "Admin");
                }

                // Kiểm tra tài khoản khách hàng
                KHACHHANG kh = db.KHACHHANGs.SingleOrDefault(x => x.Taikhoan == tendn && x.Matkhau == matkhau);
                if (kh != null)
                {
                    Session["Taikhoan"] = kh;
                    return RedirectToAction("Index", "NguoiDung");
                }

                ViewBag.Thongbao = "Tên đăng nhập hoặc mật khẩu không đúng";
            }
            return View();
        }

        [HttpGet]
        public ActionResult ThemmoiXe() 
        {
            ViewBag.MaLX = new SelectList(db.LOAIXEs.ToList().OrderBy(n => n.TenLoaiXe), "MaLX", "TenLoaiXe");
            ViewBag.MaNPP = new SelectList(db.NHAPHANPHOIs.ToList().OrderBy(n => n.TenNPP), "MaNPP", "TenNPP");
            return View();
        }

        [HttpPost]

        [ValidateInput(false)]

        public ActionResult ThemmoiXe(XEGANMAY xeganmay, HttpPostedFileBase fileUpload)
        {
            //Dua du lieu vao dropdownload
            ViewBag.MaLX = new SelectList(db.LOAIXEs.ToList().OrderBy(n => n.TenLoaiXe), "MaLX", "TenLoaiXe");
            ViewBag.MaNPP = new SelectList(db.NHAPHANPHOIs.ToList().OrderBy(n => n.TenNPP), "MaNPP", "TenNPP");
            if (fileUpload == null)
            {     
                ViewBag.Thongbao = "Vui lòng chọn ảnh bìa";
            
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {

                    //Luu ten fie, luu y bo sung thu vien using System.IO;
                    var fileName = Path.GetFileName(fileUpload.FileName);
                    //Luu duong dan cua file
                    var path = Path.Combine(Server.MapPath("~/images"), fileName); //Kiem tra hình anh ton tai chua?
                    if (System.IO.File.Exists(path))
                        ViewBag.Thongbao = "Hình ảnh đã tồn tại";
                    else
                    {
                        fileUpload.SaveAs(path);
                    }
                    //Luu hinh anh vao duong dan
                    xeganmay.Anhbia = fileName;
                    //Luu vào CSDL
                    db.XEGANMAYs.Add(xeganmay);
                    db.SaveChanges();
                }
                return RedirectToAction("Xeganmay");
            }
        }
        public ActionResult Chitietxe(int id)
        {
            XEGANMAY xe = db.XEGANMAYs.SingleOrDefault(n => n.MaXe == id);
            ViewBag.MaXe = xe.MaXe;

            if (xe == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(xe);
        }

        [HttpGet]

        public ActionResult Xoaxe(int id) {

            XEGANMAY xe = db.XEGANMAYs.SingleOrDefault(n => n.MaXe == id);
            ViewBag.MaXe = xe.MaXe;
            if (xe == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(xe);
        }

        [HttpPost, ActionName("Xoaxe")]
        public ActionResult Xacnhanxoa(int id) 
        {
            XEGANMAY xe = db.XEGANMAYs.SingleOrDefault(n => n.MaXe == id);
            ViewBag.MaXe = xe.MaXe;
            if (xe == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            db.XEGANMAYs.Remove(xe);
            db.SaveChanges();
            return RedirectToAction("Xeganmay");
        }


        [HttpGet]

        public ActionResult Suaxe(int id)
        {
            XEGANMAY xeganmay = db.XEGANMAYs.SingleOrDefault(n => n.MaXe == id);

            // Kiểm tra nếu không tìm thấy xe
            if (xeganmay == null)
            {
                Response.StatusCode = 404;
                return HttpNotFound("Xe máy không tồn tại."); // Trả về trang lỗi
            }

            ViewBag.MaLX = new SelectList(db.LOAIXEs.OrderBy(n => n.TenLoaiXe), "MaLX", "TenLoaiXe");
            ViewBag.MaNPP = new SelectList(db.NHAPHANPHOIs.OrderBy(n => n.TenNPP), "MaNPP", "TenNPP");
            return View(xeganmay); // Truyền dữ liệu xuống View
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Suaxe(XEGANMAY xeganmay, HttpPostedFileBase fileUpload)
        {
            ViewBag.MaLX = new SelectList(db.LOAIXEs.ToList().OrderBy(n => n.TenLoaiXe), "MaLX", "TenLoaiXe", xeganmay.MaLX);
            ViewBag.MaNPP = new SelectList(db.NHAPHANPHOIs.ToList().OrderBy(n => n.TenNPP), "MaNPP", "TenNPP", xeganmay.MaNPP);

            if (ModelState.IsValid)
            {
                var existingCar = db.XEGANMAYs.FirstOrDefault(c => c.MaXe == xeganmay.MaXe);
                if (existingCar == null)
                {
                    ViewBag.Announce = "Car not found.";
                    return Json(new {mess = "a"}, JsonRequestBehavior.AllowGet);
                }

                existingCar.TenXe = xeganmay.TenXe;
                existingCar.Giaban = xeganmay.Giaban;
                existingCar.Mota = xeganmay.Mota;
                existingCar.Ngaycapnhat = xeganmay.Ngaycapnhat;
                existingCar.Soluongton = xeganmay.Soluongton;
                existingCar.MaLX = xeganmay.MaLX;
                existingCar.MaNPP = xeganmay.MaNPP;

                if (fileUpload != null)
                {
                    var filename = Path.GetFileName(fileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/images"), filename);

                    if (!System.IO.File.Exists(path))
                    {
                        fileUpload.SaveAs(path);
                        existingCar.Anhbia = filename;
                    }
                    else
                    {
                        ViewBag.Announce = "Picture already exists.";
                        return Json(new { mess = "b" }, JsonRequestBehavior.AllowGet);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Xeganmay", "Admin");
            }

            ViewBag.Announce = "Model state is invalid.";
            return Json(new { mess = "b" }, JsonRequestBehavior.AllowGet); 
        }

        public ActionResult ThongKeXe()
        {
            var thongKe = db.XEGANMAYs
                            .GroupBy(x => x.MaLX) 
                            .Select(g => new
                            {
                                LoaiXe = g.FirstOrDefault().LOAIXE.TenLoaiXe, 
                                SoLuong = g.Count() 
                            })
                            .ToList();

            // Dữ liệu biểu đồ
            var chart = new Chart(width: 800, height: 400)
                        .AddTitle("Thống kê số lượng xe theo loại")
                        .AddLegend("Loại xe")
                        .AddSeries(
                            chartType: "Column", 
                            xValue: thongKe.Select(x => x.LoaiXe).ToArray(), 
                            yValues: thongKe.Select(x => x.SoLuong).ToArray() 
                        )
                        .Write(); 

            return null; 
        }
        public ActionResult Logout()
        {
            // Xóa Session tài khoản admin
            Session["Taikhoanadmin"] = null;

            // Chuyển hướng về trang Index của NguoiDung
            return RedirectToAction("Login", "Admin");
        }



    }
}