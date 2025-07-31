using BL.constants;
using DAL;
using EL;
using Paymatik_WebAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

public class SayacOkumaController : Controller
{
    private UnitOfWork _uow;

    public SayacOkumaController() => _uow = new UnitOfWork();


    public ActionResult Index(int id) // binaId
    {
        ViewBag.Bina = _uow.GetRepo<tbl_Bina>().GetByID(id);

        var donemler = _uow.GetRepo<tbl_Donem>()
            .GetAll_ByParam(x => x.BinaId == id)
            .OrderByDescending(x => x.DonemNo)
            .Select(x => new SelectListItem
            {
                Value = x.ID.ToString(),
                Text = x.DonemAdi
            }).OrderBy(x => x.Value).ToList();

        ViewBag.Donemler = donemler;

        return View(); // Sayfa açıldığında sadece seçim ekranı gelir
    }

    [HttpPost]
    public PartialViewResult GetOkumaTablosu(int binaId, int donemId)
    {
        // Burada bağımsız bölümler ve geçmiş sayaç okuma değerleriyle tabloyu oluşturacağız
        var viewModelList = OkumaViewModelHazirla(binaId, donemId);
        return PartialView("_sayacOkuma_Liste", viewModelList);
    }

    private List<SayacOkumaViewModel> OkumaViewModelHazirla(int binaId, int donemId)
    {
        var bagbolList = _uow.GetRepo<tbl_BagBol>()
            .GetAll_ByParam(x => x.BinaId == binaId)
            .OrderBy(x => x.OkumaSirasi)
            .ToList();

        var donem = _uow.GetRepo<tbl_Donem>().GetByID(donemId);
        if (donem == null) return new List<SayacOkumaViewModel>();

        int oncekiDonemNo = (int)donem.ID - 1;

        var oncekiDonem = _uow.GetRepo<tbl_Donem>().Get_ByParam(x =>
            x.BinaId == binaId && x.ID == oncekiDonemNo);

        var viewModelList = new List<SayacOkumaViewModel>();

        foreach (var bb in bagbolList)
        {
            // Önceki değerler
            double? sicakOnceki = 0, gazOnceki = 0;

            if (oncekiDonem != null)
            {
                var sicakOkuma = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                    x.BagBolId == bb.ID &&
                    x.DonemID == oncekiDonem.ID &&
                    x.SayacTuru == SayacTurleri.SicakSu);

                var gazOkuma = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                    x.BagBolId == bb.ID &&
                    x.DonemID == oncekiDonem.ID &&
                    x.SayacTuru == SayacTurleri.DogalGaz);

                sicakOnceki = sicakOkuma?.GuncelDeger ?? 0;
                gazOnceki = gazOkuma?.GuncelDeger ?? 0;
            }

