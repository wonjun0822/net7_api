using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace net7_api.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration Configuration;

        public LoginController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        string? TryGetMessage(string t) => "";

        [HttpPost]
        [Route("login")]
        public async Task<string> Login(string? id)
        {
            DataSet ds = new DataSet();

            string accessToken = string.Empty;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(Configuration["SqlConnection:MySQL"]))
                {
                    await conn.OpenAsync();

                    using (MySqlCommand cmd = new MySqlCommand("SP_Login", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new MySqlParameter("p_id", id));

                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            await da.FillAsync(ds);

                            if (ds != null && ds.Tables[0].Rows.Count > 0)
                            {
                                string? tokenSetting = Configuration.GetSection("TokenSettings").GetValue<string>("Key");

                                var securitykey = new SymmetricSecurityKey(Convert.FromBase64String(tokenSetting == null ? string.Empty : tokenSetting));
                                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                                var jwtToken = new JwtSecurityToken(
                                    issuer: Configuration.GetSection("TokenSettings").GetValue<string>("Issuer"),
                                    audience: Configuration.GetSection("TokenSettings").GetValue<string>("Audience"),
                                    expires: DateTime.Now.AddDays(7),
                                    //expires: DateTime.Now.AddMinutes(10),
                                    signingCredentials: credentials,
                                    claims: new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, id!),
                                        new Claim("auth", TryGetMessage(ds.Tables[0].Rows[0]["auth"].ToString()!)!)
                                    }
                                );

                                accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                            }
                        }
                    }
                }
            }

            catch
            {
                throw;
            }

            return accessToken;
        }

        [HttpPost]
        [Route("reissuanceToken")]
        public string ReissuanceToken()
        {
            string accessToken = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(HttpContext.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value))
                {
                    string? tokenSetting = Configuration.GetSection("TokenSettings").GetValue<string>("Key");

                    var securitykey = new SymmetricSecurityKey(Convert.FromBase64String(tokenSetting == null ? string.Empty : tokenSetting));
                    var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                    var jwtToken = new JwtSecurityToken(
                        issuer: Configuration.GetSection("TokenSettings").GetValue<string>("Issuer"),
                        audience: Configuration.GetSection("TokenSettings").GetValue<string>("Audience"),
                        expires: DateTime.Now.AddDays(7),
                        //expires: DateTime.Now.AddMinutes(10),
                        signingCredentials: credentials,
                        claims: new Claim[] {
                            new Claim(ClaimTypes.NameIdentifier, TryGetMessage(HttpContext.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value!)!),
                        }
                    );

                    accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                }
            }

            catch
            {
                throw;
            }

            return accessToken;
        }
    }
}
