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

namespace Apoteka.Pages
{
    public class ProfilApotekeModel : PageModel
    { 
        [BindProperty]
        public ApotekaModel Apoteka { get; set; }
        [BindProperty]
        public string IzabraniProizvod { get; set; }
        [BindProperty]
        public IList<Lokacija> Lokacije{ get; set; }
        [BindProperty]
        public IList<string> NoviGradovi { get; set; }
        [BindProperty]
        public IList<string> NoveAdrese { get; set; }
        [BindProperty]
        public IList<Proizvod> IzabraniProizvodi { get; set; }
        [BindProperty]
        public IList<Proizvod> Proizvodi { get; set; }
        [BindProperty]
        public IList<string> NoviBrojevi { get; set; }
        [BindProperty]
        public List<SelectListItem> SviProizvodi{ get; set; }
        [BindProperty]
        public string Cena { get; set; }
        [BindProperty]
        public IList<int> Check_Grad { get; set; }

        private readonly  AnnotationsContext _context;
        public ProfilApotekeModel(AnnotationsContext context)
        {
            _context = context;
            Lokacije = new List<Lokacija>();
            NoviGradovi = new List<string>();
            NoveAdrese = new List<string>();
            NoviBrojevi = new List<string>();
            IzabraniProizvodi = new List<Proizvod>();
            Proizvodi = new List<Proizvod>();
            SviProizvodi = new List<SelectListItem>();
            Check_Grad = new List<int>();
        }
        public async Task OnGetAsync(string id)
        {
            var apoteka = _context.GraphClient.Cypher.Match("(n:Apoteka)").Where("n.ApotekaID= " + id ).Return(n => n.As<Node<string>>());
            var rez = apoteka.Results.Single();

            ////List<ApotekaModel> ap = rez.Select(node => JsonConvert.DeserializeObject<ApotekaModel>(node.Data)).ToList();
            Apoteka= JsonConvert.DeserializeObject<ApotekaModel>(rez.Data);

            //            MATCH(n: Apoteka { ApotekaID: 1})-[:SE_NALAZI_U]->(l: Lokacija)
            //RETURN l;

            var lokacija= _context.GraphClient.Cypher.Match("(n:Apoteka {ApotekaID:"+id+"})-[:SE_NALAZI_U]->(l: Lokacija)").Return(l => l.As<Node<string>>());
            var rezultat = lokacija.Results;

            Lokacije =rezultat.Select(node => JsonConvert.DeserializeObject<Lokacija>(node.Data)).ToList();

            var proizvod= _context.GraphClient.Cypher.Match("(p:Proizvod)").Return(p => p.As<Node<string>>());
            var rezul = proizvod.Results;
            Proizvodi = rezul.Select(node => JsonConvert.DeserializeObject<Proizvod>(node.Data)).ToList();
            
            foreach(Proizvod pro in Proizvodi)
            {
                SelectListItem p=new SelectListItem();
                p.Text = pro.Naziv+" "+pro.Opis;
                p.Value = pro.ID;
                SviProizvodi.Add(p);
            }    


        }
        public async Task<IActionResult> OnPostSacuvajApotekuAsync()
        {
            await _context.GraphClient.Cypher.Match("(n:Apoteka)").Where("n.ApotekaID = " + Apoteka.ApotekaID).Set("n = { ApotekaID:"+Apoteka.ApotekaID+", Naziv: '" + Apoteka.Naziv+"', Email:'"+Apoteka.Email+"', Direktor: '"+Apoteka.Direktor+"', BrojTelefona: '"+Apoteka.BrojTelefona+"'}").ExecuteWithoutResultsAsync();
            //for(int i = 0; i < Lokacije.Count; i++)
            //{
            //    Lokacija lok = Lokacije[i];
            //    await _context.GraphClient.Cypher.Match("(l:Lokacija)").Where("l.ID=" + lok.ID).Set("l = { ID:"+lok.ID+", Grad:'" + lok.Grad + "', UlicaBr:'" + lok.UlicaBr + "', BrojTelefona:'" + lok.BrojTelefona+"'}").ExecuteWithoutResultsAsync();
            //}
            if (NoviGradovi.Count > 0)
            {
                for (var clan = 0; clan < NoviGradovi.Count; clan++)
                {
                    if (NoviGradovi[clan] != "" && NoveAdrese[clan] != "")
                    {
                        Lokacija lok = new Lokacija();
                        lok.Grad = NoviGradovi[clan];
                        lok.UlicaBr = NoveAdrese[clan];
                        lok.BrojTelefona =NoviBrojevi[clan];

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

            return RedirectToPage("ProfilApoteke", new { id = Apoteka.ApotekaID });
        }
        public async Task<IActionResult> OnPostUkloniLokaciju(string idlok)
        {
            await _context.GraphClient.Cypher.Match("(n:Lokacija)").Where("n.ID= " + idlok).DetachDelete("n").ExecuteWithoutResultsAsync();

            return RedirectToPage("ProfilApoteke", new { id = Apoteka.ApotekaID });
        }
        public async Task<IActionResult> OnPostUkloniApoteku()
        {
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
    }
}
