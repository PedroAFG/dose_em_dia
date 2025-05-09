﻿using DoseEmDia.Helpers;
using DoseEmDia.Models.db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DoseEmDia.Controllers
{
    [ApiController]
    [Route("api/campanhas")]
    public class CampanhaVacinacaoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EnvioEmail _envioEmail;

        public CampanhaVacinacaoController(ApplicationDbContext context, EnvioEmail envioEmail)
        {
            _context = context;
            _envioEmail = envioEmail;
        }

        [HttpPost("enviar")]
        public async Task<IActionResult> EnviarCampanha([FromForm] CampanhaRequest request)
        {
            if (request.Imagem == null || request.Imagem.Length == 0) //A imagem é enviada pelo front
                return BadRequest("Imagem da campanha obrigatória.");

            byte[] imagemBytes;
            using (var ms = new MemoryStream())
            {
                await request.Imagem.CopyToAsync(ms);
                imagemBytes = ms.ToArray();
            }

            var usuarios = await _context.Usuario.ToListAsync();

            foreach (var usuario in usuarios)
            {
                var corpoHtml = GerarHtmlCampanha(request.Titulo, request.Descricao, request.LinkMaisInformacoes);

                await _envioEmail.EnviarEmailCampanhaAsync(
                    usuario.Email,
                    request.Titulo,
                    corpoHtml,
                    imagemBytes
                );
            }

            return Ok("Campanha enviada para todos os usuários.");
        }

        private string GerarHtmlCampanha(string titulo, string descricao, string link)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body>");
            sb.Append($"<h2>{titulo}</h2>");
            sb.Append($"<p>{descricao}</p>");
            sb.Append("<img src='cid:bannerCampanha' alt='Campanha' width='600'/>");
            sb.Append($"<p><a href='{link}' target='_blank'>Clique aqui para mais informações</a></p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }
    }

    public class CampanhaRequest
    {
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string LinkMaisInformacoes { get; set; }
        public IFormFile Imagem { get; set; }
    }
}
