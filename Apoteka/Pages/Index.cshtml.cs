using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Apoteka.Models;
using Neo4jClient;
using Neo4jClient.Cypher;
using Neo4j.Driver;
using Neo4j.AspNetCore.Identity;
using Neo4jClient.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apoteka.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AnnotationsContext _context;
        private IList<Proizvod> Proizvodi { get; set; }
        private IList<string> Ids { get; set; }
        [BindProperty]
        public IList<Proizvod> ProizvodiPretraga { get; set; }
        [BindProperty]
        public IList<Ima> Ima { get; set; }
        [BindProperty]
        public IList<Ima> PretragaIma { get; set; }
        [BindProperty]
        public List<SelectListItem> GradoviFilter { get; set; }
        [BindProperty]
        public List<SelectListItem> SortirajPo { get; set; }
        IEnumerable<Ima> query;

        [BindProperty(SupportsGet = true)]
        public string FilterNaziv { get; set; }
        [BindProperty(SupportsGet = true)]
        public string GradFilter { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SortirajPoFilter { get; set; }
        public IndexModel(ILogger<IndexModel> logger, AnnotationsContext context)
        {
            _context = context;
            _logger = logger;
            Proizvodi = new List<Proizvod>();
            Ids = new List<string>();
            ProizvodiPretraga = new List<Proizvod>();
            GradoviFilter = new List<SelectListItem>();
            Ima = new List<Ima>();
            PretragaIma = new List<Ima>();

            List<string> SviGradovi = new List<string>();
            var svigradovi = _context.GraphClient.Cypher.Match("(p:Lokacija)").Return(p => p.As<Node<string>>());
            var rezul = svigradovi.Results;
            SviGradovi = rezul.Select(node => JsonConvert.DeserializeObject<Lokacija>(node.Data)).Select(grad => grad.Grad).Distinct().ToList();

            foreach(var grad in SviGradovi)
            {
                SelectListItem item = new SelectListItem { Text = grad, Value = grad };
                GradoviFilter.Add(item);
            }

            SortirajPo = new List<SelectListItem>
            {
                new SelectListItem { Text="Prvo najjeftinije", Value = "Najjeftinije"},
                new SelectListItem { Text="Prvo najskuplje", Value = "Najskuplje"}
            };
        }

        public async Task OnGetAsync()
        {
            var proizvod = _context.GraphClient.Cypher.Match("(p:Proizvod)").Return(p => p.As<Node<string>>());
            var rezul = proizvod.Results;
            Proizvodi = rezul.Select(node => JsonConvert.DeserializeObject<Proizvod>(node.Data)).ToList();

            if (Proizvodi.Count == 0)
            {
                _context.GraphClient.Cypher.Create("(n:Proizvod {ID:'' ,Naziv:'IBUPROFEN',Kategorija:'IBUPROFEN', Opis:'film tabl. 30x600mg',Proizvodjac:'Hemofarm',Slika:'ibuprofen-400-mg-67.png'})").ExecuteWithoutResults();
                _context.GraphClient.Cypher.Create("(n:Proizvod {ID:'' ,Naziv:'BRUFEN',Kategorija:'IBUPROFEN', Opis:'film tabl. 30x400mg',Proizvodjac:'Hemofarm',Slika:'brufen1.jpg'})").ExecuteWithoutResults();
                _context.GraphClient.Cypher.Create("(n:Proizvod {ID:'' ,Naziv:'IBUPROFEN',Kategorija:'IBUPROFEN', Opis:'film tabl. 30x200mg',Proizvodjac:'Hemofarm',Slika:'ibuprofen-400-mg-67.png'})").ExecuteWithoutResults();
                _context.GraphClient.Cypher.Create("(n:Proizvod {ID:'' ,Naziv:'DEFRINOL FORTE',Kategorija:'PSEUDOFEDRIN', Opis:'film tabl. 20x(200mg+30mg)',Proizvodjac:'Galenika',Slika:'defrinol-forte.jpg'})").ExecuteWithoutResults();
                _context.GraphClient.Cypher.Create("(n:Proizvod {ID:'' ,Naziv:'RINASEK',Kategorija:'PSEUDOFEDRIN', Opis:'sirup 100ml (100mg+30mg)/5ml',Proizvodjac:'Hemofarm',Slika:'unnamed.jpg'})").ExecuteWithoutResults();
                _context.GraphClient.Cypher.Create("(n:Proizvod {ID:'' ,Naziv:'OPERIL',Kategorija:'KAPI ZA NOS', Opis:'kapi 0.05% 10ml',Proizvodjac:'Lek',Slika:'operil-kapi-za-nos-za-odrasle-10ml-640x640.jpg'})").ExecuteWithoutResults();
                _context.GraphClient.Cypher.Create("(n:Proizvod {ID:'' ,Naziv:'MARISOL',Kategorija:'KAPI ZA NOS', Opis:'kapi 2.2% 50ml',Proizvodjac:'Esensa',Slika:'marisol-imuno-600x600.jpg'})").ExecuteWithoutResults();
                _context.GraphClient.Cypher.Create("(n:Proizvod {ID:'' ,Naziv:'JSP FFP2 325 TYPHOON ',Kategorija:'ZASTITNE MASKE', Opis:'FFP2 325 TYPHOON ',Proizvodjac:'JSP',Slika:'maska-disajni-organi-zastita-disanje-jsp-typhoon-zdj-325.jpg'})").ExecuteWithoutResults();
                proizvod = _context.GraphClient.Cypher.Match("(p:Proizvod)").Return(p => p.As<Node<string>>());
                rezul = proizvod.Results;
                Proizvodi = rezul.Select(node => JsonConvert.DeserializeObject<Proizvod>(node.Data)).ToList();
            }

            Ids = rezul.Select(node => JsonConvert.DeserializeObject<string>(node.Reference.Id.ToString())).ToList();
            for (int i = 0; i < Proizvodi.Count; i++)
            {
                Proizvodi[i].ID = Ids[i];
                await _context.GraphClient.Cypher.Match("(n:Proizvod { Naziv:'" + Proizvodi[i].Naziv + "', Proizvodjac:'" + Proizvodi[i].Proizvodjac + "',Opis:'" + Proizvodi[i].Opis + "' })").Set("n.ID = " + Ids[i]).ExecuteWithoutResultsAsync();

            }

            List<Lokacija> SveLokacije = new List<Lokacija>();
            var lokacija = _context.GraphClient.Cypher.Match("(l:Lokacija)-[:IMA]->(p:Proizvod)").Return(l => l.As<Node<string>>());
            var rezultat = lokacija.Results.Distinct();
            SveLokacije = rezultat.Select(node => JsonConvert.DeserializeObject<Lokacija>(node.Data)).ToList();

            //PRETRAGA

            int brojac = 0;
            foreach (Lokacija lok in SveLokacije)
            {
                List<Proizvod> pomProizvod = new List<Proizvod>();
                var imaProizvode = _context.GraphClient.Cypher.Match("(l:Lokacija { ID: " + lok.ID + "})-[:IMA]->(p:Proizvod)").Return(p => p.As<Node<string>>());
                var provera = imaProizvode.Results;
                pomProizvod = provera.Select(node => JsonConvert.DeserializeObject<Proizvod>(node.Data)).ToList();

                var qapoteka = _context.GraphClient.Cypher.Match("(l:Lokacija { ID: " + lok.ID + "})<-[:SE_NALAZI_U]->(a:Apoteka)").Return(a => a.As<Node<string>>());
                var rez = qapoteka.Results.Single();
                var apoteka = JsonConvert.DeserializeObject<ApotekaModel>(rez.Data);

                foreach (Proizvod p in pomProizvod)
                {
                    Ima i = new Ima();
                    i.LokacijaVeza = lok;
                    i.ApootekaVeza = apoteka;
                    var cenequery = _context.GraphClient.Cypher.Match("(p:Proizvod { ID:" + p.ID + "})<-[r:IMA]-(l:Lokacija {ID :" + lok.ID + "})").Return(r => r.As<Node<string>>());
                    var c = cenequery.Results.Single();
                    string idv = c.Reference.Id.ToString();
                    string cena = JObject.Parse(c.Data)["Cena"].ToString();
                    i.ProizvodVeza = p;
                    i.Cena = cena;
                    i.VezaID = idv;
                    Ima.Add(i);
                    brojac++;
                }
            }

            if (Ima.Count > 0)
            {

                query = Ima.Select(x => x).OrderBy(x => x.Cena);

                if (!string.IsNullOrEmpty(SortirajPoFilter))
                {
                    if (SortirajPoFilter == "Najjeftinije")
                    {
                        query = Ima.Select(x => x).OrderBy(x => x.Cena);
                    }
                    if (SortirajPoFilter == "Najskuplje")
                    {
                        query = Ima.Select(x => x).OrderByDescending(x => x.Cena);
                    }
                }

                if (!string.IsNullOrEmpty(GradFilter))
                {
                    query = Ima.Select(x => x).Where(x => x.LokacijaVeza.Grad == GradFilter);
                }

                if (!string.IsNullOrEmpty(FilterNaziv))
                {
                    query = Ima.Select(x => x).Where(x => x.ProizvodVeza.Naziv.StartsWith(FilterNaziv.ToUpper()));
                }

                if (!string.IsNullOrEmpty(FilterNaziv) && !string.IsNullOrEmpty(GradFilter))
                {
                    query = Ima.Select(x => x).Where(x => x.ProizvodVeza.Naziv.StartsWith(FilterNaziv.ToUpper()) && x.LokacijaVeza.Grad == GradFilter);
                }

                if (!string.IsNullOrEmpty(FilterNaziv) && !string.IsNullOrEmpty(SortirajPoFilter))
                {
                    if (SortirajPoFilter == "Najjeftinije")
                    {
                        query = Ima.Select(x => x).Where(x => x.ProizvodVeza.Naziv.StartsWith(FilterNaziv.ToUpper())).OrderBy(x => x.Cena);
                    }
                    if (SortirajPoFilter == "Najskuplje")
                    {
                        query = Ima.Select(x => x).Where(x => x.ProizvodVeza.Naziv.StartsWith(FilterNaziv.ToUpper())).OrderByDescending(x => x.Cena);
                    }
                }

                if (!string.IsNullOrEmpty(GradFilter) && !string.IsNullOrEmpty(SortirajPoFilter))
                {
                    if (SortirajPoFilter == "Najjeftinije")
                    {
                        query = Ima.Select(x => x).Where(x => x.LokacijaVeza.Grad == GradFilter).OrderBy(x => x.Cena);
                    }
                    if (SortirajPoFilter == "Najskuplje")
                    {
                        query = Ima.Select(x => x).Where(x => x.LokacijaVeza.Grad == GradFilter).OrderByDescending(x => x.Cena);
                    }
                }

                if (!string.IsNullOrEmpty(FilterNaziv) && !string.IsNullOrEmpty(GradFilter) && !string.IsNullOrEmpty(SortirajPoFilter))
                {
                    if (SortirajPoFilter == "Najjeftinije")
                    {
                        query = Ima.Select(x => x).Where(x => x.LokacijaVeza.Grad == GradFilter && x.ProizvodVeza.Naziv.StartsWith(FilterNaziv.ToUpper())).OrderBy(x => x.Cena);
                    }
                    if (SortirajPoFilter == "Najskuplje")
                    {
                        query = Ima.Select(x => x).Where(x => x.LokacijaVeza.Grad == GradFilter && x.ProizvodVeza.Naziv.StartsWith(FilterNaziv.ToUpper())).OrderByDescending(x => x.Cena);
                    }
                }

                query = LinqHelper.DistinctBy(query, x => new { x.ProizvodVeza.Naziv, x.ProizvodVeza.Proizvodjac });
                PretragaIma = query.ToList();

            }
        }

    }

    public static class LinqHelper
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }

}