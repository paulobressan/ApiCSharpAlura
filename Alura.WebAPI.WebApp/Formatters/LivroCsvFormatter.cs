using Alura.ListaLeitura.Modelos;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alura.WebAPI.WebApp.Formatters
{
    //Classe que representa um novo formato de resposta, a classe herda de TextOutputFormatter,
    //que significa que a saida sera formatada em texto
    public class LivroCsvFormatter : TextOutputFormatter
    {
        public LivroCsvFormatter()
        {
            //Criando o tipo da resposta que representa esse formatter
            //Temos que importar 
            //using Microsoft.Net.Http.Headers;
            var textCsvMediaType = MediaTypeHeaderValue.Parse("text/csv");
            //podemos adicionar varios mediatype, como por exemplo vamos suportar dois tipos de accepts
            //Ou tipos de retorno, o text/csv e o application/csv. Os dois vão utilizar esse formater para responder
            //o formato solicitado
            var applicationCsvMediaType = MediaTypeHeaderValue.Parse("application/csv");
            //Atributo da heraça, ele é uma lista de defini quais MediaType vão ser suportado 
            //nesse formater, no caso somente os dois criado acima text/csv e o application/csv
            SupportedMediaTypes.Add(textCsvMediaType);
            SupportedMediaTypes.Add(applicationCsvMediaType);
            //Temos que adicionar tambem o encoding de caracteres suportados
            //Tambem podemos adicionar outros formatos de caracters por ser uma lista
            SupportedEncodings.Add(Encoding.UTF8);
        }

        //Quando utilizamos uma chamada que retorna uma lista, o nosso formatter não suporta lista 
        //de objetos porque configuramos para que ele formata somente um Objeto,  if(context.Object is LivroApi)
        //Porem podemos bloquear o nosso formater para que ele formata em determinadas condição
        //A condição é, se a resposta for somente um Objeto, vamos liberar a formatação para CSV
        protected override bool CanWriteType(Type type)
        {
            //retornar verdadeiro se estivermos retornando somente um livro ou falso
            //se tivermos retornando qualquer outro valor
            var typeVerification = (type == typeof(LivroApi)) || (type == typeof(List<LivroApi>));
            return typeVerification;
        }

        //Implementação do PAI
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var livroEmCsv = "";

            //O livro que vai ser respondido fica amarzenado no contexto na variavel Object
            //Temos que convertelo para a sua classe para manipular seus valores, porem temos que verificar se ele realmente
            //É a classe que desejamos
            if(context.Object is LivroApi)
            {
                //Se for
                var livro = context.Object as LivroApi;

                //Criando o formato Csv
                livroEmCsv = $"{livro.Titulo};{livro.Subtitulo};{livro.Autor};{livro.Lista}";
            }
            //Dando suporte para listas
            else if(context.Object is List<LivroApi>)
            {
                var livros = context.Object as List<LivroApi>;
                livros.ForEach(livro => livroEmCsv += $"{livro.Titulo};{livro.Subtitulo};{livro.Autor};{livro.Lista},");
            }

            //O WriterFactory é um escritor de Stream, o Response.Body só recebe Strem e por isso temos que utilizar o WriterFactory
            //O WriteFactory espera como primeiro parametro o body que é onde ele vai escrever e em segundo o Encoding que é a representação do tipo de caracter no Stream
            using (var escritor = context.WriterFactory(context.HttpContext.Response.Body, selectedEncoding))
            {
                return escritor.WriteAsync(livroEmCsv);
            }
            //sempre que chamarmos o WriterFactory é criado uma instancia porque o WriterFactory é uma "fabrica de escritores"
            //e por isso é necessario fecha-lo
            //Poremo ele herda de Dispose e por isso podemos usar esse recurso para que feche automaticamente
            //escritor.Close();
        }
    }
}
