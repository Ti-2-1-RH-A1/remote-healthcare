﻿using System.Windows;

namespace DoctorApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ClientManager clientManager = new ClientManager();
    }
}
