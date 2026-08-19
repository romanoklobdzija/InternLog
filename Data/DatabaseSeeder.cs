using InternLog.Models;
using System.Collections.Generic;
using System.Linq;

namespace InternLog.Data;

public static class DatabaseSeeder
{
    public static void SeedEmployers(AppDbContext db)
    {
        if (db.Employers.Any())
        {
            return;
        }

        var employers = new List<Employer>
        {
            new Employer
            {
                Name = "Konto d.o.o.",
                Description = "Poduzeće specijalizirano za razvoj poslovnih i računovodstvenih informacijskih sustava.",
                ContactEmail = "info@konto.hr",
                ContactPhone = "+385 34 240 100",
                Website = "www.konto.hr",
                Industry = "Information Technology",
                Location = "Požega",
                StudentCapacity = 5,
                StudentTasks = "Rad s ERP sustavom, testiranje aplikacija, rad s bazama podataka i dokumentiranje programskih rješenja."
            },

            new Employer
            {
                Name = "TechNova d.o.o.",
                Description = "IT tvrtka koja razvija moderna web i desktop softverska rješenja za poslovne korisnike.",
                ContactEmail = "info@technova.hr",
                ContactPhone = "+385 42 310 200",
                Website = "www.technova.hr",
                Industry = "Software Development",
                Location = "Varaždin",
                StudentCapacity = 4,
                StudentTasks = "Razvoj softvera, testiranje, analiza zahtjeva i rad s razvojnim alatima."
            },

            new Employer
            {
                Name = "Digital Solutions d.o.o.",
                Description = "Tvrtka usmjerena na digitalizaciju poslovanja i razvoj digitalnih proizvoda.",
                ContactEmail = "hello@digitalsolutions.hr",
                ContactPhone = "+385 1 550 3100",
                Website = "www.digitalsolutions.hr",
                Industry = "Digital Services",
                Location = "Zagreb",
                StudentCapacity = 6,
                StudentTasks = "Izrada korisničkih sučelja, testiranje aplikacija i analiza korisničkih zahtjeva."
            },

            new Employer
            {
                Name = "CodeCraft d.o.o.",
                Description = "Softverska tvrtka koja razvija prilagođena programska rješenja za različite industrije.",
                ContactEmail = "contact@codecraft.hr",
                ContactPhone = "+385 31 420 300",
                Website = "www.codecraft.hr",
                Industry = "Software Development",
                Location = "Osijek",
                StudentCapacity = 3,
                StudentTasks = "Programiranje, debugging, code review i pisanje tehničke dokumentacije."
            },

            new Employer
            {
                Name = "Innova IT",
                Description = "IT konzultantska tvrtka koja pomaže organizacijama u modernizaciji poslovnih procesa.",
                ContactEmail = "info@innovait.hr",
                ContactPhone = "+385 51 610 400",
                Website = "www.innovait.hr",
                Industry = "IT Consulting",
                Location = "Rijeka",
                StudentCapacity = 4,
                StudentTasks = "Analiza poslovnih procesa, dokumentacija i pomoć pri implementaciji informacijskih sustava."
            },

            new Employer
            {
                Name = "WebForge d.o.o.",
                Description = "Agencija specijalizirana za razvoj web aplikacija i digitalnih platformi.",
                ContactEmail = "hello@webforge.hr",
                ContactPhone = "+385 1 620 5100",
                Website = "www.webforge.hr",
                Industry = "Web Development",
                Location = "Zagreb",
                StudentCapacity = 5,
                StudentTasks = "Frontend development, testiranje web aplikacija i održavanje postojećih projekata."
            },

            new Employer
            {
                Name = "DataCore d.o.o.",
                Description = "Tvrtka koja razvija podatkovna rješenja i sustave za obradu poslovnih podataka.",
                ContactEmail = "info@datacore.hr",
                ContactPhone = "+385 42 330 600",
                Website = "www.datacore.hr",
                Industry = "Data & Analytics",
                Location = "Varaždin",
                StudentCapacity = 3,
                StudentTasks = "Rad s bazama podataka, SQL upiti, analiza podataka i izrada izvještaja."
            },

            new Employer
            {
                Name = "ByteWorks d.o.o.",
                Description = "Razvojni studio fokusiran na poslovne aplikacije i automatizaciju procesa.",
                ContactEmail = "contact@byteworks.hr",
                ContactPhone = "+385 21 740 700",
                Website = "www.byteworks.hr",
                Industry = "Software Development",
                Location = "Split",
                StudentCapacity = 4,
                StudentTasks = "Razvoj aplikacija, testiranje i automatizacija poslovnih procesa."
            },

            new Employer
            {
                Name = "CloudPoint d.o.o.",
                Description = "Tvrtka koja pruža cloud infrastrukturu i razvoj cloud aplikacija.",
                ContactEmail = "info@cloudpoint.hr",
                ContactPhone = "+385 1 680 8100",
                Website = "www.cloudpoint.hr",
                Industry = "Cloud Computing",
                Location = "Zagreb",
                StudentCapacity = 3,
                StudentTasks = "Rad s cloud servisima, administracija sustava i razvoj pomoćnih alata."
            },

            new Employer
            {
                Name = "SoftLab d.o.o.",
                Description = "Mali razvojni studio koji izrađuje softverska rješenja za mala i srednja poduzeća.",
                ContactEmail = "info@softlab.hr",
                ContactPhone = "+385 35 450 900",
                Website = "www.softlab.hr",
                Industry = "Software Development",
                Location = "Slavonski Brod",
                StudentCapacity = 2,
                StudentTasks = "Programiranje, testiranje i izrada dokumentacije."
            },

            new Employer
            {
                Name = "NetVision d.o.o.",
                Description = "Poduzeće koje razvija mrežna i informacijska rješenja za poslovne korisnike.",
                ContactEmail = "support@netvision.hr",
                ContactPhone = "+385 23 510 100",
                Website = "www.netvision.hr",
                Industry = "IT Services",
                Location = "Zadar",
                StudentCapacity = 4,
                StudentTasks = "Administracija sustava, mrežne tehnologije i korisnička podrška."
            },

            new Employer
            {
                Name = "AppFactory d.o.o.",
                Description = "Razvojni tim usmjeren na izradu mobilnih i desktop aplikacija.",
                ContactEmail = "hello@appfactory.hr",
                ContactPhone = "+385 1 720 1100",
                Website = "www.appfactory.hr",
                Industry = "Application Development",
                Location = "Zagreb",
                StudentCapacity = 5,
                StudentTasks = "Razvoj mobilnih aplikacija, UI dizajn i testiranje."
            },

            new Employer
            {
                Name = "InfoSys d.o.o.",
                Description = "Tvrtka koja razvija informacijske sustave za različite poslovne sektore.",
                ContactEmail = "info@infosys.hr",
                ContactPhone = "+385 47 620 120",
                Website = "www.infosys.hr",
                Industry = "Information Systems",
                Location = "Karlovac",
                StudentCapacity = 3,
                StudentTasks = "Analiza sustava, dokumentiranje zahtjeva i testiranje."
            },

            new Employer
            {
                Name = "PixelWorks d.o.o.",
                Description = "Digitalni studio koji povezuje dizajn, razvoj i digitalni marketing.",
                ContactEmail = "hello@pixelworks.hr",
                ContactPhone = "+385 1 730 1300",
                Website = "www.pixelworks.hr",
                Industry = "Digital Media",
                Location = "Zagreb",
                StudentCapacity = 4,
                StudentTasks = "UI dizajn, web razvoj, izrada grafičkih materijala i testiranje."
            },

            new Employer
            {
                Name = "SmartTech d.o.o.",
                Description = "Tehnološka tvrtka koja razvija pametna digitalna rješenja.",
                ContactEmail = "info@smarttech.hr",
                ContactPhone = "+385 42 350 140",
                Website = "www.smarttech.hr",
                Industry = "Technology",
                Location = "Varaždin",
                StudentCapacity = 3,
                StudentTasks = "Razvoj softvera, istraživanje tehnologija i testiranje prototipova."
            },

            new Employer
            {
                Name = "DevPoint d.o.o.",
                Description = "Softverska tvrtka koja razvija poslovne aplikacije po mjeri korisnika.",
                ContactEmail = "contact@devpoint.hr",
                ContactPhone = "+385 31 480 150",
                Website = "www.devpoint.hr",
                Industry = "Software Development",
                Location = "Osijek",
                StudentCapacity = 5,
                StudentTasks = "Programiranje, rad s bazama podataka i razvoj korisničkih sučelja."
            },

            new Employer
            {
                Name = "SecureNet d.o.o.",
                Description = "Tvrtka specijalizirana za informacijsku sigurnost i zaštitu poslovnih sustava.",
                ContactEmail = "security@securenet.hr",
                ContactPhone = "+385 1 760 1600",
                Website = "www.securenet.hr",
                Industry = "Cybersecurity",
                Location = "Zagreb",
                StudentCapacity = 2,
                StudentTasks = "Analiza sigurnosti, dokumentiranje i testiranje sigurnosnih mehanizama."
            },

            new Employer
            {
                Name = "BusinessFlow d.o.o.",
                Description = "Poduzeće koje razvija sustave za upravljanje poslovnim procesima.",
                ContactEmail = "info@businessflow.hr",
                ContactPhone = "+385 34 270 170",
                Website = "www.businessflow.hr",
                Industry = "Business Software",
                Location = "Požega",
                StudentCapacity = 4,
                StudentTasks = "Analiza poslovnih procesa, testiranje softvera i rad s korisničkim zahtjevima."
            },

            new Employer
            {
                Name = "NextGen Systems",
                Description = "Inovativna tehnološka tvrtka koja razvija moderna digitalna rješenja.",
                ContactEmail = "hello@nextgensystems.hr",
                ContactPhone = "+385 51 640 180",
                Website = "www.nextgensystems.hr",
                Industry = "Technology",
                Location = "Rijeka",
                StudentCapacity = 3,
                StudentTasks = "Istraživanje novih tehnologija, razvoj prototipova i testiranje."
            },

            new Employer
            {
                Name = "CodeSphere d.o.o.",
                Description = "Softverski studio koji razvija moderne poslovne i web aplikacije.",
                ContactEmail = "info@codesphere.hr",
                ContactPhone = "+385 1 790 1900",
                Website = "www.codesphere.hr",
                Industry = "Software Development",
                Location = "Zagreb",
                StudentCapacity = 5,
                StudentTasks = "Frontend i backend razvoj, testiranje i rad s bazama podataka."
            }
        };

        db.Employers.AddRange(employers);
        db.SaveChanges();
    }
}