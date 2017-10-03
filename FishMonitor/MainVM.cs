using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Vision.Motion;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FishMonitor
{
	public class MainVM : BindableBase
	{

		private BlobCountingObjectsProcessing processingAlgorithm = new BlobCountingObjectsProcessing(10, 3, true);
		public MainVM()
		{
			enumerateVideoDevices();

			// create motion detector
			_detector = new MotionDetector(
				 new SimpleBackgroundModelingDetector(true, false),
				 //new TwoFramesDifferenceDetector(true),
				 processingAlgorithm);
			//new MotionAreaHighlighting());
			//new GridMotionAreaProcessing(90, 30));

			_detector.MotionZones = new System.Drawing.Rectangle[]
			{
				new System.Drawing.Rectangle(50,100, 1800, 800)
			};

			PropertyChanged += MainVM_PropertyChanged;
		}

		private void MainVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(SelectedImageOption))
			{
				RaisePropertyChanged(nameof(SourceImageViewVisibility));
				RaisePropertyChanged(nameof(ProcessedImageViewVisibility));
			}
		}

		private MotionDetector _detector;

		private void enumerateVideoDevices()
		{
			// enumerate video devices
			var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
			foreach (FilterInfo item in videoDevices)
			{
				VideoCaptureDevices.Add(
					new NamedItemWrapper<FilterInfo> { Name = item.Name, Item = item });
			}
		}

		public ObservableCollection<NamedItemWrapper<FilterInfo>> VideoCaptureDevices { get; set; } =
			new ObservableCollection<NamedItemWrapper<FilterInfo>>();

		public int CaptureDelayMS { get; set; }

		private IVideoSource _videoSource;

		public event EventHandler<NewFrameEventArgs> NewSource;
		public event EventHandler<NewFrameEventArgs> MotionFrameEvent;

		Stopwatch _stopWatch = Stopwatch.StartNew();
		private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
		{
			if (_stopWatch.ElapsedMilliseconds > CaptureDelayMS)
			{
				DetectMotion(eventArgs.Frame.Clone() as Bitmap);

				NewSource?.Invoke(sender, eventArgs);
				_stopWatch.Restart();
			}

			eventArgs.Frame.Dispose();

		}

		private void DetectMotion(Bitmap bitmap)
		{
			_detector.ProcessFrame(bitmap);
			MotionFrameEvent?.Invoke(this, new NewFrameEventArgs(bitmap));
			bitmap.Dispose();

			//processingAlgorithm.ObjectRectangles
		}

		private NamedItemWrapper<FilterInfo> _selectedVideoDevice;
		public NamedItemWrapper<FilterInfo> SelectedVideoDevice
		{
			get => _selectedVideoDevice;
			set
			{
				if (_selectedVideoDevice != value)
				{
					if (_videoSource != null && _videoSource.IsRunning)
					{
						_videoSource.NewFrame -= video_NewFrame;
						_videoSource.SignalToStop();
					}

					if (value != null)
					{
						// create video source
						_videoSource = new VideoCaptureDevice(value.Item.MonikerString);
						// set NewFrame event handler
						_videoSource.NewFrame += video_NewFrame;
						// start the video source
						_videoSource.Start();
					}
					_selectedVideoDevice = value;
				}
			}
		}

		public Visibility SourceImageViewVisibility
		{
			get => _selectedImageOption == SIO.Processed ? Visibility.Collapsed : Visibility.Visible;
		}
		public Visibility ProcessedImageViewVisibility
		{
			get => _selectedImageOption == SIO.Source ? Visibility.Collapsed : Visibility.Visible;
		}
		private SIO _selectedImageOption = SIO.Both;
		public SIO SelectedImageOption
		{
			get => _selectedImageOption;
			set => SetProperty(ref _selectedImageOption, value);
		}
		public SIO[] ShowImageOptions
		{
			get => new SIO[]
			{
				SIO.Both,
				SIO.Source,
				SIO.Processed
			};
		}
	}

	public enum SIO
	{
		Both,
		Source,
		Processed
	}
}
