using BL.constants;
using DAL;
using EL;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Paymatik_WebAdmin.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;


namespace Paymatik_WebAdmin.Controllers
{
    public class PaylasimController : Controller
    {
        UnitOfWork _uow = null;
        public PaylasimController() => _uow = new UnitOfWork();

        // GET: Paylasim
        public ActionResult Index()
        {
            return View(_uow.GetRepo<tbl_Paylasim>().GetAll());
        }


        [HttpGet]
        public ActionResult EkleDuzenle(int id)
        {
            ViewBag.Binalar = _uow.GetRepo<tbl_Bina>().GetAll();

            var ent = _uow.GetRepo<tbl_Paylasim>().GetByID(id);

            if (ent != null)
            {
                // Mevcut kayıt güncelleme ise
                var donemler = _uow.GetRepo<tbl_Donem>().GetAll_ByParam(x => x.BinaId == ent.BinaId).OrderByDescending(x => x.DonemAdi).ToList();
                ViewBag.Donem = new SelectList(donemler, "ID", "DonemAdi", ent.DonemID);
                return PartialView("_paylasimEkle", ent);
            }
            else
            {
                // Yeni kayıt
                ViewBag.Donem = new SelectList(new List<tbl_Donem>(), "ID", "DonemAdi");
                return PartialView("_paylasimEkle", new tbl_Paylasim
                {
                    SuSicakligi = 50 // Yeni kayıt ise başlangıçta 50 gelsin
                });
            }
        }

