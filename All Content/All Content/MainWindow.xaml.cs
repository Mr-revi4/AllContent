﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace All_Content
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer refresh_timer;
        DBClient client;
        List<SiteForPars> all_sites;

        object pars_lock = new object();
        public MainWindow()
        {

            InitializeComponent();
            all_sites = new List<SiteForPars>();
            InitializationSites();

            ContentUnit.Row_limit = 5000;

            client = new DBClient();

            refresh_timer = new DispatcherTimer();
            refresh_timer.Tick += Refresh_timer_Tick;
            refresh_timer.Interval = new TimeSpan(0, 0, 30);

            button_off.Click += Button_off_Click;
            button_on.Click += Button_on_Click;
            button_delete_all.Click += Delete_all_Click;

            passw.KeyDown += Passw_KeyDown;

        }


        private async void Refresh_timer_Tick(object sender, EventArgs e)
        {

            status.Background = Brushes.ForestGreen;
            status.Content = "Parsing status: active";
            time_of_last_pars.Content = "Последний парсинг начат в: " + DateTime.Now.ToShortTimeString();
            foreach (var site in all_sites)
                await Task.Run(() =>
            {
               // try
                
                    lock(pars_lock)
                        site.Pars();
                
                //catch (ExecutedRoutedEventArgs exc)
                //{
                //    Dispatcher.BeginInvoke(new Action(() =>
                //    {
                //        string name = site.GetType().FullName;
                //        all_info_block.Text += "Исключение при парсинге " + name + " " + DateTime.Now.ToShortTimeString() + "\n";
                //        all_info_block.Text += exc.GetType().FullName + " " + exc.Message + "\n";
                //    }));

                //}
            });

        }
        private void Passw_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Delete_all_Click(sender, e);
        }

        private void Delete_all_Click(object sender, RoutedEventArgs e)
        {
            if (passw.Password == "TooEasyWork")
            {
                client.Query("DELETE FROM content");
                passw.Password = "";

            }
        }

        private void Button_on_Click(object sender, RoutedEventArgs e)
        {
            refresh_timer.Start();
            status.Background = Brushes.YellowGreen;
            status.Content = "Parsing status: wait timer";
        }

        private void Button_off_Click(object sender, RoutedEventArgs e)
        {
            refresh_timer.Stop();
            status.Background = Brushes.Red;
            status.Content = "Parsing status: does not work";
        }

        private void InitializationSites()
        {
            all_sites.Add(new NplusOne());
            all_sites.Add(new KinoPoisk());
            all_sites.Add(new Kanobu());
            all_sites.Add(new LentaRu());
            all_sites.Add(new TheVillagePars());
            all_sites.Add(new TJpars());
            all_sites.Add(new RoemInvesticii());
            all_sites.Add(new RoemMedia());
            all_sites.Add(new LifeHacker());
            all_sites.Add(new RiaNews());
            all_sites.Add(new DiletantMedia());
            all_sites.Add(new LibrebookNews());
            all_sites.Add(new AdMe());
            all_sites.Add(new Starhit());

            string allSitesLink = "";
            foreach (var site in all_sites)
                allSitesLink += site.Source + ";";
            DBClient client = new DBClient();
            client.Query("UPDATE users SET favorites_source = @allSitesLink WHERE id = 0", new MySql.Data.MySqlClient.MySqlParameter("allSitesLink", allSitesLink));
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
                button_interval_Click(sender, e);

        }
        private void button_interval_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                refresh_timer.Interval = new TimeSpan(0, Convert.ToInt32(input_interval.Text), 0);
                lable_info.Content = "Новое время обновления: " + input_interval.Text;
                cur_inter_time.Content = "Curr time: " + input_interval.Text + "min";
                input_interval.Text = "";
            }
            catch (FormatException)
            {
                lable_info.Content = "Неверный формат ввода =(";
            }
        }

        private void button_limit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uint lim = (uint)Convert.ToInt32(textBox_lim.Text);
                ContentUnit.Row_limit = lim;
                limit_label.Content = "Curr limit: " + lim.ToString() + " row";
            }
            catch
            {
                lable_info.Content = "Неверный формат ввода =(";
            }
            
        }
    }
}
