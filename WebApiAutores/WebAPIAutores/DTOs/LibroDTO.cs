﻿using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using WebAPIAutores.Validations;

namespace WebAPIAutores.DTOs
{
    public class LibroDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}
