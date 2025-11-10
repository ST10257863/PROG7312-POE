// wwwroot/js/google-autocomplete.js
window.initGoogleAutocomplete = function (inputIdPrefix = "Address") {
    var input = document.getElementById(inputIdPrefix);
    if (input && window.google && google.maps && google.maps.places) {
        var autocomplete = new google.maps.places.Autocomplete(input, {
            types: ['geocode'],
            componentRestrictions: { country: 'za' }
        });
        autocomplete.addListener('place_changed', function () {
            var place = autocomplete.getPlace();
            function getComponent(type) {
                var comp = place.address_components.find(c => c.types.includes(type));
                return comp ? comp.long_name : '';
            }
            document.getElementById(inputIdPrefix + '_Street').value = getComponent('route') + ' ' + getComponent('street_number');
            document.getElementById(inputIdPrefix + '_Suburb').value = getComponent('sublocality') || getComponent('neighborhood');
            document.getElementById(inputIdPrefix + '_City').value = getComponent('locality');
            document.getElementById(inputIdPrefix + '_Province').value = getComponent('administrative_area_level_1');
            document.getElementById(inputIdPrefix + '_PostalCode').value = getComponent('postal_code');
            document.getElementById(inputIdPrefix + '_Country').value = getComponent('country');
            document.getElementById(inputIdPrefix + '_FormattedAddress').value = place.formatted_address || '';
            document.getElementById(inputIdPrefix + '_Latitude').value = place.geometry.location.lat();
            document.getElementById(inputIdPrefix + '_Longitude').value = place.geometry.location.lng();
        });
    }
};