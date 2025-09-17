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


        // GET: api/AuthDefault/5

        [HttpPost]
        [Route("api/auth/sign-in")]
        public Object GetToken()
        {
            var key = ConfigurationManager.AppSettings["JwtKey"];
            var issuer = ConfigurationManager.AppSettings["JwtIssuer"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //Create a List of Claims, Keep claims name short    
            var permClaims = new List<Claim>();
            permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            permClaims.Add(new Claim("valid", "1"));
            permClaims.Add(new Claim("userid", "1"));
            permClaims.Add(new Claim("name", "bilal"));

            //Create Security Token object by giving required parameters    
            var token = new JwtSecurityToken(issuer, //Issure    
                            issuer,  //Audience    
                            permClaims,
                            expires: DateTime.Now.AddDays(1),
                            signingCredentials: credentials);
            var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
            return new { data = "Test working" };
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

                    return Content(HttpStatusCode.Created, new
                    {
                        id = user.Id,
                        username = user.Username,
                        email = user.Email,
                        createdAt = user.CreatedAt
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
