using ATHENA.Metas.xaml;

namespace ATHENA
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(NewPage2), typeof(NewPage2));

            Routing.RegisterRoute(nameof(NewPage1), typeof(NewPage1));

            Routing.RegisterRoute(nameof(EscolherMeta), typeof(EscolherMeta));

            Routing.RegisterRoute(nameof(InterfaceMeta), typeof(InterfaceMeta));


        }
    }
}
