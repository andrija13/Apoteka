using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Neo4j.AspNetCore.Identity;
using Apoteka.Models;
using Microsoft.AspNetCore.Identity;
using Neo4jClient.DataAnnotations;
using Neo4jClient;

namespace Apoteka.Pages
{
    public class DodajApotekuModel : PageModel
    {
        [BindProperty]
        public ApotekaModel Apoteka { get; set; }
        [BindProperty]
        public IList<Lokacija> Lokacije { get; set; }
        public Korisnik Korisnik { get; set; }
        private readonly UserManager<Korisnik> _userManager;
        private readonly AnnotationsContext _context;

        public DodajApotekuModel(UserManager<Korisnik> userManager, AnnotationsContext context)
        {
            _userManager = userManager;
            _context = context;
            Lokacije = new List<Lokacija>();
        }
        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostDodajAsync()
        {

            var korisnik = await _userManager.GetUserAsync(User);
            Apoteka.Nadlezni = korisnik;

            await _context.GraphClient.Cypher.Create("(n:Apoteka { id:'" + Apoteka.ApotekaID + "', naziv:'" + Apoteka.Naziv +
               "', email:'" + Apoteka.Email + "', direktor:'" + Apoteka.Direktor + "', brojtelefona:'" + Apoteka.BrojTelefona + "', nadlezni:'" + Apoteka.Nadlezni + "'}) return n").ExecuteWithoutResultsAsync();

            foreach (Lokacija lok in Lokacije)
            {
                await _context.GraphClient.Cypher.Create("(n:Lokacija { id:'" + lok.ID + "', grad:'" + lok.Grad + "', ulicabroj:'"+ lok.UlicaBr+
               "', brojtelefona:'" + lok.BrojTelefona + "'}) return n").ExecuteWithoutResultsAsync();

                await _context.GraphClient.Cypher.Match("(a:Apoteka),(b:Lokacija)").Where("a.naziv ='" + Apoteka.Naziv + "' AND a.direktor='"+Apoteka.Direktor+"' AND b.grad ='" + lok.Grad + "' AND b.ulicabroj= '" + lok.UlicaBr + "'")
                    .Create("(a)-[r:SE_NALAZI_U]->(b) return type(r)").ExecuteWithoutResultsAsync();
            }
  
            return RedirectToPage();
        }
    }
}
