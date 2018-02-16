var dfd = $.Deferred();
var pkListRoot;

function GetPKNewFormUrl(ctID) {
    var pkContext = SP.ClientContext.get_current(); 
    var pkWeb = pkContext.get_web(); 
    var pkList = pkWeb.get_lists().getById(_spPageContextInfo.pageListId); 
    pkListRoot = pkList.get_rootFolder();

    pkContext.load(pkListRoot);
    pkContext.executeQueryAsync(
        Function.createDelegate(this, function(sender, args,test) { this.onListRootReaded(ctID); dfd.resolve(sender, args); }),
        Function.createDelegate(this, this.onListRootError)
    );
    return dfd;
}

function onListRootReaded(ctID) {
    var selectedDate=GetSelectedDate(); 
    window.location.href=_spPageContextInfo.webAbsoluteUrl + '/Lists/' + pkListRoot.get_name() + '/NewForm.aspx?ContentTypeId=' + ctID + '&RootFolder=' + encodeURIComponent('Lists/' + pkListRoot.get_name()) + '&date=' + selectedDate;
}

function onListRootError(sender, args) {
    dfd.reject(sender, args);
}

function GetSelectedDate() {

    var selectedDate = '';

    var dayScope = $('table.ms-acal-detail');
    var weekScope = $('table.ms-acal-detail');
    var monthScope = $('table.ms-acal-month');


    var isDayScope = dayScope.length > 0 ? true : false;
    var isWeekScope = weekScope.length > 0 ? true : false;
    var isMonthScope = monthScope.length > 0 ? true : false;

    var selecteddateelement = $('.ms-acal-vitem');

    if (isMonthScope) {
        var tditem = selecteddateelement.parent();
        var tritem = selecteddateelement.parent().parent();
        var prevtr = tritem.prev();
        var indx = tritem.children().index(tditem) + 2;
        var dttd = prevtr.children(':nth-child(' + indx + ')');
        if (selecteddateelement.length > 0) selectedDate = dttd.attr('date');
    }
    else if (isWeekScope) {
        var weektritem = selecteddateelement.parent();
        var weekdayindx = weektritem.children().index(selecteddateelement) + 1;
        var weekselectedhalfhourstarttime = $('[class^=ms-acal-hour]').index(weektritem) / 2;
        var weekdttd = $('.ms-acal-week-top').children(':nth-child(' + weekdayindx + ')');
        if (weekdttd.length > 0) selectedDate = weekdttd.attr('date');
    }
    else if (isDayScope) {
        var verbosedaydate = $('.ms-acal-display').text();
        var daydatesplit = verbosedaydate.split(/,| /);
        var month = daydatesplit[1];
        var dayscopemonth = (new Date()).getMonth() + 1; // default to current month


        if (month == "January") dayscopemonth = 1;
        else if (month == "February") dayscopemonth = 2;
        else if (month == "March") dayscopemonth = 3;
        else if (month == "April") dayscopemonth = 4;
        else if (month == "May") dayscopemonth = 5;
        else if (month == "June") dayscopemonth = 6;
        else if (month == "July") dayscopemonth = 7;
        else if (month == "August") dayscopemonth = 8;
        else if (month == "September") dayscopemonth = 9;
        else if (month == "October") dayscopemonth = 10;
        else if (month == "November") dayscopemonth = 11;
        else if (month == "December") dayscopemonth = 12;


        var dayscopeday = daydatesplit[2];
        var dayscopeyear = daydatesplit[3];


        selectedDate = dayscopemonth + '/' + dayscopeday + '/' + dayscopeyear;


        var daytr = selecteddateelement.parent();
        var dayselectedhalfhourstarttime = $('[class^=ms-acal-hour]').index(daytr) / 2;
    }

    return selectedDate;
}

function registerAddItemLink() {
    var newPrekoRazpisa = SP.UI.ApplicationPages.CalendarVirtualItem.prototype.$7p_0;
    var newIzvenRazpisa = newPrekoRazpisa;

    newPrekoRazpisa = newPrekoRazpisa.replace('javascript:void(0)', "javascript: GetPKNewFormUrl('0x0102002F433EA5305941B49ADAFE07A743868900126C566CEE7FCE48A0C51A4EE40CB2C9');");
    newPrekoRazpisa = newPrekoRazpisa.replace('{0}', 'Preko razpisa');
    newIzvenRazpisa = newIzvenRazpisa.replace('javascript:void(0)', "javascript: GetPKNewFormUrl('0x0102005F244A3297F54F898B6BE2FFB5B11B0500E22EDFE8E3F74045AF07B08DED229B1E');");
    newIzvenRazpisa = newIzvenRazpisa.replace('{0}', 'Izven razpisa');

    SP.UI.ApplicationPages.CalendarVirtualItem.prototype.$7p_0 = newPrekoRazpisa + "<br />" + newIzvenRazpisa;
}

$(document).ready(function () {
    ExecuteOrDelayUntilScriptLoaded(registerAddItemLink, "SP.UI.ApplicationPages.Calendar.js");
});