            // Mevcut değerler (eğer daha önce bu döneme ait kayıt yapılmışsa)
            var mevcutSicak = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                x.BagBolId == bb.ID &&
                x.DonemID == donem.ID &&
                x.SayacTuru == SayacTurleri.SicakSu);

            var mevcutGaz = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                x.BagBolId == bb.ID &&
                x.DonemID == donem.ID &&
                x.SayacTuru == SayacTurleri.DogalGaz);


            viewModelList.Add(new SayacOkumaViewModel
            {
                BinaId = binaId,
                BagimsizBolumId = bb.ID,
                KatNo = bb.KatNo,
                KapiNo = bb.KapiNo,
                Cephe = bb.Cephe,
                BagBolTur = bb.BagBolTur,
                OrtakAlan = bb.OrtakAlan,
                DonemID = donem.ID,
                DonemAdi = donem.DonemAdi,
                SicakSuOnceki = sicakOnceki,
                SicakSuGuncel = mevcutSicak?.GuncelDeger,
                DogalgazOnceki = gazOnceki,
                DogalgazGuncel = mevcutGaz?.GuncelDeger
            });
        }

        return viewModelList.OrderBy(x => x.KatNo).ThenBy(x => x.KatNo).ToList();
    }

    [HttpPost]
    public ActionResult KaydetEx(int DonemID, int BagBolId, double? SicakSuGuncel, double? DogalgazGuncel)
    {
        try
        {
            var donem = _uow.GetRepo<tbl_Donem>().GetByID(DonemID);
            if (donem == null)
                return Json(new { success = false, message = "Dönem bulunamadı." });

            int oncekiDonemNo = (int)donem.DonemNo - 1;

            // Önceki dönem sıcak su değeri
            var sicakOnceki = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                x.BagBolId == BagBolId &&
                x.SayacTuru == SayacTurleri.SicakSu &&
                x.tbl_Donem.DonemNo == oncekiDonemNo);

            // Önceki dönem doğalgaz değeri
            var gazOnceki = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                x.BagBolId == BagBolId &&
                x.SayacTuru == SayacTurleri.DogalGaz &&
                x.tbl_Donem.DonemNo == oncekiDonemNo);

            // SICAK SU
            if (SicakSuGuncel != null)
            {
                var yeniSicak = new tbl_SayacOkuma
                {
                    BagBolId = BagBolId,
                    DonemID = DonemID,
                    SayacTuru = SayacTurleri.SicakSu,
                    OncekiDeger = sicakOnceki?.GuncelDeger ?? 0,
                    GuncelDeger = SicakSuGuncel.Value,
                    OkumaTarihi = DateTime.Now
                };
                _uow.GetRepo<tbl_SayacOkuma>().Add(yeniSicak);
            }

            // DOĞALGAZ
            if (DogalgazGuncel != null)
            {
                var yeniGaz = new tbl_SayacOkuma
                {
                    BagBolId = BagBolId,
                    DonemID = DonemID,
                    SayacTuru = SayacTurleri.DogalGaz,
                    OncekiDeger = gazOnceki?.GuncelDeger ?? 0,
                    GuncelDeger = DogalgazGuncel.Value,
                    OkumaTarihi = DateTime.Now
                };
                _uow.GetRepo<tbl_SayacOkuma>().Add(yeniGaz);
            }

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    public ActionResult Kaydet(int DonemID, int BagBolId, double? SicakSuGuncel, double? DogalgazGuncel)
    {
        try
        {
            var donem = _uow.GetRepo<tbl_Donem>().GetByID(DonemID);
            if (donem == null)
                return Json(new { success = false, message = "Dönem bulunamadı." });

            int oncekiDonemNo = (int)donem.DonemNo - 1;

            // Önceki dönemler için değerleri alalım (gerekirse yeni kayıt oluştururken kullanacağız)
            var sicakOnceki = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                x.BagBolId == BagBolId &&
                x.SayacTuru == SayacTurleri.SicakSu &&
                x.tbl_Donem.DonemNo == oncekiDonemNo);

            var gazOnceki = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                x.BagBolId == BagBolId &&
                x.SayacTuru == SayacTurleri.DogalGaz &&
                x.tbl_Donem.DonemNo == oncekiDonemNo);

            // SICAK SU: varsa güncelle, yoksa ekle
            if (SicakSuGuncel != null)
            {
                var mevcutSicak = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                    x.BagBolId == BagBolId &&
                    x.DonemID == DonemID &&
                    x.SayacTuru == SayacTurleri.SicakSu);

                if (mevcutSicak != null)
                {
                    mevcutSicak.GuncelDeger = SicakSuGuncel.Value;
                    mevcutSicak.OkumaTarihi = DateTime.Now;
                    _uow.GetRepo<tbl_SayacOkuma>().Update(mevcutSicak);
                }
                else
                {
                    var yeniSicak = new tbl_SayacOkuma
                    {
                        BagBolId = BagBolId,
                        DonemID = DonemID,
                        SayacTuru = SayacTurleri.SicakSu,
                        OncekiDeger = sicakOnceki?.GuncelDeger ?? 0,
                        GuncelDeger = SicakSuGuncel.Value,
                        OkumaTarihi = DateTime.Now
                    };
                    _uow.GetRepo<tbl_SayacOkuma>().Add(yeniSicak);
                }
            }

            // DOĞALGAZ: varsa güncelle, yoksa ekle
            if (DogalgazGuncel != null)
            {
                var mevcutGaz = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                    x.BagBolId == BagBolId &&
                    x.DonemID == DonemID &&
                    x.SayacTuru == SayacTurleri.DogalGaz);

                if (mevcutGaz != null)
                {
                    mevcutGaz.GuncelDeger = DogalgazGuncel.Value;
                    mevcutGaz.OkumaTarihi = DateTime.Now;
                    _uow.GetRepo<tbl_SayacOkuma>().Update(mevcutGaz);
                }
                else
                {
                    var yeniGaz = new tbl_SayacOkuma
                    {
                        BagBolId = BagBolId,
                        DonemID = DonemID,
                        SayacTuru = SayacTurleri.DogalGaz,
                        OncekiDeger = gazOnceki?.GuncelDeger ?? 0,
                        GuncelDeger = DogalgazGuncel.Value,
                        OkumaTarihi = DateTime.Now
                    };
                    _uow.GetRepo<tbl_SayacOkuma>().Add(yeniGaz);
                }
            }

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }


    [HttpPost]
    public ActionResult TopluKaydetEx(List<SayacTopluKayitModel> liste)
    {
        try
        {
            foreach (var item in liste)
            {
                var donem = _uow.GetRepo<tbl_Donem>().GetByID(item.DonemID);
                if (donem == null)
                    continue;

                int oncekiDonemNo = (int)donem.DonemNo - 1;

                // Önceki değerler
                var sicakOnceki = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                    x.BagBolId == item.BagBolId &&
                    x.SayacTuru == SayacTurleri.SicakSu &&
                    x.tbl_Donem.DonemNo == oncekiDonemNo);

                var gazOnceki = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                    x.BagBolId == item.BagBolId &&
                    x.SayacTuru == SayacTurleri.DogalGaz &&
                    x.tbl_Donem.DonemNo == oncekiDonemNo);

                // SICAKSU
                if (item.SicakSuGuncel != null)
                {
                    var yeniSicak = new tbl_SayacOkuma
                    {
                        BagBolId = item.BagBolId,
                        DonemID = item.DonemID,
                        SayacTuru = SayacTurleri.SicakSu,
                        OncekiDeger = sicakOnceki?.GuncelDeger ?? 0,
                        GuncelDeger = item.SicakSuGuncel.Value,
                        OkumaTarihi = DateTime.Now
                    };
                    _uow.GetRepo<tbl_SayacOkuma>().Add(yeniSicak);
                }

                // DOĞALGAZ
                if (item.DogalgazGuncel != null)
                {
                    var yeniGaz = new tbl_SayacOkuma
                    {
                        BagBolId = item.BagBolId,
                        DonemID = item.DonemID,
                        SayacTuru = SayacTurleri.DogalGaz,
                        OncekiDeger = gazOnceki?.GuncelDeger ?? 0,
                        GuncelDeger = item.DogalgazGuncel.Value,
                        OkumaTarihi = DateTime.Now
                    };
                    _uow.GetRepo<tbl_SayacOkuma>().Add(yeniGaz);
                }
            }

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public ActionResult TopluKaydet(List<SayacTopluKayitModel> liste)
    {
        try
        {
            foreach (var item in liste)
            {
                // Güncel değer boşsa geç
                if (item.SicakSuGuncel == null && item.DogalgazGuncel == null)
                    continue;

                var donem = _uow.GetRepo<tbl_Donem>().GetByID(item.DonemID);
                if (donem == null) continue;

                int oncekiDonemNo = (int)donem.DonemNo - 1;

                var sicakOnceki = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                    x.BagBolId == item.BagBolId &&
                    x.SayacTuru == SayacTurleri.SicakSu &&
                    x.tbl_Donem.DonemNo == oncekiDonemNo);

                var gazOnceki = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                    x.BagBolId == item.BagBolId &&
                    x.SayacTuru == SayacTurleri.DogalGaz &&
                    x.tbl_Donem.DonemNo == oncekiDonemNo);

                // Sıcak su
                if (item.SicakSuGuncel != null)
                {
                    var mevcutSicak = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                        x.BagBolId == item.BagBolId &&
                        x.DonemID == item.DonemID &&
                        x.SayacTuru == SayacTurleri.SicakSu);

                    if (mevcutSicak != null)
                    {
                        mevcutSicak.GuncelDeger = item.SicakSuGuncel.Value;
                        mevcutSicak.OkumaTarihi = DateTime.Now;
                        _uow.GetRepo<tbl_SayacOkuma>().Update(mevcutSicak);
                    }
                    else
                    {
                        var yeniSicak = new tbl_SayacOkuma
                        {
                            BagBolId = item.BagBolId,
                            DonemID = item.DonemID,
                            SayacTuru = SayacTurleri.SicakSu,
                            OncekiDeger = sicakOnceki?.GuncelDeger ?? 0,
                            GuncelDeger = item.SicakSuGuncel.Value,
                            OkumaTarihi = DateTime.Now
                        };
                        _uow.GetRepo<tbl_SayacOkuma>().Add(yeniSicak);
                    }
                }

                // Doğalgaz
                if (item.DogalgazGuncel != null)
                {
                    var mevcutGaz = _uow.GetRepo<tbl_SayacOkuma>().Get_ByParam(x =>
                        x.BagBolId == item.BagBolId &&
                        x.DonemID == item.DonemID &&
                        x.SayacTuru == SayacTurleri.DogalGaz);

                    if (mevcutGaz != null)
                    {
                        mevcutGaz.GuncelDeger = item.DogalgazGuncel.Value;
                        mevcutGaz.OkumaTarihi = DateTime.Now;
                        _uow.GetRepo<tbl_SayacOkuma>().Update(mevcutGaz);
                    }
                    else
                    {
                        var yeniGaz = new tbl_SayacOkuma
                        {
                            BagBolId = item.BagBolId,
                            DonemID = item.DonemID,
                            SayacTuru = SayacTurleri.DogalGaz,
                            OncekiDeger = gazOnceki?.GuncelDeger ?? 0,
                            GuncelDeger = item.DogalgazGuncel.Value,
                            OkumaTarihi = DateTime.Now
                        };
                        _uow.GetRepo<tbl_SayacOkuma>().Add(yeniGaz);
                    }
                }
            }

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }



}
