using InmobiliariaULP.Models;
using Microsoft.Data.SqlClient;

namespace InmobiliariaULP.Repositories
{
    public class RepositorioReserva : RepositorioBase, IRepositorioReserva
    {
        public RepositorioReserva(IConfiguration configuration) : base(configuration) { }

        public IList<Reserva> ObtenerTodos()
        {
            var lista = new List<Reserva>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT r.IdReserva, r.FechaInicio, r.FechaFin, r.FechaFinOriginal, r.FechaTerminacion, 
                           r.MontoDiario, r.Multa, r.Estado, r.IdInmueble, r.IdInquilino,
                           i.Direccion, inq.Nombre, inq.Apellido
                    FROM Reservas r
                    INNER JOIN Inmuebles i ON r.IdInmueble = i.IdInmueble
                    INNER JOIN Inquilinos inq ON r.IdInquilino = inq.IdInquilino
                    ORDER BY r.IdReserva DESC;";

                using (var command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Reserva
                            {
                                IdReserva = reader.GetInt32(0),
                                FechaInicio = reader.GetDateTime(1),
                                FechaFin = reader.GetDateTime(2),
                                FechaFinOriginal = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                                FechaTerminacion = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                                MontoDiario = reader.GetDecimal(5),
                                Multa = reader.GetDecimal(6),
                                Estado = reader.GetString(7),
                                IdInmueble = reader.GetInt32(8),
                                IdInquilino = reader.GetInt32(9),
                                Inmueble = new Inmueble { IdInmueble = reader.GetInt32(8), Direccion = reader.GetString(10) },
                                Inquilino = new Inquilino { IdInquilino = reader.GetInt32(9), Nombre = reader.GetString(11), Apellido = reader.GetString(12) }
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public Reserva? ObtenerPorId(int id)
        {
            Reserva? r = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT r.IdReserva, r.FechaInicio, r.FechaFin, r.FechaFinOriginal, r.FechaTerminacion, 
                           r.MontoDiario, r.Multa, r.Estado, r.IdInmueble, r.IdInquilino,
                           i.Direccion, inq.Nombre, inq.Apellido
                    FROM Reservas r
                    INNER JOIN Inmuebles i ON r.IdInmueble = i.IdInmueble
                    INNER JOIN Inquilinos inq ON r.IdInquilino = inq.IdInquilino
                    WHERE r.IdReserva = @id;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            r = new Reserva
                            {
                                IdReserva = reader.GetInt32(0),
                                FechaInicio = reader.GetDateTime(1),
                                FechaFin = reader.GetDateTime(2),
                                FechaFinOriginal = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                                FechaTerminacion = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                                MontoDiario = reader.GetDecimal(5),
                                Multa = reader.GetDecimal(6),
                                Estado = reader.GetString(7),
                                IdInmueble = reader.GetInt32(8),
                                IdInquilino = reader.GetInt32(9),
                                Inmueble = new Inmueble { IdInmueble = reader.GetInt32(8), Direccion = reader.GetString(10) },
                                Inquilino = new Inquilino { IdInquilino = reader.GetInt32(9), Nombre = reader.GetString(11), Apellido = reader.GetString(12) }
                            };
                        }
                    }
                }
            }
            return r;
        }

        public bool HaySolapamiento(int idInmueble, DateTime inicio, DateTime fin, int? idReservaActual = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT COUNT(*)
                    FROM Reservas
                    WHERE IdInmueble = @idInmueble
                      AND Estado != 'Anulada'
                      AND (@idReserva IS NULL OR IdReserva != @idReserva)
                      AND (FechaInicio < @fin AND FechaFin > @inicio);";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInmueble", idInmueble);
                    command.Parameters.AddWithValue("@inicio", inicio);
                    command.Parameters.AddWithValue("@fin", fin);
                    command.Parameters.AddWithValue("@idReserva", (object?)idReservaActual ?? DBNull.Value);

                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        public int Alta(Reserva r)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO Reservas (FechaInicio, FechaFin, FechaFinOriginal, FechaTerminacion, MontoDiario, Multa, Estado, IdInmueble, IdInquilino)
                    VALUES (@inicio, @fin, @finOrig, @term, @monto, @multa, @estado, @idInmueble, @idInquilino);
                    SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@inicio", r.FechaInicio);
                    command.Parameters.AddWithValue("@fin", r.FechaFin);
                    command.Parameters.AddWithValue("@finOrig", (object?)r.FechaFinOriginal ?? DBNull.Value);
                    command.Parameters.AddWithValue("@term", (object?)r.FechaTerminacion ?? DBNull.Value);
                    command.Parameters.AddWithValue("@monto", r.MontoDiario);
                    command.Parameters.AddWithValue("@multa", r.Multa);
                    command.Parameters.AddWithValue("@estado", r.Estado ?? "Vigente");
                    command.Parameters.AddWithValue("@idInmueble", r.IdInmueble);
                    command.Parameters.AddWithValue("@idInquilino", r.IdInquilino);

                    connection.Open();
                    int nuevoId = Convert.ToInt32(command.ExecuteScalar());
                    r.IdReserva = nuevoId;
                    return nuevoId;
                }
            }
        }

        public int Modificacion(Reserva r)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Reservas
                    SET FechaInicio = @inicio, FechaFin = @fin, FechaFinOriginal = @finOrig, FechaTerminacion = @term,
                        MontoDiario = @monto, Multa = @multa, Estado = @estado, IdInmueble = @idInmueble, IdInquilino = @idInquilino
                    WHERE IdReserva = @id;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@inicio", r.FechaInicio);
                    command.Parameters.AddWithValue("@fin", r.FechaFin);
                    command.Parameters.AddWithValue("@finOrig", (object?)r.FechaFinOriginal ?? DBNull.Value);
                    command.Parameters.AddWithValue("@term", (object?)r.FechaTerminacion ?? DBNull.Value);
                    command.Parameters.AddWithValue("@monto", r.MontoDiario);
                    command.Parameters.AddWithValue("@multa", r.Multa);
                    command.Parameters.AddWithValue("@estado", r.Estado);
                    command.Parameters.AddWithValue("@idInmueble", r.IdInmueble);
                    command.Parameters.AddWithValue("@idInquilino", r.IdInquilino);
                    command.Parameters.AddWithValue("@id", r.IdReserva);

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int Baja(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM Reservas WHERE IdReserva = @id;";
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
