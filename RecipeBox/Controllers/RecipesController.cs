using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using RecipeBox.Models;


namespace RecipeBox.Controllers
{
    public class RecipesController : Controller
    {
        private readonly RecipeBoxContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecipesController(UserManager<ApplicationUser> userManager, RecipeBoxContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public ActionResult Index()
        {
            var allUserRecipes = _db.Recipes.ToList();
            return View(allUserRecipes);
        }

        [Authorize]
        public ActionResult Create()
        {
            ViewBag.TagId = new SelectList(_db.Tags, "TagId", "TagName");
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Recipe recipe, int TagId)
        {
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _userManager.FindByIdAsync(userId);
            recipe.User = currentUser;
            _db.Recipes.Add(recipe);
            if (TagId != 0)
            {
                _db.RecipeTag.Add(new RecipeTag() { TagId = TagId, RecipeId = recipe.RecipeId });
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        // In the Details route we need to find the user associated with the item so that in the view, we can show the edit, delete or add category links if the item "belongs" to that user. Line 62 involves checking if the userId has returned as null, and if it has then IsCurrentUser is set to false, if it has not, then the program evaluates whether userId is equal to thisItem.User.Id.
        //Line 63 if user id is not null compare userId to thisRecipe's userId and if true return true, if userId is null return false
        public ActionResult Details(int id)
        {
            var thisRecipe = _db.Recipes
                .Include(recipe => recipe.Tags)
                .ThenInclude(join => join.Tag)
                .Include(recipe => recipe.User)
                .FirstOrDefault(recipe => recipe.RecipeId == id);
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.IsCurrentUser = userId != null ? userId == thisRecipe.User.Id : false; //
                                                                                           // if(userId != null) if userId not null (condition ? consequent : alternative)
                                                                                           // {
                                                                                           //     if(userId == thisRecipe.User.Id) does this userId equal this recipes user id if true
                                                                                           //     {
                                                                                           //        
                                                                                           //     }
                                                                                           // }
                                                                                           // else
                                                                                           // {
                                                                                           //     ViewBat.IsCurrentUser = false
                                                                                           // }
            return View(thisRecipe);
        }

        [Authorize]
        public async Task<ActionResult> Edit(int id)
        {
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _userManager.FindByIdAsync(userId);
            var thisRecipe = _db.Recipes.Where(entry => entry.User.Id == currentUser.Id).FirstOrDefault(recipes => recipes.RecipeId == id);
            if (thisRecipe == null)
            {
                return RedirectToAction("Details", new { id = id });
            }
            ViewBag.TagId = new SelectList(_db.Tags, "TagId", "TagName");
            return View(thisRecipe);
        }

        [HttpPost]
        public ActionResult Edit(Recipe recipe, int TagId)
        {
            if (TagId != 0)
            {
                var returnedJoined = _db.RecipeTag //for preventing duplicate tags
                .Any(join => join.RecipeId == recipe.RecipeId && join.TagId == TagId);
                if (!returnedJoined)
                {
                    _db.RecipeTag.Add(new RecipeTag() { TagId = TagId, RecipeId = recipe.RecipeId });
                }
            }
            _db.Entry(recipe).State = EntityState.Modified;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<ActionResult> AddTag(int id)
        {
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _userManager.FindByIdAsync(userId);
            Recipe thisRecipe = _db.Recipes.Where(entry => entry.User.Id == currentUser.Id).FirstOrDefault(recipes => recipes.RecipeId == id);
            if (thisRecipe == null)
            {
                return RedirectToAction("Details", new { id = id });
            }
            ViewBag.TagId = new SelectList(_db.Tags, "TagId", "TagName");
            return View(thisRecipe);
        }

        [HttpPost]
        public ActionResult AddTag(Recipe recipe, int TagId)
        {
            if (TagId != 0)
            {
                _db.RecipeTag.Add(new RecipeTag() { TagId = TagId, RecipeId = recipe.RecipeId });
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _userManager.FindByIdAsync(userId);
            Recipe thisRecipe = _db.Recipes.Where(entry => entry.User.Id == currentUser.Id).FirstOrDefault(recipes => recipes.RecipeId == id);
            if (thisRecipe == null)
            {
                return RedirectToAction("Details", new { id = id });
            }
            return View(thisRecipe);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var thisRecipe = _db.Recipes.FirstOrDefault(recipes => recipes.RecipeId == id);
            _db.Recipes.Remove(thisRecipe);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteTag(int joinId)
        {
            var joinEntry = _db.RecipeTag.FirstOrDefault(entry => entry.RecipeTagId == joinId);
            _db.RecipeTag.Remove(joinEntry);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        // Search all recipes in db
        public ActionResult Search(string search)
        {
            List<Recipe> model = _db.Recipes.Where(recipe => (recipe.Ingredient.Contains(search))).ToList();
            return View(model);
        }

        // [HttpPost]
        // public async Task<ActionResult> Search(string search)
        // {
        //     var thisUserId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //     var thisCurrentUser = await _userManager.FindByIdAsync(thisUserId);
        //     var userRecipes = _db.Recipes.Where(entry => entry.User.Id == thisCurrentUser.Id);
        //     var searchedUserRecipes = userRecipes.Where(recipe => (recipe.Ingredient.Contains(search))).ToList();
        //     return View(searchedUserRecipes);
        // }
    }
}