using DAL;
using EL;
using System.Linq;
using System.Web.Mvc;

namespace Paymatik_WebAdmin.Controllers
{
    public class DonemController : Controller
    {

        UnitOfWork _uow = null;
        public DonemController() => _uow = new UnitOfWork();

        // GET: Donem
        public ActionResult Index(int id)
        {
            ViewBag.Bina = _uow.GetRepo<tbl_Bina>().GetByID(id);
            return View(_uow.GetRepo<tbl_Donem>().GetAll_ByParam(x => x.BinaId == id));
        }

        [HttpGet]
        public ActionResult EkleDuzenle(int id, int? BinaId)
        {
            ViewBag.binaID = BinaId;
            var ent = _uow.GetRepo<tbl_Donem>().GetByID(id);
            return PartialView("_donemEkleDuzenle", ent);
        }

        [HttpPost]
        public ActionResult EkleDuzenle(tbl_Donem entity)
        {
            if (entity.ID == 0)
            {
                int sonDonem = _uow.GetRepo<tbl_Donem>().GetAll_ByParam(x => x.BinaId == entity.BinaId).Select(d => d.DonemNo).Max() ?? 0; // kayıt yoksa 0
                entity.DonemNo = sonDonem + 1;
                _uow.GetRepo<tbl_Donem>().Add(entity);
            }
            else
            {
                entity.DonemNo = (entity.BaslangicTarihi.Value.Year * 100) + entity.BaslangicTarihi.Value.Month;
                _uow.GetRepo<tbl_Donem>().Update(entity);
            }
            return RedirectToAction("Index", new { id = entity.BinaId });
        }

        [HttpGet]
        public JsonResult Sil_Onay(int id)
        {
            var entity = _uow.GetRepo<tbl_Donem>().GetByID(id);

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
            int _result = _uow.GetRepo<tbl_Donem>().Delete(id);
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