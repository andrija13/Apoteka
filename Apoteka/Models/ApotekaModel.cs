﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Apoteka.Models
{
    public class ApotekaModel
    {
        public string ApotekaID { get; set; }
        public string Naziv { get; set; }
        public string Email { get; set; }
        public string Direktor { get; set; }
        public string BrojTelefona { get; set; }
        public string Slika { get; set; }
    }
}
