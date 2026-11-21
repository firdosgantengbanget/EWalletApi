using System.ComponentModel.DataAnnotations.Schema;

public class Akun
{
    public int Id { get; set; }
    public string NomorAkun { get; set; } = "";
    public string NamaPemilik { get; set; } = "";
    public string NomorHp { get; set; } = "";
    public decimal Saldo { get; set; }
    [NotMapped] public string SaldoRupiah => Rupiah.Format(Saldo);
    public string Status { get; set; } = "Aktif";
    public DateTime DibuatPada { get; set; } = DateTime.Now;
    [NotMapped] public string DibuatPadaTeks => Tanggal.Format(DibuatPada);
}