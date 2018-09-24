using Alura.ListaLeitura.Persistencia;
using Alura.ListaLeitura.Seguranca;
using Alura.ListaLeitura.Modelos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Alura.WebAPI.WebApp.Formatters;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Alura.ListaLeitura.WebApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<LeituraContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("ListaLeitura"));
            });

            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("AuthDB"));
            });

            services.AddIdentity<Usuario, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            }).AddEntityFrameworkStores<AuthDbContext>();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Usuario/Login";
            });

            services.AddTransient<IRepository<Livro>, RepositorioBaseEF<Livro>>();

            //Como default o serializador é json, porem vamos adicionar tambem o xml
            //Criamos alguns formatos exclusivos de retorno da da API e para incluilos na aplicação
            //temos que adicionar algumas opções no MVC
            services.AddMvc(options =>
            {
                //Importando o novo formato
                options.OutputFormatters.Add(new LivroCsvFormatter());
            }).AddXmlSerializerFormatters();

            //Adicionando a autenticação via jwt
            //Vamos adicionar algumas opções de autenticação
            services.AddAuthentication(options =>
            {
                //Definindo a autentição default como JwtBearer
                //O Bearer é o portador do token
                options.DefaultAuthenticateScheme = "JwtBearer";
                options.DefaultChallengeScheme = "JwtBearer";
                //Após definir o portador default, temos que adicionar o schema do token
            }).AddJwtBearer("JwtBearer", options =>
            {
                //os parametros para autenticar o token é feito atravez da classe TokenValidationParameters
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //Parametros de validações
                    //vamos validar quem é o emissor do token
                    ValidateIssuer = true,
                    //vamos validar quem esta pedindo o token
                    ValidateAudience = true,
                    //vamos validar tambem a expiração do token
                    ValidateLifetime = true,
                    //Validar chave secreta
                    ValidateIssuerSigningKey = true,
                    //Vamos validar a chave de assinatura, essa é chave secreta que valida se o token é valido
                    //Vamos usar a classe SymmetricSecurityKey para gerar a chave secreta e como parametro vamos passar os Bits
                    //De um texto que tenho mais de 256 bits, ou seja o texto tem que ser grande
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("alura-webapi-authentication-valid")),
                    //Quanto tempo o teken é valido, tempo de validação do token
                    ClockSkew = TimeSpan.FromMinutes(5),
                    //Nome da aplicação
                    ValidIssuer = "Alura.WebApp",
                    //Uma audiencia valida, ou seja um cliente valido vamos usar o postman.
                    //Aqui pode ser qualquer nome de nossos cliente que vão utilizar a aplicação
                    ValidAudience = "Postman"
                };
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
