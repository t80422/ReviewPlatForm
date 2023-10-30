//檢查檔案大小
function checkFileSize(inputElement, maxSize) {
    $(inputElement).change(function () {
        if (this.files[0].size > maxSize) {
            alert("檔案大小超過" + (maxSize / 1000000) + "MB");
            $(this).val('');
        }
    });
}

//匯入檔案
$('#export_file').on('change', function (e) {
    let file = e.target.files[0];
    if (file) {
        var data = new FormData();
        data.append("file", file);
        fetch('@Url.Action("Import","Industry")', {
            method: 'POST',
            body: data,
        })
            .then(res => res.text())
            .then(data => {
                var res = JSON.parse(data);
                alert(res);
                //if (res.success == "1") {
                //    alert("匯入成功");
                //} else {
                //    alert("匯入失敗");
                //}
            })
    }
    else {
        alert("沒有檔案");
    }
})

//function openPopup(id, type) {
//    let popupClass, actionUrl;

//    //刪除視窗
//    if (type == 'delete') {
//        popupClass = '.js-del-popup';
//        actionUrl = '@Url.Action("Delete")' + "?id=" + id;
//    }

//    $(".popup-overlay," + popupClass).fadeIn();
//    $(".popup-overlay, .popup-btn .btn-cancel, .popup-btn .btn-submit").off('click');
//    $(".popup-overlay, " + popupClass + " .popup-btn .btn-cancel").on('click', function () {
//        $(".popup-overlay, " + popupClass).fadeOut();
//    });
//    $(popupClass + ' .popup-btn .btn-submit').on('click', function () {
//        window.location.href = actionUrl;
//    });
//}