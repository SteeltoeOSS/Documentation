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

	//toggle the docs version radio
	if (window.location.href.indexOf("v2") > -1) {
    $('.versionLabel').toggleClass('active');
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