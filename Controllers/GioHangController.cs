using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoffeeStore.Models;
using iTextSharp.tool.xml.html;
using Newtonsoft.Json.Linq;

namespace CoffeeStore.Controllers
{
    public class GioHangController : Controller
    {
        // GET: GioHang
        DBCoffeeStoreDataContext data = new DBCoffeeStoreDataContext();

        public List<GioHang> Laygiohang()
        {
            List<GioHang> lsGioHang = Session["GioHang"] as List<GioHang>;
            if (lsGioHang == null)
            {
                lsGioHang = new List<GioHang>();
                Session["GioHang"] = lsGioHang;
            }
            return lsGioHang;
        }
        public ActionResult ThemGioHang(int Masp, string strURL)
        {
            List<GioHang> lsGioHang = Laygiohang();
            GioHang sp = lsGioHang.Find(n => n.MaSP == Masp);
            if (sp == null)
            {
                sp = new GioHang(Masp);
                lsGioHang.Add(sp);
                return Redirect(strURL);
            }
            else
            {
                sp.SL++;
                return Redirect(strURL);
            }
        }
        private int TongSL()
        {
            int TongSL = 0;
            List<GioHang> lsGioHang = Session["GioHang"] as List<GioHang>;
            if (lsGioHang != null)
            {
                TongSL = lsGioHang.Sum(n => n.SL);
            }
            return TongSL;
        }
        private double TongTien()
        {
            double TongTien = 0;
            List<GioHang> lsGioHang = Session["GioHang"] as List<GioHang>;
            if (lsGioHang != null)
            {
                TongTien = lsGioHang.Sum(n => n.ThanhTien);
            }
            return TongTien;
        }
        public ActionResult GioHang()
        {
            List<GioHang> lsGioHang = Laygiohang();
            if (lsGioHang.Count == 0)
            {
                return RedirectToAction("Index", "CoffeeStore");
            }
            ViewBag.TongSL = TongSL();
            ViewBag.TongTien = TongTien();
            return View(lsGioHang);
        }
        public ActionResult GioHangPartial()
        {
            ViewBag.TongSL = TongSL();
            ViewBag.TongTien = TongTien();
            return PartialView();
        }
        public ActionResult XoaGioHang(int MaSP)
        {
            List<GioHang> lsGioHang = Laygiohang();
            GioHang sp = lsGioHang.SingleOrDefault(n => n.MaSP == MaSP);
            if (sp != null)
            {
                lsGioHang.RemoveAll(n => n.MaSP == MaSP);
                return RedirectToAction("GioHang");
            }
            if (lsGioHang.Count == 0)
            {
                return RedirectToAction("Index", "CoffeeStore");
            }
            return RedirectToAction("GioHang");
        }
        public ActionResult CapNhatGioHang(int MaSP, FormCollection f)
        {
            List<GioHang> lsGioHang = Laygiohang();
            GioHang sp = lsGioHang.SingleOrDefault(n => n.MaSP == MaSP);
            if (sp != null)
            {
                sp.SL = int.Parse(f["txtSoLuong"].ToString());
            }
            return RedirectToAction("GioHang");
        }
        public ActionResult XoaTatCaGioHang()
        {
            List<GioHang> lsGiohang = Laygiohang();
            lsGiohang.Clear();
            return RedirectToAction("Index", "CoffeeStore");
        }

