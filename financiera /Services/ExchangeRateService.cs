using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Financiera.Models;

namespace Financiera.Services
{
    public class ExchangeRateService
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private const string ApiUrl = "https://www.datos.gov.co/resource/32sa-8pi3.json?$order=vigenciadesde%20DESC&$limit=1";

        public async Task<(TrmDto? trm, string errorMessage)> FetchCurrentTrmAsync()
        {
            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync(ApiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return (null, "No fue posible obtener la tasa oficial del mercado.");
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var trmList = JsonSerializer.Deserialize<TrmDto[]>(jsonResponse);

                if (trmList != null && trmList.Length > 0)
                {
                    return (trmList[0], string.Empty);
                }

                return (null, "La respuesta de la API no contiene datos válidos.");
            }
            catch (Exception)
            {
                return (null, "Error de conexión al intentar obtener la TRM.");
            }
        }
    }
}