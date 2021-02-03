function getMainSiteHost(){
	var host = document.location.hostname;
	if(document.location.hostname.indexOf('localhost') > -1){
		return "http://localhost:8080";
	}else if(host.indexOf('dev.steeltoe.io') > -1){//covering docs-dev.steeltoe.io
		return "https://dev.steeltoe.io";
	}

	return "https://steeltoe.io";
}
$(document).ready(function() {
	if(document.location.hostname.indexOf('localhost') > -1 || document.location.hostname.indexOf('dev.steeltoe.io') > -1){
		$("a[href^='https://steeltoe.io']").attr('href', function() { return this.href.replace(/^https:\/\/steeltoe\.io/, getMainSiteHost()); });
	};

	//GLOBAL REPLACE ALL VALUES FROM LOCALSTORAGE
	for (var i = 0; i < localStorage.length; i++){
		var val = localStorage.getItem(localStorage.key(i));
		
		switch(val){
			case("null"):
			case("true"):
			case("false"):
				break;
			default:
				val = "\""+val+"\"";
				break;
		};

		$("pre").each(function(idx){
			var a=$(this);
			a.html(a.html().replace('%%'+localStorage.key(i)+'%%',val));
		});
	}

	//Clean up missed placeholders
	$("pre").each(function(idx){
		var a=$(this);
		a.html(a.html().replace(/%%/g,'#'));
	});

	localStorage.clear();

	var urlParams = new URLSearchParams(window.location.search);
	//console.log(urlParams);

	urlParams.forEach(function(value,key){localStorage[key] = value;});

	// Toggle api browser vs docs
	if (window.location.href.indexOf("/api/browser/") > -1) {
		// Change current page highlight nav
		$('#docsNavLink').removeClass('active');
		$('#apiBrowserNavLink').addClass('active');

		// Change active version labels for correct linkage
		$('#version-button-api-v2').removeClass('hide');
		$('#version-button-api-v3').removeClass('hide');
		$('#version-button-doc-v2').addClass('hide');
		$('#version-button-doc-v3').addClass('hide');

		// Select correct version radio button
		$("#api-v3").prop("checked", true);

		// Set TOC font for api references
		$("[role=main]").addClass("api-browser");

		// Enable top-level namspeace nav
		if (window.location.href.indexOf("/api/browser/v3/") > -1) {
			$('#api-nav-section-v3').removeClass('hide');
		}
		else {
			$('#api-nav-section-v2').removeClass('hide');
		}
	}

	//toggle the docs version radio
	if (window.location.href.indexOf("v2") > -1) {
    $('.versionLabel').toggleClass('active');
	}else if (window.location.href.indexOf("articles") > -1) {
    $('#docsNavLink').removeClass('active');
    $('#blogNavLink').addClass('active');
	}
});

var options = {
	contentSelector: "#wrapper",
	loadDelay: 10,
	// CSS class(es) used to render the copy icon.
	copyIconClass: "oi oi-document",
	// CSS class(es) used to render the done icon.
	checkIconClass: "oi oi-check text-success",
	// hook to allow modifying the text before it's pasted
	onBeforeTextCopied: function (text, codeElement) {
		return text;   //  you can fix up the text here
	}
};
window.highlightJsBadge(options);