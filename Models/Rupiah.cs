using System.Globalization;

public static class Rupiah
{
    private static readonly CultureInfo Indonesia = new("id-ID");

    public static string Format(decimal nilai) => "Rp" + nilai.ToString("N0", Indonesia);
}

public static class Tanggal
{
    private static readonly CultureInfo Indonesia = new("id-ID");

    public static string Format(DateTime waktu) =>
        waktu.ToString("dddd, d MMMM yyyy, HH'.'mm 'WIB'", Indonesia);
}