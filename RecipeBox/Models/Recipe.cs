using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace RecipeBox.Models
{
    public class Recipe
    {
        public Recipe()
        {
            this.Tags = new HashSet<RecipeTag>();
        }

        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string Instructions { get; set; }
        public string Ingredient { get; set; }
        public virtual ApplicationUser User { get; set; }

        public ICollection<RecipeTag> Tags { get; }

    }
}