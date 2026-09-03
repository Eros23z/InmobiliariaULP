# Sistema de Gestión Inmobiliaria - Reservas Temporales

> Aplicación web desarrollada en ASP.NET Core MVC para la informatización y control de alquileres temporarios, inmuebles, propietarios, inquilinos y cobros de la agencia inmobiliaria.

---

## 👥 Integrantes del Grupo

* **Nombre y Apellido** - *ezamora@ulp.edu.ar* - [@Eros23z](https://github.com/Eros23z) - Discord: `abyss23z`

---

## 📐 Modelado de Datos

A continuación se presenta el esquema relacional del modelo de datos de la aplicación:

### Diagrama Entidad-Relación (DER)

<details open>
<summary>Ver diagrama en código Mermaid</summary>

```mermaid
erDiagram
    PROPIETARIOS {
        int IdPropietario PK
        string Dni UK
        string Nombre
        string Apellido
        string Telefono
        string Email
        bit Estado
    }

    INQUILINOS {
        int IdInquilino PK
        string Dni UK
        string Nombre
        string Apellido
        string Telefono
        string Email
        bit Estado
    }

    TIPOS_INMUEBLE {
        int IdTipoInmueble PK
        string Descripcion
    }

    INMUEBLES {
        int IdInmueble PK
        string Direccion
        int Cupo
        decimal Latitud
        decimal Longitud
        decimal PrecioPorDia
        decimal PorcentajeReserva
        bit Disponible
        string ImagenPortada
        int IdPropietario FK
        int IdTipoInmueble FK
    }

    RESERVAS {
        int IdReserva PK
        date FechaInicio
        date FechaFin
        date FechaFinOriginal
        date FechaTerminacion
        decimal MontoDiario
        decimal Multa
        string Estado
        int IdInmueble FK
        int IdInquilino FK
    }

    PROPIETARIOS ||--o{ INMUEBLES : posee
    TIPOS_INMUEBLE ||--o{ INMUEBLES : clasifica
    INMUEBLES ||--o{ RESERVAS : alquila
    INQUILINOS ||--o{ RESERVAS : solicita
```
</details>

---

## 🚀 Instrucciones para Levantar la Base de Datos

1. Abrir **SQL Server Management Studio (SSMS)** o la ventana de **SQL Server Object Explorer** en Visual Studio.
2. Conectarse a la instancia local (`(localdb)\mssqllocaldb` o `localhost`).
3. Abrir el archivo `database.sql` ubicado en la raíz del proyecto.
4. Ejecutar el script completo (`F5` o botón **Execute**).
5. Verificar que se hayan creado la base de datos `InmobiliariaDB` y las tablas con sus datos iniciales.
6. Ajustar la cadena de conexión en el archivo `appsettings.json` si su instancia local utiliza credenciales distintas.