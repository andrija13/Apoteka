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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Apoteka.Pages
{
    public class DodajApotekuModel : PageModel
    {
        [BindProperty]
        public ApotekaModel Apoteka { get; set; }
        [BindProperty]
        public IList<Lokacija> Lokacije { get; set; }
        [BindProperty]
        public IFormFile Photo { get; set; }

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<Korisnik> _userManager;
        private readonly AnnotationsContext _context;

        public DodajApotekuModel(UserManager<Korisnik> userManager, AnnotationsContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            Lokacije = new List<Lokacija>();
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostDodajAsync()
        {

            var korisnik = await _userManager.GetUserAsync(User);

            if (Photo != null)
            {
                if (Apoteka.Slika != null)
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "imagesApoteke", Apoteka.Slika);
                    System.IO.File.Delete(filePath);
                }
                Apoteka.Slika = ProcessUploadedFile();
            }

            await _context.GraphClient.Cypher.Create("(n:Apoteka { ApotekaID:'" + Apoteka.ApotekaID + "', Naziv:'" + Apoteka.Naziv +
               "', Email:'" + Apoteka.Email + "', Direktor:'" + Apoteka.Direktor + "', BrojTelefona:'" + Apoteka.BrojTelefona + "', Slika:'"+Apoteka.Slika+"'})").ExecuteWithoutResultsAsync();

            var apoteka = _context.GraphClient.Cypher.Match("(n:Apoteka)").Where("n.Naziv = '" + Apoteka.Naziv + "' AND n.Direktor= '" + Apoteka.Direktor + "'").Return(n => n.As<Node<string>>());
            var rez = apoteka.Results.Single();
            string id = rez.Reference.Id.ToString();
            Apoteka.ApotekaID = id;
            await _context.GraphClient.Cypher.Match("(n:Apoteka { Naziv:'" + Apoteka.Naziv + "', Direktor:'" + Apoteka.Direktor + "'})").Set("n.ApotekaID = " + id).ExecuteWithoutResultsAsync();

            foreach (Lokacija lok in Lokacije)
            {
                if (lok != null)
                {
                    await _context.GraphClient.Cypher.Create("(n:Lokacija { ID:'" + lok.ID + "', Grad:'" + lok.Grad + "', UlicaBr:'" + lok.UlicaBr +
                   "', BrojTelefona:'" + lok.BrojTelefona + "'}) return n").ExecuteWithoutResultsAsync();

                    var lokacija = _context.GraphClient.Cypher.Match("(n:Lokacija)").Where("n.Grad = '" + lok.Grad + "' AND n.UlicaBr= '" + lok.UlicaBr + "'").Return(n => n.As<Node<string>>());
                    var r = lokacija.Results.Single();
                    string idl = r.Reference.Id.ToString();

                    await _context.GraphClient.Cypher.Match("(n:Lokacija)").Where("n.Grad = '" + lok.Grad + "' AND n.UlicaBr= '" + lok.UlicaBr + "'").Set("n.ID = " + idl).ExecuteWithoutResultsAsync();

                    await _context.GraphClient.Cypher.Match("(a:Apoteka),(b:Lokacija)").Where("a.ApotekaID =" + id + " AND b.ID= " + idl)
                        .Create("(a)-[r:SE_NALAZI_U]->(b) return type(r)").ExecuteWithoutResultsAsync();
                }
            }

            await _context.GraphClient.Cypher.Match("(a:Apoteka),(b:IdentityUser)").Where("a.ApotekaID =" + id + " AND b.UserName= '" + korisnik.UserName + "'")
                                             .Create("(b)-[r:POSEDUJE]->(a) return type(r)").ExecuteWithoutResultsAsync();

            return RedirectToPage("ProfilApoteke",new {id=Apoteka.ApotekaID});
        }
        public string ProcessUploadedFile()
        {
            string uniqueFileName = null;
            if (Photo != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "imagesApoteke");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    Photo.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
    }
}
