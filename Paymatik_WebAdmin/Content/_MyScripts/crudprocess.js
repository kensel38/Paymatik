var _control;

//- Show Modal
function Show_Modal(id) {

    $(".modal-body").load('/' + _control + '/EkleDuzenle?id=' + id);

    if (id == 0) {
        $(".modal-title").html("Kayıt Ekle");
    } else {
        $(".modal-title").html("Kayıt Düzenle");
    }
}

//- Show Modal
function Show_Modal_Param(id,param) {

    $(".modal-body").load('/' + _control + '/EkleDuzenle?id=' + id + '&' + param);

    if (id == 0) {
        $(".modal-title").html("Kayıt Ekle");
    } else {
        $(".modal-title").html("Kayıt Düzenle");
    }
}

function Show_Modal_URL(id, url) {

    $(".modal-body").load(url);

    if (id == 0) {
        $(".modal-title").html("Kayıt Ekle");
    } else {
        $(".modal-title").html("Kayıt Düzenle");
    }
}

//- Show Modal

// Delete Process

function Delete_Confirm(_id) {
    $.ajax({
        type: 'get',
        url: '/' + _control + '/Sil_Onay?id=' + _id,
        success: function (data) {
            if (data) {
                fireText = "<div style='color:red'> <b> Dikkat! <b> <br /> Kayda Bağlı Alt Tanımlamalar Bulunmaktadır.<br/> <small>Kaydı Silerseniz Kayda Bağlı Alt Tanımlamalarda (Bağımsız Bölümler, Paylaşımlar vs.) Sistem Tarafından Silinecektir !</small> </div> <br/> Kaydı Silmek istediğinize emin misiniz ? ";
                Delete(_id, fireText);
            }
            else {
                Delete(_id);
            }
        }
    });
}

function Delete(_id, fireText = "Kaydı Silmek istediğinize emin misiniz?") {

    Swal.fire({
        title: 'Kayıt Silinecek!',
        //text: fireText,
        html: fireText,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Evet, Sil &#x2713;',
        cancelButtonText: 'İptal !',
        closeOnConfirm: false
    }).then(function (result) {
        if (result.isConfirmed) {
            $.ajax({
                type: 'post',
                url: '/' + _control + '/Sil?id=' + _id,
                success: function (data) {
                    if (data) {
                        Swal.fire('Silindi !', 'Kayıt Silme İşlemi Başarılı &#x2713;', 'success');
                        $("#tr_" + _id).fadeOut($("#tr_" + _id).remove());
                    }
                    else {
                        Swal.fire('Hata Oluştu!', 'İşlem sırasında hata oluştu?', 'warning');
                        return false;
                    }
                }
            });
        } else {
            Swal.fire('İptal !', 'İşlemden Vazgeçildi...');
        }
    });
};

function Delete_URL(id, url) {
    Swal.fire({
        title: 'Kayıt Silinecek!',
        text: "Kaydı Silmek istediğinize emin misiniz?",
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Evet, Sil &#x2713;',
        cancelButtonText: 'İptal !',
        closeOnConfirm: false
    }).then(function (result) {
        if (result.isConfirmed) {
            $.ajax({
                type: 'post',
                url: url,
                success: function (data) {
                    if (data) {
                        Swal.fire('Silindi !', 'Kayıt Silme İşlemi Başarılı &#x2713;', 'success');
                        $("#tr_" + id).remove();
                    }
                    else {
                        Swal.fire('Hata Oluştu!', 'İşlem sırasında hata oluştu?', 'warning');
                        return false;
                    }
                }
            });
        } else {
            Swal.fire('İptal !', 'İşlemden Vazgeçildi...');
        }
    });
};
//- Delete Process

// Save Process
function Add() {
    $("#loaderDiv").show();
    var entity = $("#form_entity").serialize();

    $.ajax({
        type: "POST",
        url: '/' + _control + '/EkleDuzenle',
        data: entity,
        success: function () {
            $("#loaderDiv").hide();
            $("#_modal .close").click();

            $(function () {
                var Toast = Swal.mixin({
                    toast: true,
                    position: 'top-end',
                    showConfirmButton: false,
                    timer: 2000
                });
                Toast.fire({
                    icon: 'success',
                    title: 'Bilgiler Başarı ile Kaydedildi...'
                },
                    setTimeout(function () {
                        window.location.href = '/' + _control + '/Index'
                    }, 1000)
                )
            });
        }
    })
}

