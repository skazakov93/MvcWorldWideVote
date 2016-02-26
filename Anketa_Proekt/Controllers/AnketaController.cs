using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Anketa_Proekt.Models;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Services;
using System.Web.Script.Services;
using PagedList;
using System.Web.Script.Serialization;

namespace Anketa_Proekt.Controllers
{
    public class AnketaController : Controller
    {
        private AnketiEntities5 db = new AnketiEntities5();

        // GET: /Anketa/
        public ActionResult Index(int? id)
        {
            int pageNumber = id ?? 1;

            DateTime denesenDatum = DateTime.Today;

            var anketas = db.Anketas.Include(a => a.Louse);
            anketas = anketas.Where(a => a.kraen_datum >= denesenDatum);
            anketas = anketas.OrderByDescending(a => a.datum_kreiranje);

            double pom = anketas.Count() / 5.0;
            int gornaGranica = (int)Math.Ceiling(pom);

            ViewBag.BrStrani = gornaGranica;
            ViewBag.CurrentPage = pageNumber;

            return View(anketas.ToPagedList(pageNumber, 5));
        }

        // GET: /Anketa/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Anketa anketa = db.Anketas.Find(id);
            if (anketa == null)
            {
                return HttpNotFound();
            }

            Korisnik korisnik = new Korisnik();
            Louse covek = new Louse();
            covek.ime = "Error";
            covek.prezime = "Fail";

            korisnik.Louse = covek;

            if (Session["id_lice"] != null)
            {
                korisnik = db.Korisniks.Find((int)Session["id_lice"]);
            }

            ViewBag.NajavenKorsinik = korisnik;

            return View(anketa);
        }

        public JsonResult DonatChartData(int? id)
        {
            Anketa anketa;

            List<Mozni_Odgovori> allUser;
            //Dictionary<string, int> mapa = new Dictionary<string, int>();

            var movies = new List<object>();
            using (var dc = new AnketiEntities5())
            {
                anketa = db.Anketas.Find(id);

                allUser = anketa.Mozni_Odgovori.ToList();

                foreach (Mozni_Odgovori odg in allUser)
                {
                    int brGlasovi = odg.Glasas.Count;

                    //mapa.Add(odg.ime_odg, brGlasovi);

                    movies.Add(new { Ime_odg = odg.ime_odg, Br_glasovi = brGlasovi });
                }
            }

            return Json(movies, JsonRequestBehavior.AllowGet);
        }

        public ActionResult zapisiMultiChoiceGlas()
        {
            string glasoviIds = Request.Params["odgovoriId"];
            string anketaId = Request.Params["anketa_id"];

            String[] elements = glasoviIds.Split(',');

            if (Session["id_lice"] != null)
            {
                if (glasoviIds.Length > 0)
                {
                    using (var db = new AnketiEntities5())
                    {
                        Anketa anketa = db.Anketas.Find(Convert.ToInt32(anketaId));

                        for (int i = 0; i < elements.Length; i++)
                        {
                            DateTime date = DateTime.Now;
                            string datum = date.ToString("yyyy-MM-dd HH:mm:ss");

                            Mozni_Odgovori odgovor = db.Mozni_Odgovori.Find(Convert.ToInt32(elements[i]));

                            if (anketa.Mozni_Odgovori.Contains(odgovor))
                            {

                                var novGlas = db.Glasas.Create();
                                novGlas.id_anketa = Convert.ToInt32(anketaId);
                                novGlas.id_lice = (int)Session["id_lice"];
                                novGlas.id_odg = Convert.ToInt32(elements[i]);
                                novGlas.datum_glasanje = Convert.ToDateTime(datum);

                                db.Glasas.Add(novGlas);
                            }
                        }

                        db.SaveChanges();
                    }

                    //return RedirectToAction("Details", "Anketa", new { id = anketaId });
                }
                else
                {
                    //return RedirectToAction("Details", "Anketa", new { id = anketaId });
                }
            }

            return View();
        }

        public ActionResult zapisiSingleChoiceGlas()
        {
            string glasId = Request.Params["odgovoriId"];
            string anketaId = Request.Params["anketa_id"];

            if (Session["id_lice"] != null)
            {
                if (glasId.Length > 0)
                {
                    using (var db = new AnketiEntities5())
                    {
                        Anketa anketa = db.Anketas.Find(Convert.ToInt32(anketaId));

                        DateTime date = DateTime.Now;
                        string datum = date.ToString("yyyy-MM-dd HH:mm:ss");

                        Mozni_Odgovori odgovor = db.Mozni_Odgovori.Find(Convert.ToInt32(glasId));

                        if (anketa.Mozni_Odgovori.Contains(odgovor))
                        {
                            var novGlas = db.Glasas.Create();
                            novGlas.id_anketa = Convert.ToInt32(anketaId);
                            novGlas.id_lice = (int)Session["id_lice"];
                            novGlas.id_odg = Convert.ToInt32(glasId);
                            novGlas.datum_glasanje = Convert.ToDateTime(datum);

                            db.Glasas.Add(novGlas);
                        }

                        db.SaveChanges();
                    }

                    //return RedirectToAction("Details", "Anketa", new { id = anketaId });
                }
                else
                {
                    //return View();
                }
            }
            return View();
        }

