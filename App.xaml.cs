using ATHENA.Models;
using ATHENA.Services;


namespace ATHENA
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Restaura o cronômetro salvo antes de qualquer tela aparecer.
            CronometroService.Carregar();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Models_Usuario.LoginUsuario();
            return new Window(new AppShell());
        }

    }
}
