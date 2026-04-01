using Microsoft.EntityFrameworkCore;
using TrackingApi.Domain.Entities;

namespace TrackingApi.Infrastructure.Data;

public sealed class TrackingDbContext : DbContext
{
    public TrackingDbContext(DbContextOptions<TrackingDbContext> options)
        : base(options)
    {
    }

    public DbSet<GesEcoOrdersTrackingEntity> TrackingOrders => Set<GesEcoOrdersTrackingEntity>();
    public DbSet<GesEcoOrderEntity> Orders => Set<GesEcoOrderEntity>();
    public DbSet<GesParametroEntity> Parameters => Set<GesParametroEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GesEcoOrdersTrackingEntity>(entity =>
        {
            entity.ToTable("Ges_EcoOrdersTracking");
            entity.HasKey(x => x.IdTracking);

            entity.Property(x => x.IdTracking).HasColumnName("Id_Tracking");
            entity.Property(x => x.CodPedido).HasColumnName("Cod_Pedido");
            entity.Property(x => x.IdDelivery).HasColumnName("Id_Delivery");
            entity.Property(x => x.Tipo).HasColumnName("Tipo");
            entity.Property(x => x.NombreArchivo).HasColumnName("NombreArchivo");
            entity.Property(x => x.CodigoZebra).HasColumnName("CodigoZebra");
            entity.Property(x => x.ShippingStatus).HasColumnName("shipping_status");
        });

        modelBuilder.Entity<GesEcoOrderEntity>(entity =>
        {
            entity.ToTable("Ges_EcoOrders");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.CodPedido).HasColumnName("Cod_Pedido");
            entity.Property(x => x.ShippingStatus).HasColumnName("shipping_status");
            entity.Property(x => x.IdDelivery).HasColumnName("Id_Delivery");
        });

        modelBuilder.Entity<GesParametroEntity>(entity =>
        {
            entity.ToTable("Ges_Parametros");
            entity.HasKey(x => x.CodParametro);

            entity.Property(x => x.CodParametro).HasColumnName("Cod_Parametro");
            entity.Property(x => x.Tipo).HasColumnName("Tipo");
            entity.Property(x => x.Descripcion).HasColumnName("Descripcion");
            entity.Property(x => x.Estado).HasColumnName("Estado");
        });
    }
}
