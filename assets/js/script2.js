function readURL(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#previewImg').attr('src', e.target.result);
        };
        reader.readAsDataURL(input.files[0]);
    }
}

$(function(){
    // 選單下拉
    // $('.nav-item').on('click',function(){
    //     console.log($(this).find('button'))
    //     console.log($(this).find('button').hasClass('is-active'))
    // })
    // // $('.nav-item > a , .nav-item > button').on('click',function(){
    // //     console.log($(this).hasClass('is-active'))
    // //     // if($(this).hasClass('is-active')){
    // //     //     console.log(123)
    // //     //     console.log($(this))
    // //     //     $('.nav-item > a , .nav-item > button').removeClass('is-active');
    // //     //     $(this).addClass('is-active');
    // //     //     $('.nav-item > a , .nav-item > button').siblings('.button-nav').slideUp();
    // //     //     $(this).siblings('.button-nav').slideDown();
    // //     // }
    // // })
    // $('.button-nav a').on('click',function(){
    //     $('.button-nav a').removeClass('is-active')
    //     $(this).addClass('is-active')
    // })
})

// 檔案上傳
$(function(){
    $('.onfile').on('click',function(){
        $(this).siblings('input[type="file"]').click()
    })
    $('.input.file > input[type="file"]').on('change',function(){        
        if(this.files && this.files[0]){
            $(this).siblings('span').text(this.files[0].name)
        }
    })
})
// textarea 依內容擴張	
$(function(){
	var aa = document.getElementsByClassName('textarea-autoheigh')
		for(var i = 0 ; i < aa.length;i++){
				autogrow(aa[i])
		}
})
function autogrow(textarea){
	var adh = textarea.clientHeight
	adh = Math.max(textarea.scrollHeight,adh)
	if (adh > textarea.clientHeight){
			textarea.style.height = adh+'px'
	}
}