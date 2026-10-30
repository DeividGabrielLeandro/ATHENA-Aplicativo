using ATHENA.Database;
using ATHENA.Models;
using ATHENA.Services;
using Microsoft.Extensions.DependencyInjection;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using ATHENA.Services;


namespace ATHENA
{
    public partial class App : Application
    {
        CronometroService cronometro = new CronometroService();
        public App()
        {
            InitializeComponent();

            cronometro.Carregar();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Models_Usuario.LoginUsuario();
            return new Window(new AppShell());
        }

    }
}
