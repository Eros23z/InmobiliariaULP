using System.Data;
using Microsoft.Data.SqlClient;
using InmobiliariaULP.Models;

namespace InmobiliariaULP.Repositories
{
    public class RepositorioPropietario : RepositorioBase, IRepositorioPropietario
    {
        public RepositorioPropietario(IConfiguration configuration) : base(configuration) { }

        public IList<Propietario> ObtenerTodos(string? search = null, int page = 1, int pageSize = 10)
        {
            var lista = new List<Propietario>();
            int offset = (page - 1) * pageSize;

            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT IdPropietario, Dni, Nombre, Apellido, Telefono, Email, Estado
                    FROM Propietarios
                    WHERE (@search IS NULL OR Dni LIKE '%' + @search + '%' OR Apellido LIKE '%' + @search + '%' OR Nombre LIKE '%' + @search + '%')
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
                            lista.Add(new Propietario
                            {
                                IdPropietario = reader.GetInt32(0),
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
                string sql = @"
                    SELECT COUNT(*) 
                    FROM Propietarios
                    WHERE (@search IS NULL OR Dni LIKE '%' + @search + '%' OR Apellido LIKE '%' + @search + '%' OR Nombre LIKE '%' + @search + '%');";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@search", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search);
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public Propietario? ObtenerPorId(int id)
        {
            Propietario? p = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT IdPropietario, Dni, Nombre, Apellido, Telefono, Email, Estado
                    FROM Propietarios
                    WHERE IdPropietario = @id;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            p = new Propietario
                            {
                                IdPropietario = reader.GetInt32(0),
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
            return p;
        }

        public int Alta(Propietario p)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO Propietarios (Dni, Nombre, Apellido, Telefono, Email, Estado)
                    VALUES (@dni, @nombre, @apellido, @telefono, @email, @estado);
                    SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", p.Dni);
                    command.Parameters.AddWithValue("@nombre", p.Nombre);
                    command.Parameters.AddWithValue("@apellido", p.Apellido);
                    command.Parameters.AddWithValue("@telefono", (object?)p.Telefono ?? DBNull.Value);
                    command.Parameters.AddWithValue("@email", (object?)p.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@estado", p.Estado);

                    connection.Open();
                    int nuevoId = Convert.ToInt32(command.ExecuteScalar());
                    p.IdPropietario = nuevoId;
                    return nuevoId;
                }
            }
        }

        public int Modificacion(Propietario p)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Propietarios
                    SET Dni = @dni, Nombre = @nombre, Apellido = @apellido, Telefono = @telefono, Email = @email, Estado = @estado
                    WHERE IdPropietario = @id;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", p.Dni);
                    command.Parameters.AddWithValue("@nombre", p.Nombre);
                    command.Parameters.AddWithValue("@apellido", p.Apellido);
                    command.Parameters.AddWithValue("@telefono", (object?)p.Telefono ?? DBNull.Value);
                    command.Parameters.AddWithValue("@email", (object?)p.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@estado", p.Estado);
                    command.Parameters.AddWithValue("@id", p.IdPropietario);

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int Baja(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM Propietarios WHERE IdPropietario = @id;";

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