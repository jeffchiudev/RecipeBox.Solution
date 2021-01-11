using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


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
        [DataType(DataType.MultilineText)]
        public string Instructions { get; set; }
        [DataType(DataType.MultilineText)]
        public string Ingredient { get; set; }
        public virtual ApplicationUser User { get; set; }

        public ICollection<RecipeTag> Tags { get; }

    }
}