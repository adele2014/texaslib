(function($){

var mid = $('body').data('manualId');

var bookmarkApi = '/api/index.php/v1/bookmarks/';
var bmBtn = $('a.bookmark');

checkBookmarkStatus = function(path) {
  $.ajax(bookmarkApi + mid +'/check',
    {
      data: path,
      type: 'POST',
      success: function(data) {
        if (data !== "0") {
          bmBtn.data('bid',data);
          bmBtn.addClass('stored');
          bmBtn.children('span').html('Saved');
        }
      }
    }
  );
}

saveBookmark = function(path,title) {
  var data = {url: path, title: title}
  $.ajax(bookmarkApi + mid + '/',
    {
      data: data,
      type: 'POST',
      success: function(data){
        $('a.bookmark').data('bid',data);
      }
    }
  );
}

deleteBookmark = function(bid) {
  $.ajax(bookmarkApi + bid,
    {
      type: 'DELETE',
      success: function(data){
        // console.log(data)
      }
    }
  );
}

bmBtn.click(function(e){
  e.preventDefault();
  if (bmBtn.hasClass('stored')) {
    deleteBookmark(bmBtn.data('bid'));
    bmBtn.removeClass('stored');
    bmBtn.children('span').html('Bookmark Page');
  } else {
    //check for search
    var path = location.pathname;
    var title = $('title').html();
    if (location.search.length > 0) {
      path = path + location.search;
    }
    saveBookmark(path,title);
    bmBtn.addClass('stored');
    bmBtn.children('span').html('Saved');
  }
});

//bookmarks page specific
if ($('body#bookmarks').length) {
  var blist = $('#bookmarkList');
  $.getJSON(bookmarkApi + mid + '/', function(data){
    for(var bk in data) {
        var li = $('<li>');
        var xa = $('<a>');
        xa.attr('href','#');
        xa.addClass('bookmarkRemoveLink');
        xa.html('X');
        xa.data('bid',data[bk].id);
        xa.click(function(e){
          deleteBookmark($(e.target).data('bid'));
          $(e.target).parent().remove();
        });
        li.append(xa);
        var a = $('<a>');
        a.attr('href',data[bk].url)
        a.html(data[bk].title);
        li.append(a)
        blist.append(li)
    }
  });
}

checkBookmarkStatus(location.pathname + location.search);

}(jQuery));
