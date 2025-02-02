
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scope.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Scope.ViewModels
{
	public class ScopeViewModel: ObservableObject
	{
		#region Properties

		public ObservableCollection<ChartViewModel> ChartsList { get; set; }

		public int ChartCols { get; set; }
		public int ChartRows { get; set; }

		public double MaxChartWidth { get; set; }
		public double MaxChartHeight { get; set; }
		public double ChartListWidth { get; set; }
		public double ChartListHeight { get; set; }


		#endregion Properties

		#region Fields

		private const double _minChartHeight = 250;

		private Dictionary<string, ChartViewModel> _nameToChart;

		private List<Brush> _colorsList;
		private int _colorIndex;

		#endregion Fields

		#region Constructor

		public ScopeViewModel()
		{
			ChartsList = new ObservableCollection<ChartViewModel>();
			_nameToChart = new Dictionary<string, ChartViewModel>();

			_colorIndex = 0;
			var converter = new System.Windows.Media.BrushConverter();
			_colorsList = new List<Brush>()
			{
				(Brush)converter.ConvertFromString("#e60049"),
				(Brush)converter.ConvertFromString("#0bb4ff"),
				(Brush)converter.ConvertFromString("#50e991"),
				(Brush)converter.ConvertFromString("#e6d800"), 
				(Brush) converter.ConvertFromString("#9b19f5"), 
				(Brush) converter.ConvertFromString("#ffa300"), 
				(Brush) converter.ConvertFromString("#dc0ab4"), 
				(Brush) converter.ConvertFromString("#b3d4ff"),
				(Brush) converter.ConvertFromString("#00bfa0")
			};

			ChartCols = 0;
			ChartRows = 1;
		}

		#endregion Constructor

		#region Methods

		public void AddChart(
			string chartName,
			string intervalUnits,
			ChartViewModel.XAxisTypes xAxisTypes)
		{
			ChartViewModel chartViewModel = new ChartViewModel(chartName, intervalUnits, xAxisTypes);
			ChartsList.Add(chartViewModel);
			_nameToChart.Add(chartName, chartViewModel);

			CalcNumOfCols();
		}

		public void DeleteChart(
			string chartName)
		{
			if (_nameToChart.ContainsKey(chartName) == false)
				return;


			ChartViewModel chartViewModel = _nameToChart[chartName];
			ChartsList.Remove(chartViewModel);
			_nameToChart.Remove(chartName);

			CalcNumOfCols();
		}

		public void AddSeriesToChart(
			string chartName,
			string seriesName)
		{
			AddSeriesToChart(
				chartName,
				seriesName,
				GetNextColor());
		}

		public void AddSeriesToChart(
			string chartName,
			string seriesName,
			Brush color)
		{
			if (_nameToChart.ContainsKey(chartName) == false)
				return;

			ChartViewModel chartViewModel = _nameToChart[chartName];
			chartViewModel.AddSeries(seriesName, color);
		}

		public void DeleteSeriesFromChart(
			string chartName,
			string seriesName)
		{
			if (_nameToChart.ContainsKey(chartName) == false)
				return;

			ChartViewModel chartViewModel = _nameToChart[chartName];
			chartViewModel.DeleteSeries(seriesName);
		}

		public void UpdateChart(
			double interval,
			DataFromMCU_Cahrt fromMCU_Cahrt,
			string chartName)
		{
			if (_nameToChart.ContainsKey(chartName) == false)
				return;

			ChartViewModel chartViewModel = _nameToChart[chartName];
			for (int i = 0; i < fromMCU_Cahrt.SeriesDataList.Count; i++)
			{
				chartViewModel.AddDataToSeries(
					interval,
					fromMCU_Cahrt.SeriesDataList[i].DataList,
					chartViewModel.SeriesesList[i]);
			}

			
		}

		private Brush GetNextColor()
		{
			if (_colorIndex < 0 || _colorIndex >= _colorsList.Count)
				_colorIndex = 0;

			Brush color = _colorsList[_colorIndex++];
			return color;
		}


		private void CalcNumOfCols()
		{
			if (ChartsList.Count == 1)
			{
				MaxChartWidth = ChartListWidth;
				MaxChartHeight = ChartListHeight;
				ChartCols = 1;
				return;
			}


			ChartCols = 2;

			MaxChartWidth = ChartListWidth / 2;

			int numberOfRows = ChartsList.Count / 2;
			if ((ChartsList.Count % 2) != 0)
				numberOfRows++;

			double height = ChartListHeight / numberOfRows;
			MaxChartHeight = height;
			if (MaxChartHeight < _minChartHeight)
				MaxChartHeight = _minChartHeight;
		}

		private void ChartsList_SizeChanged(SizeChangedEventArgs e)
		{
			if(!(e.Source is ListView lv))
				return;
			
			if(e.WidthChanged)
			{
				ChartListWidth = lv.ActualWidth - 25;
			}

			if (e.HeightChanged)
			{
				ChartListHeight = lv.ActualHeight - 25;
			}




			CalcNumOfCols();
		}

		#endregion Methods

		private RelayCommand<SizeChangedEventArgs> _ChartsList_SizeChangedCommand;
		public RelayCommand<SizeChangedEventArgs> ChartsList_SizeChangedCommand
		{
			get
			{
				return _ChartsList_SizeChangedCommand ?? (_ChartsList_SizeChangedCommand =
					new RelayCommand<SizeChangedEventArgs>(ChartsList_SizeChanged));
			}
		}
	}


}
