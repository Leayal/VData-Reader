using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using static VData_Explorer.Helpers.Delegates;

namespace VData_Explorer.Controls
{
    /// <summary>
    /// Interaction logic for ComboBoxAddress.xaml
    /// </summary>
    public partial class ComboBoxAddress : ComboBox
    {
        private static readonly Point EmptyPoint = new Point(0, 0);
        private ListBox itemlist;

        public ComboBoxAddress()
        {
            InitializeComponent();
            this.AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler(ComboBoxAddress_Loaded));
            /*
            var dpd = DependencyPropertyDescriptor.FromProperty(ComboBox.ItemsSourceProperty, typeof(ComboBox));
            if (dpd != null)
                dpd.AddValueChanged(this, new EventHandler(this.ItemsSourceChanged));
            //*/
        }

        private void ItemsSourceChanged(object sender, EventArgs e)
        {
            if (this._contentLoaded && this.itemlist != null)
            {
                CollectionView val = this.itemlist.ItemsSource as CollectionView;
                if (val != null)
                {
                    FormattedText ft;
                    Typeface typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
                    double fontsize = this.FontSize;
                    double longestwidth = this.ActualWidth;
                    foreach (string item in val.SourceCollection)
                    {
                        ft = Interop.Helpers.MeasureTextSize(item, typeface, fontsize);
                        if (ft.Width > longestwidth)
                            longestwidth = ft.Width;
                    }
                    this.itemlist.MinWidth = longestwidth;
                    this.itemlist.MaxWidth = longestwidth;
                    this.itemlist.BringIntoView(new Rect(EmptyPoint, EmptyPoint));
                }
                else
                {
                    this.itemlist.MinWidth = this.ActualWidth;
                    this.itemlist.MaxWidth = this.ActualWidth;
                }
            }
        }

        private void ComboBoxAddress_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.itemlist == null) return;
            CollectionView val = this.itemlist.ItemsSource as CollectionView;
            if (val != null)
            {
                FormattedText ft;
                Typeface typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
                double fontsize = this.FontSize;
                double longestwidth = this.ActualWidth;
                foreach (string item in val.SourceCollection)
                {
                    ft = Interop.Helpers.MeasureTextSize(item, typeface, fontsize);
                    if (ft.Width > longestwidth)
                        longestwidth = ft.Width;
                }
                this.itemlist.MinWidth = longestwidth;
                this.itemlist.MaxWidth = longestwidth;
            }
            else
            {
                this.itemlist.MinWidth = this.ActualWidth;
                this.itemlist.MaxWidth = this.ActualWidth;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.itemlist = (ListBox)this.Template.FindName("ItemsPresenter", this);
        }

        private void ItemsPresenter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.IsDropDownOpen = false;
        }
    }
}
