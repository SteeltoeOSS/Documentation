function getMainSiteHost() {
    var host = document.location.hostname;
    if (document.location.hostname.indexOf('localhost') > -1) {
        return "https://localhost:8080";
    } else if (host.indexOf('staging.steeltoe.io') > -1) {
        return "https://staging.steeltoe.io";
    }

    return "https://steeltoe.io";
}

function isApiBrowserPage() {
    return window.location.href.indexOf("/api/browser/") > -1;
}

function isGuidePage() {
    return window.location.href.indexOf("/guides/") > -1;
}

function isApiVersion3() {
    return window.location.href.indexOf("/api/browser/v3/") > -1;
}

function isApiVersion4() {
    return window.location.href.indexOf("/api/browser/v4/") > -1;
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

    $(`#api-namespace-${version}`).removeClass('hide');
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
            $(anchor).attr('onclick', sidebarFunction);
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
    $('.sideaffix .contribution .nav li a').attr('href', '#');

    // Select correct version radio button, activate parent label
    if (isApiVersion4()) {
        $("#api-v4").prop("checked", true);
        $("#api-v4").parent().addClass('active');
    } else if (isApiVersion3()) {
        $("#api-v3").prop("checked", true);
        $("#api-v3").parent().addClass('active');
    }
    else {
        $("#api-v2").prop("checked", true);
        $("#api-v2").parent().addClass('active');
    }
}

function inIframe() {
    try {
        return window.self !== window.top;
    }
    catch (e) {
        return true;
    }
}

if (inIframe()) {
    $(".hidewhenembedded").hide();
    $("div[role=main].container-fluid.body-content").css({ "margin-top": "0" })
}

$(function() {
    if (document.location.hostname.indexOf('localhost') > -1 || document.location.hostname.indexOf('staging.steeltoe.io') > -1) {
        $("a[href^='https://steeltoe.io']").attr('href', function () { return this.href.replace(/^https:\/\/steeltoe\.io/, getMainSiteHost()); });
    };

    // GLOBAL REPLACE ALL VALUES FROM LOCALSTORAGE
    for (var i = 0; i < localStorage.length; i++) {
        var val = localStorage.getItem(localStorage.key(i));

        switch (val) {
            case ("null"):
            case ("true"):
            case ("false"):
                break;
            default:
                val = "\"" + val + "\"";
                break;
        };

        $("pre").each(function (idx) {
            var a = $(this);
            a.html(a.html().replace('%%' + localStorage.key(i) + '%%', val));
        });
    }

    // Clean up missed placeholders
    $("pre").each(function (idx) {
        var a = $(this);
        a.html(a.html().replace(/%%/g, '#'));
    });

    localStorage.clear();

    var urlParams = new URLSearchParams(window.location.search);
    //console.log(urlParams);

    urlParams.forEach(function (value, key) { localStorage[key] = value; });

    // Toggle api browser vs docs
    if (isApiBrowserPage()) {
        showApiBrowserElements();

        if (isApiVersion4()) {
            setActiveNamespace('v4');
            bindNavigationChangeEvent('v4');
        }
        else if (isApiVersion3()) {
            setActiveNamespace('v3');
            bindNavigationChangeEvent('v3');
        }
        else {
            setActiveNamespace('v2');
            bindNavigationChangeEvent('v2');
        }
    }
    else if (window.location.href.indexOf("v4") > -1) {
        $("#v4").prop("checked", true);
        $("#v4").parent().addClass('active');
    }
    else if (window.location.href.indexOf("v3") > -1) {
        $("#v3").prop("checked", true);
        $("#v3").parent().addClass('active');
    }
    else if (window.location.href.indexOf("v2") > -1) {
        $("#v2").prop("checked", true);
        $("#v2").parent().addClass('active');
    }
    else if (window.location.href.indexOf("/articles") > -1) {
        $('#docsNavLink').removeClass('active');
        $('#blogNavLink').addClass('active');
    }
    else if (window.location.href.indexOf("/guides") > -1) {
        $('#docsNavLink').removeClass('active');
        $('#guidesNavLink').addClass('active');
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
        // you can fix up the text here
        return text;
    }
};
window.highlightJsBadge(options);
