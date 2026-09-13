using Microsoft.Data.SqlClient;
using InmobiliariaULP.Models;

namespace InmobiliariaULP.Repositories
{
    public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
    {
        public RepositorioUsuario(IConfiguration configuration) : base(configuration) { }

        public IList<Usuario> ObtenerTodos()
        {
            var lista = new List<Usuario>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT IdUsuario, Nombre, Apellido, Email, Clave, Rol, Avatar, Estado FROM Usuarios WHERE Estado = 1 ORDER BY Apellido, Nombre;";
                using (var command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Usuario
                            {
                                IdUsuario = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Email = reader.GetString(3),
                                Clave = reader.GetString(4),
                                Rol = reader.GetString(5),
                                Avatar = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Estado = reader.GetBoolean(7)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public Usuario? ObtenerPorId(int id)
        {
            Usuario? u = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT IdUsuario, Nombre, Apellido, Email, Clave, Rol, Avatar, Estado FROM Usuarios WHERE IdUsuario = @id;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            u = new Usuario
                            {
                                IdUsuario = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Email = reader.GetString(3),
                                Clave = reader.GetString(4),
                                Rol = reader.GetString(5),
                                Avatar = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Estado = reader.GetBoolean(7)
                            };
                        }
                    }
                }
            }
            return u;
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            Usuario? u = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT IdUsuario, Nombre, Apellido, Email, Clave, Rol, Avatar, Estado FROM Usuarios WHERE Email = @email AND Estado = 1;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            u = new Usuario
                            {
                                IdUsuario = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Email = reader.GetString(3),
                                Clave = reader.GetString(4),
                                Rol = reader.GetString(5),
                                Avatar = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Estado = reader.GetBoolean(7)
                            };
                        }
                    }
                }
            }
            return u;
        }

        public int Alta(Usuario u)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO Usuarios (Nombre, Apellido, Email, Clave, Rol, Avatar, Estado)
                    VALUES (@nombre, @apellido, @email, @clave, @rol, @avatar, @estado);
                    SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", u.Nombre);
                    command.Parameters.AddWithValue("@apellido", u.Apellido);
                    command.Parameters.AddWithValue("@email", u.Email);
                    command.Parameters.AddWithValue("@clave", u.Clave);
                    command.Parameters.AddWithValue("@rol", u.Rol);
                    command.Parameters.AddWithValue("@avatar", (object?)u.Avatar ?? DBNull.Value);
                    command.Parameters.AddWithValue("@estado", u.Estado);

                    connection.Open();
                    int nuevoId = Convert.ToInt32(command.ExecuteScalar());
                    u.IdUsuario = nuevoId;
                    return nuevoId;
                }
            }
        }

        public int Modificacion(Usuario u)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Usuarios
                    SET Nombre = @nombre, Apellido = @apellido, Email = @email, Rol = @rol, Estado = @estado
                    WHERE IdUsuario = @id;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", u.Nombre);
                    command.Parameters.AddWithValue("@apellido", u.Apellido);
                    command.Parameters.AddWithValue("@email", u.Email);
                    command.Parameters.AddWithValue("@rol", u.Rol);
                    command.Parameters.AddWithValue("@estado", u.Estado);
                    command.Parameters.AddWithValue("@id", u.IdUsuario);

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int ModificarPerfil(Usuario u)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Usuarios
                    SET Nombre = @nombre, Apellido = @apellido, Email = @email, Avatar = @avatar
                    WHERE IdUsuario = @id;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", u.Nombre);
                    command.Parameters.AddWithValue("@apellido", u.Apellido);
                    command.Parameters.AddWithValue("@email", u.Email);
                    command.Parameters.AddWithValue("@avatar", (object?)u.Avatar ?? DBNull.Value);
                    command.Parameters.AddWithValue("@id", u.IdUsuario);

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int ModificarClave(int id, string nuevaClave)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE Usuarios SET Clave = @clave WHERE IdUsuario = @id;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@clave", nuevaClave);
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int Baja(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE Usuarios SET Estado = 0 WHERE IdUsuario = @id;";
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