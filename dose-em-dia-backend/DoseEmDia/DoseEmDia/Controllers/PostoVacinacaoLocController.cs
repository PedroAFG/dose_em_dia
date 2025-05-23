﻿using DoseEmDia.Models;
using System.Text.Json;
using DoseEmDia.Controllers.DTO;
using DoseEmDia.Controllers.Helpers;
using Microsoft.AspNetCore.Mvc;
using DoseEmDia.Models.db;
using Microsoft.EntityFrameworkCore;

namespace DoseEmDia.Controllers
{
    [ApiController]
    [Route("api/localizacao")]
    public class PostoVacinacaoLocController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private static readonly int _limiteRequisicoes = 1500;
        private readonly HttpClient _httpClient;
        private readonly string _hereApiKey = "SUA_API_KEY_HERE"; //Falta criar uma conta na HERE e pegar a chave

        public PostoVacinacaoLocController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("buscar-postos")]
        public async Task<IActionResult> BuscarPostosVacina([FromQuery] double latitude, [FromQuery] double longitude)
        {
            var contador = await _context.ContadorRequisicoes.FirstOrDefaultAsync();

            if (contador == null)
            {
                contador = new ContadorRequisicoes { Requisicoes = 0 };
                _context.ContadorRequisicoes.Add(contador);
                await _context.SaveChangesAsync();
            }

            if (contador.Requisicoes >= _limiteRequisicoes)
            {
                return StatusCode(429, "Limite de requisições atingido. Entre em contato com o suporte.");
            }

            contador.Requisicoes++;
            await _context.SaveChangesAsync();

            var url = $"https://discover.search.hereapi.com/v1/discover" +
                      $"?q=posto+de+vacinação" +
                      $"&at={latitude},{longitude}" +
                      $"&limit=10" +
                      $"&apiKey={_hereApiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Erro ao consultar HERE API.");

            var json = await response.Content.ReadAsStringAsync();

            var resultado = JsonDocument.Parse(json);
            var locais = resultado.RootElement.GetProperty("items")
                .EnumerateArray()
                .Select(item =>
                {
                    var endereco = new Endereco
                    {
                        Logradouro = item.GetProperty("address").TryGetProperty("street", out var street) ? street.GetString() ?? "" : "",
                        Bairro = item.GetProperty("address").TryGetProperty("district", out var district) ? district.GetString() ?? "" : "",
                        Cidade = item.GetProperty("address").TryGetProperty("city", out var city) ? city.GetString() ?? "" : "",
                        Estado = item.GetProperty("address").TryGetProperty("state", out var state) ? state.GetString() ?? "" : ""
                    };

                    var distanciaMetros = item.GetProperty("distance").GetInt32();
                    var lat = item.GetProperty("position").GetProperty("lat").GetDouble();
                    var lng = item.GetProperty("position").GetProperty("lng").GetDouble();

                    return new PostoVacinacaoResponse
                    {
                        Nome = item.GetProperty("title").GetString(),
                        EnderecoCompleto = $"{endereco.Logradouro}, {endereco.Bairro} - {endereco.Cidade}/{endereco.Estado}",
                        Distancia = FormatacaoHelper.FormatarDistancia(distanciaMetros),
                        LinkGoogleMaps = GerarLinkGoogleMaps(lat, lng)
                    };
                })
                .ToList();

            return Ok(locais);
        }

        //criar api
        private static string GerarLinkGoogleMaps(double latitude, double longitude)
        {
            return $"https://www.google.com/maps/search/?api=1&query={latitude},{longitude}";
        }

        //metodo para zerar o contador de requisições, ver como aplicar
        public static void ResetarContadorRequisicoes()
        {
            
        }
    }

    public class ContadorRequisicoes
    {
        public int Id { get; set; }
        public int Requisicoes { get; set; }
    }
}


