using ATHENA.Database;
using ATHENA.Models;
using Microsoft.Extensions.DependencyInjection;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;


namespace ATHENA
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Models_Usuario.LoginUsuario();
            return new Window(new AppShell());
        }

       
    }
}