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

namespace Apoteka.Pages
{
    public class ProfilApotekeModel : PageModel
    { 
        [BindProperty]
        public ApotekaModel Apoteka { get; set; }
        [BindProperty]
        public IList<Lokacija> Lokacije{ get; set; }
        private readonly  AnnotationsContext _context;
        public ProfilApotekeModel(AnnotationsContext context)
        {
            _context = context;
            Lokacije = new List<Lokacija>(); 
        }
        public void OnGet()
        {
        }
    }
}
