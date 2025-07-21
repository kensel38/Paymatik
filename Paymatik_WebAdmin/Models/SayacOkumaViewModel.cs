namespace Paymatik_WebAdmin.Models
{
    public class SayacOkumaViewModel
    {
        public int BinaId { get; set; }

        // Bağımsız Bölüm Bilgisi
        public int BagimsizBolumId { get; set; }
        public int? KatNo { get; set; }
        public string Cephe { get; set; }
        public string KapiNo { get; set; }
        public string Metrekare { get; set; }
        public string BagBolTur { get; set; }
        public bool OrtakAlan { get; set; }
        public int OkumaSirasi { get; set; }

        // Navigasyon için Okuma Sıralaması x/y
        public int ToplamBagBolSayisi { get; set; }
        public int MevcutIndex { get; set; } // hangi sırada olduğunu gösterir

        // Sayaç Bilgileri
        public int DonemID { get; set; }
        public string DonemAdi { get; set; }
        public double? SicakSuOnceki { get; set; }
        public double? SicakSuGuncel { get; set; }
        public double? DogalgazOnceki { get; set; }
        public double? DogalgazGuncel { get; set; }
    }

}