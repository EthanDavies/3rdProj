using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FinalYearProject.Models;
using Microsoft.AspNet.Identity;

namespace FinalYearProject.Controllers
{

    /// <summary>
    /// ToDoesController contains the methods required to create and view ToDo items in the applicaiton.
    /// </summary>
    public class ToDoesController : Controller
    {
        /// <summary>
        /// db contains is the application context for the current user. Used to add/edit/delete ToDo items for that user.
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ToDoes
        [Authorize]
        /// <summary>
        /// Index returns the index view attached to the method.
        /// </summary>
        /// <returns>returns the index view</returns>
        public ActionResult Index()
        {
            return View();
        }

        // GET: ToDoes/Details/5
        /// <summary>
        /// Details returns the details of a specific ToDo item based on the id passed.
        /// </summary>
        /// <param name="id">integer null or otherwise for a ToDo item,
        /// used for retrieving todo details
        /// </param>
        ///<returns>returns the controller methods view and passes the todo item to that view</returns>
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ToDo toDo = db.ToDos.Find(id);
            if (toDo == null)
            {
                return HttpNotFound();
            }
            return View(toDo);
        }

        /// <summary>
        /// GetMyToDoes is responsible for calculating the ToDo items that a user has,
        /// it also calculates the percentage of ToDo's that are completed
        /// </summary>
        /// <returns>returns an IEnumerable containing the users ToDo's</returns>
        private IEnumerable<ToDo> GetMyToDoes()
        {
            string currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.FirstOrDefault(
                x => x.Id == currentUserId);

            IEnumerable<ToDo> myToDoes = db.ToDos.ToList().Where(x => x.User == currentUser);

            int completeCount = 0;
            foreach (ToDo toDo in myToDoes)
            {
                if (toDo.IsDone)
                {
                    completeCount++;
                }
            }

            ViewBag.Percent = Math.Round(100f * ((float)completeCount / (float)myToDoes.Count()));

            return myToDoes;
        }

        /// <summary>
        /// BuildToDoTable is responsible for calling GetMyToDoes to retrieve the users ToDo items,
        /// then passes them to the partial view _ToDoTable
        /// </summary>
        /// <returns>this method returns a partial view that is used,
        /// to display the users todo items
        /// </returns>
        public ActionResult BuildToDoTable()
        {
            
            return PartialView("_ToDoTable", GetMyToDoes());
        }

        // GET: ToDoes/Create
        /// <summary>
        /// Create is the default create method that is generated
        /// </summary>
        /// <returns>returns the relevant create view</returns>
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ToDoes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// This method is used to create a ToDo item using the data sent from the view 
        /// </summary>
        /// <param name="toDo">This is the ToDo item that is passed from the view,
        /// it contains the data required to create the new ToDo item</param>
        /// <returns>returns the index view if the model state is deemed valid</returns>
        /// <returns>returns the create view with the ToDo item if the model state,
        /// is not deemed valid</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "Id,Description,IsDone")] ToDo toDo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string currentUserId = User.Identity.GetUserId();
                    ApplicationUser currentUser = db.Users.FirstOrDefault(
                        x => x.Id == currentUserId);
                    toDo.User = currentUser;
                    db.ToDos.Add(toDo);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }

            return View(toDo);
        }

        /// <summary>
        /// AJAXCreate is used to create ToDo items in the application through using AJAX,
        /// again a ToDo item is passed from the view and its values are set. Once created,
        /// a partial view of the updated table is returned.
        /// </summary>
        /// <param name="toDo">The todo item used to set the values for the newly added todo</param>
        /// <returns>returns the partial table view that is created using ajax</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult AJAXCreate([Bind(Include = "Id,Description")] ToDo toDo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string currentUserId = User.Identity.GetUserId();
                    ApplicationUser currentUser = db.Users.FirstOrDefault(
                        x => x.Id == currentUserId);
                    toDo.User = currentUser;
                    toDo.IsDone = false;
                    db.ToDos.Add(toDo);
                    db.SaveChanges();
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                }
    
            }

            return PartialView("_ToDoTable", GetMyToDoes());
        }

        // GET: ToDoes/Edit/5
        /// <summary>
        /// Edit allows users to edit the todo items they have entered
        /// </summary>
        /// <param name="id">Takes the id of the todo item that needs to be edited as a paramater</param>
        /// <returns>returns the edit view to allow the user to begin editing the todo item</returns>
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ToDo toDo = db.ToDos.Find(id);
            if (toDo == null)
            {
                return HttpNotFound();
            }

            string currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.FirstOrDefault(
                x => x.Id == currentUserId);
            if(toDo.User != currentUser)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(toDo);
        }

        // POST: ToDoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// This edit method is responisble for making the changes to the ToDo item in the database context,
        /// once this is done the user is redirected to the index to view the updated table
        /// </summary>
        /// <param name="toDo">The todo item that is required to be changed</param>
        /// <returns>returns the View containing the ToDo item</returns>
        /// <returns>returns a redirected action if the model state is valid</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "Id,Description,IsDone")] ToDo toDo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(toDo).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }
            return View(toDo);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AJAXEdit(int? id, bool value)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ToDo toDo = db.ToDos.Find(id);
            if (toDo == null)
            {
                return HttpNotFound();
            }
            else
            {
                try
                {
                    toDo.IsDone = value;
                    db.Entry(toDo).State = EntityState.Modified;
                    db.SaveChanges();
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
                return PartialView("_ToDoTable", GetMyToDoes());
            }
            
        }

        // GET: ToDoes/Delete/5
        /// <summary>
        /// Returns the view with the ToDo item that allows the user to delete a ToDo item from their list
        /// </summary>
        /// <param name="id">Id of the todo item in question</param>
        /// <returns>returns the delete view with the todo item to confirm the deletion</returns>
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ToDo toDo = db.ToDos.Find(id);
            if (toDo == null)
            {
                return HttpNotFound();
            }
            return View(toDo);
        }

        // POST: ToDoes/Delete/5
        /// <summary>
        /// Deletes the todo item from the relevant application context by finding it by id,
        /// redirects the user to the index view with updated table once complete
        /// </summary>
        /// <param name="id">The id of the todo item that is to be deleted</param>
        /// <returns>returns the index view showing the updated todo table</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                ToDo toDo = db.ToDos.Find(id);
                db.ToDos.Remove(toDo);
                db.SaveChanges();
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Dispose method used for garbage collection when using the ToDo table
        /// </summary>
        /// <param name="disposing">boolean to dictate if the disposing,
        /// is to take place or not</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
