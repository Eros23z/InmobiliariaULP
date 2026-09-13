using System.Data;
using Microsoft.Data.SqlClient;
using InmobiliariaULP.Models;

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
                           inq.Nombre, inq.Apellido, i.Direccion,
                           p.UsuarioCreaId, p.UsuarioAnulaId
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
                                UsuarioCreaId = reader.GetInt32(9),
                                UsuarioAnulaId = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10),
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
                   inq.Nombre, inq.Apellido, i.Direccion,
                   p.UsuarioCreaId, uc.Nombre, uc.Apellido, uc.Email,
                   p.UsuarioAnulaId, ua.Nombre, ua.Apellido, ua.Email
            FROM Pagos p
            INNER JOIN Reservas r ON p.IdReserva = r.IdReserva
            INNER JOIN Inquilinos inq ON r.IdInquilino = inq.IdInquilino
            INNER JOIN Inmuebles i ON r.IdInmueble = i.IdInmueble
            LEFT JOIN Usuarios uc ON p.UsuarioCreaId = uc.IdUsuario
            LEFT JOIN Usuarios ua ON p.UsuarioAnulaId = ua.IdUsuario
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
                                    Inquilino = new Inquilino
                                    {
                                        Nombre = reader.GetString(6),
                                        Apellido = reader.GetString(7)
                                    },
                                    Inmueble = new Inmueble
                                    {
                                        Direccion = reader.GetString(8)
                                    }
                                },
                                UsuarioCreaId = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                                UsuarioCrea = reader.IsDBNull(9) || reader.IsDBNull(10) ? null : new Usuario
                                {
                                    IdUsuario = reader.GetInt32(9),
                                    Nombre = reader.GetString(10),
                                    Apellido = reader.GetString(11),
                                    Email = reader.GetString(12)
                                },
                                UsuarioAnulaId = reader.IsDBNull(13) ? (int?)null : reader.GetInt32(13),
                                UsuarioAnula = reader.IsDBNull(13) || reader.IsDBNull(14) ? null : new Usuario
                                {
                                    IdUsuario = reader.GetInt32(13),
                                    Nombre = reader.GetString(14),
                                    Apellido = reader.GetString(15),
                                    Email = reader.GetString(16)
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
                    INSERT INTO Pagos (Concepto, FechaPago, Importe, Anulado, IdReserva, UsuarioCreaId)
                    VALUES (@concepto, @fecha, @importe, 0, @idReserva, @usuarioCreaId);
                    SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@concepto", pago.Concepto);
                    command.Parameters.AddWithValue("@fecha", pago.FechaPago);
                    command.Parameters.AddWithValue("@importe", pago.Importe);
                    command.Parameters.AddWithValue("@idReserva", pago.IdReserva);
                    command.Parameters.AddWithValue("@usuarioCreaId", pago.UsuarioCreaId > 0 ? pago.UsuarioCreaId : 1);

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

        public int Anular(int id, int usuarioAnulaId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE Pagos SET Anulado = 1, UsuarioAnulaId = @usuarioAnulaId WHERE IdPago = @id;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@usuarioAnulaId", usuarioAnulaId > 0 ? usuarioAnulaId : 1);
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }
    }
}