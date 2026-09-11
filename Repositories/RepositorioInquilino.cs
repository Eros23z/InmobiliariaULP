using InmobiliariaULP.Models;
using Microsoft.Data.SqlClient;

namespace InmobiliariaULP.Repositories
{
    public class RepositorioInquilino : RepositorioBase, IRepositorioInquilino
    {
        public RepositorioInquilino(IConfiguration configuration) : base(configuration) { }

        public IList<Inquilino> ObtenerTodos(string? search = null, int page = 1, int pageSize = 10)
        {
            var lista = new List<Inquilino>();
            int offset = (page - 1) * pageSize;

            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT IdInquilino, Dni, Nombre, Apellido, Telefono, Email, Estado
                    FROM Inquilinos
                    WHERE (@search IS NULL OR Dni LIKE '%' + @search + '%' OR Apellido LIKE '%' + @search + '%')
                    ORDER BY Apellido, Nombre
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@search", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search);
                    command.Parameters.AddWithValue("@offset", offset);
                    command.Parameters.AddWithValue("@pageSize", pageSize);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Inquilino
                            {
                                IdInquilino = reader.GetInt32(0),
                                Dni = reader.GetString(1),
                                Nombre = reader.GetString(2),
                                Apellido = reader.GetString(3),
                                Telefono = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Estado = reader.GetBoolean(6)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public int Contar(string? search = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT COUNT(*) FROM Inquilinos WHERE (@search IS NULL OR Dni LIKE '%' + @search + '%' OR Apellido LIKE '%' + @search + '%');";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@search", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search);
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public Inquilino? ObtenerPorId(int id)
        {
            Inquilino? inq = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT IdInquilino, Dni, Nombre, Apellido, Telefono, Email, Estado FROM Inquilinos WHERE IdInquilino = @id;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            inq = new Inquilino
                            {
                                IdInquilino = reader.GetInt32(0),
                                Dni = reader.GetString(1),
                                Nombre = reader.GetString(2),
                                Apellido = reader.GetString(3),
                                Telefono = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Estado = reader.GetBoolean(6)
                            };
                        }
                    }
                }
            }
            return inq;
        }

        public int Alta(Inquilino inq)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Inquilinos (Dni, Nombre, Apellido, Telefono, Email, Estado)
                               VALUES (@dni, @nombre, @apellido, @telefono, @email, @estado);
                               SELECT SCOPE_IDENTITY();";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", inq.Dni);
                    command.Parameters.AddWithValue("@nombre", inq.Nombre);
                    command.Parameters.AddWithValue("@apellido", inq.Apellido);
                    command.Parameters.AddWithValue("@telefono", (object?)inq.Telefono ?? DBNull.Value);
                    command.Parameters.AddWithValue("@email", (object?)inq.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@estado", inq.Estado);

                    connection.Open();
                    int nuevoId = Convert.ToInt32(command.ExecuteScalar());
                    inq.IdInquilino = nuevoId;
                    return nuevoId;
                }
            }
        }

        public int Modificacion(Inquilino inq)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE Inquilinos SET Dni = @dni, Nombre = @nombre, Apellido = @apellido, Telefono = @telefono, Email = @email, Estado = @estado WHERE IdInquilino = @id;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", inq.Dni);
                    command.Parameters.AddWithValue("@nombre", inq.Nombre);
                    command.Parameters.AddWithValue("@apellido", inq.Apellido);
                    command.Parameters.AddWithValue("@telefono", (object?)inq.Telefono ?? DBNull.Value);
                    command.Parameters.AddWithValue("@email", (object?)inq.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@estado", inq.Estado);
                    command.Parameters.AddWithValue("@id", inq.IdInquilino);

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int Baja(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM Inquilinos WHERE IdInquilino = @id;";
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
