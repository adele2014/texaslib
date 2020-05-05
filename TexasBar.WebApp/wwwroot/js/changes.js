if ($('#change-notes').length === 1) {

  $( window ).resize(function() {
    $('.content').offset({
      top: $('.notice').height()+$('.notice').offset().top
    });
  });

  $('.content').offset({
    top: $('.notice').height()+$('.notice').offset().top
  });

}
