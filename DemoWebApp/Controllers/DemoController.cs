using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DemoWebApp.Controllers
{
    public class DemoController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult NoXssAttack()
        {
            return View();
        }
        public ActionResult InlineXssAttack()
        {
            return View();
        }
        public ActionResult BlockXssAttack()
        {
            return View();
        }
    }
}