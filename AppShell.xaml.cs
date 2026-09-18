using ATHENA.Categorias;
using ATHENA.Metas.xaml;

namespace ATHENA
{
    public partial class AppShell : Shell
    {

        public AppShell()
        {
            ///Paginas iniciais
            
            //Tela inicial
            InitializeComponent();

            Routing.RegisterRoute(nameof(NewPage1), typeof(NewPage1)); //Cronometro

            Routing.RegisterRoute(nameof(EscolherMeta), typeof(EscolherMeta));

            Routing.RegisterRoute(nameof(ListarCategoria), typeof(ListarCategoria));

            Routing.RegisterRoute(nameof(CriarCategoria), typeof(CriarCategoria));

            ///Paginas de navegação

            Routing.RegisterRoute(nameof(InterfaceMeta), typeof(InterfaceMeta));

            Routing.RegisterRoute(nameof(NewPage2), typeof(NewPage2)); //Pagina para criar meta

            Routing.RegisterRoute(nameof(EditarMeta), typeof(EditarMeta));
        }
    }
}
