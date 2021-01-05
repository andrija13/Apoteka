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
using Newtonsoft.Json;

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

            //var apoteka = _context.GraphClient.Cypher.Match("(n:Apoteka)").Where("(n.Naziv = '" + naziv + "')").Return(n => n.As<Node<string>>());
            //var rez = apoteka.Results.Single();

            ////List<ApotekaModel> ap = rez.Select(node => JsonConvert.DeserializeObject<ApotekaModel>(node.Data)).ToList();
            //ApotekaModel ap = JsonConvert.DeserializeObject<ApotekaModel>(rez.Data);
            //string id = rez.Reference.Id.ToString();

        }

        public async Task<IActionResult> OnPostDodajAsync()
        {

            var korisnik = await _userManager.GetUserAsync(User);

            await _context.GraphClient.Cypher.Create("(n:Apoteka { ApotekaID:'" + Apoteka.ApotekaID + "', Naziv:'" + Apoteka.Naziv +
               "', Email:'" + Apoteka.Email + "', Direktor:'" + Apoteka.Direktor + "', BrojTelefona:'" + Apoteka.BrojTelefona + "'}) return n").ExecuteWithoutResultsAsync();


            await _context.GraphClient.Cypher.Match("(a:Apoteka),(b:IdentityUser)").Where("a.Naziv ='" + Apoteka.Naziv + "' AND a.Direktor='" + Apoteka.Direktor + "' AND b.UserName= '" + korisnik.UserName+ "'")
                    .Create("(b)-[r:POSEDUJE]->(a) return type(r)").ExecuteWithoutResultsAsync();

            var apoteka = _context.GraphClient.Cypher.Match("(n:Apoteka)").Where("n.Naziv = '" + Apoteka.Naziv + "' AND n.Direktor= '"+Apoteka.Direktor+ "'").Return(n => n.As<Node<string>>());
            var rez = apoteka.Results.Single();
            string id = rez.Reference.Id.ToString();
            
            await _context.GraphClient.Cypher.Match("(n:Apoteka { Naziv:'" + Apoteka.Naziv + "', Direktor:'" + Apoteka.Direktor + "'})").Set("n.ApotekaID = " + id).ExecuteWithoutResultsAsync();

            foreach (Lokacija lok in Lokacije)
            {
                if (lok != null)
                {
                    await _context.GraphClient.Cypher.Create("(n:Lokacija { ID:'" + lok.ID + "', Grad:'" + lok.Grad + "', UlicaBr:'" + lok.UlicaBr +
                   "', BrojTelefona:'" + lok.BrojTelefona + "'}) return n").ExecuteWithoutResultsAsync();

                    var lokacija = _context.GraphClient.Cypher.Match("(n:Lokacija)").Where("n.Grad = '" + lok.Grad + "' AND n.UlicaBr= '" + lok.UlicaBr + "'").Return(n => n.As<Node<string>>());
                    var r = apoteka.Results.Single();
                    string idl = rez.Reference.Id.ToString();

                    await _context.GraphClient.Cypher.Match("(n:Lokacija { Grad:'" + lok.Grad + "', UlicaBr:'" + lok.UlicaBr + "'})").Set("n.ID = " + idl).ExecuteWithoutResultsAsync();

                    await _context.GraphClient.Cypher.Match("(a:Apoteka),(b:Lokacija)").Where("a.Naziv ='" + Apoteka.Naziv + "' AND a.Direktor='" + Apoteka.Direktor + "' AND b.Grad ='" + lok.Grad + "' AND b.UlicaBr= '" + lok.UlicaBr + "'")
                        .Create("(a)-[r:SE_NALAZI_U]->(b) return type(r)").ExecuteWithoutResultsAsync();
                }
            }
  
            return RedirectToPage();
        }
    }
}