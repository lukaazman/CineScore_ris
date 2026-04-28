using System.Collections.Generic;

namespace CineScore.Models
{
    public class ZM_RegistriranUporabnik_OceniFilm
    {
        public IEnumerable<Movie> Filmi { get; set; } = new List<Movie>();
        public Movie? IzbranFilm { get; set; }
        public int VnesenaOcena { get; set; }
        public string? Sporocilo { get; set; }
        public string? Napaka { get; set; }

        public int izberiFilm()
        {
            return IzbranFilm?.Id ?? 0;
        }

        public int vnesiOceno()
        {
            return VnesenaOcena;
        }

        public int prikaziPodrobnostiFilma()
        {
            return IzbranFilm?.Id ?? 0;
        }

        public int prikaziSporociloNapake()
        {
            return string.IsNullOrWhiteSpace(Napaka) ? 0 : 1;
        }

        public int prikaziPosodobljenoOceno()
        {
            return IzbranFilm?.Id ?? 0;
        }
    }
}
