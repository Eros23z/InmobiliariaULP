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
                                i.Direccion, inq.Nombre, inq.Apellido,
                                r.UsuarioCreaId, uc.Nombre, uc.Apellido, uc.Email,
                                r.UsuarioTerminaId, ut.Nombre, ut.Apellido, ut.Email
                        FROM Reservas r
                        INNER JOIN Inmuebles i ON r.IdInmueble = i.IdInmueble
                        INNER JOIN Inquilinos inq ON r.IdInquilino = inq.IdInquilino
                        LEFT JOIN Usuarios uc ON r.UsuarioCreaId = uc.IdUsuario
                        LEFT JOIN Usuarios ut ON r.UsuarioTerminaId = ut.IdUsuario
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
                        INSERT INTO Reservas (FechaInicio, FechaFin, FechaFinOriginal, FechaTerminacion, MontoDiario, Multa, Estado, IdInmueble, IdInquilino, UsuarioCreaId)
                        VALUES (@inicio, @fin, @finOrig, @term, @monto, @multa, @estado, @idInmueble, @idInquilino, @usuarioCreaId);
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
                    command.Parameters.AddWithValue("@usuarioCreaId", r.UsuarioCreaId > 0 ? r.UsuarioCreaId : 1);

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

        public int FinalizarConMulta(int idReserva, DateTime fechaTerminacion, decimal multa, int usuarioId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Actualizar el estado, fecha efectiva de corte y multa de la reserva
                        string sqlReserva = @"
                                        UPDATE Reservas
                                        SET FechaTerminacion = @fechaTerminacion,
                                            Multa = @multa,
                                            Estado = 'Finalizada',
                                            UsuarioTerminaId = @usuarioTerminaId
                                        WHERE IdReserva = @idReserva;";
                        using (var cmdReserva = new SqlCommand(sqlReserva, connection, transaction))
                        {
                            cmdReserva.Parameters.AddWithValue("@fechaTerminacion", fechaTerminacion);
                            cmdReserva.Parameters.AddWithValue("@multa", multa);
                            cmdReserva.Parameters.AddWithValue("@idReserva", idReserva);
                            cmdReserva.ExecuteNonQuery();
                        }

                        // Impactar el cobro de la multa en Pagos si el importe es mayor a 0
                        if (multa > 0)
                        {
                            string sqlPago = @"
                        INSERT INTO Pagos (Concepto, FechaPago, Importe, Anulado, IdReserva, UsuarioCreaId)
                        VALUES (@concepto, @fechaPago, @importe, 0, @idReserva, @usuarioCreaId);";

                            using (var cmdPago = new SqlCommand(sqlPago, connection, transaction))
                            {
                                cmdPago.Parameters.AddWithValue("@concepto", $"Multa por rescisión anticipada de reserva #{idReserva}");
                                cmdPago.Parameters.AddWithValue("@fechaPago", fechaTerminacion);
                                cmdPago.Parameters.AddWithValue("@importe", multa);
                                cmdPago.Parameters.AddWithValue("@idReserva", idReserva);
                                cmdPago.Parameters.AddWithValue("@usuarioCreaId", usuarioId > 0 ? usuarioId : 1);
                                cmdPago.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return 1;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
