$(function() {
    // nav
    $(".nav-btn").on('click', function() {
        $(this).addClass("is-active");
        $(".aside-wrap").addClass("is-active");
    });

    $(".aside-close").on('click', function() {
        $(".nav-btn").removeClass("is-active");
        $(".aside-wrap").removeClass("is-active");
    });

    //$(".nav-item button").on("click", function(e){
    //    $(this).toggleClass("is-active");
    //    $(this).next(".nav-sub").slideToggle();
    //});

    // check
    $("#checkAll").on('click', function() {
        var checkVal = $('#checkAll').is(":checked");
        if(checkVal == true){
            $(".list-checkbox").attr("checked", true);
            $(".list-checkbox").prop("checked", true);
        }
        else {
            $(".list-checkbox").attr("checked", false);
            $(".list-checkbox").prop("checked", false);
        }
    });


    // gotop
    $(window).scroll(function() {
        if ($(this).scrollTop() > 240) {
            $(".btn-top").fadeIn();
        } else {
            $(".btn-top").fadeOut();
        }
    });
    $(".btn-top").click(function() {
        $("body, html").animate({
            scrollTop: 0
        }, 500);
    });
});
