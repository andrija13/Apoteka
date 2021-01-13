using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Neo4j.AspNetCore.Identity;
using Neo4jClient;
using Neo4jClient.DataAnnotations;
using Apoteka.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.IO;

namespace Apoteka.Pages
{
    public class ProfilApotekeModel : PageModel
    {

        [BindProperty]
        public ApotekaModel Apoteka { get; set; }
        [BindProperty]
        public string IzabraniProizvod { get; set; }
        [BindProperty]
        public IList<Ima> Imas { get; set; }
        [BindProperty]
        public IList<Lokacija> Lokacije { get; set; }
        [BindProperty]
        public IList<string> NoviGradovi { get; set; }
        [BindProperty]
        public IList<string> NoveAdrese { get; set; }
        [BindProperty]
        public IList<Proizvod> Proizvodi { get; set; }
        [BindProperty]
        public IList<string> NoviBrojevi { get; set; }
        [BindProperty]
        public List<SelectListItem> SviProizvodi { get; set; }
        [BindProperty]
        public IList<string> Cene { get; set; }
        [BindProperty]
        public IList<Lokacija> MojeLokacije { get; set; }
        [BindProperty]
        public Proizvod ZahtevaniProizvod { get; set; }
        public Korisnik Vlasnik { get; set; }
        [BindProperty]
        public IFormFile Photo { get; set; }
        [BindProperty]
        public IFormFile PhotoProizvod { get; set; }

        private readonly UserManager<Korisnik> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AnnotationsContext _context;
        public ProfilApotekeModel(AnnotationsContext context, IWebHostEnvironment webHostEnvironment, UserManager<Korisnik> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            Lokacije = new List<Lokacija>();
            NoviGradovi = new List<string>();
            NoveAdrese = new List<string>();
            NoviBrojevi = new List<string>();
            Proizvodi = new List<Proizvod>();
            SviProizvodi = new List<SelectListItem>();
            Cene = new List<string>();
            MojeLokacije = new List<Lokacija>();
            Imas = new List<Ima>();
        }
        public void OnGet(string id)
        {
            var apoteka = _context.GraphClient.Cypher.Match("(n:Apoteka)").Where("n.ApotekaID= " + id).Return(n => n.As<Node<string>>());
            var rez = apoteka.Results.Single();
            Apoteka = JsonConvert.DeserializeObject<ApotekaModel>(rez.Data);

            var vlasnik = _context.GraphClient.Cypher.Match("(n:IdentityUser)-[:POSEDUJE]->(a:Apoteka{ ApotekaID:" + id+"})").Return(n => n.As<Node<string>>());
            var rez1 = vlasnik.Results.Single();
            Vlasnik = JsonConvert.DeserializeObject<Korisnik>(rez1.Data);

            var lokacija = _context.GraphClient.Cypher.Match("(n:Apoteka {ApotekaID:" + id + "})-[:SE_NALAZI_U]->(l: Lokacija)").Return(l => l.As<Node<string>>());
            var rezultat = lokacija.Results;

            Lokacije = rezultat.Select(node => JsonConvert.DeserializeObject<Lokacija>(node.Data)).ToList();

            var proizvod = _context.GraphClient.Cypher.Match("(p:Proizvod)").Return(p => p.As<Node<string>>());
            var rezul = proizvod.Results;
            Proizvodi = rezul.Select(node => JsonConvert.DeserializeObject<Proizvod>(node.Data)).ToList();

            foreach (Proizvod pro in Proizvodi)
            {
                SelectListItem p = new SelectListItem();
                p.Text = pro.Naziv + " " + pro.Opis;
                p.Value = pro.ID;
                SviProizvodi.Add(p);
            }

            foreach (Lokacija l in Lokacije)
            {
                MojeLokacije.Add(l);
            }

            int brojac = 0;
            foreach (Lokacija ml in MojeLokacije)
            {
                List<Proizvod> pomProizvod = new List<Proizvod>();
                
                var imaProizvode = _context.GraphClient.Cypher.Match("(l:Lokacija {ID:" + ml.ID + "})-[:IMA]->(p:Proizvod)").Return(p => p.As<Node<string>>());
                var provera = imaProizvode.Results;
                pomProizvod = provera.Select(node => JsonConvert.DeserializeObject<Proizvod>(node.Data)).ToList();
                foreach(Proizvod p in pomProizvod)
                {
                    Ima i = new Ima();
                    i.LokacijaVeza = ml;
                    var cenequery = _context.GraphClient.Cypher.Match("(p:Proizvod { ID:" + p.ID + "})<-[r:IMA]-(l:Lokacija {ID :" + ml.ID + "})").Return(r => r.As<Node<string>>());
                    var c = cenequery.Results.Single();
                    string idv = c.Reference.Id.ToString();
                    string cena = JObject.Parse(c.Data)["Cena"].ToString();
                    i.ProizvodVeza = p;
                    i.Cena = cena;
                    i.VezaID = idv;
                    Imas.Add(i);
                    brojac++;
                }
            }
        }
        public async Task<IActionResult> OnPostSacuvajApotekuAsync()
        {
            if (Photo != null)
            {
                if (Apoteka.Slika != null)
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", Apoteka.Slika);
                    System.IO.File.Delete(filePath);
                }
                Apoteka.Slika = ProcessUploadedFile(Photo);
            }