function Add_2(url, entity,tablename) {

    $.ajax({
        type: 'post',
        url: url,
        data: entity,
        success: function (data) {
            console.log(data);

            $(tablename).last().append(data);

            $(function () {
                var Toast = Swal.mixin({
                    toast: true,
                    position: 'top-end',
                    showConfirmButton: false,
                    timer: 2000
                });
                Toast.fire({
                    icon: 'success',
                    title: 'Bilgiler Başarı ile Kaydedildi...'
                })
            });

        }
    });


}


function Add_URL(url, refreshURL) {
    $("#loaderDiv").show();
    var entity = $("#form_entity").serialize();

    $.ajax({
        type: "POST",
        url: url,
        data: entity,
        success: function () {
            $("#loaderDiv").hide();
            $("#_modal .close").click();

            $(function () {
                var Toast = Swal.mixin({
                    toast: true,
                    position: 'top-end',
                    showConfirmButton: false,
                    timer: 2000
                });
                Toast.fire({
                    icon: 'success',
                    title: 'Bilgiler Başarı ile Kaydedildi...'
                },
                    setTimeout(function () { window.location.href = refreshURL }, 1000)
                )
            });
        }
    })
}

function Add_URL_notRefresh(url) {
    $("#loaderDiv").show();
    var entity = $("#form_entity").serializeArray();
       
    $.ajax({
        type: "POST",
        url: url,
        data: entity,
        success: function () {           

            $("#loaderDiv").hide();
            $("#_modal .close").click();


            $(function () {
                var Toast = Swal.mixin({
                    toast: true,
                    position: 'top-end',
                    showConfirmButton: false,
                    timer: 2000
                });
                Toast.fire({
                    icon: 'success',
                    title: 'Bilgiler Başarı ile Kaydedildi...'
                }                
                )
            });
        }
    })
}
//- Save Process

// Dropdownlist Change
function SelectChange(_selectID,targetDDL, url) {

    if (_selectID == null || _selectID == '') {
        $(targetDDL).empty();
        $(targetDDL).append($("<option></option>").attr("value", "").text("--Seçiniz--"));
    } else {
    $.ajax({
        type: "POST",
        url: url,       
        success: function (result) {
            if (result != undefined) {
                $(targetDDL).empty();
               // $(targetDDL).append($("<option></option>").attr("value", "").text("-- Select State --"));
                $.each(result, function (index, elem) {
                    if (index == 0) {
                        $(targetDDL).append($("<option></option>")
                            .attr("selected", "selected")
                            .attr("value", elem.value)
                            .text(elem.text));
                    }
                    else {
                        $(targetDDL).append($("<option></option>").attr("value", elem.value).text(elem.text));
                    }
                });
               // $(targetDDL).change();
            }
        }
    });
    }
}
// Dropdownlist Change


// Chart - Grafik Oluştur
function GrafikCiz(url, chartDiv, chartType, legendDisplay, legendPosition) {
    $.ajax({
        type: "POST",
        url: url,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (chData) {
            var aData = chData;
            var aLabels = aData[0];
            var aDatasets1 = aData[1];

            var dataT = {
                labels: aLabels,
                datasets: [{
                    label: aDatasets1,
                    data: aDatasets1,
                    fill: false,
                    backgroundColor: ['#f56954', '#00c0ef', '#00a65a', '#f39c12', '#3c8dbc', '#d2d6de', '#E1BEE7', '#90CAF9', '#80DEEA', '#A5D6A7'],
                }]
            };


            var _chart = $(chartDiv).get(0).getContext('2d')    
            
            var _optionsBar = {
                maintainAspectRatio: false,
                responsive: true,
                legend: {
                    display: legendDisplay,
                    position: legendPosition,
                    labels: {
                        fontColor: 'rgb(0, 0, 0)',
                    }
                } ,     
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true
                        }
                    }]
                },

            };

            var _optionsDought = {
                maintainAspectRatio: false,
                responsive: true,
                legend: {
                    display: legendDisplay,
                    position: legendPosition,
                    labels: {
                        fontColor: 'rgb(0, 0, 0)',
                    }
                }
            };

            var _options = legendDisplay == false ? _optionsBar : _optionsDought;

            new Chart(_chart, {
                type: chartType,
                data: dataT,
                options: _options,
            });

        }
    });
}

// Chart - Grafik Oluştur
