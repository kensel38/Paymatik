using BL.constants;
using DAL;
using EL;
using Paymatik_WebAdmin.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Paymatik_WebAdmin.Controllers
{
    public class SayacController : Controller
    {
        UnitOfWork _uow = null;
        public SayacController() => _uow = new UnitOfWork();

        // GET: Sayac
        public ActionResult Index(int id, int index = 0)
        {
            var bina = _uow.GetRepo<tbl_Bina>().GetByID(id);
            if (bina == null) return HttpNotFound();

            var bagBols = _uow.GetRepo<tbl_BagBol>().GetAll_ByParam(x => x.BinaId == id).OrderBy(x => x.OkumaSirasi).ToList();

            if (index < 0 || index >= bagBols.Count)
                return RedirectToAction("Index", new { id = id, index = 0 });

            var _aktifBagBol = bagBols[index];

            // Dönemler
            var mevcutDonem = _uow.GetRepo<tbl_Donem>().GetAll_ByParam(x => x.BinaId == id && x.Durum != true).OrderBy(x => x.DonemNo).FirstOrDefault() ?? new tbl_Donem();
            var oncekiDonem = _uow.GetRepo<tbl_Donem>().GetAll_ByParam(x => x.BinaId == id && x.DonemNo < mevcutDonem.DonemNo).OrderByDescending(x => x.DonemNo).FirstOrDefault() ?? new tbl_Donem();


            if (mevcutDonem.Durum == true)
                ViewBag.DonemDurum = "Bu Dönem Verileri Girilmiş ! Yeni Dönem Oluşturunuz..";

            // Sayaçlar
            var sicakSuMevcut = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(s => s.BagBolId == _aktifBagBol.ID && s.SayacTuru == SayacTurleri.SicakSu && s.DonemID == mevcutDonem.ID);
            var dogalgazMevcut = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(s => s.BagBolId == _aktifBagBol.ID && s.SayacTuru == SayacTurleri.DogalGaz && s.DonemID == mevcutDonem.ID);
            var sicakSuOnceki = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(s => s.BagBolId == _aktifBagBol.ID && s.SayacTuru == SayacTurleri.SicakSu && s.DonemID == oncekiDonem.ID);
            var dogalgazOnceki = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(s => s.BagBolId == _aktifBagBol.ID && s.SayacTuru == SayacTurleri.DogalGaz && s.DonemID == oncekiDonem.ID);

            var model = new SayacOkumaViewModel
            {
                BinaId = id,
                BagimsizBolumId = _aktifBagBol.ID,
                KatNo = _aktifBagBol.KatNo,
                KapiNo = _aktifBagBol.KapiNo,
                Cephe = _aktifBagBol.Cephe,
                BagBolTur = _aktifBagBol.BagBolTur,
                OkumaSirasi = _aktifBagBol.OkumaSirasi,

                ToplamBagBolSayisi = bagBols.Count,
                MevcutIndex = index,

                DonemID = mevcutDonem.ID,
                DonemAdi = mevcutDonem.DonemAdi,
                SicakSuOnceki = sicakSuOnceki?.GuncelDeger ?? 0,
                SicakSuGuncel = sicakSuMevcut?.GuncelDeger,
                DogalgazOnceki = dogalgazOnceki?.GuncelDeger ?? 0,
                DogalgazGuncel = dogalgazMevcut?.GuncelDeger
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult KaydetSicakSu(SayacOkumaViewModel model)
        {
            var sayac = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(s => s.BagBolId == model.BagimsizBolumId && s.SayacTuru == SayacTurleri.SicakSu && s.DonemID == model.DonemID);

            if (sayac == null)
            {
                sayac = new tbl_SayacOkuma
                {
                    BagBolId = model.BagimsizBolumId,
                    SayacTuru = SayacTurleri.SicakSu,
                    DonemID = model.DonemID,
                    GuncelDeger = model.SicakSuGuncel ?? 0,
                    OncekiDeger = model.SicakSuOnceki ?? 0,
                    OkumaTarihi = DateTime.Now
                };
                _uow.GetRepo<tbl_SayacOkuma>().Add(sayac);
            }
            else
            {
                sayac.GuncelDeger = model.SicakSuGuncel ?? 0;
                sayac.OncekiDeger = model.SicakSuOnceki ?? 0;
                sayac.OkumaTarihi = DateTime.Now;
                _uow.GetRepo<tbl_SayacOkuma>().Update(sayac);
            }
            Donem_Durum_Guncelleme(model);
            TempData["SuccessMessage"] = "Sıcak su verisi kaydedildi.";
            return RedirectToAction("Index", new { id = model.BinaId, index = model.MevcutIndex });
        }

        [HttpPost]
        public ActionResult KaydetDogalgaz(SayacOkumaViewModel model)
        {
            var sayac = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(s => s.BagBolId == model.BagimsizBolumId && s.SayacTuru == SayacTurleri.DogalGaz && s.DonemID == model.DonemID);

            if (sayac == null)
            {
                sayac = new tbl_SayacOkuma
                {
                    BagBolId = model.BagimsizBolumId,
                    SayacTuru = SayacTurleri.DogalGaz,
                    DonemID = model.DonemID,
                    GuncelDeger = model.DogalgazGuncel ?? 0,
                    OncekiDeger = model.DogalgazOnceki ?? 0,
                    OkumaTarihi = DateTime.Now
                };
                _uow.GetRepo<tbl_SayacOkuma>().Add(sayac);
            }
            else
            {
                sayac.GuncelDeger = model.DogalgazGuncel ?? 0;
                sayac.OncekiDeger = model.DogalgazOnceki ?? 0;
                sayac.OkumaTarihi = DateTime.Now;
                _uow.GetRepo<tbl_SayacOkuma>().Update(sayac);
            }

            Donem_Durum_Guncelleme(model);

            TempData["SuccessMessage"] = "Doğalgaz verisi kaydedildi.";
            return RedirectToAction("Index", new { id = model.BinaId, index = model.MevcutIndex });
        }

        private void Donem_Durum_Guncelleme(SayacOkumaViewModel model)
        {
            var donemOkumaSayisi = _uow.GetRepo<tbl_SayacOkuma>().GetAll_ByParam(x => x.tbl_BagBol.BinaId == model.BinaId && x.DonemID == model.DonemID).Count();
            var bagBols = (_uow.GetRepo<tbl_BagBol>().GetAll_ByParam(x => x.BinaId == model.BinaId).Count()) * 2;//2 sayaç okunuyor

            if (donemOkumaSayisi == bagBols)
            {
                var donem = _uow.GetRepo<tbl_Donem>().GetByID(model.DonemID);
                donem.Durum = true;
                _uow.GetRepo<tbl_Donem>().Update(donem);
            }
        }

        public ActionResult DonemOkumaListesi(int binaID, int donemID)
        {
            var ent = _uow.GetRepo<view_BinaSayacOkuma>().GetAll_ByParam(x => x.BinaID == binaID && x.DonemID == donemID);
            ViewBag.Bina = ent.FirstOrDefault();
            return View(ent);
        }

        [HttpGet]
        public ActionResult EkleDuzenle(int id)
        {
            var ent = _uow.GetRepo<tbl_SayacOkuma>().GetByID(id);
            ViewBag.BinaID = ent.tbl_BagBol.BinaId;
            return PartialView("_sayacOkumaDuzenle", ent);
        }

        [HttpPost]
        public ActionResult EkleDuzenle(tbl_SayacOkuma entity, int BinaID)
        {
            _uow.GetRepo<tbl_SayacOkuma>().Update(entity);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Sil_Onay(int id)
        {
            var entity = _uow.GetRepo<tbl_SayacOkuma>().GetByID(id);

            if (entity != null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Sil(int id)
        {
            int _result = _uow.GetRepo<tbl_SayacOkuma>().Delete(id);
            if (_result > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }



    }
}