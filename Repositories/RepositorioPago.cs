using InmobiliariaULP.Models;
using Microsoft.Data.SqlClient;

namespace InmobiliariaULP.Repositories
{
    public class RepositorioPago : RepositorioBase, IRepositorioPago
    {
        public RepositorioPago(IConfiguration configuration) : base(configuration) { }

        public IList<Pago> ObtenerTodos(int? idReserva = null, string? search = null)
        {
            var lista = new List<Pago>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT p.IdPago, p.Concepto, p.FechaPago, p.Importe, p.Anulado, p.IdReserva,
                           inq.Nombre, inq.Apellido, i.Direccion
                    FROM Pagos p
                    INNER JOIN Reservas r ON p.IdReserva = r.IdReserva
                    INNER JOIN Inquilinos inq ON r.IdInquilino = inq.IdInquilino
                    INNER JOIN Inmuebles i ON r.IdInmueble = i.IdInmueble
                    WHERE (@idReserva IS NULL OR p.IdReserva = @idReserva)
                      AND (@search IS NULL OR p.Concepto LIKE '%' + @search + '%' OR inq.Apellido LIKE '%' + @search + '%')
                    ORDER BY p.FechaPago DESC;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idReserva", (object?)idReserva ?? DBNull.Value);
                    command.Parameters.AddWithValue("@search", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Pago
                            {
                                IdPago = reader.GetInt32(0),
                                Concepto = reader.GetString(1),
                                FechaPago = reader.GetDateTime(2),
                                Importe = reader.GetDecimal(3),
                                Anulado = reader.GetBoolean(4),
                                IdReserva = reader.GetInt32(5),
                                Reserva = new Reserva
                                {
                                    IdReserva = reader.GetInt32(5),
                                    Inquilino = new Inquilino { Nombre = reader.GetString(6), Apellido = reader.GetString(7) },
                                    Inmueble = new Inmueble { Direccion = reader.GetString(8) }
                                }
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public IList<Pago> ObtenerPorReserva(int idReserva)
        {
            return ObtenerTodos(idReserva, null);
        }

        public Pago? ObtenerPorId(int id)
        {
            Pago? pago = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT p.IdPago, p.Concepto, p.FechaPago, p.Importe, p.Anulado, p.IdReserva,
                           inq.Nombre, inq.Apellido, i.Direccion
                    FROM Pagos p
                    INNER JOIN Reservas r ON p.IdReserva = r.IdReserva
                    INNER JOIN Inquilinos inq ON r.IdInquilino = inq.IdInquilino
                    INNER JOIN Inmuebles i ON r.IdInmueble = i.IdInmueble
                    WHERE p.IdPago = @id;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pago = new Pago
                            {
                                IdPago = reader.GetInt32(0),
                                Concepto = reader.GetString(1),
                                FechaPago = reader.GetDateTime(2),
                                Importe = reader.GetDecimal(3),
                                Anulado = reader.GetBoolean(4),
                                IdReserva = reader.GetInt32(5),
                                Reserva = new Reserva
                                {
                                    IdReserva = reader.GetInt32(5),
                                    Inquilino = new Inquilino { Nombre = reader.GetString(6), Apellido = reader.GetString(7) },
                                    Inmueble = new Inmueble { Direccion = reader.GetString(8) }
                                }
                            };
                        }
                    }
                }
            }
            return pago;
        }

        public int Alta(Pago pago)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO Pagos (Concepto, FechaPago, Importe, Anulado, IdReserva)
                    VALUES (@concepto, @fecha, @importe, @anulado, @idReserva);
                    SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@concepto", pago.Concepto);
                    command.Parameters.AddWithValue("@fecha", pago.FechaPago);
                    command.Parameters.AddWithValue("@importe", pago.Importe);
                    command.Parameters.AddWithValue("@anulado", false);
                    command.Parameters.AddWithValue("@idReserva", pago.IdReserva);

                    connection.Open();
                    int nuevoId = Convert.ToInt32(command.ExecuteScalar());
                    pago.IdPago = nuevoId;
                    return nuevoId;
                }
            }
        }

        public int ModificarConcepto(int id, string concepto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE Pagos SET Concepto = @concepto WHERE IdPago = @id AND Anulado = 0;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@concepto", concepto);
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int Anular(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE Pagos SET Anulado = 1 WHERE IdPago = @id;";
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
