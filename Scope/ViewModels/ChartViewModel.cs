
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scope.Models;
using Syncfusion.UI.Xaml.Charts;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using CsvHelper;
using System.Globalization;

namespace Scope.ViewModels
{
	public class ChartViewModel: ObservableObject
	{
		public enum XAxisTypes { TimeSpan, Double}


		#region Properties

		public SfChart Chart { get; set; }
		public string Name { get; set; }


		public List<string> SeriesesList { get; set; }

		public VerticalLineAnnotation VerticalMarker1
		{
			get => Chart.Annotations[0] as VerticalLineAnnotation;
		}
		public VerticalLineAnnotation VerticalMarker2
		{
			get => Chart.Annotations[1] as VerticalLineAnnotation;
		}

		public HorizontalLineAnnotation HorizontalMarker1
		{
			get => Chart.Annotations[2] as HorizontalLineAnnotation;
		}
		public HorizontalLineAnnotation HorizontalMarker2
		{
			get => Chart.Annotations[3] as HorizontalLineAnnotation;
		}

		

		#endregion Properties

		#region Fields

		private Dictionary<string, LineSeries> _nameToSeries;
		private ChartTrackBallBehavior _trackBall;

		private TextWriter _textWriter;
		private CsvWriter _csvWriter;

		private bool _isEnablePanning;

		#endregion Fields

		#region Constructor

		public ChartViewModel(
			string chartName,
			string intervalUnits,
			XAxisTypes xAxisTypes)
		{
			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
				"MjQ2MzU2NkAzMjMwMmUzMzJlMzBOaGhMVVJBelp0Y1c1eXdoNHRTcHI4bGVOdmdxQWNXZkZxeklweENobmdjPQ==");

			
			Name = chartName;
			SeriesesList = new List<string>();

			ShowHideRiderCommand = new RelayCommand(ShowHideRider);
			ShowHideMarkersCommand = new RelayCommand(ShowHideMarkers);
			SelectZoomCommand = new RelayCommand(SelectZoom);
			GetImageCommand = new RelayCommand(GetImage);
			ExportCommand = new RelayCommand(Export);

			_nameToSeries = new Dictionary<string, LineSeries>();
			Chart = new SfChart();

			AddAxes(intervalUnits, xAxisTypes);

			Chart.Legend = new ChartLegend()
			{
				CheckBoxVisibility = Visibility.Visible,
				HorizontalAlignment = HorizontalAlignment.Left,
				Padding = new Thickness(0),
			};

			_isEnablePanning = true;
			AddBehaviors();

			_trackBall = new ChartTrackBallBehavior()
			{

			};

			AddCursors();

		}

		#endregion Constructor

		#region Methods

		private void AddAxes(string intervalUnits, XAxisTypes xAxisTypes)
		{
			string lableFormat = string.Empty;
			if (xAxisTypes == XAxisTypes.TimeSpan)
			{
				lableFormat = @"hh\:mm\:ss\:fff";
				if (intervalUnits != null && intervalUnits.ToLower() == "sec")
					lableFormat = @"ss\:ffffff";
			}

			if (xAxisTypes == XAxisTypes.TimeSpan)
			{
				Chart.PrimaryAxis = new TimeSpanAxis()
				{
					LabelFormat = lableFormat,
					ShowGridLines = true,
					ShowTrackBallInfo = true,
				};
			}
			else
			{
				Chart.PrimaryAxis = new NumericalAxis()
				{
					LabelFormat = lableFormat,
					ShowGridLines = true,
					ShowTrackBallInfo = true,
				};
			}

			Chart.SecondaryAxis = new NumericalAxis()
			{
				ShowGridLines = true,
			};
		}

		private void AddBehaviors()
		{
			Chart.Behaviors.Clear();
			ChartZoomPanBehavior zooming = new ChartZoomPanBehavior()
			{
				ResetOnDoubleTap = true,
				EnablePanning = _isEnablePanning,
				EnableSelectionZooming = !_isEnablePanning,
			};


			Chart.Behaviors.Add(zooming);			
		}

