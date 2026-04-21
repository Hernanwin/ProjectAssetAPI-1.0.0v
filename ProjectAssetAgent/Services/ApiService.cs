using Microsoft.Win32;
using ProjectAssetAgent.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace ProjectAssetAgent.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;

        public ApiService()
        {
            _client = new HttpClient();
            //reemplázar siempre que se cambie el puerto del backend
            _client.BaseAddress = new Uri("http://localhost:5000");
        }

        public async Task SendAssetAsync(VisualLab asset)
        {
            var response = await _client.PostAsJsonAsync(
                "api/VisualLabs/register",
                asset
            );

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Asset enviado correctamente");
            }
            else
            {
                Console.WriteLine("Error enviando asset");
            }
        }
    }
}