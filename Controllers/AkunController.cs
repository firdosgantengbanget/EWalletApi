
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public record BuatAkunRequest(string NamaPemilik, string NomorHp, decimal SaldoAwal);

[ApiController]
[Route("api/akun")]
public class AkunController : ControllerBase
{
    private readonly AppDbContext _db;
    public AkunController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> BuatAkun(BuatAkunRequest r)
    {
        if (r.SaldoAwal < 0)
            return BadRequest(new { pesan = "Saldo awal tidak boleh negatif" });

        if (await _db.DaftarAkun.AnyAsync(a => a.NomorHp == r.NomorHp))
            return Conflict(new { pesan = "Nomor HP sudah terdaftar" });

        var akun = new Akun
        {
            NomorAkun = "DW" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
            NamaPemilik = r.NamaPemilik,
            NomorHp = r.NomorHp,
            Saldo = r.SaldoAwal
        };
        _db.DaftarAkun.Add(akun);
        await _db.SaveChangesAsync();
        return Ok(new { pesan = "Akun berhasil dibuat", akun });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> DetailAkun(int id)
    {
        var akun = await _db.DaftarAkun.FindAsync(id);
        return akun == null ? NotFound(new { pesan = "Akun tidak ditemukan" }) : Ok(akun);
    }

    [HttpGet("{id}/saldo")]
    public async Task<IActionResult> CekSaldo(int id)
    {
        var a = await _db.DaftarAkun.FindAsync(id);
        if (a == null) return NotFound(new { pesan = "Akun tidak ditemukan" });
        return Ok(new { a.Id, a.NomorAkun, a.NamaPemilik, a.Saldo, a.SaldoRupiah, a.Status });
    }
}
