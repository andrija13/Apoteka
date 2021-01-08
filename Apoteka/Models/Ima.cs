namespace Apoteka.Models
{
    public class Ima
    {
        public string VezaID { get; set; }
        public Lokacija LokacijaVeza { get; set; }
        public Proizvod ProizvodVeza { get; set; }
        public string Cena { get; set; }
    }
}
