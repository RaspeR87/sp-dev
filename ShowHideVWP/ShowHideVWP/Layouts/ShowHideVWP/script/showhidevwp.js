$(document).ready(function () {
    var divShowHideVWPContent = $("div.showhidevwp-content");
    var divChrome = divShowHideVWPContent.closest('div.ms-webpart-chrome');
    var h2Title = divChrome.find('span.js-webpart-titleCell h2 nobr');

    if (h2Title.find("img.showhidevwp-img").length == 0) {
        h2Title.append("<span style=\"float:right; padding-right:5px; padding-top:2px;\"><img class=\"showhidevwp-img\" src=\"/_layouts/15/images/ShowHideVWP/appbar.chevron.down.png\" width=\"20\" style=\"cursor:pointer; float:right;\" onclick=\"AsLacnShowHide(this); return false;\"/></span>");
    }
});

function AsLacnShowHide(element) {
    var divChrome = $(element).closest('div.ms-webpart-chrome');
    var divShowHideVWPContent = divChrome.find("div.showhidevwp-content");
    var h2Title = divChrome.find('span.js-webpart-titleCell h2 nobr');

    if (divShowHideVWPContent.css("display") != "none") {
        divShowHideVWPContent.css("display", "none");
        h2Title.find("img.showhidevwp-img").attr("src", "/_layouts/15/images/ShowHideVWP/appbar.chevron.down.png");
    }
    else {
        divShowHideVWPContent.css("display", "");
        h2Title.find("img.showhidevwp-img").attr("src", "/_layouts/15/images/ShowHideVWP/appbar.chevron.up.png");
    }
}