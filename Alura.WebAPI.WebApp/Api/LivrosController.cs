using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Microsoft.AspNetCore.Mvc;

namespace Alura.WebAPI.WebApp.Api
{
	public class LivrosController : Controller
	{
		private readonly IRepository<Livro> _repository;
		public LivrosController(IRepository<Livro> repository)
		{
			_repository = repository;
		}

		//Buscar livros por id
		[HttpGet]
		public IActionResult Recupera(int id)
		{
			var livro = _repository.Find(id);
			if (livro == null)
				return NotFound();
			//toModel Metodo criado para converter o model para um "modelView"
			return Json(livro.ToModel());
		}

		//Incluir livro
		[HttpPost]
		public IActionResult Incluir(LivroUpload model)
		{
			if (ModelState.IsValid)
			{
				var livro = model.ToLivro();
				_repository.Incluir(livro);
				//O Controller, classe pai do controller livros, tem varios metodos que auxiliam o desenvolvimento WEB
				//Um deles é a criação de URL com base em uma action, podemos passar parametro em um objeto anonimo.
				var uri = Url.Action("Recuperar", new { id = livro.Id });
				return Created(uri, livro);
			}
			//Se o codigo não for valido vamos retornar uma requisição invalida
			return BadRequest();
		}

		//Alterar Livro
		[HttpPost]
		public IActionResult Alterar(LivroUpload model)
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
		[HttpPost]
		public IActionResult Remover(int id)
		{
			var model = _repository.Find(id);
			if (model == null)
			{
				return NotFound();
			}
			_repository.Excluir(model);
			return RedirectToAction("Index", "Home");
		}
	}
}