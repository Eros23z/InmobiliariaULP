CREATE DATABASE InmobiliariaDB;
GO

USE InmobiliariaDB;
GO

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

CREATE TABLE TiposInmueble (
    IdTipoInmueble INT IDENTITY(1,1) PRIMARY KEY,
    Descripcion VARCHAR(50) NOT NULL
);
GO

CREATE TABLE Inmuebles (
    IdInmueble INT IDENTITY(1,1) PRIMARY KEY,
    Direccion VARCHAR(150) NOT NULL,
    Cupo INT NOT NULL,
    Latitud DECIMAL(10, 8) NOT NULL,
    Longitud DECIMAL(11, 8) NOT NULL,
    PrecioPorDia DECIMAL(12, 2) NOT NULL,
    PorcentajeReserva DECIMAL(5, 2) NOT NULL DEFAULT 100.00,
    Disponible BIT NOT NULL DEFAULT 1,
    ImagenPortada VARCHAR(255) NULL,
    IdPropietario INT NOT NULL,
    IdTipoInmueble INT NOT NULL,
    CONSTRAINT FK_Inmuebles_Propietarios FOREIGN KEY (IdPropietario) REFERENCES Propietarios(IdPropietario),
    CONSTRAINT FK_Inmuebles_TiposInmueble FOREIGN KEY (IdTipoInmueble) REFERENCES TiposInmueble(IdTipoInmueble)
);
GO

CREATE TABLE Reservas (
    IdReserva INT IDENTITY(1,1) PRIMARY KEY,
    FechaInicio DATE NOT NULL,
    FechaFin DATE NOT NULL,
    FechaFinOriginal DATE NOT NULL,
    FechaTerminacion DATE NULL,
    MontoDiario DECIMAL(12, 2) NOT NULL,
    Multa DECIMAL(12, 2) NOT NULL DEFAULT 0.00,
    Estado VARCHAR(30) NOT NULL DEFAULT 'Vigente',
    IdInmueble INT NOT NULL,
    IdInquilino INT NOT NULL,
    CONSTRAINT FK_Reservas_Inmuebles FOREIGN KEY (IdInmueble) REFERENCES Inmuebles(IdInmueble),
    CONSTRAINT FK_Reservas_Inquilinos FOREIGN KEY (IdInquilino) REFERENCES Inquilinos(IdInquilino)
);
GO

-- Datos semilla 
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

INSERT INTO TiposInmueble (Descripcion) 
VALUES 
('Casa'), ('Departamento'), ('Monoambiente'), ('Cabaña');

INSERT INTO Inmuebles (Direccion, Cupo, Latitud, Longitud, PrecioPorDia, PorcentajeReserva, Disponible, ImagenPortada, IdPropietario, IdTipoInmueble)
VALUES 
('Av. del Sol 1234', 4, -32.341200, -65.012300, 45000.00, 30.00, 1, '/img/casa1.jpg', 1, 1),
('Calle Los Almendros 56', 2, -32.345600, -65.018900, 28000.00, 50.00, 1, '/img/depto1.jpg', 2, 2);
GO