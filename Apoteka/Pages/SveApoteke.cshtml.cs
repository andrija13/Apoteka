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

namespace Apoteka.Pages
{
    public class SveApotekeModel : PageModel
    {
        private readonly AnnotationsContext _context;
        [BindProperty]
        public IList<ApotekaModel> Apoteke { get; set; }

        public SveApotekeModel(AnnotationsContext context)
        {
            _context = context;
            Apoteke = new List<ApotekaModel>();
        }
        public void OnGet()
        {
            var apoteke = _context.GraphClient.Cypher.Match("(a:Apoteka)").Return(a => a.As<Node<string>>());
            var rez = apoteke.Results;
            Apoteke = rez.Select(node => JsonConvert.DeserializeObject<ApotekaModel>(node.Data)).ToList();
        }
    }
}
