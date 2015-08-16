using Anketa_Proekt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Anketa_Proekt.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/
        public Anketa_Proekt.Models.Louse MainLayoutViewModel { get; set; }

        public UserController()
        {
            this.MainLayoutViewModel = new Anketa_Proekt.Models.Louse();//has property PageTitle
            //this.MainLayoutViewModel. = "my title";

            this.ViewData["MainLayoutViewModel"] = this.MainLayoutViewModel;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogIn(Anketa_Proekt.Models.Louse user)
        {
            //if (ModelState.IsValid)
            //{
                if (user.isValidUser(user.e_mail, user.lozinka))
                {
                    //FormsAuthentication.SetAuthCookie(user.e_mail, true);
                    //FormsAuthentication.SetAuthCookie()
                    Session["id_lice"] = user.id_lice;
                    return RedirectToAction("Index", "Anketa");
                }
                else
                {
                    ModelState.AddModelError("", "Login data is incorrect!");
                }
            //}
            return View(user);
        }

        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registration(Anketa_Proekt.Models.Louse user)
        {
            if (ModelState.IsValid)
            {
                using (var db = new AnketiEntities3())
                {
                    var newUser = db.Lice.Create();

                    newUser.ime = user.ime;
                    newUser.prezime = user.prezime;
                    newUser.e_mail = user.e_mail;
                    newUser.lozinka = user.lozinka;
                    newUser.tel_broj = user.tel_broj;
                    newUser.ulica = user.ulica;
                    newUser.grad = user.grad;
                    newUser.datum_r = user.datum_r;

                    db.Lice.Add(newUser);

                    db.SaveChanges();

                    Session["id_lice"] = newUser.id_lice;

                    return RedirectToAction("Index", "Anketa");
                }
            }
            else
            {
                ModelState.AddModelError("", "The data that you Entered is incorrect!");
            }

            return View();
        }

        public ActionResult LogOut()
        {
            if (Session["id_lice"] != null)
            {
                Session["id_lice"] = null;
                return RedirectToAction("Index", "Anketa");
            }

            return View();
        }
	}
}