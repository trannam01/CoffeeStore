using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoffeeStore.Models;
using PagedList;
using PagedList.Mvc;

namespace CoffeeStore.Controllers
{
    public class CoffeeStoreController : Controller
    {
        // GET: CoffeeStore
        DBCoffeeStoreDataContext data = new DBCoffeeStoreDataContext();

        private List<CAFE> Laycoffeemoi(int count)
        {
            return data.CAFEs.OrderBy(a => a.MaCAFE).Take(count).ToList();
        }

        public ActionResult Index(FormCollection fc)
        {
            string name = fc["Search"];
            if (name != null && name != "")
            {
                var sp = data.CAFEs.Where(p => p.TenCAFE.ToUpper().Contains(name.ToUpper())).ToList();
                return View(sp);
            }
            else
            {
                var coffeemoi = Laycoffeemoi(15);
                return View(coffeemoi);
            }
        }

        private List<CAFE> Laycoffee()
        {
            return data.CAFEs.OrderByDescending(a => a.Ngaycapnhat).ToList();
        }

        public ActionResult Coffee(int ? page)
        {
            int pageSize = 9;
            int PageNum = (page ?? 1);
            var cfm = Laycoffeemoi(35);
            return View(cfm.ToPagedList(PageNum, pageSize));
        }

        public ActionResult LoaiCoffee()
        {
            var loaicoffee = from lcf in data.LOAICAFEs select lcf;
            return PartialView(loaicoffee);
        }

        public ActionResult SPTheoLoaiCoffee(int id, int ? page)
        {
            int pageSize = 9;
            int PageNum = (page ?? 1);
            var sptl = from s in data.CAFEs where s.MaLoai == id select s;
            return View(sptl.ToPagedList(PageNum, pageSize));
        }

        public ActionResult Details(int id)
        {
            var ctcf = from s in data.CAFEs where s.MaCAFE == id select s;
            return View(ctcf.Single());
        }

        public ActionResult CoffeeNew()
        {
            var cf = data.CAFEs.OrderByDescending(a => a.TenCAFE.Contains("Arabica")).Take(1).ToList();
            return View(cf.Single());
        }

        public ActionResult Search()
        {
            return PartialView();
        }
    }
}