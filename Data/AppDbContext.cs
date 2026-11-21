
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Akun> DaftarAkun => Set<Akun>();
    public DbSet<Transaksi> DaftarTransaksi => Set<Transaksi>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Akun>(e =>
        {
            e.ToTable("Akun");
            e.Property(a => a.Saldo).HasPrecision(18, 2);
            e.HasIndex(a => a.NomorAkun).IsUnique();
            e.HasIndex(a => a.NomorHp).IsUnique();
        });

        mb.Entity<Transaksi>(e =>
        {
            e.ToTable("Transaksi");
            e.Property(t => t.Jumlah).HasPrecision(18, 2);
            e.Property(t => t.BiayaAdmin).HasPrecision(18, 2);
            e.Property(t => t.SaldoSebelum).HasPrecision(18, 2);
            e.Property(t => t.SaldoSesudah).HasPrecision(18, 2);
            e.HasIndex(t => t.KodeTransaksi).IsUnique();
            e.HasIndex(t => t.KunciIdempotensi).IsUnique();
        });
    }
}
