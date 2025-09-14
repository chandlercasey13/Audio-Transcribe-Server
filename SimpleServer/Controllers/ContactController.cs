using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SimpleServer.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace SimpleServer.Controllers
{   

    public class ContactController : ApiController
    {
        Contact[] contacts = new Contact[]
            {
                new Contact() { Id = 1, FirstName = "John", LastName = "Doe"},
                new Contact() { Id = 2, FirstName = "Jane", LastName="Doe"},
                new Contact() { Id = 3, FirstName = "Sammy", LastName = "Doe"},

            };
        // GET: api/Contact
        public IEnumerable<Contact> Get()
        {
            return contacts;
        }

        // GET: api/Contact/5
        public string Get(int id)
        {
            return "value";
        }

        [Authorize]
        [HttpPost]
        public Object GetName2()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                var name = claims.Where(p => p.Type == "name").FirstOrDefault()?.Value;
                return new
                {
                    data = name
                };

            }
            return null;
        }

        // PUT: api/Contact/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Contact/5
        public void Delete(int id)
        {
        }

        
    }
}
