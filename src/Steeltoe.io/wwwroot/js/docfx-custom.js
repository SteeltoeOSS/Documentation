function isApiBrowserPage() {
    return window.location.href.indexOf("/api/") > -1;
}
function isBlog() {
    return window.location.href.indexOf("/articles/") > -1;
}
function isGuide() {
    return window.location.href.indexOf("/guides/") > -1;
}

if (window.location.href.indexOf("v4") > -1) {
    $(".v4").addClass("btn-primary");
    $(".v2, .v3").addClass("btn-outline-primary");
}
else if (window.location.href.indexOf("v3") > -1) {
    $(".v3").addClass("btn-primary");
    $(".v2, .v4").addClass("btn-outline-primary");
}
else if (window.location.href.indexOf("v2") > -1) {
    $(".v2").addClass("btn-primary");
    $(".v3, .v4").addClass("btn-outline-primary");
}
else if (isBlog()) {
    $("div.toc-offcanvas").remove();
    $("#docsNavLink").removeClass("active");
    $("#blogNavLink").addClass("active");
}
else if (isGuide()) {
    $("#version-toggles").remove();
    $("#docsNavLink").removeClass("active");
    $("#guidesNavLink").addClass("active");
}
else if (window.location.href.indexOf("/initializr") > -1) {
    $(".v2, .v3, .v4").addClass("btn-outline-primary");
}

if (isApiBrowserPage()) {
    $(".api-browser-controls").show();
    $("#api-docs-toggle").hide();
}