        public JsonResult GraphChartData(int? id)
        {
            Anketa anketa;

            List<Glasa> glasovi;

            var myGlas = new List<object>();
            using (var dc = new AnketiEntities5())
            {
                anketa = db.Anketas.Find(id);

                glasovi = anketa.Glasas.ToList();

                SortedDictionary<string, int> mapa = new SortedDictionary<string, int>();

                foreach (Glasa g in glasovi)
                {
                    string datum = g.datum_glasanje.ToString();

                    int start = datum.IndexOf(" ");
                    int end = datum.Length;

                    int delete = end - start;

                    string myDatum = datum.Remove(datum.IndexOf(" "), delete);

                    String[] elements = myDatum.Split('.');

                    string dateFinal = elements[2] + "-";
                    dateFinal += elements[1] + "-";
                    dateFinal += elements[0];

                    if (mapa.ContainsKey(dateFinal))
                    {
                        int mom = mapa[dateFinal];
                        mom++;

                        mapa[dateFinal] = mom;
                    }
                    else
                    {
                        mapa.Add(dateFinal, 1);
                    }
                }

                foreach (KeyValuePair<string, int> entry in mapa)
                {
                    string den = entry.Key;
                    int br_glasovi = entry.Value;

                    myGlas.Add(new { Den = den, Br_glasovi = br_glasovi });
                }
            }

            return Json(myGlas, JsonRequestBehavior.AllowGet);
        }

        // GET: /Anketa/Create
        public ActionResult Create()
        {
            if (Session["id_lice"] != null)
            {
                Anketa anketa = null;

                using (var db = new AnketiEntities5())
                {
                    List<SelectListItem> kategoriiLista = new List<SelectListItem>();

                    foreach (Kategorija k in db.Kategorijas)
                    {
                        SelectListItem selectList = new SelectListItem()
                        {
                            Text = k.ime_k,
                            Value = k.id_k.ToString()
                        };

                        kategoriiLista.Add(selectList);
                    }

                    ViewBag.MyKategorii = kategoriiLista;

                    ViewBag.id_lice = new SelectList(db.Lice, "id_lice", "ime");
                }

                anketa = new Anketa();

                return View(anketa);
            }

            return RedirectToAction("Index");
        }

        // POST: /Anketa/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        public ActionResult myCreate()
        {
            var myOdgovori = Request.Params["odgovori"];
            string myPrasanje = Request.Params["prasanje"];
            string myDesc = Request.Params["description"];
            string myDueDate = Request.Params["dueDate"];
            string myMultiChoice = Request.Params["multiChoice"];
            string myIdKategorii = Request.Params["idKategorii"];
            string url_slika = Request.Params["urlSlika"];

            String[] elements = myOdgovori.Split(',');

            String[] idKategorii = new string[1] { "kola" };
            bool zname = false;

            if (myIdKategorii.Length > 0)
            {
                idKategorii = myIdKategorii.Split(',');
                zname = true;
            }

            int multiChoice = 0;
            if (myMultiChoice.Equals("1"))
            {
                multiChoice = 1;
            }

            if (Session["id_lice"] != null)
            {
                using (var db = new AnketiEntities5())
                {
                    Korisnik korisnik = db.Korisniks.Find((int)Session["id_lice"]);
                    Premium_Korisnik premiumKorisnik = db.Premium_Korisnik.Find((int)Session["id_lice"]);

                    bool flag = true;

                    if (korisnik.br_anketi >= 3)
                    {
                        flag = false;
                    }

                    if (premiumKorisnik != null)
                    {
                        flag = true;
                    }

                    if (flag)
                    {

                        DateTime date = DateTime.Now;
                        string datum = date.ToString("yyyy-MM-dd HH:mm:ss");

                        var novaAnketa = db.Anketas.Create();
                        novaAnketa.prasanje = myPrasanje;
                        novaAnketa.opis_a = myDesc;
                        novaAnketa.kraen_datum = Convert.ToDateTime(myDueDate);
                        novaAnketa.id_lice = (int)Session["id_lice"];
                        novaAnketa.datum_kreiranje = Convert.ToDateTime(datum);
                        novaAnketa.multi_choice = multiChoice;

                        if (url_slika.Length > 1)
                        {
                            novaAnketa.url_slika = url_slika;
                        }

                        db.Anketas.Add(novaAnketa);

                        db.SaveChanges();

                        Anketa anketa = db.Anketas.Find(novaAnketa.id_anketa);

                        for (int i = 0; i < elements.Length; i++)
                        {
                            string str = elements[i];
                            str = str.Replace(";;;", ",");

                            Mozni_Odgovori odg = db.Mozni_Odgovori.Create();
                            odg.ime_odg = str;

                            db.Mozni_Odgovori.Add(odg);

                            db.SaveChanges();

                            anketa.Mozni_Odgovori.Add(odg);
                        }

                        db.SaveChanges();

                        if (zname)
                        {
                            for (int i = 0; i < idKategorii.Length; i++)
                            {
                                int id_k = Convert.ToInt32(idKategorii[i]);

                                Kategorija kategorija = db.Kategorijas.Find(id_k);

                                anketa.Kategorijas.Add(kategorija);

                                db.SaveChanges();
                            }

                            db.SaveChanges();
                        }

                        korisnik.br_anketi = korisnik.br_anketi + 1;
                        db.SaveChanges();

                        string strJson = "Your pool has been added. Thanks for your participation!!";

                        return Json(strJson);
                    }
                    else
                    {
                        string strJson = "You have already posted 3 pools. If you want to post more pools You have to upgrade to Premium User!";

                        return Json(strJson);
                    }
                }
            }

            string strJson2 = "You are Not Logged IN!!!";
            return Json(strJson2);
        }

