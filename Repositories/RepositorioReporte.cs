using Microsoft.Data.SqlClient;
using InmobiliariaULP.Models;

namespace InmobiliariaULP.Repositories
{
    public class RepositorioReporte : RepositorioBase, IRepositorioReporte
    {
        public RepositorioReporte(IConfiguration configuration) : base(configuration) { }

        private Inmueble MapearInmueble(SqlDataReader reader)
        {
            return new Inmueble
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
                Propietario = new Propietario
                {
                    IdPropietario = reader.GetInt32(9),
                    Nombre = reader.GetString(10),
                    Apellido = reader.GetString(11),
                    Telefono = reader.GetString(12),
                    Email = reader.GetString(13)
                },
                IdTipoInmueble = reader.GetInt32(14),
                TipoInmueble = new TipoInmueble
                {
                    IdTipoInmueble = reader.GetInt32(14),
                    Descripcion = reader.GetString(15)
                }
            };
        }

        public IList<Inmueble> ObtenerInmueblesPorDisponibilidad(bool? disponible)
        {
            var lista = new List<Inmueble>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT i.IdInmueble, i.Direccion, i.Cupo, i.Latitud, i.Longitud, i.PrecioPorDia, i.PorcentajeReserva, i.Disponible, i.ImagenPortada,
                           i.IdPropietario, p.Nombre, p.Apellido, p.Telefono, p.Email,
                           i.IdTipoInmueble, t.Descripcion
                    FROM Inmuebles i
                    INNER JOIN Propietarios p ON i.IdPropietario = p.IdPropietario
                    INNER JOIN TiposInmueble t ON i.IdTipoInmueble = t.IdTipoInmueble
                    WHERE (@disponible IS NULL OR i.Disponible = @disponible)
                    ORDER BY i.Direccion;";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@disponible", (object?)disponible ?? DBNull.Value);
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearInmueble(reader));
                        }
                    }
                }
            }
            return lista;
        }

        public IList<Inmueble> ObtenerInmueblesPorPropietario(int idPropietario)
        {
            var lista = new List<Inmueble>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT i.IdInmueble, i.Direccion, i.Cupo, i.Latitud, i.Longitud, i.PrecioPorDia, i.PorcentajeReserva, i.Disponible, i.ImagenPortada,
                           i.IdPropietario, p.Nombre, p.Apellido, p.Telefono, p.Email,
                           i.IdTipoInmueble, t.Descripcion
                    FROM Inmuebles i
                    INNER JOIN Propietarios p ON i.IdPropietario = p.IdPropietario
                    INNER JOIN TiposInmueble t ON i.IdTipoInmueble = t.IdTipoInmueble
                    WHERE i.IdPropietario = @idPropietario
                    ORDER BY i.Direccion;";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@idPropietario", idPropietario);
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearInmueble(reader));
                        }
                    }
                }
            }
            return lista;
        }

        public IList<InmuebleConReservasVM> ObtenerMasReservadosUltimoAno()
        {
            var lista = new List<InmuebleConReservasVM>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT i.IdInmueble, i.Direccion, i.Cupo, i.Latitud, i.Longitud, i.PrecioPorDia, i.PorcentajeReserva, i.Disponible, i.ImagenPortada,
                           i.IdPropietario, p.Nombre, p.Apellido, p.Telefono, p.Email,
                           i.IdTipoInmueble, t.Descripcion,
                           COUNT(r.IdReserva) AS CantidadReservas
                    FROM Inmuebles i
                    INNER JOIN Propietarios p ON i.IdPropietario = p.IdPropietario
                    INNER JOIN TiposInmueble t ON i.IdTipoInmueble = t.IdTipoInmueble
                    INNER JOIN Reservas r ON i.IdInmueble = r.IdInmueble
                    WHERE r.FechaInicio >= DATEADD(day, -365, GETDATE())
                    GROUP BY i.IdInmueble, i.Direccion, i.Cupo, i.Latitud, i.Longitud, i.PrecioPorDia, i.PorcentajeReserva, i.Disponible, i.ImagenPortada,
                             i.IdPropietario, p.Nombre, p.Apellido, p.Telefono, p.Email,
                             i.IdTipoInmueble, t.Descripcion
                    ORDER BY CantidadReservas DESC;";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new InmuebleConReservasVM
                            {
                                Inmueble = MapearInmueble(reader),
                                CantidadReservas = reader.GetInt32(16)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public IList<Inmueble> ObtenerInmueblesSinReservasEnDias(int dias)
        {
            var lista = new List<Inmueble>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT i.IdInmueble, i.Direccion, i.Cupo, i.Latitud, i.Longitud, i.PrecioPorDia, i.PorcentajeReserva, i.Disponible, i.ImagenPortada,
                           i.IdPropietario, p.Nombre, p.Apellido, p.Telefono, p.Email,
                           i.IdTipoInmueble, t.Descripcion
                    FROM Inmuebles i
                    INNER JOIN Propietarios p ON i.IdPropietario = p.IdPropietario
                    INNER JOIN TiposInmueble t ON i.IdTipoInmueble = t.IdTipoInmueble
                    WHERE NOT EXISTS (
                        SELECT 1 FROM Reservas r
                        WHERE r.IdInmueble = i.IdInmueble
                          AND r.FechaInicio >= DATEADD(day, -@dias, GETDATE())
                    )
                    ORDER BY i.Direccion;";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@dias", dias);
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearInmueble(reader));
                        }
                    }
                }
            }
            return lista;
        }

        public IList<Reserva> ObtenerReservasVigentesEnRango(DateTime desde, DateTime hasta)
        {
            var lista = new List<Reserva>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT r.IdReserva, r.FechaInicio, r.FechaFin, r.FechaFinOriginal, r.MontoDiario, r.Estado,
                           r.IdInmueble, i.Direccion,
                           r.IdInquilino, inq.Nombre, inq.Apellido
                    FROM Reservas r
                    INNER JOIN Inmuebles i ON r.IdInmueble = i.IdInmueble
                    INNER JOIN Inquilinos inq ON r.IdInquilino = inq.IdInquilino
                    WHERE r.Estado = 'Vigente'
                      AND r.FechaInicio <= @hasta
                      AND r.FechaFin >= @desde
                    ORDER BY r.FechaInicio ASC;";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@desde", desde);
                    cmd.Parameters.AddWithValue("@hasta", hasta);
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Reserva
                            {
                                IdReserva = reader.GetInt32(0),
                                FechaInicio = reader.GetDateTime(1),
                                FechaFin = reader.GetDateTime(2),
                                FechaFinOriginal = reader.GetDateTime(3),
                                MontoDiario = reader.GetDecimal(4),
                                Estado = reader.GetString(5),
                                IdInmueble = reader.GetInt32(6),
                                Inmueble = new Inmueble { IdInmueble = reader.GetInt32(6), Direccion = reader.GetString(7) },
                                IdInquilino = reader.GetInt32(8),
                                Inquilino = new Inquilino { IdInquilino = reader.GetInt32(8), Nombre = reader.GetString(9), Apellido = reader.GetString(10) }
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public IList<Reserva> ObtenerReservasPorFinalizar(int dias)
        {
            var lista = new List<Reserva>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT r.IdReserva, r.FechaInicio, r.FechaFin, r.FechaFinOriginal, r.MontoDiario, r.Estado,
                           r.IdInmueble, i.Direccion,
                           r.IdInquilino, inq.Nombre, inq.Apellido
                    FROM Reservas r
                    INNER JOIN Inmuebles i ON r.IdInmueble = i.IdInmueble
                    INNER JOIN Inquilinos inq ON r.IdInquilino = inq.IdInquilino
                    WHERE r.Estado = 'Vigente'
                      AND r.FechaFin >= CAST(GETDATE() AS DATE)
                      AND r.FechaFin <= DATEADD(day, @dias, CAST(GETDATE() AS DATE))
                    ORDER BY r.FechaFin ASC;";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@dias", dias);
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Reserva
                            {
                                IdReserva = reader.GetInt32(0),
                                FechaInicio = reader.GetDateTime(1),
                                FechaFin = reader.GetDateTime(2),
                                FechaFinOriginal = reader.GetDateTime(3),
                                MontoDiario = reader.GetDecimal(4),
                                Estado = reader.GetString(5),
                                IdInmueble = reader.GetInt32(6),
                                Inmueble = new Inmueble { IdInmueble = reader.GetInt32(6), Direccion = reader.GetString(7) },
                                IdInquilino = reader.GetInt32(8),
                                Inquilino = new Inquilino { IdInquilino = reader.GetInt32(8), Nombre = reader.GetString(9), Apellido = reader.GetString(10) }
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public IList<Inmueble> BuscarInmueblesDisponibles(DateTime fechaInicio, DateTime fechaFin)
        {
            var lista = new List<Inmueble>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT i.IdInmueble, i.Direccion, i.Cupo, i.Latitud, i.Longitud, i.PrecioPorDia, i.PorcentajeReserva, i.Disponible, i.ImagenPortada,
                           i.IdPropietario, p.Nombre, p.Apellido, p.Telefono, p.Email,
                           i.IdTipoInmueble, t.Descripcion
                    FROM Inmuebles i
                    INNER JOIN Propietarios p ON i.IdPropietario = p.IdPropietario
                    INNER JOIN TiposInmueble t ON i.IdTipoInmueble = t.IdTipoInmueble
                    WHERE i.Disponible = 1
                      AND NOT EXISTS (
                          SELECT 1 FROM Reservas r
                          WHERE r.IdInmueble = i.IdInmueble
                            AND r.Estado = 'Vigente'
                            AND r.FechaInicio < @fechaFin
                            AND r.FechaFin > @fechaInicio
                      )
                    ORDER BY i.Direccion;";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fechaFin", fechaFin);
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearInmueble(reader));
                        }
                    }
                }
            }
            return lista;
        }
    }
}