        [HttpGet]
        public ActionResult DatHang()
        {
            if (Session["USERNAME"] == null || Session["USERNAME"].ToString() == "")
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }
            if (Session["GioHang"] == null)
            {
                return RedirectToAction("Index", "BanHang");
            }
            List<GioHang> lsGioHang = Laygiohang();
            ViewBag.MaLoaiTT = new SelectList(data.LoaiThanhToans.ToList().OrderBy(n => n.TenLoai), "MaLoaiTT", "TenLoai");
            ViewBag.TongSL = TongSL();
            ViewBag.TongTien = TongTien();
            return View(lsGioHang);
        }
        [HttpPost]
        public ActionResult DatHang(DONDATHANG ddh, FormCollection collection)
        {
            KHACHHANG kh = (KHACHHANG)Session["USERNAME"];
            List<GioHang> gh = Laygiohang();
            if (ddh.MaLoaiTT == 1)
            {
                ddh.MaKH = kh.MaKH;
                ddh.Ngaydat = DateTime.Now;
                var ngaygiao = String.Format("{0:MM/dd/yyyy}", collection["NGAYNHANHANG"]);
                if (ngaygiao != "")
                {
                    ddh.Ngaygiao = DateTime.Parse(ngaygiao);
                    ddh.Dathanhtoan = true;
                    ViewBag.MaLoaiTT = new SelectList(data.LoaiThanhToans.ToList().OrderBy(n => n.TenLoai), "MaLoaiTT", "TenLoai");
                    data.DONDATHANGs.InsertOnSubmit(ddh);
                    data.SubmitChanges();
                    foreach (var item in gh)
                    {
                        CHITIETDONTHANG cthd = new CHITIETDONTHANG();
                        cthd.MaDonHang = ddh.MaDonHang;
                        cthd.MaCAFE = item.MaSP;
                        cthd.Soluong = item.SL;
                        cthd.Dongia = (decimal)item.DonGia;
                        data.CHITIETDONTHANGs.InsertOnSubmit(cthd);
                        CAFE sp = data.CAFEs.SingleOrDefault(n => n.MaCAFE == item.MaSP);
                        sp.Soluongton = sp.Soluongton - item.SL;
                        UpdateModel(sp);
                    }
                    data.SubmitChanges();
                    return RedirectToAction("ThanhToan", "GioHang");
                }
                else
                {
                    ViewBag.MaLoaiTT = new SelectList(data.LoaiThanhToans.ToList().OrderBy(n => n.TenLoai), "MaLoaiTT", "TenLoai");
                    ViewBag.Tongtien = gh.Sum(p => p.ThanhTien);
                    ViewBag.Thongbao = "Ngày giao không được để trống!";
                    return View(gh);
                }    
            }
            else
            {
                ddh.MaKH = kh.MaKH;
                ddh.Ngaydat = DateTime.Now;
                var ngaygiao = String.Format("{0:MM/dd/yyyy}", collection["NGAYNHANHANG"]);
                if (ngaygiao != "")
                {
                    ddh.Ngaygiao = DateTime.Parse(ngaygiao);
                    ViewBag.MaLoaiTT = new SelectList(data.LoaiThanhToans.ToList().OrderBy(n => n.TenLoai), "MaLoaiTT", "TenLoai");
                    ddh.Dathanhtoan = false;
                    data.DONDATHANGs.InsertOnSubmit(ddh);
                    data.SubmitChanges();
                    foreach (var item in gh)
                    {
                        CHITIETDONTHANG cthd = new CHITIETDONTHANG();
                        cthd.MaDonHang = ddh.MaDonHang;
                        cthd.MaCAFE = item.MaSP;
                        cthd.Soluong = item.SL;
                        cthd.Dongia = (decimal)item.DonGia;
                        data.CHITIETDONTHANGs.InsertOnSubmit(cthd);
                        CAFE sp = data.CAFEs.SingleOrDefault(n => n.MaCAFE == item.MaSP);
                        sp.Soluongton = sp.Soluongton - item.SL;
                        UpdateModel(sp);
                    }
                    data.SubmitChanges();
                    return RedirectToAction("Xacnhandonhang", "Giohang");
                }
                else
                {
                    ViewBag.MaLoaiTT = new SelectList(data.LoaiThanhToans.ToList().OrderBy(n => n.TenLoai), "MaLoaiTT", "TenLoai");
                    ViewBag.Tongtien = gh.Sum(p => p.ThanhTien);
                    ViewBag.Thongbao = "Ngày giao không được để trống!";
                    return View(gh);
                }    
            }
        }
        //Thanh toán = momo
        public ActionResult ThanhToan()
        {
            List<GioHang> gh = Laygiohang();
            string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
            string partnerCode = "MOMO8C7V20220327";
            string accessKey = "799hYhzCGytOsb6F";
            string serectkey = "h9Jz42YJMHxA7qkYvIqS1cbS3GbwB6Qn";
            string orderInfo = "Thanh toán hóa đơn";
            string returnUrl = "https://Coffeeteams.tk//GioHang/Xacnhandonhang";
            string notifyurl = "http://ba1adf48beba.ngrok.io/GioHang/SavePayment";

            string amount = gh.Sum(p => p.ThanhTien).ToString();
            string orderid = DateTime.Now.Ticks.ToString();
            string requestId = DateTime.Now.Ticks.ToString();
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "partnerCode=" +
                partnerCode + "&accessKey=" +
                accessKey + "&requestId=" +
                requestId + "&amount=" +
                amount + "&orderId=" +
                orderid + "&orderInfo=" +
                orderInfo + "&returnUrl=" +
                returnUrl + "&notifyUrl=" +
                notifyurl + "&extraData=" +
                extraData;


            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);
            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "extraData", extraData },
                { "requestType", "captureMoMoWallet" },
                { "signature", signature }

            };


            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());


            JObject jmessage = JObject.Parse(responseFromMomo);

            return Redirect(jmessage.GetValue("payUrl").ToString());
        }

        /*public ActionResult ReturnUrl()
        {
            string param = Request.QueryString.ToString().Substring(0, Request.QueryString.ToString().IndexOf("signature") - 1);
            param = Server.UrlDecode(param);
            MoMoSecurity cryto = new MoMoSecurity();
            string serectkey = "h9Jz42YJMHxA7qkYvIqS1cbS3GbwB6Qn";
            string signature = cryto.signSHA256(param, serectkey);
            if(signature != Request["signature"].ToString())
            {
                ViewBag.Thongbao = "Thông tin Request không hợp lệ";
                return View();
            }
            if(!Request.QueryString["errorCode"].Equals("0"))
            {
                ViewBag.Thongbao = "Thanh toán thất bại";
            }    
            else
            {
                ViewBag.Thongbao = "Thanh toán thành công";
                Session["GioHang"] = new List<GioHang>();
            }    
            return View();
        }

        public ActionResult NotifyUrl()
        {
            string param = "";
            param = "partner_code" + Request["partner_code"] +
                "&access_key=" + Request["access_key"] +
                "&amount=" + Request["amount"] +
                "&order_id=" + Request["order_id"] +
                "&order_info=" + Request["order_info"] +
                "&order_type=" + Request["order_type"] +
                "&transaction_id=" + Request["transaction_id"] +
                "&message=" + Request["message"] +
                "&response_time=" + Request["response_time"] +
                "&status_code=" + Request["status_code"];
            param = Server.UrlDecode(param);
            MoMoSecurity cryto = new MoMoSecurity();
            string serectkey = "h9Jz42YJMHxA7qkYvIqS1cbS3GbwB6Qn";
            string status_code = Request["status_code"].ToString();
            if(status_code != "0")
            {
                ViewBag.Thongbao = "Thông tin Request không hợp lệ";
                return View();
            }
            else
            {
                ViewBag.Thongbao = "Thanh toán thành công";
                Session["GioHang"] = new List<GioHang>();
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }*/

        public void SavePayment()
        {
            //cap nhat du lieu vao database
        }

        //Thanh toán trực tiếp
        public ActionResult Xacnhandonhang()
        {
            Session["GioHang"] = null;
            return View();
        }
    }
}