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
            //if (user.isValidUser(user.e_mail, user.lozinka))
            //{
            //FormsAuthentication.SetAuthCookie(user.e_mail, true);
            //FormsAuthentication.SetAuthCookie()
            using (var db = new AnketiEntities5())
            {
                var query = from a in db.Lice
                            where a.e_mail.Equals(user.e_mail) & a.lozinka.Equals(user.lozinka)
                            select a.id_lice;

                int idLice = query.FirstOrDefault();

                if (idLice > 0)
                {
                    Session["id_lice"] = idLice;
                    return RedirectToAction("Index", "Anketa");
                }
                else
                {
                    ModelState.AddModelError("", "Login data is incorrect!");
                }
            }
            //}
            //else
            //{
            //ModelState.AddModelError("", "Login data is incorrect!");
            //}
            //}
            return View(user);
        }

        [HttpGet]
        public ActionResult Registration()
        {
            using (var db = new AnketiEntities5())
            {
                List<SelectListItem> listSelectListItems = new List<SelectListItem>();

                foreach (Grad city in db.Grads)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = city.ime_grad,
                        Value = city.id_grad.ToString()
                    };

                    listSelectListItems.Add(selectList);
                }

                ViewBag.MyGradovi = listSelectListItems;

                return View();
            }
        }

        [HttpPost]
        public ActionResult Registration(Anketa_Proekt.Models.Louse user)
        {
            using (var db = new AnketiEntities5())
            {
                List<SelectListItem> listSelectListItems = new List<SelectListItem>();

                foreach (Grad city in db.Grads)
                {
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = city.ime_grad,
                        Value = city.id_grad.ToString(),
                        //Selected = false
                    };

                    listSelectListItems.Add(selectList);
                }

                ViewBag.MyGradovi = listSelectListItems;

                //return View();
            }

            if (ModelState.IsValid)
            {
                using (var db = new AnketiEntities5())
                {
                    var newUser = db.Lice.Create();

                    newUser.ime = user.ime;
                    newUser.prezime = user.prezime;
                    newUser.e_mail = user.e_mail;
                    newUser.lozinka = user.lozinka;
                    newUser.tel_broj = user.tel_broj;
                    newUser.ulica = user.ulica;
                    newUser.id_grad = user.id_grad;
                    newUser.datum_r = user.datum_r;

                    db.Lice.Add(newUser);

                    db.SaveChanges();

                    Session["id_lice"] = newUser.id_lice;

                    var korisnik = db.Korisniks.Create();

                    korisnik.id_lice = newUser.id_lice;
                    korisnik.br_anketi = 0;

                    db.Korisniks.Add(korisnik);

                    db.SaveChanges();

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

        public ActionResult CheckStatus()
        {
            if (Session["id_lice"] != null)
            {
                using (var db = new AnketiEntities5())
                {
                    Korisnik korisnik = db.Korisniks.Find((int)Session["id_lice"]);

                    Premium_Korisnik premiumKorsinik = db.Premium_Korisnik.Find((int)Session["id_lice"]);

                    Louse lice = db.Lice.Find((int)Session["id_lice"]);

                    if (premiumKorsinik != null)
                    {
                        ViewBag.PremiumUserStartDate = premiumKorsinik.datum_starts;
                        ViewBag.PremiumUserEndDate = premiumKorsinik.datum_end;

                        ViewBag.UserName = lice.ime;
                        ViewBag.UserLastName = lice.prezime;

                        return View(korisnik);
                    }
                    else
                    {
                        ViewBag.UserName = lice.ime;
                        ViewBag.UserLastName = lice.prezime;

                        return View(korisnik);
                    }
                }
            }
            else
            {
                return RedirectToAction("Index", "Anketa");
            }
        }

        public ActionResult BuyPremium()
        {
            if (Session["id_lice"] != null)
            {
                using (var db = new AnketiEntities5())
                {
                    Premium_Korisnik premiumKorisnik = db.Premium_Korisnik.Find((int)Session["id_lice"]);

                    if (premiumKorisnik == null)
                    {
                        return View(premiumKorisnik);
                    }
                    else
                    {
                        return RedirectToAction("CheckStatus", "User");
                    }
                }
            }
            else
            {
                return RedirectToAction("Index", "Anketa");
            }
        }

        [HttpPost]
        public ActionResult BuyPremium(Premium_Korisnik premiumKorsinik)
        {
            if (Session["id_lice"] != null)
            {
                using (var db = new AnketiEntities5())
                {
                    Premium_Korisnik premiumUser = db.Premium_Korisnik.Create();

                    premiumUser.id_lice = (int)Session["id_lice"];
                    premiumUser.datum_starts = DateTime.Today;
                    premiumUser.datum_end = DateTime.Today.AddMonths(12);

                    db.Premium_Korisnik.Add(premiumUser);

                    db.SaveChanges();

                    return RedirectToAction("CheckStatus", "User");
                }
            }
            else
            {
                return RedirectToAction("Index", "Anketa");
            }

            return View();
        }
    }
}