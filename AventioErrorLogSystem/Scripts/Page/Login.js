$(document).ready(function () {

    SetClientNodeDetails();
});

function SetClientNodeDetails()
{
    
    /*{
  "geoplugin_request": "123.231.124.113",
  "geoplugin_status": 200,
  "geoplugin_credit": "Some of the returned data includes GeoLite data created by MaxMind, available from <a href='http://www.maxmind.com'>http://www.maxmind.com</a>.",
  "geoplugin_city": "Colombo",
  "geoplugin_region": "Western",
  "geoplugin_areaCode": "0",
  "geoplugin_dmaCode": "0",
  "geoplugin_countryCode": "LK",
  "geoplugin_countryName": "Sri Lanka",
  "geoplugin_continentCode": "AS",
  "geoplugin_latitude": "6.9319",
  "geoplugin_longitude": "79.8478",
  "geoplugin_regionCode": "36",
  "geoplugin_regionName": "Western",
  "geoplugin_currencyCode": "LKR",
  "geoplugin_currencySymbol": "&#8360;",
  "geoplugin_currencySymbol_UTF8": "₨",
  "geoplugin_currencyConverter": 153.55
}*/
    var locationDetailsObj={};

    locationDetailsObj["IPAddress"]=geoplugin_request();
    locationDetailsObj["City"] = geoplugin_city();
    locationDetailsObj["CountryCode"] = geoplugin_countryCode();
    locationDetailsObj["Country"]=geoplugin_countryName();
    locationDetailsObj["Region"]=geoplugin_regionName();


    var param = JSON.stringify({ 'locationDetailsObj': locationDetailsObj });

    $.ajax({
        url: '/Login/SetIPAddressDetails',
        data:param,
        type: 'post',
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {
                      

        }

    });
}