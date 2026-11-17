using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;
using OnlineStore.Data;
using OnlineStore.Models;
using OnlineStore.ViewModels;
using PagedList;

namespace OnlineStore.Controllers
{
    public class ProductsController : Controller
    {
        private OnlineStoreContext db = new OnlineStoreContext();

        // GET: Products
        public ActionResult Index(string category, string search, string sortBy, int? page)
        {
            ProductIndexViewModel viewmodel = new ProductIndexViewModel();
            var products = db.Products.Include(p => p.Category);

            //ViewBag.Category = new SelectList(products.Where(p => p.CategoryID != null).Select(p => p.Category.Name).Distinct());

            //LINQ syntax
            viewmodel.CatsWithCount = from p in products
                                      where p.CategoryID != null
                                      group p by p.Category.Name into g
                                      select new CategoryWithCount
                                      {
                                          CategoryName = g.Key,
                                          ProductCount = g.Count()
                                      };



            if (!String.IsNullOrEmpty(category))
            { 
                products = products.Where(p => p.Category.Name == category);
            }

            if (!String.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search) ||
                                               p.Category.Name.Contains(search) ||
                                               p.Description.Contains(search));
            }

            //sort the results
            switch (sortBy)
            {
                case "price_lowest":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_highest":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            viewmodel.SortBy = sortBy;
            viewmodel.Sorts = new Dictionary<string, string>
            {
                {"Price low to high", "price_lowest" },
                {"Price high to low", "price_highest" }
            };

            const int PageItems = 3;
            int currentPage = (page ?? 1);
            viewmodel.Products = products.ToPagedList(currentPage, PageItems);
            viewmodel.Search = search;
            viewmodel.Category = category;

            return View(viewmodel);
        }

    //        switch (sortby)
    //        {
    //            case "price_lowest":
    //                products = products.OrderBy(p => p.Price);
    //                break;
    //            case "price_highest":
    //                //products = products.OrderByDescending(p => p.Price);
    //                products = products.OrderBy(p => -p.Price);
    //                break;
    //            case "category":
    //                products = products.OrderByDescending(p => p.Category.Name);
    //                break;
    //            default:
    //                products = products.OrderBy(p => p.Name);
    //                break;
    //        }

    //viewmodel.SortBy = sortby;
    //        viewmodel.Sorting = new Dictionary<string, string>
    //        {
    //            {"price_lowest", "Price low to high" },
    //            { "price_highest", "Price high to low" },
    //            { "category", "Category" }
    //        };

//return Json(products.Select(x => new {
//    x.Name,
//    x.Price,
//    x.Description
//     }),JsonRequestBehavior.AllowGet);


// GET: Products/Details/5
public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
            
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,Price,Description,CategoryID")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", product.CategoryID);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", product.CategoryID);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Price,Description,CategoryID")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", product.CategoryID);
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
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
