using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;//biblioteca para importar las funcionalidades de Entity Framework Core y asi poder usar el dbcontext para la base de datos
using Ejercicio2.Models;//los modelos de datos en los que se basa la api 

namespace Ejercicio2.Data
{
    public class AppDbContext : DbContext
    {
        //public DbSet<Producto> Productos => Set<Producto>();
        //public DbSet<OrdenDetalle> Ordenes => Set<OrdenDetalle>();
        //No esta mal pero es mas rigido porque no puedes asignar otra instancia al DbSet y no puedes inicializarla manualmente. Solo sirve para usarla dentro del DbContext.



        public DbSet<Producto> Productos { get; set; }
        public DbSet<Orden> Ordenes { get; set; }
        public DbSet<OrdenDetalle> OrdenesDetalle { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
          //  Database.EnsureCreated(); no soporta actualizaciones por eso es mejor las migraciones porque soportan acctualizaciones
        }

      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //base on model creating para configuraciones iniciales con relaciones
            base.OnModelCreating(modelBuilder);

          
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Nombre)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.Precio)
                      .IsRequired()
                      .HasPrecision(18, 2);

                entity.Property(p => p.CantidadDisponible)
                      .IsRequired();

                // Relación uno a muchos con OrdenDetalle
                entity.HasMany(p => p.OrdenesDetalle)
                      .WithOne(od => od.Producto)
                      .HasForeignKey(od => od.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict); // evita borrar producto con ordenes
            });

      
            modelBuilder.Entity<Orden>(entity =>
            {
                entity.HasKey(o => o.OrdenId);

                entity.Property(o => o.Fecha)
                      .IsRequired();

                // Relación uno a muchos con OrdenDetalle
                entity.HasMany(o => o.Detalles)
                      .WithOne(od => od.Orden)
                      .HasForeignKey(od => od.OrdenId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

      
            modelBuilder.Entity<OrdenDetalle>(entity =>
            {
                entity.HasKey(od => od.OrdenDetalleId);

                entity.Property(od => od.Cantidad)
                      .IsRequired();

               
            });

            //semillas
            modelBuilder.Entity<Producto>().HasData(
                new Producto { Id = 1, Nombre = "Laptop", CantidadDisponible = 10, Precio = 1500M },
                new Producto { Id = 2, Nombre = "Monitor", CantidadDisponible = 25, Precio = 300M },
                new Producto { Id = 3, Nombre = "Teclado", CantidadDisponible = 50, Precio = 25M }
            );

            modelBuilder.Entity<Orden>().HasData(
                new Orden { OrdenId = 1, Fecha = DateTime.Now.AddDays(-1) },
                new Orden { OrdenId = 2, Fecha = DateTime.Now }
            );

            modelBuilder.Entity<OrdenDetalle>().HasData(
                new OrdenDetalle { OrdenDetalleId = 1, OrdenId = 1, ProductoId = 1, Cantidad = 2 },
                new OrdenDetalle { OrdenDetalleId = 2, OrdenId = 1, ProductoId = 3, Cantidad = 5 },
                new OrdenDetalle { OrdenDetalleId = 3, OrdenId = 2, ProductoId = 2, Cantidad = 1 }
            );
        }
    }
}
