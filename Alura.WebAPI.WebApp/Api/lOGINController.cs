using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Alura.ListaLeitura.Seguranca;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Alura.WebAPI.WebApp.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        //classe do identity usada para fazer a autenticação pelo identity
        private readonly SignInManager<Usuario> _signInManager;

        public LoginController(SignInManager<Usuario> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpPost]
        //validar usuário e gerar token para retorna lo
        public async Task<IActionResult> Token(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                //realizar autenticação com o identity
                var result = await _signInManager.PasswordSignInAsync(model.Login, model.Password, true, true);
                //Se o usuário for valido retorna o token
                if (result.Succeeded)
                {
                    //CRIAR TOKEN (header + payload >> claims ou direitos de acesso + signature)

                    //Payload do token
                    var direitos = new[]
                    {
                        //a primeira claim é o sujeito do token, ou seja o usuário dono do token
                        new Claim(JwtRegisteredClaimNames.Sub, model.Login),
                        //Identificador unico do token, o Guid é utilizado porque não se repete
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                    //Gerar assinatura
                    //A assinatura tem que ser a mesma que a da aplicação definida no startup
                    var chave = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("alura-webapi-authentication-valid"));
                    //Definir as credenciais para gerar o token, como a chave secreta e a criptografia
                    var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

                    //Gerar o token, com as informações necessarias definidas no startup para gerar o token
                    var token = new JwtSecurityToken(
                            //Nome da aplicação
                            issuer: "Alura.WebApp",
                            //Nome do cliente
                            audience: "Postman",
                            //PayLoad, direitos do cliente
                            claims: direitos,
                            //Credenciais para gerar o token, chave e criptografia
                            signingCredentials: credenciais,
                            //Token expira da data atual mais 30 minutos
                            expires: DateTime.Now.AddMinutes(30)
                        );

                    //Transformando o token em uma string
                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                    return Ok(tokenString);
                }
                //se não, retorna 401 Não autorizado
                else
                    return Unauthorized();
            }
            return BadRequest();
        }
    }
}