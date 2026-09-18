using Microsoft.Data.SqlClient;
using InmobiliariaULP.Models;

namespace InmobiliariaULP.Repositories
{
    public class RepositorioImagenInmueble : RepositorioBase, IRepositorioImagenInmueble
    {
        public RepositorioImagenInmueble(IConfiguration configuration) : base(configuration) { }

        public IList<ImagenInmueble> ObtenerPorInmueble(int idInmueble)
        {
            var lista = new List<ImagenInmueble>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT IdImagen, Url, IdInmueble FROM ImagenesInmueble WHERE IdInmueble = @idInmueble;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInmueble", idInmueble);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ImagenInmueble
                            {
                                IdImagen = reader.GetInt32(0),
                                Url = reader.GetString(1),
                                IdInmueble = reader.GetInt32(2)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public ImagenInmueble? ObtenerPorId(int idImagen)
        {
            ImagenInmueble? img = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT IdImagen, Url, IdInmueble FROM ImagenesInmueble WHERE IdImagen = @id;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", idImagen);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            img = new ImagenInmueble
                            {
                                IdImagen = reader.GetInt32(0),
                                Url = reader.GetString(1),
                                IdInmueble = reader.GetInt32(2)
                            };
                        }
                    }
                }
            }
            return img;
        }

        public int Alta(ImagenInmueble imagen)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO ImagenesInmueble (Url, IdInmueble)
                    VALUES (@url, @idInmueble);
                    SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@url", imagen.Url);
                    command.Parameters.AddWithValue("@idInmueble", imagen.IdInmueble);
                    connection.Open();
                    int nuevoId = Convert.ToInt32(command.ExecuteScalar());
                    imagen.IdImagen = nuevoId;
                    return nuevoId;
                }
            }
        }

        public int Borrar(int idImagen)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM ImagenesInmueble WHERE IdImagen = @id;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", idImagen);
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }
    }
}