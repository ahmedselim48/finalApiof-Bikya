﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.DTOs.CategoryDTOs
{
    public class UpdateCategoryDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public string? IconUrl { get; set; }

        public int? ParentCategoryId { get; set; }

    }
}