            await _context.GraphClient.Cypher.Match("(n:Apoteka)").Where("n.ApotekaID = " + Apoteka.ApotekaID).Set("n = { ApotekaID:" + Apoteka.ApotekaID + ", Naziv: '" + Apoteka.Naziv + "', Email:'" + Apoteka.Email + "', Direktor: '" + Apoteka.Direktor + "', BrojTelefona: '" + Apoteka.BrojTelefona + "', Slika: '"+ Apoteka.Slika +"'}").ExecuteWithoutResultsAsync();

            if (NoviGradovi.Count > 0)
            {
                for (var clan = 0; clan < NoviGradovi.Count; clan++)
                {
                    if (NoviGradovi[clan] != "" && NoveAdrese[clan] != "")
                    {
                        Lokacija lok = new Lokacija();
                        lok.Grad = NoviGradovi[clan];
                        lok.UlicaBr = NoveAdrese[clan];
                        lok.BrojTelefona = NoviBrojevi[clan];

                        lok.ID = "";
                        await _context.GraphClient.Cypher.Create("(n:Lokacija { ID:'" + lok.ID + "', Grad:'" + lok.Grad + "', UlicaBr:'" + lok.UlicaBr +
                       "', BrojTelefona:'" + lok.BrojTelefona + "'}) return n").ExecuteWithoutResultsAsync();

                        var lokacija = _context.GraphClient.Cypher.Match("(n:Lokacija)").Where("n.Grad = '" + lok.Grad + "' AND n.UlicaBr= '" + lok.UlicaBr + "'").Return(n => n.As<Node<string>>());
                        var r = lokacija.Results.Single();
                        string idl = r.Reference.Id.ToString();

                        await _context.GraphClient.Cypher.Match("(n:Lokacija)").Where("n.Grad = '" + lok.Grad + "' AND n.UlicaBr= '" + lok.UlicaBr + "'").Set("n.ID = " + idl).ExecuteWithoutResultsAsync();

                        await _context.GraphClient.Cypher.Match("(a:Apoteka),(b:Lokacija)").Where("a.ApotekaID =" + Apoteka.ApotekaID + " AND b.ID= " + idl)
                            .Create("(a)-[r:SE_NALAZI_U]->(b) return type(r)").ExecuteWithoutResultsAsync();
                    }
                }
            }

           foreach(Ima i in Imas)
            {
                await _context.GraphClient.Cypher.Match("(p:Proizvod)<-[r:IMA{ID:"+i.VezaID+"}]-(l:Lokacija)").Set("r.Cena = '" +i.Cena+"'").ExecuteWithoutResultsAsync();

            }


            return RedirectToPage("ProfilApoteke", new { id = Apoteka.ApotekaID });
        }
        public async Task<IActionResult> OnPostUkloniLokacijuAsync(string idlok)
        {
            await _context.GraphClient.Cypher.Match("(n:Lokacija)").Where("n.ID= " + idlok).DetachDelete("n").ExecuteWithoutResultsAsync();

            return RedirectToPage("ProfilApoteke", new { id = Apoteka.ApotekaID });
        }
        public async Task<IActionResult> OnPostUkloniApotekuAsync()
        {
            var lokacija = _context.GraphClient.Cypher.Match("(n:Apoteka {ApotekaID:" + Apoteka.ApotekaID + "})-[:SE_NALAZI_U]->(l: Lokacija)").Return(l => l.As<Node<string>>());
            var rezultat = lokacija.Results;

            Lokacije = rezultat.Select(node => JsonConvert.DeserializeObject<Lokacija>(node.Data)).ToList();

            foreach (Lokacija lok in Lokacije)
            {
                if (lok != null)
                {
                    await _context.GraphClient.Cypher.Match("(l:Lokacija)").Where("l.ID= " + lok.ID).DetachDelete("l").ExecuteWithoutResultsAsync();
                }
            }

            await _context.GraphClient.Cypher.Match("(n:Apoteka)").Where("n.ApotekaID= " + Apoteka.ApotekaID).DetachDelete("n").ExecuteWithoutResultsAsync();

            return RedirectToPage("./Index");
        }
        public async Task<IActionResult> OnPostDodajProizvodAsync()
        {
            foreach (Lokacija l in Lokacije)
            {
                if (l.Sel == true)
                    MojeLokacije.Add(l);

            }
            int br = 0;
            foreach (Lokacija ml in MojeLokacije)
            {
                var lokacija = _context.GraphClient.Cypher.Match("(l:Lokacija {ID:" + ml.ID + "})-[:IMA]->(p:Proizvod{ID:" + IzabraniProizvod + "})").Return(p => p.As<Node<string>>());
                var provera = lokacija.Results;
                if (provera.Count() == 0)
                {
                    while(Cene[br] == null)
                    {
                        br++;
                    }
                    await _context.GraphClient.Cypher.Match("(p:Proizvod),(l:Lokacija)").Where("p.ID=" + IzabraniProizvod + " AND l.ID=" + ml.ID)
                                        .Create("(l)-[r:IMA {Cena:'" + Cene[br] + "'}]->(p)").ExecuteWithoutResultsAsync();

                    var veza = _context.GraphClient.Cypher.Match("(p:Proizvod{ID:" + IzabraniProizvod + "})<-[r:IMA]-(l:Lokacija{ID:" + ml.ID + "})").Return(r => r.As<Node<string>>());
                    var r = veza.Results.Single();
                    string idv = r.Reference.Id.ToString();

                    await _context.GraphClient.Cypher.Match("(p:Proizvod{ID:" + IzabraniProizvod + "})<-[r:IMA]-(l:Lokacija{ID:" + ml.ID + "})").Set("r.ID = " + idv).ExecuteWithoutResultsAsync();
                    
                    br++;
                }
                
            }

            return RedirectToPage("ProfilApoteke", new { id = Apoteka.ApotekaID });
           
        }
        public async Task<IActionResult> OnPostUkloniVezuAsync(string idVeza)
        {
            await _context.GraphClient.Cypher.Match("(p:Proizvod)<-[r:IMA {ID:"+idVeza+"}]-(l:Lokacija)").Delete("r").ExecuteWithoutResultsAsync();

            return RedirectToPage("ProfilApoteke", new { id = Apoteka.ApotekaID });
        }
        public async Task<IActionResult> OnPostZahtevajProizvodAsync()
        {
            if (PhotoProizvod != null)
            {
                if (ZahtevaniProizvod.Slika != null)
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", PhotoProizvod.Name);
                    System.IO.File.Delete(filePath);
                }
                ZahtevaniProizvod.Slika = ProcessUploadedFile(PhotoProizvod);
            }

