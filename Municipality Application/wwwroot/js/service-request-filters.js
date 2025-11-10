// Wrapper to call the universal autocomplete for the SearchArea field
function initGoogleAutocompleteForSearchArea() {
    window.initGoogleAutocomplete("SearchArea");
}
window.showLocationWarningIfNeeded('location-warning');

// Date range preset logic
function setDateRangePreset() {
    const preset = document.getElementById('DateRangePreset').value;
    const today = new Date();
    let start = '';
    let end = '';

    switch (preset) {
        case 'thisMonth':
            start = new Date(today.getFullYear(), today.getMonth(), 1);
            end = new Date(today.getFullYear(), today.getMonth() + 1, 0);
            break;
        case 'last30':
            start = new Date(today);
            start.setDate(today.getDate() - 29);
            end = today;
            break;
        case 'thisYear':
            start = new Date(today.getFullYear(), 0, 1);
            end = today;
            break;
        case 'last365':
            start = new Date(today);
            start.setDate(today.getDate() - 364);
            end = today;
            break;
        default:
            start = '';
            end = '';
    }

    function formatDate(d) {
        if (!d) return '';
        const month = (d.getMonth() + 1).toString().padStart(2, '0');
        const day = d.getDate().toString().padStart(2, '0');
        return `${d.getFullYear()}-${month}-${day}`;
    }

    document.getElementsByName('StartDate')[0].value = formatDate(start);
    document.getElementsByName('EndDate')[0].value = formatDate(end);
}