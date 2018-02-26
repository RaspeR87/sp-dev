var invokeAfterEntityEditorCallback = function (func) {
    var oldEntityEditorCallback = EntityEditorCallback;
    if (typeof EntityEditorCallback != 'function') {
        EntityEditorCallback = func;
    } else {
        EntityEditorCallback = function (result, ctx) {
            oldEntityEditorCallback(result, ctx);
            func(result, ctx);
        }
    }
}

function onPeoplePickerFieldSet(result, ctx) {
    if (result != undefined) {
        parser = new DOMParser();
        xmlDoc = parser.parseFromString(result, "text/xml");

        DoSomethingWithUserName(xmlDoc.getElementsByTagName("Entity")[0].getAttribute('Key'));
    }
}

$(document).ready(function () {
    invokeAfterEntityEditorCallback(onPeoplePickerFieldSet);
});