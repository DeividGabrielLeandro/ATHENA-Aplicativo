using ATHENA.Categorias;
using ATHENA.Metas.xaml;
using ATHENA.SessaoEstudo;

namespace ATHENA
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(CriarCategoria), typeof(CriarCategoria));

            Routing.RegisterRoute(nameof(InterfaceMeta), typeof(InterfaceMeta));

            Routing.RegisterRoute(nameof(InterfaceCategoria), typeof(InterfaceCategoria));

            Routing.RegisterRoute(nameof(InterfaceSessao), typeof(InterfaceSessao));

            Routing.RegisterRoute(nameof(MetasCategoria), typeof(MetasCategoria));

            Routing.RegisterRoute(nameof(NewPage2), typeof(NewPage2));

            Routing.RegisterRoute(nameof(EditarMeta), typeof(EditarMeta));

            Routing.RegisterRoute(nameof(CriarSessao), typeof(CriarSessao));
        }
    }
}