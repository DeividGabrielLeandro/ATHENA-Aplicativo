using ATHENA.Models;
using ATHENA.Services;


namespace ATHENA
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            CronometroService.Carregar();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Models_Usuario.LoginUsuario();
            return new Window(new AppShell());
        }

    }
}
