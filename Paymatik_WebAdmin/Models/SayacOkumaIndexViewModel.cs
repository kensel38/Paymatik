using System.Collections.Generic;
using System.Web.Mvc;

namespace Paymatik_WebAdmin.Models
{
    public class SayacOkumaIndexViewModel
    {
        public int BinaId { get; set; }

        public int? SecilenDonemId { get; set; }
        public string OkumaTuru { get; set; } // "SicakSu" | "Karma"

        public List<SelectListItem> Donemler { get; set; } = new List<SelectListItem>();
        public List<SayacOkumaSatirViewModel> OkumaListesi { get; set; } = new List<SayacOkumaSatirViewModel>();
    }
}
