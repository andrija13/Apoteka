using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Neo4j.AspNetCore.Identity;
using Neo4jClient.DataAnnotations;
using Apoteka.Models;
using Neo4jClient;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using NugetJObject;
using Newtonsoft.Json.Linq;

namespace Apoteka.Pages
{
    public class ProfilProizvodaModel : PageModel
    {
        [BindProperty]
        public Proizvod Proizvod { get; set; }
        [BindProperty]
        public IList<Ima> Imas { get; set; }
        [BindProperty]
        public IList<ApotekaModel> Apoteke { get; set; }
        [BindProperty]
        public IList<Lokacija> Lokacije { get; set; }
        [BindProperty]
        public IDictionary<Lokacija,string> Cene { get; set; }

        private readonly AnnotationsContext _context;
        public ProfilProizvodaModel(AnnotationsContext context)
        {
            _context = context;
            Apoteke = new List<ApotekaModel>();
            Lokacije = new List<Lokacija>();
            Cene= new Dictionary<Lokacija,string>();
            Imas = new List<Ima>();

        }

        public void OnGet(string id) 
        {
            //match(n: Apoteka) -[:SE_NALAZI_U]->(l: Lokacija) -[:IMA]->(p: Proizvod{ ID: 187}) return p
            var proizvod = _context.GraphClient.Cypher.Match("(p: Proizvod{ ID:" + id + "}) ").Return(p => p.As<Node<string>>());
            var rezp = proizvod.Results.Single();
            Proizvod = JsonConvert.DeserializeObject<Proizvod>(rezp.Data);

            var apoteka =_context.GraphClient.Cypher.Match("(n:Apoteka) -[:SE_NALAZI_U]->(l: Lokacija) -[:IMA]->(p: Proizvod{ ID:"+id+"}) ").Return(n => n.As<Node<string>>());
            var rez = apoteka.Results;
            Apoteke=rez.Select(node => JsonConvert.DeserializeObject<ApotekaModel>(node.Data)).ToList();
            foreach(ApotekaModel ap in Apoteke)
            {
                
                var lokacija = _context.GraphClient.Cypher.Match("(n:Apoteka{ApotekaID:"+ap.ApotekaID+"}) -[:SE_NALAZI_U]->(l: Lokacija) -[:IMA]->(p: Proizvod{ ID:" + id + "}) ").Return(l => l.As<Node<string>>());
                var rezultat = lokacija.Results;
                Lokacije = rezultat.Select(node => JsonConvert.DeserializeObject<Lokacija>(node.Data)).ToList();

                foreach(Lokacija lok in Lokacije)
                {
                    Ima i = new Ima();
                    i.ApootekaVeza = ap;
                    i.LokacijaVeza = lok;

                    var cena = _context.GraphClient.Cypher.Match("(n:Apoteka) -[:SE_NALAZI_U]->(l: Lokacija{ID:"+lok.ID+"}) -[r:IMA]->(p: Proizvod{ ID:" + id + "}) ").Return(r => r.As<Node<string>>());
                    var c = cena.Results.Single();
                    string idv = c.Reference.Id.ToString();
                    i.Cena= JObject.Parse(c.Data)["Cena"].ToString();
                    i.ProizvodVeza = Proizvod;


                    Imas.Add(i);
                }

            }
               


        }
    }
}
