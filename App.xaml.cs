using Microsoft.UI.Xaml;
using InternLog.Data;

namespace InternLog
{
    public partial class App : Application
    {
        public static Window? MainWindow { get; private set; }

        public App()
        {
            InitializeComponent();

            using (var db = new AppDbContext())
            {
                //db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                DatabaseSeeder.SeedEmployers(db);
            }
        }

        protected override void OnLaunched(
            Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}