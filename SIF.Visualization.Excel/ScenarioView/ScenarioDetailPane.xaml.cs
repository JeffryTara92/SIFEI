﻿using System;
using System.Collections.Generic;
using SIF.Visualization.Excel.Cells;
using SIF.Visualization.Excel.ScenarioCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Microsoft.Office.Tools;
using SIF.Visualization.Excel.Core;

namespace SIF.Visualization.Excel.ScenarioView
{
    /// <summary>
    /// Interaktionslogik für ScenarioDetailPane.xaml
    /// </summary>
    public partial class ScenarioDetailPane : UserControl
    {
        #region Properties

        internal CompositeCollection ScenarioDataCollection { get; private set; }

        #endregion

        #region Methods

        public ScenarioDetailPane()
        {
            InitializeComponent();

            DataContextChanged += ScenarioDetailPane_DataContextChanged;

            IsVisibleChanged += ScenarioDetailPane_VisibilityChanged;
        }

        private void ScenarioDetailPane_VisibilityChanged(object sender,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!IsVisible)
            {
                MessageBoxResult result = DiscardChanges();
                if (result == MessageBoxResult.No)
                {
                    Visibility = Visibility.Visible;
                    foreach (
                        KeyValuePair<Tuple<WorkbookModel, string>, CustomTaskPane> customTaskPane in
                            Globals.ThisAddIn.TaskPanes)
                    {
                        if (customTaskPane.Value.Title == "Scenario")
                        {

                            
                            if (customTaskPane.Value != null)
                            {
                                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => { customTaskPane.Value.Visible = true; }));
                            }
                        }
                    }
                }
            }
        }

        #region Event Handling Methods

        private void ScenarioDetailPane_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext == null || !(DataContext is Scenario)) return;

            var myScenario = DataContext as Scenario;

            //update bindings

            #region binding definitions

            var titleBinding = new Binding()
            {
                Source = myScenario,
                Path = new PropertyPath("Title"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };

            var authorBinding = new Binding()
            {
                Source = myScenario,
                Path = new PropertyPath("Author"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };

            var creationDateBinding = new Binding()
            {
                Source = myScenario,
                Path = new PropertyPath("CrationDate"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };

            var ratingBinding = new Binding()
            {
                Source = myScenario,
                Path = new PropertyPath("Rating"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };

            var descriptionBinding = new Binding()
            {
                Source = myScenario,
                Path = new PropertyPath("Description"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };

            #endregion

            #region set bindings

            //task pane title
            PaneTitle.SetBinding(TextBlock.TextProperty, titleBinding);

            //general

            #region general

            //title

            TitleTextBox.SetBinding(TextBox.TextProperty, titleBinding);

            //author
            AuthorTextbox.SetBinding(TextBox.TextProperty, authorBinding);

            //creation date
            CreateDatePicker.SetBinding(DatePicker.SelectedDateProperty, creationDateBinding);

            //rating
            RatingTextBox.SetBinding(TextBox.TextProperty, ratingBinding);

            #endregion

            //description
            DescriptionTextBox.SetBinding(TextBox.TextProperty, descriptionBinding);

            // input, intermediate and result cells data
            ScenarioDataCollection = new CompositeCollection();

            #region collection container

            var inputDataCContainer = new CollectionContainer()
            {
                Collection = (DataContext as Scenario).Inputs
            };
            ScenarioDataCollection.Add(inputDataCContainer);

            var intermediateCContainer = new CollectionContainer()
            {
                Collection = (DataContext as Scenario).Intermediates
            };
            ScenarioDataCollection.Add(intermediateCContainer);

            var resultDataCContainer = new CollectionContainer()
            {
                Collection = (DataContext as Scenario).Results
            };
            ScenarioDataCollection.Add(resultDataCContainer);

            #endregion

            /*this.ScenarioDataView = new ListCollectionView(scenarioDataCollection);
            this.ScenarioDataView.SortDescriptions.Add(new SortDescription("Location", ListSortDirection.Ascending));*/
            ScenarioDataListBox.ItemsSource = ScenarioDataCollection;

            #endregion
        }

        #endregion

        #region Click Methods

        /// <summary>
        /// Updates the changes Scenario values into the datamodel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveDataButton_OnClickDataButton_Click(object sender, RoutedEventArgs e)
        {
            BindingExpression be = TitleTextBox.GetBindingExpression(TextBox.TextProperty);
            if (be != null) be.UpdateSource();
            be = AuthorTextbox.GetBindingExpression(TextBox.TextProperty);
            if (be != null) be.UpdateSource();
            be = DescriptionTextBox.GetBindingExpression(TextBox.TextProperty);
            if (be != null) be.UpdateSource();
            be = CreateDatePicker.GetBindingExpression(DatePicker.SelectedDateProperty);
            if (be != null) be.UpdateSource();
            be = RatingTextBox.GetBindingExpression(TextBox.TextProperty);
            if (be != null) be.UpdateSource();
        }

        /// <summary>
        /// Remove the selected item.
        /// Get the observable collection via the composite collection and the collection containers and remove the selected item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteDataButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = ScenarioDataListBox.SelectedItem;

            if (selectedItem == null) return;

            foreach (var cont in ScenarioDataCollection)
            {
                //remove input cell data
                if ((selectedItem is InputCellData)
                    && (cont is CollectionContainer)
                    && (cont as CollectionContainer).Collection is ObservableCollection<InputCellData>)
                {
                    ((cont as CollectionContainer).Collection as ObservableCollection<InputCellData>).Remove(
                        selectedItem as InputCellData);
                }

                //remove intermediate cell data
                if ((selectedItem is IntermediateCellData)
                    && (cont is CollectionContainer)
                    && (cont as CollectionContainer).Collection is ObservableCollection<IntermediateCellData>)
                {
                    ((cont as CollectionContainer).Collection as ObservableCollection<IntermediateCellData>).Remove(
                        selectedItem as IntermediateCellData);
                }

                //remove result cell data
                if ((selectedItem is ResultCellData)
                    && (cont is CollectionContainer)
                    && (cont as CollectionContainer).Collection is ObservableCollection<ResultCellData>)
                {
                    ((cont as CollectionContainer).Collection as ObservableCollection<ResultCellData>).Remove(
                        selectedItem as ResultCellData);
                }
            }
        }

        /// <summary>
        /// Discard Changes if the Button gets clicked and the Message Box is confirmed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DiscardDataButton_OnClickDataButton_Click(object sender, RoutedEventArgs e)
        {
            DiscardChanges();
        }

        #endregion

        private void ScenarioDataListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (sender as ListBox).SelectedItem as CellData;

            if (selectedItem != null)
            {
                //synchronize selection
                CellManager.Instance.SelectCell(DataModel.Instance.CurrentWorkbook, selectedItem.Location);
            }
        }

        #endregion

        /// <summary>
        /// Updates the Data in the Detailpane with the Data saved in the Datamodel discarding the last changes.
        /// </summary>
        /// <returns></returns>
        public MessageBoxResult DiscardChanges()
        {
            try
            {
                foreach (
                    KeyValuePair<Tuple<WorkbookModel, string>, CustomTaskPane> customTaskPane in
                        Globals.ThisAddIn.TaskPanes
                    )
                {
                    
                    if (customTaskPane.Value == null || customTaskPane.Key == null) continue;
                    if (customTaskPane.Value.Title != "Scenario") continue;
                    string messageText =
                        Properties.Resources.tl_ScenarioDetailPane_DiscardDataMessageBox;
                    MessageBoxResult result = MessageBox.Show(messageText,
                        Properties.Resources.tl_ScenarioDetailPane_DiscardDataMessageBoxTitle,
                        MessageBoxButton.YesNo);
                    if (result != MessageBoxResult.Yes) return MessageBoxResult.No;
                    Tuple<WorkbookModel, string> tempTaskPane = null;
                    // Update the target with whatever is in source. Since the source only gets updated by an explicit save
                    // the source still has the "old" values
                    BindingExpression be = TitleTextBox.GetBindingExpression(TextBox.TextProperty);
                    if (be != null) be.UpdateTarget();
                    be = AuthorTextbox.GetBindingExpression(TextBox.TextProperty);
                    if (be != null) be.UpdateTarget();
                    be = DescriptionTextBox.GetBindingExpression(TextBox.TextProperty);
                    if (be != null) be.UpdateTarget();
                    be = CreateDatePicker.GetBindingExpression(DatePicker.SelectedDateProperty);
                    if (be != null) be.UpdateTarget();
                    be = RatingTextBox.GetBindingExpression(TextBox.TextProperty);
                    if (be != null) be.UpdateTarget();
                }
            }
            catch (ObjectDisposedException ex)
            {
                //Quietly swallow the exception. Should only occur if the Pane is never opened and then the complete file is closed.
            }
            return MessageBoxResult.Yes;
        }
    }
}
