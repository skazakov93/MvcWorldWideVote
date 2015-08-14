using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Anketa_Proekt.Models;

namespace Anketa_Proekt.Controllers
{
    public class AnketaController : Controller
    {
        private AnketiEntities db = new AnketiEntities();

        // GET: /Anketa/
        public ActionResult Index()
        {
            var anketas = db.Anketas.Include(a => a.Louse);
            anketas = anketas.OrderBy(a => a.datum_kreiranje);
            return View(anketas.ToList());
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
            return View(anketa);
        }

        // GET: /Anketa/Create
        public ActionResult Create()
        {
            ViewBag.id_lice = new SelectList(db.Lice, "id_lice", "ime");
            return View();
        }

        // POST: /Anketa/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id_anketa,prasanje,opis_a,kraen_datum,id_lice,datum_kreiranje,multi_choice")] Anketa anketa)
        {
            if (ModelState.IsValid)
            {
                db.Anketas.Add(anketa);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_lice = new SelectList(db.Lice, "id_lice", "ime", anketa.id_lice);
            return View(anketa);
        }

        // GET: /Anketa/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.id_lice = new SelectList(db.Lice, "id_lice", "ime", anketa.id_lice);
            return View(anketa);
        }

        // POST: /Anketa/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id_anketa,prasanje,opis_a,kraen_datum,id_lice,datum_kreiranje,multi_choice")] Anketa anketa)
        {
            if (ModelState.IsValid)
            {
                db.Entry(anketa).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_lice = new SelectList(db.Lice, "id_lice", "ime", anketa.id_lice);
            return View(anketa);
        }

        // GET: /Anketa/Delete/5
        public ActionResult Delete(int? id)
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
            return View(anketa);
        }

        // POST: /Anketa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Anketa anketa = db.Anketas.Find(id);
            db.Anketas.Remove(anketa);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

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
