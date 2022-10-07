function getMainSiteHost(){
	var host = document.location.hostname;
	if(document.location.hostname.indexOf('localhost') > -1){
		return "http://localhost:8080";
	}else if(host.indexOf('dev.steeltoe.io') > -1){//covering docs-dev.steeltoe.io
		return "https://dev.steeltoe.io";
	}

	return "https://steeltoe.io";
}

function isApiBrowserPage() {
	return window.location.href.indexOf("/api/browser/") > -1;
}

function isApiVersion3() {
	return window.location.href.indexOf("/api/browser/v3/") > -1;
}

function getNamespaceFolder(url) {
	var urlSegments = url.split('/');
	return urlSegments[urlSegments.length - 2];
}

function setActiveNamespace(version) {
	var urlNamespace = getNamespaceFolder(window.location.href);
	
	$(`#api-namespace-${version} option`).each((index, option) => {
		var optionNamespace = getNamespaceFolder(option.value);

		if (urlNamespace.toLowerCase() === optionNamespace.toLowerCase()) {
			$(option).prop('selected', true);
		}
	});
}

function bindNavigationChangeEvent(version) {
	$(`#api-namespace-${version}`).change((event) => {
		var url = event.target.value;

		if (url) {
			window.location = url;
		}

		return false;
	});
}

function showMobileToc() {
	$('.sidetoggle.collapse').toggleClass('show');
}

function showMobileSidebar() {
	$('[role=complementary]').toggleClass('d-none');

	// Remove mobile sidebar when something is selected
	$('.sideaffix ul.nav li a').each((index, anchor) => {
		var sidebarFunction = 'showMobileSidebar();';
		var anchorOnclick = $(anchor).attr('onclick');

		if (anchorOnclick !== sidebarFunction) {
			$(anchor).attr('onclick',sidebarFunction);
		}
	});
}

function showApiBrowserElements() {
	// Change current page highlight nav
	$('#docsNavLink').removeClass('active');
	$('#apiBrowserNavLink').addClass('active');

	// Change active version labels for correct linkage
	$('.versionLabel').toggleClass('hide');

	// Set TOC font for api references
	$("[role=main]").addClass("api-browser");

	// Show api navigation above article
	$('#api-navigation').removeClass('hide');

	// Hide/Disable Contribution elements (view/edit source)
	$('.sideaffix .contribution').addClass('hide');
	$('.sideaffix .contribution .nav li a').attr('href','#');

	if(!isApiVersion3()) {
		// Select correct version radio button
		$("#api-v2").prop("checked", true);

		// Change namespace dropdown to v2
		$('.api-namespace').toggleClass('hide');
	}
}

function inIframe () {
	try {
		return window.self !== window.top;
	}
	catch (e) {
		return true;
	}
}

if (inIframe()) {
	$(".hidewhenembedded").hide();
	$("div[role=main].container-fluid.body-content").css({"margin-top":"0"})
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
	if (isApiBrowserPage()) {
		showApiBrowserElements();

		if (isApiVersion3()) {
			setActiveNamespace('v3');
			bindNavigationChangeEvent('v3');
		}
		else {
			setActiveNamespace('v2');
			bindNavigationChangeEvent('v2');
		}
	}

	//toggle the docs version radio
	if (window.location.href.indexOf("v2") > -1) {
		$('.versionLabel').toggleClass('active');
	}
	else if (window.location.href.indexOf("/articles") > -1) {
		$('#docsNavLink').removeClass('active');
		$('#blogNavLink').addClass('active');
	}
	else if (window.location.href.indexOf("/guides") > -1) {
		$('#docsNavLink').removeClass('active');
		$('#guidesNavLink').addClass('active');
	}

	// Add numbers to toc
	setTimeout(() => {
		let level1length = $('#sidetoc #toc .level1 > li').length;

		// Add chapter to level 1 li anchors
		for( i = 0; i < level1length; i++) {
			$("#sidetoc #toc .level1").children().eq(i).children('a').prepend(i+1 + ". ");
			
			// Add chapter to level 2 li anchors
			let level2length = $("#sidetoc #toc .level1").children().eq(i).find("li").length;
			
			for( j = 0; j < level2length; j++) {
				$("#sidetoc #toc .level1").children().eq(i).find("li").children().eq(j).prepend((i+1) + "." + (j+1) + " ");
			}
		}
		document.querySelector('.active.in .active.in .active').appendChild(document.getElementById('affix'));

		let level3length = $(".affix .level1").find("li").length;

		for( k = 0; k < level3length; k++) {
			console.log(k);
			// $("affix .level1").children().eq(k).children('a').prepend(k+1 + ". ");

			$("affix .level1").children().eq(1).children('a').prepend(". ");
		}
	}, "100")
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