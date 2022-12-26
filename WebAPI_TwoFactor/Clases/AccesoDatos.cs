using System.Data;
using System.Data.SqlClient;

namespace WebAPI_TwoFactor.Clases
{
    public class AccesoDatos
    {
        // Cadena de conexión a la BBDD SQL
        public static string cadenaConexion = "";

        #region " Acceso a datos "

        /// <summary>
        /// Ejecuta una consulta SQL en la base de datos, y devuelve los resultados obtenidos en un objeto DataTable.
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="parametros">Array de string con formato: nombre:valor</param>
        /// <returns>Devuelve un objeto DataTable con los resultados obtenidos tras ejecución de la consulta.</returns>
        public static DataTable GetDataTable(string SQL, string[] parametros)
        {
            try
            {
                SqlConnection conexion = new SqlConnection(cadenaConexion);
                SqlCommand comando = new SqlCommand(SQL, conexion);
                for (int i = 0; i < parametros.Length; i++)
                    comando.Parameters.Add(new SqlParameter(parametros[i].Split(':')[0], parametros[i].Split(':')[1]));
                SqlDataAdapter da = new SqlDataAdapter(comando);
                DataSet ds = new DataSet();
                da.Fill(ds);
                conexion.Close();
                da.Dispose();
                conexion.Dispose();
                return ds.Tables[0];
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Ejecuta una consulta SQL en la base de datos, y devuelve los resultados obtenidos en un objeto DataTable.
        /// Este método es vulnerable a inyección de dependencias, por lo que debe usarse sólamente de forma interna.
        /// Para consultas que vengan desde fuera, usar GetDataTable.
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="parametros">Array de string con formato: nombre:valor</param>
        /// <returns>Devuelve un objeto DataTable con los resultados obtenidos tras ejecución de la consulta.</returns>
        public static DataTable GetTmpDataTable(string SQL)
        {
            try
            {
                SqlConnection conexion = new SqlConnection(cadenaConexion);
                SqlCommand comando = new SqlCommand(SQL, conexion);
                SqlDataAdapter da = new SqlDataAdapter(comando);
                DataSet ds = new DataSet();
                da.Fill(ds);
                conexion.Close();
                da.Dispose();
                conexion.Dispose();
                return ds.Tables[0];
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Ejecuta un procedimiento almacenado en la base de datos, y devuelve los resultados obtenidos en un objeto DataTable.
        /// </summary>
        /// <param name="procedimientoAlmacenado"></param>
        /// <param name="parametros"></param>
        /// <returns>Devuelve un objeto DataTable con los resultados obtenidos tras ejecutar el procedimiento almacenado.</returns>
        public static DataTable ExecuteStoredProcedure(string procedimientoAlmacenado, SqlParameter[] parametros)
        {
            try
            {
                SqlConnection conexion = new SqlConnection(cadenaConexion);
                SqlCommand comando = new SqlCommand();
                comando.CommandType = CommandType.StoredProcedure;
                comando.CommandText = procedimientoAlmacenado;
                comando.Connection = conexion;
                if (parametros != null)
                {
                    for (int i = 0; i < parametros.Length; i++)
                    {
                        if (parametros[i].DbType == DbType.DateTime && parametros[i].Value != null) { parametros[i].Value = parametros[i].Value.ToString().Replace(" ", "T"); }
                        if (parametros[i].DbType == DbType.DateTime && parametros[i].SqlValue != null) { parametros[i].SqlValue = parametros[i].SqlValue.ToString().Replace(" ", "T"); }
                        comando.Parameters.Add(parametros[i]);
                    }
                }
                DataTable dt = new DataTable();
                conexion.Open();
                dt.Load(comando.ExecuteReader());
                conexion.Close();
                comando.Dispose(); conexion.Dispose();
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Ejecutar un comando SQL en la base de datos, sin devolución de resultados.
        /// </summary>
        /// <param name="SQL"></param>
        public static void ExecuteQuery(string SQL)
        {
            SqlConnection con = new SqlConnection(cadenaConexion);
            SqlCommand cmd = new SqlCommand(SQL, con);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }

        #endregion
    }
}
