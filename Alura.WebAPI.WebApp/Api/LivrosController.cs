using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alura.WebAPI.WebApp.Api
{
    //Para tornar a aplicação segura vamos utilizar o anotação Authorize
    [Authorize]
    //Para identificar que essa classe é uma classe de API, vamos usar o ApiController
    //Ele auxilia a identificação da classe como uma API
    [ApiController]
    //O atributo Route é obrigatório quando identificamos que a classe é uma API
    [Route("api/[controller]")]
    //ControllerBase é especifico para Web Api
    //Nele não contem recursos para a renderização de Views(Apresentação)
    public class LivrosController : ControllerBase
    {
        private readonly IRepository<Livro> _repository;
        public LivrosController(IRepository<Livro> repository)
        {
            _repository = repository;
        }

        //PADRONIZANDO REST

        [HttpGet]
        public IActionResult Get()
        {
            var listaLivros = _repository.All.Select(l => l.ToApi()).ToList();
            return Ok(listaLivros);
        }

        //Buscar livros por id
        //O segundo seguimento ou parametro é o parametro que o metodo espera na URL
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var livro = _repository.Find(id);
            if (livro == null)
                return NotFound();
            //toModel Metodo criado para converter o model para um "modelView"
            return Ok(livro.ToApi());
        }

        //Pegar capa do livro
        [HttpGet("{id}/capa")]
        public IActionResult GetCapa(int id)
        {
            byte[] img = _repository.All
               .Where(l => l.Id == id)
               .Select(l => l.ImagemCapa)
               .FirstOrDefault();
            if (img != null)
            {
                return File(img, "image/png");
            }
            return File("~/images/capas/capa-vazia.png", "image/png");
        }

        //Incluir livro
        //Para definir que o parametro esta vindo no corpo da requisição
        //vamos utilizar o FromBody
        [HttpPost]
        public IActionResult Post([FromBody]LivroUpload model)
        {
            if (ModelState.IsValid)
            {
                var livro = model.ToLivro();
                _repository.Incluir(livro);
                //O Controller, classe pai do controller livros, tem varios metodos que auxiliam o desenvolvimento WEB
                //Um deles é a criação de URL com base em uma action, podemos passar parametro em um objeto anonimo.
                var uri = Url.Action("Get", new { id = livro.Id });
                return Created(uri, livro);
            }
            //Se o codigo não for valido vamos retornar uma requisição invalida
            return BadRequest();
        }

        //Alterar Livro
        //Para definir que o parametro esta vindo no corpo da requisição
        //vamos utilizar o FromBody
        [HttpPut]
        public IActionResult Put([FromBody]LivroUpload model)
        {
            //Se o modelo for valido
            if (ModelState.IsValid)
            {
                var livro = model.ToLivro();
                if (model.Capa == null)
                {
                    livro.ImagemCapa = _repository.All
                        .Where(l => l.Id == livro.Id)
                        .Select(l => l.ImagemCapa)
                        .FirstOrDefault();
                }
                _repository.Alterar(livro);
                return Ok();
            }
            //Senão formato invalido
            return BadRequest();
        }

        //Metodo para remover objeto pelo id
        //O metodo delete espera como parametro o id, para isso definimos como segundo
        //segmento o id como parametro na URL
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var model = _repository.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            _repository.Excluir(model);
            return NoContent();
        }
    }
}