		private void AddCursors()
		{
			object x1 = null;
			if (Chart.PrimaryAxis is NumericalAxis)
				x1 = 1;
			else
				x1 = TimeSpan.FromMilliseconds(1000);


			VerticalLineAnnotation marker1 = new VerticalLineAnnotation()
			{
				X1 = x1,
				StrokeThickness = 2,
				CanDrag = true,
				Cursor = Cursors.Hand,
				ShowAxisLabel = true,		
				Visibility = Visibility.Collapsed,
			};
			Chart.Annotations.Add(marker1);


			object x2 = null;
			if (Chart.PrimaryAxis is NumericalAxis)
				x2 = 20;
			else
				x2 = TimeSpan.FromMilliseconds(20);

			VerticalLineAnnotation marker2 = new VerticalLineAnnotation()
			{
				X1 = x2,
				StrokeThickness = 2,
				CanDrag = true,
				Cursor = Cursors.Hand,
				ShowAxisLabel = true,
				Visibility = Visibility.Collapsed,
			};
			Chart.Annotations.Add(marker2);


			HorizontalLineAnnotation marker3 = new HorizontalLineAnnotation()
			{
				Y1 = 50,
				StrokeThickness = 2,
				CanDrag = true,
				Cursor = Cursors.Hand,
				ShowAxisLabel = true,		
				Visibility = Visibility.Collapsed,
			};
			Chart.Annotations.Add(marker3);


			HorizontalLineAnnotation marker4 = new HorizontalLineAnnotation()
			{
				Y1 = 500,
				StrokeThickness = 2,
				CanDrag = true,
				Cursor = Cursors.Hand,
				ShowAxisLabel = true,
				Visibility = Visibility.Collapsed,
			};
			Chart.Annotations.Add(marker4);

		}

		public void AddSeries(
			string name,
			Brush color)
		{
			string xBindingPath = "Time";
			if (Chart.PrimaryAxis is NumericalAxis)
				xBindingPath = "Seconds";
			LineSeries lineSeries = new LineSeries()
			{
				XBindingPath = xBindingPath,
				YBindingPath = "Value",
				Interior = color,
				Label = name,
			};

			Chart.Series.Add(lineSeries);

			if (_nameToSeries.ContainsKey(name))
			{
				return;
			}

			_nameToSeries.Add(name, lineSeries);
			SeriesesList.Add(name);
		}

		public void DeleteSeries(string name)
		{
			if (_nameToSeries.ContainsKey(name) == false)
			{
				return;
			}

			LineSeries lineSeries = _nameToSeries[name];



			Chart.Series.Remove(lineSeries);
			_nameToSeries.Remove(name);
			SeriesesList.Remove(name);
		}

