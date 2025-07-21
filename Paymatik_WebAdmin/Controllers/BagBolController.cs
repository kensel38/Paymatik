using DAL;
using EL;
using System.Linq;
using System.Web.Mvc;

namespace Paymatik_WebAdmin.Controllers
{
    public class BagBolController : Controller
    {
        // Bağımsız Bölümler
        UnitOfWork _uow = null;
        public BagBolController() => _uow = new UnitOfWork();

        // GET: BagBol
        public ActionResult Index(int id)
        {
            ViewBag.Bina = _uow.GetRepo<tbl_Bina>().GetByID(id);
            ViewBag.binaID = id;
            var _tanimlar = _uow.GetRepo<tbl_Tanimlar>().GetAll();
            ViewBag.Cephe = new SelectList(_tanimlar.Where(x => x.Adi == "Cephe"), "Degeri", "Degeri");
            ViewBag.BagBolTur = new SelectList(_tanimlar.Where(x => x.Adi == "BagBolTur"), "Degeri", "Degeri");
            return View();
        }

        public PartialViewResult BagBol_Liste(int id)
        {
            ViewBag.binaID = id;
            var ent = _uow.GetRepo<tbl_BagBol>().GetAll_ByParam(x => x.BinaId == id);

            var _tanimlar = _uow.GetRepo<tbl_Tanimlar>().GetAll();
            ViewBag.Cephe = new SelectList(_tanimlar.Where(x => x.Adi == "Cephe"), "Degeri", "Degeri");
            ViewBag.BagBolTur = new SelectList(_tanimlar.Where(x => x.Adi == "BagBolTur"), "Degeri", "Degeri");

            return PartialView("_bagBol_Liste", ent);
        }

        [HttpPost]
        public JsonResult BagBol_Ekle(tbl_BagBol entity)
        {
            string _row = "";
            if (ModelState.IsValid)
            {
                _uow.GetRepo<tbl_BagBol>().Add(entity);
                string ortakAlan = entity.OrtakAlan == true ? "Evet" : "Hayır";

                _row = "<tr id='tr_" + entity.ID + "'>" +
                    "<td hidden>" + entity.ID + "</td>" +
                    "<td>" + entity.KatNo + "</td>" +
                    "<td>" + entity.Cephe + "</td>" +
                    "<td>" + entity.KapiNo + "</td>" +
                    "<td>" + entity.Metrekare + "</td>" +
                    "<td>" + entity.BagBolTur + "</td>" +
                    "<td>" + ortakAlan + "</td>" +
                    "<td>" +
                    "<div style='text-align:center'>" +
                    "<a class='btn btn-outline-primary btn-xs' title='Düzenle' onclick=Show_Modal_URL(" + entity.ID + ",'/BagBol/BagBol_Duzenle?id=" + entity.ID + "') data-toggle='modal' data-target='#_modal'>" +
                    "<span class='far fa-edit'></span></a>" +
                    "<a class='btn btn-outline-danger btn-xs' title='Kaydı Sil !' data-id=" + entity.ID + " onclick=Delete_URL(" + entity.ID + ",'/BagBol/BagBol_Sil?id=" + entity.ID + "')>" +
                    "<i class='fas fa-trash-alt'></i> </a> " +
                    "</div>" +
                    "</td>" +
                    "</tr>";
            }
            return Json(_row, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult BagBol_Duzenle(int id)
        {
            var _tanimlar = _uow.GetRepo<tbl_Tanimlar>().GetAll();
            var ent = _uow.GetRepo<tbl_BagBol>().GetByID(id);

            ViewBag.Cephe = new SelectList(_tanimlar.Where(x => x.Adi == "Cephe"), "Degeri", "Degeri", ent.Cephe);
            ViewBag.BagBolTur = new SelectList(_tanimlar.Where(x => x.Adi == "BagBolTur"), "Degeri", "Degeri", ent.BagBolTur);

            return PartialView("_bagBol_Duzenle", ent);
        }

        [HttpPost]
        public JsonResult BagBol_Duzenle(tbl_BagBol entity)
        {
            _uow.GetRepo<tbl_BagBol>().Update(entity);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BagBol_Sil(int id)
        {
            int _result = _uow.GetRepo<tbl_BagBol>().Delete(id);
            if (_result > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetBagBolListesi(int BinaId)
        {
            var ent = _uow.GetRepo<tbl_BagBol>().GetAll_ByParam(x => x.BinaId == BinaId).Select(x => new
            {
                x.ID,
                x.BinaId,
                x.KatNo,
                x.Cephe,
                x.KapiNo,
                x.Metrekare,
                x.BagBolTur,
                x.OrtakAlan
            })
       .ToList();
            return Json(ent, JsonRequestBehavior.AllowGet);

        }

        public ActionResult OkumaSiralamasi(int id)
        {
            ViewBag.Bina = _uow.GetRepo<tbl_Bina>().GetByID(id);
            return View();
        }

        public PartialViewResult OkumaSiralamasi_Liste(int id)
        {
            ViewBag.binaID = id;
            var ent = _uow.GetRepo<tbl_BagBol>().GetAll_ByParam(x => x.BinaId == id);

            return PartialView("OkumaSiralamasi_Liste", ent);
        }


        [HttpPost]
        public JsonResult UpdateOkumaSirasi(int id, int okumaSirasi)
        {
            var bagbol = _uow.GetRepo<tbl_BagBol>().GetByID(id);

            if (bagbol != null)
            {
                bagbol.OkumaSirasi = okumaSirasi;
                _uow.GetRepo<tbl_BagBol>().Update(bagbol);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }



    }
}