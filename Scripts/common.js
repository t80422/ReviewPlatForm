//檢查檔案大小
function checkFileSize(inputElement, maxSize) {
    $(inputElement).change(function () {
        if (this.files[0].size > maxSize) {
            alert("檔案大小超過" + (maxSize / 1000000) + "MB");
            $(this).val('');
        }
    });
}