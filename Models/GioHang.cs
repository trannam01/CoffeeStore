using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoffeeStore.Models;

namespace CoffeeStore.Models
{
    public class GioHang
    {
        DBCoffeeStoreDataContext data = new DBCoffeeStoreDataContext();

        public int MaSP { set; get; }
        public string TenSP { set; get; }
        public string AnhSP { set; get; }
        public Double DonGia { set; get; }
        public int SL { set; get; }
        public Double ThanhTien
        {
            get { return SL * DonGia; }
        }

        public GioHang(int Masp)
        {
            MaSP = Masp;
            CAFE cf = data.CAFEs.Single(n => n.MaCAFE == MaSP);
            TenSP = cf.TenCAFE;
            AnhSP = cf.Anhbia;
            DonGia = double.Parse(cf.Giaban.ToString());
            SL = 1;
        }
    }
}