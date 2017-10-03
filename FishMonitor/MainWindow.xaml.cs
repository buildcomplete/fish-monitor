using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;

namespace FishMonitor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainVM Viewmodel = new MainVM();
		public MainWindow()
		{
			DataContext = Viewmodel;
			InitializeComponent();

			Viewmodel.NewSource += Viewmodel_NewFrame;
			Viewmodel.MotionFrameEvent += Viewmodel_MotionFrameEvent;
		}

		private void Viewmodel_MotionFrameEvent(object sender, NewFrameEventArgs e)
		{
			Bitmap bitmap = e.Frame;
			Dispatcher.Invoke(() =>
			{
				_imageMotionFrameControl.Source = Util.LoadBitmap(bitmap);
			});
		}

		private void Viewmodel_NewFrame(object sender, NewFrameEventArgs eventArgs)
		{
			// get new frame
			Bitmap bitmap = eventArgs.Frame;
			Dispatcher.Invoke(() =>
			{
				_imageRawFrameControl.Source = Util.LoadBitmap(bitmap);
			});
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			// signal to stop when you no longer need capturing
			Viewmodel.SelectedVideoDevice = null;
		}
	}
}
