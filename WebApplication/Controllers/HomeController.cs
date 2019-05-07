using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;
using System.Xml.Linq;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {


        public ActionResult Index()
        {
            return View();
        }
        public ActionResult LoadData()
        {
            return View();
        }
        [HttpGet]
        public ActionResult GetDataDetails(string Id)
        {
            ApplicationContext db = new ApplicationContext();
            int id = Convert.ToInt32(Id);
            Data Data = db.Data.SingleOrDefault(element => element.id == id);
            if (Data.kpp == null)
                Data.kpp = "Отсутствует";
            if (Data.bankName == null)
                Data.bankName = "Отсутствует";
            if (Data.bik == null)
                Data.bik = "Отсутствует";
            if (Data.rs == null)
                Data.rs = "Отсутствует";
            if (Data.ks == null)
                Data.ks = "Отсутствует";
            ViewBag.Data = Data;
            IEnumerable<Lot> Lots = db.Lot.Where(element => element.notificationNumber == Data.notificationNumber);
            ViewBag.Lots = Lots;
            return View();
        }
        public ActionResult GetData()
        {
            ApplicationContext db = new ApplicationContext();
            int count = db.Data.Count();
            IEnumerable<Data> Data = db.Data.Where(element => element.id > (count - 20));
            ViewBag.Data = Data;

            ViewBag.CurrantPage = 1;
            ViewBag.NextPage = ViewBag.CurrantPage + 1;
            ViewBag.PreviousPage = ViewBag.CurrantPage - 1;
            return View();
            db.Dispose();
        }
        [HttpPost]
        public ActionResult GetData(string Page)
        {
            ApplicationContext db = new ApplicationContext();
            int page = Convert.ToInt32(Page);
            if (page <= 0)
                page = 1;
            int count = db.Data.Count();
            IEnumerable<Data> Data = db.Data.Where(element => element.id > (count - 20 * page)).Take(20);
            ViewBag.Data = Data;

            ViewBag.CurrantPage = page;
            ViewBag.NextPage = ViewBag.CurrantPage + 1;
            ViewBag.PreviousPage = ViewBag.CurrantPage - 1;
            return View();
            db.Dispose();
        }
        [HttpPost]
        public void Delete(string Id, string Page)
        {
            ApplicationContext db = new ApplicationContext();
            int id = Convert.ToInt32(Id);
            Data Data = db.Data.SingleOrDefault(element => element.id == id);
            IEnumerable<Lot> Lots = db.Lot.Where(element => element.notificationNumber == Data.notificationNumber);
            foreach (Lot lot in Lots)
                db.Lot.Remove(lot);
            db.Data.Remove(Data);
            db.SaveChanges();
            db.Dispose();
        }
        [HttpPost]
        public void EditDescription(string id, string description)
        {
            ApplicationContext db = new ApplicationContext();
            int Id = Convert.ToInt32(id);
            Data Data = db.Data.SingleOrDefault(element => element.id == Id);
            if (Data != null)
            {
                Data.description = description;
                db.SaveChanges();
            }
            db.Dispose();
        }
        [HttpPost]
        public void LoadData(string dateStart, string dateEnd, string type, string infoId)
        {
            ApplicationContext db = new ApplicationContext();

            int InfoId = Convert.ToInt32(infoId);
            //периоды начала и конца даты на извещении
            string startPeriod = dateStart.Replace("-", "") + "T0000";
            string endPeriod = dateEnd.Replace("-", "") + "T0000";
            WebRequest request = WebRequest.Create("https://torgi.gov.ru/opendata/7710349494-torgi/data-" + type + "-" + startPeriod + "-" + endPeriod + "-structure-20130401T0000.xml");
            WebResponse response = request.GetResponse();

            XmlTextReader XMLReader = new XmlTextReader(request.GetResponse().GetResponseStream());
            XDocument notifications = XDocument.Load(XMLReader);
            XNamespace ns = notifications.Root.GetDefaultNamespace();
            //цикл по внешнему документы
            foreach (XElement notification in notifications.Root.Elements())
            {
                //ссылка на внутренний док
                string newUrl = notification.Element(ns + "odDetailedHref").Value;
                request = WebRequest.Create(newUrl);
                response = request.GetResponse();

                XMLReader = new XmlTextReader(response.GetResponseStream());
                XDocument detail = XDocument.Load(XMLReader);

                //беру номер документа из url'ки
                string[] array = newUrl.Split('/');
                int notificationNumber = Convert.ToInt32(array[array.Length - 1]);

                //проверяю есть ли запись в таблице 
                Data Data = db.Data.SingleOrDefault(b => b.notificationNumber == notificationNumber);
                if (Data == null)
                {
                    Data = new Data();
                    Data.notificationNumber = notificationNumber;
                    Data.fio = detail.Root.Element(ns + "notification").Element(ns + "common").Element(ns + "fio").Value;
                    Data.notificationType = notification.Element(ns + "bidKindName").Value;
                    Data.organizationName = detail.Root.Element(ns + "notification").Element(ns + "bidOrganization").Element(ns + "fullName").Value;
                    Data.inn = detail.Root.Element(ns + "notification").Element(ns + "bidOrganization").Element(ns + "inn").Value;
                    if (detail.Root.Element(ns + "notification").Element(ns + "bidOrganization").Element(ns + "kpp") != null)
                        Data.kpp = detail.Root.Element(ns + "notification").Element(ns + "bidOrganization").Element(ns + "kpp").Value;

                    //поиск всех лотов в документе
                    foreach (XElement lot in detail.Root.Element(ns + "notification").Elements(ns + "lot"))
                    {
                        Lot Lot = new Lot();
                        Lot.notificationNumber = notificationNumber;
                        Lot.lotNumber = Convert.ToInt32(lot.Element(ns + "lotNum").Value);

                        if (lot.Element(ns + "location") != null)
                            Lot.location = lot.Element(ns + "location").Value;
                        else if (detail.Root.Element(ns + "notification").Element(ns + "bidOrganization").Element(ns + "location") != null)
                            Lot.location = detail.Root.Element(ns + "notification").Element(ns + "bidOrganization").Element(ns + "location").Value;

                        if (lot.Element(ns + "propDesc") != null)
                            Lot.description = lot.Element(ns + "propDesc").Value;
                        else if (lot.Element(ns + "objectDesc") != null)
                            Lot.description = lot.Element(ns + "objectDesc").Value;

                        db.Lot.Add(Lot);

                        if (lot.Element(ns + "paymentRequisites") != null)
                        {
                            if (lot.Element(ns + "paymentRequisites").Element(ns + "bik") != null)
                                Data.bik = lot.Element(ns + "paymentRequisites").Element(ns + "bik").Value;
                            if (lot.Element(ns + "paymentRequisites").Element(ns + "bankName") != null)
                                Data.bankName = lot.Element(ns + "paymentRequisites").Element(ns + "bankName").Value;
                            if (lot.Element(ns + "paymentRequisites").Element(ns + "ks") != null)
                                Data.ks = lot.Element(ns + "paymentRequisites").Element(ns + "ks").Value;
                            if (lot.Element(ns + "paymentRequisites").Element(ns + "rs") != null)
                                Data.rs = lot.Element(ns + "paymentRequisites").Element(ns + "rs").Value;
                        }
                    }
                    db.Data.Add(Data);
					
					//думаю эту строчку лучше поместить в строку 184, нагрузка на бд будет меньше
					//но оставлю тут, что бы было видно, что код работает, и в базе появляются новые записи постоянно
					db.SaveChanges();
                }
            }
            
            db.Dispose();
        }
    }
}