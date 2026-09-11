using InmobiliariaULP.Models;
using Microsoft.Data.SqlClient;

namespace InmobiliariaULP.Repositories
{
    public class RepositorioTipoInmueble : RepositorioBase, IRepositorioTipoInmueble
    {
        public RepositorioTipoInmueble(IConfiguration configuration) : base(configuration) { }

        public IList<TipoInmueble> ObtenerTodos()
        {
            var lista = new List<TipoInmueble>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT IdTipoInmueble, Descripcion FROM TiposInmueble ORDER BY Descripcion;";
                using (var command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new TipoInmueble
                            {
                                IdTipoInmueble = reader.GetInt32(0),
                                Descripcion = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public TipoInmueble? ObtenerPorId(int id)
        {
            TipoInmueble? tipo = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT IdTipoInmueble, Descripcion FROM TiposInmueble WHERE IdTipoInmueble = @id;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            tipo = new TipoInmueble
                            {
                                IdTipoInmueble = reader.GetInt32(0),
                                Descripcion = reader.GetString(1)
                            };
                        }
                    }
                }
            }
            return tipo;
        }

        public int Alta(TipoInmueble tipo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "INSERT INTO TiposInmueble (Descripcion) VALUES (@descripcion); SELECT SCOPE_IDENTITY();";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@descripcion", tipo.Descripcion);
                    connection.Open();
                    int nuevoId = Convert.ToInt32(command.ExecuteScalar());
                    tipo.IdTipoInmueble = nuevoId;
                    return nuevoId;
                }
            }
        }

        public int Modificacion(TipoInmueble tipo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE TiposInmueble SET Descripcion = @descripcion WHERE IdTipoInmueble = @id;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@descripcion", tipo.Descripcion);
                    command.Parameters.AddWithValue("@id", tipo.IdTipoInmueble);
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int Baja(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM TiposInmueble WHERE IdTipoInmueble = @id;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }
    }
}
