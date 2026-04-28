namespace CineScore.Models
{
    public class StatistikaFilma
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int SteviloOcen { get; set; }
        public float PovprecjeOcen { get; set; }

        public Movie? Movie { get; set; }

        public float izracunajPovprecje()
        {
            return PovprecjeOcen;
        }

        public void posodobiStatistiko(int steviloOcen, float povprecjeOcen)
        {
            SteviloOcen = steviloOcen;
            PovprecjeOcen = povprecjeOcen;
        }
    }
}
