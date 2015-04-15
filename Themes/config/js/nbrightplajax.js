


function nbxget(cmd, selformdiv, target, selformitemdiv, appendreturn)
{
    $.ajaxSetup({ cache: false });

    var cmdupdate = '/DesktopModules/NBright/NBrightPL/XmlConnector.ashx?cmd=' + cmd;
    var values = '';
    if (selformitemdiv == null) {
        values = $.fn.genxmlajax(selformdiv);
    }
    else {
        values = $.fn.genxmlajaxitems(selformdiv, selformitemdiv);
    }
    var request = $.ajax({ type: "POST",
		url: cmdupdate,
		cache: false,
		data: { inputxml: encodeURI(values) }		
	});

    request.done(function(data) {
	    if (data != 'noaction') {
	        if (appendreturn == null) {
	            $(target).children().remove();
	            $(target).html(data).trigger('change');
	        } else
	            $(target).append(data).trigger('change');

	        $.event.trigger({
	            type: "nbxgetcompleted",
	            cmd: cmd
	        });
	    }

	});

	request.fail(function (jqXHR, textStatus) {
		alert("Request failed: " + textStatus);
	});
}


