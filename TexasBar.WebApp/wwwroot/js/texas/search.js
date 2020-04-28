(function($){
    var numFound = 0;
    var resultStart = 0;
    var perPage = 30;
    var resultPages = 0;

    $('#search_btn').click(function(e){
        e.preventDefault();
        var field = $('#search_field');
        field.val(field.val().replace(/ +/, '+'));
        if (location.pathname != '/search.html') {
            window.open('http://'+location.hostname+'/search.html?q='+field.val() );
        } else {
            // var q = location.search.replace('?q=','');
            // search(field.val(),0);
            location.href = '/search.html?q='+field.val();
        }
    });


    function search(q,start) {
        if (!start) {
             start = 0
        }

	if ($(':checked', '.facet-chooser').length === 1) {
	    fq = "resourceType:\""+$(':checked', '.facet-chooser')[0].value+"\""
	} else {
	    fq = ''
	}

          var manualCode = $('body').data('manual-code');

          $.ajax({
              url:'/api/index.php/v1/search/'+manualCode+'/',
            data: {
                'q': 'content:'+q.replace(/(%20)+/, '+'),
                'fq': fq,
                'wt': 'json',
                'start': start,
                'rows': perPage,
                'hl': 'true',
                'hl.fl': 'content',
                'hl.simple.pre': '<em>',
                'hl.simple.post': '</em>',
                'facet': 'true',
                'facet.field': 'resourceType',
            },
            dataType: 'json',

            success: searchResult
        });
    }

    $(function(e){
        if ($('body#search').length === 1) {
            var qParams = location.search.split('?')[1].split('&');
            var q = qParams[0].split('=')[1]

            if (qParams[1]) {
                var qs = qParams[1].split('=')[1];
                resultStart = qs;
            }

            $('#search_field').val(decodeURIComponent(q));
            search(q,resultStart);
        }

    });

    /*
    searchResult = function(data) {
        console.log(data)
    }
    */

    searchResult = function(data) {
        numFound = data.response.numFound;

        resultPages = Math.ceil(numFound/perPage);



        // console.log("totalPages", resultPages)
        // console.log(resultStart)
        buildResultNav();

/* Facet Widget  **********/

        $('.facet-chooser').html('').append($('<li>').html('Include in results: '))
        var i = 0
        var templates = {'practice_notes_checkbox': $('<li>'), 'forms_checkbox': $('<li>')}
        while (i < data.facet_counts.facet_fields.resourceType.length) {
          //var template = $('<li>')
          var lab = $('<label>')
          var chk = $('<input type="checkbox">')
          var name = data.facet_counts.facet_fields.resourceType[i]
          var slug = name.toLowerCase().replace(/ +/, '_')+'_checkbox'
          i++
          var count = data.facet_counts.facet_fields.resourceType[i]
          i++
          lab.attr({'for': slug}).html(name + ' <span>(' + count + ')</span>')
          chk.attr({"id": slug, "name": slug, "value": name, "checked": "checked"})
          if (data.responseHeader.params.fq && !data.responseHeader.params.fq.match(name)) {
	    chk.attr({'checked':false})
	  }
          chk.change(function(e) {
	    e.preventDefault();
            var field = $('#search_field');
            search(field.val(),0);
	  });
          templates[slug].append(chk);
          templates[slug].append(lab);
        }
        $('.facet-chooser').append(templates['practice_notes_checkbox'])
	$('.facet-chooser').append(templates['forms_checkbox'])
        console.log($('.facet-chooser').html())

/**************************/

        var results = $('#results');

        $("#search_result_display").html($("#search_field").val())
        $("title").text('Search Results: ' + $("#search_field").val())
        results.html('');
        console.log(data)
        for(var i = 0; i<data.response.docs.length; i++) {
            var doc = data.response.docs[i]
            //console.log(doc)

            var result = $("<div>");
            result.addClass('result');

            //var path = doc.id.split('/')[1] + '/' + doc.id.split('/')[2];
            //console.log(doc.url)
            var path = doc.url[0].split('/').slice(-(doc.url[0].length - 1)).slice(-2).join('/');

            var a = $("<a>");
            a.attr('href',path);
            a.attr('target','_blank');
            a.html(doc.title[0]);

            var p = $("<p>");
            p.addClass('result_title');
            p.addClass(doc.id)
            p.append(a)

            result.append(p);

            var urlp = $("<p>");
            urlp.html(path);
            urlp.addClass('result_url');
            result.append(urlp);


            //show highlighting
            console.log(data.highlighting[doc.id].content)
            if (data.highlighting[doc.id]) {
                if (data.highlighting[doc.id].content) {
                      var snippet = data.highlighting[doc.id].content;
                    var p = $("<p>");
                    p.html(snippet)

                    result.append(p);
                }

            }


            results.append(result);

        }



    }

    buildResultNav = function() {
        $("#result_nav").html('');
        var c = 0;
        // console.log(resultStart)
        while (c < resultPages) {
            var a = $("<a>");
            a.attr('href','/search.html?q='+$('#search_field').val()+'&start='+c*perPage);
            a.html(c+1);

            if (resultStart == (c*perPage)) {
                // console.log(resultStart, c*perPage)
                a.addClass('current_result_page');

            }

            $("#result_nav").append(a);
            // console.log( c, c*perPage );
            c++
        }
    }


}(jQuery));
