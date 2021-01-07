using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apoteka.Models
{
    public class Proizvod
    {
        public string ID { get; set; } 
        public string Naziv { get; set; }
        public string Proizvodjac { get; set; }
        public string Kategorija{ get; set; }
        public double Cena { get; set; }
        public string Opis { get; set; }
        public string Slika { get; set; }
    }
}
