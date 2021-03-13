using System;
using Serilog;

namespace PCController.WPF.Views
{
    /// <summary>
    /// Interaction logic for RootView.xaml
    /// </summary>

    public partial class RootView
    {
        public RootView()
        {
            InitializeComponent();  
            Log.Logger.Information("hello from Serilog");
            Console.WriteLine("hello from console");
        }
    }
}
