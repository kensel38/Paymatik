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
            var ent = _uow.GetRepo<tbl_Bina>().GetByID(id);
            return PartialView("_binaEkleDuzenlePartial", ent);
        }

        [HttpPost]
        public ActionResult EkleDuzenle(tbl_Bina entity)
        {
            if (entity.ID == 0)
            {
                _uow.GetRepo<tbl_Bina>().Add(entity);
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