        [HttpGet]
        public JsonResult GetDonemlerByBina(int binaId)
        {
            var donemler = _uow.GetRepo<tbl_Donem>().GetAll_ByParam(x => x.tbl_Bina.ID == binaId).OrderByDescending(x => x.DonemAdi).Select(d => new
            {
                DonemID = d.ID,
                DonemAdi = d.DonemAdi
            }).OrderBy(x => x.DonemID).ToList();
            return Json(donemler, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EkleDuzenle(tbl_Paylasim entity)
        {
            if (entity.ID == 0)
            {
                _uow.GetRepo<tbl_Paylasim>().Add(entity);
            }
            else
            {
                _uow.GetRepo<tbl_Paylasim>().Update(entity);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult Sil_Onay(int id)
        {
            var entity = _uow.GetRepo<tbl_Paylasim>().GetByID(id);

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
            int _result = _uow.GetRepo<tbl_Paylasim>().Delete(id);
            if (_result > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult Detay(int PaylasimId)
        {
            ViewBag.PaylasimId = PaylasimId;
            PaylasimDetayViewModel viewModel = PaylasimDetayViewModelOlustur(PaylasimId);
            return View(viewModel);
        }

        private PaylasimDetayViewModel PaylasimDetayViewModelOlustur(int PaylasimId)
        {
            var _paylasim = _uow.GetRepo<tbl_Paylasim>().GetByID(PaylasimId);
            var _bagbolList = _uow.GetRepo<tbl_BagBol>().GetAll_ByParam(x => x.BinaId == _paylasim.BinaId);
            var _sayaclar = _uow.GetRepo<tbl_SayacOkuma>().GetAll_ByParam(x => x.tbl_BagBol.BinaId == _paylasim.BinaId && x.tbl_Donem.ID == _paylasim.DonemID);

            double _toplamMetrekare = _bagbolList.Sum(b => b.Metrekare ?? 0);
            double _toplamKalorimetre = _bagbolList.Sum(b => _sayaclar.FirstOrDefault(s => s.BagBolId == b.ID && s.SayacTuru == SayacTurleri.DogalGaz)?.GuncelDeger
                        - _sayaclar.FirstOrDefault(s => s.BagBolId == b.ID && s.SayacTuru == SayacTurleri.DogalGaz)?.OncekiDeger ?? 0);
            double _toplamSicakSu = _bagbolList.Sum(b => _sayaclar.FirstOrDefault(s => s.BagBolId == b.ID && s.SayacTuru == SayacTurleri.SicakSu)?.GuncelDeger
                        - _sayaclar.FirstOrDefault(s => s.BagBolId == b.ID && s.SayacTuru == SayacTurleri.SicakSu)?.OncekiDeger ?? 0);

            double _yakitBirimFiyati = (double)(_paylasim.FaturaTutar / _paylasim.SayactanOkunanHacim);


            var detayList = new List<PaylasimDetayItem>();

            // 1. Hesaplamaları tüm daireler için yap (ortak alanlar dahil).
            foreach (var bagbol in _bagbolList)
            {
                var kalorimetre = _sayaclar.FirstOrDefault(s => s.BagBolId == bagbol.ID && s.SayacTuru == SayacTurleri.DogalGaz);
                var sicakSu = _sayaclar.FirstOrDefault(s => s.BagBolId == bagbol.ID && s.SayacTuru == SayacTurleri.SicakSu);

                double kalorimetreFark = (kalorimetre?.GuncelDeger ?? 0) - (kalorimetre?.OncekiDeger ?? 0);
                double sicakSuFark = (sicakSu?.GuncelDeger ?? 0) - (sicakSu?.OncekiDeger ?? 0);

                double isinmaOrtak = (double)_paylasim.FaturaTutar * 0.3 * (bagbol.Metrekare ?? 0) / _toplamMetrekare;
                double isinmaDaire = (double)_paylasim.FaturaTutar * 0.7 * kalorimetreFark / _toplamMetrekare;

                double suIsitmaBedeli = 1.2 * _toplamSicakSu * ((int)_paylasim.SuSicakligi - 10) / 1000 * _yakitBirimFiyati;
                double suBedeli = sicakSuFark * (double)_paylasim.SogukSuFiyat; // soğuksu birim fiyatı

                detayList.Add(new PaylasimDetayItem
                {
                    bagbolID = bagbol.ID,
                    KatNo = bagbol.KatNo ?? 0,
                    Cephe = bagbol.Cephe,
                    KapiNo = bagbol.KapiNo,
                    Metrekare = bagbol.Metrekare ?? 0,
                    BagBolTur = bagbol.BagBolTur,
                    IsOrtakAlan = bagbol.OrtakAlan,
                    KalorimetreOnceki = kalorimetre?.OncekiDeger ?? 0,
                    KalorimetreGuncel = kalorimetre?.GuncelDeger ?? 0,
                    SicakSuOnceki = sicakSu?.OncekiDeger ?? 0,
                    SicakSuGuncel = sicakSu?.GuncelDeger ?? 0,
                    IsinmaOrtak = isinmaOrtak,
                    IsinmaDaire = isinmaDaire,
                    SicakSuIsitma = suIsitmaBedeli * (sicakSuFark / _toplamSicakSu),
                    SicakSuTuketim = suBedeli
                });
            }

            //2. Ortak alan olan dairelerin ToplamBorc toplamını Diğerlerine metrekareleri oranında dağıt:
            var ortakAlanToplamBorcu = detayList.Where(x => x.IsOrtakAlan == true).Sum(x => x.ToplamBorc);
            var ortakOlmayanToplamMetrekare = detayList.Where(x => x.IsOrtakAlan != true).Sum(x => x.Metrekare);
            foreach (var item in detayList)
            {
                if (item.IsOrtakAlan != true)
                {
                    var pay = item.Metrekare / ortakOlmayanToplamMetrekare;
                    item.OrtakAlanToplam = ortakAlanToplamBorcu * pay;
                }
                else
                {
                    //// Ortak alanlara sıfır borç yaz
                    //item.IsinmaOrtak = 0;
                    //item.IsinmaDaire = 0;
                    //item.SicakSuIsitma = 0;
                    //item.SicakSuTuketim = 0;
                    //item.OrtakAlanToplam = 0;
                }
            }

            //3. Düşük Kullanım Cezası uygula. 0 veya belirli bir değerin altında ısı tüketen dairelere ceza ver ve onlardan gelen değerleri diğerlerine alanları oranında paylaştır.

            var cezaAlanlar = detayList.Where(x => x.IsOrtakAlan != true && x.KalorimetreFark == 0).ToList();
            var cezaAlmayanlar = detayList.Where(x => x.IsOrtakAlan != true && x.KalorimetreFark > 0).ToList();
            var _faturaOrtalamaDeger = _paylasim.FaturaTutar / detayList.Where(x => x.IsOrtakAlan != true).Count();

            // Ceza alanlara cezaları ekle ve toplam tutarı hesapla
            double cezaToplam = 0;
            foreach (var item in cezaAlanlar)
            {
                var fark = (double)_faturaOrtalamaDeger - item.ToplamBorc;

                if (fark > 0)
                {
                    item.DusukKullanimTutar = fark;
                    cezaToplam += fark;
                }
            }

            // Cezayı Ceza Dağıtım Türüne Göre Paylaştır
            if (_paylasim.CezaDagitimTur == CezaDagitimTur.Genel)
            {
                CezaDagitim_Genel(cezaAlmayanlar, cezaToplam);
            }
            else
            {
                CezaDagitim_Katlar(_bagbolList, cezaAlanlar, cezaAlmayanlar);
            }

            var viewModel = new PaylasimDetayViewModel
            {
                BinaAdi = _paylasim.tbl_Bina.Ad,
                DonemAdi = _paylasim.DonemAdi,
                FaturaTutar = _paylasim.FaturaTutar ?? 0,
                SayactanOkunanHacim = _paylasim.SayactanOkunanHacim ?? 0,
                SuSicakligi = _paylasim.SuSicakligi ?? 0,
                DusukKullanimCeza = _paylasim.DusukKullanimCeza ?? false,
                CezaDagitimTur = _paylasim.CezaDagitimTur,
                SonOdemeTarihi = _paylasim.FaturaSonOdemeTarihi ?? DateTime.MinValue,
                Detaylar = detayList
            };
            return viewModel;
        }

        //Cezayı Diğerlerine Metrekare Oranlı Paylaştır  
        private static void CezaDagitim_Genel(List<PaylasimDetayItem> cezaAlmayanlar, double cezaToplam)
        {
            var toplamMetrekare = cezaAlmayanlar.Sum(x => x.Metrekare);
            foreach (var item in cezaAlmayanlar)
            {
                var oran = item.Metrekare / toplamMetrekare;
                var pay = cezaToplam * oran;

                item.DusukKullanimTutar -= pay; // indirimi negatif olarak yansıt                
            }
        }

        //Cezayı Cezalının Komşu Katlarına Metrekare Oranlı Paylaştır  
        private static void CezaDagitim_Katlar(List<tbl_BagBol> _bagbolList, List<PaylasimDetayItem> cezaAlanlar, List<PaylasimDetayItem> cezaAlmayanlar)
        {
            int _altDagitimOran = 50;
            int _ustDagitimOran = 50;
            int _minkat = _bagbolList.Min(x => x.KatNo) ?? 0; //bina en düşük kat
            int _maxkat = _bagbolList.Max(x => x.KatNo) ?? 0; //bina en yüksek kat

            List<PaylasimDetayItem> uygunAltKat = new List<PaylasimDetayItem>();
            List<PaylasimDetayItem> uygunUstKat = new List<PaylasimDetayItem>();

            foreach (var cezali in cezaAlanlar)
            {
                uygunAltKat.Clear();
                uygunUstKat.Clear();

                // Alt kata doğru kontrol
                if (cezali.KatNo != _minkat)
                {
                    for (int altKat = cezali.KatNo - 1; altKat >= _minkat; altKat--)
                    {
                        var altDaire = cezaAlmayanlar.FirstOrDefault(x => x.KatNo == altKat && x.Cephe == cezali.Cephe);

                        if (altDaire != null)
                        {
                            uygunAltKat.Add(altDaire);
                            break;
                        }
                    }
                    if (uygunAltKat.Count == 0)
                    {
                        for (int altKat = cezali.KatNo - 1; altKat >= _minkat; altKat--)
                        {
                            var altKatTumDaireler = cezaAlmayanlar.Where(x => x.KatNo == altKat).ToList();

                            if (altKatTumDaireler != null)
                            {
                                uygunAltKat.AddRange(altKatTumDaireler);
                                break;
                            }
                        }
                    }
                }

                // Üst kata doğru kontrol   
                if (cezali.KatNo != _maxkat)
                {
                    for (int ustKat = cezali.KatNo + 1; ustKat <= _maxkat; ustKat++)
                    {
                        var ustDaire = cezaAlmayanlar.FirstOrDefault(x => x.KatNo == ustKat && x.Cephe == cezali.Cephe);

                        if (ustDaire != null)
                        {
                            uygunUstKat.Add(ustDaire);
                            break;
                        }
                    }
                    if (uygunUstKat.Count == 0)
                    {
                        for (int ustKat = cezali.KatNo + 1; ustKat <= _maxkat; ustKat++)
                        {
                            var ustKatTumDaireler = cezaAlmayanlar.Where(x => x.KatNo == ustKat).ToList();

                            if (ustKatTumDaireler != null)
                            {
                                uygunUstKat.AddRange(ustKatTumDaireler);
                                break;
                            }
                        }
                    }
                }

                // Dağıtım Oranlarını Kontrol et
                if (uygunAltKat.Count == 0)
                {
                    _ustDagitimOran += _altDagitimOran; // alt kat yoksa hepsini üste ver
                }
                else if (uygunUstKat.Count == 0)
                {
                    _altDagitimOran += _ustDagitimOran;
                }

                // Daire Alanlarına göre dağıtım yap...
                if (uygunAltKat.Count > 0)
                {
                    var toplamAlan = uygunAltKat.Sum(x => x.Metrekare);
                    foreach (var item in uygunAltKat)
                    {
                        var oran = item.Metrekare / toplamAlan;
                        var pay = cezali.DusukKullanimTutar * oran;
                        item.DusukKullanimTutar -= pay;
                    }
                }

                if (uygunUstKat.Count > 0)
                {
                    var toplamAlan = uygunUstKat.Sum(x => x.Metrekare);
                    foreach (var item in uygunUstKat)
                    {
                        var oran = item.Metrekare / toplamAlan;
                        var pay = cezali.DusukKullanimTutar * oran;
                        item.DusukKullanimTutar -= pay;
                    }
                }
            }
        }


        public ActionResult ExportPaylasimDetayToExcel(int paylasimID)
        {
            PaylasimDetayViewModel model = PaylasimDetayViewModelOlustur(paylasimID);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {

                var ws = package.Workbook.Worksheets.Add("Paylaşım Detayı");

                int row = 1;

                // Üst bilgiler
                ws.Cells[row, 1].Value = "Bina Adı";
                ws.Cells[row, 2].Value = model.BinaAdi;
                row++;

                ws.Cells[row, 1].Value = "Dönem";
                ws.Cells[row, 2].Value = model.DonemAdi;
                row++;

                ws.Cells[row, 1].Value = "Fatura Tutarı";
                ws.Cells[row, 2].Value = model.FaturaTutar;
                row++;

                ws.Cells[row, 1].Value = "Sayaçtan Okunan Hacim";
                ws.Cells[row, 2].Value = model.SayactanOkunanHacim;
                row++;

                ws.Cells[row, 1].Value = "Su Sıcaklığı";
                ws.Cells[row, 2].Value = model.SuSicakligi;
                row++;

                ws.Cells[row, 1].Value = "Ceza Dağıtım Türü";
                ws.Cells[row, 2].Value = model.CezaDagitimTur;
                row++;

                ws.Cells[row, 1].Value = "Son Ödeme Tarihi";
                ws.Cells[row, 2].Value = model.SonOdemeTarihi.ToShortDateString();
                row += 2;

                // Başlıklar
                var headers = new[]
                {
                    "D.No", "Alan", "D.Türü",
                    "Kal. İlk", "Kal. Son", "Kal. Fark",
                    "Su İlk", "Su Son", "Su Fark",
                    "Isınma Ortak", "Isınma Daire", "Toplam Isınma",
                    "Isıtma Bedeli", "Su Bedeli", "Toplam Sıcak Su",
                    "Ortak Alanlar", "Düşük Kullanım", "Toplam Borç"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[row, i + 1].Value = headers[i];
                    ws.Cells[row, i + 1].Style.Font.Bold = true;
                    ws.Cells[row, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                row++;

                // Detaylar
                foreach (var item in model.Detaylar)
                {
                    int col = 1;
                    ws.Cells[row, col++].Value = item.KapiNo;
                    ws.Cells[row, col++].Value = item.Metrekare;
                    ws.Cells[row, col++].Value = item.BagBolTur;

                    ws.Cells[row, col++].Value = item.KalorimetreOnceki;
                    ws.Cells[row, col++].Value = item.KalorimetreGuncel;
                    ws.Cells[row, col++].Value = item.KalorimetreFark;

                    ws.Cells[row, col++].Value = item.SicakSuOnceki;
                    ws.Cells[row, col++].Value = item.SicakSuGuncel;
                    ws.Cells[row, col++].Value = item.SicakSuFark;

                    ws.Cells[row, col++].Value = item.IsinmaOrtak;
                    ws.Cells[row, col++].Value = item.IsinmaDaire;
                    ws.Cells[row, col++].Value = item.IsinmaToplam;

                    ws.Cells[row, col++].Value = item.SicakSuIsitma;
                    ws.Cells[row, col++].Value = item.SicakSuTuketim;
                    ws.Cells[row, col++].Value = item.SicakSuToplam;

                    ws.Cells[row, col++].Value = item.OrtakAlanToplam;
                    ws.Cells[row, col++].Value = item.DusukKullanimTutar;
                    ws.Cells[row, col++].Value = item.ToplamBorc;

                    // Renkleme
                    if (item.IsOrtakAlan)
                    {
                        ws.Cells[row, 1, row, headers.Length].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, 1, row, headers.Length].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                    }
                    else if (item.DusukKullanimTutar > 0)
                    {
                        ws.Cells[row, 1, row, headers.Length].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, 1, row, headers.Length].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MistyRose);
                    }


                    row++;
                }

                ws.Cells[1, 1, row, headers.Length].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = "PaylasimDetayi.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }



    }
}