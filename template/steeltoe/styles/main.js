function hov(a) {
	var s = document.getElementById("scope");
	var v = document.getElementById(a + '-items');

	unhov('why');
	unhov('learn');
	unhov('project');
	unhov('community');

	s.classList.add(a +'-scope');
	v.classList.add("active");
}
function unhovAll() {
	unhov('why');
	unhov('learn');
	unhov('project');
	unhov('community');
}
function unhov(a) {
	var s = document.getElementById("scope");
	var v = document.getElementById(a + '-items');

	if (s) { s.classList.remove(a + '-scope'); }
	if (v) { v.classList.remove("active"); }
}

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

$("pre").each(function(idx){
	var a=$(this);
	a.html(a.html().replace(/%%/g,'#'));
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