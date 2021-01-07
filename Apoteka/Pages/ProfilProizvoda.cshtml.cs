using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Neo4j.AspNetCore.Identity;
using Neo4jClient.DataAnnotations;
using Apoteka.Models;

namespace Apoteka.Pages
{
    public class ProfilProizvodaModel : PageModel
    {
        [BindProperty]
        public Proizvod Proizvod { get; set; }
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

        }
    }
}