        [HttpPost]
        public ActionResult myEdit()
        {
            var myOdgovori = Request.Params["odgovori"];
            string myPrasanje = Request.Params["prasanje"];
            string myDesc = Request.Params["description"];
            string myDueDate = Request.Params["dueDate"];
            string myMultiChoice = Request.Params["multiChoice"];
            string myIdKategorii = Request.Params["idKategorii"];
            string url_slika = Request.Params["urlSlika"];
            string idAnketaStr = Request.Params["idAnketa"];

            int idAnketa = Convert.ToInt32(idAnketaStr);

            String[] elements = myOdgovori.Split(',');

            String[] idKategorii = new string[1] { "kola" };
            bool zname = false;

            if (myIdKategorii.Length > 0)
            {
                idKategorii = myIdKategorii.Split(',');
                zname = true;
            }

            int multiChoice = 0;
            if (myMultiChoice.Equals("1"))
            {
                multiChoice = 1;
            }

            if (Session["id_lice"] != null)
            {
                using (var db = new AnketiEntities5())
                {
                    Korisnik korisnik = db.Korisniks.Find((int)Session["id_lice"]);
                    Premium_Korisnik premiumKorisnik = db.Premium_Korisnik.Find((int)Session["id_lice"]);

                    List<Anketa> anketiNaKorisnik = korisnik.Louse.Anketas.ToList();

                    Anketa anketaP = db.Anketas.Find(idAnketa);

                    if (anketiNaKorisnik.Contains(anketaP))
                    {

                        DateTime date = DateTime.Now;
                        string datum = date.ToString("yyyy-MM-dd HH:mm:ss");

                        var novaAnketa = db.Anketas.Find(idAnketa);
                        novaAnketa.prasanje = myPrasanje;
                        novaAnketa.opis_a = myDesc;
                        novaAnketa.kraen_datum = Convert.ToDateTime(myDueDate);
                        novaAnketa.id_lice = (int)Session["id_lice"];
                        novaAnketa.datum_kreiranje = Convert.ToDateTime(datum);
                        novaAnketa.multi_choice = multiChoice;

                        if (url_slika.Length > 1)
                        {
                            novaAnketa.url_slika = url_slika;
                        }

                        db.SaveChanges();

                        Anketa anketa = db.Anketas.Find(novaAnketa.id_anketa);

                        //ne gi brisam odgovorite
                        //ke ovozmozam samo nivno editiranje i dodavanje na drugi opcii
                        //anketa.Mozni_Odgovori.Clear();

                        List<Mozni_Odgovori> mozniOdgovori = anketa.Mozni_Odgovori.ToList();

                        for (int i = 0; i < elements.Length; i++)
                        {
                            string str = elements[i];
                            str = str.Replace(";;;", ",");

                            if (str.Length > 0)
                            {
                                if (mozniOdgovori.Count > i)
                                {
                                    mozniOdgovori[i].ime_odg = str;
                                }
                                else
                                {
                                    Mozni_Odgovori odgovor = new Mozni_Odgovori();
                                    odgovor.ime_odg = str;

                                    mozniOdgovori.Add(odgovor);
                                }
                            }
                        }

                        anketa.Mozni_Odgovori = mozniOdgovori;

                        db.SaveChanges();

                        //brisenje na dosegasnite kategorii na anketata
                        //duri i ako ne sme pratile niedna Selektrana Kategotija,
                        //toa znaci deka ne pripagja na nitu edna Kategorija!!!
                        anketa.Kategorijas.Clear();

                        if (zname)
                        {
                            for (int i = 0; i < idKategorii.Length; i++)
                            {
                                int id_k = Convert.ToInt32(idKategorii[i]);

                                Kategorija kategorija = db.Kategorijas.Find(id_k);

                                anketa.Kategorijas.Add(kategorija);

                                db.SaveChanges();
                            }

                            db.SaveChanges();
                        }

                        korisnik.br_anketi = korisnik.br_anketi + 1;
                        db.SaveChanges();

                        string strJson = "Your have succesfuly Edited you pool!!";

                        return Json(strJson);
                    }
                    else
                    {
                        string strJson22 = "You have not permissions to edit this poll!!!";
                        return Json(strJson22);
                    }
                }
            }

            string strJson2 = "You are Not Logged IN!!!";
            return Json(strJson2);
        }

