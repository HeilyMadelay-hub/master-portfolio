//using Microsoft.EntityFrameworkCore;
//using System.Collections.Generic;
//using System.Reflection.Emit;
//using WebMvc.Models;

//namespace WebMvc.Data
//{
//    public class AppDbContext : DbContext
//    {
//      
//        Esto esta bien pero las propiedad automaticas son mejores porque son mas compatibles con migraciones y mas facil 
//        de remplazar porque no estas limitado a la estructura del dbcontext

//        public DbSet<Dispositivo> Dispositivos => Set<Dispositivo>();
//        public DbSet<Reserva> Reservas => Set<Reserva>(); 

//        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
//        {
//            Database.EnsureCreated();

//            Funciona para crear la base de datos y las tablas si no existen
//            Pero es mejor dejarlo vacio porque no soporta migraciones ni actualizaciones
//            Es mejor usar las migraciones de toda la vida en el program

//        }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
// falta relaciones para las migraciones ,la semilla para rellenar la base de datos y el base.OnModelCreating(modelBuilder); para realizar las configuraciones internas principales 

//        }
//    }
//}

using Ejercicio1.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;


namespace Ejercicio1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Dispositivo> Dispositivos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Relaciones y configuraciones principales

            // Un Usuario tiene muchas Reservas (1:N)
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Reservas)
                .WithOne(r => r.Usuario)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade); // Si se elimina un usuario, se eliminan sus reservas

            // Un Dispositivo tiene muchas Reservas (1:N)
            modelBuilder.Entity<Dispositivo>()
                .HasMany(d => d.Reservas)
                .WithOne(r => r.Dispositivo)
                .HasForeignKey(r => r.DispositivoId)
                .OnDelete(DeleteBehavior.Cascade); // Si se elimina un dispositivo, se eliminan sus reservas


            //Configuraciones extras

            // Se pueden agregar constraints adicionales, como índices únicos
            modelBuilder.Entity<Dispositivo>()
                .HasIndex(d => d.Nombre)
                .IsUnique(); // Evita que haya dos dispositivos con el mismo nombre


            // Limitar longitud máxima y obligar campos requeridos
            modelBuilder.Entity<Dispositivo>()
                .Property(d => d.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Usuario>()
                .Property(u => u.NombreCompleto)
                .HasMaxLength(50)
                .IsRequired();

            //Para las fechas de las reservas,
            //ya que no se puede poner que obtenga la fecha ahora con datetime.now por
            //limitaciones de HasData y las migraciones estaticas
            //asi se actualiza cada dia en la bd automaticamente al asignar fechas o insertar reservas en tiempo de ejecucion
            modelBuilder.Entity<Reserva>()
            .Property(r => r.FechaInicio)
            .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Reserva>()
                .Property(r => r.FechaFin)
                .HasDefaultValueSql("DATEADD(DAY, 1, GETDATE())");


            //Semilla

            // Usuarios
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    UsuarioId = 1,
                    NombreCompleto = "Juan Garcia"
                },
                new Usuario
                {
                    UsuarioId = 2,
                    NombreCompleto = "Maria Lopez"
                },
                new Usuario
                {
                    UsuarioId = 3,
                    NombreCompleto = "Pedro Martinez"
                },
                new Usuario
                {
                    UsuarioId = 4,
                    NombreCompleto = "Ana Rodriguez"
                }
            );

            // Dispositivos
            modelBuilder.Entity<Dispositivo>().HasData(
                new Dispositivo
                {
                    DispositivoId = 1,
                    Nombre = "Laptop HP"
                },
                new Dispositivo
                {
                    DispositivoId = 2,
                    Nombre = "Monitor Samsung"
                },
                new Dispositivo
                {
                    DispositivoId = 3,
                    Nombre = "Teclado Logitech"
                },
                new Dispositivo
                {
                    DispositivoId = 4,
                    Nombre = "Mouse Razer"
                },
                new Dispositivo
                {
                    DispositivoId = 5,
                    Nombre = "Tablet iPad"
                }
            );

            var now = new DateTime(2025, 12, 16);//valor fijo para que se aplique sin errores y la base tenga datos coherentes de prueba

            // Reservas
            modelBuilder.Entity<Reserva>().HasData(
                // Reserva activa - Juan tiene la Laptop
                new Reserva
                {
                    ReservaId = 1,
                    DispositivoId = 1,
                    UsuarioId = 1,
                    FechaInicio = DateTime.Now.AddDays(-2),
                    FechaFin = DateTime.Now.AddDays(5)
                },
                // Reserva pasada - Maria tuvo el Monitor
                new Reserva
                {
                    ReservaId = 2,
                    DispositivoId = 2,
                    UsuarioId = 2,
                    FechaInicio = DateTime.Now.AddDays(-10),
                    FechaFin = DateTime.Now.AddDays(-3)
                },
                // Reserva futura - Pedro reservó el Teclado
                new Reserva
                {
                    ReservaId = 3,
                    DispositivoId = 3,
                    UsuarioId = 3,
                    FechaInicio = DateTime.Now.AddDays(3),
                    FechaFin = DateTime.Now.AddDays(7)
                },
                // Reserva activa - Ana tiene el Mouse
                new Reserva
                {
                    ReservaId = 4,
                    DispositivoId = 4,
                    UsuarioId = 4,
                    FechaInicio = DateTime.Now.AddDays(-1),
                    FechaFin = DateTime.Now.AddDays(2)
                }
            );


        }

    }
}