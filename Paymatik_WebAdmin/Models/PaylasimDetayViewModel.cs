using System;
using System.Collections.Generic;

namespace Paymatik_WebAdmin.Models
{
    public class PaylasimDetayItem
    {
        public int bagbolID { get; set; }
        public int KatNo { get; set; }
        public string Cephe { get; set; }
        public string KapiNo { get; set; }
        public double Metrekare { get; set; }
        public string BagBolTur { get; set; }
        public bool IsOrtakAlan { get; set; }

        public double KalorimetreOnceki { get; set; }
        public double KalorimetreGuncel { get; set; }
        public double KalorimetreFark => KalorimetreGuncel - KalorimetreOnceki;

        public double SicakSuOnceki { get; set; }
        public double SicakSuGuncel { get; set; }
        public double SicakSuFark => SicakSuGuncel - SicakSuOnceki;

        public double IsinmaOrtak { get; set; }
        public double IsinmaDaire { get; set; }
        public double IsinmaToplam => IsinmaOrtak + IsinmaDaire;

        public double SicakSuIsitma { get; set; }
        public double SicakSuTuketim { get; set; }
        public double SicakSuToplam => SicakSuIsitma + SicakSuTuketim;

        public double OrtakAlanToplam { get; set; }
        public double DusukKullanimTutar { get; set; }
        public double ToplamBorc => IsinmaToplam + SicakSuToplam + OrtakAlanToplam + DusukKullanimTutar;
    }

    public class PaylasimDetayViewModel
    {
        public string BinaAdi { get; set; }
        public string DonemAdi { get; set; }
        public decimal FaturaTutar { get; set; }
        public decimal SayactanOkunanHacim { get; set; }
        public int SuSicakligi { get; set; }
        public decimal SuBirimFiyat { get; set; }
        public bool DusukKullanimCeza { get; set; }
        public string CezaDagitimTur { get; set; }
        public DateTime SonOdemeTarihi { get; set; }

        public List<PaylasimDetayItem> Detaylar { get; set; }
    }


}