            await _context.GraphClient.Cypher.Create("(n:Proizvod {ID:'' ,Naziv:'"+ZahtevaniProizvod.Naziv.ToUpper() +"', Kategorija:'"+ZahtevaniProizvod.Kategorija.ToUpper() +"', Opis:'"+ZahtevaniProizvod.Opis+"',Proizvodjac:'"+ZahtevaniProizvod.Proizvodjac+"',Slika:'"+ZahtevaniProizvod.Slika+"'})").ExecuteWithoutResultsAsync();

            var proizvod = _context.GraphClient.Cypher.Match("(n:Proizvod { Naziv:'" + ZahtevaniProizvod.Naziv.ToUpper() + "', Proizvodjac:'" + ZahtevaniProizvod.Proizvodjac + "', Kategorija:'" + ZahtevaniProizvod.Kategorija.ToUpper() + "' })").Return(n => n.As<Node<string>>());
            var rez = proizvod.Results.Single();
            string id = rez.Reference.Id.ToString();
            ZahtevaniProizvod.ID = id;

            await _context.GraphClient.Cypher.Match("(n:Proizvod { Naziv:'" + ZahtevaniProizvod.Naziv.ToUpper() + "', Proizvodjac:'" + ZahtevaniProizvod.Proizvodjac + "', Kategorija:'" + ZahtevaniProizvod.Kategorija.ToUpper() + "' })").Set("n.ID =" +id).ExecuteWithoutResultsAsync();

            foreach (Lokacija l in Lokacije)
            {
                if (l.Sel == true)
                    MojeLokacije.Add(l);

            }

            foreach (Lokacija ml in MojeLokacije)
            {

                var lokacija = _context.GraphClient.Cypher.Match("(l:Lokacija {ID:" + ml.ID + "})-[:IMA]->(p:Proizvod{ID:" + ZahtevaniProizvod.ID + "})").Return(p => p.As<Node<string>>());
                var provera = lokacija.Results;
                int br = 0;
                if (provera.Count() == 0)
                {
                    while (Cene[br] == null)
                    {
                        br++;
                    }
                    await _context.GraphClient.Cypher.Match("(p:Proizvod),(l:Lokacija)").Where("p.ID=" + ZahtevaniProizvod.ID + " AND l.ID=" + ml.ID)
                                        .Create("(l)-[r:IMA {Cena:'" + Cene[br] + "'}]->(p)").ExecuteWithoutResultsAsync();

                    var veza = _context.GraphClient.Cypher.Match("(p:Proizvod{ID:" + ZahtevaniProizvod.ID + "})<-[r:IMA]-(l:Lokacija{ID:" + ml.ID + "})").Return(r => r.As<Node<string>>());
                    var r = veza.Results.Single();
                    string idv = r.Reference.Id.ToString();

                    await _context.GraphClient.Cypher.Match("(p:Proizvod{ID:" + ZahtevaniProizvod.ID + "})<-[r:IMA]-(l:Lokacija{ID:" + ml.ID + "})").Set("r.ID = " + idv).ExecuteWithoutResultsAsync();

                    br++;
                }

            }
            return RedirectToPage("ProfilApoteke", new { id = Apoteka.ApotekaID });
        }
        public string ProcessUploadedFile(IFormFile Photo)
        {
            string uniqueFileName = null;
            if (Photo != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
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

