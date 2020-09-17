﻿using HealthcareManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;


namespace HealthcareManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private HealthCareContext db=new HealthCareContext();
        public ActionResult Index()
        {
            string userid = "admin@demo.com";
            string password = "Admin@123";
            var o = db.Users.Where(u => u.UserId.Equals(userid)).FirstOrDefault();
            if (o == null)
            {
                //Add admin role in Role Table
                var RoleTable = db.Roles.ToList();
                if (RoleTable.ToList().Count == 0)
                {
                    Role usr = new Role();
                    usr.RoleName = "Admin";
                    db.Roles.Add(usr);
                    db.SaveChanges();
                    usr.RoleName = "Manager";
                    db.Roles.Add(usr);
                    db.SaveChanges();
                    usr.RoleName = "Reception";
                    db.Roles.Add(usr);
                    db.SaveChanges();
                    usr.RoleName = "Pharmacy";
                    db.Roles.Add(usr);
                    db.SaveChanges();
                    usr.RoleName = "Nurse";
                    db.Roles.Add(usr);
                    db.SaveChanges();
                }
                User admin = new User();
                admin.Name = "Adminname"; 
                admin.UserId = userid;
                admin.Password = Encrypt(password);
                admin.Confirmpassword = Encrypt(password);
                admin.RoleId = 1;
                db.Users.Add(admin);
                db.SaveChanges();
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(User Recept)
        {
            Recept.Password = Encrypt(Recept.Password);
            var reception = db.Users.Where(r => r.UserId.Equals(Recept.UserId) && r.Password.Equals(Recept.Password)).FirstOrDefault();
            if(reception!=null)
            {
                FormsAuthentication.SetAuthCookie(Recept.UserId,false);
                Session["UserId"] = reception.UserId.ToString();
                Session["Name"] = reception.Name.ToString();
                Session["Role"] = reception.Roles.RoleName.ToString();
                string role = Session["Role"].ToString();
                if (role == "Admin")
                    return RedirectToAction("Index", "Admin");
                else if (role == "Manager")
                    return RedirectToAction("Index", "Pharmacy");
                else if (role == "Nurse")
                    return RedirectToAction("Index", "Home");
                else if (role == "Pharmacy")
                    return RedirectToAction("Index", "Home");
                else
                    return RedirectToAction("Index", "Home");
            }
            else
            { 
                ModelState.AddModelError("", "Invalid credentials"); 
            }
            return View(Recept);
        }

        [Authorize]
        public ActionResult Reception()
        {
            string role = Session["Role"].ToString();
            if (role == "Reception")
                return View();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        
        //NewPatient 
        [Authorize]
        public ActionResult NewPatient()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        public ActionResult NewPatient(Patient pt)
        {
            if(ModelState.IsValid)
            {
                db.Patients.Add(pt);
                db.SaveChanges();
                return RedirectToAction("Reception");
            }
            return View(pt);
        }
        public ActionResult ViewPatient()
        {
            return View(db.Patients.ToList());
        }

        [Authorize]
        public ActionResult Profile()
        {
            if (Session["UserId"] != null && Session["Role"].ToString() == "Admin")
                return View();
            else
                return RedirectToAction("Index", "Home");
        }
        [Authorize]
        [HttpPost]
        public ActionResult Profile(User usr)
        {
            string username = User.Identity.Name;
            User user = db.Users.FirstOrDefault(u => u.UserId.Equals(username));
            user.Name = usr.Name;
            Session["Name"] = user.Name.ToString();
            db.Entry(user).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return View(usr);
        }





        //User Logout action
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index");
        }

        //Encrypt password method
        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (System.IO.MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        //Dispose the database
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