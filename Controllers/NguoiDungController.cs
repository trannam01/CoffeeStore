using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CoffeeStore.Models;

namespace CoffeeStore.Controllers
{
    public class NguoiDungController : Controller
    {
        // GET: NguoiDung
        DBCoffeeStoreDataContext data = new DBCoffeeStoreDataContext();

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
        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangKy(FormCollection collection, KHACHHANG kh)
        {
            var hoten = collection["HoTenKH"];
            var tendn = collection["TenDN"];
            var matkhau = collection["MatKhau"];
            var nhaplaiMK = collection["nhaplaiMK"];
            var diachi = collection["DiaChi"];
            var sdt = collection["SDT"];
            var email = collection["Email"];
            var encryp_password = ecryption(matkhau);
            KHACHHANG kh1 = data.KHACHHANGs.SingleOrDefault(n => n.Taikhoan == tendn);
            KHACHHANG kh2 = data.KHACHHANGs.SingleOrDefault(n => n.Email == email);
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
            else if (String.IsNullOrEmpty(diachi))
            {
                ViewData["Loi6"] = "Địa chỉ không được để trống";
            }
            else if (String.IsNullOrEmpty(sdt))
            {
                ViewData["Loi7"] = "Số điện thoại không được để trống";
            }
            else if (String.IsNullOrEmpty(email))
            {
                ViewData["Loi8"] = "Email không được để trống";
            }
            else if (kh1 != null)
            {
                ViewData["Loi9"] = "Trùng tên đăng nhập";
            }
            else if (kh2 != null)
            {
                ViewData["Loi10"] = "Email này đã liên kết với tài khoản khác";
            }
            else
            {
                kh.HoTen = hoten;
                kh.Taikhoan = tendn;
                kh.Matkhau = encryp_password;
                kh.DiachiKH = diachi;
                kh.DienthoaiKH = sdt;
                kh.Email = email;
                data.KHACHHANGs.InsertOnSubmit(kh);
                data.SubmitChanges();
                ViewBag.ThongBao = "Đăng ký thành công";
            }
            return this.DangKy();
        }

        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangNhap(FormCollection collection)
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
                KHACHHANG kh = data.KHACHHANGs.SingleOrDefault(n => n.Taikhoan == tendn && n.Matkhau == matkhau);
                if (kh != null)
                {
                    ViewBag.ThongBao = "Đăng nhập thành công";
                    Session["USERNAME"] = kh;
                    return Redirect("~/CoffeeStore/Index");
                }
                else
                {
                    ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return View();
        }
    }
}