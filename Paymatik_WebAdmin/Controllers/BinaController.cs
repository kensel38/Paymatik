using BL.Helpers;
using DAL;
using EL;
using System.Web.Mvc;

namespace Paymatik_WebAdmin.Controllers
{
    public class BinaController : Controller
    {

        // GET: tbl_Bina
        UnitOfWork _uow = null;
        public BinaController() => _uow = new UnitOfWork();

        // GET: Bina
        public ActionResult Index()
        {
            return View(_uow.GetRepo<tbl_Bina>().GetAll());
        }

        [HttpGet]
        public ActionResult EkleDuzenle(int id)
        {
            tbl_Bina ent = null;
            if (id == 0)
            {
                ent = new tbl_Bina { BinaKodu = "Bk_10" + (_uow.GetRepo<tbl_Bina>().GetAll().Count + 1), SayacsizCezaOrani = (decimal?)1.6f };
            }
            else
            {
                ent = _uow.GetRepo<tbl_Bina>().GetByID(id);
            }
            return PartialView("_binaEkleDuzenlePartial", ent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EkleDuzenle(tbl_Bina entity)
        {
            if (!ModelState.IsValid)
                return PartialView("_binaEkleDuzenlePartial", entity);

            if (entity.ID == 0)
            {
                _uow.GetRepo<tbl_Bina>().Add(entity);

                DonemHelper.DonemleriYilSonunaKadarOlustur(entity.ID, _uow);

            }
            else
            {
                _uow.GetRepo<tbl_Bina>().Update(entity);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult Sil_Onay(int id)
        {
            var entity = _uow.GetRepo<tbl_Bina>().GetByID(id);

            if (entity.tbl_BagBol.Count > 0)
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
            int _result = _uow.GetRepo<tbl_Bina>().Delete(id);
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