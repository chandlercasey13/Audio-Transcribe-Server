using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web.Http;
using System.Configuration;
using SimpleServer.Data;         
using SimpleServer.Models;
using SimpleServer.Utils;

namespace SimpleServer.Controllers
{
    
    public class AuthController : ApiController
    {

        private static readonly int MaxFailed = 5;                 
        private static readonly TimeSpan LockoutFor = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan AccessLifetime = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(14);
        // GET: api/AuthDefault/5

        [HttpPost]
        [Route("api/auth/sign-in")]
        public IHttpActionResult SignIn([FromBody] LoginRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return Content(HttpStatusCode.BadRequest, new { error = "email and password are required" });

            try
            {
                using (var db = new AppDbContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Email == req.Email);
                    if (user == null || !PasswordHasher.Verify(req.Password, user.PasswordHash))
                        return Content(HttpStatusCode.Unauthorized, new { error = "invalid credentials" });

                    // 🔑 Build JWT
                    var key = ConfigurationManager.AppSettings["JwtKey"];
                    var issuer = ConfigurationManager.AppSettings["JwtIssuer"];
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("username", user.Username)
            };

                    var token = new JwtSecurityToken(
                        issuer,
                        issuer,
                        claims,
                        expires: DateTime.UtcNow.AddDays(7),
                        signingCredentials: credentials
                    );

                    var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

                    // ✅ Return token + user object
                    return Ok(new
                    {
                        token = jwtToken,
                        user = new
                        {
                            id = user.Id,
                            username = user.Username,
                            email = user.Email,
                            createdAt = user.CreatedAt
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                var msg = (ex.InnerException ?? ex).GetBaseException().Message;
                return Content(HttpStatusCode.InternalServerError, new { error = msg });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("api/auth/sign-up")]
        public IHttpActionResult Signup([FromBody] SignupRequest req)
        {
            if (req == null ||
                string.IsNullOrWhiteSpace(req.Username) ||
                string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password))
                return Content(HttpStatusCode.BadRequest, new { error = "username, email, and password are required" });

            req.Email = req.Email.Trim();
            req.Username = req.Username.Trim();

            try
            {
                using (var db = new AppDbContext())
                {
                    var exists = db.Users.Any(u => u.Username == req.Username || u.Email == req.Email);
                    if (exists)
                        return Content(HttpStatusCode.Conflict, new { error = "username or email already in use" });

                    var user = new User
                    {
                        Username = req.Username,
                        Email = req.Email,
                        PasswordHash = PasswordHasher.Hash(req.Password),
                        CreatedAt = DateTime.UtcNow
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    // build JWT
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        ConfigurationManager.AppSettings["JwtKey"]));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var claims = new[]
                    {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id.ToString())
            };

                    var token = new JwtSecurityToken(
                        issuer: ConfigurationManager.AppSettings["JwtIssuer"],
                        audience: ConfigurationManager.AppSettings["JwtAudience"],
                        claims: claims,
                        expires: DateTime.UtcNow.AddDays(7),
                        signingCredentials: creds);

                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                    return Content(HttpStatusCode.Created, new
                    {
                        id = user.Id,
                        username = user.Username,
                        email = user.Email,
                        createdAt = user.CreatedAt,
                        token = tokenString
                    });
                }
            }
            catch (Exception ex)
            {
                var msg = (ex.InnerException ?? ex).GetBaseException().Message;
                return Content(HttpStatusCode.InternalServerError, new { error = msg });
            }
        }

        //public String GetName1()
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        if (identity != null)
        //        {
        //            IEnumerable<Claim> claims = identity.Claims;
        //        }
        //        return "Valid";
        //    }
        //    else
        //    {
        //        return "Invalid";
        //    }
        //}




    }
}
