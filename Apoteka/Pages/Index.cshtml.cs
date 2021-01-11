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

namespace Apoteka.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AnnotationsContext _context;
        private IList<Proizvod> Proizvodi { get; set; }
        private IList<string> Ids { get; set; }
        public IndexModel(ILogger<IndexModel> logger, AnnotationsContext context)
        {
            _context = context;
            _logger = logger;
            Proizvodi = new List<Proizvod>();
            Ids = new List<string>();
        }

        public async Task OnGetAsync()
        {
            var proizvod = _context.GraphClient.Cypher.Match("(p:Proizvod)").Return(p => p.As<Node<string>>());
            var rezul = proizvod.Results;
            Proizvodi = rezul.Select(node => JsonConvert.DeserializeObject<Proizvod>(node.Data)).ToList();

            if (Proizvodi.Count==0)
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
            for (int i=0;i<Proizvodi.Count;i++)
            {
                Proizvodi[i].ID = Ids[i];
                await _context.GraphClient.Cypher.Match("(n:Proizvod { Naziv:'" + Proizvodi[i].Naziv + "', Proizvodjac:'" + Proizvodi[i].Proizvodjac + "',Opis:'"+Proizvodi[i].Opis+"' })").Set("n.ID = " + Ids[i]).ExecuteWithoutResultsAsync();

            }


        }
    }
}