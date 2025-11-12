// wwwroot/js/event-filters.js
window.clearEventFilters = function (formId, addressId, latId, lngId, redirectUrl) {
    var form = document.getElementById(formId);
    if (form) form.reset();
    if (addressId) {
        var addressInput = document.getElementById(addressId);
        if (addressInput) addressInput.value = '';
    }
    if (latId) {
        var latInput = document.getElementById(latId);
        if (latInput) latInput.value = '';
    }
    if (lngId) {
        var lngInput = document.getElementById(lngId);
        if (lngInput) lngInput.value = '';
    }
    if (redirectUrl) {
        window.location.href = redirectUrl;
    }
};