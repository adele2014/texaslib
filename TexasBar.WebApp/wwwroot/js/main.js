(function($){
  $('.tbb_thumb_logo').click(function(e){
    window.location = "/";
  });

  $('.chapter_content').annotator()
    .annotator('addPlugin', 'Store', {
      // The endpoint of the store on your server.
      prefix: '/api/index.php/v1/annotations',

      // Attach the uri of the current page to all annotations to allow search.
      annotationData: {
        'uri': location.origin + location.pathname
      },

      // This will perform a "search" action when the plugin loads. Will
      // request the last 20 annotations for the current url.
      // eg. /store/endpoint/search?limit=20&uri=http://this/document/only
      loadFromSearch: {
      'uri': location.origin + location.pathname
      }
    });

  $(".chapter_group_nav.collapse > li").click(function(e){
    var cur = $(e.currentTarget);
    cur.siblings().removeClass('current');
    cur.toggleClass('current');
  });

  $(".content_nav").click(function(e){
    $("body").toggleClass("closed");
  })

  var sub_nav_container = $(".sub_nav_container");
  $("#practice_notes_ctrl").click(function(e){
    e.preventDefault();
    sub_nav_container.addClass('chapter');
    sub_nav_container.removeClass('form');
  });
  $("#forms_ctrl").click(function(e){
    e.preventDefault();
    sub_nav_container.addClass('form');
    sub_nav_container.removeClass('chapter');
  });

  var waypoints = $('.FM_TitleSection').waypoint({
    handler: function(direction) {
      var sect = $(this.element).data('sect');
      if (direction === 'down') {
        $('.current_section').removeClass('current_section');
        $(document.getElementById("nav_"+sect)).addClass('current_section')
      }
    },
    context: $('.content')
  })

  var waypoints2 = $('.FM_TitleSection').waypoint({
  handler: function(direction) {
    var sect = $(this.element).data('sect');
    if (direction === 'up') {
        $('.current_section').removeClass('current_section');
        $(document.getElementById("nav_"+sect)).addClass('current_section')
      }
    },
    context: $('.content'),
    offset: -1
  })

  //Main Menu Click Event
  //console.log("working")
  $('a.menu, div.close_side').click(function(e){
  $('.sidebar').toggleClass('move');
  $('.container').toggleClass('move');
  });
$('ul.section_nav a').click(function(e){
  if($(window).width() <= 480){
    $('.sub_sidebar').toggleClass('open');
  }
})


$(".ctrls_container").click(function(e){
  e.preventDefault();
  if($(window).width() <= 480){
    var sub_sidebar = $('.sub_sidebar');
    if(!sub_sidebar.hasClass('open')){
      sub_sidebar.addClass('open')
  }
  }
});

// $('#practice_notes_ctrl').click(function(e){
//  if($(window).width() <= 480){
//    $('.sub_sidebar').toggleClass('open');
//    $('ul.section_nav a').click(function(e){
//      $('.sub_sidebar').removeClass('open');
//    });
//  }
// });
// $('#forms_ctrl').click(function(e){
//  if($(window).width() <= 480){
//    $('.sub_sidebar').toggleClass('open');
//  }
//});

}(jQuery));
