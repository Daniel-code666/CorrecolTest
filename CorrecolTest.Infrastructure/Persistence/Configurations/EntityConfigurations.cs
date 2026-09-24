using CorrecolTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorrecolTest.Infrastructure.Persistence.Configurations;

internal static class AuditConfiguration
{
    public static void ConfigureAudit<T>(this EntityTypeBuilder<T> builder) where T : AuditTable
    {
        builder.Property(x => x.CreationDate).HasColumnType("datetime").HasDefaultValueSql("GETUTCDATE()")
            .HasConversion(x => x, x => DateTime.SpecifyKind(x, DateTimeKind.Utc));
        builder.Property(x => x.UpdatedDate).HasColumnType("datetime")
            .HasConversion(x => x, x => x.HasValue ? DateTime.SpecifyKind(x.Value, DateTimeKind.Utc) : x);
    }
}

public class PaisConfiguration : IEntityTypeConfiguration<Pais>
{
    public void Configure(EntityTypeBuilder<Pais> b)
    {
        b.ToTable("Pais");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasColumnName("PaisCodigo").ValueGeneratedNever();
        b.Property(x => x.Nombre).HasColumnName("PaisNombre").HasMaxLength(100).IsUnicode(false);
        b.Property(x => x.Iso1).HasColumnName("PaisIso1").HasMaxLength(5).IsUnicode(false);
        b.Property(x => x.Iso2).HasColumnName("PaisIso2").HasMaxLength(3).IsUnicode(false);
        b.Property(x => x.Capital).HasColumnName("PaisCapital").HasMaxLength(100).IsUnicode(false);
        b.HasIndex(x => x.Nombre).IsUnique();
        b.ConfigureAudit();
    }
}

public class DepartamentoConfiguration : IEntityTypeConfiguration<Departamento>
{
    public void Configure(EntityTypeBuilder<Departamento> b)
    {
        b.ToTable("DepartamentosColombia");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasColumnName("DptColCodigoDane").ValueGeneratedNever();
        b.Property(x => x.Nombre).HasColumnName("DptColNombredelDepartamento").HasMaxLength(100).IsUnicode(false);
        b.Property(x => x.PaisCodigo).HasColumnName("DptColPaisCodigo");
        b.HasOne(x => x.Pais).WithMany().HasForeignKey(x => x.PaisCodigo).OnDelete(DeleteBehavior.Restrict);
        b.HasAlternateKey(x => new { x.Codigo, x.PaisCodigo });
        b.HasIndex(x => x.Nombre).IsUnique();
        b.ConfigureAudit();
    }
}

public class CiudadConfiguration : IEntityTypeConfiguration<Ciudad>
{
    public void Configure(EntityTypeBuilder<Ciudad> b)
    {
        b.ToTable("DivisionPoliticaColombia");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasColumnName("DvsPltColCodigoDane").ValueGeneratedNever();
        b.Property(x => x.Nombre).HasColumnName("DvsPltColNombreMunicipio").HasMaxLength(100).IsUnicode(false);
        b.Property(x => x.DepartamentoCodigo).HasColumnName("DvsPltColDptColCodigoDane");
        b.HasOne(x => x.Departamento).WithMany().HasForeignKey(x => x.DepartamentoCodigo).OnDelete(DeleteBehavior.Restrict);
        b.HasAlternateKey(x => new { x.Codigo, x.DepartamentoCodigo });
        b.ConfigureAudit();
    }
}

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("Cliente", table =>
        {
            table.HasCheckConstraint("CK_Cliente_CiudadDepartamento", "[ClnDvsPltColCodigoDane] IS NULL OR [ClnDptColCodigoDane] IS NOT NULL");
            table.HasCheckConstraint("CK_Cliente_TipoIdentificacion", "[ClnTpoIdnId] IN (1,2,3,4,5)");
            table.HasCheckConstraint("CK_Cliente_Identificacion", "LEN(LTRIM(RTRIM([ClnNumeroIdentificacion]))) > 0");
            table.HasCheckConstraint("CK_Cliente_RazonSocial", "LEN(LTRIM(RTRIM([ClnRazonSocial]))) > 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("ClnId").UseIdentityColumn();
        b.Property(x => x.TipoIdentificacion).HasColumnName("ClnTpoIdnId").HasConversion<short>();
        b.Property(x => x.NumeroIdentificacion).HasColumnName("ClnNumeroIdentificacion").HasMaxLength(30).IsUnicode(false);
        b.Property(x => x.RazonSocial).HasColumnName("ClnRazonSocial").HasMaxLength(150).IsUnicode(false);
        b.Property(x => x.PaisCodigo).HasColumnName("ClnPaisCodigo");
        b.Property(x => x.DepartamentoCodigo).HasColumnName("ClnDptColCodigoDane");
        b.Property(x => x.CiudadCodigo).HasColumnName("ClnDvsPltColCodigoDane");
        b.Property(x => x.Active).HasDefaultValue(true).IsConcurrencyToken();
        b.HasIndex(x => new { x.TipoIdentificacion, x.NumeroIdentificacion }).IsUnique();
        b.HasOne(x => x.Pais).WithMany().HasForeignKey(x => x.PaisCodigo).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Departamento).WithMany()
            .HasForeignKey(x => new { x.DepartamentoCodigo, x.PaisCodigo })
            .HasPrincipalKey(x => new { x.Codigo, x.PaisCodigo }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Ciudad).WithMany()
            .HasForeignKey(x => new { x.CiudadCodigo, x.DepartamentoCodigo })
            .HasPrincipalKey(x => new { x.Codigo, x.DepartamentoCodigo }).OnDelete(DeleteBehavior.Restrict);
        b.ConfigureAudit();
    }
}
