using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//Alias, usado para importar um namespace que o mesmo nome existe em uma classe
using Lista = Alura.ListaLeitura.Modelos.ListaLeitura;

namespace Alura.WebAPI.WebApp.Api
{
    [ApiController]
    [Route("[controller]")]
    public class ListasLeituraController : ControllerBase
    {
        private readonly IRepository<Livro> _repository;

        public ListasLeituraController(IRepository<Livro> repository)
        {
            _repository = repository;
        }

        private Lista CriaLista (TipoListaLeitura tipo)
        {
            var livros = _repository
                .All
                .Where(l => l.Lista == tipo).ToList()
                .Select(l => l.ToApi());
            return new Lista
            {
                Tipo = tipo.ParaString(),
                Livros = livros
            };
        }

        [HttpGet]
        public IActionResult Get()
        {
            Lista paraLer = CriaLista(TipoListaLeitura.ParaLer);
            Lista lendo = CriaLista(TipoListaLeitura.Lendo);
            Lista lidos = CriaLista(TipoListaLeitura.Lidos);
            var colecao = new List<Lista>
            {
                paraLer, lendo, lidos
            };
            return Ok(colecao);
        }
    }
}