		public void AddDataToSeries(
			double timeIntervalMs,
			List<double> valuesList,
			string name)
		{
			if (_nameToSeries.ContainsKey(name) == false)
				return;

			TimeSpan time = TimeSpan.FromMilliseconds(0);
			double seconds = 0;
			List<ScopeData> scopeDatasList = new List<ScopeData>();
			foreach (double value in valuesList)
			{
				if (Chart.PrimaryAxis is TimeSpanAxis)
				{
					ScopeData scopeData = new ScopeData()
					{
						Value = value,
						Time = time,
					};

					scopeDatasList.Add(scopeData);

					time += TimeSpan.FromMilliseconds(timeIntervalMs);
				}
				else if (Chart.PrimaryAxis is NumericalAxis)
				{
					ScopeData scopeData = new ScopeData()
					{
						Value = value,
						Seconds = seconds,
					};

					scopeDatasList.Add(scopeData);

					seconds += timeIntervalMs / 1000;
				}
			}

			LineSeries series = _nameToSeries[name];

			if (Application.Current != null)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					series.ItemsSource = scopeDatasList;
				});
			}
		}

		public void UpdateRecord()
		{
			if (Chart.Series.Count == 0)
				return;

			string fileName = "Record " + Name + " - " + DateTime.Now.ToString("dd-MMM-yyyy HH-mm-ss") + ".csv";

			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string path = System.IO.Path.Combine(documentsPath, "Logs");
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			path = System.IO.Path.Combine(path, "ScopeRecord");
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			path = System.IO.Path.Combine(path, fileName);


			_textWriter = new StreamWriter(path, false, System.Text.Encoding.UTF8);
			_csvWriter = new CsvWriter(_textWriter, CultureInfo.CurrentCulture);
			_csvWriter.WriteField("Time");


			try
			{
				

				// Write headers
				foreach (ChartSeries series in Chart.Series)
				{
					_csvWriter.WriteField(series.Label);
				}
				_csvWriter.NextRecord();

				if (!(Chart.Series[0] is LineSeries firstSeries))
					return;

				List<object> pointsList = firstSeries.GetDataPoints(
					Chart.PrimaryAxis.VisibleRange.Start,
					Chart.PrimaryAxis.VisibleRange.End,
					Chart.SecondaryAxis.VisibleRange.Start,
					Chart.SecondaryAxis.VisibleRange.End);


				for (int i = 0; i < pointsList.Count; i++)
				{
					if (!(pointsList[i] is ScopeData data))
						continue;

					_csvWriter.WriteField(data.Time.ToString(@"hh\:mm\:ss\:fff"));
					foreach (ChartSeries series in Chart.Series)
					{
						if (!(series.ItemsSource is List<ScopeData> seriesData))
							continue;

						_csvWriter.WriteField(seriesData[i].Value);

						System.Threading.Thread.Sleep(1);
					}

					System.Threading.Thread.Sleep(1);

					_csvWriter.NextRecord();
				}

				MessageBox.Show($"Saved successfuly to {fileName}", "Export");

			}
			catch
			{
				MessageBox.Show($"Failed to saved to {fileName}", "Export");
			}

			_csvWriter.Dispose();
			_textWriter.Close();

			_textWriter = null;
			_csvWriter = null;
		}




		private void ShowHideRider()
		{
			if (Chart.Behaviors.Contains(_trackBall))
				Chart.Behaviors.Remove(_trackBall);
			else
				Chart.Behaviors.Add(_trackBall);
		}

		private void ShowHideMarkers()
		{
			if (VerticalMarker1.Visibility == Visibility.Visible)
			{
				VerticalMarker1.Visibility = Visibility.Collapsed;
				VerticalMarker2.Visibility = Visibility.Collapsed;
				HorizontalMarker1.Visibility = Visibility.Collapsed;
				HorizontalMarker2.Visibility = Visibility.Collapsed;
			}
			else
			{
				VerticalMarker1.Visibility = Visibility.Visible;
				VerticalMarker2.Visibility = Visibility.Visible;
				HorizontalMarker1.Visibility = Visibility.Visible;
				HorizontalMarker2.Visibility = Visibility.Visible;

				VerticalMarker1.X1 = Chart.PrimaryAxis.VisibleRange.Start + (Chart.PrimaryAxis.VisibleRange.Delta * 0.10);
				VerticalMarker2.X1 = Chart.PrimaryAxis.VisibleRange.Start + (Chart.PrimaryAxis.VisibleRange.Delta * 0.80);

				HorizontalMarker1.Y1 = Chart.SecondaryAxis.VisibleRange.Start + (Chart.SecondaryAxis.VisibleRange.Delta * 0.10);
				HorizontalMarker2.Y1 = Chart.SecondaryAxis.VisibleRange.Start + (Chart.SecondaryAxis.VisibleRange.Delta * 0.80);
			}
		}

		private void GetImage()
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "Bitmap(*.bmp)|*.bmp|JPEG(*.jpg,*.jpeg)|*.jpg;*.jpeg|Gif (*.gif)|*.gif|PNG(*.png)|*.png|TIFF(*.tif,*.tiff)|*.tif|All files (*.*)|*.*";
			bool? results = sfd.ShowDialog();
			if (results != true)
				return;
			
			using (Stream fs = sfd.OpenFile())
				Chart.Save(fs, new PngBitmapEncoder());
			
		}

		private void Export()
		{
			//SaveFileDialog sfd = new SaveFileDialog();
			//sfd.Filter = "CSV File (*.csv)|*.csv";
			//bool? results = sfd.ShowDialog();
			//if (results != true)
			//	return;

			UpdateRecord();
		}

		private void SelectZoom()
		{
			_isEnablePanning = !_isEnablePanning;
			AddBehaviors();
		}

		#endregion Methods

		#region Commands

		public RelayCommand ShowHideRiderCommand { get; private set; }
		public RelayCommand ShowHideMarkersCommand { get; private set; }
		public RelayCommand SelectZoomCommand { get; private set; }

		public RelayCommand GetImageCommand { get; private set; }
		public RelayCommand ExportCommand { get; private set; }

		#endregion Commands
	}
}
