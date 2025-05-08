using System.Data;
using Microsoft.Extensions.Configuration;

namespace WebAPI_TwoFactor.Clases
{
    public class Usuarios
    {
        private readonly IConfiguration _configuration;

        public Usuarios()
        {
            AccesoDatos.cadenaConexion = @"Data Source=NICK;Initial Catalog=EPV;Integrated Security=true;";
        }

        public Usuarios(IConfiguration configuration)
        {
            _configuration = configuration;
            AccesoDatos.cadenaConexion = _configuration?.GetConnectionString("DefaultConnection")
                ?? @"Data Source=NICK;Initial Catalog=EPV;Integrated Security=true;";
        }

        public void CreateUser(string email, string passwordHash)
        {
            string sql = "INSERT INTO dbo.Usuarios (Email, PasswordHash) VALUES (@email, @passwordHash)";
            string[] parametros = new string[] { "@email:" + email, "@passwordHash:" + passwordHash };
            AccesoDatos.GetDataTable(sql, parametros);
        }

        public string GetPasswordHash(string email)
        {
            string sql = "SELECT PasswordHash FROM dbo.Usuarios WHERE email=@email";
            string[] parametros = new string[] { "@email:" + email };
            DataTable dt = AccesoDatos.GetDataTable(sql, parametros);

            if (dt.Rows.Count == 1)
            {
                return dt.Rows[0]["PasswordHash"].ToString();
            }
            else
            {
                return "";
            }
        }

        public bool IsTwoFactorEnabled(string email)
        {
            string sql = "SELECT TwoFactorEnabled FROM dbo.Usuarios WHERE email=@email";
            string[] parametros = new string[] { "@email:" + email };
            DataTable dt = AccesoDatos.GetDataTable(sql, parametros);

            if (dt.Rows.Count == 1)
            {
                return Convert.ToBoolean(dt.Rows[0]["TwoFactorEnabled"]);
            }
            else
            {
                return false;
            }
        }

        public void EnableTwoFactor(string email)
        {
            string sql = "UPDATE dbo.Usuarios SET TwoFactorEnabled=1 WHERE email=@email";
            string[] parametros = new string[] { "@email:" + email };
            AccesoDatos.GetDataTable(sql, parametros);
        }

        public void SetSecret(string email, string code)
        {
            string sql = "UPDATE dbo.Usuarios SET TwoFactorSecret=@code, FechaModificacion=GETDATE() WHERE email=@email";
            string[] parametros = new string[] { "@code:" + code, "@email:" + email };
            AccesoDatos.GetDataTable(sql, parametros);
        }

        public string GetSecret(string email)
        {
            string sql = "SELECT TwoFactorSecret FROM dbo.Usuarios WHERE email=@email";
            string[] parametros = new string[] { "@email:" + email };
            DataTable dt = AccesoDatos.GetDataTable(sql, parametros);

            if (dt.Rows.Count == 1)
            {
                return dt.Rows[0]["TwoFactorSecret"].ToString();
            }
            else
            {
                return "";
            }
        }
    }
}