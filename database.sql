-- Creación de la base de datos
CREATE DATABASE InmobiliariaDB;
GO

USE InmobiliariaDB;
GO

-- Tabla: Propietarios
CREATE TABLE Propietarios (
    IdPropietario INT IDENTITY(1,1) PRIMARY KEY,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    Apellido VARCHAR(50) NOT NULL,
    Telefono VARCHAR(30) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Estado BIT NOT NULL DEFAULT 1
);
GO

-- Tabla: Inquilinos
CREATE TABLE Inquilinos (
    IdInquilino INT IDENTITY(1,1) PRIMARY KEY,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(50) NOT NULL,
    Apellido VARCHAR(50) NOT NULL,
    Telefono VARCHAR(30) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Estado BIT NOT NULL DEFAULT 1
);
GO

-- Datos semilla para pruebas iniciales
INSERT INTO Propietarios (Dni, Nombre, Apellido, Telefono, Email, Estado)
VALUES 
('30111222', 'Carlos', 'Gómez', '2664112233', 'carlos.gomez@gmail.com', 1),
('32444555', 'Mariana', 'López', '2664445566', 'mariana.lopez@hotmail.com', 1),
('28999888', 'Esteban', 'Pérez', '2664778899', 'esteban.perez@yahoo.com', 1);

INSERT INTO Inquilinos (Dni, Nombre, Apellido, Telefono, Email, Estado)
VALUES 
('38123456', 'Lucía', 'Fernández', '2665123456', 'lucia.fernandez@gmail.com', 1),
('40987654', 'Martín', 'Díaz', '2665987654', 'martin.diaz@gmail.com', 1);
GO