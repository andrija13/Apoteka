using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.AspNetCore.Identity;

namespace Apoteka.Models
{
    public class Korisnik : IdentityUser
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public Korisnik() : base()
        {

        }
        public Korisnik(string userName, string email) : base(userName,email)
        {

        }

    }
}
