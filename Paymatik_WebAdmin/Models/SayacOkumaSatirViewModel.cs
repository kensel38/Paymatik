namespace Paymatik_WebAdmin.Models
{
    public class SayacOkumaSatirViewModel
    {
        public int BinaId { get; set; }
        public int BagimsizBolumId { get; set; }
        public int? KatNo { get; set; }
        public string Cephe { get; set; }
        public string KapiNo { get; set; }
        public string Metrekare { get; set; }
        public string BagBolTur { get; set; }
        public bool OrtakAlan { get; set; }
        public int OkumaSirasi { get; set; }

        public int DonemID { get; set; }
        public string DonemAdi { get; set; }

        public double? SicakSuOnceki { get; set; }
        public double? SicakSuGuncel { get; set; }

        public double? DogalgazOnceki { get; set; }
        public double? DogalgazGuncel { get; set; }
    }
}
