function binaGorseliniGuncelle(daireler) {
    const binaDiv = document.getElementById("bina-gorseli");
    binaDiv.innerHTML = "";

    const maxGenislik = binaDiv.clientWidth * 0.8; // Taşmayı önlemek için %80 olarak ölçekleme
    const cepheSirasi = ["Batı", "Kuzey Batı", "Kuzey", "Kuzey Doğu", "Doğu", "Güney Doğu", "Güney", "Güney Batı"];
    const katlar = [...new Set(daireler.map(d => d.KatNo))].sort((a, b) => b - a);

    // Her katın toplam genişliğini belirleme
    const katGenislikleri = {};
    let maxKatGenislik = 0;

    katlar.forEach(katNo => {
        let toplamMetrekare = daireler
            .filter(d => d.KatNo === katNo)
            .reduce((toplam, daire) => toplam + daire.Metrekare, 0);

        katGenislikleri[katNo] = toplamMetrekare;
        if (toplamMetrekare > maxKatGenislik) {
            maxKatGenislik = toplamMetrekare;
        }
    });

    // Maksimum genişlik baz alınarak katlar yüzdesel olarak ölçekleniyor
    for (let katNo in katGenislikleri) {
        katGenislikleri[katNo] = (katGenislikleri[katNo] / maxKatGenislik) * maxGenislik;
    }

    // Çatı genişliği en geniş katın alanına eşit olacak
    const maxKatNo = Math.max(...katlar); // En üst kat numarasını alır
    const catiGenisligi = katGenislikleri[maxKatNo]; // En üst katın genişliği


    // Çatı
    const catiWrapper = document.createElement("div");
    catiWrapper.className = "kat-wrapper";

    const labelCati = document.createElement("div");
    labelCati.className = "kat-label";
   
    const catiDiv = document.createElement("div");
    catiDiv.className = "cati";
    catiDiv.style.width = `${catiGenisligi}px`;

    catiWrapper.appendChild(labelCati);
    catiWrapper.appendChild(catiDiv);
    binaDiv.appendChild(catiWrapper);

    katlar.forEach(katNo => {
        const katWrapper = document.createElement("div");
        katWrapper.className = "kat-wrapper";

        const labelKat = document.createElement("div");
        labelKat.className = "kat-label";
        labelKat.innerText = `${katNo}. Kat`;

        const katDiv = document.createElement("div");
        katDiv.className = "kat";

        if (katNo < 0) katDiv.classList.add("eksi-kat");

        const dairelerBuKatta = daireler
            .filter(d => d.KatNo === katNo)
            .sort((a, b) => cepheSirasi.indexOf(a.Cephe) - cepheSirasi.indexOf(b.Cephe));

        let toplamGenislik = 0;
        const katGenisligi = katGenislikleri[katNo];

        dairelerBuKatta.forEach(daire => {
            const daireDiv = document.createElement("div");
            daireDiv.className = "daire";
            if (daire.OrtakAlan === true) daireDiv.classList.add("OrtakAlan");

            let yuzdeselGenislik = (daire.Metrekare / katGenislikleri[katNo]) * katGenisligi;
            daireDiv.style.width = `${yuzdeselGenislik}px`;
            toplamGenislik += yuzdeselGenislik;

            daireDiv.innerHTML = `
                No: ${daire.KapiNo}<br/>
                <small> ${daire.BagBolTur} (${daire.Cephe}) </small>
                <a class='btn btn-outline-primary btn-xs' title='Düzenle' 
                   onclick=Show_Modal_URL(${daire.ID},'/BagBol/BagBol_Duzenle?id=${daire.ID}') 
                   data-toggle='modal' data-target='#_modal'>
                   <span class='far fa-edit'></span></a>
            `;

            katDiv.appendChild(daireDiv);
        });

        katDiv.style.width = `${katGenisligi}px`;
        katWrapper.appendChild(labelKat);
        katWrapper.appendChild(katDiv);
        binaDiv.appendChild(katWrapper);

        // ===> ZEMİN ÇİZGİ EKLEME KISMI
        if (katNo == 0) {
            const zeminCizgi = document.createElement("div");
            zeminCizgi.className = "zemin-cizgi";
            zeminCizgi.style.width = `${binaDiv.clientWidth }px`; // %110 kadar
            binaDiv.appendChild(zeminCizgi);
        }

    });
}