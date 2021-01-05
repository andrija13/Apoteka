using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4jClient;
using Neo4jClient.DataAnnotations;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4j.AspNetCore.Identity;

namespace Apoteka.Models
{
    public class Neo4JContext : AnnotationsContext
    {
        public Neo4JContext(IGraphClient graphClient) : base(graphClient)
        {
        }

        public Neo4JContext(IGraphClient graphClient, EntityService entityService) : base(graphClient, entityService)
        {
        }

        public Neo4JContext(IGraphClient graphClient, EntityResolver resolver) : base(graphClient, resolver)
        {
        }

        public Neo4JContext(IGraphClient graphClient, EntityConverter converter) : base(graphClient, converter)
        {
        }

        public Neo4JContext(IGraphClient graphClient, EntityResolver resolver, EntityService entityService) : base(graphClient, resolver, entityService)
        {
        }

        public Neo4JContext(IGraphClient graphClient, EntityConverter converter, EntityService entityService) : base(graphClient, converter, entityService)
        {
        }

        public EntitySet<Korisnik> Korisnici { get; set; }
        public EntitySet<IdentityRole> Uloge { get; set; }
    }
}
