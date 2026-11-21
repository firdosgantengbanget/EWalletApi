using System.ComponentModel.DataAnnotations.Schema;
public class Transaksi
{
    public int Id { get; set; }
    public string KodeTransaksi { get; set; } = "";
    public string Jenis { get; set; } = "";          // IsiSaldo atau Transfer
    public int? AkunAsalId { get; set; }             // kosong untuk IsiSaldo
    public int AkunTujuanId { get; set; }
    public decimal Jumlah { get; set; }
    public decimal BiayaAdmin { get; set; }
    public decimal SaldoSebelum { get; set; }        // saldo akun yang berubah (pengirim / akun yang diisi)
    public decimal SaldoSesudah { get; set; }
    [NotMapped] public string JumlahRupiah => Rupiah.Format(Jumlah);
    [NotMapped] public string BiayaAdminRupiah => Rupiah.Format(BiayaAdmin);
    [NotMapped] public string SaldoSebelumRupiah => Rupiah.Format(SaldoSebelum);
    [NotMapped] public string SaldoSesudahRupiah => Rupiah.Format(SaldoSesudah);
    public string? Keterangan { get; set; }
    public string Status { get; set; } = "Berhasil";
    public string KunciIdempotensi { get; set; } = "";
    public DateTime WaktuTransaksi { get; set; } = DateTime.Now;
    [NotMapped] public string WaktuTransaksiTeks => Tanggal.Format(WaktuTransaksi);
}
