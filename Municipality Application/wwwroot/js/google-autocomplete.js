window.initGoogleAutocomplete = function (inputIdPrefix = "Address") {
    var input = document.getElementById(inputIdPrefix);
    // Defensive: Only run if Google Maps API and input are available
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
            // Only set fields that exist in the DOM
            var setField = function(suffix, value) {
                var el = document.getElementById(inputIdPrefix + suffix);
                if (el) el.value = value;
            };
            setField('_Street', getComponent('route') + ' ' + getComponent('street_number'));
            setField('_Suburb', getComponent('sublocality') || getComponent('neighborhood'));
            setField('_City', getComponent('locality'));
            setField('_Province', getComponent('administrative_area_level_1'));
            setField('_PostalCode', getComponent('postal_code'));
            setField('_Country', getComponent('country'));
            setField('_FormattedAddress', place.formatted_address || '');
            setField('_Latitude', place.geometry.location.lat());
            setField('_Longitude', place.geometry.location.lng());
        });
    }
};