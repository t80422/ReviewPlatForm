//檢查檔案大小
function checkFileSize(inputElement) {
    $(inputElement).change(function () {
        if (this.files[0].size > 5000000) {
            alert("檔案大小超過 5MB");
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

