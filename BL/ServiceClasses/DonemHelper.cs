using DAL;
using EL;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BL.Helpers
{
    public static class DonemHelper
    {
        public static void DonemleriYilSonunaKadarOlustur(int binaId, UnitOfWork uow)
        {
            DateTime bugun = DateTime.Now;
            int yil = bugun.Year;
            int baslangicAy = bugun.Month;

            List<tbl_Donem> _donemler = new List<tbl_Donem>();

            for (int ay = baslangicAy; ay <= 12; ay++)
            {
                DateTime baslangic = new DateTime(yil, ay, 1);
                DateTime bitis = baslangic.AddMonths(1).AddDays(-1);
                string ad = baslangic.ToString("yyyy-MMMM", new CultureInfo("tr-TR"));

                // Bu dönemde aynı bina için kayıt var mı?
                var mevcut = uow.GetRepo<tbl_Donem>().Get_ByParam(
                    x => x.BinaId == binaId && x.DonemAdi == ad
                );

                if (mevcut != null)
                    continue;

                // Yeni dönem oluştur
                var yeniDonem = new tbl_Donem
                {
                    BinaId = binaId,
                    DonemNo = (yil * 100) + ay, //202507
                    DonemAdi = ad,
                    BaslangicTarihi = baslangic,
                    BitisTarihi = bitis,
                    Durum = false // false → okunmadı, açık
                };

                _donemler.Add(yeniDonem);
            }
            uow.GetRepo<tbl_Donem>().AddRange(_donemler);
        }
    }
}
