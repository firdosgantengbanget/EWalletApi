using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public record IsiSaldoRequest(decimal Jumlah, string KunciIdempotensi, string? Keterangan);
public record PermintaanTransfer(int AkunAsalId, int AkunTujuanId, decimal Jumlah, string KunciIdempotensi, string? Keterangan);

[ApiController]
[Route("api")]
public class TransaksiController : ControllerBase
{
    private const decimal BiayaAdmin = 2500m;
    private const decimal MinimalNominal = 10000m;

    private readonly AppDbContext _db;
    public TransaksiController(AppDbContext db) => _db = db;

    private static string BuatKode() =>
        $"TRX-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

    // ---------- ISI SALDO ----------

    [HttpPost("akun/{id}/isi-saldo")]
    public async Task<IActionResult> IsiSaldo(int id, IsiSaldoRequest r)
    {
        if (r.Jumlah < MinimalNominal)
            return BadRequest(new { pesan = "Minimal isi saldo Rp10.000" });

        var sudah = await _db.DaftarTransaksi.FirstOrDefaultAsync(t => t.KunciIdempotensi == r.KunciIdempotensi);
        if (sudah != null)
            return Ok(new { pesan = "Transaksi sudah pernah diproses", transaksi = sudah });

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        try
        {
            var akun = await _db.DaftarAkun
                .FromSqlInterpolated($"SELECT * FROM Akun WITH (UPDLOCK, ROWLOCK) WHERE Id = {id}")
                .SingleOrDefaultAsync();
            if (akun == null) return NotFound(new { pesan = "Akun tidak ditemukan" });
            if (akun.Status != "Aktif") return BadRequest(new { pesan = "Akun sedang diblokir" });

            var saldoSebelum = akun.Saldo;
            akun.Saldo += r.Jumlah;

            var trx = new Transaksi
            {
                KodeTransaksi = BuatKode(),
                Jenis = "IsiSaldo",
                AkunAsalId = null,
                AkunTujuanId = akun.Id,
                Jumlah = r.Jumlah,
                BiayaAdmin = 0,
                SaldoSebelum = saldoSebelum,
                SaldoSesudah = akun.Saldo,
                Keterangan = r.Keterangan,
                KunciIdempotensi = r.KunciIdempotensi
            };
            _db.DaftarTransaksi.Add(trx);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return Ok(new { pesan = "Isi saldo berhasil", transaksi = trx });
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            return Conflict(new { pesan = "Kunci idempotensi sudah dipakai oleh permintaan lain" });
        }
    }

    // ---------- TRANSFER (INTI SINKRONISASI) ----------

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(PermintaanTransfer r)
    {
        if (r.Jumlah < MinimalNominal)
            return BadRequest(new { pesan = "Minimal transfer Rp10.000" });
        if (r.AkunAsalId == r.AkunTujuanId)
            return BadRequest(new { pesan = "Akun asal dan akun tujuan tidak boleh sama" });

        // Idempotensi: permintaan yang sama tidak diproses dua kali
        var sudah = await _db.DaftarTransaksi.FirstOrDefaultAsync(t => t.KunciIdempotensi == r.KunciIdempotensi);
        if (sudah != null)
            return Ok(new { pesan = "Transaksi sudah pernah diproses", transaksi = sudah });

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        try
        {
            // Kunci kedua akun dengan urutan Id yang konsisten -> mencegah deadlock
            var daftarId = new[] { r.AkunAsalId, r.AkunTujuanId }.OrderBy(x => x).ToArray();
            var terkunci = new Dictionary<int, Akun>();
            foreach (var id in daftarId)
            {
                var akun = await _db.DaftarAkun
                    .FromSqlInterpolated($"SELECT * FROM Akun WITH (UPDLOCK, ROWLOCK) WHERE Id = {id}")
                    .SingleOrDefaultAsync();
                if (akun == null)
                    return NotFound(new { pesan = $"Akun {id} tidak ditemukan" });
                terkunci[id] = akun;
            }

            var asal = terkunci[r.AkunAsalId];
            var tujuan = terkunci[r.AkunTujuanId];

            if (asal.Status != "Aktif")
                return BadRequest(new { pesan = "Akun asal sedang diblokir" });
            if (tujuan.Status != "Aktif")
                return BadRequest(new { pesan = "Akun tujuan sedang diblokir" });

            var total = r.Jumlah + BiayaAdmin;
            if (asal.Saldo < total)
                return BadRequest(new { pesan = "Saldo tidak cukup (jumlah + biaya admin Rp2.500)" });

            var saldoSebelum = asal.Saldo;
            asal.Saldo -= total;
            tujuan.Saldo += r.Jumlah;

            var trx = new Transaksi
            {
                KodeTransaksi = BuatKode(),
                Jenis = "Transfer",
                AkunAsalId = asal.Id,
                AkunTujuanId = tujuan.Id,
                Jumlah = r.Jumlah,
                BiayaAdmin = BiayaAdmin,
                SaldoSebelum = saldoSebelum,
                SaldoSesudah = asal.Saldo,
                Keterangan = r.Keterangan,
                KunciIdempotensi = r.KunciIdempotensi
            };
            _db.DaftarTransaksi.Add(trx);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return Ok(new { pesan = "Transfer berhasil", transaksi = trx });
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            return Conflict(new { pesan = "Kunci idempotensi sudah dipakai oleh permintaan lain" });
        }
    }

    // ---------- RIWAYAT ----------

    [HttpGet("akun/{id}/mutasi")]
    public async Task<IActionResult> Mutasi(int id, DateTime? dari, DateTime? sampai)
    {
        if (!await _db.DaftarAkun.AnyAsync(a => a.Id == id))
            return NotFound(new { pesan = "Akun tidak ditemukan" });

        var q = _db.DaftarTransaksi.Where(t => t.AkunAsalId == id || t.AkunTujuanId == id);
        if (dari.HasValue)
            q = q.Where(t => t.WaktuTransaksi >= dari.Value.Date);
        if (sampai.HasValue)
            q = q.Where(t => t.WaktuTransaksi < sampai.Value.Date.AddDays(1));

        return Ok(await q.OrderByDescending(t => t.WaktuTransaksi).ToListAsync());
    }

    [HttpGet("transaksi/{kode}")]
    public async Task<IActionResult> DetailTransaksi(string kode)
    {
        var trx = await _db.DaftarTransaksi.FirstOrDefaultAsync(t => t.KodeTransaksi == kode);
        return trx == null ? NotFound(new { pesan = "Transaksi tidak ditemukan" }) : Ok(trx);
    }
}
