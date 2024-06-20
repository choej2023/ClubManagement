using ClubManagement.Model;
using ClubManagement.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Navigation;

namespace ClubManagement.Views
{
    public partial class ClubApplicationFormDetails : Window
    {
        public ClubApplicationFormDetails(ClubApplicationForm applicationForm)
        {
            InitializeComponent();
            DataContext = applicationForm;
        }
    }

}
