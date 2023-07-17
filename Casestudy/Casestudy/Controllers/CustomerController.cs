using Casestudy.DAL;
using Casestudy.DAL.DAO;
using Casestudy.DAL.DomainClasses;
using Casestudy.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Casestudy.Controllers
{
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        readonly AppDbContext? _ctx;
        readonly IConfiguration configuration;

        public CustomerController(AppDbContext context, IConfiguration config) // injected here
        {
            _ctx = context;
            this.configuration = config;
        }

        [HttpPost]
        [Route("api/[controller]/Login")]
        [Produces("application/json")]
        [AllowAnonymous]
        public async Task<ActionResult<CustomerHelper>> Login(CustomerHelper helper)
        {
            CustomerDAO dao = new(_ctx!);
            Customer? customer = await dao.GetByEmail(helper.Email);
            if (customer != null)
            {
                if (VerifyPassword(helper.Password, customer.Hash!, customer.Salt!))
                {
                    helper.Password = "";
                    var appSettings = configuration.GetSection("AppSettings").GetValue<string>("Secret");
                    // authentication successful so generate jwt token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(appSettings!);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, customer.Id.ToString()) }),
                        Expires = DateTime.UtcNow.AddDays(7),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    string returnToken = tokenHandler.WriteToken(token);
                    helper.Token = returnToken;
                }
                else
                {
                    helper.Token = "username or password invalid - login failed";
                }
            }
            else
            {
                helper.Token = "no such user - login failed";
            }
            return helper;
        }

        [HttpPost]
        [Route("api/[controller]/Register")]
        [Produces("application/json")]
        [AllowAnonymous]
        public async Task<ActionResult<CustomerHelper>> Register(CustomerHelper helper)
        {
            CustomerDAO dao = new(_ctx!);
            Customer? already = await dao.GetByEmail(helper.Email);
            if (already == null)
            {
                HashSalt hashSalt = GenerateSaltedHash(64, helper.Password!);
                helper.Password = ""; // don’t need the string anymore
                Customer dbCustomer = new()
                {
                    Firstname = helper.Firstname!,
                    Lastname = helper.Lastname!,
                    Email = helper.Email!,
                    Hash = hashSalt.Hash!,
                    Salt = hashSalt.Salt!
                };
                dbCustomer = await dao.Register(dbCustomer);
                if (dbCustomer.Id > 0)
                    helper.Token = "user registered";
                else
                    helper.Token = "user registration failed";
            }
            else
            {
                helper.Token = "user registration failed - email already in use";
            }
            return helper;
        }
        private static HashSalt GenerateSaltedHash(int size, string password)
        {
            var saltBytes = new byte[size];
            var provider = RandomNumberGenerator.Create();
            // Fills an array of bytes with a cryptographically strong sequence of random nonzero values.
            provider.GetNonZeroBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);
            // a password, salt, and iteration count, an algorithm, then generates a binary key
            Rfc2898DeriveBytes rfc2898DeriveBytes = new(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            var hashPassword = Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(256));
            HashSalt hashSalt = new() { Hash = hashPassword, Salt = salt };
            return hashSalt;
        }

        public static bool VerifyPassword(string? enteredPassword, string storedHash, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);
            Rfc2898DeriveBytes rfc2898DeriveBytes = new(enteredPassword!, saltBytes, 10000,
            HashAlgorithmName.SHA256);
            return Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(256)) == storedHash;
        }
    }

    public class HashSalt
    {
        public string? Hash { get; set; }
        public string? Salt { get; set; }
    }
}
