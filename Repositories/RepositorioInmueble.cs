using InmobiliariaULP.Models;
using Microsoft.Data.SqlClient;

namespace InmobiliariaULP.Repositories
{
    public class RepositorioInmueble : RepositorioBase, IRepositorioInmueble
    {
        public RepositorioInmueble(IConfiguration configuration) : base(configuration) { }

        public IList<Inmueble> ObtenerTodos(string? search = null, int? idTipo = null, bool? soloDisponibles = null)
        {
            var lista = new List<Inmueble>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT i.IdInmueble, i.Direccion, i.Cupo, i.Latitud, i.Longitud, i.PrecioPorDia, 
                           i.PorcentajeReserva, i.Disponible, i.ImagenPortada, i.IdPropietario, i.IdTipoInmueble,
                           p.Nombre, p.Apellido, t.Descripcion
                    FROM Inmuebles i
                    INNER JOIN Propietarios p ON i.IdPropietario = p.IdPropietario
                    INNER JOIN TiposInmueble t ON i.IdTipoInmueble = t.IdTipoInmueble
                    WHERE (@search IS NULL OR i.Direccion LIKE '%' + @search + '%' OR p.Apellido LIKE '%' + @search + '%')
                      AND (@idTipo IS NULL OR i.IdTipoInmueble = @idTipo)
                      AND (@soloDisponibles IS NULL OR i.Disponible = @soloDisponibles)
                    ORDER BY i.IdInmueble DESC;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@search", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search);
                    command.Parameters.AddWithValue("@idTipo", (object?)idTipo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@soloDisponibles", (object?)soloDisponibles ?? DBNull.Value);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Inmueble
                            {
                                IdInmueble = reader.GetInt32(0),
                                Direccion = reader.GetString(1),
                                Cupo = reader.GetInt32(2),
                                Latitud = reader.GetDecimal(3),
                                Longitud = reader.GetDecimal(4),
                                PrecioPorDia = reader.GetDecimal(5),
                                PorcentajeReserva = reader.GetDecimal(6),
                                Disponible = reader.GetBoolean(7),
                                ImagenPortada = reader.IsDBNull(8) ? null : reader.GetString(8),
                                IdPropietario = reader.GetInt32(9),
                                IdTipoInmueble = reader.GetInt32(10),
                                Propietario = new Propietario { IdPropietario = reader.GetInt32(9), Nombre = reader.GetString(11), Apellido = reader.GetString(12) },
                                TipoInmueble = new TipoInmueble { IdTipoInmueble = reader.GetInt32(10), Descripcion = reader.GetString(13) }
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public Inmueble? ObtenerPorId(int id)
        {
            Inmueble? i = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT i.IdInmueble, i.Direccion, i.Cupo, i.Latitud, i.Longitud, i.PrecioPorDia, 
                           i.PorcentajeReserva, i.Disponible, i.ImagenPortada, i.IdPropietario, i.IdTipoInmueble,
                           p.Nombre, p.Apellido, t.Descripcion
                    FROM Inmuebles i
                    INNER JOIN Propietarios p ON i.IdPropietario = p.IdPropietario
                    INNER JOIN TiposInmueble t ON i.IdTipoInmueble = t.IdTipoInmueble
                    WHERE i.IdInmueble = @id;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            i = new Inmueble
                            {
                                IdInmueble = reader.GetInt32(0),
                                Direccion = reader.GetString(1),
                                Cupo = reader.GetInt32(2),
                                Latitud = reader.GetDecimal(3),
                                Longitud = reader.GetDecimal(4),
                                PrecioPorDia = reader.GetDecimal(5),
                                PorcentajeReserva = reader.GetDecimal(6),
                                Disponible = reader.GetBoolean(7),
                                ImagenPortada = reader.IsDBNull(8) ? null : reader.GetString(8),
                                IdPropietario = reader.GetInt32(9),
                                IdTipoInmueble = reader.GetInt32(10),
                                Propietario = new Propietario { IdPropietario = reader.GetInt32(9), Nombre = reader.GetString(11), Apellido = reader.GetString(12) },
                                TipoInmueble = new TipoInmueble { IdTipoInmueble = reader.GetInt32(10), Descripcion = reader.GetString(13) }
                            };
                        }
                    }
                }
            }
            return i;
        }

        public int Alta(Inmueble i)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO Inmuebles (Direccion, Cupo, Latitud, Longitud, PrecioPorDia, PorcentajeReserva, Disponible, ImagenPortada, IdPropietario, IdTipoInmueble)
                    VALUES (@dir, @cupo, @lat, @long, @precio, @porc, @disp, @img, @idProp, @idTipo);
                    SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dir", i.Direccion);
                    command.Parameters.AddWithValue("@cupo", i.Cupo);
                    command.Parameters.AddWithValue("@lat", i.Latitud);
                    command.Parameters.AddWithValue("@long", i.Longitud);
                    command.Parameters.AddWithValue("@precio", i.PrecioPorDia);
                    command.Parameters.AddWithValue("@porc", i.PorcentajeReserva);
                    command.Parameters.AddWithValue("@disp", i.Disponible);
                    command.Parameters.AddWithValue("@img", (object?)i.ImagenPortada ?? DBNull.Value);
                    command.Parameters.AddWithValue("@idProp", i.IdPropietario);
                    command.Parameters.AddWithValue("@idTipo", i.IdTipoInmueble);

                    connection.Open();
                    int nuevoId = Convert.ToInt32(command.ExecuteScalar());
                    i.IdInmueble = nuevoId;
                    return nuevoId;
                }
            }
        }

        public int Modificacion(Inmueble i)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Inmuebles
                    SET Direccion = @dir, Cupo = @cupo, Latitud = @lat, Longitud = @long, PrecioPorDia = @precio, 
                        PorcentajeReserva = @porc, Disponible = @disp, ImagenPortada = @img, IdPropietario = @idProp, IdTipoInmueble = @idTipo
                    WHERE IdInmueble = @id;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dir", i.Direccion);
                    command.Parameters.AddWithValue("@cupo", i.Cupo);
                    command.Parameters.AddWithValue("@lat", i.Latitud);
                    command.Parameters.AddWithValue("@long", i.Longitud);
                    command.Parameters.AddWithValue("@precio", i.PrecioPorDia);
                    command.Parameters.AddWithValue("@porc", i.PorcentajeReserva);
                    command.Parameters.AddWithValue("@disp", i.Disponible);
                    command.Parameters.AddWithValue("@img", (object?)i.ImagenPortada ?? DBNull.Value);
                    command.Parameters.AddWithValue("@idProp", i.IdPropietario);
                    command.Parameters.AddWithValue("@idTipo", i.IdTipoInmueble);
                    command.Parameters.AddWithValue("@id", i.IdInmueble);

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int Baja(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM Inmuebles WHERE IdInmueble = @id;";
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
