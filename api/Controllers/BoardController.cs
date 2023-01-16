using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using MySql.Data.MySqlClient;

using System.Data;

namespace net7_api.Controllers
{
    [ApiController]
    public class BoardController : ControllerBase
    {
        private readonly IConfiguration Configuration;

        public BoardController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        string? TryGetMessage(string t) => "";

        [HttpGet]
        [Route("boards/{idx}")]
        public string Boards(int idx = 0)
        {
            DataSet ds = new DataSet();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(Configuration["SqlConnection:MySQL"]))
                {
                    conn.Open();

                    using (MySqlCommand cmd = new MySqlCommand("SP_Board", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new MySqlParameter("p_idx", idx));

                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            da.Fill(ds);
                        }
                    }
                }
            }

            catch
            {
                throw;
            }

            return JsonConvert.SerializeObject(ds.Tables[0]);
        }
    }
}
