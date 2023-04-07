using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoffeeStore.Models;
using PagedList;
using PagedList.Mvc;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CoffeeStore.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        DBCoffeeStoreDataContext data = new DBCoffeeStoreDataContext();

        public ActionResult IndexTrangChu()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        #region Login
        public string ecryption(string matkhau)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] encrypt;
            UTF8Encoding encode = new UTF8Encoding();
            encrypt = md5.ComputeHash(encode.GetBytes(matkhau));
            StringBuilder encryptdata = new StringBuilder();
            for (int i = 0; i < encrypt.Length; i++)
            {
                encryptdata.Append(encrypt[i].ToString());
            }
            return encryptdata.ToString();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var tendn = collection["TenDN"];
            var matkhau = ecryption(collection["MatKhau"]);
            if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Tên đăng nhập không được để trống";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Mật khẩu không được để trống";
            }
            else
            {
                Admin ad = data.Admins.SingleOrDefault(n => n.UserAdmin == tendn && n.PassAdmin == matkhau);
                if (ad != null)
                {
                    ViewBag.ThongBao = "Đăng nhập thành công";
                    Session["UserAdmin"] = ad;
                    return RedirectToAction("IndexTrangChu", "Admin");
                }
                else
                {
                    ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangKy(FormCollection collection, Admin ad)
        {
            var hoten = collection["HoTen"];
            var tendn = collection["UserAdmin"];
            var matkhau = collection["PassAdmin"];
            var nhaplaiMK = collection["nhaplaiPassAdmin"];
            var encryp_password = ecryption(matkhau);
            Admin ad1 = data.Admins.SingleOrDefault(n => n.UserAdmin == tendn);
            if (String.IsNullOrEmpty(hoten))
            {
                ViewData["Loi1"] = "Họ tên không được để trống";
            }
            else if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi2"] = "Tên đăng nhập không được để trống";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi3"] = "Mật khẩu không được để trống";
            }
            else if (String.IsNullOrEmpty(nhaplaiMK))
            {
                ViewData["Loi4"] = "Mật khẩu nhập lại không được để trống";
            }
            else if (matkhau != nhaplaiMK)
            {
                ViewData["Loi5"] = "Mật khẩu nhập lại không đúng";
            }
            else if (ad1 != null)
            {
                ViewData["Loi6"] = "Trùng tên đăng nhập";
            }
            else
            {
                ad.HoTen = hoten;
                ad.UserAdmin = tendn;
                ad.PassAdmin = encryp_password;
                data.Admins.InsertOnSubmit(ad);
                data.SubmitChanges();
                ViewBag.ThongBao = "Đăng ký thành công";
            }
            return this.DangKy();
        }
        #endregion

        #region Coffee
        public ActionResult Coffee(int? page, FormCollection fc)
        {
            if (Session["UserAdmin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            else
            {
                string name = fc["Search"];
                if (name != null && name != "")
                {
                    int pageNumber = (page ?? 1);
                    int pageSize = 5;
                    var sp = data.CAFEs.Where(p => p.TenCAFE.ToUpper().Contains(name.ToUpper())).ToList();
                    return View(sp.ToPagedList(pageNumber, pageSize));
                }
                else
                {
                    int pageNumber = (page ?? 1);
                    int pageSize = 5;
                    return View(data.CAFEs.ToList().OrderBy(a => a.MaCAFE).ToPagedList(pageNumber, pageSize));
                }
            }
        }

        public ActionResult Search()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult Themcoffeemoi()
        {
            ViewBag.MaLoai = new SelectList(data.LOAICAFEs.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MaNSX = new SelectList(data.NHASANXUATs.ToList().OrderBy(n => n.TenNSX), "MaNSX", "TenNSX");
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Themcoffeemoi(CAFE cf, HttpPostedFileBase fileupload)
        {
            ViewBag.MaLoai = new SelectList(data.LOAICAFEs.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MaNSX = new SelectList(data.NHASANXUATs.ToList().OrderBy(n => n.TenNSX), "MaNSX", "TenNSX");
            if (fileupload == null)
            {
                ViewBag.Thongbao = "Vui lòng chọn ảnh bìa";
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var fileName = Path.GetFileName(fileupload.FileName);
                    var path = Path.Combine(Server.MapPath("~/HinhSanPham"), fileName);
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.Thongbao = "Hình ảnh đã tồn tại";
                    }
                    else
                    {
                        fileupload.SaveAs(path);
                    }
                    cf.Anhbia = fileName;
                    data.CAFEs.InsertOnSubmit(cf);
                    data.SubmitChanges();
                }
                return RedirectToAction("Coffee");
            }
        }

        public ActionResult Chitietcoffee(int id)
        {
            CAFE sp = data.CAFEs.SingleOrDefault(n => n.MaCAFE == id);
            ViewBag.MaCAFE = sp.MaCAFE;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }

        [HttpGet]
        public ActionResult Xoacoffee(int id)
        {
            CAFE sp = data.CAFEs.SingleOrDefault(n => n.MaCAFE == id);
            ViewBag.MaCAFE = sp.MaCAFE;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }
        [HttpPost, ActionName("Xoacoffee")]
        public ActionResult XacNhanXoa(int id)
        {
            CAFE sp = data.CAFEs.SingleOrDefault(n => n.MaCAFE == id);
            CHITIETDONTHANG ctdh = data.CHITIETDONTHANGs.FirstOrDefault(p => p.MaCAFE == id);
            ViewBag.MaCAFE = sp.MaCAFE;
            if (ctdh != null)
            {
                ViewBag.Thongbao = "Sản phẩm này đang có đơn hàng! Bạn không thể xóa";
                return View(sp);
            }
            else
            {
                if (sp == null)
                {
                    Response.StatusCode = 404;
                    return null;
                }
                data.CAFEs.DeleteOnSubmit(sp);
                data.SubmitChanges();
                return RedirectToAction("Coffee");
            }
        }

        [HttpGet]
        public ActionResult Suacoffee(int id)
        {
            CAFE sp = data.CAFEs.SingleOrDefault(n => n.MaCAFE == id);
            ViewBag.MaCAFE = sp.MaCAFE;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ViewBag.MaLoai = new SelectList(data.LOAICAFEs.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MaNSX = new SelectList(data.NHASANXUATs.ToList().OrderBy(n => n.TenNSX), "MaNSX", "TenNSX");
            return View(sp);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Suacoffee(int id, CAFE sp, HttpPostedFileBase fileupload)
        {
            sp = data.CAFEs.SingleOrDefault(n => n.MaCAFE == id);
            ViewBag.MaCAFE = sp.MaCAFE;
            ViewBag.MaLoai = new SelectList(data.LOAICAFEs.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MaNSX = new SelectList(data.NHASANXUATs.ToList().OrderBy(n => n.TenNSX), "MaNSX", "TenNSX");
            if (fileupload == null)
            {
                CAFE sp1 = data.CAFEs.SingleOrDefault(p => p.MaCAFE == id);
                sp.Anhbia = sp1.Anhbia;
                UpdateModel(sp);
                data.SubmitChanges();
                return RedirectToAction("Coffee");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var fileName = Path.GetFileName(fileupload.FileName);
                    var path = Path.Combine(Server.MapPath("~/HinhSanPham"), fileName);
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.Thongbao = "Hình ảnh đã tồn tại";
                    }
                    else
                    {
                        fileupload.SaveAs(path);
                    }
                    sp.Anhbia = fileName;
                    UpdateModel(sp);
                    data.SubmitChanges();
                }
                return RedirectToAction("Coffee");
            }
        }
        #endregion

        #region Loại Coffee
        public ActionResult LoaiCoffee(int ? page)
        {
            if (Session["UserAdmin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            else
            {
                int pageNumber = (page ?? 1);
                int pageSize = 5;
                return View(data.LOAICAFEs.ToList().OrderBy(a => a.MaLoai).ToPagedList(pageNumber, pageSize));
            }
        }

        [HttpGet]
        public ActionResult Themloaicoffee()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Themloaicoffee(LOAICAFE lcf)
        {
            if (ModelState.IsValid)
            {
                data.LOAICAFEs.InsertOnSubmit(lcf);
                data.SubmitChanges();
            }
                return RedirectToAction("LoaiCoffee");
        }

        public ActionResult Chitietloaicoffee(int id)
        {
            LOAICAFE sp = data.LOAICAFEs.SingleOrDefault(n => n.MaLoai == id);
            ViewBag.MaLoai = sp.MaLoai;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }

        [HttpGet]
        public ActionResult Xoaloaicoffee(int id)
        {
            LOAICAFE sp = data.LOAICAFEs.SingleOrDefault(n => n.MaLoai == id);
            ViewBag.MaLoai = sp.MaLoai;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }
        [HttpPost, ActionName("Xoaloaicoffee")]
        public ActionResult Xacnhanxoa(int id)
        {
            LOAICAFE sp = data.LOAICAFEs.SingleOrDefault(n => n.MaLoai == id);
            ViewBag.MaLoai = sp.MaLoai;
            CHITIETDONTHANG ctdh = data.CHITIETDONTHANGs.FirstOrDefault(p => p.CAFE.MaLoai == id);
            if (ctdh != null)
            {
                ViewBag.Thongbao = "Loại coffee này có sản phẩm đang có đơn hàng! Bạn không thể xóa";
                return View(sp);
            }
            else
            {
                if (sp == null)
                {
                    Response.StatusCode = 404;
                    return null;
                }
                var cfs = data.CAFEs.Where(p => p.MaLoai == id).ToList();
                data.CAFEs.DeleteAllOnSubmit(cfs);
                data.LOAICAFEs.DeleteOnSubmit(sp);
                data.SubmitChanges();
                return RedirectToAction("LoaiCoffee");
            }
        }

        [HttpGet]
        public ActionResult Sualoaicoffee(int id)
        {
            LOAICAFE sp = data.LOAICAFEs.SingleOrDefault(n => n.MaLoai == id);
            ViewBag.MaLoai = sp.MaLoai;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }
        [HttpPost, ActionName("Sualoaicoffee")]
        [ValidateInput(false)]
        public ActionResult Xacnhansua(int id)
        {
            LOAICAFE sp = data.LOAICAFEs.SingleOrDefault(n => n.MaLoai == id);
            ViewBag.MaLoai = sp.MaLoai;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else
            {
                UpdateModel(sp);
                data.SubmitChanges();
            }    
            return RedirectToAction("LoaiCoffee");
        }
        #endregion

        #region Nhà Sản Xuất
        public ActionResult NhaSanXuat(int ? page)
        {
            if (Session["UserAdmin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            else
            {
                int pageNumber = (page ?? 1);
                int pageSize = 5;
                return View(data.NHASANXUATs.ToList().OrderBy(a => a.MaNSX).ToPagedList(pageNumber, pageSize));
            }
        }

        [HttpGet]
        public ActionResult ThemNSX()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemNSX(NHASANXUAT nsx)
        {
            if (ModelState.IsValid)
            {
                data.NHASANXUATs.InsertOnSubmit(nsx);
                data.SubmitChanges();
            }
            return RedirectToAction("NhaSanXuat");
        }

        public ActionResult ChitietNSX(int id)
        {
            NHASANXUAT sp = data.NHASANXUATs.SingleOrDefault(n => n.MaNSX == id);
            ViewBag.MaNSX = sp.MaNSX;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }

        [HttpGet]
        public ActionResult XoaNSX(int id)
        {
            NHASANXUAT sp = data.NHASANXUATs.SingleOrDefault(n => n.MaNSX == id);
            ViewBag.MaNSX = sp.MaNSX;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }
        [HttpPost, ActionName("XoaNSX")]
        public ActionResult XacnhanxoaNSX(int id)
        {
            NHASANXUAT sp = data.NHASANXUATs.SingleOrDefault(n => n.MaNSX == id);
            ViewBag.MaNSX = sp.MaNSX;
            CHITIETDONTHANG ctdh = data.CHITIETDONTHANGs.FirstOrDefault(p => p.CAFE.MaNSX == id);
            if (ctdh != null)
            {
                ViewBag.Thongbao = "NSX này có sản phẩm đang có đơn hàng! Bạn không thể xóa";
                return View(sp);
            }
            else
            {
                if (sp == null)
                {
                    Response.StatusCode = 404;
                    return null;
                }
                var cfs = data.CAFEs.Where(p => p.MaNSX == id).ToList();
                data.CAFEs.DeleteAllOnSubmit(cfs);
                data.NHASANXUATs.DeleteOnSubmit(sp);
                data.SubmitChanges();
                return RedirectToAction("NhaSanXuat");
            }
        }

        [HttpGet]
        public ActionResult SuaNSX(int id)
        {
            NHASANXUAT sp = data.NHASANXUATs.SingleOrDefault(n => n.MaNSX == id);
            ViewBag.MaNSX = sp.MaNSX;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }
        [HttpPost, ActionName("SuaNSX")]
        [ValidateInput(false)]
        public ActionResult XacnhansuaNSX(int id)
        {
            NHASANXUAT sp = data.NHASANXUATs.SingleOrDefault(n => n.MaNSX == id);
            ViewBag.MaNSX = sp.MaNSX;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else
            {
                UpdateModel(sp);
                data.SubmitChanges();
            }
            return RedirectToAction("NhaSanXuat");
        }
        #endregion

        #region Đơn Đặt Hàng
        public ActionResult DonDatHang(int? page)
        {
            if (Session["UserAdmin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            else
            {
                int pageNumber = (page ?? 1);
                int pageSize = 5;
                return View(data.DONDATHANGs.ToList().OrderBy(a => a.MaDonHang).ToPagedList(pageNumber, pageSize));
            }
        }

        public ActionResult ChitietDDH(int id)
        {
            DONDATHANG sp = data.DONDATHANGs.SingleOrDefault(n => n.MaDonHang == id);
            ViewBag.MaDonHang = sp.MaDonHang;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }

        [HttpGet]
        public ActionResult XoaDDH(int id)
        {
            DONDATHANG sp = data.DONDATHANGs.SingleOrDefault(n => n.MaDonHang == id);
            ViewBag.MaDonHang = sp.MaDonHang;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }
        [HttpPost, ActionName("XoaDDH")]
        public ActionResult XacnhanxoaDDH(int id)
        {
            DONDATHANG sp = data.DONDATHANGs.SingleOrDefault(n => n.MaDonHang == id);
            ViewBag.MaDonHang = sp.MaDonHang;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            var ctdh = data.CHITIETDONTHANGs.Where(p => p.MaDonHang == id).ToList();
            data.CHITIETDONTHANGs.DeleteAllOnSubmit(ctdh);
            data.DONDATHANGs.DeleteOnSubmit(sp);
            data.SubmitChanges();
            return RedirectToAction("DonDatHang");
        }

        [HttpGet]
        public ActionResult SuaDDH(int id)
        {
            DONDATHANG sp = data.DONDATHANGs.SingleOrDefault(n => n.MaDonHang == id);
            ViewBag.MaDonHang = sp.MaDonHang;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ViewBag.MaKH = new SelectList(data.KHACHHANGs.ToList().OrderBy(n => n.MaKH), "MaKH", "HoTen");
            ViewBag.MaLoaiTT = new SelectList(data.LoaiThanhToans.ToList().OrderBy(n => n.MaLoaiTT), "MaLoaiTT", "TenLoai");
            return View(sp);
        }
        [HttpPost, ActionName("SuaDDH")]
        [ValidateInput(false)]
        public ActionResult XacnhansuaDDH(int id)
        {
            DONDATHANG sp = data.DONDATHANGs.SingleOrDefault(n => n.MaDonHang == id);
            ViewBag.MaKH = new SelectList(data.KHACHHANGs.ToList().OrderBy(n => n.MaKH), "MaKH", "HoTen");
            ViewBag.MaLoaiTT = new SelectList(data.LoaiThanhToans.ToList().OrderBy(n => n.MaLoaiTT), "MaLoaiTT", "TenLoai");
            ViewBag.MaDonHang = sp.MaDonHang;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else
            {
                UpdateModel(sp);
                data.SubmitChanges();
            }
            return RedirectToAction("DonDatHang");
        }
        #endregion

        #region Khách Hàng
        public ActionResult KhachHang(int? page, FormCollection fc)
        {
            if (Session["UserAdmin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            else
            {
                string name = fc["Search"];
                if (name != null && name != "")
                {
                    int pageNumber = (page ?? 1);
                    int pageSize = 5;
                    var sp = data.KHACHHANGs.Where(p => p.HoTen.ToUpper().Contains(name.ToUpper())).ToList();
                    return View(sp.ToPagedList(pageNumber, pageSize));
                }
                else
                {
                    int pageNumber = (page ?? 1);
                    int pageSize = 5;
                    return View(data.KHACHHANGs.ToList().OrderBy(a => a.MaKH).ToPagedList(pageNumber, pageSize));
                }
            }
        }

        public ActionResult SearchKH()
        {
            return PartialView();
        }

        public ActionResult ChitietKH(int id)
        {
            KHACHHANG sp = data.KHACHHANGs.SingleOrDefault(n => n.MaKH == id);
            ViewBag.MaKH = sp.MaKH;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }

        [HttpGet]
        public ActionResult XoaKH(int id)
        {
            KHACHHANG sp = data.KHACHHANGs.SingleOrDefault(n => n.MaKH == id);
            ViewBag.MaKH = sp.MaKH;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }
        [HttpPost, ActionName("XoaKH")]
        public ActionResult XacnhanxoaKH(int id)
        {
            KHACHHANG sp = data.KHACHHANGs.SingleOrDefault(n => n.MaKH == id);
            ViewBag.MaKH = sp.MaKH;
            CHITIETDONTHANG ctdh = data.CHITIETDONTHANGs.FirstOrDefault(p => p.CAFE.MaNSX == id);
            if (ctdh != null)
            {
                ViewBag.Thongbao = "Khách Hàng này đang có đơn hàng! Bạn không thể xóa";
                return View(sp);
            }
            else
            {
                if (sp == null)
                {
                    Response.StatusCode = 404;
                    return null;
                }
                data.KHACHHANGs.DeleteOnSubmit(sp);
                data.SubmitChanges();
                return RedirectToAction("KhachHang");
            }
        }

        [HttpGet]
        public ActionResult SuaKH(int id)
        {
            KHACHHANG sp = data.KHACHHANGs.SingleOrDefault(n => n.MaKH == id);
            ViewBag.MaKH = sp.MaKH;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }
        [HttpPost, ActionName("SuaKH")]
        [ValidateInput(false)]
        public ActionResult XacnhansuaKH(int id)
        {
            KHACHHANG sp = data.KHACHHANGs.SingleOrDefault(n => n.MaKH == id);
            ViewBag.MaKH = sp.MaKH;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else
            {
                UpdateModel(sp);
                data.SubmitChanges();
            }
            return RedirectToAction("KhachHang");
        }
        #endregion
    }
}