        // GET: /Anketa/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["id_lice"] != null)
            {

                int idAnketa = id ?? 1;

                Anketa anketa = null;

                //znaci deka ke treba da editiram!!!
                if (idAnketa > 0)
                {
                    using (var db = new AnketiEntities5())
                    {
                        anketa = db.Anketas.Find(idAnketa);

                        Korisnik korisnik = db.Korisniks.Find((int)Session["id_lice"]);
                        Premium_Korisnik premiumKorisnik = db.Premium_Korisnik.Find((int)Session["id_lice"]);

                        List<Anketa> anketiNaKorisnik = korisnik.Louse.Anketas.ToList();

                        Anketa anketaP = db.Anketas.Find(idAnketa);

                        if (anketiNaKorisnik.Contains(anketaP))
                        {

                            List<SelectListItem> kategoriiLista = new List<SelectListItem>();

                            foreach (Kategorija k in db.Kategorijas)
                            {
                                SelectListItem selectList = null;

                                if (anketa.Kategorijas.Contains(k))
                                {
                                    selectList = new SelectListItem()
                                    {
                                        Text = k.ime_k,
                                        Value = k.id_k.ToString(),
                                        Selected = true
                                    };
                                }
                                else
                                {
                                    selectList = new SelectListItem()
                                    {
                                        Text = k.ime_k,
                                        Value = k.id_k.ToString(),
                                        Selected = false
                                    };
                                }

                                kategoriiLista.Add(selectList);
                            }

                            ViewBag.MyKategorii = kategoriiLista;

                            HashSet<Anketa_Proekt.Models.Mozni_Odgovori> odgovori = (HashSet<Anketa_Proekt.Models.Mozni_Odgovori>)anketa.Mozni_Odgovori;
                            odgovori.ToString();

                            return View(anketa);
                        }
                        else
                        {
                            //samo da zakacam uste poraka!!!
                            TempData["notice"] = "You do not have permissions to EDIT that poll!!!";

                            return RedirectToAction("Index");
                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ZapisiGlas(Anketa anketa)
        {
            if (Session["id_lice"] != null)
            {
                using (var db = new AnketiEntities5())
                {
                    foreach (int i in anketa.GlasoviId)
                    {
                        DateTime date = DateTime.Now;
                        string datum = date.ToString("yyyy-MM-dd HH:mm:ss");

                        var novGlas = db.Glasas.Create();
                        novGlas.id_anketa = anketa.id_anketa;
                        novGlas.id_lice = (int)Session["id_lice"];
                        novGlas.id_odg = i;
                        novGlas.datum_glasanje = Convert.ToDateTime(datum);

                        db.Glasas.Add(novGlas);
                    }

                    db.SaveChanges();
                }

                return RedirectToAction("Details", "Anketa", new { id = anketa.id_anketa });
            }
            //ViewBag.id_lice = new SelectList(db.Lice, "id_lice", "ime", anketa.id_lice);
            return View(anketa);
        }

        public ActionResult ZapisiKomentar()
        {
            string sodrzinaKom = Request.Params["sodrzinaKom"];
            string anketaId = Request.Params["anketa_id"];

            if (Session["id_lice"] != null)
            {
                using (var db = new AnketiEntities5())
                {
                    DateTime date = DateTime.Now;
                    string datum = date.ToString("yyyy-MM-dd HH:mm:ss");

                    var novKom = db.Komentars.Create();
                    novKom.sodrzina = sodrzinaKom;

                    db.Komentars.Add(novKom);

                    var komentarZa = db.Komentar_Za.Create();
                    komentarZa.id_anketa = Convert.ToInt32(anketaId);
                    komentarZa.id_kom = novKom.id_kom;
                    komentarZa.id_lice = (int)Session["id_lice"];
                    komentarZa.datum = Convert.ToDateTime(datum);

                    db.Komentar_Za.Add(komentarZa);

                    db.SaveChanges();
                }
                //return RedirectToAction("Details", "Anketa", new { id = anketa.id_anketa });
            }

            //ViewBag.id_lice = new SelectList(db.Lice, "id_lice", "ime", anketa.id_lice);
            return View();
        }

        // POST: /Anketa/Delete/5
        [HttpPost]
        public ActionResult DeleteConfirmed(int? id)
        {
            Anketa anketa = db.Anketas.Find(id);

            anketa.Mozni_Odgovori.Clear();

            anketa.Kategorijas.Clear();

            anketa.Glasas.Clear();

            anketa.Komentar_Za.Clear();

            anketa.Ogranicuvanjas.Clear();

            db.Anketas.Remove(anketa);

            db.SaveChanges();
            return RedirectToAction("MyPool");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult MyPool()
        {
            if (Session["id_lice"] != null)
            {
                int id = (int)Session["id_lice"];
                var anketas = db.Anketas.Include(a => a.Louse).Where(a => a.id_lice == id);
                anketas = anketas.OrderBy(a => a.datum_kreiranje);
                return View(anketas.ToList());
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public JsonResult MyAutoComplete(string term)
        {
            List<string> strJsonKategorii = new List<string>();
            List<string> strJsonOdgovori = new List<string>();
            List<string> strJsonAnketi = new List<string>();

            using (var db = new AnketiEntities5())
            {
                strJsonKategorii = (from a in db.Kategorijas
                                    where a.ime_k.StartsWith(term)
                                    select a.ime_k).ToList();

                strJsonAnketi = (from a in db.Anketas
                                 where a.prasanje.StartsWith(term)
                                 select a.prasanje).ToList();

                strJsonOdgovori = (from a in db.Mozni_Odgovori
                                   where a.ime_odg.StartsWith(term)
                                   select a.ime_odg).ToList();
            }

            List<string> retrunJson = new List<string>();

            foreach (string s in strJsonKategorii)
            {
                retrunJson.Add(s);
            }

            foreach (string s in strJsonAnketi)
            {
                retrunJson.Add(s);
            }

            foreach (string s in strJsonOdgovori)
            {
                retrunJson.Add(s);
            }

            //List<string> pom = new List<string>();
            //pom.Add("dd");
            //pom.Add("ddd");

            return Json(retrunJson);
        }

        [HttpGet]
        public ActionResult MySearch()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult MySearch(string searchText)
        {
            string search_text = searchText;

            List<Kategorija> kategorii = new List<Kategorija>();
            List<Mozni_Odgovori> odgovori = new List<Mozni_Odgovori>();
            List<Anketa> anketi = new List<Anketa>();

            using (var db = new AnketiEntities5())
            {
                kategorii = (from a in db.Kategorijas
                             where a.ime_k.StartsWith(search_text)
                             select a).ToList();

                anketi = (from a in db.Anketas
                          where a.prasanje.StartsWith(search_text)
                          select a).ToList();

                odgovori = (from a in db.Mozni_Odgovori
                            where a.ime_odg.StartsWith(search_text)
                            select a).ToList();

                if (anketi.Count > 0)
                {
                    ViewBag.AnketiPrasanje = anketi.Count;
                }

                if (kategorii.Count > 0)
                {
                    foreach (Kategorija k in kategorii)
                    {
                        List<Anketa> anketa = k.Anketas.ToList();

                        if (anketa.Count > 0)
                        {
                            ViewBag.AnketiVoKategorijata = anketa;
                        }
                    }
                }

                if (odgovori.Count > 0)
                {
                    foreach (Mozni_Odgovori odg in odgovori)
                    {
                        List<Anketa> anketa = odg.Anketas.ToList();

                        if (anketa.Count > 0)
                        {
                            ViewBag.AnketaSoOdg = anketa;
                        }
                    }
                }

                ViewBag.SearchWord = search_text;
            }

            //return RedirectToAction("MySearch", "Anketa");
            return View(anketi);
        }
    }
}
