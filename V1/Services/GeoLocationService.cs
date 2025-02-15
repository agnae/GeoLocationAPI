﻿using GeoLocationAPI.V1.Models;
using MaxMind.GeoIP2;

namespace GeoLocationAPI.V1.Services
{
    /// <summary>
    /// GeoLocation Service
    /// </summary>
    public class GeoLocationService : IGeoLocationService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// GeoLocationService
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public GeoLocationService(
            ILogger<GeoLocationService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="incomingIP">The IP passed in from the IGeoLocationService Interface</param>
        /// <returns></returns>
        public async Task<GeoLocation> GetGeoLocationByIPAsync(string incomingIP)

        {
            var response = new GeoLocation(incomingIP);
            if (System.Net.IPAddress.TryParse(incomingIP, out var ipParseResult))
            {
                var geoDB = _configuration["DBSettings:GeoLite2CityDB"];
                response.Date = DateTime.UtcNow.ToUniversalTime();
                response.IPAddress = ipParseResult.ToString();

                using (var reader = new DatabaseReader(geoDB))
                {
                    if (reader.TryCity(response.IPAddress, out var trycityResponse))
                    {
                        var cityResponse = reader.City(response.IPAddress);
                        if (cityResponse != null)
                        {
                            response.City = cityResponse.City.ToString();
                            response.TimeZone = cityResponse.Location.TimeZone?.ToString();
                            response.Latitude = cityResponse.Location.Latitude?.ToString();
                            response.Longitude = cityResponse.Location.Longitude?.ToString();
                            response.Continent = cityResponse.Continent?.ToString();
                            response.Country = cityResponse.Country?.ToString();
                            response.State = cityResponse.MostSpecificSubdivision?.ToString();
                            response.Postal = cityResponse.Postal?.Code?.ToString();
                            response.IPFoundInGeoDB = true;
                            response.Message = (response.IPAddress + " found in the GeoDB");
                        }
                        else
                        {
                            response.IPFoundInGeoDB = false;
                            _logger.LogWarning(response.IPAddress + " not found in the GeoDB");
                            response.Message = (response.IPAddress + " not found in the GeoDB");
                        }
                    }
                    else
                    {
                        response.IPFoundInGeoDB = false;
                        _logger.LogWarning(response.IPAddress + " not found in the GeoDB");
                        response.Message = (response.IPAddress + " not found in the GeoDB");
                    }
                }
                return await Task.FromResult(response);
            }
            else
            {
                response.IPFoundInGeoDB = false;
                _logger.LogWarning(incomingIP + " Unable to Parse");
                response.Message = (incomingIP + " Unable to Parse");

                return await Task.FromResult(response);
            }
        }
    }
}
