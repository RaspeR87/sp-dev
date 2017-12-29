function CheckOverload() {
    var ponavljajociDogodek = $('input[id$="RecurrenceField"]').prop('checked');
    if (ponavljajociDogodek) {
        var context = new SP.ClientContext.get_current();
        var web = context.get_web();
        context.load(web);
        context.executeQueryAsync(function () {
            var _zacDatum = moment(new Date(0001, 0, 1, 0, 0, 0, 0));
            var _konDatum = moment(new Date(0001, 0, 1, 0, 0, 0, 0));

            var _dateHoursInputs = $('select[id$="DateTimeFieldDateHours"]');
            if (_dateHoursInputs.length >= 2) {
                var _zacUra = parseInt(_dateHoursInputs.eq(0).val());
                if (_dateHoursInputs.eq(0).val().indexOf("PM") >= 0) {
                    _zacUra += 12;
                }
                _zacDatum = _zacDatum.add(_zacUra, 'hours');

                var _konUra = parseInt(_dateHoursInputs.eq(1).val());
                if (_dateHoursInputs.eq(1).val().indexOf("PM") >= 0) {
                    _konUra += 12;
                }
                _konDatum = _konDatum.add(_konUra, 'hours');
            }
            else {
                _zacDatum = _zacDatum.add(0, 'hours');
                _konDatum = _konDatum.add(24, 'hours');
            }

            var _dateMinutesInputs = $('select[id$="DateTimeFieldDateMinutes"]');
            if (_dateMinutesInputs.length >= 2) {
                var _zacMinuta = parseInt(_dateMinutesInputs.eq(0).val());
                _zacDatum = _zacDatum.add(_zacMinuta, 'minutes');

                var _konMinuta = parseInt(_dateMinutesInputs.eq(1).val());
                _konDatum = _konDatum.add(_konMinuta, 'minutes');
            }

            var _datumOd = moment($('input[id$="windowStart_windowStartDate"]').val(), 'D.M.YYYY');
            var _datumDo = moment($('input[id$="windowEnd_windowEndDate"]').val(), 'D.M.YYYY');

            $.ajax({
                url: "/_vti_bin/{web-service-path}/{web-service-name}.svc/SejneSobe_PreveriPrekrivanjePonavljajoce",
                data: JSON.stringify({
                    siteUrl: web.get_url(),
                    listGuid: _spPageContextInfo.pageListId,
                    uraOd: _zacDatum.format("HH:mm"),
                    uraDo: _konDatum.format("HH:mm"),
                    tipPonovitve: $('input[name$="RecurrencePatternType"]:checked').val(),
                    dnevnoTipVzorec: $('input[name$="DailyRecurType"]:checked').val(),
                    dnevnoVsakihDni: $('input[id$="daily_dayFrequency"]').val(),
                    tedenskoVsakihTednov: $('input[id$="weekly_weekFrequency"]').val(),
                    dnevi: $('input[name$="weekly_multiDays$0"]:checked').length + "," +
                            $('input[name$="weekly_multiDays$1"]:checked').length + "," +
                            $('input[name$="weekly_multiDays$2"]:checked').length + "," +
                            $('input[name$="weekly_multiDays$3"]:checked').length + "," +
                            $('input[name$="weekly_multiDays$4"]:checked').length + "," +
                            $('input[name$="weekly_multiDays$5"]:checked').length + "," +
                            $('input[name$="weekly_multiDays$6"]:checked').length,
                    mesecnoTipVzorec: $('input[name$="MonthlyRecurType"]:checked').val(),
                    mesecnoDan: $('input[id$="monthly_day"]').val(),
                    mesecnoVsakihMesecev: $('input[id$="monthly_monthFrequency"]').val(),
                    mesecnoKateri: $('select[id$="monthlyByDay_weekOfMonth"]').val(),
                    mesecnoDan2: $('select[id$="monthlyByDay_day"]').val(),
                    mesecnoVsakihMesecev2: $('input[id$="monthlyByDay_monthFrequency"]').val(),
                    letnoTipVzorec: $('input[name$="YearlyRecurType"]:checked').val(),
                    letnoVsakMesec: $('select[id$="yearly_month"]').val(),
                    letnoDan: $('input[id$="yearly_day"]').val(),
                    letnoKateri: $('select[id$="yearlyByDay_weekOfMonth"]').val(),
                    letnoDan2: $('select[id$="yearlyByDay_day"]').val(),
                    letnoVMesecu: $('select[id$="yearlyByDay_month"]').val(),
                    datumOd: _datumOd.format("DD.MM.YYYY"),
                    tipDatumDo: $('input[name$="EndDateRangeType"]:checked').val(),
                    datumDo: _datumDo.format("DD.MM.YYYY"),
                    stPonovitev: $('input[id$="repeatInstances"]').val()
                }),
                type: "POST",
                cache: false,
                dataType: 'json',
                contentType: "application/json; charset=utf-8"
            })
            .done(function (data) {
                if (data.ErrorMessage) {
                    alert(data.ErrorMessage);
                }
                else {
                    console.log(data.Data);
                    if (!data.Data) {
                        originalSaveButtonClickHandler();
                    } else {
                        // izpiši napake
                        ssIzpisiNapakoCas("Končni čas", "End Time");
                    }
                }
            });
        });
    }
    else {
        var _zacDatum;
        var _konDatum;

        $('span#sIzpisiNapakoCas').hide();

        var _dateInputs = $('input[id$="DateTimeFieldDate"]');
        if (_dateInputs.length >= 2) {
            _zacDatum = moment(_dateInputs.eq(0).val(), 'D.M.YYYY');
            _konDatum = moment(_dateInputs.eq(1).val(), 'D.M.YYYY');
        }
        else {
            originalSaveButtonClickHandler();
            return;
        }

        var _dateHoursInputs = $('select[id$="DateTimeFieldDateHours"]');
        if (_dateHoursInputs.length >= 2) {
            var _zacUra = parseInt(_dateHoursInputs.eq(0).val());
            if (_dateHoursInputs.eq(0).val().indexOf("PM") >= 0) {
                _zacUra += 12;
            }
            _zacDatum = _zacDatum.add(_zacUra, 'hours');

            var _konUra = parseInt(_dateHoursInputs.eq(1).val());
            if (_dateHoursInputs.eq(1).val().indexOf("PM") >= 0) {
                _konUra += 12;
            }
            _konDatum = _konDatum.add(_konUra, 'hours');
        }
        else {
            _zacDatum = _zacDatum.add(0, 'hours');
            _konDatum = _konDatum.add(24, 'hours');
        }

        var _dateMinutesInputs = $('select[id$="DateTimeFieldDateMinutes"]');
        if (_dateMinutesInputs.length >= 2) {
            var _zacMinuta = parseInt(_dateMinutesInputs.eq(0).val());
            _zacDatum = _zacDatum.add(_zacMinuta, 'minutes');

            var _konMinuta = parseInt(_dateMinutesInputs.eq(1).val());
            _konDatum = _konDatum.add(_konMinuta, 'minutes');
        }

        var context = new SP.ClientContext.get_current();
        var web = context.get_web();
        context.load(web);
        context.executeQueryAsync(function () {
            $.ajax({
                url: "/_vti_bin/{web-service-path}/{web-service-name}.svc/SejneSobe_PreveriPrekrivanje",
                data: JSON.stringify({
                    SiteUrl: web.get_url(),
                    ListGuid: _spPageContextInfo.pageListId,
                    DatumOd: _zacDatum.format("DD.MM.YYYY HH:mm:ss"),
                    DatumDo: _konDatum.format("DD.MM.YYYY HH:mm:ss")
                }),
                type: "POST",
                cache: false,
                dataType: 'json',
                contentType: "application/json; charset=utf-8"
            })
            .done(function (data) {
                if (data.ErrorMessage) {
                    alert(data.ErrorMessage);
                }
                else {
                    console.log(data.Data);
                    if (!data.Data) {
                        originalSaveButtonClickHandler();
                    } else {
                        // izpiši napake
                        ssIzpisiNapakoCas("Končni čas", "End Time");
                    }
                }
            });
        });
    }
}

function ssIzpisiNapakoCas() {
    var paramItems = arguments;

    if ($('span[id="sIzpisiNapakoCas"]').length) {
        $('span[id="sIzpisiNapakoCas"]').remove();
    }

    for (var i = 0; i < paramItems.length; i++) {
        var buildingTypeDOM = $("table.ms-formtable .ms-formlabel nobr").filter(function () {
            return $.text([this]).startsWith(paramItems[i]);
        }).closest('tr').children('td').eq(1).append("<span id=\"sIzpisiNapakoCas\" class=\"ms-formvalidation\"><span role=\"alert\">Vaš dogodek se terminsko prekriva že z enim od obstoječih.</span><br></span>");
    }
}

var originalSaveButtonClickHandler = function () {
};

$(document).ready(function () {
    var saveButton = $("[name$='diidIOSaveItem']");
    if (saveButton.length > 0) {
        originalSaveButtonClickHandler = saveButton[0].onclick;
    }
    $(saveButton).attr("onClick", "CheckOverload()");
});