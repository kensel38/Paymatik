using DAL;
using EL;
using System;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Web.Mvc;

namespace Paymatik_WebAdmin.Controllers
{
    public class HomeController : Controller
    {

        UnitOfWork _uow = null;
        public HomeController() => _uow = new UnitOfWork();

        // GET: Home
        public ActionResult Index()
        {
            ViewBag.ToplamBina = _uow.GetRepo<tbl_Bina>().GetAll().Count();
            ViewBag.ToplamDaire = _uow.GetRepo<tbl_BagBol>().GetAll().Count();

            ViewBag.TamamlananDonem = _uow.GetRepo<tbl_Donem>().GetAll_ByParam(x => x.SicakSuOkumaDurum == true).Count();

            ViewBag.AktifDonem = _uow.GetRepo<tbl_Donem>().GetAll_ByParam(x => (x.BitisTarihi >= DateTime.Now && x.BaslangicTarihi <= DateTime.Now) &&
            (!x.SicakSuOkumaDurum == true)
                ).Count();

            var faturaRepo = _uow.GetRepo<tbl_Paylasim>();

            ViewBag.ToplamFatura = faturaRepo.GetAll().Count();

            ViewBag.ToplamFaturaTutar = faturaRepo
                .GetAll()
                .Sum(x => x.FaturaTutar);


            return View();
        }

        public JsonResult AylikFaturaAnalizi()
        {

            var baslangic = DateTime.Now.AddYears(-1);
            var bitis = DateTime.Now.AddMonths(2);

            var faturaList = _uow.GetRepo<tbl_Paylasim>()
                .GetAll_ByParam(x => x.FaturaSonOdemeTarihi >= baslangic && x.FaturaSonOdemeTarihi <= bitis);

            var aylikVeri = faturaList
                .GroupBy(x => x.FaturaSonOdemeTarihi.Value.ToString("yyyy-MM"))
                .Select(g => new
                {
                    Ay = g.Key,
                    Tutar = g.Sum(x => x.FaturaTutar)
                })
                .OrderBy(x => x.Ay)
                .ToList();

            return Json(aylikVeri, JsonRequestBehavior.AllowGet);
        }



    }
}